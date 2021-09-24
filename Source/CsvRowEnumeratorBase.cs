using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Open.Text.CSV
{
	public abstract class CsvRowEnumeratorBase : IDisposable
	{
		const int BUFFER_SIZE = 4096;

		protected readonly Channel<ArraySegment<string>> RowBuffer;

		protected CsvRowEnumeratorBase(TextReader source, int rowBufferCount = 3)
		{
			if (rowBufferCount < 1) throw new ArgumentOutOfRangeException(nameof(rowBufferCount), rowBufferCount, "Must be at least 1.");
			RowBuffer = Channel.CreateBounded<ArraySegment<string>>(new BoundedChannelOptions(rowBufferCount)
			{
				SingleWriter = true,
				AllowSynchronousContinuations = true
			});

			_ = Task.Run(async () =>
			{
				try
				{
					var pool = ArrayPool<char>.Shared;
					var cNext = pool.Rent(BUFFER_SIZE);
					char[] cCurrent;
					var next = source.ReadAsync(cNext, 0, BUFFER_SIZE);

				loop:
					var n = await next.ConfigureAwait(false);
					if (n == 0)
					{
						RowBuffer.Writer.Complete();
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
				catch (Exception ex)
				{
					RowBuffer.Writer.Complete(ex);
				}
			});
		}

		protected const string CORRUPT_FIELD = "Corrupt field found. A double quote is not escaped or there is extra data after a quoted field.";

		protected List<string> _currentRow = new();
		protected readonly StringBuilder _fieldBuffer = new();
		async ValueTask<bool> AddChars(ArraySegment<char> chars)
		{
			foreach(var c in chars)
			{

			}
		}

		public virtual void Dispose()
		{
			RowBuffer.Writer.TryComplete();
		}

		public IReadOnlyList<string> Current { get; private set; } = Array.Empty<string>();


		protected enum State
		{
			BeforeField,
			InField,
			InQuotedField,
			Quote,
			AfterQuotedField,
			EndOfRow,
		}
	}
}
