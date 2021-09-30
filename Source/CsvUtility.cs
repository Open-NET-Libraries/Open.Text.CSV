using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Open.Text.CSV;

public static class CsvUtility
{
	public const string NEWLINE = "\r\n";
	public static readonly Regex QUOTESNEEDED = new("^\\s+|[,\r\n]|\\s+$");

	public static string WrapQuotes(string value)
	{
		if (value is null)
			return string.Empty;
#if NETSTANDARD2_1_OR_GREATER
			return '"' + value.Replace("\"", "\"\"", StringComparison.Ordinal) + '"';
#else
		return '"' + value.Replace("\"", "\"\"") + '"';
#endif
	}

	public static string FormatValue(string value, bool forceQuotes = false)
	{
		if (value is null)
			return string.Empty;

		if (!string.IsNullOrEmpty(value) && forceQuotes || QUOTESNEEDED.IsMatch(value))
			return WrapQuotes(value);

		return value;
	}

	public static string ExportValue(object? value, bool forceQuotes = false)
		=> FormatValue(value switch
		{
			DateTime datetime => datetime.TimeOfDay == TimeSpan.Zero ?
				datetime.ToString("d", CultureInfo.InvariantCulture) : // Use short date.
				datetime.ToString(CultureInfo.InvariantCulture),
			_ => value is null ? string.Empty : value.ToString(),
		}, forceQuotes) + ",";

}
