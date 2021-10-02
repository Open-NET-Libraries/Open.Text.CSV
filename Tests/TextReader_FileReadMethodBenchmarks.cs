using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Open.Text.CSV.Test;

public class TextReader_FileReadMethodBenchmarks
{
	protected const string TEST_FILE = CsvFileReadTests.TEST_DATA_CSV;

	[Params(1024, 4096, 16384)]
	public int BufferSize { get; set; } = 4096;

	[Params(true, false)]
	public bool UseAsync { get; set; } = false;

	[Params(true, false)]
	public bool Preload { get; set; } = false;

	FileStream GetStream() => new(
		TEST_FILE,
		FileMode.Open,
		FileAccess.Read,
		FileShare.Read,
		BufferSize,
		UseAsync);

	FileStream Stream;
	TextReader Reader;
	char[] Buffer;

	readonly Lazy<string> Preloaded;

	public TextReader_FileReadMethodBenchmarks()
		=> Preloaded = new Lazy<string>(() => File.ReadAllText(TEST_FILE));

	[IterationSetup]
	public void Setup()
	{
		Stream = Preload ? null : GetStream();
		Reader = Preload ? new StringReader(Preloaded.Value) : new StreamReader(Stream);
		Buffer = new char[BufferSize];
	}

	[IterationCleanup]
	public void Cleanup()
	{
		Reader?.Dispose();
		Stream?.Dispose();
		Reader = null;
		Stream = null;
		Buffer = null;
	}

	protected void Run(Action test)
	{
		Setup();
		test();
		Cleanup();
	}

	protected async Task Run(Func<Task> test)
	{
		Setup();
		await test();
		Cleanup();
	}

	[Benchmark(Baseline = true)]
	public int StreamReader_Read()
	{
		var count = 0;
		int next;
		while ((next = Reader.Read(Buffer)) is not 0)
			count += next;
		return count;
	}

	[Benchmark]
	public async Task<int> StreamReader_ReadAsync()
	{
		var count = 0;
		int next;
		while ((next = await Reader.ReadAsync(Buffer).ConfigureAwait(false)) is not 0)
			count += next;
		return count;
	}

	[Benchmark]
	public int StreamReader_ReadLine()
	{
		var count = 0;
		while (Reader.ReadLine() is not null)
			count++;
		return count;
	}

	[Benchmark]
	public async Task<int> StreamReader_ReadLineAsync()
	{
		var count = 0;
		while (await Reader.ReadLineAsync().ConfigureAwait(false) is not null)
			count++;
		return count;
	}

	[Benchmark]
	public async Task<int> StreamReader_SingleBufferReadAsync()
	{
		var count = 0;
		await foreach (var buffer in Reader.SingleBufferReadAsync(BufferSize))
			count += buffer.Length;
		return count;
	}

	[Benchmark]
	public async Task<int> StreamReader_DualBufferReadAsync()
	{
		var count = 0;
		await foreach (var buffer in Reader.DualBufferReadAsync(BufferSize))
			count += buffer.Length;
		return count;
	}

	[Benchmark]
	public async Task<int> StreamReader_PreemptiveReadLineAsync()
	{
		var count = 0;
		await foreach (var buffer in Reader.PreemptiveReadLineAsync())
			count++;
		return count;
	}
}

public class FileReadMethodTests : TextReader_FileReadMethodBenchmarks
{
	static readonly int ExpectedCharacterCount = GetExpectedCharacterCount();

	static int GetExpectedCharacterCount()
	{
		var buffer = new char[4096];
		using var sr = new StreamReader(TEST_FILE);
		var count = 0;
		int next;
		while ((next = sr.Read(buffer)) is not 0)
			count += next;
		return count;
	}

	[Fact]
	public Task StreamReader_SingleBufferReadAsyncTest() => Run(
		async () => Assert.Equal(
			ExpectedCharacterCount,
			await StreamReader_SingleBufferReadAsync()));

	[Fact]
	public Task StreamReader_DualBufferReadAsyncTest() => Run(
		async () => Assert.Equal(
			ExpectedCharacterCount,
			await StreamReader_DualBufferReadAsync()));

	[Fact]
	public Task StreamReader_PreemptiveReadLineAsyncTest() => Run(
		async () => Assert.Equal(
			CsvFileReadTests.ExpectedLineCount,
			await StreamReader_PreemptiveReadLineAsync()));
}
