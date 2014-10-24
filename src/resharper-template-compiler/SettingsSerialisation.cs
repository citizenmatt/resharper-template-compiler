using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Xml;

namespace CitizenMatt.ReSharper.TemplateCompiler
{
    public class SettingsSerialisation
    {
        private readonly IDictionary<string, object> dictionary;

        public SettingsSerialisation(IDictionary<string, object> dictionary)
        {
            this.dictionary = dictionary;
        }

        public SettingsStore GetSettings(params string[] pathSegments)
        {
            return new SettingsStore(dictionary, "/" + string.Join("/", pathSegments));
        }

        public static void SerialiseToXaml(TextWriter textWriter, SettingsSerialisation serialisation)
        {
            //var resourceDictionary = new ResourceDictionary();
            //foreach (var pair in serialisation.dictionary)
            //    resourceDictionary.Add(pair.Key, pair.Value);

            // Writes correctly, but in indeterminate order, annoyingly
            //var xmlWriter = XmlWriter.Create(textWriter, new XmlWriterSettings
            //{
            //    Indent = true
            //});
            //XamlWriter.Save(resourceDictionary, xmlWriter);

            using (var xmlWriter = XmlWriter.Create(textWriter, new XmlWriterSettings
            {
                ConformanceLevel = ConformanceLevel.Auto,
                Indent = true,
                IndentChars = "\t",
                NewLineHandling = NewLineHandling.Entitize
            }))
            {
                xmlWriter.WriteStartElement("wpf", "ResourceDictionary",
                    "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                xmlWriter.WriteAttributeString("xml", "space", null, "preserve");
                xmlWriter.WriteAttributeString("xmlns", "x", null, "http://schemas.microsoft.com/winfx/2006/xaml");
                xmlWriter.WriteAttributeString("xmlns", "s", null, "clr-namespace:System;assembly=mscorlib");
                xmlWriter.WriteAttributeString("xmlns", "ss", null, "urn:shemas-jetbrains-com:settings-storage-xaml");
                foreach (var entry in serialisation.dictionary.OrderBy(p => p.Key).Where(p => p.Value != null))
                {
                    xmlWriter.WriteStartElement("s", entry.Value.GetType().Name,
                        "clr-namespace:System;assembly=mscorlib");
                    xmlWriter.WriteAttributeString("x", "Key", "http://schemas.microsoft.com/winfx/2006/xaml", entry.Key);
                    xmlWriter.WriteValue(entry.Value);
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
            }
        }

        public static SettingsDeserialisation DeserialiseFromXaml(TextReader textReader)
        {
            using(var xmlReader = XmlReader.Create(textReader))
            {
                var resourceDictionary = (ResourceDictionary)XamlReader.Load(xmlReader);
                var dictionary = new Dictionary<string, object>();
                foreach (var key in resourceDictionary.Keys)
                    dictionary.Add((string) key, resourceDictionary[key]);
                return new SettingsDeserialisation(dictionary);
            }
        }
    }
}