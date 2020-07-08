using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CitizenMatt.ReSharper.TemplateCompiler.Markdown;
using CommandLine;

namespace CitizenMatt.ReSharper.TemplateCompiler
{
    public static class Program
    {
        private static Regex InvalidFileCharsRegex = new Regex(
            $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", RegexOptions.Compiled);

        static int Main(string[] args)
        {
            // Case sensitive by default. Meh.
            var parser = new Parser(settings =>
            {
                settings.CaseSensitive = false;
                settings.HelpWriter = Console.Error;
            });
            var result = parser.ParseArguments<CompileOptions, DecompileOptions>(args) as Parsed<object>;
            if (result == null)
                return 1;

            switch (result.Value)
            {
                case CompileOptions compileOptions:
                    DoCompile(compileOptions);
                    break;

                case DecompileOptions decompileOptions:
                    DoDecompile(decompileOptions);
                    break;
            }

            return 0;
        }

        private static void DoCompile(CompileOptions compileOptions)
        {
            var dictionary = new Dictionary<string, object>();
            var serialisation = new SettingsSerialisation(dictionary);
            var store = new TemplateStore(serialisation);

            var inputFiles = GetInputFiles(compileOptions.InputFiles);

            var parser = new TemplateParser();
            foreach (var inputFile in inputFiles)
            {
                if (inputFile.EndsWith("readme.md", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("Ignoring readme.md...");
                    continue;
                }

                var markdown = File.ReadAllText(inputFile);
                var template = parser.Parse(markdown);
                template.InputFile = Path.Combine(Directory.GetCurrentDirectory(), inputFile);
                store.AddTemplate(template);
                // TODO: Concatenate markdown to a readme.md
            }

            var stream = File.Open(compileOptions.OutputFile, FileMode.Create, FileAccess.Write);
            using(var streamWriter = new StreamWriter(stream))
                SettingsSerialisation.SerialiseToXaml(streamWriter, serialisation);

            stream = File.Open(compileOptions.ReadMeFile, FileMode.Create, FileAccess.Write);
            using (var streamWriter = new StreamWriter(stream))
            {
                var readme = new ReadmeFormatter(streamWriter, Path.GetDirectoryName(Path.GetFullPath(compileOptions.ReadMeFile)));
                readme.FormatTemplates(store);
            }
        }

        private static IEnumerable<string> GetInputFiles(IEnumerable<string> inputFiles)
        {
            return inputFiles.SelectMany(f => f.Split(';')).SelectMany(f => Directory.GetFiles(".", f));
        }

        private static void DoDecompile(DecompileOptions decompileOptions)
        {
            IList<Template> templates;

            var stream = File.OpenRead(decompileOptions.InputFile);
            using (var streamReader = new StreamReader(stream))
            {
                var deserialiser = SettingsSerialisation.DeserialiseFromXaml(streamReader);
                templates = deserialiser.DeserialiseTemplates();
            }

            foreach (var template in templates)
            {
                var name = template.Shortcut ?? template.Description.Replace(' ', '_');
                var filename = InvalidFileCharsRegex.Replace(name + ".md", string.Empty);
                var file = File.Open(Path.Combine(decompileOptions.OutDir, filename), FileMode.Create, FileAccess.Write);
                using (var writer = new StreamWriter(file))
                {
                    var formatter = new TemplateFormatter(writer);
                    formatter.FormatTemplate(template);
                }
            }
        }
    }
}