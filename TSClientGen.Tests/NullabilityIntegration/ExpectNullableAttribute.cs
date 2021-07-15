using System;

namespace TSClientGen.Tests.NullabilityIntegration
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ExpectNullableAttribute : Attribute
    {
        
    }
}