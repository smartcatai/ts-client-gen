using System;
using System.Linq;
using NUnit.Framework;

namespace TSClientGen.Tests
{
	public static class TextAssert
	{
		public static void ContainsLine(string expectedLine, string output)
		{
			Assert.Contains(
				expectedLine,
				output.Split("\r\n").Select(s => s.Trim('\t')).ToList(),
				$"Expected line '{expectedLine}' not found\nActual output: \n{output}");
		}

		public static void DoesNotContainLine(string notExpectedLine, string output)
		{
			CollectionAssert.DoesNotContain(
				output.Split("\r\n").Select(s => s.Trim('\t')).ToList(),
				notExpectedLine,
				$"Found line '{notExpectedLine}' that shouldn't be there\nActual output: \n{output}");
		}

		public static void ContainsLinesInCorrectOrder(string output, params string[] expectedLines)
		{
			var actualLines = output.Split("\r\n").Select(s => s.Trim('\t')).ToList();
			int ix = 0;
			foreach (var expectedLine in expectedLines)
			{
				ix = actualLines.IndexOf(expectedLine, ix);
				if (ix == -1)
					Assert.Fail($"Expected line '{expectedLine}' not found in correct order\nActual output: \n{output}");

				ix++;
			}
		}
	}
}