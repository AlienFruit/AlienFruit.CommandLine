using AlienFruit.CommandLine.Abstractions;
using AlienFruit.CommandLine.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AlienFruit.CommandLine
{
    public class ConverterFactory
    {
        private readonly List<ConverterRule> propertyValuesFactoryMap = new()
        {
            //List
            new ConverterRule
            {
                //Predicate = x => x.GetTypeInfo().IsGenericType && (
                //    x.GetGenericTypeDefinition() == typeof(List<>) ||
                //    x.GetGenericTypeDefinition() == typeof(IList<>) ||
                //    x.GetGenericTypeDefinition() == typeof(ICollection<>)),
                //Factory = (type, args) => Activator.CreateInstance(typeof(PrimitiveConverter<>).MakeGenericType(type.GetTypeInfo().GetGenericArguments()), args),

                Predicate = x => x.IsPrimitive || x == typeof(decimal),
                Factory = (type, args) => Activator.CreateInstance(typeof(PrimitiveConverter<>).MakeGenericType(type), args)

                //RegisterFormatter(typeof(PrimitiveFormatter<>),
                //x => x.IsPrimitive || x == typeof(decimal),
                //(f, v, args) => Activator.CreateInstance(f.MakeGenericType(v), args) as IFormatter);
            }
        };

        private class ConverterRule
        {
            public Func<Type, bool> Predicate { get; set; }
            public Func<Type, object[], object> Factory { get; set; }
        }

        public IConverter<T> GetConverter<T>()
        {
            var type = typeof(T);
            var result = this.propertyValuesFactoryMap
                .Where(x => x.Predicate(type))
                .ToArray();

            if (result.Any() == false)
                throw new Exception();

            if (result.Length > 1)
                throw new Exception();

            var converter = result.First().Factory.Invoke(type, GetConstructorArguments(type)) as IConverter<T>;
            return converter;
        }

        public IConverter GetConverter(Type objectType)
        {
            var result = this.propertyValuesFactoryMap
                .Where(x => x.Predicate(objectType))
                .ToArray();

            if (result.Any() == false)
                throw new InvalidOperationException($"There are no converter rules for type {objectType.Name}");

            if (result.Length > 1)
                throw new InvalidOperationException($"Should be only single converter rule for type {objectType.Name}. Current rules number:{result.Length}");

            var converter = result.First().Factory.Invoke(objectType, GetConstructorArguments(objectType)) as IConverter;
            return converter;
        }

        private object[] GetConstructorArguments(Type type)
        {
            var ctors = type.GetConstructors();
            if (!ctors.Any())
                return Enumerable.Empty<object>().ToArray();

            if (ctors.Length > 1)
                throw new InvalidOperationException($"Convertor {type.Name} should have only one constructor");

            return ctors[0].GetParameters()
                .Select(x => ResolveConverterDependency(x.ParameterType))
                .ToArray();
        }

        private object ResolveConverterDependency(Type constructorArgumentType)
        {
            if (constructorArgumentType == typeof(ConverterFactory))
                return this;

            throw new ArgumentException($"Cannot resolve unregistered type: {constructorArgumentType.Name}");
        }
    }
}
