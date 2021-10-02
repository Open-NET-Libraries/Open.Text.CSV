using BenchmarkDotNet.Running;

namespace Open.Text.CSV.Test.Benchmarks;

class Program
{
	static void Main()
	{
		//BenchmarkRunner.Run<StringBuilderVsMemoryOwner>();
		//BenchmarkRunner.Run<CsvFileReadTests>();
		BenchmarkRunner.Run<FileReadMethodBenchmarks>();
		//BenchmarkRunner.Run<CsvFileParallelReadTests>();
	}
}
