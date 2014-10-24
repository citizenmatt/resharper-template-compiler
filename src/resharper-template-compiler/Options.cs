using System.Collections.Generic;
using CommandLine;

namespace CitizenMatt.ReSharper.TemplateCompiler
{
    [Verb("compile", HelpText = "Compile multiple markdown files to .dotSettings file")]
    public class CompileOptions
    {
        [Option('i', "input", Required = true, HelpText = "List of markdown files to include.", Min = 1, Max = int.MaxValue)]
        public IEnumerable<string> InputFiles { get; set; }

        [Option('o', "output", DefaultValue = "templates.dotSettings", HelpText = "Name of the file to output")]
        public string OutputFile { get; set; }

        [Option('r', "readme", DefaultValue = "README.md", HelpText = "Name of the readme file to generate")]
        public string ReadMeFile { get; set; }
    }

    [Verb("decompile", HelpText = "Decompile .dotSettings file to multiple markdown files")]
    public class DecompileOptions
    {
        [Option('i', "input", DefaultValue = "templates.dotSettings", HelpText = "Name of .dotSettings to decompile.")]
        public string InputFile { get; set; }

        [Option('o', "outDir", DefaultValue = "", HelpText = "Directory to output decompiled markdown files.")]
        public string OutDir { get; set; }
    }
}