using System.Collections.Generic;
using Xunit;

namespace Open.Text.CSV.Test
{
	public static class CsvRowBuilderTests
	{
		[Fact]
		public static void BasicRowBuildTest()
		{
			var rows = new List<List<string>>(3);
			var rb = new CsvRowBuilder(rows.Add);
			List<string> row;
			rb.AddNextChars("\"A\", B, C,\r\n", out _);
			Assert.Single(rows);
			row = rows[0];
			Assert.Equal(3, row.Count);
			Assert.Equal("C", row[2]);

			AddRow("D, \"E\", F\n");
			Assert.Equal(2, rows.Count);
			row = rows[1];
			Assert.Equal(3, row.Count);
			Assert.Equal("F", row[2]);

			AddRow("G, H, I");
			rb.AddChar(-1);
			Assert.Equal(3, rows.Count);
			row = rows[2];
			Assert.Equal(3, row.Count);
			Assert.Equal("I", row[2]);

			AddRow("J, K, \"L\nX\"");
			rb.AddChar(-1);
			Assert.Equal(4, rows.Count);
			row = rows[3];
			Assert.Equal(3, row.Count);
			Assert.Equal("L\nX", row[2]);


			void AddRow(string row)
			{
				foreach (var c in row) rb.AddChar(c);
			}

		}
	}
}
