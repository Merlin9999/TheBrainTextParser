using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Nito.AsyncEx;

namespace TheBrainTextParser
{
    public class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ProgramArguments>(args)
                .WithParsed(opts => AsyncContext.Run(() => RunOptionsAndReturnExitCodeAsync(opts)))
                .WithNotParsed((errs) => HandleParseError(errs));
        }

        public static async Task RunOptionsAndReturnExitCodeAsync(ProgramArguments opts)
        {
            string[] lines = File.ReadAllLines(opts.InputFile);
            Node rootNode = Node.Read(lines);
            IAeonEvent rootEvent = AeonEvent.Read(rootNode);
            EventValidationResults evr = rootEvent.Validate();
            var csv = AeonTimelineCsv.Create(rootEvent);
            csv.Write(opts.OutputFile);
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (Error error in errs)
                Console.WriteLine(error);
        }
    }

    public class ProgramArguments
    {
        [Option('i', "input", Required = true, HelpText = "Input files to be processed.")]
        public string InputFile { get; set; }
        [Option('o', "output", Required = true, HelpText = "Output file to be processed.")]
        public string OutputFile { get; set; }
    }
}
