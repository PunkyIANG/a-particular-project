using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Kari.GeneratorCore.Workflow
{
    public readonly struct AttributeSymbolWrapper<T>
    {
        public readonly INamedTypeSymbol symbol;

        private static INamedTypeSymbol GetKnownSymbol(Compilation compilation, System.Type t)
        {
            return (INamedTypeSymbol) compilation.GetTypeByMetadataName(t.FullName);
        }

        public AttributeSymbolWrapper(Compilation compilation, Logger logger)
        {
            symbol = GetKnownSymbol(compilation, typeof(T));
            if (symbol is null) logger.LogError($"{typeof(T)} not found in the compilation");
        }
    }
    
    public static class AttributeExtensions
    {
        public static T MapToType<T>(this AttributeData attributeData) where T : Attribute
        {
            T attribute;
            bool a = typeof(T).Name == "ParserAttribute";

            if (attributeData.ConstructorArguments.Length > 0 && attributeData.AttributeConstructor != null)
            {
                attribute = (T) Activator.CreateInstance(typeof(T), attributeData.GetActualConstuctorParams().ToArray());
            }
            else
            {
                attribute = (T) Activator.CreateInstance(typeof(T));
            }
            foreach (var p in attributeData.NamedArguments)
            {
                var propertyInfo = typeof(T).GetProperty(p.Key);
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(attribute, p.Value.Value);
                    continue;
                }

                var fieldInfo = typeof(T).GetField(p.Key);
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(attribute, p.Value.Value);
                    continue;
                }

                throw new Exception($"No field or property {p}");
            }
            return attribute;
        }

        public static IEnumerable<object> GetActualConstuctorParams(this AttributeData attributeData)
        {
            foreach (var arg in attributeData.ConstructorArguments)
            {
                if (arg.Kind == TypedConstantKind.Array)
                {
                    // Assume they are strings, but the array that we get from this
                    // should actually be of type of the objects within it, be it strings or ints
                    // This is definitely possible with reflection, I just don't know how exactly. 
                    yield return arg.Values.Select(a => a.Value).OfType<string>().ToArray();
                }
                else
                {
                    yield return arg.Value;
                }
            }
        }
        public static bool TryGetAttributeData(this ISymbol symbol, ISymbol attributeType, out AttributeData attributeData)
        {
            var attrs = symbol.GetAttributes();
            for (int i = 0; i < attrs.Length; i++)
            {
                if (SymbolEqualityComparer.Default.Equals(attrs[i].AttributeClass, attributeType))
                {
                    attributeData = attrs[i];
                    return true;
                }
            }
            attributeData = default;
            return false;
        }

        public static bool TryGetAttribute<T>(this ISymbol symbol, AttributeSymbolWrapper<T> attributeSymbolWrapper, out T attribute) where T : System.Attribute
        {
            Logger.Debug.LogDebug(symbol.Name);
            if (TryGetAttributeData(symbol, attributeSymbolWrapper.symbol, out var attributeData))
            {
                try
                {
                    attribute = attributeData.MapToType<T>();
                }
                catch
                {
                    attribute = null;
                    Logger.Debug.LogError("Invalid?");
                }
                return true;
            }
            attribute = default;
            return false;
        }

        public static IEnumerable<T> GetAttributes<T>(this ISymbol symbol, AttributeSymbolWrapper<T> attributeSymbolWrapper) where T : System.Attribute
        {
            var attributes = symbol.GetAttributes();
            for (int i = 0; i < attributes.Length; i++)
            {
                if (SymbolEqualityComparer.Default.Equals(attributes[i].AttributeClass, attributeSymbolWrapper.symbol))
                {
                    yield return attributes[i].MapToType<T>();
                }
            }
        }

        public static bool HasAttribute(this ISymbol symbol, ISymbol attributeType)
        {
            foreach (var a in symbol.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType))
                {
                    return true;
                }
            }
            return false;
        }
    }
}