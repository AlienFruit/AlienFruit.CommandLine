using AlienFruit.CommandLine.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlienFruit.CommandLine.Converters
{
    public class PrimitiveConverter<T> : IConverter<T>
    {
        public T Convert(IEnumerable<string> values)
        {
            if (values.Count() > 1)
                throw new ArgumentException($"Property type {typeof(T).Name} doesn't allow multiple values");

            return (T)System.Convert.ChangeType(values.First(), typeof(T));
        }

        public object ConvertObject(IEnumerable<string> values) => Convert(values);
    }
}
