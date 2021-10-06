using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Open.Text.CSV.Excel;

public static class ExcelCsvExtensions
{
	public static void WriteCsvExcelValue(this TextWriter writer, string value)
	{
		if (writer is null) throw new NullReferenceException();
		Contract.EndContractBlock();

		CsvWriter.WriteValue(writer, '=' + CsvUtility.WrapQuotes(value));
	}

	public static void WriteCsvExcelHyperlink(this TextWriter writer, Uri link, string? text = null)
	{
		if (writer is null) throw new NullReferenceException();
		if (link is null) throw new ArgumentNullException(nameof(link));
		Contract.EndContractBlock();

		writer.WriteCsvExcelHyperlink(link.ToString(), text);
	}

	public static void WriteCsvExcelHyperlink(this TextWriter writer, string link, string? text = null)
	{
		if (writer is null) throw new NullReferenceException();
		Contract.EndContractBlock();

		WriteCsvExcelValue(writer, $"=HYPERLINK({CsvUtility.WrapQuotes(link)}{(text is null ? string.Empty : (',' + CsvUtility.WrapQuotes(text)))})");
	}

	public static void WriteExcelValue(this CsvWriter writer, string value)
	{
		if (writer is null) throw new NullReferenceException();
		Contract.EndContractBlock();

		CsvWriter.WriteValue(writer.Target, '=' + CsvUtility.WrapQuotes(value));
	}

	public static void WriteExcelHyperlink(this CsvWriter writer, Uri link, string? text = null)
	{
		if (writer is null) throw new NullReferenceException();
		if (link is null) throw new ArgumentNullException(nameof(link));
		Contract.EndContractBlock();

		WriteExcelHyperlink(writer, link.ToString(), text);
	}

	public static void WriteExcelHyperlink(this CsvWriter writer, string link, string? text = null)
	{
		if (writer is null) throw new NullReferenceException();
		Contract.EndContractBlock();

		WriteCsvExcelHyperlink(writer.Target, link, text);
	}
}
