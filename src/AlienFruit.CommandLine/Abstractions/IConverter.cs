using System.Collections.Generic;

namespace AlienFruit.CommandLine.Abstractions
{
    public interface IConverter<T> : IConverter
    {
        T Convert(IEnumerable<string> values);
    }

    public interface IConverter
    {
        object ConvertObject(IEnumerable<string> values);
    }
}
