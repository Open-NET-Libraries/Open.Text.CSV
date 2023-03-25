using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Open.Text.CSV;

public static class CsvUtility
{
	public const string NEWLINE = "\r\n";
	public static readonly Regex QUOTESNEEDED = new(@"^\s+|["",\r\n]|\s+$", RegexOptions.Compiled);

	/// <summary>
	/// Wraps a string in quotes and escapes any quotes within the string.
	/// </summary>
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

	/// <summary>
	/// Formats a value for CSV export.
	/// </summary>
	public static string FormatValue(
		string value,
		bool forceQuotes = false)
		=> string.IsNullOrEmpty(value)
		? string.Empty
		: forceQuotes || QUOTESNEEDED.IsMatch(value)
		? WrapQuotes(value)
		: value;

	/// <summary>
	/// Formats a CSV value. Adds a comma to the end of the string unless <paramref name="omitComma"/> is true.
	/// </summary>
	public static string ExportValue(
		object? value,
		bool forceQuotes = false,
		bool omitComma = false)
	{
		if (value is null)
			return omitComma ? string.Empty : ",";

		var v = value switch
		{
			string s => s,
			DateTime datetime => datetime.TimeOfDay == TimeSpan.Zero ?
				datetime.ToString("d", CultureInfo.InvariantCulture) : // Use short date.
				datetime.ToString(CultureInfo.InvariantCulture),
			_ => value.ToString(),
		};

		var formatted = FormatValue(v!, forceQuotes);
		return omitComma ? formatted : $"{formatted},";
	}
}
