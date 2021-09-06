using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Open.Text.CSV.Test
{
	public class CsvReaderTests
	{
		public CsvReaderTests(ITestOutputHelper output)
		{
			Output = output;
		}

		[Theory]
		[InlineData("A,B,C, \"D\" ,E", new[] { "A", "B", "C", "D", "E" })]
		[InlineData("A,B ,C, \"D\" ,E", new[] { "A", "B", "C", "D", "E" })]
		[InlineData("A,B,C,\"D\",,E", new[] { "A", "B", "C", "D", "", "E" })]
		[InlineData("A,B,C,\"D\",,E,", new[] { "A", "B", "C", "D", "", "E" })]
		[InlineData("A,B,C,\"D\",,\"E\"", new[] { "A", "B", "C", "D", "", "E" })]
		[InlineData("A,B,\"\"\"C, c\"\"\",\"D\",,\"E\" ", new[] { "A", "B", "\"C, c\"", "D", "", "E" })]
		[InlineData("A,B,\"\"\"C,\n c\"\"\",\"D\",,\"E\" ", new[] { "A", "B", "\"C,\n c\"", "D", "", "E" })]
		public void SingleRowTests(string input, string[] expected)
		{
			var csv = CsvReader.GetRowsFromText(input);
			Assert.Single(csv);

			var result = csv[0].ToArray();
			Assert.Equal(expected, result);
		}

		[Fact]
		public void MutliRowTests()
		{
			var csvs = new[] {
				CsvReader.GetRowsFromText("A,B,C,\r\nD,E,F"),
				CsvReader.GetRowsFromText("A,B,C\r\nD,E,F"),
				CsvReader.GetRowsFromText("A,B,C,\nD,E,F"),
				CsvReader.GetRowsFromText("A,B,C\nD,E,F"),
				CsvReader.GetRowsFromText("A,B,C\n D,E,F"),
				CsvReader.GetRowsFromText("A,B,C\r\n D,E,F"),
				CsvReader.GetRowsFromText("A,B,\"C\"\r\n D,E,F"),
				CsvReader.GetRowsFromText("A,B,\"C\" \r\n D,E,F"),
				CsvReader.GetRowsFromText("A,B, \"C\" \r\n D,E,F"),
				CsvReader.GetRowsFromText("A,B,C\nD,E,F "),
				CsvReader.GetRowsFromText("A,B,C\nD,E,F\r"),
				CsvReader.GetRowsFromText("A,B,C\nD,E,F\r\n"),
				CsvReader.GetRowsFromText("A,B,C\nD,E,F\n"),
				CsvReader.GetRowsFromText("A,B,C\nD,E,F \n")
			};

			foreach (var csv in csvs)
			{
				Assert.Equal(2, csv.Count);

				var result = csv[0].ToArray();
				Assert.Equal(new[] { "A", "B", "C" }, result);

				result = csv[1].ToArray();
				Assert.Equal(new[] { "D", "E", "F" }, result);
			}
		}

		const string TEST_DATA_CSV = "TestData.csv";

		readonly ITestOutputHelper Output;

		[Fact]
		public void FilePerformanceTest()
		{
			using var fs = new FileInfo(TEST_DATA_CSV).OpenRead();
			using var sr = new StreamReader(fs);
			var rows = new List<string>();
			while (!sr.EndOfStream) rows.Add(sr.ReadLine());
			Assert.NotEmpty(rows);
		}

		[Fact]
		public async Task AsyncFileLinePerformanceTest()
		{
			using var fs = new FileInfo(TEST_DATA_CSV).OpenRead();
			using var sr = new StreamReader(fs);
			var rows = new List<string>();
			while (!sr.EndOfStream) rows.Add(await sr.ReadLineAsync().ConfigureAwait(false));
			Assert.NotEmpty(rows);
		}

		[Fact]
		public async Task PremptiveAsyncFileLinePerformanceTest()
		{
			using var fs = new FileInfo(TEST_DATA_CSV).OpenRead();
			using var sr = new StreamReader(fs);
			var rows = new List<string>();
			await foreach (var line in sr.PreemptiveReadLineAsync())
			{
				rows.Add(line);
			}
			Assert.NotEmpty(rows);
		}

		[Fact]
		public void CsvFilePerformanceTest()
		{
			var rows = CsvReader.GetRowsFromFile(TEST_DATA_CSV);
			Assert.NotEmpty(rows);
		}

		[Fact]
		public void CsvPerformanceTest()
		{
			var source = File.ReadAllText(TEST_DATA_CSV);
			var sw = Stopwatch.StartNew();
			var rows = CsvReader.GetRowsFromText(source);
			Output.WriteLine($"CsvParse Time: {sw.Elapsed.TotalSeconds} seconds");
			Assert.NotEmpty(rows);
		}

		[Fact]
		public async Task SingleBufferTest()
		{
			using var fs = new FileInfo(TEST_DATA_CSV).OpenRead();
			using var sr = new StreamReader(fs);
			var i = 0;
			await foreach (var buffer in sr.SingleBufferReadAsync())
			{
				i++;
			}
			Assert.True(i != 0);
		}

		[Fact]
		public async Task DualBufferTest()
		{
			using var fs = new FileInfo(TEST_DATA_CSV).OpenRead();
			using var sr = new StreamReader(fs);
			var i = 0;
			await foreach (var buffer in sr.DualBufferReadAsync())
			{
				i++;
			}
			Assert.True(i != 0);
		}
	}
}
