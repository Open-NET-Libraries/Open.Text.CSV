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

namespace Open.Text.CSV;

public abstract class CsvReaderBase<TRow> : IDisposable
{
	public const int DEFAULT_BUFFER_SIZE = 4096;

	protected CsvReaderBase(TextReader source, CsvRowBuilderBase<TRow> rowBuilder)
	{
		_source = source ?? throw new ArgumentNullException(nameof(source));
		_rowBuilder = rowBuilder ?? throw new ArgumentNullException(nameof(rowBuilder));
	}

	TextReader? _source;
	readonly CsvRowBuilderBase<TRow> _rowBuilder;

	protected TextReader Source => _source ?? throw new ObjectDisposedException(GetType().ToString());

	public void Dispose()
	{
		_source = null; // The intention here is if this object is disposed, then prevent further reading.
	}

	ArraySegment<char> _remaining = default;

	public bool EndReached { get; private set; }

	/* 
	// Slightly sub optimal.
	public bool TryReadNextRow(out TRow? row)
	{
		var s = Source;
		int c;

	loop:
		c = s.Read();

		if (c == -1)
		{
			var rowReady = RowBuilder.EndRow();
			row = NextRow;
			return rowReady;
		}

		if (RowBuilder.AddChar(c))
		{
			row = NextRow;
			return true;

		}

		goto loop;
	} */
	public TRow? ReadNextRow()
	{
		var s = Source;
		if (EndReached) return default;

		int c;
		var pool = ArrayPool<char>.Shared;

		var buffer = _remaining.Count == 0 ? pool.Rent(DEFAULT_BUFFER_SIZE) : _remaining.Array;
		if (_remaining.Count != 0)
		{
			goto add;
		}

	loop:
		c = s.Read(buffer, 0, buffer.Length);
		if (c == 0)
		{
			EndReached = true;
			pool.Return(buffer, true);
			return _rowBuilder.EndRow() ? _rowBuilder.LatestCompleteRow : default;
		}
		_remaining = new ArraySegment<char>(buffer, 0, c);

	add:
		if (_rowBuilder.Add(in _remaining, out _remaining))
		{
			if (_remaining.Count == 0)
				pool.Return(buffer, true);
			return _rowBuilder.LatestCompleteRow;
		}

		goto loop;

	}

	public bool TryReadNextRow(out TRow? row)
	{
		row = ReadNextRow();
		return row is not null;
	}

	public async ValueTask<TRow?> ReadNextRowAsync()
	{
		var s = Source;
		if (EndReached) return default;

		int c;
		var pool = ArrayPool<char>.Shared;

		var buffer = _remaining.Count == 0 ? pool.Rent(DEFAULT_BUFFER_SIZE) : _remaining.Array;
		if (_remaining.Count != 0)
		{
			goto add;
		}

	loop:
		c = await s.ReadAsync(buffer, 0, buffer.Length);
		if (c == 0)
		{
			EndReached = true;
			pool.Return(buffer, true);
			return _rowBuilder.EndRow() ? _rowBuilder.LatestCompleteRow : default;
		}
		_remaining = new ArraySegment<char>(buffer, 0, c);

	add:
		if (_rowBuilder.Add(in _remaining, out _remaining))
		{
			if (_remaining.Count == 0)
				pool.Return(buffer, true);
			return _rowBuilder.LatestCompleteRow;
		}

		goto loop;
	}

	public IEnumerable<TRow> ReadRows()
	{
		while (TryReadNextRow(out var rowBuffer))
			yield return rowBuffer!;
	}

}
