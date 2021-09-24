using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Open.Text.CSV
{
	public class CsvRowEnumerator : IEnumerator<IReadOnlyList<string>>
	{
		const int BUFFER_SIZE = 4096;

		readonly Channel<ArraySegment<string>> _rowBuffer;


		public CsvRowEnumerator(TextReader source, int rowBufferCount = 3)
		{
			if (rowBufferCount < 1) throw new ArgumentOutOfRangeException(nameof(rowBufferCount), rowBufferCount, "Must be at least 1.");
			_rowBuffer = Channel.CreateBounded<ArraySegment<string>>(new BoundedChannelOptions(rowBufferCount)
			{
				SingleWriter = true,
				AllowSynchronousContinuations = true
			});

			_ = Task.Run(async () =>
			{
				try
				{
					var source = source;
					var pool = ArrayPool<char>.Shared;
					var cNext = pool.Rent(BUFFER_SIZE);
					char[] cCurrent;
					var next = source.ReadAsync(cNext, 0, BUFFER_SIZE);

				loop:
					var n = await next.ConfigureAwait(false);
					if (n == 0)
					{
						_rowBuffer.Writer.Complete();
						return;
					}

					// Preemptive request before yielding.
					cCurrent = pool.Rent(BUFFER_SIZE);
					var current = source.ReadAsync(cCurrent, 0, BUFFER_SIZE);
					if (!await AddChars(new ArraySegment<char>(cNext, 0, n))) return;

					cNext = cCurrent;
					next = current;
					goto loop;
				}
				catch(Exception ex)
				{
					_rowBuffer.Writer.Complete(ex);
				}
			});
		}

		async ValueTask<bool> AddChars(ArraySegment<char> chars)
		{

		}

		protected const string CORRUPT_FIELD = "Corrupt field found. A double quote is not escaped or there is extra data after a quoted field.";
		TextReader? _source;
		protected TextReader Source => _source ?? throw new ObjectDisposedException(GetType().ToString());

		public IReadOnlyList<string> Current { get; private set; } = ArraySegment<string>.Empty;

		object IEnumerator.Current => Current;

		public void Dispose()
		{
			_rowBuffer.Writer.Complete();
			_source = null; // The intention here is if this object is disposed, then prevent further reading.
		}

		public bool MoveNext()
		{
			if (_rowBuffer.TryTake(out var e))
			{
				Current = e;
				return true;
			}
			Current = ArraySegment<string>.Empty;
			return false;
		}

		public void Reset() => throw new NotImplementedException();
	}
}
