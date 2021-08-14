using System.Linq;
using Xunit;

namespace Open.Text.CSV.Test
{
	public class CsvReaderTests
	{
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
	}
}
