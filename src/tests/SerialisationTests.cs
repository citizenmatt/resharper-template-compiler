using System;
using System.Collections.Generic;
using System.Linq;
using CitizenMatt.ReSharper.TemplateCompiler;
using NUnit.Framework;

namespace tests
{
    public class SerialisationTests
    {
        [Test]
        public void Should_round_trip_simple_template()
        {
            var template = GetBaseTemplate();

            var dictionary = Serialise(template);
            var deserialiser = new SettingsDeserialisation(dictionary);
            var templates = deserialiser.DeserialiseTemplates();

            Assert.NotNull(templates);
            Assert.AreEqual(1, templates.Count);
            Assert.AreEqual(template.Guid, templates[0].Guid);
            Assert.AreEqual(template.Type, templates[0].Type);
            Assert.AreEqual(template.Shortcut, templates[0].Shortcut);
            Assert.AreEqual(template.Description, templates[0].Description);
            Assert.AreEqual(template.Text, templates[0].Text);
            Assert.AreEqual(template.Reformat, templates[0].Reformat);
            Assert.AreEqual(template.ShortenQualifiedReferences, templates[0].ShortenQualifiedReferences);
        }

        [Test]
        public void Should_round_trip_single_scope()
        {
            var template = GetBaseTemplate();
            template.Scopes.Add(new Scope
            {
                Guid = Guid.NewGuid(),
                Type = "InCSharpStatement"
            });

            var dictionary = Serialise(template);
            var deserialiser = new SettingsDeserialisation(dictionary);
            var templates = deserialiser.DeserialiseTemplates();

            Assert.AreEqual(1, templates[0].Scopes.Count);
            Assert.AreEqual(template.Scopes[0].Guid, templates[0].Scopes[0].Guid);
            Assert.AreEqual(template.Scopes[0].Type, templates[0].Scopes[0].Type);
            Assert.IsEmpty(templates[0].Scopes[0].Parameters);
        }

        [Test]
        public void Should_round_trip_single_scope_with_parameters()
        {
            var template = GetBaseTemplate();
            template.Scopes.Add(new Scope
            {
                Guid = Guid.NewGuid(),
                Type = "InCSharpStatement",
                Parameters = new Dictionary<string, string>()
                {
                    {"minimumLanguageVersion", "2.0"},
                    {"foo", "bar"}
                }
            });

            var dictionary = Serialise(template);
            var deserialiser = new SettingsDeserialisation(dictionary);
            var templates = deserialiser.DeserialiseTemplates();

            Assert.AreEqual(template.Scopes[0].Parameters.Count, templates[0].Scopes[0].Parameters.Count);
            Assert.AreEqual(template.Scopes[0].Parameters.First().Key, templates[0].Scopes[0].Parameters.First().Key);
            Assert.AreEqual(template.Scopes[0].Parameters.First().Value, templates[0].Scopes[0].Parameters.First().Value);
            Assert.AreEqual(template.Scopes[0].Parameters.Skip(1).First().Key, templates[0].Scopes[0].Parameters.Skip(1).First().Key);
            Assert.AreEqual(template.Scopes[0].Parameters.Skip(1).First().Value, templates[0].Scopes[0].Parameters.Skip(1).First().Value);
        }

        [Test]
        public void Should_round_trip_multiple_scopes()
        {
            var template = GetBaseTemplate();
            template.Scopes.Add(new Scope
            {
                Guid = Guid.NewGuid(),
                Type = "InCSharpStatement",
                Parameters = new Dictionary<string, string>()
                {
                    {"minimumLanguageVersion", "2.0"},
                    {"foo", "bar"}
                }
            });
            template.Scopes.Add(new Scope
            {
                Guid = Guid.NewGuid(),
                Type = "InJSStatement",
                Parameters = new Dictionary<string, string>()
                {
                    {"minimumLanguageVersion", "2.0"},
                    {"foo", "bar"}
                }
            });

            var dictionary = Serialise(template);
            var deserialiser = new SettingsDeserialisation(dictionary);
            var templates = deserialiser.DeserialiseTemplates();

            Assert.AreEqual(2, templates[0].Scopes.Count);
            Assert.AreEqual(template.Scopes[0].Guid, templates[0].Scopes[0].Guid);
            Assert.AreEqual(template.Scopes[0].Type, templates[0].Scopes[0].Type);
            Assert.AreEqual(template.Scopes[0].Parameters.Count, templates[0].Scopes[0].Parameters.Count);
            Assert.AreEqual(template.Scopes[0].Parameters.First().Key, templates[0].Scopes[0].Parameters.First().Key);
            Assert.AreEqual(template.Scopes[0].Parameters.First().Value, templates[0].Scopes[0].Parameters.First().Value);
            Assert.AreEqual(template.Scopes[0].Parameters.Skip(1).First().Key, templates[0].Scopes[0].Parameters.Skip(1).First().Key);
            Assert.AreEqual(template.Scopes[0].Parameters.Skip(1).First().Value, templates[0].Scopes[0].Parameters.Skip(1).First().Value);

            Assert.AreEqual(template.Scopes[1].Guid, templates[0].Scopes[1].Guid);
            Assert.AreEqual(template.Scopes[1].Type, templates[0].Scopes[1].Type);
            Assert.AreEqual(template.Scopes[1].Parameters.Count, templates[0].Scopes[1].Parameters.Count);
            Assert.AreEqual(template.Scopes[1].Parameters.First().Key, templates[0].Scopes[1].Parameters.First().Key);
            Assert.AreEqual(template.Scopes[1].Parameters.First().Value, templates[0].Scopes[1].Parameters.First().Value);
            Assert.AreEqual(template.Scopes[1].Parameters.Skip(1).First().Key, templates[0].Scopes[1].Parameters.Skip(1).First().Key);
            Assert.AreEqual(template.Scopes[1].Parameters.Skip(1).First().Value, templates[0].Scopes[1].Parameters.Skip(1).First().Value);
        }

        [Test]
        public void Should_round_trip_categories()
        {
            var template = GetBaseTemplate();
            template.Categories.Add("xunit");
            template.Categories.Add("xunit-assert");

            var dictionary = Serialise(template);
            var deserialiser = new SettingsDeserialisation(dictionary);
            var templates = deserialiser.DeserialiseTemplates();

            Assert.AreEqual(2, templates[0].Categories.Count);
            Assert.AreEqual("xunit", templates[0].Categories[0]);
            Assert.AreEqual("xunit-assert", templates[0].Categories[1]);
        }

        [Test]
        public void Should_round_trip_single_field()
        {
            var template = GetBaseTemplate();
            template.Fields.Add(new Field
            {
                Name = "var1",
                Editable = true,
                Expression = "constant(\"true\")"
            });

            var dictionary = Serialise(template);
            var deserialiser = new SettingsDeserialisation(dictionary);
            var templates = deserialiser.DeserialiseTemplates();

            Assert.AreEqual(1, templates[0].Fields.Count);
            Assert.AreEqual("var1", templates[0].Fields[0].Name);
            Assert.AreEqual(true, templates[0].Fields[0].Editable);
            Assert.AreEqual("constant(\"true\")", templates[0].Fields[0].Expression);
        }

        [Test]
        public void Should_round_trip_multiple_fields()
        {
            var template = GetBaseTemplate();
            template.Fields.Add(new Field
            {
                Name = "var1",
                Editable = true,
                Expression = "constant(\"true\")"
            });
            template.Fields.Add(new Field
            {
                Name = "var2",
                Editable = false,
                Expression = "constant(\"false\")"
            });

            var dictionary = Serialise(template);
            var deserialiser = new SettingsDeserialisation(dictionary);
            var templates = deserialiser.DeserialiseTemplates();

            Assert.AreEqual(2, templates[0].Fields.Count);
            Assert.AreEqual("var1", templates[0].Fields[0].Name);
            Assert.AreEqual(true, templates[0].Fields[0].Editable);
            Assert.AreEqual("constant(\"true\")", templates[0].Fields[0].Expression);

            Assert.AreEqual("var2", templates[0].Fields[1].Name);
            Assert.AreEqual(false, templates[0].Fields[1].Editable);
            Assert.AreEqual("constant(\"false\")", templates[0].Fields[1].Expression);
        }

        private Template GetBaseTemplate()
        {
            return new Template
            {
                Guid = new Guid("0204BF84-37E4-4117-9602-BCCDCE369341"),
                Type = TemplateType.Surround,
                Shortcut = "shortcut",
                Description = "description",
                Text = "text",
                Reformat = true,
                ShortenQualifiedReferences = true
            };
        }

        private IDictionary<string, object> Serialise(Template template)
        {
            var dictionary = new Dictionary<string, object>();
            var serialisation = new SettingsSerialisation(dictionary);
            var store = new TemplateStore(serialisation);
            store.AddTemplate(template);
            return dictionary;
        }
    }
}
