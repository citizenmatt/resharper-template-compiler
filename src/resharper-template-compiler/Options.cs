using System.Collections.Generic;
using CommandLine;

namespace CitizenMatt.ReSharper.TemplateCompiler
{
    public enum NewLine
    {
        OS,
        Unix,
        Windows
    }

    [Verb("compile", HelpText = "Compile multiple markdown files to .dotSettings file")]
    public class CompileOptions
    {
        [Option('i', "input", Required = true, HelpText = "List of markdown files to include.", Min = 1, Max = int.MaxValue)]
        public IEnumerable<string> InputFiles { get; set; }

        [Option('o', "output", Default = "templates.dotSettings", HelpText = "Name of the file to output.")]
        public string OutputFile { get; set; }

        [Option('r', "readme", Default = "README.md", HelpText = "Name of the readme file to generate.")]
        public string ReadMeFile { get; set; }

        [Option('n', "newline", Default = NewLine.OS, HelpText = "New line character(s) to use while writing files.")]
        public NewLine NewLine { get; set; }
    }

    [Verb("decompile", HelpText = "Decompile .dotSettings file to multiple markdown files")]
    public class DecompileOptions
    {
        [Option('i', "input", Default = "templates.dotSettings", HelpText = "Name of .dotSettings to decompile.")]
        public string InputFile { get; set; }

        [Option('o', "outDir", Default = "", HelpText = "Directory to output decompiled markdown files.")]
        public string OutDir { get; set; }

        [Option('n', "newline", Default = NewLine.OS, HelpText = "New line character(s) to use while writing files.")]
        public NewLine NewLine { get; set; }
    }
}
