namespace TSClientGen
{
	/// <summary>
	/// Provides JSON serialization functionality.
	/// Is used to serialize the Content property of <see cref="TSClientGen.TSStaticContentAttribute"/>. 
	/// </summary>
	public interface IJsonSerializer
	{
		/// <summary>
		/// Serializes arbitrary dictionary of arbitrary objects to JSON.
		/// Is used to serialize the Content property of <see cref="TSClientGen.TSStaticContentAttribute"/>. 
		/// </summary>
		string Serialize(object obj);
	}
}