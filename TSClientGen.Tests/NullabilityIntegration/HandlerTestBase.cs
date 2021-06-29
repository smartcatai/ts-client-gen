using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TSClientGen.Nullability;

namespace TSClientGen.Tests.NullabilityIntegration
{
    public abstract class HandlerTestBase<TCases>
    {
        protected abstract TypeMappingConfig Config { get; }

        protected static IEnumerable<TestCaseData> EnumerateTestCases()
        {
            var props = typeof(TCases).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in props)
            {
                var attrs = p.GetCustomAttributes(inherit: false);
                var optional = attrs.Any(x => x.GetType() == typeof(ExpectOptionalAttribute));
                var nullable = attrs.Any(x => x.GetType() == typeof(ExpectNullableAttribute));
                var propName = p.Name;
                var expected = TsZeroType.OptionalNullable(nullable, optional);
                yield return new TestCaseData(p, expected) {TestName = propName};
            }
        }

        [Test]
        [TestCaseSource(nameof(EnumerateTestCases))]
        public void Test(PropertyInfo property, TsZeroType expected)
        {
            var handler = NullabilityHandlerResolver.FromConfig(Config);
            var actual = handler.GetTsNullability(property);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}