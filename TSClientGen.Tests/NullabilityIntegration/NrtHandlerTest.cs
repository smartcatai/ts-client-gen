using NUnit.Framework;

namespace TSClientGen.Tests.NullabilityIntegration
{
    [TestFixture]
    public class NrtHandlerTest : HandlerTestBase<NrtHandlerTest.TestCases>
    {
        protected override TypeMappingConfig Config => new TypeMappingConfig(NullabilityHandling.Nrt);
        
        public class TestCases
        {
#nullable enable
            // ReSharper disable below InconsistentNaming
            [ExpectOptional]
            public int? Value_type__nullable__no_override { get; set; }

            [ExpectOptional]
            [TSSubstituteType("any")]
            public int? Value_type__nullable__with_override { get; set; }
            
            public int Value_type__not_nullable__no_override { get; set; }

            [TSSubstituteType("any")]
            public int Value_type__not_nullable__with_override { get; set; }

#nullable disable

            public string Reference_type__nrt_disabled__no_override { get; set; }

            [TSSubstituteType("any")]
            public string Reference_type__nrt_disabled__with_override { get; set; }
            
#nullable enable
            
            public string Reference_type__not_null__no_override { get; set; } = null!;

            [TSSubstituteType("any")]
            public string Reference_type__not_null__with_override { get; set; } = null!;
            
            [ExpectNullable]
            public string? Reference_type__null__no_override { get; set; }

            [ExpectNullable]
            [TSSubstituteType("any")]
            public string? Reference_type__null__with_override { get; set; }
        }
    }
}