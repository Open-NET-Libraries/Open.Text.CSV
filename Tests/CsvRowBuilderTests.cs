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
			rb.Add("\"A\", B, C,\r\n", out _);
			Assert.Single(rows);
			row = rows[0];
			Assert.Equal(3, row.Count);
			Assert.Equal("C", row[2]);

			rb.Add("D, \"E\", F\n", out _);
			Assert.Equal(2, rows.Count);
			row = rows[1];
			Assert.Equal(3, row.Count);
			Assert.Equal("F", row[2]);

			rb.Add("G, H, I", out _);
			rb.EndRow();
			Assert.Equal(3, rows.Count);
			row = rows[2];
			Assert.Equal(3, row.Count);
			Assert.Equal("I", row[2]);

			rb.Add("J, K, \"L\nX\"", out _);
			rb.EndRow();
			Assert.Equal(4, rows.Count);
			row = rows[3];
			Assert.Equal(3, row.Count);
			Assert.Equal("L\nX", row[2]);

			rb.Add("L, M, N,\nO,P,Q", out var remaining);
			Assert.Equal(5, rows.Count);
			row = rows[4];
			Assert.Equal(3, row.Count);
			Assert.Equal("N", row[2]);
			Assert.Equal(5, remaining.Length);


		}
	}
}
