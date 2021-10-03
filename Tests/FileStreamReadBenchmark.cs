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
		while ((next = Stream.Read(ByteBuffer)) is not 0)
			count += next;
		return count;
	}

	[Benchmark]
	public async Task<int> FileStream_ReadAsync()
	{
		var count = 0;
		int next;
		while ((next = await Stream.ReadAsync(ByteBuffer).ConfigureAwait(false)) is not 0)
			count += next;
		return count;
	}
}
