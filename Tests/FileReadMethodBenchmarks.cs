using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Open.Text.CSV.Test
{
	public class FileReadMethodBenchmarks
	{
		[Params(1024, 4096, 16384)]
		public int BufferSize { get; set; } = 4096;

		[Params(true, false)]
		public bool UseAsync { get; set; } = false;

		[Params(100000)]
		public int MaxRows { get; set; } = -1;


		FileStream GetStream() => new(
			CsvFileReadTests.TEST_DATA_CSV,
			FileMode.Open,
			FileAccess.Read,
			FileShare.Read,
			BufferSize,
			UseAsync);

		FileStream Stream;
		StreamReader Reader;
		char[] Buffer;

		[IterationSetup]
		public void Setup()
		{
			Stream = GetStream();
			Reader = new StreamReader(Stream);
			Buffer = new char[BufferSize];
		}

		[IterationCleanup]
		public void Cleanup()
		{
			Reader.Dispose();
			Stream.Dispose();
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
			while (Reader.Read(Buffer) is not 0)
			{
				count++;
				if (count == MaxRows) break;
			}
			return count;
		}

		[Benchmark]
		public async Task<int> StreamReader_ReadAsync()
		{
			var count = 0;
			while (await Reader.ReadAsync(Buffer).ConfigureAwait(false) is not 0)
			{
				count++;
				if (count == MaxRows) break;
			}
			return count;
		}

		[Benchmark]
		public int StreamReader_ReadLine()
		{
			var count = 0;
			while (Reader.ReadLine() is not null)
			{
				count++;
				if (count == MaxRows) break;
			}
			return count;
		}

		[Benchmark]
		public async Task<int> StreamReader_ReadLineAsync()
		{
			var count = 0;
			while (await Reader.ReadLineAsync().ConfigureAwait(false) is not null)
			{
				count++;
				if (count == MaxRows) break;
			}
			return count;
		}

		[Benchmark]
		public async Task<int> StreamReader_SingleBufferReadAsync()
		{
			var count = 0;
			await foreach (var buffer in Reader.SingleBufferReadAsync(BufferSize))
			{
				count++;
				if (count == MaxRows) break;
			}
			return count;
		}

		[Benchmark]
		public async Task<int> StreamReader_DualBufferReadAsync()
		{
			var count = 0;
			await foreach (var buffer in Reader.DualBufferReadAsync(BufferSize))
			{
				count++;
				if (count == MaxRows) break;
			}
			return count;
		}

		[Benchmark]
		public async Task<int> StreamReader_PreemptiveReadLineAsync()
		{
			var count = 0;
			await foreach (var buffer in Reader.PreemptiveReadLineAsync())
			{
				count++;
				if (count == MaxRows) break;
			}
			return count;
		}
	}

	public class FileReadMethodTests : FileReadMethodBenchmarks
	{

		[Fact]
		public void StreamReader_ReadLineTest() => Run(
			()=> Assert.Equal(
				CsvFileReadTests.ExpectedLineCount,
				StreamReader_ReadLine()));

		[Fact]
		public Task StreamReader_SingleBufferReadAsyncTest() => Run(
			async () => Assert.NotEqual(
				0, await StreamReader_SingleBufferReadAsync()));

		[Fact]
		public Task StreamReader_DualBufferReadAsyncTest() => Run(
			async () => Assert.NotEqual(
				0, await StreamReader_DualBufferReadAsync()));

		[Fact]
		public Task StreamReader_PreemptiveReadLineAsyncTest() => Run(
			async () => Assert.Equal(
				CsvFileReadTests.ExpectedLineCount,
				await StreamReader_PreemptiveReadLineAsync()));
	}
}
