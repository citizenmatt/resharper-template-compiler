using System;
using System.Collections.Generic;
using System.Linq;

namespace CitizenMatt.ReSharper.TemplateCompiler
{
    public class SettingsDeserialisation
    {
        private static readonly string[] RootPath = {"Default", "PatternsAndTemplates", "LiveTemplates", "Template"};

        private readonly Trie trie = new Trie();

        public SettingsDeserialisation(IDictionary<string, object> dictionary)
        {
            foreach (var pair in dictionary)
                trie.Add(pair.Key.Split('/').Where(s => !string.IsNullOrEmpty(s)), pair.Value);
        }

        public IList<Template> DeserialiseTemplates()
        {
            var templates = new List<Template>();

            var guids = GetTemplateGuids();
            foreach (var guid in guids)
            {
                var template = DeserialiseTemplate(guid);
                templates.Add(template);
            }

            return templates;
        }

        private Template DeserialiseTemplate(Guid guid)
        {
            var templateKey = SerialisationMetadata.FormatKey(SerialisationMetadata.FormatGuid(guid));
            var templatePath = MakePath(RootPath, templateKey);
            var template = new Template {Guid = guid};
            var index = GetIndexedValues<bool>(templatePath, "Applicability").First().Key;
            Enum.TryParse(index, out template.Type);
            template.Shortcut = GetValue<string>(templatePath, "Shortcut");
            string description;
            if (!TryGetValue(templatePath, "Description", out description))
                Console.WriteLine("Warning: Template {0} does not have a description.", template.Shortcut);
            template.Description = description;
            template.Text = GetValue<string>(templatePath, "Text");
            template.Reformat = TryGetValue(templatePath, "Reformat", true);
            template.ShortenQualifiedReferences = TryGetValue(templatePath, "ShortenQualifiedReferences", true);
            template.Scopes = DeserialiseScopes(templatePath);
            template.Categories = DeserialiseCategories(templatePath);
            template.Fields = DeserialiseFields(templatePath);
            return template;
        }

        private IList<Field> DeserialiseFields(IList<string> templatePath)
        {
            var fields = new Dictionary<long, Field>();
            var fieldsPath = MakePath(templatePath, "Field");

            foreach (var key in EnumerateKeys(fieldsPath))
            {
                var fieldPath = MakePath(fieldsPath, SerialisationMetadata.FormatKey(key));
                bool editable = true;
                long initialRange;
                if (TryGetValue(fieldPath, "InitialRange", out initialRange))
                    editable = initialRange != -1;
                string expression;
                TryGetValue(fieldPath, "Expression", out expression);
                var order = GetValue<long>(fieldPath, "Order");
                var field = new Field
                {
                    Name = key,
                    Editable = editable,
                    Expression = expression
                };
                fields.Add(order, field);
            }

            return fields.OrderBy(f => f.Key).Select(f => f.Value).ToList();
        }

        private IList<string> DeserialiseCategories(IList<string> templatePath)
        {
            return GetIndexedValues<string>(templatePath, "Categories").Values.ToList();
        }

        private IList<Scope> DeserialiseScopes(IList<string> templatePath)
        {
            var scopes = new List<Scope>();

            var guids = GetScopeGuids(templatePath);
            foreach (var guid in guids)
            {
                var scope = DeserialiseScope(templatePath, guid);
                scopes.Add(scope);
            }

            return scopes;
        }

        private Scope DeserialiseScope(IEnumerable<string> templatePath, Guid guid)
        {
            var scopePath = MakePath(templatePath, "Scope",
                SerialisationMetadata.FormatKey(SerialisationMetadata.FormatGuid(guid)));
            var scope = new Scope
            {
                Guid = guid,
                Type = GetValue<string>(scopePath, "Type"),
                Parameters = DeserialiseScopeParameters(scopePath)
            };
            return scope;
        }

        private IDictionary<string, string> DeserialiseScopeParameters(IList<string> scopePath)
        {
            return GetIndexedValues<string>(scopePath, "CustomProperties");
        }

        private IEnumerable<Guid> GetTemplateGuids()
        {
            return from key in EnumerateKeys(RootPath)
                select SerialisationMetadata.ParseGuid(key);
        }

        private IEnumerable<Guid> GetScopeGuids(IEnumerable<string> templatePath)
        {
            return from key in EnumerateKeys(MakePath(templatePath, "Scope"))
                   select SerialisationMetadata.ParseGuid(key);
        }

        private static IList<string> MakePath(IEnumerable<string> rootPath, params string[] segments)
        {
            return rootPath.Concat(segments).ToList();
        }

        private IEnumerable<string> EnumerateKeys(IEnumerable<string> path)
        {
            return from key in trie.GetChildKeys(path)
                select SerialisationMetadata.ParseKey(key);
        }

        private IDictionary<string, T> GetIndexedValues<T>(IList<string> path, string name)
        {
            var dictionary = new Dictionary<string, T>();
            var keys = trie.GetChildKeys(path.Concat(new[] {name}));
            foreach (var index in keys)
            {
                var value = trie.GetValue(MakePath(path, name, index, SerialisationMetadata.EntryIndexedValue));
                var key = SerialisationMetadata.ParseKey(index);
                dictionary.Add(key, (T)value);
            }
            return dictionary;
        }

        private T GetValue<T>(IEnumerable<string> path, string name)
        {
            return (T) trie.GetValue(MakePath(path, name, SerialisationMetadata.EntryValue));
        }

        private T TryGetValue<T>(IEnumerable<string> path, string name, T defaultValue)
        {
            T value;
            return TryGetValue(path, name, out value) ? value : defaultValue;
        }

        private bool TryGetValue<T>(IEnumerable<string> path, string name, out T value)
        {
            value = default(T);
            object o;
            if (trie.TryGetValue(MakePath(path, name, SerialisationMetadata.EntryValue), out o))
            {
                value = o == null ? default(T) : (T) Convert.ChangeType(o, typeof(T));
                return true;
            }
            return false;
        }
    }
}