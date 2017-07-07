using System;
using System.Reflection;
using System.Resources;
using System.Text;

namespace TSClientGen
{
	/// <summary>
	/// Атрибут, который позволяет связать enum с resx-файлом, в котором содержится
	/// локализация значений этого enum'а - чтобы иметь доступ к этим локализациям на клиенте
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class TypeScriptEnumLocalizationAttribute : TypeScriptExtendEnumAttribute
	{
		public TypeScriptEnumLocalizationAttribute(Type enumType, Type resxType, bool usePrefix = false)
			: base(enumType)
		{
			if (resxType == null) throw new ArgumentNullException(nameof(resxType));

			var resourceManagerProperty = resxType.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (resourceManagerProperty == null)
				throw new ArgumentException($"Parameter must have static property ResourceManager (type {resxType.FullName})", nameof(resxType));
			if (resourceManagerProperty.PropertyType != typeof(ResourceManager))
				throw new ArgumentException("Static property ResourceManager has unexpected type (type {resxType.FullName})", nameof(resxType));

			ResxName = resxType.FullName;
			ResourceManager = (ResourceManager)resourceManagerProperty.GetValue(null);
			UsePrefix = usePrefix;
		}

		public string ResxName { get; }

		public ResourceManager ResourceManager { get; }

		public bool UsePrefix { get; }

		public override void GenerateStaticMembers(StringBuilder sb)
		{
			sb.AppendLine($"\texport function localize(enumValue: {EnumType.Name}) {{");
			sb.AppendLine($"\t\treturn getResource('{EnumType.Name}_' + {EnumType.Name}[enumValue]);");
			sb.AppendLine("\t}");

			sb.AppendLine("\texport function getLocalizedValues() {");
			sb.AppendLine($"\t\treturn Object.keys({EnumType.Name}).map(key => parseInt(key)).filter(key => !isNaN(key)).map(id => {{");
			sb.AppendLine("\t\t\treturn {");
			sb.AppendLine("\t\t\t\tid: id,");
			sb.AppendLine($"\t\t\t\tname: {EnumType.Name}.localize(id)");
			sb.AppendLine("\t\t\t};");
			sb.AppendLine("\t\t});");
			sb.AppendLine("\t}");
		}
	}
}