using BenchmarkDotNet.Attributes;
using System.Threading.Tasks;
using System.IO.Pipelines;

namespace Open.Text.CSV.Test;

public class PipelineBenchmarks : FileReadBenchmarkBase
{
	[Benchmark]
	public async Task<long> PipeReader_EnumerateAsync()
	{
		long count = 0;
		await foreach(var sequence in PipeReader.Create(Stream).EnumerateAsync())
		{
			count += sequence.Length;
		}

		return count;
	}
}
