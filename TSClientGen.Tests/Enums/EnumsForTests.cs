using System;
using System.Reflection;
using System.Text;
using TSClientGen.Tests;
using TSClientGen.Tests.Enums;

[assembly: ExtendEnumAsm(typeof(EnumToExtendViaAssemblyAttr))]
[assembly: ExtendEnumAsm(typeof(EnumToExtendViaTwoAttributes))]

[assembly: TSClientGen.ForAssembly.TSEnumLocalization(typeof(EnumToLocalizeViaAssemblyAttribute), typeof(EnumsForTestsResources))]

namespace TSClientGen.Tests
{
	public enum EnumToExtendViaAssemblyAttr
	{
		Value1,
		Value2
	}

	[ExtendEnum]
	public enum EnumToExtendViaAttributeOnEnum
	{
		Value1,
		Value2
	}

	[ExtendEnum]
	public enum EnumToExtendViaTwoAttributes
	{
		Value1,
		Value2
	}
	
	
	[TSEnumLocalization(typeof(EnumsForTestsResources))]
	public enum EnumToLocalizeViaAttributeOnEnum
	{
		Value1,
		Value2
	}

	public enum EnumToLocalizeViaAssemblyAttribute
	{
		Value1,
		Value2
	}

	public enum Enum1
	{
		Value0,
		Value1,
	}

	public enum Enum2
	{
		Value0,
		Value1,
	}

	public class ExtendEnumAttribute : TSClientGen.TSExtendEnumAttribute
	{
		public override string GenerateStaticMembers()
		{
			return string.Empty;
		}
	}
	
	public class ExtendEnumAsmAttribute : TSClientGen.ForAssembly.TSExtendEnumAttribute
	{
		public ExtendEnumAsmAttribute(Type enumType) : base(enumType)
		{
		}

		public override string GenerateStaticMembers()
		{
			return string.Empty;
		}
	}
}