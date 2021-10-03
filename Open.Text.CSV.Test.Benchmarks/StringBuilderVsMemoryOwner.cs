using BenchmarkDotNet.Attributes;
using System.Text;

namespace Open.Text.CSV.Test.Benchmarks;

[MemoryDiagnoser]
public class StringBuilderVsMemoryOwner
{
	readonly StringBuilder _sb = new();
	readonly ExpandableMemory<char> _em = new();

	[Benchmark]
	public string StringBuilderTest()
	{
		for (char i = (char)0; i < char.MaxValue; i++)
		{
			_sb.Append(i);
		}

		var result = _sb.ToString();
		_sb.Clear();
		return result;
	}

	[Benchmark]
	public string MemoryOwnerTest()
	{
		for (char i = (char)0; i < char.MaxValue; i++)
		{
			_em.Add(i);
		}

		var result = _em.Span.ToString();
		_em.Clear();
		return result;
	}
}
