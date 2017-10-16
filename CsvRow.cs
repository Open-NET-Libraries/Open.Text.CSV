using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Open.Text.CSV
{
	public class CsvRow
	{
		public CsvRow(string[] headerRow)
		{
			_headerRow = headerRow ?? throw new ArgumentNullException("headerRow");
		}

		string[] _headerRow;

		public string[] HeaderRow => _headerRow.ToArray();

		public string[] GetRow(IDictionary<string, object> values)
		{
			if(values==null) throw new ArgumentNullException("values");
			return _headerRow
				.Select(key => values.TryGetValue(key, out object value) ? CsvUtility.ExportValue(value) : null)
				.ToArray();
		}
    }
}
