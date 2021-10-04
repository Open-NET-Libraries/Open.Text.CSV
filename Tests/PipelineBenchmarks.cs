using BenchmarkDotNet.Attributes;
using System.Threading.Tasks;
using System.IO.Pipelines;

namespace Open.Text.CSV.Test;

[MemoryDiagnoser]
public class PipelineBenchmarks : FileStreamReadBenchmark
{
	[Benchmark]
	public async Task<long> PipeReader_EnumerateAsync()
	{
		long count = 0;
		await foreach(var sequence in PipeReader
			.Create(Stream, new StreamPipeReaderOptions(bufferSize: ByteBufferSize))
			.EnumerateAsync())
		{
			foreach (var mem in sequence)
				count += mem.Length;
		}

		return count;
	}
}
