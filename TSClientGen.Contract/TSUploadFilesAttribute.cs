using System;

namespace TSClientGen
{
	/// <summary>
	/// For applying to api controller method.
	/// Specifies that it is a multipart data request for file uploading
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class TSUploadFilesAttribute: Attribute
	{
	}
}