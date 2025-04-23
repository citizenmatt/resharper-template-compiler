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
        public void Should_round_trip_live_template()
        {
            var template = GetBaseLiveTemplate();

            var dictionary = Serialise(template);
            var deserialiser = new SettingsDeserialisation(dictionary);
            var templates = deserialiser.DeserialiseTemplates();

            Assert.NotNull(templates);
            Assert.AreEqual(1, templates.Count);
            Assert.AreEqual(template.Guid, templates[0].Guid);
            Assert.AreEqual(TemplateType.Live, templates[0].Type);
            Assert.AreEqual(template.Type, templates[0].Type);
            Assert.AreEqual(template.Shortcut, templates[0].Shortcut);
            Assert.AreEqual(template.Description, templates[0].Description);
            Assert.AreEqual(template.Text, templates[0].Text);
            Assert.AreEqual(template.Reformat, templates[0].Reformat);
            Assert.AreEqual(template.ShortenQualifiedReferences, templates[0].ShortenQualifiedReferences);
        }

        [Test]
        public void Should_round_trip_surround_template()
        {
            var template = GetBaseLiveTemplate();
            template.Type = TemplateType.Surround;

            var dictionary = Serialise(template);
            var deserialiser = new SettingsDeserialisation(dictionary);
            var templates = deserialiser.DeserialiseTemplates();

            Assert.NotNull(templates);
            Assert.AreEqual(1, templates.Count);
            Assert.AreEqual(template.Guid, templates[0].Guid);
            Assert.AreEqual(TemplateType.Surround, templates[0].Type);
            Assert.AreEqual(template.Type, templates[0].Type);
            Assert.AreEqual(template.Shortcut, templates[0].Shortcut);
            Assert.AreEqual(template.Description, templates[0].Description);
            Assert.AreEqual(template.Text, templates[0].Text);
            Assert.AreEqual(template.Reformat, templates[0].Reformat);
            Assert.AreEqual(template.ShortenQualifiedReferences, templates[0].ShortenQualifiedReferences);
        }

        [Test]
        public void Should_round_trip_live_and_surround_template()
        {
            var template = GetBaseLiveTemplate();
            template.Type = TemplateType.Both;

            var dictionary = Serialise(template);
            var deserialiser = new SettingsDeserialisation(dictionary);
            var templates = deserialiser.DeserialiseTemplates();

            Assert.NotNull(templates);
            Assert.AreEqual(1, templates.Count);
            Assert.AreEqual(template.Guid, templates[0].Guid);
            Assert.AreEqual(TemplateType.Both, templates[0].Type);
            Assert.AreEqual(template.Type, templates[0].Type);
            Assert.AreEqual(template.Shortcut, templates[0].Shortcut);
            Assert.AreEqual(template.Description, templates[0].Description);
            Assert.AreEqual(template.Text, templates[0].Text);
            Assert.AreEqual(template.Reformat, templates[0].Reformat);
            Assert.AreEqual(template.ShortenQualifiedReferences, templates[0].ShortenQualifiedReferences);
        }


        [Test]
        public void Should_round_trip_file_template()
        {
            var template = GetBaseFileTemplate();

            var dictionary = Serialise(template);
            var deserialiser = new SettingsDeserialisation(dictionary);
            var templates = deserialiser.DeserialiseTemplates();

            Assert.NotNull(templates);
            Assert.AreEqual(1, templates.Count);
            Assert.AreEqual(template.Guid, templates[0].Guid);
            Assert.AreEqual(TemplateType.File, templates[0].Type);
            Assert.AreEqual(template.Type, templates[0].Type);
            Assert.Null(templates[0].Shortcut);
            Assert.AreEqual(template.Description, templates[0].Description);
            Assert.AreEqual(template.Text, templates[0].Text);
            Assert.AreEqual(template.Reformat, templates[0].Reformat);
            Assert.AreEqual(template.ShortenQualifiedReferences, templates[0].ShortenQualifiedReferences);
        }

        [Test]
        public void Should_round_trip_single_scope()
        {
            var template = GetBaseLiveTemplate();
            template.Scopes.Add(new Scope("InCSharpStatement", Guid.NewGuid()));

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
            var template = GetBaseLiveTemplate();
            template.Scopes.Add(new Scope("InCSharpStatement", Guid.NewGuid())
            {
                Parameters = new Dictionary<string, string>
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
            var template = GetBaseLiveTemplate();
            template.Scopes.Add(new Scope("InCSharpStatement", Guid.NewGuid())
            {
                Parameters = new Dictionary<string, string>
                {
                    {"minimumLanguageVersion", "2.0"},
                    {"foo", "bar"}
                }
            });
            template.Scopes.Add(new Scope("InJSStatement", Guid.NewGuid())
            {
                Parameters = new Dictionary<string, string>
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
            var template = GetBaseLiveTemplate();
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
            var template = GetBaseLiveTemplate();
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
            Assert.IsTrue(templates[0].Fields[0].Editable);
            Assert.AreEqual("constant(\"true\")", templates[0].Fields[0].Expression);
        }

        [Test]
        public void Should_round_trip_multiple_fields()
        {
            var template = GetBaseLiveTemplate();
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
                Expression = "constant(\"false\")",
                EditableInstance = 2
            });

            var dictionary = Serialise(template);
            var deserialiser = new SettingsDeserialisation(dictionary);
            var templates = deserialiser.DeserialiseTemplates();

            Assert.AreEqual(2, templates[0].Fields.Count);
            Assert.AreEqual("var1", templates[0].Fields[0].Name);
            Assert.IsTrue(templates[0].Fields[0].Editable);
            Assert.AreEqual("constant(\"true\")", templates[0].Fields[0].Expression);

            Assert.AreEqual("var2", templates[0].Fields[1].Name);
            Assert.IsTrue(templates[0].Fields[1].Editable);
            Assert.AreEqual("constant(\"false\")", templates[0].Fields[1].Expression);
            Assert.AreEqual(2, templates[0].Fields[1].EditableInstance);
        }

        [Test]
        public void Should_round_trip_image()
        {
            var template = GetBaseLiveTemplate();
            template.Image = "MyTemplateImage";

            var dictionary = Serialise(template);
            var deserialiser = new SettingsDeserialisation(dictionary);
            var templates = deserialiser.DeserialiseTemplates();

            Assert.AreEqual(1, templates.Count);
            Assert.AreEqual("MyTemplateImage", templates[0].Image);
        }

        private Template GetBaseLiveTemplate()
        {
            return new Template
            {
                Guid = new Guid("0204BF84-37E4-4117-9602-BCCDCE369341"),
                Type = TemplateType.Live,
                Shortcut = "shortcut",
                Description = "description",
                Text = "text",
                Reformat = true,
                ShortenQualifiedReferences = true
            };
        }

        private Template GetBaseFileTemplate()
        {
            return new Template
            {
                Guid = new Guid("504C64D1-352C-4841-B1F1-DCE0829A63BD"),
                Type = TemplateType.File,
                Description = "description",
                Text = "text",
                Reformat = true,
                Scopes = new List<Scope> { new Scope("InCSharpProject", Guid.NewGuid()) },
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
