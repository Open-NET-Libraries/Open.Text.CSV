using Xunit;

namespace Open.Text.CSV.Test
{
	public static class CsvRowBuilderTests
	{
		[Fact]
		public static void BasicRowBuildTest()
		{
			var rb = new CsvRowBuilder();
			rb.Add("\"A\", B, C,\r\n", out _);
			var row = rb.LatestCompleteRow;
			Assert.Equal(3, row.Count);
			Assert.Equal("C", row[2]);

			rb.Add("D, \"E\", F\n", out _);
			row = rb.LatestCompleteRow;
			Assert.Equal(3, row.Count);
			Assert.Equal("F", row[2]);

			rb.Add("G, H, I", out _);
			rb.EndRow();
			row = rb.LatestCompleteRow;
			Assert.Equal(3, row.Count);
			Assert.Equal("I", row[2]);

			rb.Add("J, K, \"L\nX\"", out _);
			rb.EndRow();
			row = rb.LatestCompleteRow;
			Assert.Equal(3, row.Count);
			Assert.Equal("L\nX", row[2]);

			rb.Add("L, M, N,\nO,P,Q", out var remaining);
			row = rb.LatestCompleteRow;
			Assert.Equal(3, row.Count);
			Assert.Equal("N", row[2]);
			Assert.Equal(5, remaining.Length);


		}


		[Fact]
		public static void BasicRowBuildTest2()
		{
			var rb = new CsvRowBuilder2(3);
			rb.Add("\"A\", B, C,\r\n", out _);
			var row = rb.LatestCompleteRow.Memory.Span;
			Assert.Equal(3, row.Length);
			Assert.Equal("C", row[2]);

			rb.Add("D, \"E\", F\n", out _);
			row = rb.LatestCompleteRow.Memory.Span;
			Assert.Equal(3, row.Length);
			Assert.Equal("F", row[2]);

			rb.Add("G, H, I", out _);
			rb.EndRow();
			row = rb.LatestCompleteRow.Memory.Span;
			Assert.Equal(3, row.Length);
			Assert.Equal("I", row[2]);

			rb.Add("J, K, \"L\nX\"", out _);
			rb.EndRow();
			row = rb.LatestCompleteRow.Memory.Span;
			Assert.Equal(3, row.Length);
			Assert.Equal("L\nX", row[2]);

			rb.Add("L, M, N,\nO,P,Q", out var remaining);
			row = rb.LatestCompleteRow.Memory.Span;
			Assert.Equal(3, row.Length);
			Assert.Equal("N", row[2]);
			Assert.Equal(5, remaining.Length);


		}
	}
}
