using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Open.Text.CSV.Test
{
	public class FileReadMethodTests
	{
		const int BUFFER_SIZE = 4096;

		static int FileRowCount()
		{
			using var sr = new FileInfo(CsvFileReadTests.TEST_DATA_CSV).OpenText();
			var rows = new List<string>();
			while (!sr.EndOfStream) rows.Add(sr.ReadLine());
			return rows.Count;
		}

		[Fact]
		[Benchmark(Baseline = true)]
		public void FilePerformanceTest()
		{
			Assert.NotEqual(0, FileRowCount());
		}


		[Fact]
		[Benchmark]
		public async Task SingleBufferTest()
		{
			using var fs = new FileStream(CsvFileReadTests.TEST_DATA_CSV, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, true);
			using var sr = new StreamReader(fs);
			var i = 0;
			await foreach (var buffer in sr.SingleBufferReadAsync())
			{
				i++;
			}
			Assert.NotEqual(0, i);
		}

		[Fact]
		[Benchmark]
		public async Task DualBufferTest()
		{
			using var fs = new FileStream(CsvFileReadTests.TEST_DATA_CSV, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, true);
			using var sr = new StreamReader(fs);
			var i = 0;
			await foreach (var buffer in sr.DualBufferReadAsync())
			{
				i++;
			}
			Assert.NotEqual(0, i);
		}


		[Fact]
		[Benchmark]
		public async Task AsyncFileLinePerformanceTest()
		{
			using var fs = new FileStream(CsvFileReadTests.TEST_DATA_CSV, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, true);
			using var sr = new StreamReader(fs);
			var rows = new List<string>();
			while (!sr.EndOfStream) rows.Add(await sr.ReadLineAsync().ConfigureAwait(false));
			Assert.NotEmpty(rows);
		}


		[Fact]
		[Benchmark]
		public async Task PremptiveAsyncFileLinePerformanceTest()
		{
			using var fs = new FileStream(CsvFileReadTests.TEST_DATA_CSV, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, true);
			using var sr = new StreamReader(fs);
			var rows = new List<string>();
			await foreach (var line in sr.PreemptiveReadLineAsync())
			{
				rows.Add(line);
			}
			Assert.NotEmpty(rows);
		}
	}
}
