using System;
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
            var extensions = Enumerable.Empty<TSExtendEnumAttribute>().ToLookup(x => default(Type));
            generator.Write(new[] { typeof(Foo) }, true, null, extensions);
            
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
            var extensions = Enumerable.Empty<TSExtendEnumAttribute>().ToLookup(x => default(Type));
            generator.Write(new[] { typeof(Foo) }, false, null, extensions);
            
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
        /// This code compiles but it will crash at runtime because in JS declaration order matters.
        [Test]
        public void Should_write_static_members_for_enums_after_all_enums_declaration()
        {
            var generator = new EnumModuleGenerator();
            var extensions = new []
                {
                    (Type: typeof(Foo), Extension: (TSExtendEnumAttribute) new ExtendFooAttribute())
                }
                .ToLookup(_ => _.Type, _ => _.Extension);

            generator.Write(
                new [] { typeof(Foo), typeof(Bar)},
                false,
                "locale",
                extensions);

            var result = generator.GetResult();
            TextAssert.ContainsLinesInCorrectOrder(result,
                "export enum Foo {",
                "export enum Bar {",
                "export namespace Foo {",
                ExtendFooAttribute.StaticMemberContents);
        }

        enum Foo
        {
            A = 1,
            B = 3
        }

        enum Bar {}

        class ExtendFooAttribute : TSExtendEnumAttribute
        {
            public ExtendFooAttribute() : base(typeof(Foo))
            {
            }

            public override void GenerateStaticMembers(StringBuilder sb)
            {
                sb.AppendLine(StaticMemberContents);
            }

            public const string StaticMemberContents = "// Extending Enum with static members";
        }
    }
}