﻿// Autogenerated from RelevantSymbols.tt

namespace Kari.GeneratorCore.CodeAnalysis
{
	using System;
	using Microsoft.CodeAnalysis;
	using Kari;

	public class RelevantSymbols
	{
		public AttributeSymbolWrapper<KariTestAttribute> KariTestAttribute;
		public AttributeSymbolWrapper<CommandAttribute> CommandAttribute;
		public AttributeSymbolWrapper<OptionAttribute> OptionAttribute;
		public AttributeSymbolWrapper<ArgumentAttribute> ArgumentAttribute;
		public AttributeSymbolWrapper<ParserAttribute> ParserAttribute;

		public ITypeSymbol Short;
		public ITypeSymbol Int;
		public ITypeSymbol Long;
		public ITypeSymbol Ushort;
		public ITypeSymbol Uint;
		public ITypeSymbol Ulong;
		public ITypeSymbol Float;
		public ITypeSymbol Double;
		public ITypeSymbol Bool;
		public ITypeSymbol Byte;
		public ITypeSymbol Sbyte;
		public ITypeSymbol Decimal;
		public ITypeSymbol Char;
		public ITypeSymbol String;
		public ITypeSymbol Object;
		public ITypeSymbol Void;
		
		public RelevantSymbols(Compilation compilation, Action<string> logger)
		{
			KariTestAttribute	.Init(compilation);
			CommandAttribute	.Init(compilation);
			OptionAttribute		.Init(compilation);
			ArgumentAttribute	.Init(compilation);
			ParserAttribute		.Init(compilation);

			Short 	= compilation.GetSpecialType(SpecialType.System_Int16);
			Int 	= compilation.GetSpecialType(SpecialType.System_Int32);
			Long 	= compilation.GetSpecialType(SpecialType.System_Int64);
			Ushort 	= compilation.GetSpecialType(SpecialType.System_UInt16);
			Uint 	= compilation.GetSpecialType(SpecialType.System_UInt32);
			Ulong 	= compilation.GetSpecialType(SpecialType.System_UInt64);
			Float 	= compilation.GetSpecialType(SpecialType.System_Single);
			Double	= compilation.GetSpecialType(SpecialType.System_Double);
			Bool 	= compilation.GetSpecialType(SpecialType.System_Boolean);
			Byte	= compilation.GetSpecialType(SpecialType.System_Byte);
			Sbyte 	= compilation.GetSpecialType(SpecialType.System_SByte);
			Decimal = compilation.GetSpecialType(SpecialType.System_Decimal);
			Char 	= compilation.GetSpecialType(SpecialType.System_Char);
			String 	= compilation.GetSpecialType(SpecialType.System_String);
			Object 	= compilation.GetSpecialType(SpecialType.System_Object);
			Void 	= compilation.GetSpecialType(SpecialType.System_Void);
		}
	}
}