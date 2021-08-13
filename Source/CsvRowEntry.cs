using System;
using System.Collections.Generic;
using System.Text;

namespace Open.Text.CSV
{
	public struct CsvRowEntry
	{
		public CsvRowEntry(int index, int start, int length, ReadOnlyMemory<char> value)
		{
			Index = index;
			Start = start;
			Length = length;
			Value = value;
		}

		public CsvRowEntry(int index, int start, ReadOnlyMemory<char> value)
			: this(index, start, value.Length, value)
		{
		}
		public int Index { get; }
		public int Start { get; }
		public int Length { get; }
		public int End => Start + Length;
		public ReadOnlyMemory<char> Value { get; }

		public override string ToString() => Value.ToString();

		public static implicit operator ReadOnlyMemory<char>(CsvRowEntry e) => e.Value;
		public static implicit operator ReadOnlySpan<char>(CsvRowEntry e) => e.Value.Span;
		public static implicit operator string(CsvRowEntry e) => e.ToString();
	}

}
