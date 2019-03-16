using System;

namespace TSClientGen.ApiDescriptors
{
	/// <summary>
	/// Describes a parameter for an api method
	/// </summary>
	public class MethodParamDescriptor
	{
		public MethodParamDescriptor(string name, string type, bool isOptional, bool isBodyContent, bool isUploadedFile, bool isModelWithFiles)
		{
			if (!isBodyContent && (isUploadedFile || isModelWithFiles))
				throw new ArgumentException($"Method parameter {name} represents file(s) to upload but is not marked as a request body content");
			
			OriginalName = name;
			GeneratedName = name;
			Type = type;
			IsOptional = isOptional;
			IsBodyContent = isBodyContent;
			IsUploadedFile = isUploadedFile;
			IsModelWithFiles = isModelWithFiles;
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
		/// Parameter TypeScript type
		/// </summary>
		public string Type { get; }
		
		/// <summary>
		/// Whether a parameter should be generated as optional in TypeScript method
		/// </summary>
		public bool IsOptional { get; }
		
		/// <summary>
		/// Whether a parameter value is passed in request body
		/// </summary>
		public bool IsBodyContent { get; }

		/// <summary>
		/// Whether a parameter represents a file (or multiple files) to upload by a multipart content request
		/// </summary>
		public bool IsUploadedFile { get; }
		
		/// <summary>
		/// Whether a parameter represents a file or multiple files to upload by a multipart content request
		/// along with a dedicated request body part containing a json object
		/// </summary>
		public bool IsModelWithFiles { get; }
	}
}