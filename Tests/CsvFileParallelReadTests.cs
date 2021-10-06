using BenchmarkDotNet.Attributes;
using System.Threading.Tasks;

namespace Open.Text.CSV.Test;

public class CsvFileParallelReadTests
{
	readonly CsvFileReadBenchmarks _root = new();

	[Benchmark]
	public void CsvReader_GetAllRowsFromFileInParallel()
	{
		void action() => _root.CsvReader_GetAllRowsFromFile();
		Parallel.Invoke(action, action, action, action);
	}

	[Benchmark]
	public Task CsvReader_GetAllRowsFromFileAsyncParallel()
		=> Task.WhenAll(
			_root.CsvReader_GetAllRowsFromFileAsync(),
			_root.CsvReader_GetAllRowsFromFileAsync(),
			_root.CsvReader_GetAllRowsFromFileAsync(),
			_root.CsvReader_GetAllRowsFromFileAsync());
}
