#if NETSTANDARD2_1_OR_GREATER
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Open.Text.CSV
{
	public class CsvAsyncReader : CsvReaderBase, IAsyncEnumerable<ArraySegment<string>>
	{
		public CsvAsyncReader(TextReader source) : base(source)
		{
			
		}

		CancellationTokenSource? _tokenSource = new();
		public override void Dispose()
		{
			base.Dispose();
			var ts = _tokenSource;
			_tokenSource = null;
			ts?.Cancel();
			ts?.Dispose();
		}

		const int MIN_BUFFER_SIZE = 4096;
		async IAsyncEnumerable<ArraySegment<char>> DualBufferReadAsync()
		{
			var source = Source;
			var token = _tokenSource!.Token;
			var pool = ArrayPool<char>.Shared;
			var cNext = pool.Rent(MIN_BUFFER_SIZE);
			var cCurrent = pool.Rent(MIN_BUFFER_SIZE);
			try
			{
				var next = source.ReadAsync(cNext);
				while (true)
				{
					var n = await next.ConfigureAwait(false);
					if (n == 0 || token.IsCancellationRequested) break;

					// Preemptive request before yielding.
					var current = source.ReadAsync(cCurrent);
					yield return n == cNext.Length ? cNext : new ArraySegment<char>(cNext, 0, n);
					if (token.IsCancellationRequested) break;

					var swap = cNext;
					cNext = cCurrent;
					cCurrent = swap;
					next = current;
				}
			}
			finally
			{
				pool.Return(cNext, true);
				pool.Return(cCurrent, true);
			}

		}

		public async IAsyncEnumerator<ArraySegment<string>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
		{
			await foreach(var buffer in DualBufferReadAsync())
			{

			}
		}

		ArraySegment<char>? _remaining;


	}
}
#endif