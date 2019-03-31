using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Open.Text.CSV
{
	public static class CsvUtility
	{
		public const string LINE_PATTERN = "((?:\")([^\"]+)(?:\")|([^,\"]+))(?:\\s*)(?:,|$)";
		public static readonly Regex LinePattern = new Regex(LINE_PATTERN);

		public static IEnumerable<string> GetLine(string line)
			=> string.IsNullOrEmpty(line)
				? Enumerable.Empty<string>()
				: GetLineCore(line);

		static IEnumerable<string> GetLineCore(string line)
		{
			var mc = LinePattern.Matches(line);
			var c = mc.Count;

			for (var i = 0; i < c; i++)
				yield return mc[i].Groups[1].Value.Trim('"');
		}

		//static string[] GetLineCore(in ReadOnlySpan<char> span)
		//{
		//	if (span.IsEmpty)
		//		return Array.Empty<string>();

		//	var line = span.ToString();
		//	var mc = LinePattern.Matches(line);
		//	var c = mc.Count;

		//	var result = new string[c];
		//	for (var i = 0; i < c; i++)
		//	{
		//		var g = mc[i].Groups[1];
		//		var s = span.Slice(g.Index, g.Length).Trim('"');
		//		result[i] = s.ToString();
		//	}

		//	return result;
		//}

		public static IEnumerable<IEnumerable<string>> GetLines(string csv)
			=> string.IsNullOrEmpty(csv)
				? Enumerable.Empty<IEnumerable<string>>()
				: GetLinesCore(csv);

		static IEnumerable<IEnumerable<string>> GetLinesCore(string csv)
		{
			foreach(var line in csv.SplitAsEnumerable('\n'))
				yield return GetLineCore(line);
		}

		public const string NEWLINE = "\r\n";
		public static readonly Regex QUOTESNEEDED = new Regex("^\\s+|[,\n]|\\s+$");

		public static string WrapQuotes(string value)
		{
			if (value == null)
				return string.Empty;

			return '"' + value.Replace("\"", "\"\"") + '"';
		}

		public static string FormatValue(string value, bool forceQuotes = false)
		{
			if (value == null)
				return string.Empty;

			if (!string.IsNullOrEmpty(value) && forceQuotes || QUOTESNEEDED.IsMatch(value))
				return WrapQuotes(value);

			return value;
		}

		public static string ExportValue(object value, bool forceQuotes = false)
		{
			var result = string.Empty;
			switch (value)
			{
				case null:
					return FormatValue(result, forceQuotes) + ",";
				case DateTime datetime:
					result = datetime.TimeOfDay == TimeSpan.Zero ?
						datetime.ToString("d") : // Use short date.
						datetime.ToString(CultureInfo.InvariantCulture);
					break;
				default:
					result = value.ToString();
					break;
			}

			return FormatValue(result, forceQuotes) + ",";
		}

	}
}
