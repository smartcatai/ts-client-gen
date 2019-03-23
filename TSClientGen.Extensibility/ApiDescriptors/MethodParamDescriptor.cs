using System;

namespace TSClientGen.Extensibility.ApiDescriptors
{
	/// <summary>
	/// Describes a parameter for an api method
	/// </summary>
	public class MethodParamDescriptor
	{
		public MethodParamDescriptor(string name, Type type, bool isOptional, bool isBodyContent)
		{			
			OriginalName = name;
			GeneratedName = name;
			Type = type;
			IsOptional = isOptional;
			IsBodyContent = isBodyContent;
		}
		
		
		/// <summary>
		/// Original parameter name
		/// </summary>
		public string OriginalName { get; }
		
		/// <summary>
		/// Parameter name in a generated method
		/// (can differ from the original one to avoid conflicts with another identifiers in a module)
		/// </summary>
		public string GeneratedName { get; set; }
		
		/// <summary>
		/// Parameter type
		/// </summary>
		public Type Type { get; }
		
		/// <summary>
		/// Whether a parameter should be generated as optional in TypeScript method
		/// </summary>
		public bool IsOptional { get; }
		
		/// <summary>
		/// Whether a parameter value is passed in request body
		/// </summary>
		public bool IsBodyContent { get; }
	}
}