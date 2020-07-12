using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace Open.Text.CSV
{
	public class CsvWriter : IDisposable
	{
		TextWriter? _target;
		internal TextWriter Target => _target ?? throw new ObjectDisposedException(GetType().ToString());

		public CsvWriter(TextWriter target)
		{
			_target = target ?? throw new ArgumentNullException(nameof(target));
		}

		public void Dispose()
		{
			_target = null; // The intention here is if this object is disposed, then prevent further writing.
		}

		public void WriteRow<T>(IEnumerable<T> row, bool forceQuotes = false)
		{
			WriteRow(Target, row, forceQuotes);
		}

		public void WriteRows<T>(IEnumerable<IEnumerable<T>> rows, bool forceQuotes = false)
		{
			WriteRows(Target, rows, forceQuotes);
		}

		public static void WriteValue(TextWriter writer, object? value = null, bool forceQuotes = false)
		{
			if (writer is null) throw new ArgumentNullException(nameof(writer));
			Contract.EndContractBlock();

			writer.Write(CsvUtility.ExportValue(value, forceQuotes));
		}

		public static void WriteRow<T>(TextWriter writer, IEnumerable<T> row, bool forceQuotes = false)
		{
			if (writer is null) throw new ArgumentNullException(nameof(writer));
			if (row is null) throw new ArgumentNullException(nameof(row));
			Contract.EndContractBlock();

			foreach (var o in row)
				WriteValue(writer, o, forceQuotes);

			writer.WriteLineNoTabs();
		}

		public static void WriteRows<T>(TextWriter writer, IEnumerable<IEnumerable<T>> rows, bool forceQuotes = false)
		{
			if (writer is null) throw new ArgumentNullException(nameof(writer));
			if (rows is null) throw new ArgumentNullException(nameof(rows));
			Contract.EndContractBlock();

			foreach (var row in rows)
				WriteRow(writer, row, forceQuotes);
		}
	}
}
