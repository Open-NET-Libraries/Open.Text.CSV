using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Open.Text.CSV.Test;

public static class CsvWriterTests
{
	[Fact]
	public static void WriteValueTests()
	{
		var sb = new StringBuilder();
		using var sw = new StringWriter(sb);

		CsvWriter.WriteValue(sw, "Hello");
		Assert.Equal("Hello,", sb.ToString());
		sb.Clear();
		CsvWriter.WriteValue(sw, "Hello, World");
		Assert.Equal("\"Hello, World\",", sb.ToString());
		sb.Clear();
		CsvWriter.WriteValue(sw, "Hello\"World");
		Assert.Equal("\"Hello\"\"World\",", sb.ToString());
		sb.Clear();
		CsvWriter.WriteValue(sw, "Hello\r\nWorld");
		Assert.Equal("\"Hello\r\nWorld\",", sb.ToString());
		sb.Clear();
		CsvWriter.WriteValue(sw, "Hello\rWorld");
		Assert.Equal("\"Hello\rWorld\",", sb.ToString());
		sb.Clear();

		CsvWriter.WriteValue(sw, "Hello", true, true);
		Assert.Equal("\"Hello\"", sb.ToString());
		sb.Clear();
		CsvWriter.WriteValue(sw, "Hello, World", true, true);
		Assert.Equal("\"Hello, World\"", sb.ToString());
		sb.Clear();
		CsvWriter.WriteValue(sw, "Hello\"World", true, true);
		Assert.Equal("\"Hello\"\"World\"", sb.ToString());
		sb.Clear();
		CsvWriter.WriteValue(sw, "Hello\r\nWorld", true, true);
		Assert.Equal("\"Hello\r\nWorld\"", sb.ToString());
		sb.Clear();
		CsvWriter.WriteValue(sw, "Hello\rWorld", true, true);
		Assert.Equal("\"Hello\rWorld\"", sb.ToString());
		sb.Clear();
	}

	[Fact]
	public static void WriteRowTests()
	{
		var sb = new StringBuilder();
		using var sw = new StringWriter(sb);
		CsvWriter.WriteRow(sw, new[] { "Hello", "World" });
		Assert.Equal("Hello,World,\r\n", sb.ToString());
		sb.Clear();
		CsvWriter.WriteRow(sw, new[] { "Hello", "World" }, true, true);
		Assert.Equal("\"Hello\",\"World\"\r\n", sb.ToString());
	}

	[Fact]
	public static async Task WriteValueTestsAsync()
	{
		var sb = new StringBuilder();
		using var sw = new StringWriter(sb);

		await CsvWriter.WriteValueAsync(sw, "Hello");
		Assert.Equal("Hello,", sb.ToString());
		sb.Clear();
		await CsvWriter.WriteValueAsync(sw, "Hello, World");
		Assert.Equal("\"Hello, World\",", sb.ToString());
		sb.Clear();
		await CsvWriter.WriteValueAsync(sw, "Hello\"World");
		Assert.Equal("\"Hello\"\"World\",", sb.ToString());
		sb.Clear();
		await CsvWriter.WriteValueAsync(sw, "Hello\r\nWorld");
		Assert.Equal("\"Hello\r\nWorld\",", sb.ToString());
		sb.Clear();
		await CsvWriter.WriteValueAsync(sw, "Hello\rWorld");
		Assert.Equal("\"Hello\rWorld\",", sb.ToString());
		sb.Clear();

		await CsvWriter.WriteValueAsync(sw, "Hello", true, true);
		Assert.Equal("\"Hello\"", sb.ToString());
		sb.Clear();
		await CsvWriter.WriteValueAsync(sw, "Hello, World", true, true);
		Assert.Equal("\"Hello, World\"", sb.ToString());
		sb.Clear();
		await CsvWriter.WriteValueAsync(sw, "Hello\"World", true, true);
		Assert.Equal("\"Hello\"\"World\"", sb.ToString());
		sb.Clear();
		await CsvWriter.WriteValueAsync(sw, "Hello\r\nWorld", true, true);
		Assert.Equal("\"Hello\r\nWorld\"", sb.ToString());
		sb.Clear();
		await CsvWriter.WriteValueAsync(sw, "Hello\rWorld", true, true);
		Assert.Equal("\"Hello\rWorld\"", sb.ToString());
		sb.Clear();
	}

	[Fact]
	public static async Task WriteRowTestsAsync()
	{
		var sb = new StringBuilder();
		using var sw = new StringWriter(sb);
		await CsvWriter.WriteRowAsync(sw, new[] { "Hello", "World" });
		Assert.Equal("Hello,World,\r\n", sb.ToString());
		sb.Clear();
		await CsvWriter.WriteRowAsync(sw, new[] { "Hello", "World" }, true, true);
		Assert.Equal("\"Hello\",\"World\"\r\n", sb.ToString());
	}
}
