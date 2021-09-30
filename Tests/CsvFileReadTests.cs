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

		[Fact]
		[Benchmark]
		public List<List<string>> GetAllRowsFromFile_Sylvan()
		{
			using var reader = Sylvan.Data.Csv.CsvDataReader.Create(TEST_DATA_CSV, new Sylvan.Data.Csv.CsvDataReaderOptions
			{
				HasHeaders = false
			});
			var fields = reader.FieldCount;
			var rows = new List<List<string>>();
			while (reader.Read())
			{
				var row = new List<string>(fields);
				for (var i = 0; i < fields; i++)
					row.Add(reader.GetString(i));
				rows.Add(row);
			}
			return rows;
		}

		[Fact]
		public void GetAllRowsFromFileTest_Sylvan()
		{
			var rows = GetAllRowsFromFile_Sylvan();
			Assert.Equal(ExpectedLineCount, rows.Count);
			Assert.Equal(Data, rows);
		}

		[Fact]
		[Benchmark]
		public List<Record> GetAllRowsFromFile_Sylvan_StrongType()
		{
			using var reader = Sylvan.Data.Csv.CsvDataReader.Create(TEST_DATA_CSV);
			var fields = reader.FieldCount;
			var rows = new List<Record>();
			while (reader.Read())
			{
				var record = new Record();
				record.GlobalRank = reader.GetInt32(0);
				record.TldRank = reader.GetInt32(1);
				record.Domain = reader.GetString(2);
				record.TLD = reader.GetString(3);
				record.RefSubNets = reader.GetInt32(4);
				record.RefIPs = reader.GetInt32(5);
				record.IDN_Domain = reader.GetString(6);
				record.IDN_TLD = reader.GetString(7);
				record.PrevGlobalRank = reader.GetInt32(8);
				record.PrevTldRank = reader.GetInt32(9);
				record.PrevRefSubNets = reader.GetInt32(10);
				record.PrevRefIPs = reader.GetInt32(11);
				rows.Add(record);
			}
			return rows;
		}

		public class Record
		{
			public int GlobalRank { get; set; }
			public int TldRank { get; set; }
			public string Domain { get; set; }
			public string TLD { get; set; }
			public int RefSubNets { get; set; }
			public int RefIPs { get; set; }
			public string IDN_Domain { get; set; }
			public string IDN_TLD { get; set; }
			public int PrevGlobalRank { get; set; }
			public int PrevTldRank { get; set; }
			public int PrevRefSubNets { get; set; }
			public int PrevRefIPs { get; set; }
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
