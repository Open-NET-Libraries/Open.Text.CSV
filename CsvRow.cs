using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Open.Text.CSV
{
	public class CsvRow
	{
		public CsvRow(string[] headerRow)
		{
			_headerRow = headerRow ?? throw new ArgumentNullException(nameof(headerRow));
		}

		readonly string[] _headerRow;

		public string[] HeaderRow => _headerRow.ToArray();

		public string[] GetRow(IDictionary<string, object> values)
		{
			if (values == null) throw new ArgumentNullException(nameof(values));
			Contract.EndContractBlock();

			return _headerRow
				.Select(key => values.TryGetValue(key, out var value) ? CsvUtility.ExportValue(value) : null)
				.ToArray();
		}
	}
}
