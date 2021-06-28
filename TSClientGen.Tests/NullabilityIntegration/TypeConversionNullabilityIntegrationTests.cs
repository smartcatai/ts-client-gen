using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

namespace TSClientGen.Tests.NullabilityIntegration
{
	[TestFixture]
	public class TypeConversionNullabilityIntegrationTests
	{
		[Theory]
		[TestCaseSource(nameof(testCases))]
		public void Test(Type type, TypeMappingConfig config, string expectedGeneration)
		{
			var mapping = new TypeMapping(config: config);
			mapping.AddType(type);
			var generatedTs = mapping.GetGeneratedTypes()[type];
			Assert.That(generatedTs, Is.EqualTo(expectedGeneration));
		}

		private static IEnumerable<TestCaseData> testCases() =>
			CodeGen.NullabilityIntegrationTestCases.EnumerateCases()
				.Select(x => new TestCaseData(x.testType, x.config, x.expectation) { TestName = x.caseName });
	}
}