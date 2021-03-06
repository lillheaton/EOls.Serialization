﻿using JsonContractSimplifier.Services.Reflection;
using System;
using System.Linq;

namespace JsonContractSimplifier.Services.ConverterLocator
{
    public class ConverterLocatorService : IConverterLocatorService
    {
        protected readonly (IConverter Converter, Type Target)[] _converters;
        
        public ConverterLocatorService()
        {
            _converters = LoadConverters()
                .Select(x => 
                    (
                        Activator.CreateInstance(x) as IConverter,
                        ReflectionService.GetGenericTypeOfInterface(x)
                    ))
                .ToArray();
        }

        public virtual object Convert(object target, IConverter converter)
        {
            Type converterType = converter.GetType();

            if (!converterType.ImplementsGenericInterface(typeof(IObjectConverter<>)))
            {
                throw new ArgumentException($"Converter type {converterType.Name} does not implement IObjectConverter");
            }

            var method = converterType.GetMethod("Convert");
            return method.Invoke(converter, new[] { target });
        }

        public virtual Type[] LoadConverters()
        {
            return ReflectionService
                .GetAssemblyClassesInheritGenericInterface(
                    typeof(IObjectConverter<>), 
                    AppDomain.CurrentDomain.GetAssemblies())
                .ToArray();
        }

        public virtual bool TryFindConverterFor(Type type, out IConverter converter)
        {
            bool exist = _converters.Any(x => x.Target == type);

            if (!exist)
            {
                converter = null;
                return false;
            }

            converter = _converters.First(x => x.Target == type).Converter;
            return true;
        }
    }
}