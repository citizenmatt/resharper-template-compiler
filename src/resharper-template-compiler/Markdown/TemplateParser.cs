using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommonMark;
using CommonMark.Syntax;

namespace CitizenMatt.ReSharper.TemplateCompiler.Markdown
{
    public partial class TemplateParser
    {
        public Template Parse(string markdown)
        {
            var document = ParseDocument(markdown);
            var metadata = ParseMetadata(document);
            var content = SkipMetadata(document);
            var type = (TemplateType) Enum.Parse(typeof(TemplateType), metadata["type"], true);
            var shortcut = ExtractShortcut(content, type);
            var description = ExtractDescription(content, type);
            var text = ExtractText(content);
            return new Template
            {
                Guid = new Guid(metadata["guid"]),
                Type = type,
                Shortcut = shortcut,
                Description = description,
                Text = text,
                Reformat = GetBool(metadata, "reformat", true),
                ShortenQualifiedReferences = GetBool(metadata, "shortenReferences", true),
                CustomProperties = ParseCustomProperties(metadata),
                Categories = ParseCategories(metadata),
                Scopes = ParseScopes(metadata),
                Fields = ParseFields(metadata)
            };
        }

        private static Block ParseDocument(string text)
        {
            using(var stringReader = new StringReader(text))
            {
                // This is a truly awful API
                var document = CommonMarkConverter.ProcessStage1(stringReader);
                CommonMarkConverter.ProcessStage2(document);
                return document;
            }
        }

        private static IDictionary<string, string> ParseMetadata(Block document)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            if (document.FirstChild.Tag == BlockTag.HorizontalRuler)
            {
                var visitor = new MetadataVisitor(dictionary);
                visitor.Accept(document);
            }

            return dictionary;
        }

        private static bool GetBool(IDictionary<string, string> metadata, string key, bool @default)
        {
            return metadata.TryGetValue(key, out var value) ? bool.Parse(value) : @default;
        }

        private static Block SkipMetadata(Block document)
        {
            if (document.FirstChild.Tag != BlockTag.HorizontalRuler)
                return document;
            var block = document.FirstChild.NextSibling;
            while (block != null && block.Tag != BlockTag.HorizontalRuler)
                block = block.NextSibling;
            return block;
        }

        private static string ExtractShortcut(Block block, TemplateType type)
        {
            if (type == TemplateType.File)
                return null;
            var visitor = new ExtractFirstHeaderVisitor();
            visitor.Accept(block);
            return visitor.Header;
        }

        private static string ExtractDescription(Block block, TemplateType type)
        {
            // Files don't have shortcuts, so the file header is the description
            if (type == TemplateType.File)
            {
                var visitor = new ExtractFirstHeaderVisitor();
                visitor.Accept(block);
                return visitor.Header;
            }
            else
            {
                var visitor = new ExtractFirstParagraphVisitor();
                visitor.Accept(block);
                return visitor.Paragraph;
            }
        }

        private static string ExtractText(Block block)
        {
            var visitor = new ExtractFirstCodeBlockVisitor();
            visitor.Accept(block);
            return visitor.CodeBlock;
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

            return (from s in SplitAndTrim(value, ';')
                select ParseScope(s)).ToList();
        }

        private static Scope ParseScope(string scope)
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
                var s = new Scope
                {
                    Type = match.Groups["type"].Value
                };
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
                let editable = !(f.Contains("(") && f.Contains(")"))
                let name = f.Replace("(", string.Empty).Replace(")", string.Empty)
                select new Field
                {
                    Name = name,
                    Editable = editable,
                    Expression = GetFieldExpression(name, metadata)
                }).ToList();
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