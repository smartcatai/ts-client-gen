using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;
using NUnit.Framework;
using TSClientGen.Extensibility;

namespace TSClientGen.Tests
{
	[TestFixture]
	public class RunnerTests
	{
		[TestCase]
		public void Extend_enum_attributes_are_collected_when_applied_to_enum_type_or_assembly()
		{
			var runner = createRunner();
			var allEnums = new HashSet<Type>
			{
				typeof(EnumToExtendViaAssemblyAttr),
				typeof(EnumToExtendViaAttributeOnEnum),
				typeof(EnumToExtendViaTwoAttributes)
			};

			var staticMemberGeneratorsByEnumType = runner
				.CollectEnumStaticMemberGenerators(new[] { Assembly.GetExecutingAssembly() }, allEnums)
				.Select(pair => (EnumType: pair.Key, GeneratorsCount: pair.Value.Count))
				.ToList();

			CollectionAssert.Contains(
				staticMemberGeneratorsByEnumType,
				(EnumType: typeof(EnumToExtendViaAssemblyAttr), GeneratorsCount: 1),
				"Static members generator attribute is not picked up when applied to assembly");

			CollectionAssert.Contains(
				staticMemberGeneratorsByEnumType,
				(EnumType: typeof(EnumToExtendViaAttributeOnEnum), GeneratorsCount: 1),
				"Static members generator attribute is not picked up when applied to enum type");

			CollectionAssert.Contains(
				staticMemberGeneratorsByEnumType,
				(EnumType: typeof(EnumToExtendViaTwoAttributes), GeneratorsCount: 2),
				"Static members generator attributes aren't picked up when applied both to assembly and to enum type");
		}

		[TestCase]
		public void Enums_with_static_members_added_via_assembly_attribute_are_included_in_all_enums_list()
		{
			var runner = createRunner();
			var allEnums = new HashSet<Type>();

			runner.CollectEnumStaticMemberGenerators(new[] { Assembly.GetExecutingAssembly() }, allEnums);

			CollectionAssert.AreEquivalent(
				new[]
				{
					typeof(EnumToExtendViaAssemblyAttr),
					typeof(EnumToExtendViaTwoAttributes)
				},
				allEnums);
		}

		[TestCase]
		public void Enum_localization_attributes_are_collected_when_applied_to_enum_type_or_assembly()
		{
			var runner = createRunner();
			var allEnums = new HashSet<Type>
			{
				typeof(EnumToLocalizeViaAttributeOnEnum)
			};

			var localizationAttrsByEnumType = runner.CollectEnumLocalizationAttributes(
				new[] { Assembly.GetExecutingAssembly() }, allEnums);

			Assert.AreEqual(
				typeof(ForAssembly.TSEnumLocalizationAttribute),
				localizationAttrsByEnumType[typeof(EnumToLocalizeViaAssemblyAttribute)].GetType(),
				"Enum localization attribute is not picked up when applied to assembly");

			Assert.AreEqual(
				typeof(TSEnumLocalizationAttribute),
				localizationAttrsByEnumType[typeof(EnumToLocalizeViaAttributeOnEnum)].GetType(),
				"Enum localization attribute is not picked up when applied to enum type");
		}

		[TestCase]
		public void Enums_with_localization_attributes_applied_to_assembly_are_included_in_all_enums_list()
		{
			var runner = createRunner();
			var allEnums = new HashSet<Type>();

			runner.CollectEnumLocalizationAttributes(new[] { Assembly.GetExecutingAssembly() }, allEnums);

			CollectionAssert.AreEquivalent(new[] { typeof(EnumToLocalizeViaAssemblyAttribute) }, allEnums);
		}

		private static Runner createRunner()
		{
			return new Runner(
				new Arguments(),
				new Mock<IApiDiscovery>().Object,
				null,
				null,
				new Mock<IResultFileWriter>().Object,
				o => o.ToString());
		}
	}
}