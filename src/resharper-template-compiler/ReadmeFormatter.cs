using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CitizenMatt.ReSharper.TemplateCompiler
{
    public class ReadmeFormatter
    {
        private readonly TextWriter writer;

        public ReadmeFormatter(TextWriter writer)
        {
            this.writer = writer;
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
                foreach (var category in templatesByCategory.Keys.OrderBy(c => c))
                    writer.WriteLine("* [{0}](#{1})", category, category.Replace(' ', '_'));
                writer.WriteLine();
            }

            writer.WriteLine("## All");
            FormatTemplates(templates.Templates);
            
            foreach (var category in templatesByCategory.Keys.OrderBy(s => s))
            {
                writer.WriteLine("<a name=\"{0}\"></a>", category.Replace(' ', '_'));
                writer.WriteLine("## Category: {0}", category);
                FormatTemplates(templatesByCategory[category]);
            }
        }

        private void FormatTemplates(IEnumerable<Template> templates)
        {
            foreach (var groupedTemplates in templates.GroupBy(t => t.Type))
                FormatTemplates(groupedTemplates.Key, groupedTemplates);
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
            writer.WriteLine();
            writer.WriteLine("### Live Templates");
            writer.WriteLine();
            writer.WriteLine("Shortcut | Description");
            writer.WriteLine("---------|------------");
            foreach (var template in templates.OrderBy(t => t.Shortcut))
                writer.WriteLine("[{0}]({1}) | {2}", template.Shortcut, template.InputFile, template.Description);

            writer.WriteLine();
        }

        private void FormatSurroundTemplates(IEnumerable<Template> templates)
        {
            throw new NotImplementedException("Surround templates not supported in README generation");
        }

        private void FormatFileTemplates(IEnumerable<Template> templates)
        {
            writer.WriteLine();
            writer.WriteLine("### File Templates");
            writer.WriteLine();
            writer.WriteLine("Description |");
            writer.WriteLine("------------|");
            foreach (var template in templates.OrderBy(t => t.Description))
                writer.WriteLine("[{0}]({1}) |", template.Description, template.InputFile);
            
            writer.WriteLine();
        }
    }
}