using BenchmarkDotNet.Attributes;
using System.Threading.Tasks;

namespace Open.Text.CSV.Test;

public class FileStreamReadBenchmark : FileReadBenchmarkBase
{
	[Benchmark(Baseline = true)]
	public int FileStream_Read()
	{
		var count = 0;
		int next;
		while ((next = Stream.Read(ByteBufferMemory.Span)) is not 0)
			count += next;
		return count;
	}

	[Benchmark]
	public async Task<int> FileStream_ReadAsync()
	{
		var count = 0;
		int next;
		while ((next = await Stream.ReadAsync(ByteBufferMemory).ConfigureAwait(false)) is not 0)
			count += next;
		return count;
	}

	[Benchmark]
	public async Task<long> FileStream_EnumerateAsync()
	{
		long count = 0;
		await foreach(var sequence in Stream.EnumerateAsync(ByteBufferSize).ConfigureAwait(false))
		{
			foreach(var mem in sequence)
				count += mem.Length;
		}
		return count;
	}
}
