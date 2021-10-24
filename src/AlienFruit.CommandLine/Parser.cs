using AlienFruit.CommandLine.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace AlienFruit.CommandLine
{
    public class Parser
    {
        public Result TryParse(string[] args, out ILookup<string, string> result)
        {
            result = Enumerable.Empty<(string, string)>()
                .ToLookup(x => x.Item1, x => x.Item1);
            var argumentValues = new List<(string name, List<string> values)>();

            foreach (var argument in args.Skip(1))
            {
                if (argument.StartsWith("--"))
                {
                    argumentValues.Add((argument.Trim('-'), new List<string>()));
                }
                else if (argument.StartsWith('-'))
                {
                    var shortName = argument.TrimStart('-');
                    if (shortName.Length > 1)
                    {
                        return new ParseFailure
                        {
                            Argument = argument,
                            Message = "Short name cannot contains more then one char"
                        };
                    }

                    argumentValues.Add((shortName, new List<string>()));
                }
                else if (argumentValues.Any())
                {
                    argumentValues.Last().values.Add(argument);
                }
                else return new ParseFailure
                {
                    Argument = argument,
                    Message = "There is no option for value"
                };
            }

            result = argumentValues.SelectMany(x => x.values, (x, y) => (x.name, value: y))
                .ToLookup(x => x.name, x => x.value);

            return new ParseSuccess();
        }
    }

    public class ParseFailure : Result
    {
        public string Argument { get; set; }
        public string Message { get; set; }

        public override bool IsSuccess => false;
    }

    public class ParseSuccess : Result
    {
        public override bool IsSuccess => true;
    }
}
