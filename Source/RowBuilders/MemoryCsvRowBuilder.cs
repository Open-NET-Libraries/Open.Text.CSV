using Microsoft.Toolkit.HighPerformance.Buffers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Open.Text.CSV;

/// <summary>
/// Receives characters in a CSV sequence and translates them into values in a row.
/// </summary>
public sealed class MemoryCsvRowBuilder
	: CsvRowBuilderBase<IMemoryOwner<string>>, IDisposable, ICsvRowBuilder<IList<string>>
{
	readonly StringPool _stringPool = new();
	readonly ExpandableMemory<string> _fields = new();
	readonly StringBuilder _fb = new();

	IList<string>? ICsvRowBuilder<IList<string>>.LatestCompleteRow => LatestCompleteRow?.Memory.ToArray();

	/// <inheritdoc />
	public override void Reset()
	{
		_fields.Clear();
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

	protected override bool Complete()
	{
		try
		{
			var count = _fields.Length;
			if (count == 0) return false;
			if (count > MaxFields) MaxFields = count;
			LatestCompleteRow = _fields.ExtractOwner();
			return true;
		}
		finally
		{
			Reset();
		}
	}

	public void Dispose() => _fields.Dispose();
}
