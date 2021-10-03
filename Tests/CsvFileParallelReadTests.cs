using BenchmarkDotNet.Attributes;
using System.Threading.Tasks;

namespace Open.Text.CSV.Test;

public class CsvFileParallelReadTests
{
	readonly CsvFileReadTests _root = new();

	[Benchmark]
	public void CsvReader_GetAllRowsFromFileInParallel()
	{
		Parallel.Invoke(
			_root.CsvReader_GetAllRowsFromFileTest,
			_root.CsvReader_GetAllRowsFromFileTest,
			_root.CsvReader_GetAllRowsFromFileTest,
			_root.CsvReader_GetAllRowsFromFileTest
		);
	}

	[Benchmark]
	public Task CsvReader_GetAllRowsFromFileAsyncParallel()
	{
		return Task.WhenAll(
			_root.CsvReader_GetAllRowsFromFileAsync(),
			_root.CsvReader_GetAllRowsFromFileAsync(),
			_root.CsvReader_GetAllRowsFromFileAsync(),
			_root.CsvReader_GetAllRowsFromFileAsync()
		);
	}
}
