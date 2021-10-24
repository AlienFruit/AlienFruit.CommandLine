using CommandLine;
using System;

namespace CommandLineParserTest
{
    class Program
    {
        public class Options
        {
            [Option('v', "verbose", Required = true, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }
        }

        public class Options2
        {
            [Option('v', "verbose", Required = true, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default
                .ParseArguments<Options, Options2>(args)
                .WithParsed<Options>(o =>
                {
                    if (o.Verbose)
                    {
                        Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
                        Console.WriteLine("Quick Start Example! App is in Verbose mode!");
                    }
                    else
                    {
                        Console.WriteLine($"Current Arguments: -v {o.Verbose}");
                        Console.WriteLine("Quick Start Example!");
                    }
                });
        }
    }
}
