using System.Collections.Generic;
using System.Text;
using Kari.GeneratorCore.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Kari.GeneratorCore
{
    public partial class CommandsTemplate
    {
        private List<CommandMethodInfo> _infos;
        private RelevantSymbols _symbols;
        private INamespaceSymbol _rootNamespace;
        private Dictionary<ITypeSymbol, string> _builtinConverters;
        private Dictionary<ITypeSymbol, ParserInfo> _customConverters;

        public CommandsTemplate(Environment environment)
        {
            _rootNamespace = environment.RootNamespace;
            _symbols = environment.Symbols;
            _builtinConverters = new Dictionary<ITypeSymbol, string>
            {
                { _symbols.Bool, "bool.Parse" },
                { _symbols.Int, "int.Parse" },
                { _symbols.String, "" }
            };
            _customConverters = new Dictionary<ITypeSymbol, ParserInfo>();
            _infos = new List<CommandMethodInfo>();
        }

        public void Collect()
        {
            foreach (var type in _rootNamespace.GetNotNestedTypes())
            foreach (var method in type.GetMethods())
            {
                if (!method.IsStatic) continue;

                if (method.TryGetAttribute(_symbols.ParserAttribute, out var parserAttr))
                {
                    if (_customConverters.TryGetValue(method.ReturnType, out var converter))
                    {
                        while (converter.Next != null)
                        {
                            converter = converter.Next;
                        }
                        converter.Next = new ParserInfo(method, parserAttr);
                    }
                }

                if (method.TryGetAttribute(_symbols.CommandAttribute, out var commandAttribute))
                {
                    _infos.Add(new CommandMethodInfo(method, commandAttribute));
                }
            }
        }

        private string GetConverter(IArgumentInfo argument)
        {
            var customParser = argument.GetAttribute().Parser;

            if (!(customParser is null))
            {
                if (_customConverters.TryGetValue(argument.Symbol.Type, out var converter))
                {
                    while (converter.Attribute.Name != customParser)
                    {
                        if (converter.Next is null)
                        {
                            throw new System.Exception($"No such converter {converter.Attribute.Name} for type {argument.Symbol.Type}");
                        }
                        converter = converter.Next;
                    }
                    return converter.Symbol.GetFullyQualifiedName();
                }
            }
            {
                if (_builtinConverters.TryGetValue(argument.Symbol.Type, out var result))
                {
                    return result;
                }

                if (_customConverters.TryGetValue(argument.Symbol.Type, out var converter))
                {
                    return converter.Symbol.GetFullyQualifiedName();
                }
            }

            throw new System.Exception($"Found no converters for type {argument.Symbol.Type}");
        }

        private string TransformSingle(CommandMethodInfo info, string initialIndentation = "")
        {
            var classBuilder = new CodeBuilder(indentation: "    ", initialIndentation);
            classBuilder.AppendLine($"public class {info.Name}Command : ICommand");
            classBuilder.StartBlock();

            var executeBuilder = classBuilder.NewWithPreservedIndentation();
            executeBuilder.AppendLine("public string Execute(CommandContext context)");
            executeBuilder.StartBlock();

            List<OptionInfo> options = new List<OptionInfo>();
            List<ArgumentInfo> positionalArguments = new List<ArgumentInfo>();
            List<ArgumentInfo> optionLikeArguments = new List<ArgumentInfo>();

            for (int i = 0; i < info.Symbol.Parameters.Length; i++)
            {
                var parameter = info.Symbol.Parameters[i];
                if (parameter.TryGetAttribute(_symbols.ArgumentAttribute, out var argumentAttribute))
                {
                    var argInfo = new ArgumentInfo(parameter, argumentAttribute);
                    if (!argumentAttribute.IsOptionLike)
                    {
                        positionalArguments.Add(argInfo);
                    }
                    else
                    {
                        optionLikeArguments.Add(argInfo);
                    }
                    continue;
                }
                if (parameter.TryGetAttribute(_symbols.OptionAttribute, out var optionAttribute))
                {
                    options.Add(new OptionInfo(parameter, optionAttribute));
                    continue;
                }
                positionalArguments.Add(new ArgumentInfo(parameter, new ArgumentAttribute("")));
            }

            var usageBuilder = new StringBuilder();
            var argsBuilder = new EvenTableBuilder("Argument/Option", "Type", "Description");
            
            usageBuilder.Append($"Usage: {info.Name} ");
            for (int i = 0; i < positionalArguments.Count; i++)
            {
                var arg = positionalArguments[i];
                usageBuilder.Append($"{arg.Name} ");
                
                argsBuilder.Append(column: 0, arg.Symbol.Name);
                argsBuilder.Append(column: 1, arg.Symbol.Type.Name);
                argsBuilder.Append(column: 2, arg.Attribute.Help);
            }

            for (int i = 0; i < optionLikeArguments.Count; i++)
            {
                var arg = optionLikeArguments[i];
                usageBuilder.Append($"{arg.Attribute.Name}|-{arg.Attribute.Name}=value ");

                argsBuilder.Append(column: 0, $"{arg.Attribute.Name}|-{arg.Attribute.Name}");
                argsBuilder.Append(column: 1, arg.Symbol.Type.Name);
                argsBuilder.Append(column: 2, arg.Attribute.Help);
            }
            
            for (int i = 0; i < options.Count; i++)
            {
                var op = options[i];
                usageBuilder.Append($"[-{op.Name}=value] ");

                argsBuilder.Append(column: 0, "-" + op.Name);
                argsBuilder.Append(column: 2, op.Attribute.Help);
                argsBuilder.Append(column: 1, 
                    op.Attribute.IsFlag 
                        ? $"Flag, ={op.Symbol.GetDefaultValueText()}"
                        : $"{op.Symbol.Type.Name}");
            }

            var helpMessageBuilder = new StringBuilder();
            helpMessageBuilder.AppendLine(usageBuilder.ToString());
            helpMessageBuilder.AppendLine();
            helpMessageBuilder.AppendLine(info.CommandAttribute.Help);
            helpMessageBuilder.AppendLine();
            helpMessageBuilder.AppendLine(argsBuilder.ToString());

            // If the function takes in any positional arguments, an empty input is considered help
            if (positionalArguments.Count > 0)
            {
                executeBuilder.AppendLine("if (context.Parser.IsEmpty) return HelpMessage;");
            }
            // Check for the -help flag as the first flag
            executeBuilder.AppendLine("if (ExecuteHelper.IsHelp(context.Parser)) return HelpMessage;");

            executeBuilder.AppendLine("// Take in all the positional arguments");
            for (int i = 0; i < positionalArguments.Count; i++)
            {
                var converterText = GetConverter(positionalArguments[i]);
                executeBuilder.AppendLine($"string __posInput{i} = context.Parser.GetString();");
                executeBuilder.AppendLine($"if (__posInput{i} == null)");
                executeBuilder.StartBlock();
                executeBuilder.AppendLine($"throw new System.Exception(\"Expected a positional argument '{positionalArguments[i].Symbol.Name}'\");");
                executeBuilder.EndBlock();
                executeBuilder.AppendLine($"var __posArg{i} = {converterText}(__posInput{i});");
                executeBuilder.AppendLine("context.Parser.SkipWhitespace();");
            }

            if (optionLikeArguments.Count > 0)
            {
                executeBuilder.AppendLine("// Take in all the option-like positional arguments");

                for (int i = 0; i < optionLikeArguments.Count; i++)
                {
                    executeBuilder.AppendLine($"bool __isPresentOptionLikeArg{i} = false;");
                    var typeText = optionLikeArguments[i].Symbol.Type.GetFullyQualifiedName();
                    var defaultValueText = optionLikeArguments[i].Symbol.GetDefaultValueText();
                    executeBuilder.AppendLine($"{typeText} __optionLikeArg{i} = {defaultValueText};");
                }

                executeBuilder.StartBlock();
                executeBuilder.AppendLine("string __input;");
                for (int i = 0; i < optionLikeArguments.Count; i++)
                {
                    var converterText = GetConverter(optionLikeArguments[i]);
                    executeBuilder.AppendLine($"__input = context.Parser.GetString();");
                    executeBuilder.AppendLine($"if (__input is null)");
                    executeBuilder.StartBlock();
                    executeBuilder.AppendLine($"goto __afterOptionLike;");
                    executeBuilder.EndBlock();
                    executeBuilder.AppendLine("context.Parser.SkipWhitespace();");
                    executeBuilder.AppendLine($"__isPresentOptionLikeArg{i} = true;");
                    executeBuilder.AppendLine($"__optionLikeArg{i} = {converterText}(__input);");
                }
                executeBuilder.AppendLine("__afterOptionLike: ;");
                executeBuilder.EndBlock();
            }

            if (options.Count > 0 || optionLikeArguments.Count > 0)
            {
                for (int i = 0; i < options.Count; i++)
                {
                    var typeText = options[i].Symbol.Type.GetFullyQualifiedName();
                    var defaultValueText = options[i].Symbol.GetDefaultValueText();
                    executeBuilder.AppendLine($"{typeText} __option{i} = {defaultValueText};");
                }

                executeBuilder.AppendLine("while (context.Parser.TryGetOption(out Option __option))");
                executeBuilder.StartBlock();
                executeBuilder.AppendLine("context.Parser.SkipWhitespace();");
                executeBuilder.AppendLine("switch (__option.Name)");
                executeBuilder.StartBlock();

                for (int i = 0; i < options.Count; i++)
                {
                    executeBuilder.AppendLine($"case \"{options[i].Name}\":");
                    executeBuilder.StartBlock();

                    if (options[i].Attribute.IsFlag)
                    {
                        executeBuilder.AppendLine($"__option{i} = __option.GetFlagValue();");
                    }
                    else
                    {
                        var converterText = GetConverter(options[i]);
                        executeBuilder.AppendLine($"__option{i} = {converterText}(__option.Value);");
                    }
                    executeBuilder.AppendLine("break;");
                    executeBuilder.EndBlock();
                }

                for (int i = 0; i < optionLikeArguments.Count; i++)
                {
                    executeBuilder.AppendLine($"case \"{optionLikeArguments[i].Attribute.Name}\":");
                    executeBuilder.StartBlock();

                    var converterText = GetConverter(optionLikeArguments[i]);
                    executeBuilder.AppendLine($"__optionLikeArg{i} = {converterText}(__option.Value);");
                    executeBuilder.AppendLine($"__isPresentOptionLikeArg{i} = true;");
                    executeBuilder.AppendLine("break;");
                    executeBuilder.EndBlock();
                }
                executeBuilder.AppendLine("default: throw new System.Exception($\"Unknown option: '{__option.Name}'\");");
                executeBuilder.EndBlock();
                executeBuilder.EndBlock();
            }

            // TODO: Add requirability to options
            if (optionLikeArguments.Count > 0)
            {
                executeBuilder.AppendLine("// Make sure all required parameters have been given");
                for (int i = 0; i < optionLikeArguments.Count; i++)
                {
                    executeBuilder.AppendLine($"if (!__isPresentOptionLikeArg{i})");
                    executeBuilder.StartBlock();
                    executeBuilder.AppendLine($"throw new System.Exception(\"Option-like argument '{optionLikeArguments[i].Attribute.Name}' not given\");");
                    executeBuilder.EndBlock();
                }
            }

            executeBuilder.AppendLine("// Call the function with correct arguments");
            executeBuilder.Indent();
            if (info.Symbol.ReturnsVoid)
            {
                executeBuilder.Append(info.Symbol.GetFullyQualifiedName() + "(");
            }
            else
            {
                executeBuilder.Append($"return {info.Symbol.GetFullyQualifiedName()}(");
            }

            var parameters = new ListBuilder(", ");

            for (int i = 0; i < positionalArguments.Count; i++)
            {
                parameters.Append($"{positionalArguments[i].Symbol.Name} : __posArg{i}");
            }

            for (int i = 0; i < optionLikeArguments.Count; i++)
            {
                parameters.Append($"{optionLikeArguments[i].Symbol.Name} : __optionLikeArg{i}");
            }

            for (int i = 0; i < options.Count; i++)
            {
                parameters.Append($"{options[i].Symbol.Name} : __option{i}");
            }

            executeBuilder.Append(parameters.ToString());
            executeBuilder.Append(")");

            if (!info.Symbol.ReturnsVoid)
            {
                executeBuilder.Append(".ToString()");
            }
            executeBuilder.Append(";");
            executeBuilder.AppendLine("");

            executeBuilder.EndBlock();
            
            classBuilder.Indent();
            classBuilder.Append("public string HelpMessage => @\"");
            classBuilder.Append(helpMessageBuilder.ToString().Replace("\"", "\"\""));
            classBuilder.Append("\";");
            classBuilder.AppendLine("");
            classBuilder.Append(executeBuilder.ToString());
            classBuilder.AppendLine("");
            classBuilder.EndBlock();

            return classBuilder.ToString();
        }
    }
}
