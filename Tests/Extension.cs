using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace Open.Text.CSV.Test
{
	public static class TextReaderExtension
	{

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
	}
}
