using AlienFruit.CommandLine.Abstractions;
using AlienFruit.CommandLine.Attributes;
using AlienFruit.CommandLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AlienFruit.CommandLine
{
    public class CommandFactory
    {

        private readonly ConverterFactory converterFactory;

        public CommandFactory(ConverterFactory converterFactory)
        {
            this.converterFactory = converterFactory;
        }

        public Result TryCreateCommand(CommandData commandData, ILookup<string, string> options, out ICommand command)
        {
            command = Activator.CreateInstance(commandData.CommandType) as ICommand
                ?? throw new InvalidOperationException($"{commandData.CommandType.Name} does't implement ICommand interface");

            var result = CommandFactoryResult.Success();

            foreach (var property in commandData.CommandType.GetProperties())
            {
                var optionAttribute = property.GetCustomAttribute<OptionAttribute>();
                var values = options[optionAttribute.ShortName.ToString()];
                if(!values.Any())
                {
                    values = options[optionAttribute.LongName.ToString()];
                }

                if(values.Any())
                {
                    SetPropertyValue(command, property, values, optionAttribute);
                }
                else if(optionAttribute.Required)
                {
                    result = result.Merge(new CommandOptionFailure
                    {
                        OptionAttribute = optionAttribute,
                        Message = "Required option is missing."
                    });
                }
            }

            return result;

            void SetPropertyValue(ICommand command, PropertyInfo property, IEnumerable<string> values, OptionAttribute attribute)
            {
                if (TryGetPropertyValues(property, values, attribute, out var resultValue) is CommandFactoryFailure failure)
                {
                    result = result.Merge(failure);
                }
                else
                {
                    property.SetValue(command, resultValue);
                }
            }
        }

        private Result TryGetPropertyValues(PropertyInfo property, IEnumerable<string> values, OptionAttribute attribute, out object result)
        {
            try
            {
                var converter = this.converterFactory.GetConverter(property.PropertyType);
                result = converter.ConvertObject(values);
                return new CommandFactorySuccess();
            }
            catch(Exception ex)
            {
                result = null;
                return new CommandPropertyFailure
                {
                    Name = property.Name,
                    Message = ex.Message,
                    OptionAttribute = attribute
                };
            }
        }


        public abstract class CommandFactoryResult : Result
        {
            public abstract CommandFactoryResult Merge(CommandFactoryResult result);

            public static CommandFactoryResult Success() => new CommandFactorySuccess();
        }

        public class CommandFactorySuccess : CommandFactoryResult
        {
            public override bool IsSuccess => true;

            public override CommandFactoryResult Merge(CommandFactoryResult result) => result;
        }

        public abstract class CommandFactoryFailure : CommandFactoryResult
        {
            public string Message { get; set; }
            public CommandFactoryFailure InnerFailure { get; set; }

            public override bool IsSuccess => false;

            public override CommandFactoryResult Merge(CommandFactoryResult result)
            {
                if (result is CommandFactoryFailure failure)
                {
                    SetFailure(failure);
                }
                
                return this;
            }

            private void SetFailure(CommandFactoryFailure failure)
            {
                if (InnerFailure is null)
                    InnerFailure = failure;
                else
                    InnerFailure.Merge(failure);
            }
        }

        public class CommandOptionFailure : CommandFactoryFailure
        {
            public OptionAttribute OptionAttribute { get; set; }
        }

        public class CommandPropertyFailure : CommandFactoryFailure
        {
            public string Name { get; set; }
            public OptionAttribute OptionAttribute { get; set; }
        }

    }
}
