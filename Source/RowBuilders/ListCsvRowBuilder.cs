using Microsoft.Toolkit.HighPerformance.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Open.Text.CSV;

/// <summary>
/// Receives characters in a CSV sequence and translates them into values in a row.
/// </summary>
public sealed class ListCsvRowBuilder : CsvRowBuilderBase<IList<string>>
{
	List<string>? _fields;
	readonly StringPool _stringPool = new();
	readonly StringBuilder _fb = new();

	public ListCsvRowBuilder()
	{
	}

	/// <inheritdoc />
	public override void Reset()
	{
		_fields = null;
		base.Reset();
	}

	protected override void ResetFieldBuffer() => _fb.Clear();

	protected override void AddNextChar(in char c, bool ws = false)
	{
		_fb.Append(c);
		if (!ws) FieldLen = _fb.Length;
	}

	protected override void AddEntry()
	{
		_fields ??= new List<string>(MaxFields);
		if (FieldLen == 0)
		{
			_fields.Add(string.Empty);
			if (_fb.Length != 0) _fb.Clear();
			return;
		}

		if (FieldLen < _fb.Length) _fb.Length = FieldLen;
		_fields.Add(_stringPool.GetOrAdd(_fb.ToString()));
		_fb.Clear();
		FieldLen = 0;
	}

	protected override bool Complete(
#if NULL_ANALYSIS
	[NotNullWhen(true)]
#endif
	out IList<string>? row)
	{
		var f = _fields;
		Reset();
		if (f is null)
		{
			row = default;
			return false;
		}

		var count = f.Count;
		if (count == 0)
		{
			row = default;
			return false;
		}

		if (count > MaxFields) MaxFields = count;

		row = f;
		return true;
	}

}
