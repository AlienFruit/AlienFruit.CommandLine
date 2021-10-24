using System;

namespace AlienFruit.CommandLine.Attributes
{
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }

        public string Help { get; set; }
    }
}
