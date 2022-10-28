using System;
using NUnit.Framework;

namespace TSClientGen.Tests
{
	[TestFixture]
	public class ImportEnumModuleGeneratorTests
	{
		[Test]
		public void Should_write_import_and_export_statements_of_enums()
		{
			var generator = new ImportEnumModuleGenerator();
			var enumModuleName = Guid.NewGuid().ToString();
			const string enum1 = nameof(Enum1);
			const string enum2 = nameof(Enum2);

			var result = generator.Generate(new[] { typeof(Enum1), typeof(Enum2) }, enumModuleName);

			Assert.Multiple( () =>
				{
					TextAssert.ContainsLine($"import {{ {enum1} }} from './{enumModuleName}/{enum1}'", result);
					TextAssert.ContainsLine($"import {{ {enum2} }} from './{enumModuleName}/{enum2}'", result);
					TextAssert.ContainsLinesInCorrectOrder(result,
						"export {",
						$"{enum1},",
						$"{enum2},",
						"}"
					);
				}
			);
		}
	}
}