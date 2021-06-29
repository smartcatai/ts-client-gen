using System.ComponentModel.DataAnnotations;
using NUnit.Framework;

namespace TSClientGen.Tests.NullabilityIntegration
{
    [TestFixture]
    public class DataAnnotationsHandlerTest : HandlerTestBase<DataAnnotationsHandlerTest.TestCases>
    {
        protected override TypeMappingConfig Config => new TypeMappingConfig(NullabilityHandling.DataAnnotations);
        
        public class TestCases
        {
            // ReSharper disable below InconsistentNaming
            [ExpectOptional]
            public int? Value_type__nullable__no_attr__no_override { get; set; }

            [ExpectOptional]
            [TSSubstituteType("any")]
            public int? Value_type__nullable__no_attr__with_override { get; set; }

            [ExpectOptional]
            public int? Value_type__nullable__required__no_override { get; set; }

            [ExpectOptional]
            [TSSubstituteType("any")]
            public int? Value_type__nullable__required__with_override { get; set; }
            
            public int Value_type__not_nullable__no_attr__no_override { get; set; }

            [TSSubstituteType("any")]
            public int Value_type__not_nullable__no_attr__with_override { get; set; }
            
            [Required]
            public int Value_type__not_nullable__required__no_override { get; set; }

            [Required]
            [TSSubstituteType("any")]
            public int Value_type__not_nullable__required__with_override { get; set; }
            
            [ExpectNullable]
            public string Reference_type__no_attr__no_override { get; set; }
            
            [Required]
            public string Reference_type__required__no_override { get; set; }

            [ExpectNullable]
            [TSSubstituteType("any")]
            public string Reference_type__no_attr__with_override { get; set; }
            
            [Required]
            public string Reference_type__required__with_override { get; set; }
        }
    }
}