using BenchmarkDotNet.Running;

namespace Open.Text.CSV.Test.Benchmarks
{
	class Program
	{
		static void Main()
		{
			//BenchmarkRunner.Run<CsvFileReadTests>();
			//BenchmarkRunner.Run<FileReadMethodTests>();
			BenchmarkRunner.Run<CsvFileParallelReadTests>();
		}
	}
}
