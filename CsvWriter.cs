using Open.Disposable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Open.Text.CSV
{
	public class CsvWriter : DisposableBase
	{
		internal TextWriter Target;

		public CsvWriter(TextWriter target)
		{
			Target = target;
		}

		protected override void OnDispose(bool calledExplicitly)
		{
			Target = null; // The intention here is if this object is disposed, then prevent further writing.
		}

		public void WriteRow(object[] row, bool forceQuotes = false)
		{
			WriteRow(Target, row, forceQuotes);
		}

		public void WriteRows(object[][] rows, bool forceQuotes = false)
		{
			WriteRows(Target, rows, forceQuotes);
		}

		public static void WriteValue(TextWriter writer, object value = null, bool forceQuotes = false)
		{
			writer.Write(CsvUtility.ExportValue(value, forceQuotes));
		}

		public static void WriteRow(TextWriter writer, object[] row, bool forceQuotes = false)
		{
			if (row == null) throw new ArgumentNullException("row");

			foreach (var o in row)
				WriteValue(writer, o, forceQuotes);

			writer.WriteLineNoTabs();
		}

		public static void WriteRows(TextWriter writer, object[][] rows, bool forceQuotes = false)
		{
			if (rows == null) throw new ArgumentNullException("rows");

			foreach (var row in rows)
				WriteRow(writer, row, forceQuotes);
		}
	}
}
