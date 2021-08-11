using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Open.Text.CSV
{
	public class CsvRow
	{
		public CsvRow(in ReadOnlyMemory<string> headerRow)
		{
			_headerRow = headerRow;
		}

		readonly ReadOnlyMemory<string> _headerRow;

		public ReadOnlySpan<string> HeaderRow => _headerRow.Span;

		public IEnumerable<string?> GetRow(IDictionary<string, object> values)
		{
			if (values is null) throw new ArgumentNullException(nameof(values));
			Contract.EndContractBlock();

			var len = _headerRow.Length;
			var header = _headerRow.Span;

			for (var i = 0; i < len; i++)
			{
				yield return values.TryGetValue(header[i], out var value) ? CsvUtility.ExportValue(value) : null;
			}
		}
	}
}
