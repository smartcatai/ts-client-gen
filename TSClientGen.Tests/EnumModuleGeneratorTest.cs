using System;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace TSClientGen.Tests
{
    [TestFixture]
    public class EnumModuleGeneratorTest
    {
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
                "locale",
                extensions);

            var result = generator.GetResult();
            var expected =
@"export enum Foo {
}

export enum Bar {
}

export namespace Foo {
	// !!!
}
";

            Assert.AreEqual(expected, result);
        }

        enum Foo {}

        enum Bar {}

        class ExtendFooAttribute : TSExtendEnumAttribute
        {
            public ExtendFooAttribute() : base(typeof(Foo))
            {
            }

            public override void GenerateStaticMembers(StringBuilder sb)
            {
                sb.AppendLine("// !!!");
            }
        }
    }
}