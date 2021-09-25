using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open.Text.CSV.Test
{
	public class CsvFileParallelReadTests
	{
		readonly CsvFileReadTests _root = new();

		[Benchmark]
		public void GetAllRowsFromFileInParallel()
		{
			Parallel.Invoke(
				_root.GetAllRowsFromFile,
				_root.GetAllRowsFromFile,
				_root.GetAllRowsFromFile,
				_root.GetAllRowsFromFile
			);
		}

		[Benchmark]
		public Task GetAllRowsFromFileAsyncParallel()
		{
			return Task.WhenAll(
				_root.GetAllRowsFromFileAsync(),
				_root.GetAllRowsFromFileAsync(),
				_root.GetAllRowsFromFileAsync(),
				_root.GetAllRowsFromFileAsync()
			);
		}
	}
}
