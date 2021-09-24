using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Open.ChannelExtensions;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Open.Text.CSV.Test
{
	[MemoryDiagnoser]
	public class CsvFileReadTests
	{
		static readonly int ExpectedLineCount = 1000001;// FileReadMethodTests.FileRowCount();

		const int ROW_BUFFER = -1;

		internal const string TEST_DATA_CSV = "TestData.csv";

		static readonly IList<IList<string>> Data = CsvReader.GetAllRowsFromFile(TEST_DATA_CSV);

		[Fact]
		[Benchmark(Baseline = true)]
		public void GetAllRowsFromFile()
		{
			var rows = CsvReader.GetAllRowsFromFile(TEST_DATA_CSV);
			Assert.Equal(ExpectedLineCount, rows.Count);
		}

		[Fact]
		[Benchmark]
		public void GetAllRowsFromFileBuffered()
		{
			var rows = CsvReader.GetAllRowsFromFileBuffered(TEST_DATA_CSV);
			Assert.Equal(ExpectedLineCount, rows.Count);
		}

		[Benchmark]
		public async Task<IList<IList<string>>> GetAllRowsFromFileAsync()
		{
			return await CsvReader.GetAllRowsFromFileAsync(TEST_DATA_CSV);
		}

		[Fact]
		[Benchmark]
		public async Task GetAllRowsFromFileAsyncTest()
		{
			var rows = await GetAllRowsFromFileAsync();
			Assert.Equal(ExpectedLineCount, rows.Count);
			Assert.Equal(Data, rows);
		}

		[Benchmark]
		public async Task<IList<IList<string>>> ReadRowsToChannel()
		{
			var rows = new List<IList<string>>();
			await CsvReader.ReadRowsToChannel(TEST_DATA_CSV, ROW_BUFFER).ReadAll(rows.Add);
			return rows;
		}

		[Fact]
		public async Task ReadRowsToChannelTest()
		{
			var rows = await ReadRowsToChannel();
			Assert.Equal(ExpectedLineCount, rows.Count);
			Assert.Equal(Data, rows);
		}

		[Benchmark]
		public IList<IList<string>> ReadRowsBuffered()
		{
			using var fs = new FileInfo(TEST_DATA_CSV).OpenRead();
			using var sr = new StreamReader(fs);
			return CsvReader.ReadRowsBuffered(sr, ROW_BUFFER).ToList();
		}

		[Fact]
		public void ReadRowsBufferedTest()
		{
			var rows = ReadRowsBuffered();
			Assert.Equal(ExpectedLineCount, rows.Count);
			Assert.Equal(Data, rows);
		}

		[Benchmark]
		public async Task<IList<IList<string>>> ReadRowsBufferedAsync()
		{
			using var fs = new FileStream(TEST_DATA_CSV, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
			using var sr = new StreamReader(fs);
			var rows = new List<IList<string>>();
			await foreach (var row in CsvReader.ReadRowsBufferedAsync(sr, ROW_BUFFER)) rows.Add(row);
			return rows;
		}

		[Fact]
		public async Task ReadRowsBufferedAsyncTest()
		{
			var rows = await ReadRowsBufferedAsync();
			Assert.Equal(ExpectedLineCount, rows.Count);
			Assert.Equal(Data, rows);
		}
	}
}
