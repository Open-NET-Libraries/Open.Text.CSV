using System;
using System.Collections.Generic;
using System.Text;

/*
 * Awaiting System.Data support...
 */

namespace Open.Text.CSV
{
    public static class CsvData
    {

		public static DataTable GetDataTable(string csv, bool firstRowIsHeader = false)
		{
			string[][] data = CsvUtility.GetArray(csv, out int maxColumns);

			var t = new DataTable();

			if (data.Any())
			{
				if (firstRowIsHeader)
				{
					var firstRow = data.First();
					for (int i = 0; i < maxColumns; i++)
						t.Columns.Add(firstRow[i], typeof(string));
				}
				else
				{
					for (int i = 0; i < maxColumns; i++)
						t.Columns.Add("Column" + i.ToString("00"), typeof(string));
				}

				for (int n = firstRowIsHeader ? 1 : 0; n < data.Length; n++)
				{
					var s = data[n];
					DataRow r = t.NewRow();
					for (int i = 0; i < s.Length; i++)
						r["Column" + i.ToString("00")] = s[i];
					t.Rows.Add(r);
				}
			}

			return t;
		}

		// NOTE: ID as a first column first row item without quotes will cause Excel to fail loading the file.

		public static void WriteCsvHeaderRow(TextWriter writer, DataTable t)
		{
			if (t == null) throw new ArgumentNullException("t");

			foreach (DataColumn c in t.Columns)
			{
				CsvWriter.WriteCsvValue(writer, c.ColumnName, c.Ordinal == 0 && c.ColumnName == "ID");
			}
			writer.WriteLineNoTabs();
		}

		public static void WriteCsvHeaderRow(TextWriter writer, IDataReader reader)
		{
			if (reader == null) throw new ArgumentNullException("reader");

			int fc = reader.FieldCount;
			for (int i = 0; i < fc; i++)
			{
				var name = reader.GetName(i);
				CsvWriter.WriteValue(writer, name, i == 0 && name == "ID");
			}
			writer.WriteLineNoTabs();
		}

		public static void WriteCsvRows(TextWriter writer, IEnumerable<DataRow> rows)
		{
			if (rows == null) throw new ArgumentNullException("rows");

			foreach (DataRow row in rows)
			{
				foreach (DataColumn c in row.Table.Columns)
				{
					CsvWriter.WriteValue(writer, row[c]);
				}
				writer.WriteLineNoTabs();
			}
		}

		public static void WriteCsvRows(TextWriter writer, IDataReader reader)
		{
			if (reader == null) throw new ArgumentNullException("reader");

			var fc = reader.FieldCount;
			while (reader.Read())
			{
				for (int i = 0; i < fc; i++)
					CsvWriter.WriteValue(writer, reader.IsDBNull(i) ? null : reader.GetValue(i));
				writer.WriteLineNoTabs();
			}
		}

		public static void WriteCsv(TextWriter writer, DataTable t, string sort = null)
		{
			if (t == null) throw new ArgumentNullException("t");

			WriteCsvHeaderRow(writer, t);
			writer.WriteCsvRows(writer, t.Select(null, sort));
		}
	}
}
