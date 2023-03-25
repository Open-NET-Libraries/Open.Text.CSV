using System;
using System.IO;

namespace Open.Text.CSV.Test;
public abstract class TextReaderAsyncBenchmarkBase
{
	protected const string TEST_FILE = "TestData.csv";
	protected TextReaderAsyncBenchmarkBase(string path = TEST_FILE)
	{
		using var reader = File.OpenText(path);
		Source = reader.ReadToEnd();
	}

	public string Source { get; }

	public TextReader GetReader() => new StringReader(Source);

	protected int Consume(ReadOnlyMemory<char> chunk)
	{
		var span = chunk.Span;
		var len = span.Length;
		for(var i = 0; i<len; i++)
		{
			_ = Read(span, i);
		}
		return len;
	}

	protected char Read(ReadOnlySpan<char> chunk, int index)
		=> chunk[index];
}
