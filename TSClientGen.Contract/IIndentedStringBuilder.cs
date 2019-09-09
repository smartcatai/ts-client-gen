namespace TSClientGen
{
	/// <summary>
	/// Interface that encapsulates working with indents and lines when generating the code
	/// </summary>
	public interface IIndentedStringBuilder
	{
		IIndentedStringBuilder Indent();
		IIndentedStringBuilder Unindent();
		IIndentedStringBuilder Append(string text);
		IIndentedStringBuilder AppendLine(string text = null);
		IIndentedStringBuilder AppendText(string text);
	}
}