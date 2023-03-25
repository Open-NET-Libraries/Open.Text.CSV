using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;

namespace Open.Text.CSV;

public class CsvWriter : IDisposable
{
	TextWriter? _target;
	internal TextWriter Target => _target ?? throw new ObjectDisposedException(GetType().ToString());

	public CsvWriter(TextWriter target)
	{
		_target = target ?? throw new ArgumentNullException(nameof(target));
	}

	public void Dispose() => _target = null; // The intention here is if this object is disposed, then prevent further writing.

	public void WriteRow<T>(IEnumerable<T> row, bool forceQuotes = false, bool omitLastComma = false)
		=> WriteRow(Target, row, forceQuotes, omitLastComma);

	public void WriteRows<T>(IEnumerable<IEnumerable<T>> rows, bool forceQuotes = false, bool omitLastComma = false)
		=> WriteRows(Target, rows, forceQuotes, omitLastComma);

	public static void WriteValue(TextWriter writer, object? value = null, bool forceQuotes = false, bool lastElement = false)
	{
		if (writer is null) throw new ArgumentNullException(nameof(writer));
		Contract.EndContractBlock();

		writer.Write(CsvUtility.ExportValue(value, forceQuotes, lastElement));
	}

	public static Task WriteValueAsync(TextWriter writer, object? value = null, bool forceQuotes = false, bool lastElement = false)
	{
		if (writer is null) throw new ArgumentNullException(nameof(writer));
		Contract.EndContractBlock();

		return writer.WriteAsync(CsvUtility.ExportValue(value, forceQuotes, lastElement));
	}

	public static void WriteRow<T>(TextWriter writer, IEnumerable<T> row, bool forceQuotes = false, bool omitLastComma = false)
	{
		if (writer is null) throw new ArgumentNullException(nameof(writer));
		if (row is null) throw new ArgumentNullException(nameof(row));
		Contract.EndContractBlock();

		T last = default(T);
		bool firstElem = true;
		foreach (var o in row)
		{
			if (!firstElem)
				WriteValue(writer, last, forceQuotes);
			else
				firstElem = false;

			last = o;
		}
		WriteValue(writer, last, forceQuotes, omitLastComma);

		writer.Write(CsvUtility.NEWLINE);
	}

	public static async ValueTask WriteRowAsync<T>(TextWriter writer, IEnumerable<T> row, bool forceQuotes = false, bool omitLastComma = false)
	{
		if (writer is null) throw new ArgumentNullException(nameof(writer));
		if (row is null) throw new ArgumentNullException(nameof(row));
		Contract.EndContractBlock();

		T last = default(T);
		bool firstElem = true;
		foreach (var o in row)
		{ 
			if (!firstElem)
				await WriteValueAsync(writer, o, forceQuotes).ConfigureAwait(false);
			else
				firstElem = false;

			last = o;
		}
		await WriteValueAsync(writer, last, forceQuotes, omitLastComma).ConfigureAwait(false);

		await writer.WriteAsync(CsvUtility.NEWLINE).ConfigureAwait(false);
	}

	public static void WriteRows<T>(TextWriter writer, IEnumerable<IEnumerable<T>> rows, bool forceQuotes = false, bool omitLastComma = false)
	{
		if (writer is null) throw new ArgumentNullException(nameof(writer));
		if (rows is null) throw new ArgumentNullException(nameof(rows));
		Contract.EndContractBlock();

		foreach (var row in rows)
			WriteRow(writer, row, forceQuotes, omitLastComma);
	}

#if NETSTANDARD2_1_OR_GREATER
		public static async ValueTask WriteRowsAsync<T>(TextWriter writer, IAsyncEnumerable<IEnumerable<T>> rows, bool forceQuotes = false, bool omitLastComma = false)
		{
			if (writer is null) throw new ArgumentNullException(nameof(writer));
			if (rows is null) throw new ArgumentNullException(nameof(rows));
			Contract.EndContractBlock();

			// Don't wait for the write to complete before getting the next row.
			var next = new ValueTask();
			await foreach (var row in rows)
			{
				await next.ConfigureAwait(false);
				next = WriteRowAsync(writer, row, forceQuotes, omitLastComma);
			}

			await next.ConfigureAwait(false);
		}
#endif

}
