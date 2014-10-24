using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CitizenMatt.ReSharper.TemplateCompiler.Markdown;
using CommandLine;

namespace CitizenMatt.ReSharper.TemplateCompiler
{
    class Program
    {
        private static Regex InvalidFileCharsRegex = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))), RegexOptions.Compiled);

        static int Main(string[] args)
        {
            // Case sensitive by default. Meh.
            var parser = new Parser(settings =>
            {
                settings.CaseSensitive = false;
                settings.HelpWriter = Console.Error;
            });
            var result = parser.ParseArguments<CompileOptions, DecompileOptions>(args);
            if (result.Errors.Any())
                return 1;

            var compileOptions = result.Value as CompileOptions;
            if (compileOptions != null)
                DoCompile(compileOptions);

            var decompileOptions = result.Value as DecompileOptions;
            if (decompileOptions != null)
                DoDecompile(decompileOptions);

            return 0;
        }

        private static void DoCompile(CompileOptions compileOptions)
        {
            var dictionary = new Dictionary<string, object>();
            var serialisation = new SettingsSerialisation(dictionary);
            var store = new TemplateStore(serialisation);

            var inputFiles = ExpandWildcards(compileOptions.InputFiles);

            var parser = new TemplateParser();
            foreach (var inputFile in inputFiles)
            {
                if (inputFile.EndsWith("readme.md", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("Ignoring readme.md...");
                    continue;
                }

                var markdown = File.ReadAllText(inputFile);
                store.AddTemplate(parser.Parse(markdown));
                // TODO: Concatenate markdown to a readme.md
            }

            var stream = File.Open(compileOptions.OutputFile, FileMode.Create, FileAccess.Write);
            using(var streamWriter = new StreamWriter(stream))
                SettingsSerialisation.SerialiseToXaml(streamWriter, serialisation);

            stream = File.Open(compileOptions.ReadMeFile, FileMode.Create, FileAccess.Write);
            using (var streamWriter = new StreamWriter(stream))
            {
                var readme = new ReadmeFormatter(streamWriter);
                readme.FormatTemplates(store);
            }
        }

        private static IEnumerable<string> ExpandWildcards(IEnumerable<string> inputFiles)
        {
            return inputFiles.SelectMany(f => Directory.GetFiles(".", f));
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
                var filename = InvalidFileCharsRegex.Replace(template.Shortcut + ".md", string.Empty);
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