using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CitizenMatt.ReSharper.TemplateCompiler.Markdown
{
    public class TemplateFormatter
    {
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
            writer.WriteLine("categories: {0}", string.Join(", ", template.Categories));
            FormatScopes(template);
            FormatFields(template);
            writer.WriteLine("---");
            writer.WriteLine();
            writer.WriteLine("# {0}", template.Shortcut);
            writer.WriteLine();
            writer.WriteLine(template.Description);
            writer.WriteLine();
            writer.WriteLine("```");
            writer.WriteLine(template.Text);
            writer.WriteLine("```");
        }

        private void FormatScopes(Template template)
        {
            var scopes = new List<string>();
            foreach (var scope in template.Scopes)
            {
                var content = string.Format("{0}", scope.Type);
                var parameters = string.Join(", ", scope.Parameters.Select(p => string.Format("{0}={1}", p.Key, p.Value)));
                if (!string.IsNullOrEmpty(parameters))
                    content = content + string.Format("({0})", parameters);
                scopes.Add(content);
            }
            writer.WriteLine("scopes: {0}", string.Join(", ", scopes));
        }

        private void FormatFields(Template template)
        {
            if (!template.Fields.Any())
                return;
            var fields = template.Fields.Select(f => f.Editable ? f.Name : string.Format("({0})", f.Name));
            writer.WriteLine("parameterOrder: {0}", string.Join(", ", fields));
            foreach (var field in template.Fields.Where(f => !string.IsNullOrEmpty(f.Expression)))
                writer.WriteLine("{0}-expression: {1}", field.Name, field.Expression);
        }
    }
}