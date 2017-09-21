using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

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
            XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml";
            var dictionary = new Dictionary<string, object>();
            
            var document = XDocument.Load(textReader);
            var xElements = document.Root.Elements();
            foreach (var xElement in xElements)
            {
                var value = CoerceValue(xElement);
                var key = xElement.Attribute(ns + "Key").Value;
                dictionary.Add(key, value);
            }

            return new SettingsDeserialisation(dictionary);
        }

        private static object CoerceValue(XElement xElement)
        {
            switch (xElement.Name.LocalName)
            {
                case "String": return xElement.Value;
                case "Boolean": return bool.Parse(xElement.Value);
                case "Int64": return long.Parse(xElement.Value);
                default: throw new InvalidOperationException($"Unknown element type: {xElement.Name.LocalName}");
            }
        }
    }
}