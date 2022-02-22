using BenchmarkDotNet.Attributes;
using System.Threading.Tasks;
using System.IO.Pipelines;
using Open.IO.Extensions;

namespace Open.Text.CSV.Test;

//[MemoryDiagnoser]
public class PipelineBenchmarks : FileStreamReadBenchmark
{
	public PipelineBenchmarks(string testFile = null) : base(testFile) { }

	[Benchmark]
	public async Task<long> PipeReader_EnumerateAsync()
	{
		long count = 0;
		using var stream = GetStream();
		await foreach (var sequence in PipeReader
			.Create(stream, new StreamPipeReaderOptions(bufferSize: ByteBufferSize))
			.EnumerateAsync())
		{
			foreach (var mem in sequence)
				count += mem.Length;
		}

		return count;
	}
}
