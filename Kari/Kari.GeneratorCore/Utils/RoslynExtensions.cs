﻿
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kari.GeneratorCore.CodeAnalysis
{
    // Utility and Extension methods for Roslyn
    internal static class RoslynExtensions
    {
        public static IEnumerable<INamedTypeSymbol> GetNamedTypeSymbols(this Compilation compilation)
        {
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semModel = compilation.GetSemanticModel(syntaxTree);

                foreach (var item in syntaxTree.GetRoot()
                    .DescendantNodes()
                    .Select(x => semModel.GetDeclaredSymbol(x))
                    .Where(x => x != null))
                {
                    if (item is INamedTypeSymbol namedType)
                    {
                        yield return namedType;
                    }
                }
            }
        }

        public static AttributeData FindAttribute(this IEnumerable<AttributeData> attributeDataList, string typeName)
        {
            return attributeDataList
                .Where(x => x.AttributeClass.ToDisplayString() == typeName)
                .FirstOrDefault();
        }

        public static AttributeData FindAttributeShortName(
            this IEnumerable<AttributeData> attributeDataList,
            string typeName)
        {
            return attributeDataList
                .Where(x => x.AttributeClass.Name == typeName)
                .FirstOrDefault();
        }

        public static AttributeData FindAttributeIncludeBasePropertyShortName(
            this IPropertySymbol property,
            string typeName)
        {
            do
            {
                var data = FindAttributeShortName(property.GetAttributes(), typeName);
                if (data != null)
                {
                    return data;
                }

                property = property.OverriddenProperty;
            }
            while (property != null);

            return null;
        }

        public static AttributeSyntax FindAttribute(
            this BaseTypeDeclarationSyntax typeDeclaration,
            SemanticModel model,
            string typeName)
        {
            return typeDeclaration.AttributeLists
                .SelectMany(x => x.Attributes)
                .Where(x => model.GetTypeInfo(x).Type?.ToDisplayString() == typeName)
                .FirstOrDefault();
        }

        public static INamedTypeSymbol FindBaseTargetType(this ITypeSymbol symbol, string typeName)
        {
            return symbol.BaseType.GetReversedTypeHierarchy()
                .Where(x => x.OriginalDefinition?.ToDisplayString() == typeName)
                .FirstOrDefault();
        }

        public static bool IsNullable(this INamedTypeSymbol symbol)
        {
            if (symbol.IsGenericType)
            {
                if (symbol.ConstructUnboundGenericType().ToDisplayString() == "T?")
                {
                    return true;
                }
            }

            return false;
        }

        public static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol symbol)
        {
            var t = symbol;
            while (t != null)
            {
                foreach (var item in t.GetMembers())
                {
                    yield return item;
                }

                t = t.BaseType;
            }
        }

        public static IEnumerable<ISymbol> GetAllInterfaceMembers(this ITypeSymbol symbol)
        {
            return symbol.GetMembers()
                .Concat(symbol.AllInterfaces.SelectMany(x => x.GetMembers()));
        }

        public static bool ApproximatelyEqual(this ITypeSymbol left, ITypeSymbol right)
        {
            if (left is IErrorTypeSymbol || right is IErrorTypeSymbol)
            {
                return left.ToDisplayString() == right.ToDisplayString();
            }
            else
            {
                return SymbolEqualityComparer.Default.Equals(left, right);
            }
        }
    }
}
