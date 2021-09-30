using BenchmarkDotNet.Attributes;
using Open.ChannelExtensions;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Open.Text.CSV.Test
{
	[MemoryDiagnoser]
	public class CsvFileReadTests
	{
		static readonly int ExpectedLineCount = 1000001;// FileReadMethodTests.FileRowCount();
		const int BENCH_ROWS = 10000; // ExpectedLineCount;

		[Params(BENCH_ROWS)]
		public int MaxRows { get; set; } = ExpectedLineCount;

		const int ROW_BUFFER = -1;

		internal const string TEST_DATA_CSV = "TestData.csv";

		static readonly IList<IList<string>> Data = CsvReader.GetAllRowsFromFile(TEST_DATA_CSV);

		[Benchmark(Baseline = true)]
		public List<IList<string>> GetAllRowsFromFile()
		{
			int count = 0;
			var rows = new List<IList<string>>();
			using var sr = new FileInfo(TEST_DATA_CSV).OpenText();
			foreach (var row in CsvReader.ReadRows(sr))
			{
				rows.Add(row);
				if (++count == MaxRows) break;
			}
			return rows;
		}

		[Benchmark(Baseline = true)]
		public int GetAllRowsFromFile2()
		{
			int count = 0;
			using var sr = new FileInfo(TEST_DATA_CSV).OpenText();
			using var csv = new CsvReader2(16, sr);
			foreach (var row in csv.ReadRows())
			{
				row.Dispose();
				if (++count == MaxRows) break;
			}
			return count;
		}

		[Fact]

		public void GetAllRowsFromFileTest()
			=> Assert.Equal(ExpectedLineCount, GetAllRowsFromFile().Count);

		[Fact]

		public void GetAllRowsFromFileTest2()
			=> Assert.Equal(ExpectedLineCount, GetAllRowsFromFile2());

		[Benchmark]
		public List<List<string>> GetAllRowsFromFile_Sylvan()
		{
			var count = 0;
			var rows = new List<List<string>>();
			using var reader = Sylvan.Data.Csv.CsvDataReader.Create(TEST_DATA_CSV, new Sylvan.Data.Csv.CsvDataReaderOptions
			{
				HasHeaders = false
			});
			var fields = reader.FieldCount;
			while (reader.Read())
			{
				var row = new List<string>(fields);
				for (var i = 0; i < fields; i++)
					row.Add(reader.GetString(i));
				rows.Add(row);
				if (++count == MaxRows) break;
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

		[Benchmark]
		public List<Record> GetAllRowsFromFile_Sylvan_StrongType()
		{
			var count = 0;
			var rows = new List<Record>();
			using var reader = Sylvan.Data.Csv.CsvDataReader.Create(TEST_DATA_CSV);
			var fields = reader.FieldCount;
			while (reader.Read())
			{
				rows.Add(new()
				{
					GlobalRank = reader.GetInt32(0),
					TldRank = reader.GetInt32(1),
					Domain = reader.GetString(2),
					TLD = reader.GetString(3),
					RefSubNets = reader.GetInt32(4),
					RefIPs = reader.GetInt32(5),
					IDN_Domain = reader.GetString(6),
					IDN_TLD = reader.GetString(7),
					PrevGlobalRank = reader.GetInt32(8),
					PrevTldRank = reader.GetInt32(9),
					PrevRefSubNets = reader.GetInt32(10),
					PrevRefIPs = reader.GetInt32(11)
				});
				if (++count == MaxRows) break;
			}
			return rows;
		}

		[Fact]
		public void GetAllRowsFromFileTest_Sylvan_StrongType()
			=> Assert.Equal(ExpectedLineCount-1, GetAllRowsFromFile_Sylvan_StrongType().Count);

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

		//[Benchmark]
		public async Task<IList<IList<string>>> GetAllRowsFromFileAsync()
		{
			var count = 0;
			var list = new List<IList<string>>();
			IList<string> row;

			var fs = new FileStream(TEST_DATA_CSV, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
			var sr = new StreamReader(fs);
			var csv = new CsvReader(sr);
			while ((row = await csv.ReadNextRowAsync().ConfigureAwait(false)) is not null)
			{
				list.Add(row);
				if (++count == MaxRows) break;
			}
			return list;
		}

		[Fact]
		public async Task GetAllRowsFromFileAsyncTest()
		{
			var rows = await GetAllRowsFromFileAsync();
			Assert.Equal(ExpectedLineCount, rows.Count);
			Assert.Equal(Data, rows);
		}

		//[Benchmark]
		public async Task<IList<IList<string>>> ReadRowsToChannel()
		{
			var rows = new List<IList<string>>();
			using var canceller = new CancellationTokenSource();
			await CsvReader.ReadRowsToChannel(TEST_DATA_CSV, ROW_BUFFER).ReadUntilCancelled(canceller.Token, (row, i) =>
			{
				rows.Add(row);
				if (i == MaxRows) canceller.Cancel();
			});
			return rows;
		}

		[Fact]
		public async Task ReadRowsToChannelTest()
		{
			var rows = await ReadRowsToChannel();
			Assert.Equal(ExpectedLineCount, rows.Count);
			Assert.Equal(Data, rows);
		}

		//[Benchmark]
		public IList<IList<string>> ReadRowsBuffered()
		{
			int count = 0;
			var rows = new List<IList<string>>();
			using var fs = new FileInfo(TEST_DATA_CSV).OpenRead();
			using var sr = new StreamReader(fs);
			foreach (var row in CsvReader.ReadRowsBuffered(sr, ROW_BUFFER))
			{
				rows.Add(row);
				if (++count == MaxRows) break;
			}
			return rows;
		}

		[Fact]
		public void ReadRowsBufferedTest()
		{
			var rows = ReadRowsBuffered();
			Assert.Equal(ExpectedLineCount, rows.Count);
			Assert.Equal(Data, rows);
		}

		//[Benchmark]
		public async Task<IList<IList<string>>> ReadRowsBufferedAsync()
		{
			var count = 0;
			var rows = new List<IList<string>>();
			using var fs = new FileStream(TEST_DATA_CSV, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
			using var sr = new StreamReader(fs);
			await foreach (var row in CsvReader.ReadRowsBufferedAsync(sr, ROW_BUFFER))
			{
				rows.Add(row);
				if (++count == MaxRows) break;
			}
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
