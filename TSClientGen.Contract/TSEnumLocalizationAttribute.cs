using System;
using System.Resources;

namespace TSClientGen
{
	/// <summary>
	/// Is not intended for direct use, use its descendants <see cref="TSEnumLocalizationAttribute"/> or <see cref="TSClientGen.ForAssembly.TSEnumLocalizationAttribute"/> instead.
	/// Base class for attributes that allow binding enum type to a server-side resx file and to expose enum values' localized names to the frontend.
	/// </summary>
	public abstract class TSEnumLocalizationAttributeBase : Attribute
	{
		protected TSEnumLocalizationAttributeBase(Type resxType, bool usePrefix = false,
			string[] additionalContexts = null)
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
	}

	/// <summary>
	/// For applying to enum type.
	/// Allows binding enum type to a server-side resx file and to expose enum values' localized names to the frontend.
	/// You should provide a plugin that handles resource file generation for the frontend when using this attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Enum)]
	public sealed class TSEnumLocalizationAttribute : TSEnumLocalizationAttributeBase
	{
		public TSEnumLocalizationAttribute(Type resxType, bool usePrefix = false, string[] additionalContexts = null)
			: base(resxType, usePrefix, additionalContexts)
		{
		}
	}

	namespace ForAssembly
	{
		/// <summary>
		/// For applying to assembly.
		/// Allows binding enum type to a server-side resx file and to expose enum values' localized names to the frontend.
		/// You should provide a plugin that handles resource file generation for the frontend when using this attribute.
		/// </summary>
		[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
		public sealed class TSEnumLocalizationAttribute : TSEnumLocalizationAttributeBase
		{
			public TSEnumLocalizationAttribute(Type enumType, Type resxType, bool usePrefix = false,
				string[] additionalContexts = null)
				: base(resxType, usePrefix, additionalContexts)
			{
				EnumType = enumType;
			}

			/// <summary>
			/// Enum type
			/// </summary>
			public Type EnumType { get; }
		}
	}
}