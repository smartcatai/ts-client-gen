using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace TSClientGen.Tests
{
	[TestFixture]
	public class EnumModuleGeneratorTest
	{
		[Test]
		public void String_enums_are_written_if_option_is_specified()
		{
			var generator = new EnumModuleGenerator();
			generator.Write(
				typeof(Foo),
				true,
				null,
				null,
				null);

			var result = generator.GetResult();
			TextAssert.ContainsLinesInCorrectOrder(result,
				"export enum Foo {",
				"A = 'A',",
				"B = 'B',");
		}

		[Test]
		public void Numeric_enum_values_are_written_to_TypeScript_enum_definition()
		{
			var generator = new EnumModuleGenerator();

			generator.Write(
				typeof(Foo),
				false,
				null,
				null,
				null);

			var result = generator.GetResult();
			TextAssert.ContainsLinesInCorrectOrder(result,
				"export enum Foo {",
				"A = 1,",
				"B = 3,");
		}

		/// If we don't do that we can end up having this issue:
		///
		/// <code>
		/// export enum Foo {
		///     A, B
		/// }
		///
		/// export namespace Foo {
		///     function getBar(foo: Foo) {
		///         return Bar.C;
		///     }
		/// }
		///
		/// export enum Bar {
		///     C
		/// }
		/// </code>
		///
		/// This code compiles but it will crash at runtime because in TypeScript declaration order matters.
		[Test]
		public void Should_write_static_members_for_enums_after_all_enums_declaration()
		{
			var generator = new EnumModuleGenerator();
			var staticMembers = new StaticMembers();
			staticMembers.AddGenerator(() => StaticMemberContents);

			generator.Write(
				typeof(Foo),
				false,
				"locale",
				 staticMembers,
				null);

			var result = generator.GetResult();
			TextAssert.ContainsLinesInCorrectOrder(result,
				"export enum Foo {",
				"export namespace Foo {",
				StaticMemberContents);
		}

		[Test]
		public void Should_write_enum_imports_before_namespace()
		{
			var generator = new EnumModuleGenerator();
			var staticMembers = new StaticMembers(enumImportTypes: new[] { typeof(Boo)});
			staticMembers.AddGenerator(() => StaticMemberContents);

			generator.Write(
				typeof(Foo),
				false,
				"locale",
				 staticMembers,
				null);

			var result = generator.GetResult();
			TextAssert.ContainsLinesInCorrectOrder(result,
				"import { Boo } from '../Boo';",
				"export namespace Foo {",
				StaticMemberContents);
		}

		enum Foo
		{
			A = 1,
			B = 3
		}

		enum Boo
		{
		}

		public const string StaticMemberContents = "// Extending Enum with static members";
	}
}
