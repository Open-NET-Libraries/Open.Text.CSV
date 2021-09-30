using System;

namespace Open.Text.CSV;

public interface ICsvRowBuilder<TRow>
{
	TRow? LatestCompleteRow { get; }
	int MaxFields { get; }

	bool Add(in ArraySegment<char> chars, out ArraySegment<char> remaining);
	bool Add(in ReadOnlyMemory<char> chars, out ReadOnlyMemory<char> remaining);
	bool Add(in ReadOnlySpan<char> chars, out ReadOnlySpan<char> remaining);
	bool Add(string chars, out ReadOnlySpan<char> remaining);
	bool EndRow();
	void Reset();
}