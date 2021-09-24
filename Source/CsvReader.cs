using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;

namespace Open.Text.CSV
{
	public class CsvReader : CsvReaderBase
	{
		public CsvReader(TextReader source) : base(source)
		{
			_rowBuilder = new(row => _nextRow = row);
		}

		readonly CsvRowBuilder _rowBuilder;

		List<string>? _nextRow;

		public bool ReadNextRow(out IList<string>? rowBuffer)
		{
			var s = Source;
			int c;

		loop:
			c = s.Read();

			if (c == -1)
			{
				var rowReady = _rowBuilder.EndRow();
				rowBuffer = _nextRow;
				return rowReady;
			}

			if (_rowBuilder.AddChar(c))
			{
				rowBuffer = _nextRow;
				return true;

			}

			goto loop;
		}

		public IList<string>? ReadNextRow()
		{
			var result = ReadNextRow(out var list);
			Debug.Assert(result == (list is not null));
			return list;
		}

		public IEnumerable<IList<string>> ReadRows()
		{
			while (ReadNextRow(out var rowBuffer))
			{
				Debug.Assert(rowBuffer is not null);
				yield return rowBuffer!;
			}
		}

		public static List<IList<string>> GetRowsFromFile(string filepath)
		{
			if (filepath is null)
				throw new ArgumentNullException(nameof(filepath));
			if (string.IsNullOrWhiteSpace(filepath))
				throw new ArgumentException("Cannot be empty or only whitespace.", nameof(filepath));
			Contract.EndContractBlock();

			using var fs = new FileInfo(filepath).OpenRead();
			using var sr = new StreamReader(fs);
			using var csv = new CsvReader(sr);
			var list = new List<IList<string>>();
			foreach (var row in csv.ReadRows()) list.Add(row);
			return list;
		}

		public static List<IList<string>> GetRowsFromText(string csvText)
		{
			if (csvText is null)
				throw new ArgumentNullException(nameof(csvText));
			Contract.EndContractBlock();

			using var sr = new StringReader(csvText);
			using var csv = new CsvReader(sr);
			var list = new List<IList<string>>();
			foreach (var row in csv.ReadRows()) list.Add(row);
			return list;
		}
	}
}
