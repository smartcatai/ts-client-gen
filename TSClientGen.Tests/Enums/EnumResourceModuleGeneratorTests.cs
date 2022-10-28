using System;
using Moq;
using NUnit.Framework;
using TSClientGen.Extensibility;
using TSClientGen.Tests.Enums;

namespace TSClientGen.Tests
{
	[TestFixture]
	public class EnumResourceModuleGeneratorTests
	{
		[Test]
		public void Should_add_resources_for_each_enum_value_and_additional_sets()
		{
			var writer = new Mock<IResourceModuleWriter>();
			var generator = new EnumResourceModuleGenerator(writer.Object);
			var set1 = Guid.NewGuid().ToString();
			var set2 = Guid.NewGuid().ToString();

			generator.WriteEnumLocalizations(
				typeof(EnumToLocalizeViaAttributeOnEnum),
				new TSEnumLocalizationAttribute(typeof(EnumsForTestsResources), additionalSets: new [] { set1, set2 })
			);

			const string enumName = nameof(EnumToLocalizeViaAttributeOnEnum);
			var key1 = EnumToLocalizeViaAttributeOnEnum.Value1;
			var key2 = EnumToLocalizeViaAttributeOnEnum.Value2;
			var value1 = EnumsForTestsResources.Value1;
			var value2 = EnumsForTestsResources.Value2;
			writer.Verify(w => w.AddResource($"{enumName}_{key1}", value1));
			writer.Verify(w => w.AddResource($"{enumName}_{key2}", value2));
			writer.Verify(w => w.AddResource($"{enumName}_{set1}_{key1}", value1));
			writer.Verify(w => w.AddResource($"{enumName}_{set2}_{key1}", value1));
			writer.Verify(w => w.AddResource($"{enumName}_{set1}_{key2}", value2));
			writer.Verify(w => w.AddResource($"{enumName}_{set2}_{key2}", value2));
		}
	}
}