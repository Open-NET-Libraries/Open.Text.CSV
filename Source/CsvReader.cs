using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Open.Text.CSV
{
	public sealed class CsvReader : CsvReaderBase<IList<string>>
	{
		public CsvReader(TextReader source)
			: base(source, new CsvRowBuilder())
		{
		}

		public static IList<IList<string>> GetAllRowsFromFile(string filepath)
		{
			if (filepath is null)
				throw new ArgumentNullException(nameof(filepath));
			if (string.IsNullOrWhiteSpace(filepath))
				throw new ArgumentException("Cannot be empty or only whitespace.", nameof(filepath));
			Contract.EndContractBlock();

			var list = new List<IList<string>>();
			using var sr = new FileInfo(filepath).OpenText();
			foreach (var row in ReadRows(sr)) list.Add(row);
			return list;
		}

		public static async ValueTask<IList<IList<string>>> GetAllRowsFromFileAsync(string filepath)
		{
			if (filepath is null)
				throw new ArgumentNullException(nameof(filepath));
			if (string.IsNullOrWhiteSpace(filepath))
				throw new ArgumentException("Cannot be empty or only whitespace.", nameof(filepath));
			Contract.EndContractBlock();

			IList<string>? row;
			var list = new List<IList<string>>();
			var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, DEFAULT_BUFFER_SIZE, true);
			var sr = new StreamReader(fs);
			var csv = new CsvReader(sr);
			while ((row = await csv.ReadNextRowAsync().ConfigureAwait(false)) is not null)
				list.Add(row);
			return list;
		}

		public static IEnumerable<IList<string>> ReadRows(TextReader source)
		{
			using var csv = new CsvReader(source);
			foreach (var row in csv.ReadRows())
				yield return row;
		}

		public static IEnumerable<IList<string>> ReadRows(Stream stream)
		{
			if (stream is null)
				throw new ArgumentNullException(nameof(stream));
			Contract.EndContractBlock();

			using var sr = new StreamReader(stream);
			using var csv = new CsvReader(sr);
			foreach (var row in csv.ReadRows())
				yield return row;
		}

		public static IEnumerable<IList<string>> ReadRows(string csvText)
		{
			if (csvText is null) throw new ArgumentNullException(nameof(csvText));
			Contract.EndContractBlock();

			using var sr = new StringReader(csvText);
			using var csv = new CsvReader(sr);
			foreach (var row in csv.ReadRows())
				yield return row;
		}

		public static async ValueTask ReadRowsToChannelAsync(
			TextReader source,
			ChannelWriter<IList<string>> writer,
			int charBufferSize = 4096,
			CancellationToken cancellationToken = default)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				writer.Complete();
				return;
			}
			var rowBuilder = new CsvRowBuilder();
			var pool = ArrayPool<char>.Shared;
			var cNext = pool.Rent(charBufferSize);
			var cCurrent = pool.Rent(charBufferSize);
			await Task.Yield();
			try
			{
#if NETSTANDARD2_1_OR_GREATER
				var next = source.ReadAsync(cNext, cancellationToken);
#else
				var next = source.ReadAsync(cNext, 0, cNext.Length);
#endif
			loop:
				var n = await next.ConfigureAwait(false);
				if (n == 0 || cancellationToken.IsCancellationRequested)
				{
					writer.Complete();
					return;
				}

				// Preemptive request.
#if NETSTANDARD2_1_OR_GREATER
				var current = source.ReadAsync(cCurrent, cancellationToken);
#else
				var current = source.ReadAsync(cCurrent, 0, cCurrent.Length);
#endif
				if (rowBuilder.Add(cNext.AsMemory(0, n), out var remaining))
				{
					do
					{
						var nextRow = rowBuilder.LatestCompleteRow;
						Debug.Assert(nextRow != null);
						await writer.WriteAsync(nextRow!, cancellationToken).ConfigureAwait(false);
					}
					while (rowBuilder.Add(remaining, out remaining));
				}

				var swap = cNext;
				cNext = cCurrent;
				cCurrent = swap;
				next = current;

				goto loop;
			}
			catch (OperationCanceledException)
			{
				writer.TryComplete();
			}
			catch (Exception ex)
			{
				writer.TryComplete(ex);
			}
			finally
			{
				pool.Return(cNext);
				pool.Return(cCurrent);
			}
		}

		static Channel<IList<string>> CreateRowBuffer(int maxRows)
		{
			if (maxRows == 0) throw new ArgumentException("Cannot be zero.", nameof(maxRows));
			if (maxRows < -1) throw new ArgumentOutOfRangeException(nameof(maxRows), maxRows, "Cannot be less than -1.");
			return maxRows == -1
				? Channel.CreateUnbounded<IList<string>>(new UnboundedChannelOptions()
				{
					SingleWriter = true,
					AllowSynchronousContinuations = true
				})
				: Channel.CreateBounded<IList<string>>(new BoundedChannelOptions(maxRows)
				{
					SingleWriter = true,
					AllowSynchronousContinuations = true
				});
		}

		public static IEnumerable<IList<string>> ReadRowsBuffered(
			TextReader source,
			int rowBufferCount = -1,
			int charBufferSize = 4096,
			CancellationToken cancellationToken = default)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			var rowBuffer = CreateRowBuffer(rowBufferCount);
			Contract.EndContractBlock();

			_ = ReadRowsToChannelAsync(source, rowBuffer, charBufferSize, cancellationToken).AsTask();
			var reader = rowBuffer.Reader;

		loop:
			while (reader.TryRead(out var row))
				yield return row;

			if (cancellationToken.IsCancellationRequested)
				yield break;

			try
			{
				if (reader.WaitToReadAsync(cancellationToken).AsTask().Result)
					goto loop;
			}
			catch (OperationCanceledException)
			{
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2016:Forward the 'CancellationToken' parameter to methods", Justification = "Is handled internally.")]
		public static ChannelReader<IList<string>> ReadRowsToChannel(
			string filepath,
			int rowBufferCount = -1,
			int charBufferSize = 4096,
			CancellationToken cancellationToken = default)
		{
			if (filepath is null)
				throw new ArgumentNullException(nameof(filepath));
			if (string.IsNullOrWhiteSpace(filepath))
				throw new ArgumentException("Cannot be empty or only whitespace.", nameof(filepath));
			var rowBuffer = CreateRowBuffer(rowBufferCount);
			Contract.EndContractBlock();

			var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, charBufferSize, true);
			var sr = new StreamReader(fs);
			_ = ReadRowsToChannelAsync(sr, rowBuffer, charBufferSize, cancellationToken).AsTask().ContinueWith(t =>
			{
				sr.Dispose();
				fs.Dispose();
			});

			return rowBuffer.Reader;
		}

#if NETSTANDARD2_1_OR_GREATER

		public async IAsyncEnumerable<IList<string>> ReadRowsAsync()
		{
			IList<string>? row;
			while ((row = await ReadNextRowAsync().ConfigureAwait(false)) is not null)
				yield return row;
		}

		public static async IAsyncEnumerable<IList<string>> ReadRowsBufferedAsync(
			TextReader source,
			int rowBufferCount = 3,
			int charBufferSize = 4096,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			var rowBuffer = CreateRowBuffer(rowBufferCount);
			Contract.EndContractBlock();

			_ = ReadRowsToChannelAsync(source, rowBuffer, charBufferSize, cancellationToken).AsTask();
			var reader = rowBuffer.Reader;

		loop:
			while (reader.TryRead(out var row))
				yield return row;

			if (cancellationToken.IsCancellationRequested)
				yield break;

			try
			{
				if (await reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
					goto loop;
			}
			catch (OperationCanceledException)
			{
			}
		}
#endif

	}
}
