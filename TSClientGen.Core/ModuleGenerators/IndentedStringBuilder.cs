using System;
using System.Linq;
using System.Text;

namespace TSClientGen
{
	public class IndentedStringBuilder : IIndentedStringBuilder
	{
		public IIndentedStringBuilder Indent()
		{
			if (!_shouldWriteIndenting && _result.Length > 0)
				throw new InvalidOperationException("Trying to indent while not finished writing the current line");

			_indentLevel++;
			_shouldWriteIndenting = true;
			return this;
		}

		public IIndentedStringBuilder Unindent()
		{
			if (_indentLevel == 0)
				throw new InvalidOperationException("Trying to unindent while current indent level is already 0");
			
			if (!_shouldWriteIndenting && _result.Length > 0)
				throw new InvalidOperationException("Trying to unindent while not finished writing the current line");
			
			_indentLevel--;
			return this;
		}

		public IIndentedStringBuilder Append(string text)
		{
			writeIndentingIfNeeded();
			_result.Append(text);
			return this;
		}

		public IIndentedStringBuilder AppendLine(string text = null)
		{
			writeIndentingIfNeeded();
			if (text != null)
				_result.AppendLine(text);
			else
				_result.AppendLine();
			
			_shouldWriteIndenting = true;
			return this;
		}

		public IIndentedStringBuilder AppendText(string text)
		{
			var lines = text.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
			foreach (var line in lines)
			{
				AppendLine(line);
			}
			return this;
		}

		public override string ToString()
		{
			return _result.ToString();
		}

		private void writeIndentingIfNeeded()
		{
			if (_shouldWriteIndenting)
			{
				_shouldWriteIndenting = false;
				for (int i = 0; i < _indentLevel; i++)
					_result.Append("\t");
			}
		}
		
		private readonly StringBuilder _result = new StringBuilder();
		private int _indentLevel;
		private bool _shouldWriteIndenting;
	}
}