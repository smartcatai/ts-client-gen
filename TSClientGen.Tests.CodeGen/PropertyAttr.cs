using Newtonsoft.Json;

namespace TSClientGen.Tests.CodeGen
{
    internal abstract class PropertyAttr
    {
        public static PropertyAttr[] All { get; } =
            new[]
            {
                (PropertyAttr)new None(),
                new RequiredAttr(),
                new JsonDefault(),
                new JsonAlways(),
                new JsonAllowNull(),
                new JsonDisallowNull()
            };
			
        private PropertyAttr()
        {	
        }

        public sealed class None : PropertyAttr
        {
            public override string ToString() => nameof(None);
        }

        public sealed class RequiredAttr : PropertyAttr
        {
            public override string ToString() => nameof(RequiredAttr);
        }
			
        public sealed class JsonDefault : PropertyAttr
        {
            public override string ToString() => nameof(JsonDefault);
            public Required Value => Required.Default;
        }

        public sealed class JsonAllowNull : PropertyAttr
        {
            public override string ToString() => nameof(JsonAllowNull);
            public Required Value => Required.AllowNull;
        }

        public sealed class JsonAlways : PropertyAttr
        {
            public override string ToString() => nameof(JsonAlways);
            public Required Value => Required.Always;
        }

        public sealed class JsonDisallowNull : PropertyAttr
        {
            public override string ToString() => nameof(JsonDisallowNull);
            public Required Value => Required.DisallowNull;
        }
    }
}