using System;
using CitizenMatt.ReSharper.TemplateCompiler;
using NUnit.Framework;

namespace tests
{
    public class GuidSerialisationTests
    {
        [Test]
        public void Should_deserialise_correctly()
        {
            var guid = new Guid("ABB24050-BC0E-4AE7-A396-E0534EA8E95D");
            var formatted = SerialisationMetadata.FormatGuid(guid);

            Assert.AreEqual("5040B2AB0EBCE74AA396E0534EA8E95D", formatted);
        }

        [Test]
        public void Should_serialise_correctly()
        {
            const string formatted = "5040B2AB0EBCE74AA396E0534EA8E95D";
            var guid = SerialisationMetadata.ParseGuid(formatted);

            Assert.AreEqual(guid, new Guid("ABB24050-BC0E-4AE7-A396-E0534EA8E95D"));
        }
    }
}