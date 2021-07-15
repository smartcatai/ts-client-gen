using NUnit.Framework;

namespace TSClientGen.Tests.NullabilityIntegration
{
    [TestFixture]
    public class DefaultHandlerTest : HandlerTestBase<DefaultHandlerTest.TestCases>
    {
        protected override TypeMappingConfig Config => new TypeMappingConfig(NullabilityHandling.Default);

        public class TestCases
        {
            // ReSharper disable below InconsistentNaming
            [ExpectOptional]
            public int? Value_type__nullable__no_override { get; set; }

            [ExpectOptional]
            [TSSubstituteType("any")]
            public int? Value_type__nullable__with_override { get; set; }
            
            public int Value_type__not_nullable__no_override { get; set; }

            [TSSubstituteType("any")]
            public int Value_type__not_nullable__with_override { get; set; }
            
            public string Reference_type__default__no_override { get; set; }

            [TSSubstituteType("any")]
            public string Reference_type__default__with_override { get; set; }
        }
    }
}