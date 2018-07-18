﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Open.Text.CSV
{
	public static partial class CsvUtility
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
			foreach (var line in lines)
				result.Add(GetLine(line, ref maxColumns));

			return result.ToArray();
		}

		public static string[][] GetArray(string csv)
		{
			return GetArray(csv, out var maxColumns);
		}


		public const string NEWLINE = "\r\n";
		public readonly static Regex QUOTESNEEDED = new Regex("^\\s+|[,\n]|\\s+$");

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
			if (value != null)// && value != DBNull.Value)
			{
				if (value is DateTime datetime)
				{
					result = datetime.TimeOfDay == TimeSpan.Zero ?
						datetime.ToString("d") : // Use short date.
						datetime.ToString();
				}
				else
					result = value.ToString();
			}
			return FormatValue(result, forceQuotes) + ",";
		}

	}
}
