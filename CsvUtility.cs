using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Open.Text.CSV
{
	public static class CsvUtility
	{
		private enum State
		{
			BeforeField,
			InField,
			InQuotedField,
			Quote,
			AfterQuotedField,
		}

		/// <summary>
		/// Returns the raw entries including quotes from the line provided.
		/// </summary>
		/// <param name="row">The row to parse.</param>
		/// <param name="buffer">An optional buffer to use if parsing synchronously.</param>
		public static IEnumerable<CsvRowEntry> ReadRow(ReadOnlyMemory<char> row, List<char>? buffer = null)
		{
			if (row.IsEmpty) yield break;

			var len = row.Length;
			var state = State.BeforeField;
			var fS = 0;
			var fE = 0;
			var entry = 0;

			CsvRowEntry GetEntry() => new(entry++, fS, row.Slice(fS, fE - fS));

			CsvRowEntry GetBufferedEntry()
			{
				var len = fE - fS;
				var result = len == buffer!.Count ? GetEntry() : new(entry++, fS, len, buffer!.ToArray());
				buffer.Clear();
				return result;
			}

			try
			{

				for (var i = 0; i < len; i++)
				{
					var c = row.Span[i];
					switch (state)
					{
						case State.BeforeField:
							switch (c)
							{
								case ' ':
								case '\t':
									break; // ignore leading white-space.

								case '"':
									state = State.InQuotedField;
									if (buffer is null) buffer = new List<char>();
									else if (buffer.Count != 0) buffer.Clear();
									fS = i + 1;
									fE = fS;
									break;

								case ',':
									fE = fS = i;
									yield return GetEntry();
									break;

								case '\r':
								case '\n':
									yield break;

								default:
									state = State.InField;
									fS = i;
									fE = i + 1;
									break;
							}
							break;

						case State.InField:
							switch (c)
							{
								case ' ':
								case '\t':
									break;

								case ',':
									yield return GetEntry();
									state = State.BeforeField;
									break;

								case '\r':
								case '\n':
									yield return GetEntry();
									yield break;

								default:
									fE = i + 1;
									break;
							}
							break;

						case State.InQuotedField:
							if (c == '"')
							{
								state = State.Quote;
							}
							else
							{
								buffer!.Add(c);
								fE = i + 1;
							}
							break;

						case State.Quote:
							switch (c)
							{
								case ' ':
								case '\t':
									yield return GetBufferedEntry();
									state = State.AfterQuotedField;
									break;

								case '"':
									buffer!.Add('"');
									state = State.InQuotedField;
									fE = i + 1;
									break;

								case ',':
									yield return GetBufferedEntry();
									state = State.BeforeField;
									break;

								case '\r':
								case '\n':
									yield return GetBufferedEntry();
									yield break;

								default:
									throw new InvalidDataException($"Corrupt field found starting at {fS}. A double quote at character {i - 1} is not escaped or there is extra data after a quoted field.");
							}
							break;

						case State.AfterQuotedField:
							switch (c)
							{
								case ' ':
								case '\t':
									break;

								case ',':
									state = State.BeforeField;
									break;

								case '\r':
								case '\n':
									yield break;

								default:
									throw new InvalidDataException($"Corrupt field found starting at {fS}. A double quote is not escaped or there is extra data after a quoted field.");
							}
							break;
					}
				}

				switch (state)
				{
					case State.InField:
						yield return GetEntry();
						break;

					case State.Quote:
						yield return GetBufferedEntry();
						break;

					case State.InQuotedField:
						throw new InvalidDataException("When the line ends with a quoted field, the last character should be an unescaped double quote.");
				}

			}
			finally
			{
				buffer?.Clear();
			}
		}
		public static IEnumerable<CsvRowEntry> ReadRow(string row)
			=> string.IsNullOrWhiteSpace(row)
				? Enumerable.Empty<CsvRowEntry>()
				: ReadRow(row.AsMemory());

		public static IEnumerable<CsvRowEntry[]> GetRows(string csv)
			=> string.IsNullOrWhiteSpace(csv)
				? Enumerable.Empty<CsvRowEntry[]>()
				: GetRowsCore(csv);
		static IEnumerable<CsvRowEntry[]> GetRowsCore(string csv)
		{
			var memory = csv.AsMemory();
			var buffer = new List<char>();

		loop:

			var line = ReadRow(memory, buffer).ToArray();
			yield return line;
			var last = line[line.Length - 1];
			var end = last.End;
			if (end == memory.Length) yield break;
			var next = end + 1;
			if (next < memory.Length)
			{
				var span = memory.Span;
				if (span[end] == '\r' && span[next] == '\n') end = next;
			}
			end++;
			if (end == memory.Length) yield break;
			memory = memory.Slice(end);

			goto loop;
		}

		public const string NEWLINE = "\r\n";
		public static readonly Regex QUOTESNEEDED = new("^\\s+|[,\r\n]|\\s+$");

		public static string WrapQuotes(string value)
		{
			if (value is null)
				return string.Empty;
#if NETSTANDARD2_1
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
}
