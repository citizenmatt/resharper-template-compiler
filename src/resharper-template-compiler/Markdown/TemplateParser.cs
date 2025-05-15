using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace CitizenMatt.ReSharper.TemplateCompiler.Markdown
{
    public class TemplateParser
    {
        public Template Parse(string markdown)
        {
            var document = ParseDocument(markdown);
            var metadata = ParseMetadata(document);
            var type = (TemplateType) Enum.Parse(typeof(TemplateType), metadata["type"], true);
            var shortcut = ExtractShortcut(document, type);
            var description = ExtractDescription(document, type);
            var text = ExtractText(document);
            return new Template
            {
                Guid = new Guid(metadata["guid"]),
                Type = type,
                Shortcut = shortcut,
                Description = description,
                Text = text,
                Image = GetString(metadata, "image", null),
                Reformat = GetBool(metadata, "reformat", true),
                ShortenQualifiedReferences = GetBool(metadata, "shortenReferences", true),
                BuildAction = GetString(metadata, "buildAction", null),
                CustomProperties = ParseCustomProperties(metadata),
                Categories = ParseCategories(metadata),
                Scopes = ParseScopes(metadata),
                Fields = ParseFields(metadata),
                UITag = GetString(metadata, "UITag", null)
            };
        }

        private static MarkdownDocument ParseDocument(string text)
        {
            var pipeline = new MarkdownPipelineBuilder().UseYamlFrontMatter().Build();
            var document = Markdig.Markdown.Parse(text, pipeline);
            return document;
        }

        private static IDictionary<string, string> ParseMetadata(MarkdownDocument document)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            var frontMatter = document.Descendants<YamlFrontMatterBlock>().First();
            foreach (StringLine line in frontMatter.Lines)
            {
                var text = line.ToString();
                var colon = text.IndexOf(":", StringComparison.Ordinal);
                if (colon == -1) continue;
                var key = text.Substring(0, colon).Trim();
                var value = text.Substring(colon + 1, text.Length - (colon + 1)).Trim();
                dictionary.Add(key, value);
            }

            return dictionary;
        }

        private static bool GetBool(IDictionary<string, string> metadata, string key, bool @default)
        {
            return metadata.TryGetValue(key, out var value) ? bool.Parse(value) : @default;
        }

        private static string GetString(IDictionary<string, string> metadata, string key, string @default)
        {
            return metadata.TryGetValue(key, out var value) ? value : @default;
        }

        private static string ExtractLiteral(ContainerInline inline)
        {
            return string.Join("", inline.Descendants<LiteralInline>().Select(i => i.ToString()));
        }

        private static string ExtractShortcut(MarkdownDocument document, TemplateType type)
        {
            if (type == TemplateType.File)
                return null;

            var headingBlock = document.Descendants<HeadingBlock>().First();
            return ExtractLiteral(headingBlock.Inline);
        }

        private static string ExtractDescription(MarkdownDocument document, TemplateType type)
        {
            // Files don't have shortcuts, so the file header is the description
            if (type == TemplateType.File)
            {
                var headingBlock = document.Descendants<HeadingBlock>().First();
                return ExtractLiteral(headingBlock.Inline);
            }
            else
            {
                var paragraphBlock = document.Descendants<ParagraphBlock>().First();
                return ExtractLiteral(paragraphBlock.Inline);
            }
        }

        private static string ExtractText(MarkdownDocument document)
        {
            var codeBlock = document.Descendants<FencedCodeBlock>().First();
            var content = new StringBuilder();
            foreach (StringLine line in codeBlock.Lines)
                content.AppendLine(line.ToString());

            // Trim any trailing linefeeds
            return content.ToString().Trim();
        }

        private static IDictionary<string, string> ParseCustomProperties(IDictionary<string, string> metadata)
        {
            var properties = new Dictionary<string, string>();
            if (metadata.TryGetValue("customProperties", out var value))
            {
                var values = SplitAndTrim(value, ',');
                foreach (var kvp in values)
                {
                    var parts = SplitAndTrim(kvp, '=');
                    properties[parts[0]] = parts[1];
                }
            }
            return properties;
        }

        private static IList<string> ParseCategories(IDictionary<string, string> metadata)
        {
            return metadata.TryGetValue("categories", out var value) ? SplitAndTrim(value, ',') : new string[0];
        }

        private static IList<Scope> ParseScopes(IDictionary<string, string> metadata)
        {
            if (!metadata.TryGetValue("scopes", out var value))
                return new Scope[0];

            var names = SplitAndTrim(value, ';').ToList();

            // Create the GUIDs for the scopes. It is important to try to keep the scopes in the same order as they are
            // defined in the template.md file - the grouping and ordering of the templates are based on the name of the
            // first scope point that the template declares (see RIDER-10132). I don't believe the settings subsystem
            // has any guarantees on ordering, but it works for now.
            // The downside is that we can't also have deterministic GUIDs, so we will always get churn on the GUID key
            // with every run.
            var guids = (from _ in names
                let g = Guid.NewGuid()
                orderby SerialisationMetadata.FormatGuid(g)
                select g).ToList();

            return names.Select((t, i) => ParseScope(t, guids[i])).ToList();
        }

        private static Scope ParseScope(string scope, Guid guid)
        {
            var match = Regex.Match(scope,
@"^(?<type>\w+) (?<parameters>\(
(?:
  (?: (?:(?<=\()\s*) | (?:,\s*) ) (?# Optional whitespace following an open bracket, or a comma with optional whitespace)
  (?<key>\w+) \s* = \s* (?<value>[^\s,]*) \s*
)+
\))?$", RegexOptions.IgnorePatternWhitespace);

            if (match.Success)
            {
                var s = new Scope(match.Groups["type"].Value, guid);
                if (match.Groups["parameters"].Success)
                {
                    for (var i = 0; i < match.Groups["key"].Captures.Count; i++)
                        s.Parameters.Add(match.Groups["key"].Captures[i].Value, match.Groups["value"].Captures[i].Value);
                }
                return s;
            }
            throw new InvalidDataException($"Cannot parse scope: {scope}");
        }

        private static IList<Field> ParseFields(IDictionary<string, string> metadata)
        {
            if (!metadata.TryGetValue("parameterOrder", out var value))
                return new List<Field>();

            return (from f in SplitAndTrim(value, ',')
                select ParseField(f, metadata)).ToList();
        }

        private static Field ParseField(string field, IDictionary<string, string> metadata)
        {
            var editable = !(field.StartsWith("(") && field.EndsWith(")"));
            var name = field.Replace("(", string.Empty).Replace(")", string.Empty);
            long editableInstance = -1;
            var i = field.IndexOf("#", StringComparison.OrdinalIgnoreCase);
            if (editable && i != -1)
            {
                editableInstance = int.Parse(name.Substring(i + 1));
                name = name.Substring(0, i);
            }

            return new Field
            {
                Name = name,
                Editable = editable,
                EditableInstance = editableInstance,
                Expression = GetFieldExpression(name, metadata)
            };
        }

        private static string GetFieldExpression(string name, IDictionary<string, string> metadata)
        {
            metadata.TryGetValue(name + "-expression", out var expression);
            return expression ?? string.Empty;
        }

        private static IList<string> SplitAndTrim(string value, char c)
        {
            return value.Split(c).Select(s => s.Trim()).ToList();
        }
    }
}