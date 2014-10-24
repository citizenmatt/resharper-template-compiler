using System.Collections.Generic;

namespace CitizenMatt.ReSharper.TemplateCompiler
{
    public class TemplateStore
    {
        private readonly SettingsStore liveTemplateSettings;
        private readonly IList<Template> templates; 

        public TemplateStore(SettingsSerialisation settingsSerialisation)
        {
            liveTemplateSettings = settingsSerialisation.GetSettings("Default", "PatternsAndTemplates", "LiveTemplates");
            templates = new List<Template>();
        }

        public IEnumerable<Template> Templates
        {
            get { return templates; }
        }

        public void AddTemplate(Template template)
        {
            templates.Add(template);

            var templateSettings = liveTemplateSettings.AddIndexedSettings("Template", template.Guid);
            templateSettings.AddValue("Shortcut", template.Shortcut);
            templateSettings.AddValue("Description", template.Description);
            templateSettings.AddValue("Text", template.Text);
            templateSettings.AddValue("Reformat", template.Reformat);
            templateSettings.AddValue("ShortenQualifiedReferences", template.ShortenQualifiedReferences);
            templateSettings.AddIndexedValue("Applicability", template.Type.ToString(), true);
            AddCategories(templateSettings, template.Categories);
            AddScopes(templateSettings, template.Scopes);
            AddFields(templateSettings, template.Fields);
        }

        private static void AddCategories(SettingsStore templateSettings, IEnumerable<string> categories)
        {
            foreach (var category in categories)
                templateSettings.AddIndexedValue("Categories", category, category);
        }

        private static void AddScopes(SettingsStore templateSettings, IEnumerable<Scope> scopes)
        {
            foreach (var scope in scopes)
            {
                var scopeSettings = templateSettings.AddIndexedSettings("Scope", scope.Guid);
                scopeSettings.AddValue("Type", scope.Type);
                foreach (var parameter in scope.Parameters)
                    scopeSettings.AddIndexedValue("CustomProperties", parameter.Key, parameter.Value);
            }
        }

        private static void AddFields(SettingsStore templateSettings, IEnumerable<Field> fields)
        {
            long order = 0; // This needs to be serialised as an Int64
            foreach (var field in fields)
            {
                var fieldSettings = templateSettings.AddIndexedSettings("Field", field.Name);
                fieldSettings.AddValue("Order", order++);
                if (!string.IsNullOrEmpty(field.Expression))
                    fieldSettings.AddValue("Expression", field.Expression);
                if (!field.Editable)
                    fieldSettings.AddValue("InitialRange", -1);
                // TODO: File sections
            }
        }
    }
}