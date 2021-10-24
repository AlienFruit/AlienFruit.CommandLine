using AlienFruit.CommandLine.Abstractions;
using System;
using System.Collections.Generic;

namespace AlienFruit.CommandLine.Models
{
    public class CommandData
    {
        public string Name { get; set; }
        public string Help { get; set; }
        public Type CommandType { get; set; }
        public IDictionary<string, string> Options { get; set; }
        public Action<ICommand> Handler { get; set; }
    }
}
