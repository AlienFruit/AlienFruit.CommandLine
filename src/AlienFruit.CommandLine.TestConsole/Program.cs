using AlienFruit.CommandLine.Abstractions;
using AlienFruit.CommandLine.Attributes;
using System;

namespace AlienFruit.CommandLine.TestConsole
{
    [Command("test1", Help = "this is help")]
    public class TestCommand : ICommand
    {
        [Option('p', "prop", Help = "help")]
        public int IntProp { get; set; }

        [Option('s', "second", Help = "help", Required = true)]
        public int SecondProperty { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var processor = new Processor()
                .RegisterCommand<TestCommand>(x =>
                {
                    Console.WriteLine($"Name:{x.IntProp}");
                });

            processor.Parse(args);
        }
    }
}
