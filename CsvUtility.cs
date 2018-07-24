using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Open.Text.CSV
{
	public static class CsvUtility
	{
		public const string LINE_PATTERN = "((?:\")([^\"]+)(?:\")|([^,\"]+))(?:\\s*)(?:,|$)";
		public static readonly Regex LinePattern = new Regex(LINE_PATTERN);
		internal static readonly string[] StringArrayEmpty = new string[0];

		public static string[] GetLine(string line, ref int maxColumns)
		{
			if (line == null)
				return StringArrayEmpty;

			var mc = LinePattern.Matches(line);
			var c = mc.Count;
			if (c > maxColumns)
				maxColumns = c;

			var result = new string[c];
			for (var i = 0; i < c; i++)
				result[i] = mc[i].Groups[1].Value.Trim('"');

			return result;
		}

		public static string[][] GetArray(string csv, out int maxColumns)
		{
			maxColumns = 0;
			var lines = csv == null ? new string[0] : csv.Split('\n');

			var result = new List<string[]>(lines.Length);
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var line in lines)
				result.Add(GetLine(line, ref maxColumns));

			return result.ToArray();
		}

		public static string[][] GetArray(string csv)
		{
			return GetArray(csv, out _);
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
