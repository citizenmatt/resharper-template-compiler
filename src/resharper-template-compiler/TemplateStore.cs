using System;
using System.Collections.Generic;

namespace CitizenMatt.ReSharper.TemplateCompiler
{
    public class TemplateStore
    {
        private readonly Dictionary<Guid, Template> templates = new Dictionary<Guid, Template>();
        private readonly SettingsStore liveTemplateSettings;

        public TemplateStore(SettingsSerialisation settingsSerialisation)
        {
            liveTemplateSettings = settingsSerialisation.GetSettings("Default", "PatternsAndTemplates", "LiveTemplates");
        }

        public IEnumerable<Template> Templates => templates.Values;

        public void AddTemplate(Template template)
        {
            if (templates.ContainsKey(template.Guid))
                throw new InvalidOperationException($"Duplicate template {template.Guid}");

            templates.Add(template.Guid, template);

            var templateSettings = liveTemplateSettings.AddIndexedSettings("Template", template.Guid);
            templateSettings.AddValue("Shortcut", template.Shortcut);
            templateSettings.AddValue("Description", template.Description);
            templateSettings.AddValue("Text", template.Text);
            templateSettings.AddValue("Image", template.Image);
            templateSettings.AddValue("Reformat", template.Reformat);
            templateSettings.AddValue("ShortenQualifiedReferences", template.ShortenQualifiedReferences);
            if (template.Type == TemplateType.Both)
            {
                templateSettings.AddIndexedValue("Applicability", nameof(TemplateType.Live), true);
                templateSettings.AddIndexedValue("Applicability", nameof(TemplateType.Surround), true);
            }
            else
                templateSettings.AddIndexedValue("Applicability", template.Type.ToString(), true);
            AddCustomProperties(templateSettings, template.CustomProperties);
            AddCategories(templateSettings, template.Categories);
            AddScopes(templateSettings, template.Scopes);
            AddFields(templateSettings, template.Fields);
            templateSettings.AddValue("UITag", template.UITag);
        }

        private static void AddCustomProperties(SettingsStore store, IDictionary<string, string> customProperties)
        {
            foreach (var property in customProperties)
                store.AddIndexedValue("CustomProperties", property.Key, property.Value);
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

                // I like the editable flag, as we use that most, but don't let it override editable instance
                long initialRange = field.Editable ? 0 : -1;
                if (field.EditableInstance > 0)
                    initialRange = field.EditableInstance;
                if (initialRange != 0)
                    fieldSettings.AddValue("InitialRange", initialRange);
                // TODO: File sections
            }
        }
    }
}