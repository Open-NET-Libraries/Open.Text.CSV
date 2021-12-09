using System.Buffers;
using System.Collections.Generic;
using Xunit;

namespace Open.Text.CSV.Test;

public static class CsvRowBuilderTests
{
	[Fact]
	public static void BasicRowBuildTest()
	{
		var rb = new ListCsvRowBuilder();
		rb.Add("\"A\", B, C,\r\n", out _, out IList<string> row);
		Assert.Equal(4, row.Count);
		Assert.Equal("C", row[2]);

		rb.Add("D, \"E\", F\n", out _, out row);
		Assert.Equal(3, row.Count);
		Assert.Equal("F", row[2]);

		rb.Add("G, H, I", out _, out _);
		rb.EndRow(out row);
		Assert.Equal(3, row.Count);
		Assert.Equal("I", row[2]);

		rb.Add("J, K, \"L\nX\"", out _, out _);
		rb.EndRow(out row);
		Assert.Equal(3, row.Count);
		Assert.Equal("L\nX", row[2]);

		rb.Add("L, M, N,\nO,P,Q", out var remaining, out row);
		Assert.Equal(4, row.Count);
		Assert.Equal("N", row[2]);
		Assert.Equal(5, remaining.Length);
	}


	[Fact]
	public static void BasicRowBuildTest2()
	{
		var rb = new MemoryCsvRowBuilder();
		rb.Add("\"A\", B, C,\r\n", out _, out IMemoryOwner<string> mem);
		var row = mem.Memory.Span;
		Assert.Equal(4, row.Length);
		Assert.Equal("C", row[2]);

		rb.Add("D, \"E\", F\n", out _, out mem);
		row = mem.Memory.Span;
		Assert.Equal(3, row.Length);
		Assert.Equal("F", row[2]);

		rb.Add("G, H, I", out _, out _);
		rb.EndRow(out mem);
		row = mem.Memory.Span;
		Assert.Equal(3, row.Length);
		Assert.Equal("I", row[2]);

		rb.Add("J, K, \"L\nX\"", out _, out _);
		rb.EndRow(out mem);
		row = mem.Memory.Span;
		Assert.Equal(3, row.Length);
		Assert.Equal("L\nX", row[2]);

		rb.Add("L, M, N,\nO,P,Q", out var remaining, out mem);
		row = mem.Memory.Span;
		Assert.Equal(4, row.Length);
		Assert.Equal("N", row[2]);
		Assert.Equal(5, remaining.Length);
	}
}
