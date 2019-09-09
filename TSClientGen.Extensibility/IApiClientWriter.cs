namespace TSClientGen.Extensibility
{
	/// <summary>
	/// Interface to extend API client code generation with some custom logic
	/// </summary>
	/// <remarks>
	/// Usage example: register API clients in app's DI container
	/// </remarks>
	public interface IApiClientWriter
	{
		/// <summary>
		/// Write required imports
		/// </summary>
		void WriteImports(IIndentedStringBuilder builder);

		/// <summary>
		/// Write custom code before generated API client class
		/// </summary>
		void WriteCodeBeforeApiClientClass(IIndentedStringBuilder builder);

		/// <summary>
		/// Write custom code after generated API client class
		/// </summary>
		void WriteCodeAfterApiClientClass(IIndentedStringBuilder builder);
	}
}