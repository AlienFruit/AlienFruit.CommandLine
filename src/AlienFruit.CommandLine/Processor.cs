using AlienFruit.CommandLine.Abstractions;
using AlienFruit.CommandLine.Attributes;
using AlienFruit.CommandLine.Models;
using AlienFruit.FluentConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static AlienFruit.CommandLine.CommandFactory;

namespace AlienFruit.CommandLine
{
    public class Processor
    {
        private Func<Type, Action<ICommand>> handlersFactory;
        private readonly Dictionary<string, CommandData> commands;
        private readonly Parser parser;
        private readonly CommandFactory commandFactory;

        public Processor()
        {
            this.commands = new Dictionary<string, CommandData>();
            this.parser = new Parser();
            this.commandFactory = new CommandFactory(new ConverterFactory());
        }

        public Processor UseHandlersFactory(Func<Type, Action<ICommand>> handlersFactory)
        {
            this.handlersFactory = handlersFactory;
            return this;
        }

        public Processor RegisterCommand<T>() where T : ICommand
        {
            var cmdType = typeof(T);
            var cmdAttribute = cmdType.GetCustomAttribute<CommandAttribute>();
            if (cmdAttribute is null)
                throw new ArgumentException($"Object '{cmdType.Name}' has no {nameof(CommandAttribute)}");

            var properties = cmdType.GetProperties(BindingFlags.Public);

            this.commands.Add(cmdAttribute.Name, new CommandData
            {
                Handler = this.handlersFactory?.Invoke(cmdType) ?? throw new InvalidOperationException("Use 'UseHandlersFactory' before call this method"),
                CommandType = cmdType,
                Name = cmdAttribute.Name
            });

            return this;
        }

        public Processor RegisterCommand<T>(Action<T> handler) where T : ICommand
        {
            var cmdType = typeof(T);
            var cmdAttribute = cmdType.GetCustomAttribute<CommandAttribute>();
            if (cmdAttribute is null)
                throw new ArgumentException($"Object '{cmdType.Name}' has no {nameof(CommandAttribute)}");

            this.commands.Add(cmdAttribute.Name, new CommandData
            {
                Handler = x => handler((T)x),
                CommandType = cmdType,
                Name = cmdAttribute.Name,
                Help = cmdAttribute.Help
            });

            return this;
        }

        public void Parse(string[] args)
        {
            if (args.Any() == false)
            {
                return;
            }

            if (this.commands.TryGetValue(args[0], out var commandData) == false)
            {
                PrintErrors();
                PrintRows(new[] { (args[0], ConsoleColor.Yellow), ("Verb is not recognized.", ConsoleColor.Red) });
                return;
            }

            if(this.parser.TryParse(args, out var options) is ParseFailure failure)
            {
                PrintErrors();
                WriteFailure(failure);
                return;
            }

            if(this.commandFactory.TryCreateCommand(commandData, options, out var command) is CommandFactoryFailure commandFactoryFailure)
            {
                PrintErrors();
                var failureRows = new List<(string text, ConsoleColor color)[]>();
                WriteFailure(commandFactoryFailure, x => failureRows.Add(x));
                PrintRows(failureRows.ToArray());
                return;
            }

            commandData.Handler(command);

            static void PrintErrors()
                => FConsole.Color(ConsoleColor.Red)
                    .WriteLine("ERROR(S):")
                    .ResetColors();
        }

        private static void WriteFailure(ParseFailure failure)
        {
            PrintRows(new[] { (failure.Argument, ConsoleColor.Yellow), (failure.Message, ConsoleColor.Red) });
        }

        private void WriteFailure(CommandFactoryFailure commandFactoryFailure, Action<(string text, ConsoleColor color)[]> newFailureRowCallback)
        {
            switch (commandFactoryFailure)
            {
                case CommandOptionFailure optionFailure:
                    newFailureRowCallback(new[] 
                    { 
                        (GetOptionAttributeString(optionFailure.OptionAttribute), ConsoleColor.Yellow), 
                        (optionFailure.Message, ConsoleColor.Red) 
                    });
                    break;
                case CommandPropertyFailure propertyFailure:
                    newFailureRowCallback(new[]
                    {
                        (GetOptionAttributeString(propertyFailure.OptionAttribute), ConsoleColor.Yellow),
                        (propertyFailure.Message, ConsoleColor.Red)
                    });
                    break;
                default:
                    FConsole
                        .Color(ConsoleColor.Red)
                        .WriteLine(commandFactoryFailure.Message)
                        .ResetColors();
                    break;
            }

            if (commandFactoryFailure.InnerFailure is not null)
                WriteFailure(commandFactoryFailure.InnerFailure, newFailureRowCallback);

            static string GetOptionAttributeString(OptionAttribute attribute)
                => $"-{attribute.ShortName}(--{attribute.LongName})";
        }

        private static void PrintRows(params (string text, ConsoleColor color)[][] rows)
        {
            var columnsWidth = rows
                .SelectMany(x => x.Select((y, i) => (i, y.text.Length)))
                .GroupBy(x => x.i, x => x.Length)
                .Select(x => x.Max())
                .ToArray();

            foreach (var row in rows)
            {
                for (int column = 0; column < row.Length; column++)
                {
                    FConsole
                        .Color(row[column].color)
                        .Write(AlignLeft(row[column].text, columnsWidth[column]))
                        .ResetColors();
                }
                FConsole.NextLine();
            }
        }

        private static string AlignLeft(string text, int width)
        {
            int rightPadding = 5;
            int leftPadding = 4;

            var totalWidth = width + rightPadding + leftPadding;

            text = text.Length > totalWidth ? text.Substring(0, totalWidth - 4) + "... " : text;
           
            return string.IsNullOrEmpty(text)
                ? new string(' ', totalWidth)
                : text.PadRight(width + rightPadding).PadLeft(totalWidth);
        }
    }
}
