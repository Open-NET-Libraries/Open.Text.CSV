using BenchmarkDotNet.Attributes;
using Open.ChannelExtensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Open.Text.CSV.Test
{
	//[MemoryDiagnoser]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Benchmarking")]
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

		[Benchmark]
		public List<List<string>> GetAllRowsFromFile_Sylvan()
		{
			using var reader = Sylvan.Data.Csv.CsvDataReader.Create(TEST_DATA_CSV);
			var fields = reader.FieldCount;
			var rows = new List<List<string>>();
			while(reader.Read())
			{
				var row = new List<string>();
				for (var i = 0; i < fields; i++)
					row.Add(reader.GetString(i));
				rows.Add(row);
			}
			return rows;

		}

		[Benchmark]
		public async Task<IList<IList<string>>> GetAllRowsFromFileAsync()
			=> await CsvReader.GetAllRowsFromFileAsync(TEST_DATA_CSV);

		[Fact]
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
