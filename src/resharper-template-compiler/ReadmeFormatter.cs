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

            foreach (var category in templatesByCategory.Keys.OrderBy(s => s))
            {
                writer.WriteLine("## {0}", category);
                writer.WriteLine();
                writer.WriteLine("Shortcut | Description");
                writer.WriteLine("---------|------------");
                foreach (var template in templatesByCategory[category].OrderBy(t => t.Shortcut))
                    writer.WriteLine("{0} | {1}", template.Shortcut, template.Description);

                writer.WriteLine();
            }
        }
    }
}