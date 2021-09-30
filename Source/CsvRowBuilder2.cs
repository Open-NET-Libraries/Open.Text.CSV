using Microsoft.Toolkit.HighPerformance.Buffers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Open.Text.CSV;

/// <summary>
/// Receives characters in a CSV sequence and translates them into values in a row.
/// </summary>
public sealed class CsvRowBuilder2 : CsvRowBuilderBase<IMemoryOwner<string>>
{
	MemoryOwner<string>? _fields;
	readonly StringPool _stringPool = new();
	readonly StringBuilder _fb = new();
	int _fieldCount = 0;

	public CsvRowBuilder2(int maxFields)
	{
		MaxFields = maxFields;
	}

	/// <inheritdoc />
	public override void Reset()
	{
		_fieldCount = 0;
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
		_fields ??= MemoryOwner<string>.Allocate(MaxFields);

		if (FieldLen == 0)
		{
			_fields.Memory.Span[_fieldCount++] = string.Empty;
			if (_fb.Length != 0) _fb.Clear();
			return;
		}

		if (FieldLen < _fb.Length) _fb.Length = FieldLen;
		_fields.Memory.Span[_fieldCount++] = _stringPool.GetOrAdd(_fb.ToString());
		_fb.Clear();
		FieldLen = 0;
	}

	protected override bool Complete()
	{
		var f = _fields;
		var count = _fieldCount;
		Reset();
		if (f is null || count==0) return false;

		LatestCompleteRow = f.Slice(0, count);
		return true;
	}

}
