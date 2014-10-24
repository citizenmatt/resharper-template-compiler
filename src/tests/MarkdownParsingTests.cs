using System;
using System.IO;
using System.Linq;
using CitizenMatt.ReSharper.TemplateCompiler;
using CitizenMatt.ReSharper.TemplateCompiler.Markdown;
using CommonMark;
using NUnit.Framework;

namespace tests
{
    public class MarkdownParsingTests
    {
        [Test]
        public void Should_parse_simple_template()
        {
            const string markdown =
@"---
guid: {04BEDCE3-7CC8-48CE-92C4-A95A72C8B0F6}
type: live
reformat: true
shortenReferences: true
---

# tda

xUnit.net [Theory]

```cs
[Xunit.Extensions.Theory]
```
";

            var parser = new TemplateParser();
            var template = parser.Parse(markdown);

            DumpTree(markdown);

            Assert.NotNull(template);
            Assert.AreEqual(new Guid("{04BEDCE3-7CC8-48CE-92C4-A95A72C8B0F6}"), template.Guid);
            Assert.AreEqual(TemplateType.Live, template.Type);
            Assert.AreEqual("tda", template.Shortcut);
            Assert.AreEqual("xUnit.net [Theory]", template.Description);
            Assert.AreEqual("[Xunit.Extensions.Theory]", template.Text);
            Assert.AreEqual(true, template.Reformat);
            Assert.AreEqual(true, template.ShortenQualifiedReferences);
            Assert.IsEmpty(template.Categories);
            Assert.IsEmpty(template.Scopes);
            Assert.IsEmpty(template.Fields);
        }

        [Test]
        public void Should_parse_single_category()
        {
            const string markdown =
@"---
guid: {04BEDCE3-7CC8-48CE-92C4-A95A72C8B0F6}
type: Live
reformat: true
shortenReferences: true
categories: xunit
---

# tda

xUnit.net [Theory]

```cs
[Xunit.Extensions.Theory]
```
";

            var parser = new TemplateParser();
            var template = parser.Parse(markdown);

            DumpTree(markdown);

            Assert.NotNull(template);
            CollectionAssert.AreEqual(new[] { "xunit" }, template.Categories);
        }

        [Test]
        public void Should_parse_multiple_categories()
        {
            const string markdown =
@"---
guid: {04BEDCE3-7CC8-48CE-92C4-A95A72C8B0F6}
type: Live
reformat: true
shortenReferences: true
categories: xunit, xunit-attributes
---

# tda

xUnit.net [Theory]

```cs
[Xunit.Extensions.Theory]
```
";

            var parser = new TemplateParser();
            var template = parser.Parse(markdown);

            DumpTree(markdown);

            Assert.NotNull(template);
            CollectionAssert.AreEqual(new[] { "xunit", "xunit-attributes" }, template.Categories);
        }

        [Test]
        public void Should_parse_single_scope_with_no_properties()
        {
            const string markdown =
@"---
guid: {04BEDCE3-7CC8-48CE-92C4-A95A72C8B0F6}
type: Live
reformat: true
shortenReferences: true
scopes: InCSharpStatement
---

# tda

xUnit.net [Theory]

```cs
[Xunit.Extensions.Theory]
```
";

            var parser = new TemplateParser();
            var template = parser.Parse(markdown);

            DumpTree(markdown);

            Assert.NotNull(template);
            Assert.AreEqual(1, template.Scopes.Count);
            Assert.AreEqual("InCSharpStatement", template.Scopes[0].Type);
            Assert.AreEqual(0, template.Scopes[0].Parameters.Count);
        }


        [Test]
        public void Should_parse_multiple_scopes_with_no_properties()
        {
            const string markdown =
@"---
guid: {04BEDCE3-7CC8-48CE-92C4-A95A72C8B0F6}
type: Live
reformat: true
shortenReferences: true
scopes: InCSharpStatement; InJSStament
---

# tda

xUnit.net [Theory]

```cs
[Xunit.Extensions.Theory]
```
";

            var parser = new TemplateParser();
            var template = parser.Parse(markdown);

            DumpTree(markdown);

            Assert.NotNull(template);
            Assert.AreEqual(2, template.Scopes.Count);
            Assert.AreEqual("InCSharpStatement", template.Scopes[0].Type);
            Assert.AreEqual(0, template.Scopes[0].Parameters.Count);
            Assert.AreEqual("InJSStament", template.Scopes[1].Type);
            Assert.AreEqual(0, template.Scopes[1].Parameters.Count);
        }

        [Test]
        public void Should_parse_single_scope_with_single_parameters()
        {
            const string markdown =
@"---
guid: {04BEDCE3-7CC8-48CE-92C4-A95A72C8B0F6}
type: Live
reformat: true
shortenReferences: true
scopes: InCSharpStatement(minimumVersion = 2.0)
---

# tda

xUnit.net [Theory]

```cs
[Xunit.Extensions.Theory]
```
";

            var parser = new TemplateParser();
            var template = parser.Parse(markdown);

            DumpTree(markdown);

            Assert.NotNull(template);
            Assert.AreEqual(1, template.Scopes.Count);
            Assert.AreEqual("InCSharpStatement", template.Scopes[0].Type);
            var parameters = template.Scopes[0].Parameters;
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual("minimumVersion", parameters.First().Key);
            Assert.AreEqual("2.0", parameters.First().Value);
        }

        [Test]
        public void Should_parse_single_scope_with_multiple_parameters()
        {
            const string markdown =
@"---
guid: {04BEDCE3-7CC8-48CE-92C4-A95A72C8B0F6}
type: Live
reformat: true
shortenReferences: true
scopes: InCSharpStatement(minimumVersion = 2.0, something = whatever)
---

# tda

xUnit.net [Theory]

```cs
[Xunit.Extensions.Theory]
```
";

            var parser = new TemplateParser();
            var template = parser.Parse(markdown);

            DumpTree(markdown);

            Assert.NotNull(template);
            Assert.AreEqual(1, template.Scopes.Count);
            Assert.AreEqual("InCSharpStatement", template.Scopes[0].Type);
            var parameters = template.Scopes[0].Parameters;
            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual("minimumVersion", parameters.First().Key);
            Assert.AreEqual("2.0", parameters.First().Value);
            Assert.AreEqual("something", parameters.Skip(1).First().Key);
            Assert.AreEqual("whatever", parameters.Skip(1).First().Value);
        }

        [Test]
        public void Should_parse_single_field()
        {
            const string markdown =
@"---
guid: {04BEDCE3-7CC8-48CE-92C4-A95A72C8B0F6}
type: Live
reformat: true
shortenReferences: true
parameterOrder: var1
---

# tda

xUnit.net [Theory]

```cs
$var1$
```
";

            var parser = new TemplateParser();
            var template = parser.Parse(markdown);

            DumpTree(markdown);

            Assert.NotNull(template);
            Assert.AreEqual(1, template.Fields.Count);
            Assert.AreEqual("var1", template.Fields[0].Name);
            Assert.AreEqual(true, template.Fields[0].Editable);
            Assert.IsEmpty(template.Fields[0].Expression);
        }

        [Test]
        public void Should_parse_single_field_with_expression()
        {
            const string markdown =
@"---
guid: {04BEDCE3-7CC8-48CE-92C4-A95A72C8B0F6}
type: Live
reformat: true
shortenReferences: true
parameterOrder: var1
var1-expression: completeSmart()
---

# tda

xUnit.net [Theory]

```cs
$var1$
```
";

            var parser = new TemplateParser();
            var template = parser.Parse(markdown);

            DumpTree(markdown);

            Assert.NotNull(template);
            Assert.AreEqual(1, template.Fields.Count);
            Assert.AreEqual("var1", template.Fields[0].Name);
            Assert.IsTrue(template.Fields[0].Editable);
            Assert.AreEqual("completeSmart()", template.Fields[0].Expression);
        }

        [Test]
        public void Should_parse_multiple_fields()
        {
            const string markdown =
@"---
guid: {04BEDCE3-7CC8-48CE-92C4-A95A72C8B0F6}
type: Live
reformat: true
shortenReferences: true
parameterOrder: var1, var2
var1-expression: completeSmart()
var2-expression: constant(""true"")
---

# tda

xUnit.net [Theory]

```cs
$var1$
```
";

            var parser = new TemplateParser();
            var template = parser.Parse(markdown);

            DumpTree(markdown);

            Assert.NotNull(template);
            Assert.AreEqual(2, template.Fields.Count);
            Assert.AreEqual("var1", template.Fields[0].Name);
            Assert.IsTrue(template.Fields[0].Editable);
            Assert.AreEqual("completeSmart()", template.Fields[0].Expression);
            Assert.AreEqual("var2", template.Fields[1].Name);
            Assert.IsTrue(template.Fields[1].Editable);
            Assert.AreEqual("constant(\"true\")", template.Fields[1].Expression);
        }

        [Test]
        public void Should_parse_non_editable_fields()
        {
            const string markdown =
@"---
guid: {04BEDCE3-7CC8-48CE-92C4-A95A72C8B0F6}
type: Live
reformat: true
shortenReferences: true
parameterOrder: var1, (var2), var3
var1-expression: completeSmart()
var2-expression: constant(""true"")
var3-expression: completeSmart()
---

# tda

xUnit.net [Theory]

```cs
$var1$
```
";

            var parser = new TemplateParser();
            var template = parser.Parse(markdown);

            DumpTree(markdown);

            Assert.NotNull(template);
            Assert.AreEqual(3, template.Fields.Count);
            Assert.AreEqual("var1", template.Fields[0].Name);
            Assert.IsTrue(template.Fields[0].Editable);
            Assert.AreEqual("completeSmart()", template.Fields[0].Expression);
            Assert.AreEqual("var2", template.Fields[1].Name);
            Assert.IsFalse(template.Fields[1].Editable);
            Assert.AreEqual("constant(\"true\")", template.Fields[1].Expression);
            Assert.AreEqual("var3", template.Fields[2].Name);
            Assert.IsTrue(template.Fields[2].Editable);
            Assert.AreEqual("completeSmart()", template.Fields[2].Expression);
        }

        private static void DumpTree(string markdown)
        {
            using (var stringReader = new StringReader(markdown))
            {
                var document = CommonMarkConverter.ProcessStage1(stringReader);
                CommonMarkConverter.ProcessStage2(document);

                CommonMarkConverter.ProcessStage3(document, Console.Out, new CommonMarkSettings
                {
                    OutputFormat = OutputFormat.SyntaxTree
                });
            }

            Console.WriteLine();
            Console.WriteLine();
        }
    }
}