using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CitizenMatt.ReSharper.TemplateCompiler
{
    public class ReadmeFormatter
    {
        private readonly TextWriter writer;
        private readonly Uri baseUri;

        public ReadmeFormatter(TextWriter writer, string basePath)
        {
            this.writer = writer;
            basePath = basePath.Replace('\\', '/');
            if (!basePath.EndsWith(@"/")) basePath += @"/";
            baseUri = new Uri(basePath);
        }

        public void FormatTemplates(TemplateStore templates)
        {
            var templatesByCategory = new MultiValueDictionary<string, Template>();
            foreach (var template in templates.Templates)
            {
                foreach (var category in template.Categories)
                    templatesByCategory.Add(category, template);
            }

            writer.WriteLine("# Templates");
            writer.WriteLine();

            if (templatesByCategory.Count > 0)
            {
                writer.WriteLine("Categorised:");
                writer.WriteLine();
                foreach (var category in templatesByCategory.Keys.OrderBy(c => c, StringComparer.Ordinal))
                    writer.WriteLine("* [{0}](#{1})", category, category.Replace(' ', '_'));
                writer.WriteLine();
            }

            writer.WriteLine("## All");
            FormatTemplates(templates.Templates.ToList());

            foreach (var category in templatesByCategory.Keys.OrderBy(s => s, StringComparer.Ordinal))
            {
                writer.WriteLine("<a name=\"{0}\"></a>", category.Replace(' ', '_'));
                writer.WriteLine("## Category: {0}", category);
                FormatTemplates(templatesByCategory[category].ToList());
            }
        }

        private void FormatTemplates(IReadOnlyCollection<Template> templates)
        {
            FormatTemplates(TemplateType.File, templates.Where(t => t.Type.HasFlag(TemplateType.File)));
            FormatTemplates(TemplateType.Live, templates.Where(t => t.Type.HasFlag(TemplateType.Live)));
            FormatTemplates(TemplateType.Surround, templates.Where(t => t.Type.HasFlag(TemplateType.Surround)));
        }

        private void FormatTemplates(TemplateType templateType, IEnumerable<Template> templates)
        {
            switch(templateType)
            {
                case TemplateType.Live:
                    FormatLiveTemplates(templates);
                    break;
                case TemplateType.Surround:
                    FormatSurroundTemplates(templates);
                    break;
                case TemplateType.File:
                    FormatFileTemplates(templates);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(templateType), templateType, null);
            }
        }

        private void FormatLiveTemplates(IEnumerable<Template> templates)
        {
            GenerateLiveTemplatesTable(templates, "Live Templates");
        }

        private void FormatSurroundTemplates(IEnumerable<Template> templates)
        {
            GenerateLiveTemplatesTable(templates, "Surround Templates");
        }

        private void GenerateLiveTemplatesTable(IEnumerable<Template> templates, string title)
        {
            var sortedTemplates = templates.OrderBy(t => t.Shortcut, StringComparer.Ordinal).ToList();
            if (!sortedTemplates.Any()) return;

            writer.WriteLine();
            writer.WriteLine("### " + title);
            writer.WriteLine();

            var columns = new[]
            {
                new string[sortedTemplates.Count],
                new string[sortedTemplates.Count]
            };
            for (var i = 0; i < sortedTemplates.Count; i++)
            {
                var template = sortedTemplates[i];
                columns[0][i] = $"[{template.Shortcut}]({GetRelativePath(template.InputFile)})";
                columns[1][i] = template.Description;
            }
            FormatTable(new[] { "Shortcut", "Description" }, columns);

            writer.WriteLine();
        }

        private void FormatFileTemplates(IEnumerable<Template> templates)
        {
            writer.WriteLine();
            writer.WriteLine("### File Templates");
            writer.WriteLine();

            var sortedTemplates = templates.OrderBy(t => t.Description, StringComparer.Ordinal).ToList();
            var columns = new[] { new string[sortedTemplates.Count] };
            for (var i = 0; i < sortedTemplates.Count; i++)
            {
                var template = sortedTemplates[i];
                columns[0][i] = $"[{template.Description}]({GetRelativePath(template.InputFile)})";
            }
            FormatTable(new[] { "Description" }, columns);

            writer.WriteLine();
        }

        private void FormatTable(IReadOnlyList<string> headers, IReadOnlyList<string[]> columns)
        {
            var columnWidths = new int[headers.Count];
            for (var i = 0; i < columns.Count; i++)
            {
                var width = headers[i].Length;
                foreach (var row in columns[i])
                    width = Math.Max(width, row.Length);
                columnWidths[i] = width;
            }

            writer.Write("|");
            for (var i = 0; i < headers.Count; i++)
                writer.Write(" " + headers[i].PadRight(columnWidths[i]) + " |");
            writer.WriteLine();

            writer.Write("|");
            for (var i = 0; i < headers.Count; i++)
                writer.Write(new string('-', columnWidths[i] + 2) + "|");
            writer.WriteLine();

            for (var i = 0; i < columns[0].Length; i++)
            {
                writer.Write("|");
                for (var j = 0; j < columns.Count; j++)
                    writer.Write(" " + columns[j][i].PadRight(columnWidths[j]) + " |");
                writer.WriteLine();
            }
        }

        private string GetRelativePath(string fullPath)
        {
            var fullUri = new Uri(fullPath.Replace('\\', '/'), UriKind.Absolute);
            var relativeUri = baseUri.MakeRelativeUri(fullUri);
            return relativeUri.ToString();
        }
    }
}
