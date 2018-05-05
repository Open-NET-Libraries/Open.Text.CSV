using Open.Disposable;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace Open.Text.CSV
{
	public class CsvReader : DisposableBase
	{
		public CsvReader(StreamReader source)
		{
			_source = source;
		}

		StreamReader _source;

		int _maxColumns;
		public int MaxColumns => _maxColumns;

		protected override void OnDispose(bool calledExplicitly)
		{
			_source = null; // The intention here is if this object is disposed, then prevent further reading.
		}

		public string[] ReadRow()
		{
			return GetRow(_source, ref _maxColumns);
		}

		public string[][] ReadRows()
		{
			var rows = GetRows(_source, out int maxColumns);
			if (maxColumns > _maxColumns) _maxColumns = maxColumns;
			return rows;
		}

		public static bool TryGetRow(StreamReader source, out string[] row, ref int maxColumns)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			Contract.EndContractBlock();

			if (!source.EndOfStream)
			{
				row = CsvUtility.GetLine(source.ReadLine(), ref maxColumns);
				return true;
			}

			row = null;
			return false;
		}

		public static string[] GetRow(StreamReader source, ref int maxColumns)
		{
			TryGetRow(source, out string[] row, ref maxColumns);
			return row;
		}

		public static string[][] GetRows(StreamReader source, out int maxColumns)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			Contract.EndContractBlock();

			maxColumns = 0;
			var lines = new List<string[]>();
			while (TryGetRow(source, out string[] row, ref maxColumns))
				lines.Add(row);

			return lines.ToArray();
		}

		public static string[][] GetRowsFromFile(string filepath, out int maxColumns)
		{
			if (filepath == null)
				throw new ArgumentNullException(nameof(filepath));
			if (String.IsNullOrWhiteSpace(filepath))
				throw new ArgumentException("Cannot be empty or only whitespace.", nameof(filepath));
			Contract.EndContractBlock();

			if (File.Exists(filepath))
				using (var s = new StreamReader((new FileInfo(filepath)).OpenRead()))
					return GetRows(s, out maxColumns);

			maxColumns = 0;
			return new string[0][];
		}
	}
}
