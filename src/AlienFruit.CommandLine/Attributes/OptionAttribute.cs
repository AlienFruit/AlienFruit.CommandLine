using System;

namespace AlienFruit.CommandLine.Attributes
{
    public class OptionAttribute : Attribute
    {
        public OptionAttribute(char shortName, string longName)
        {
            this.ShortName = shortName;
            this.LongName = longName;
        }

        public char ShortName { get; set; }

        public string LongName { get; set; }

        public bool Required { get; set; }

        public string Help { get; set; }
    }
}
