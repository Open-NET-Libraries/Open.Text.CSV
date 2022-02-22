using BenchmarkDotNet.Attributes;
using Open.IO.Extensions;
using System;
using System.Threading.Tasks;

namespace Open.Text.CSV.Test;

public class FileStreamReadBenchmark : FileReadBenchmarkBase
{
	public FileStreamReadBenchmark(string testFile = null) : base(testFile) { }

	/*
	 * NOTES:
	 * 
	 * There should be no difference between FileStream_ReadAsync and FileStream_SingleBufferReadAsync
	 * as they are fundamentally the same except for FileStream_SingleBufferReadAsync
	 * might have a larger buffer from the pool.
	 */

	[Benchmark(Baseline = true)]
	public int FileStream_Read()
	{
		var count = 0;
		int next;
		var buffer = new byte[ByteBufferSize];
		var span = buffer.AsSpan();
		using var stream = GetStream();
		while ((next = stream.Read(span)) is not 0)
			count += next;
		return count;
	}

	//[Benchmark]
	public async Task<int> FileStream_ReadAsync()
	{
		var count = 0;
		int next;
		var buffer = new byte[ByteBufferSize];
		var mem = buffer.AsMemory();
		using var stream = GetStream();
		while ((next = await stream
			.ReadAsync(mem)
			.ConfigureAwait(false)) is not 0)
		{
			count += next;
		}

		return count;
	}

	[Benchmark]
	public async Task<long> FileStream_SingleBufferReadAsync()
	{
		long count = 0;
		using var stream = GetStream();
		await foreach (var buffer in stream
			.SingleBufferReadAsync(ByteBufferSize)
			.ConfigureAwait(false))
		{
			count += buffer.Length;
		}

		return count;
	}

	[Benchmark]
	public async Task<long> FileStream_DualBufferReadAsync()
	{
		long count = 0;
		using var stream = GetStream();
		await foreach (var buffer in stream
			.DualBufferReadAsync(ByteBufferSize)
			.ConfigureAwait(false))
		{
			count += buffer.Length;
		}

		return count;
	}

	[Benchmark]
	public async Task<long> FileStream_DualBufferReadAsync_Yielded()
	{
		long count = 0;
		using var stream = GetStream();
		await foreach (var buffer in stream
			.DualBufferReadAsync(ByteBufferSize)
			.ConfigureAwait(false))
		{
			count += buffer.Length;
			await Task.Yield();
		}

		return count;
	}
}
