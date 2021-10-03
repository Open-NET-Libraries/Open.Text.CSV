using BenchmarkDotNet.Attributes;
using Open.ChannelExtensions;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Open.Text.CSV.Test;

[MemoryDiagnoser]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Some benchmarks are disabled (commented out).")]

public class CsvFileReadTests : TextReaderBenchmarkBase
{
	public static readonly int ExpectedLineCount = 1000001;// FileReadMethodTests.FileRowCount();

	const int ROW_BUFFER = -1;

	static readonly IList<IList<string>> Data = CsvReader.GetAllRowsFromFile(TEST_FILE);

	[Benchmark(Baseline = true)]
	public int CsvReader_GetAllRowsFromFile()
	{
		int count = 0;
		foreach (var _ in CsvReader.ReadRows(Reader))
			count++;
		return count;
	}

	[Benchmark]
	public int CsvMemoryReader_GetAllRowsFromFile()
	{
		int count = 0;
		using var csv = new CsvMemoryReader(Reader);
		foreach (var row in csv.ReadRows())
		{
			row.Dispose();
			count++;
		}

		return count;
	}

	[Fact]

	public void CsvReader_GetAllRowsFromFileTest() => Run(() =>
		Assert.Equal(ExpectedLineCount, CsvReader_GetAllRowsFromFile()));

	[Fact]

	public void CsvMemoryReader_GetAllRowsFromFileTest() => Run(() =>
	{
		var rows = new List<List<string>>();
		using var csv = new CsvMemoryReader(Reader);
		foreach (var row in csv.ReadRows())
		{
			var list = new List<string>(csv.MaxFields);
			foreach (var e in row.Memory.Span)
				list.Add(e);
			row.Dispose();
			rows.Add(list);
		}

		Assert.Equal(ExpectedLineCount, rows.Count);
		Assert.Equal(Data, rows);
	});

	//[Benchmark]
	public List<List<string>> Sylvan_GetAllRowsFromFile()
	{
		var count = 0;
		var rows = new List<List<string>>();
		using var reader = Sylvan.Data.Csv.CsvDataReader.Create(
			TEST_FILE,
			new Sylvan.Data.Csv.CsvDataReaderOptions
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
			count++;
		}

		return rows;
	}

	[Fact]
	public void Sylvan_GetAllRowsFromFileTest() => Run(() =>
	{
		var rows = Sylvan_GetAllRowsFromFile();
		Assert.Equal(ExpectedLineCount, rows.Count);
		Assert.Equal(Data, rows);
	});

	//[Benchmark]
	public List<Record> Sylvan_GetAllRowsFromFile_StrongType()
	{
		var count = 0;
		var rows = new List<Record>();
		using var reader = Sylvan.Data.Csv.CsvDataReader.Create(TEST_FILE);
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
			count++;
		}

		return rows;
	}

	[Fact]
	public void Sylvan_GetAllRowsFromFileTest_StrongType() => Run(() =>
	   Assert.Equal(ExpectedLineCount - 1, Sylvan_GetAllRowsFromFile_StrongType().Count));

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
	public async Task<IList<IList<string>>> CsvReader_GetAllRowsFromFileAsync()
	{
		var list = new List<IList<string>>();
		IList<string> row;

		var csv = new CsvReader(Reader);
		while ((row = await csv.ReadNextRowAsync().ConfigureAwait(false)) is not null)
			list.Add(row);
		return list;
	}

	[Fact]
	public Task CsvReader_GetAllRowsFromFileAsyncTest() => RunAsync(async () =>
	{
		var rows = await CsvReader_GetAllRowsFromFileAsync();
		Assert.Equal(ExpectedLineCount, rows.Count);
		Assert.Equal(Data, rows);
	});


	//[Benchmark]
	public async Task<IList<IList<string>>> CsvReader_ReadRowsToChannel()
	{
		var rows = new List<IList<string>>();
		_ = await CsvReader.ReadRowsToChannel(TEST_FILE, ROW_BUFFER)
			.ReadAll(rows.Add);
		return rows;
	}

	[Fact]
	public Task CsvReader_ReadRowsToChannelTest() => RunAsync(async () =>
	{
		var rows = await CsvReader_ReadRowsToChannel();
		Assert.Equal(ExpectedLineCount, rows.Count);
		Assert.Equal(Data, rows);
	});

	//[Benchmark]
	public IList<IList<string>> CsvReader_ReadRowsBuffered()
	{
		var rows = new List<IList<string>>();
		foreach (var row in CsvReader.ReadRowsBuffered(Reader, ROW_BUFFER))
			rows.Add(row);
		return rows;
	}

	[Fact]
	public void CsvReader_ReadRowsBufferedTest() => Run(() =>
	{
		var rows = CsvReader_ReadRowsBuffered();
		Assert.Equal(ExpectedLineCount, rows.Count);
		Assert.Equal(Data, rows);
	});

	//[Benchmark]
	public async Task<IList<IList<string>>> CsvReader_ReadRowsBufferedAsync()
	{
		var rows = new List<IList<string>>();
		await foreach (var row in CsvReader.ReadRowsBufferedAsync(Reader, ROW_BUFFER))
			rows.Add(row);
		return rows;
	}

	[Fact]
	public Task CsvReader_ReadRowsBufferedAsyncTest() => RunAsync(async () =>
	{
		var rows = await CsvReader_ReadRowsBufferedAsync();
		Assert.Equal(ExpectedLineCount, rows.Count);
		Assert.Equal(Data, rows);
	});

	[Benchmark]
	public async Task<int> CsvReader_PipeRowsAsync()
	{
		int count = 0;
		await foreach (var row in CsvReader.PipeRowsAsync(Stream))
			count++;
		return count;
	}

	[Fact]
	public Task CsvReader_PipeRowsAsyncTest() => RunAsync(async () =>
	{
		var count = await CsvReader_PipeRowsAsync();
		Assert.Equal(ExpectedLineCount, count);
	});
}
