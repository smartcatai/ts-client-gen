using System;
using System.Linq;
using System.Resources;
using System.Text;

namespace TSClientGen
{
	/// <summary>
	/// For applying to assembly.
	/// Allows to bind enum type to a server-side resx file and to expose enum values' localized names to the frontend.
	/// You should provide a plugin that handles resource file generation for the frontend when using this attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class TSEnumLocalizationAttribute : TSExtendEnumAttribute
	{
		public TSEnumLocalizationAttribute(Type enumType, Type resxType, bool usePrefix = false, string[] additionalContexts = null)
			: base(enumType)
		{
			if (resxType == null) throw new ArgumentNullException(nameof(resxType));

			ResxName = resxType.FullName;
			ResourceManager = resxType.GetResourceManager();
			AdditionalContexts = additionalContexts;
			UsePrefix = usePrefix;
		}

		/// <summary>
		/// Full name of the server-side resx file
		/// </summary>
		public string ResxName { get; }

		/// <summary>
		/// <see cref="ResourceManager"/> instance for the server-side resx file to be exposed to frontend
		/// </summary>
		public ResourceManager ResourceManager { get; }

		/// <summary>
		/// Specifies whether enum values are prefixed with enum name in a server-side resource file
		/// </summary>
		public bool UsePrefix { get; }
		
		/// <summary>
		/// Allows for generating more than one set of enum values' localized names
		/// </summary>
		public string[] AdditionalContexts { get; }
		

		public override void GenerateStaticMembers(StringBuilder sb)
		{
			if (AdditionalContexts == null)
			{
				sb
					.AppendLine($"export function localize(enumValue: {EnumType.Name}) {{")
					.AppendLine($"\treturn getResource('{EnumType.Name}_' + {EnumType.Name}[enumValue]);")
					.AppendLine("}");
			}
			else
			{
				var contextTypeScriptType = string.Join("|", AdditionalContexts.Select(s => $"'{s}'"));
				sb
					.AppendLine($"export function localize(enumValue: {EnumType.Name}, context?: {contextTypeScriptType}) {{")
					.AppendLine($"\tconst prefix = '{EnumType.Name}_' + (context ? context + '_' : '');")
					.AppendLine($"\treturn getResource(prefix + {EnumType.Name}[enumValue]);")
					.AppendLine("}");
			}

			sb
				.AppendLine("export function getLocalizedValues() {")
				.AppendLine($"\treturn Object.keys({EnumType.Name}).map(key => parseInt(key)).filter(key => !isNaN(key)).map(id => {{")
				.AppendLine("\t\treturn {")
				.AppendLine("\t\t\tid: id,")
				.AppendLine($"\t\t\tname: {EnumType.Name}.localize(id)")
				.AppendLine("\t\t};")
				.AppendLine("\t});")
				.AppendLine("}");
		}
	}
}