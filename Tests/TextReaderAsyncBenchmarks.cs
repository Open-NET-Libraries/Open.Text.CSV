using BenchmarkDotNet.Attributes;

namespace Open.Text.CSV.Test;

[MemoryDiagnoser]
public class TextReaderAsyncBenchmarks : TextReaderAsyncBenchmarkBase
{
	public TextReaderAsyncBenchmarks(string path = null) : base(path)
	{
	}
}
