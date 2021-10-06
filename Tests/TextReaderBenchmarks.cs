using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Xunit;

namespace Open.Text.CSV.Test;

public class TextReaderBenchmarks : FileReadBenchmarkBase
{
	public TextReaderBenchmarks(string testFile = null) : base(testFile) { }


	[Benchmark]
	public int StreamReader_Read()
	{
		var count = 0;
		int next;
		var buffer = new char[ByteBufferSize];
		var span = buffer.AsSpan();
		using var stream = GetStream();
		using var reader = new StreamReader(stream);
		while ((next = reader.Read(span)) is not 0)
			count += next;
		return count;
	}

	[Benchmark]
	public async Task<int> StreamReader_ReadAsync()
	{
		var count = 0;
		int next;
		var buffer = new char[ByteBufferSize];
		var mem = buffer.AsMemory();
		using var stream = GetStream();
		using var reader = new StreamReader(stream);
 		while ((next = await reader.ReadAsync(mem).ConfigureAwait(false)) is not 0)
			count += next;
		return count;
	}

	[Benchmark]
	public async Task<long> PipeReader_EnumerateAsync()
	{
		long count = 0;
		using var stream = GetStream();
		await foreach (var buffer in PipeReader
			.Create(stream, new StreamPipeReaderOptions(bufferSize: ByteBufferSize))
			.EnumerateAsync()
			.DecodeAsync())
		{
			count += buffer.WrittenMemory.Length;
		}

		return count;
	}

	[Benchmark]
	public int StreamReader_ReadLine()
	{
		var count = 0;
		using var stream = GetStream();
		using var reader = new StreamReader(stream);
		while (reader.ReadLine() is not null)
			count++;
		return count;
	}

	[Benchmark]
	public async Task<int> StreamReader_ReadLineAsync()
	{
		var count = 0;
		using var stream = GetStream();
		using var reader = new StreamReader(stream);
		while (await reader.ReadLineAsync().ConfigureAwait(false) is not null)
			count++;
		return count;
	}

	[Benchmark]
	public async Task<int> StreamReader_SingleBufferReadAsync()
	{
		var count = 0;
		using var stream = GetStream();
		using var reader = new StreamReader(stream);
		await foreach (var buffer in reader.SingleBufferReadAsync(ByteBufferSize))
			count += buffer.Length;
		return count;
	}

	[Benchmark]
	public async Task<int> StreamReader_DualBufferReadAsync()
	{
		var count = 0;
		using var stream = GetStream();
		using var reader = new StreamReader(stream);
		await foreach (var buffer in reader.DualBufferReadAsync(ByteBufferSize))
			count += buffer.Length;
		return count;
	}

	[Benchmark]
	public async Task<int> StreamReader_PreemptiveReadLineAsync()
	{
		var count = 0;
		using var stream = GetStream();
		using var reader = new StreamReader(stream);
		await foreach (var buffer in reader.PreemptiveReadLineAsync())
			count++;
		return count;
	}
}

public class FileReadMethodTests : TextReaderBenchmarks
{
	static readonly int ExpectedCharacterCount = GetExpectedCharacterCount();

	static int GetExpectedCharacterCount()
	{
		var count = 0;
		int next;
		var buffer = new char[4096];
		using var sr = new StreamReader(TEST_FILE);
		while ((next = sr.Read(buffer)) is not 0)
		{
			count += next;
		}

		return count;
	}

	[Fact]
	public async Task StreamReader_SingleBufferReadAsyncTest()
		=> Assert.Equal(
			ExpectedCharacterCount,
			await StreamReader_SingleBufferReadAsync());

	[Fact]
	public async Task StreamReader_DualBufferReadAsyncTest()
		=> Assert.Equal(
			ExpectedCharacterCount,
			await StreamReader_DualBufferReadAsync());

	[Fact]
	public async Task StreamReader_PreemptiveReadLineAsyncTest()
		 => Assert.Equal(
			CsvFileReadTests.ExpectedLineCount,
			await StreamReader_PreemptiveReadLineAsync());
}
