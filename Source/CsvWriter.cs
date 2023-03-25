using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;

namespace Open.Text.CSV;

/// <summary>
/// A simple utility for writing CSV files.
/// </summary>
public class CsvWriter : IDisposable
{
	TextWriter? _target;
	[ExcludeFromCodeCoverage]
	internal TextWriter Target
		=> _target ?? throw new ObjectDisposedException(GetType().ToString());

	/// <summary>
	/// Constructs a <see cref="CsvWriter"/>.
	/// </summary>
	/// <param name="target">The <see cref="TextWriter"/> to write to.</param>
	/// <exception cref="ArgumentNullException">If the <paramref name="target"/> is null.</exception>
	[ExcludeFromCodeCoverage]
	public CsvWriter(TextWriter target)
		=> _target = target ?? throw new ArgumentNullException(nameof(target));

	/// <summary>
	/// Releases the underlying <see cref="TextWriter"/> to prevent reusing this instance.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public void Dispose()
		=> _target = null; // The intention here is if this object is disposed, then prevent further writing.

	/// <summary>
	/// Writes a single CSV formatted row to the underlying <see cref="TextWriter"/>.
	/// </summary>
	/// <inheritdoc cref="WriteRow{T}(TextWriter, IEnumerable{T}, bool, bool)"/>
	[ExcludeFromCodeCoverage]
	public void WriteRow<T>(
		IEnumerable<T> row,
		bool forceQuotes = false,
		bool omitRowComma = false)
		=> WriteRow(Target, row, forceQuotes, omitRowComma);

	/// <summary>
	/// Writes multiple CSV formatted rows to the underlying <see cref="TextWriter"/>.
	/// </summary>
	/// <inheritdoc cref="WriteRows{T}(TextWriter, IEnumerable{IEnumerable{T}}, bool, bool)"/>
	[ExcludeFromCodeCoverage]
	public void WriteRows<T>(
		IEnumerable<IEnumerable<T>> rows,
		bool forceQuotes = false,
		bool omitRowCommas = false)
		=> WriteRows(Target, rows, forceQuotes, omitRowCommas);

	/// <summary>
	/// Writes a single CSV formatted row to the underlying <see cref="TextWriter"/>.
	/// </summary>
	/// <inheritdoc cref="WriteRowAsync{T}(TextWriter, IEnumerable{T}, bool, bool)"/>
	[ExcludeFromCodeCoverage]
	public ValueTask WriteRowAsync<T>(
		IEnumerable<T> row,
		bool forceQuotes = false,
		bool omitRowComma = false)
		=> WriteRowAsync(Target, row, forceQuotes, omitRowComma);

	/// <summary>
	/// Writes multiple CSV formatted rows to the underlying <see cref="TextWriter"/>.
	/// </summary>
	/// <inheritdoc cref="WriteRowsAsync{T}(TextWriter, IEnumerable{IEnumerable{T}}, bool, bool)"/>
	[ExcludeFromCodeCoverage]
	public ValueTask WriteRowsAsync<T>(
		IEnumerable<IEnumerable<T>> rows,
		bool forceQuotes = false,
		bool omitRowCommas = false)
		=> WriteRowsAsync(Target, rows, forceQuotes, omitRowCommas);

	/// <summary>
	/// Writes a CSV formatted value to the <paramref name="writer"/>.
	/// </summary>
	/// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
	/// <param name="value">The value to write.</param>
	/// <param name="forceQuotes">Adds quotes to values even if they don't need them.</param>
	/// <param name="omitTralingComma">When <see langword="true"/>, the trailing comma will be omitted.</param>
	/// <exception cref="ArgumentNullException">If <paramref name="writer"/> is null.</exception>
	public static void WriteValue(
		TextWriter writer,
		object? value = null,
		bool forceQuotes = false,
		bool omitTralingComma = false)
	{
		if (writer is null) throw new ArgumentNullException(nameof(writer));
		Contract.EndContractBlock();

		writer.Write(CsvUtility.ExportValue(value, forceQuotes, omitTralingComma));
	}

	/// <summary>
	/// Asynchronously writes a CSV formatted value to the <paramref name="writer"/>.
	/// </summary>
	/// <inheritdoc cref="WriteValue(TextWriter, object?, bool, bool)"/>
	public static Task WriteValueAsync(
		TextWriter writer,
		object? value = null,
		bool forceQuotes = false,
		bool omitTrailingComma = false)
	{
		if (writer is null) throw new ArgumentNullException(nameof(writer));
		Contract.EndContractBlock();

		return writer.WriteAsync(CsvUtility.ExportValue(value, forceQuotes, omitTrailingComma));
	}

	/// <summary>
	/// Writes a single CSV formatted row to the <paramref name="writer"/>.
	/// </summary>
	/// <typeparam name="T">The value type.</typeparam>
	/// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
	/// <param name="forceQuotes">Adds quotes to values even if they don't need them.</param>
	/// <param name="omitRowComma">When <see langword="true"/>, the trailing comma at the end of the row (line) will be omitted.</param>
	/// <exception cref="ArgumentNullException">If <paramref name="row"/> is null.</exception>
	public static void WriteRow<T>(TextWriter writer, IEnumerable<T> row, bool forceQuotes = false, bool omitRowComma = false)
	{
		if (writer is null) throw new ArgumentNullException(nameof(writer));
		if (row is null) throw new ArgumentNullException(nameof(row));
		Contract.EndContractBlock();

		if (omitRowComma)
		{
			using var e = row.GetEnumerator();
			if (e.MoveNext())
			{
				var last = e.Current;
				while (e.MoveNext())
				{
					WriteValue(writer, last, forceQuotes);
					last = e.Current;
				}
				WriteValue(writer, last, forceQuotes, true);
			}
		}
		else
		{
			foreach (var e in row)
				WriteValue(writer, e, forceQuotes);
		}

		writer.Write(CsvUtility.NEWLINE);
	}

	/// <inheritdoc cref="WriteRow{T}(TextWriter, IEnumerable{T}, bool, bool)"/>
	public static async ValueTask WriteRowAsync<T>(TextWriter writer, IEnumerable<T> row, bool forceQuotes = false, bool omitRowComma = false)
	{
		if (writer is null) throw new ArgumentNullException(nameof(writer));
		if (row is null) throw new ArgumentNullException(nameof(row));
		Contract.EndContractBlock();

		if (omitRowComma)
		{
			using var e = row.GetEnumerator();
			if (e.MoveNext())
			{
				var last = e.Current;
				while (e.MoveNext())
				{
					await WriteValueAsync(writer, last, forceQuotes).ConfigureAwait(false);
					last = e.Current;
				}
				await WriteValueAsync(writer, last, forceQuotes, true).ConfigureAwait(false);
			}
		}
		else
		{
			foreach (var e in row)
				await WriteValueAsync(writer, e, forceQuotes).ConfigureAwait(false);
		}

		await writer.WriteAsync(CsvUtility.NEWLINE).ConfigureAwait(false);
	}

	/// <summary>
	/// Writes multiple rows to the <paramref name="writer"/>.
	/// </summary>
	/// <typeparam name="T">The value type of each entry.</typeparam>
	/// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
	/// <param name="rows">The enumerable containing the rows.</param>
	/// <param name="forceQuotes">Adds quotes to values even if they don't need them.</param>
	/// <param name="omitRowCommas">When <see langword="true"/>, the trailing comma at the end of each row (line) will be omitted.</param>
	/// <exception cref="ArgumentNullException">If <paramref name="writer"/> is null.</exception>
	public static void WriteRows<T>(
		TextWriter writer,
		IEnumerable<IEnumerable<T>> rows,
		bool forceQuotes = false,
		bool omitRowCommas = false)
	{
		if (writer is null) throw new ArgumentNullException(nameof(writer));
		if (rows is null) throw new ArgumentNullException(nameof(rows));
		Contract.EndContractBlock();

		foreach (var row in rows)
			WriteRow(writer, row, forceQuotes, omitRowCommas);
	}

	/// <summary>
	/// Asynchronously writes multiple rows to the <paramref name="writer"/>.
	/// </summary>
	/// <inheritdoc cref="WriteRows{T}(TextWriter, IEnumerable{IEnumerable{T}}, bool, bool)"/>
	public static async ValueTask WriteRowsAsync<T>(
		TextWriter writer,
		IEnumerable<IEnumerable<T>> rows,
		bool forceQuotes = false,
		bool omitRowCommas = false)
	{
		if (writer is null) throw new ArgumentNullException(nameof(writer));
		if (rows is null) throw new ArgumentNullException(nameof(rows));
		Contract.EndContractBlock();

		foreach (var row in rows)
			await WriteRowAsync(writer, row, forceQuotes, omitRowCommas);
	}

#if NETSTANDARD2_1_OR_GREATER
	/// <inheritdoc cref="WriteRowsAsync{T}(TextWriter, IEnumerable{IEnumerable{T}}, bool, bool)"/>
	public static async ValueTask WriteRowsAsync<T>(
		TextWriter writer,
		IAsyncEnumerable<IEnumerable<T>> rows,
		bool forceQuotes = false,
		bool omitRowCommas = false)
	{
		if (writer is null) throw new ArgumentNullException(nameof(writer));
		if (rows is null) throw new ArgumentNullException(nameof(rows));
		Contract.EndContractBlock();

		// Don't wait for the write to complete before getting the next row.
		var next = new ValueTask();
		await foreach (var row in rows)
		{
			await next.ConfigureAwait(false);
			next = WriteRowAsync(writer, row, forceQuotes, omitRowCommas);
		}

		await next.ConfigureAwait(false);
	}

	/// <summary>
	/// Asynchronously writes multiple rows to the underlying <see cref="TextWriter"/>.
	/// </summary>
	/// <inheritdoc cref="WriteRowsAsync{T}(TextWriter, IEnumerable{IEnumerable{T}}, bool, bool)"/>
	public ValueTask WriteRowsAsync<T>(
		IAsyncEnumerable<IEnumerable<T>> rows,
		bool forceQuotes = false,
		bool omitRowCommas = false)
		=> WriteRowsAsync(Target, rows, forceQuotes, omitRowCommas);
#endif

}
