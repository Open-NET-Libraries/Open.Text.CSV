using System.Linq;
using Xunit;

namespace Open.Text.CSV.Test
{
	public static class CsvReaderTests
	{
		[Theory]
		[InlineData("A,B,C, \"D\" ,E", new[] { "A", "B", "C", "D", "E" })]
		[InlineData("A,B ,C, \"D\" ,E", new[] { "A", "B", "C", "D", "E" })]
		[InlineData("A,B,C,\"D\",,E", new[] { "A", "B", "C", "D", "", "E" })]
		[InlineData("A,B,C,\"D\",,E,", new[] { "A", "B", "C", "D", "", "E" })]
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
				CsvReader.ReadRows("A,B,C,\r\nD,E,F"),
				CsvReader.ReadRows("A,B,C\r\nD,E,F"),
				CsvReader.ReadRows("A,B,C,\nD,E,F"),
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
	}
}
