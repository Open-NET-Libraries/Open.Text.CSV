using System;
using System.IO;

namespace Open.Text.CSV.Excel
{
	public static class ExcelCsvExtensions
	{
		public static void WriteCsvExcelValue(this TextWriter writer, string value)
		{
			CsvWriter.WriteValue(writer, "=" + CsvUtility.WrapQuotes(value));
		}

		public static void WriteCsvExcelHyperlink(this TextWriter writer, Uri link, string text = null)
		{
			if (link == null) throw new ArgumentNullException("link");

			writer.WriteCsvExcelHyperlink(link.ToString(), text);
		}

		public static void WriteCsvExcelHyperlink(this TextWriter writer, string link, string text = null)
		{
			WriteCsvExcelValue(writer, "=HYPERLINK(" + CsvUtility.WrapQuotes(link) + (text == null ? String.Empty : ("," + CsvUtility.WrapQuotes(text))) + ")");
		}

		public static void WriteExcelValue(this CsvWriter writer, string value)
		{
			CsvWriter.WriteValue(writer.Target, "=" + CsvUtility.WrapQuotes(value));
		}

		public static void WriteExcelHyperlink(this CsvWriter writer, Uri link, string text = null)
		{
			if (link == null) throw new ArgumentNullException("link");

			WriteExcelHyperlink(writer, link.ToString(), text);
		}

		public static void WriteExcelHyperlink(this CsvWriter writer, string link, string text = null)
		{
			WriteCsvExcelHyperlink(writer.Target, link, text);
		}
	}
}
