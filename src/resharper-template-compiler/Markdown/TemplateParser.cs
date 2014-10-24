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
            var shortcut = ExtractShortcut(content);
            var description = ExtractDescription(content);
            var text = ExtractText(content);
            return new Template
            {
                Guid = new Guid(metadata["guid"]),
                Type = (TemplateType) Enum.Parse(typeof(TemplateType), metadata["type"], true),
                Shortcut = shortcut,
                Description = description,
                Text = text,
                Reformat = GetBool(metadata, "reformat", true),
                ShortenQualifiedReferences = GetBool(metadata, "shortenReferences", true),
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
            string value;
            if (metadata.TryGetValue(key, out value))
                return bool.Parse(value);
            return @default;
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

        private static string ExtractShortcut(Block block)
        {
            var visitor = new ExtractFirstHeaderVisitor();
            visitor.Accept(block);
            return visitor.Header;
        }

        private static string ExtractDescription(Block block)
        {
            var visitor = new ExtractFirstParagraphVisitor();
            visitor.Accept(block);
            return visitor.Paragraph;
        }

        private static string ExtractText(Block block)
        {
            var visitor = new ExtractFirstCodeBlockVisitor();
            visitor.Accept(block);
            return visitor.CodeBlock;
        }

        private static IList<string> ParseCategories(IDictionary<string, string> metadata)
        {
            string value;
            if (metadata.TryGetValue("categories", out value))
                return SplitAndTrim(value, ',');
            return new string[0];
        }

        private static IList<Scope> ParseScopes(IDictionary<string, string> metadata)
        {
            string value;
            if (!metadata.TryGetValue("scopes", out value))
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
            throw new InvalidDataException(string.Format("Cannot parse scope: {0}", scope));
        }

        private static IList<Field> ParseFields(IDictionary<string, string> metadata)
        {
            string value;
            if (!metadata.TryGetValue("parameterOrder", out value))
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
            string expression;
            metadata.TryGetValue(name + "-expression", out expression);
            return expression ?? string.Empty;
        }

        private static IList<string> SplitAndTrim(string value, char c)
        {
            return value.Split(c).Select(s => s.Trim()).ToList();
        }
    }
}