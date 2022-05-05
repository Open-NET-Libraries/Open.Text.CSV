using System;
using System.Diagnostics.CodeAnalysis;

namespace Open.Text.CSV;

public interface ICsvRowBuilder<TRow>
{
	int MaxFields { get; }

	bool Add(in ArraySegment<char> chars, out ArraySegment<char> remaining,
#if NULL_ANALYSIS
	[NotNullWhen(true)]
#endif
	out TRow? row);
	bool Add(ReadOnlyMemory<char> chars, out ReadOnlyMemory<char> remaining,
#if NULL_ANALYSIS
	[NotNullWhen(true)]
#endif
	out TRow? row);
	bool Add(ReadOnlySpan<char> chars, out ReadOnlySpan<char> remaining,
#if NULL_ANALYSIS
	[NotNullWhen(true)]
#endif
	out TRow? row);
	bool Add(string chars, out ReadOnlySpan<char> remaining,
#if NULL_ANALYSIS
	[NotNullWhen(true)]
#endif
	out TRow? row);
	bool EndRow(
#if NULL_ANALYSIS
	[NotNullWhen(true)]
#endif
	out TRow? row);
	void Reset();
}