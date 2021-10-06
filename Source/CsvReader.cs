using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Open.Text.CSV;

public class CsvReader<TRow> : IDisposable
{
	public const int DEFAULT_BUFFER_SIZE = 4096;

	protected CsvReader(TextReader source, ICsvRowBuilder<TRow> rowBuilder)
	{
		_source = source ?? throw new ArgumentNullException(nameof(source));
		_rowBuilder = rowBuilder ?? throw new ArgumentNullException(nameof(rowBuilder));
	}

	TextReader? _source;
	readonly ICsvRowBuilder<TRow> _rowBuilder;
	public int MaxFields => _rowBuilder.MaxFields;

	protected TextReader Source => _source ?? throw new ObjectDisposedException(GetType().ToString());

	public void Dispose()
	{
		_source = null; // The intention here is if this object is disposed, then prevent further reading.
		if (_rowBuilder is IDisposable d) d.Dispose();
	}

	ArraySegment<char> _remaining = default;

	public bool EndReached { get; private set; }

	public TRow? ReadNextRow()
	{
		var s = Source;
		if (EndReached) return default;

		int c;
		TRow? row;
		var pool = ArrayPool<char>.Shared;

		var buffer = _remaining.Count == 0 ? pool.Rent(DEFAULT_BUFFER_SIZE) : _remaining.Array;
		if (_remaining.Count != 0)
		{
			goto add;
		}

	loop:
		c = s.Read(buffer!, 0, buffer!.Length);
		if (c == 0)
		{
			EndReached = true;
			pool.Return(buffer, true);
			return _rowBuilder.EndRow(out row) ? row : default;
		}

		_remaining = new ArraySegment<char>(buffer, 0, c);

	add:
		if (_rowBuilder.Add(in _remaining, out _remaining, out row))
		{
			if (_remaining.Count == 0)
				pool.Return(buffer!, true);
			return row;
		}

		goto loop;

	}

	public bool TryReadNextRow(
#if NULL_ANALYSIS
	[NotNullWhen(true)]
#endif
		out TRow? row)
	{
		row = ReadNextRow();
		return row is not null;
	}

	public async ValueTask<TRow?> ReadNextRowAsync()
	{
		var s = Source;
		if (EndReached) return default;

		int c;
		TRow? row;
		var pool = ArrayPool<char>.Shared;

		var buffer = _remaining.Count == 0 ? pool.Rent(DEFAULT_BUFFER_SIZE) : _remaining.Array;
		if (_remaining.Count != 0)
		{
			goto add;
		}

	loop:
		c = await s.ReadAsync(buffer!, 0, buffer!.Length);
		if (c == 0)
		{
			EndReached = true;
			pool.Return(buffer, true);
			return _rowBuilder.EndRow(out row) ? row : default;
		}

		_remaining = new ArraySegment<char>(buffer, 0, c);

	add:
		if (_rowBuilder.Add(in _remaining, out _remaining, out row))
		{
			if (_remaining.Count == 0)
				pool.Return(buffer!, true);
			return row;
		}

		goto loop;
	}

	public IEnumerable<TRow> ReadRows()
	{
		while (TryReadNextRow(out var rowBuffer))
			yield return rowBuffer!;
	}

	protected static Channel<T> CreateRowBuffer<T>(int maxRows) => maxRows switch
	{
		0 => throw new ArgumentException("Cannot be zero.", nameof(maxRows)),
		< -1 => throw new ArgumentOutOfRangeException(nameof(maxRows), maxRows, "Cannot be less than -1."),
		-1 => Channel.CreateUnbounded<T>(new UnboundedChannelOptions()
		{
			SingleWriter = true,
			AllowSynchronousContinuations = true
		}),
		_ => Channel.CreateBounded<T>(new BoundedChannelOptions(maxRows)
		{
			SingleWriter = true,
			AllowSynchronousContinuations = true
		})
	};

	public async ValueTask ReadRowsToChannelAsync(
		ChannelWriter<TRow> writer,
		int charBufferSize = 4096,
		CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			writer.Complete();
			return;
		}

		await Task.Yield();
		var pool = ArrayPool<char>.Shared;
		var cNext = pool.Rent(charBufferSize);
		var cCurrent = pool.Rent(charBufferSize);
		var source = Source;
		try
		{
#if BUFFERS
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
#if BUFFERS
			var current = source.ReadAsync(cCurrent, cancellationToken);
#else
			var current = source.ReadAsync(cCurrent, 0, cCurrent.Length);
#endif
			if (_rowBuilder.Add(cNext.AsMemory(0, n), out var remaining, out var nextRow))
			{
				do
				{
					Debug.Assert(nextRow != null);
					await writer.WriteAsync(nextRow!, cancellationToken).ConfigureAwait(false);
				}
				while (_rowBuilder.Add(remaining, out remaining, out nextRow));
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

	public IEnumerable<TRow> ReadRowsBuffered(
		int rowBufferCount = -1,
		int charBufferSize = 4096,
		CancellationToken cancellationToken = default)
	{
		var rowBuffer = CreateRowBuffer<TRow>(rowBufferCount);
		Contract.EndContractBlock();

		_ = ReadRowsToChannelAsync(rowBuffer, charBufferSize, cancellationToken).AsTask();
		var reader = rowBuffer.Reader;

	loop:

		try
		{
			if (!reader.WaitToReadAsync(cancellationToken).AsTask().Result)
				yield break;
		}
		catch (OperationCanceledException)
		{
		}

		while (reader.TryRead(out var row))
			yield return row;

		if (!cancellationToken.IsCancellationRequested)
			goto loop;
	}

	public ChannelReader<TRow> ReadRowsToChannel(
		int rowBufferCount = -1,
		int charBufferSize = 4096,
		CancellationToken cancellationToken = default)
	{
		var rowBuffer = CreateRowBuffer<TRow>(rowBufferCount);
		Contract.EndContractBlock();

		_ = ReadRowsToChannelAsync(rowBuffer, charBufferSize, cancellationToken).AsTask();
		return rowBuffer.Reader;
	}


#if ASYNC_ENUMERABLE

	public async IAsyncEnumerable<TRow> ReadRowsAsync()
	{
		TRow? row;
		while ((row = await ReadNextRowAsync().ConfigureAwait(false)) is not null)
			yield return row;
	}

	public async IAsyncEnumerable<TRow> ReadRowsBufferedAsync(
		int rowBufferCount = 3,
		int charBufferSize = 4096,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var rowBuffer = CreateRowBuffer<TRow>(rowBufferCount);
		Contract.EndContractBlock();

		_ = ReadRowsToChannelAsync(rowBuffer, charBufferSize, cancellationToken).AsTask();
		var reader = rowBuffer.Reader;

	loop:

		try
		{
			if (!await reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
				yield break;
		}
		catch (OperationCanceledException)
		{
			yield break;
		}

		while (reader.TryRead(out var row))
			yield return row;

		if (!cancellationToken.IsCancellationRequested)
			goto loop;

	}
#endif

}


public sealed class CsvReader : CsvReader<IList<string>>
{
	public CsvReader(TextReader source)
		: base(source, new ListCsvRowBuilder())
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
		using var reader = new CsvReader(source);
		await reader.ReadRowsToChannelAsync(writer, charBufferSize, cancellationToken).ConfigureAwait(false);
	}


	public static IEnumerable<IList<string>> ReadRowsBuffered(
		TextReader source,
		int rowBufferCount = -1,
		int charBufferSize = 4096,
		CancellationToken cancellationToken = default)
	{
		using var reader = new CsvReader(source);
		foreach (var row in reader.ReadRowsBuffered(rowBufferCount, charBufferSize, cancellationToken))
			yield return row;
	}

	[SuppressMessage("Reliability", "CA2016:Forward the 'CancellationToken' parameter to methods", Justification = "Is handled internally.")]
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
		var rowBuffer = CreateRowBuffer<IList<string>>(rowBufferCount);
		Contract.EndContractBlock();

		var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, charBufferSize, true);
		var sr = new StreamReader(fs);
		var csv = new CsvReader(sr);
		_ = csv.ReadRowsToChannelAsync(rowBuffer, charBufferSize, cancellationToken).AsTask().ContinueWith(t =>
		{
			csv.Dispose();
			sr.Dispose();
			fs.Dispose();
		});

		return rowBuffer.Reader;
	}

#if ASYNC_ENUMERABLE
	public static async IAsyncEnumerable<IList<string>> ReadRowsBufferedAsync(
		TextReader source,
		int rowBufferCount = 3,
		int charBufferSize = 4096,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		using var reader = new CsvReader(source);
		await foreach (var row in reader.ReadRowsBufferedAsync(rowBufferCount, charBufferSize, cancellationToken))
			yield return row;
	}
#endif

#if BUFFERWRITER_DECODE
	public static async IAsyncEnumerable<IMemoryOwner<string>> PipeRowsAsync(
		Stream source,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var rowBuilder = new MemoryCsvRowBuilder();
		await foreach(var bufferWriter in PipeReader.Create(source)
			.EnumerateAsync(cancellationToken)
			.DecodeAsync(cancellationToken: cancellationToken))
		{
			var chunk = bufferWriter.WrittenMemory;
			while(rowBuilder.Add(chunk, out chunk, out var row))
				yield return row;
		}
	}
#endif

}

public sealed class CsvMemoryReader : CsvReader<IMemoryOwner<string>>
{
	public CsvMemoryReader(TextReader source)
		: base(source, new MemoryCsvRowBuilder())
	{
	}

	public static IEnumerable<IMemoryOwner<string>> ReadRows(TextReader source)
	{
		using var csv = new CsvMemoryReader(source);
		foreach (var row in csv.ReadRows())
			yield return row;
	}

	public static IEnumerable<IMemoryOwner<string>> ReadRows(Stream stream)
	{
		if (stream is null)
			throw new ArgumentNullException(nameof(stream));
		Contract.EndContractBlock();

		using var sr = new StreamReader(stream);
		using var csv = new CsvMemoryReader(sr);
		foreach (var row in csv.ReadRows())
			yield return row;
	}

	public static IEnumerable<IMemoryOwner<string>> ReadRows(string csvText)
	{
		if (csvText is null) throw new ArgumentNullException(nameof(csvText));
		Contract.EndContractBlock();

		using var sr = new StringReader(csvText);
		using var csv = new CsvMemoryReader(sr);
		foreach (var row in csv.ReadRows())
			yield return row;
	}

	public static async ValueTask ReadRowsToChannelAsync(
		TextReader source,
		ChannelWriter<IMemoryOwner<string>> writer,
		int charBufferSize = 4096,
		CancellationToken cancellationToken = default)
	{
		using var reader = new CsvMemoryReader(source);
		await reader.ReadRowsToChannelAsync(writer, charBufferSize, cancellationToken).ConfigureAwait(false);
	}


	public static IEnumerable<IMemoryOwner<string>> ReadRowsBuffered(
		TextReader source,
		int rowBufferCount = -1,
		int charBufferSize = 4096,
		CancellationToken cancellationToken = default)
	{
		using var reader = new CsvMemoryReader(source);
		foreach (var row in reader.ReadRowsBuffered(rowBufferCount, charBufferSize, cancellationToken))
			yield return row;
	}

	[SuppressMessage("Reliability", "CA2016:Forward the 'CancellationToken' parameter to methods", Justification = "Is handled internally.")]
	public static ChannelReader<IMemoryOwner<string>> ReadRowsToChannel(
		string filepath,
		int rowBufferCount = -1,
		int charBufferSize = 4096,
		CancellationToken cancellationToken = default)
	{
		if (filepath is null)
			throw new ArgumentNullException(nameof(filepath));
		if (string.IsNullOrWhiteSpace(filepath))
			throw new ArgumentException("Cannot be empty or only whitespace.", nameof(filepath));
		var rowBuffer = CreateRowBuffer<IMemoryOwner<string>>(rowBufferCount);
		Contract.EndContractBlock();

		var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, charBufferSize, true);
		var sr = new StreamReader(fs);
		var csv = new CsvMemoryReader(sr);
		_ = csv.ReadRowsToChannelAsync(rowBuffer, charBufferSize, cancellationToken).AsTask().ContinueWith(t =>
		{
			csv.Dispose();
			sr.Dispose();
			fs.Dispose();
		});

		return rowBuffer.Reader;
	}

#if ASYNC_ENUMERABLE
	public static async IAsyncEnumerable<IMemoryOwner<string>> ReadRowsBufferedAsync(
		TextReader source,
		int rowBufferCount = 3,
		int charBufferSize = 4096,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		using var reader = new CsvMemoryReader(source);
		await foreach (var row in reader.ReadRowsBufferedAsync(rowBufferCount, charBufferSize, cancellationToken))
			yield return row;
	}
#endif
}