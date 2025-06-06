﻿using System;
using System.Collections.Generic;
using System.IO;
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
            foreach (var keyValuePair in GetIndexedValues<bool>(templatePath, "Applicability"))
            {
                // "Both" is serialised as two entries - "Live" and "Surround", both of which will be true. If the user
                // changes this, the settings system might delete one of the entries, or write it with a value of false
                if (keyValuePair.Value)
                {
                    if (keyValuePair.Key == "Both")
                        throw new InvalidDataException("Both is not a valid value for ReSharper templates");
                    Enum.TryParse(keyValuePair.Key, out TemplateType type);
                    template.Type |= type;
                }
            }
            if (template.Type != TemplateType.File)
                template.Shortcut = GetValue<string>(templatePath, "Shortcut");
            if (!TryGetValue(templatePath, "Description", out string description))
                Console.WriteLine("Warning: Template {0} does not have a description.", template.Shortcut);
            template.Description = description;
            template.Text = GetValue<string>(templatePath, "Text");
            template.Image = TryGetValue(templatePath, "Image", (string) null);
            template.Reformat = TryGetValue(templatePath, "Reformat", true);
            template.ShortenQualifiedReferences = TryGetValue(templatePath, "ShortenQualifiedReferences", true);
            template.Scopes = DeserialiseScopes(templatePath);
            template.CustomProperties = DeserialiseCustomProperties(templatePath);
            template.Categories = DeserialiseCategories(templatePath);
            template.Fields = DeserialiseFields(templatePath);
            template.UITag = TryGetValue(templatePath, "UITag", (string) null);
            return template;
        }

        private IList<Field> DeserialiseFields(IList<string> templatePath)
        {
            var fields = new Dictionary<long, Field>();
            var fieldsPath = MakePath(templatePath, "Field");

            foreach (var key in EnumerateKeys(fieldsPath))
            {
                var fieldPath = MakePath(fieldsPath, SerialisationMetadata.FormatKey(key));
                var editable = true;
                if (TryGetValue(fieldPath, "InitialRange", out long initialRange))
                    editable = initialRange != -1;
                TryGetValue(fieldPath, "Expression", out string expression);
                var order = GetValue<long>(fieldPath, "Order");
                var field = new Field
                {
                    Name = key,
                    Editable = editable,
                    Expression = expression,
                    EditableInstance = initialRange
                };
                fields.Add(order, field);
            }

            return fields.OrderBy(f => f.Key).Select(f => f.Value).ToList();
        }

        private IDictionary<string, string> DeserialiseCustomProperties(IList<string> templatePath)
        {
            return GetIndexedValues<string>(templatePath, "CustomProperties");
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
            var scope = new Scope(GetValue<string>(scopePath, "Type"), guid)
            {
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
            return TryGetValue(path, name, out T value) ? value : defaultValue;
        }

        private bool TryGetValue<T>(IEnumerable<string> path, string name, out T value)
        {
            value = default(T);
            if (trie.TryGetValue(MakePath(path, name, SerialisationMetadata.EntryValue), out var o))
            {
                value = o == null ? default(T) : (T) Convert.ChangeType(o, typeof(T));
                return true;
            }
            return false;
        }
    }
}