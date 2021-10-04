using Microsoft.Toolkit.HighPerformance.Buffers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Open.Text.CSV;

public static class Extensions
{
#if ASYNC_ENUMERABLE

	public static async IAsyncEnumerable<ReadOnlyMemory<char>> SingleBufferReadAsync(
		this TextReader reader,
		int bufferSize = 4096)
	{
		var pool = MemoryPool<char>.Shared;
		using var A = pool.Rent(bufferSize);
		var buffer = A.Memory;

		while (true)
		{
			var n = await reader.ReadAsync(buffer).ConfigureAwait(false);
			if (n == 0) break;
			yield return n == buffer.Length ? buffer : buffer.Slice(0, n);
		}
	}

	public static async IAsyncEnumerable<ReadOnlyMemory<char>> DualBufferReadAsync(
		this TextReader reader,
		int bufferSize = 4096)
	{
		var pool = MemoryPool<char>.Shared;
		using var A = pool.Rent(bufferSize);
		using var B = pool.Rent(bufferSize);
		var cNext = A.Memory;
		var cCurrent = B.Memory;

		var next = reader.ReadAsync(cNext);
		while (true)
		{
			var n = await next.ConfigureAwait(false);
			if (n == 0) break;

			// Preemptive request before yielding.
			var current = reader.ReadAsync(cCurrent);
			yield return n == cNext.Length ? cNext : cNext.Slice(0, n);

			var swap = cNext;
			cNext = cCurrent;
			cCurrent = swap;
			next = current;
		}
	}

	public static async IAsyncEnumerable<string> PreemptiveReadLineAsync(
		this TextReader reader)
	{
		var next = reader.ReadLineAsync();
		while (true)
		{
			var n = await next.ConfigureAwait(false);
			if (n == null) break;

			// Preemptive request before yielding.
			next = reader.ReadLineAsync();
			yield return n;
		}
	}

	public static async IAsyncEnumerable<ReadOnlySequence<byte>> EnumerateAsync(
		this PipeReader pipeReader,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var readResult = await pipeReader
			.ReadAsync(cancellationToken: cancellationToken)
			.ConfigureAwait(continueOnCapturedContext: false);

		if (readResult.IsCompleted)
			yield break;

		do
		{
			try
			{
				yield return readResult.Buffer;
			}
			finally
			{
				pipeReader.AdvanceTo(consumed: readResult.Buffer.End);
			}

			readResult = await pipeReader
				.ReadAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(continueOnCapturedContext: false);
		}
		while (!readResult.IsCompleted);
	}

	public static async IAsyncEnumerable<ReadOnlySequence<byte>> EnumerateAsync(
		this Stream stream,
		int bufferSize = 4096,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		using var bufferWriter = new ArrayPoolBufferWriter<byte>();

		var memory = bufferWriter.GetMemory(sizeHint: bufferSize);
		var readResult = await stream
			.ReadAsync(memory, cancellationToken)
			.ConfigureAwait(continueOnCapturedContext: false);

		if (0 < readResult)
		{
			do
			{
				bufferWriter.Advance(count: readResult);
				yield return new ReadOnlySequence<byte>(bufferWriter.WrittenMemory);
				bufferWriter.Clear();

				readResult = await stream
					.ReadAsync(memory, cancellationToken)
					.ConfigureAwait(continueOnCapturedContext: false);
			} while (0 < readResult);
		}
	}

#endif

#if BUFFERWRITER_DECODE
	public static async IAsyncEnumerable<ArrayPoolBufferWriter<char>> DecodeAsync(
		this IAsyncEnumerable<ReadOnlySequence<byte>> source,
		Decoder? decoder = default,
		int initialBufferSize = 4096,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		decoder ??= Encoding.UTF8.GetDecoder();

		using var decodedBlock = new ArrayPoolBufferWriter<char>(initialCapacity: initialBufferSize);

		await foreach (var encodedBlock in source
			.WithCancellation(cancellationToken)
			.ConfigureAwait(false))
		{
			bool isDecodingCompleted;

			do
			{
				decoder.Convert(
					bytes: in encodedBlock,
					charsUsed: out _,
					completed: out isDecodingCompleted,
					flush: false,
					writer: decodedBlock
				);

				yield return decodedBlock;

				decodedBlock.Clear();
			} while (!isDecodingCompleted && !cancellationToken.IsCancellationRequested);
		}

		decoder.Convert(
			bytes: in ReadOnlySequence<byte>.Empty,
			charsUsed: out var numberOfCharsUsed,
			completed: out _,
			flush: true,
			writer: decodedBlock);

		if (0 < numberOfCharsUsed)
			yield return decodedBlock;
	}
#endif
}