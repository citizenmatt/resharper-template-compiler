# ReSharper Template Compiler

A simple utility to create a ReSharper `.dotSettings` file for Live Template stored as structured markdown. It can run as a command line tool or as a NuGet package, integrated with your build. See below for usage details.

ReSharper's templates are very powerul, generating complex code snippets from a simple text shortcut, with fields tied to macros for code completion and suggestion, GUID creation, file names and more. And they can even be packaged up as extensions and distributed in the [Extension Manager](http://resharper-plugins.jetbrains.com/packages?q=Tags%3A%22templates%22).

But have you ever tried to work with the `.dotSettings` file format they're stored in?

It's most definitely a machine readable file format, and not intended for human consumption (although once you get to know it, you realise it's just an xml file of name/value pairs where each name is a hierarchical key path, but it's not, you know, cuddly).

Maintaining a template file requires mounting the file in ReSharper -> Manage Options and using the Template Explorer, but if you ever encounter a file in a source control repo, you're on your own.

This utility will decompile a `.dotSettings` file into a number of structured markdown files, which are much more readable, especially on GitHub, and can then compile the markdown files into a new `.dotSettings` file ready for distribution. When compiling, it will also create a simple README.md, listing all template shortcuts and descriptions, grouped by category.

And yes, I know this isn't really a compiler, but it takes a bunch of plain text, human readable files and generates a single unreadable, machine friendly file. That sounds like a compiler to me.

## Samples

You can see some samples [here](/samples/xunit). GitHub will display the sample [README.md](/samples/xunit/README.md) listing each template's shortcut and description, and the shortcut will be a link to the individual file. For example, [theory.md](/samples/xunit/theory.md) shows a template with a shortcut of `theory`, a description of "Create theory method" and the text of the template is shown in a code block. GitHub rather nicely displays the metadata as a simple table.

## File Format

The markdown files look like this:

    ---
    guid: 58defe14-2132-4e4e-8126-5fa7e9a8f472
    type: Live
    reformat: True
    shortenReferences: True
    categories: xunit
    scopes: InCSharpTypeMember(minimumLanguageVersion=2.0)
    parameterOrder: TheoryMethodName, DataAttribute, parameters
    DataAttribute-expression: completeSmart()
    ---

    # theory

    Create theory method

    ```
    [Xunit.Extensions.TheoryAttribute]
    [$DataAttribute$]
    public void $TheoryMethodName$($parameters$)
    {
        $END$
    }
    ```

Metadata about the template is stored in the YAML-like front matter at the top of the file, separated by two horizontal rules. The heading is used as the shortcut (in this example, the shortcut is 'theory'), and the first paragraph is the description. The first code block encountered is the text of the template. Any other content (subsequent paragraphs or code blocks, or other formatting such as lists) are simply ignored when generating the `.dotSettings` file.

The metadata is stored as a set of name/value pairs, with the name separated from the value with a colon `:`

| Name                   | Value                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
|------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| guid                   | Each template requires a GUID key. This must be specified so that if the `.dotSettings` file is generated multiple times, the template always has the same GUID. This allows a user to soft-delete or disable a template, and not have it reset each time the `.dotSettings` file is regenerated.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| type                   | What kind of template this is. Must be either `Live`, `Surround`, `File` or `Both` (for Live/Surround templates)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| reformat               | Whether the generated code should be reformatted after inserting. Defaults to `True`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| shortenReferences      | Whether fully qualified type references are shortened by inserting `using` statements. Defaults to `True` and is a best practice when creating templates. If you don't use fully qualified types, the user must have an appropriate `using` statement already in place, or the generated code is invalid.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| categories             | A comma separated list of categories, as displayed in the Template Explorer UI.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| scopes                 | A comma separated list of scopes, which state where the template is avaialble. See below for more details.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| parameterOrder         | A comma separated list of the parameters in the templates, and the order they are visited when the user hits tab.<br> If the user is not intended to edit a particular parameter (perhaps it refers to another parameter), the name of the parameter should be placed in brackets, e.g. given `methodName, (varName), args`. In this example, the order of parameters is first `methodName`, then when the user hits tab, `args`. The `varName` parameter is not edited. It's order is still important, however, as it will be evaluated after the `methodName` parameter is evaluated.<br> If there are multiple instances of a parameter in the text, only one is considered to be editable. This is indicated in the name of the parameter, as a 1-based index of the occurrences in the text. E.g. `methodName#2, (varName), args` will edit the second occurrence of the `methodName` parameter in the text.<br>  Note that there is *no* validation between the number and order of parameters, and the placeholders in the template text. |
| *parameter*-expression | The expression used for the macro associated with a parameter. The name should be the parameter name followed by `-expression`. So for a parameter called `varName`, the expression would be `varName-expression: completeSmart()`. See below for more details.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| image                  | Specify the icon to use. Unused in the UI, and requires custom code in a plugin to register a template image. A plugin must create a mapping between a template image name and an `IconId` instance. This can be done through a call to e.g. `new TemplateImage("MyTemplateImageName", MyIcons.MyTemplate.Id)`, which registers the map in a static dictionary. These calls can be created as part of initialisation of static fields in a `[ShellComponent]`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |

### Scopes

One of the less obvious, but more powerful aspects of templates is that you can specify where they are valid. Instead of a template being available at any location you can enter text, you can restrict it to a particular file type, or a location within that file type. For example, you can create a template that is only valid in C# files, or only valid in C# 3.0 and greater files, or even C# 3.0 and greater files, where a statement is expected.

These scopes are editable in the Template Explorer UI, and can be specified in the `scopes` metadata in the markdown file. The format for the scopes parameter is a **semi-colon** separated list of scope names. Each scope name can include a list of parameters, given as a **comma** separated list of name/value pairs, wrapped in parentheses. This sounds more complicated than it is:

    scopes: Scope1(arg1=val1, arg2=val2); Scope2(param1=val1); Scope3

E.g.:

    scopes: InCSharpFile(minimumLanguageVersion=2.0)

The values for the scopes are the values that are used in the `.dotSettings` file. The easiest way to find out what the scope values should be is to decompile an existing `.dotSettings` file to markdown files.

**TODO**: It might be nice to have a list of known scopes here.

### Parameter Expressions

Each parameter can include an expression that describes the macro, and its parameters, that executes when the parameter field is tabbed into. The name of the metadata item is based on the parameter name plus `-expression`. So for a parameter called `suffix`, the expression would be:

    suffix-expression: constant(".min")

The expression comes from the macro definitions in ReSharper. Each macro has a name, in this example `"constant"`, and all parameters are added as a comma separated list in parentheses. Again, the values here are best retrieved by decompiling an existing `.dotSettings` file.

**TODO**: It might be nice to have a list of known expressions here.

## Usage

The utility can be used to both compile and decompile a `.dotSettings` file. It is perhaps easier to set up a template by decompiling an existing `.dotSettings` file, especially when the templates contain scopes or parameter expressions.

### Downloading

To get a copy of the executable, either grab the source and build, or look for the latest release.

### Decompiling

To decompile, simply:

    rstc decompile -i foo.dotSettings -o outDir

* `-i` points to the `.dotSettings` file to decompile, and defaults to `templates.dotSettings` if not specified.
* `-o` specifies the directory to output the resulting markdown files. If not specified, the markdown files are generated in the current directory.

### Compiling

To compile a set of markdown files into a `.dotSettings` file ready to be distributed:

    rstc compile -i *.md -o templates.dotSettings

* `-i` is for the input files and must be provided.
* `-o` is the output file, and defaults to `templates.dotSettings` in the current directory if not specified.

## NuGet package

This utility is also available as a NuGet package, which will automatically compile a .dotSettings file from a set of markdown files as you build. It supports incremental build and will only update the output file if any of the input files have changed.

Firstly, add a reference to the `CitizenMatt.ReSharper.LiveTemplateCompiler` package. Then add something like the following to your .csproj file:

```xml
<ItemGroup>
  <LiveTemplate Include="templates\**\*.md">
    <OutputFile>templates\templates.dotSettings</OutputFile>
  </LiveTemplate>
</ItemGroup>
```

Multiple .dotSettings files can be created, by giving the item group different `OutputFile` metadata:

```xml
<ItemGroup>
  <LiveTemplate Include="templates\one\**\*.md">
    <OutputFile>templates\one.dotSettings</OutputFile>
  </LiveTemplate>
  <LiveTemplate Include="templates\two\**\*.md">
    <OutputFile>templates\two.dotSettings</OutputFile>
  </LiveTemplate>
</ItemGroup>
```

And the generated files can be included as embedded resources as normal:

```xml
<ItemGroup>
  <LiveTemplate Include="templates\**\*.md">
    <OutputFile>templates\templates.dotSettings</OutputFile>
    <ReadmeFile>templates\README.md</ReadmeFile>
  </LiveTemplate>
  <EmbeddedResource Include="templates\templates.dotSettings" />
</ItemGroup>
```

If the `ReadmeFile` metadata isn't defined, the readme file is generated simply as `README.md`, and will appear in the current working directory (most likely next to the .csproj file).

## Known Limitations

There are a couple of known limitations:

* The `.dotSettings` file will be slightly different each time it's generated. A template's scope (where it's available) is stored as an indexed value, with a GUID as a key. However, the GUID isn't actually used anywhere, so the value is arbitrary, and rather than storing it in the markdown file, a new GUID is generated each time the `.dotSettings` file is created. Apart from that, the `.dotSettings` file should be the same.
* File templates with multiple sections (multiple files) are currently not supported. I'd like to support it, but it's just not implemented yet. I suspect each section will be delimited with a horizontal rule, with the heading giving the filename, and the first code block being the snippet used.

