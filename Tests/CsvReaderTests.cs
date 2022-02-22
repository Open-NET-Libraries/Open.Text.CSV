using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace Open.Text.CSV.Test;

public static class CsvReaderTests
{
	[Theory]
	[InlineData("A,B,C, \"D\" ,E", new[] { "A", "B", "C", "D", "E" })]
	[InlineData("A,B ,C, \"D\" ,E", new[] { "A", "B", "C", "D", "E" })]
	[InlineData("A,B,C,\"D\",,E", new[] { "A", "B", "C", "D", "", "E" })]
	[InlineData("A,B,C,\"D\",,E,", new[] { "A", "B", "C", "D", "", "E", "" })]
	[InlineData("A,B,C,\"D\",,\"E\"", new[] { "A", "B", "C", "D", "", "E" })]
	[InlineData("A,B,\"\"\"C, c\"\"\",\"D\",,\"E\" ", new[] { "A", "B", "\"C, c\"", "D", "", "E" })]
	[InlineData("A,B,\"\"\"C,\n c\"\"\",\"D\",,\"E\" ", new[] { "A", "B", "\"C,\n c\"", "D", "", "E" })]
	public static void SingleRowTests(string input, string[] expected)
	{
		var csv = CsvReader.ReadRows(input).ToArray();
		Assert.Single(csv);

		var result = csv[0].ToArray();
		Assert.Equal(expected, result);
	}

	[Fact]
	public static void MutliRowTests()
	{
		var csvs = new[] {
				CsvReader.ReadRows("A,B,C\r\nD,E,F"),
				CsvReader.ReadRows("A,B,C\nD,E,F"),
				CsvReader.ReadRows("A,B,C\n D,E,F"),
				CsvReader.ReadRows("A,B,C\r\n D,E,F"),
				CsvReader.ReadRows("A,B,\"C\"\r\n D,E,F"),
				CsvReader.ReadRows("A,B,\"C\" \r\n D,E,F"),
				CsvReader.ReadRows("A,B, \"C\" \r\n D,E,F"),
				CsvReader.ReadRows("A,B,C\nD,E,F "),
				CsvReader.ReadRows("A,B,C\nD,E,F\r"),
				CsvReader.ReadRows("A,B,C\nD,E,F\r\n"),
				CsvReader.ReadRows("A,B,C\nD,E,F\n"),
				CsvReader.ReadRows("A,B,C\nD,E,F \n")
			};

		foreach (var csv in csvs.Select(c => c.ToArray()))
		{
			Assert.Equal(2, csv.Length);

			var result = csv[0].ToArray();
			Assert.Equal(new[] { "A", "B", "C" }, result);

			result = csv[1].ToArray();
			Assert.Equal(new[] { "D", "E", "F" }, result);
		}
	}

	const string TEST_FILE = "PackageAssets.csv";

	static List<IList<string>> StringSplit()
	{
		var rows = new List<IList<string>>();

		using var reader = File.OpenText(TEST_FILE);
		string line;
		while ((line = reader.ReadLine()) != null)
			rows.Add(line.Split(','));

		return rows;
	}

	static List<IList<string>> CsvRead()
	{
		var rows = new List<IList<string>>();

		using var reader = File.OpenText(TEST_FILE);
		using var csv = new CsvReader(reader);
		IList<string> line;
		while ((line = csv.ReadNextRow()) != null)
			rows.Add(line.ToArray());

		return rows;
	}

	[Fact]
	public static void LineParityTest()
	{
		var rows = new List<IList<string>>();

		using var reader = File.OpenText(TEST_FILE);
		string line;
		while ((line = reader.ReadLine()) != null)
		{
			var s = line.Split(',');
			var csv = CsvReader.ReadRows(line).ToArray();
			Assert.Single(csv);
			var r = csv[0].ToArray();
			if (r.Length != s.Length)
			{
				Debug.WriteLine("Found One.");
				var commas = line.Count(c => c == ',');
				var retest = CsvReader.ReadRows(line).ToArray()[0];
				Debug.Assert(retest.Count == r.Length);
				_ = CsvReader.ReadRows(line).ToArray()[0];
			}

			Assert.Equal(s.Length, r.Length);
			Assert.Equal(s, r);
		}
	}

	[Fact]
	public static void ParityTest()
	{
		var splitRecords = StringSplit();
		var readRecords = CsvRead();
		Assert.Equal(splitRecords.Count, readRecords.Count);

		var len = splitRecords.Count;
		for (int i = 0; i < len; i++)
		{
			var s = splitRecords[i];
			var r = readRecords[i];
#if DEBUG
			if (r.Count != s.Count)
			{
				using var reader = File.OpenText(TEST_FILE);
				using var csv = new CsvReader(reader);
				for (var x = 0; x < i; x++) _ = csv.ReadNextRow();
				Debugger.Break();
				_ = csv.ReadNextRow();
				break;
			}
#endif
			Assert.Equal(s.Count, r.Count);
			Assert.Equal(s, r);
		}
	}
}
