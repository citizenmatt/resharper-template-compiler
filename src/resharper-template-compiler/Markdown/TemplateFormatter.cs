using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CitizenMatt.ReSharper.TemplateCompiler.Markdown
{
    public class TemplateFormatter
    {
        private static Regex NoramliseRegex = new Regex(@"\r\n|\n\r|\n|\r", RegexOptions.Compiled);

        private readonly TextWriter writer;

        public TemplateFormatter(TextWriter writer)
        {
            this.writer = writer;
        }

        public void FormatTemplate(Template template)
        {
            writer.WriteLine("---");
            writer.WriteLine("guid: {0}", template.Guid);
            writer.WriteLine("type: {0}", template.Type);
            writer.WriteLine("reformat: {0}", template.Reformat);
            writer.WriteLine("shortenReferences: {0}", template.ShortenQualifiedReferences);
            if (template.Categories.Any())
                writer.WriteLine("categories: {0}", string.Join(", ", template.Categories));
            FormatCustomProperties(template);
            FormatScopes(template);
            FormatFields(template);
            writer.WriteLine("---");
            writer.WriteLine();
            if (template.Type == TemplateType.File)
            {
                writer.WriteLine("# {0}", template.Description);
            }
            else
            {
                writer.WriteLine("# {0}", template.Shortcut);
                writer.WriteLine();
                writer.WriteLine(template.Description);
            }
            writer.WriteLine();
            writer.WriteLine("```");
            writer.WriteLine(NoramliseRegex.Replace(template.Text, "\r\n"));
            writer.WriteLine("```");
        }

        private void FormatCustomProperties(Template template)
        {
            if (!template.CustomProperties.Any())
                return;

            var properties = string.Join(", ", template.CustomProperties.Select(p => $"{p.Key}={p.Value}"));
            writer.WriteLine("customProperties: {0}", properties);
        }

        private void FormatScopes(Template template)
        {
            if (!template.Scopes.Any())
                return;
            
            var scopes = new List<string>();
            foreach (var scope in template.Scopes)
            {
                var content = $"{scope.Type}";
                var parameters = string.Join(", ", scope.Parameters.Select(p => $"{p.Key}={p.Value}"));
                if (!string.IsNullOrEmpty(parameters))
                    content = content + $"({parameters})";
                scopes.Add(content);
            }
            writer.WriteLine("scopes: {0}", string.Join("; ", scopes));
        }

        private void FormatFields(Template template)
        {
            if (!template.Fields.Any())
                return;
            var fields = template.Fields.Select(f => f.Editable ? f.Name : $"({f.Name})");
            writer.WriteLine("parameterOrder: {0}", string.Join(", ", fields));
            foreach (var field in template.Fields.Where(f => !string.IsNullOrEmpty(f.Expression)))
                writer.WriteLine("{0}-expression: {1}", field.Name, field.Expression);
        }
    }
}