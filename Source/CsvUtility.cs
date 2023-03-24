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
#if NETSTANDARD2_0
		return '"' + value.Replace("\"", "\"\"") + '"';
#else
		return '"' + value.Replace("\"", "\"\"", StringComparison.Ordinal) + '"';
#endif
	}

	public static string FormatValue(string value, bool forceQuotes = false)
		=> string.IsNullOrEmpty(value)
		? string.Empty
		: forceQuotes || QUOTESNEEDED.IsMatch(value)
		? WrapQuotes(value)
		: value;

	public static string ExportValue(object? value, bool forceQuotes = false, bool lastElement = false)
	{
		if (value is null) return ",";
		var v = value switch
		{
			DateTime datetime => datetime.TimeOfDay == TimeSpan.Zero ?
				datetime.ToString("d", CultureInfo.InvariantCulture) : // Use short date.
				datetime.ToString(CultureInfo.InvariantCulture),
			_ => value.ToString(),
		};

		return $"{FormatValue(v!, forceQuotes)}{(lastElement?"":",")}";
	}
}
