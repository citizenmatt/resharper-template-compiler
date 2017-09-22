using System;
using System.Collections.Generic;

namespace CitizenMatt.ReSharper.TemplateCompiler
{
    public class SettingsStore
    {
        private readonly IDictionary<string, object> dictionary;
        private readonly string path;

        public SettingsStore(IDictionary<string, object> dictionary, string path)
        {
            this.dictionary = dictionary;
            this.path = path;
        }

        public SettingsStore AddIndexedSettings(string name, Guid index)
        {
            return AddIndexedSettings(name, SerialisationMetadata.FormatGuid(index));
        }

        public SettingsStore AddIndexedSettings(string name, string index)
        {
            var newPath = Combine(path, name, SerialisationMetadata.FormatKey(index));
            var settings = new SettingsStore(dictionary, newPath);
            settings.Add(SerialisationMetadata.KeyIndexDefined, true);
            return settings;
        }

        public void AddValue(string name, object value)
        {
            if (value == null)
                return;
            var relativeValuePath = Combine(name, SerialisationMetadata.EntryValue);
            Add(relativeValuePath, value);
        }

        public void AddIndexedValue(string name, string key, object value)
        {
            Add(Combine(name, SerialisationMetadata.FormatKey(key), SerialisationMetadata.EntryIndexedValue), value);
        }

        private void Add(string relativePath, object value)
        {
            dictionary.Add(Combine(path, relativePath), value);
        }

        private static string Combine(params string[] segments)
        {
            return string.Join("/", segments);
        }
    }
}