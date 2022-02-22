using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Open.Text.CSV;

public class CsvRowTransformer
{
	public CsvRowTransformer(IReadOnlyList<string> headerRow)
	{
		HeaderRow = headerRow;
	}

	public IReadOnlyList<string> HeaderRow { get; }

	public IEnumerable<object?> GetRow(IDictionary<string, object> values)
	{
		if (values is null) throw new ArgumentNullException(nameof(values));
		Contract.EndContractBlock();

		return GetRow(key => values.TryGetValue(key, out var value) ? value : null);
	}

	public IEnumerable<object?> GetRow(Func<string, object?> values)
	{
		return values is null
			? throw new ArgumentNullException(nameof(values))
			: GetRowCore();

		IEnumerable<object?> GetRowCore()
		{
			Contract.EndContractBlock();

			var len = HeaderRow.Count;
			for (var i = 0; i < len; i++)
			{
				yield return values(HeaderRow[i]);
			}
		}
	}
}
