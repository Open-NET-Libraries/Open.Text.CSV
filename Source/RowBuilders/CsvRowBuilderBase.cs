using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Open.Text.CSV;

/// <summary>
/// Receives characters in a CSV sequence and translates them into values in a row.
/// </summary>
public abstract class CsvRowBuilderBase<TRow> : ICsvRowBuilder<TRow>
{
	const string CORRUPT_FIELD = "Corrupt field found. A double quote is not escaped or there is extra data after a quoted field.";

	State _state = State.RowStart;
	protected int FieldLen = 0;
	public int MaxFields { get; protected set; }

	/// <summary>
	/// Resets the row builder to the beginning of a row.
	/// </summary>
	public virtual void Reset()
	{
		_state = State.RowStart;
		ResetFieldBuffer();
		FieldLen = 0;
	}

	protected abstract void ResetFieldBuffer();

	/// <summary>
	/// Attempts to finalize a row.
	/// </summary>
	/// <returns>true if a new row is emitted; otherwise false.</returns>
	/// <exception cref="InvalidDataException">If called in the middle of a quoted field.</exception>
	public bool EndRow(
#if NULL_ANALYSIS
	[NotNullWhen(true)]
#endif
	out TRow? row)
	{
		switch (_state)
		{
			case State.BeforeField:
			case State.InField:
			case State.Quote:
				AddEntry();
				break;

			case State.InQuotedField:
				ResetFieldBuffer();
				throw new InvalidDataException("When the line ends with a quoted field, the last character should be an unescaped double quote.");
		}

		return Complete(out row);
	}

	private bool AddChar(in char c,
#if NULL_ANALYSIS
	[NotNullWhen(true)]
#endif
	out TRow? row)
	{
		row = default;
		switch (_state)
		{
			case State.RowStart:
			case State.BeforeField:
				switch (c)
				{
					case ' ':
					case '\t':
						return false; // ignore leading white-space.

					case '"':
						_state = State.InQuotedField;
						return false;

					case ',':
						AddEntry();
						_state = State.BeforeField;
						return false;

					case '\r':
						if(_state == State.BeforeField) AddEntry();
						_state = State.EndOfRow;
						return false;

					case '\n':
						if (_state == State.BeforeField) AddEntry();
						_state = State.RowStart;
						break;

					default:
						AddNextChar(in c);
						_state = State.InField;
						return false;
				}

				break;

			case State.InField:
				switch (c)
				{
					case ' ':
					case '\t':
						// Potentially the end of a field.
						AddNextChar(in c, true);
						return false;

					case ',':
						AddEntry();
						_state = State.BeforeField;
						return false;

					case '\r':
						AddEntry();
						_state = State.EndOfRow;
						return false;

					case '\n':
						AddEntry();
						_state = State.RowStart;
						break;

					default:
						AddNextChar(in c);
						return false;
				}

				break;

			case State.InQuotedField:

				if (c == '"') _state = State.Quote;
				else AddNextChar(in c);
				return false;

			case State.Quote:
				switch (c)
				{
					case ' ':
					case '\t':
						AddEntry();
						_state = State.AfterQuotedField;
						return false;

					case '"':
						AddNextChar(in c);
						_state = State.InQuotedField;
						return false;

					case ',':
						AddEntry();
						_state = State.BeforeField;
						return false;

					case '\r':
						AddEntry();
						_state = State.EndOfRow;
						return false;

					case '\n':
						AddEntry();
						_state = State.RowStart;
						break;

					default:
						ResetFieldBuffer();
						throw new InvalidDataException(CORRUPT_FIELD);
				}

				break;

			case State.AfterQuotedField:
				switch (c)
				{
					case ' ':
					case '\t':
						return false;

					case ',':
						_state = State.BeforeField;
						return false;

					case '\r':
						_state = State.EndOfRow;
						return false;

					case '\n':
						_state = State.RowStart;
						break;

					default:
						throw new InvalidDataException(CORRUPT_FIELD);
				}

				break;

			case State.EndOfRow:
				if (c != '\n') throw new InvalidDataException("Expected new-line character after carrage return.");
				_state = State.EndOfRow;
				break;
		}

		Debug.Assert(c == '\n');
		return Complete(out row);
	}

	/// <summary>
	/// Takes all the characters in an ArraySegment and attemtps to add them to the row.
	/// </summary>
	/// <param name="chars">The characters to attempt to add.</param>
	/// <param name="remaining">
	/// The remaining characters after adding.
	/// If there are not enough to complete a row, an empty segment is returned.
	/// </param>
	/// <returns>true if a new row is emitted; otherwise false.</returns>
#pragma warning disable IDE0079 // Remove unnecessary suppression
	[SuppressMessage("Roslynator", "RCS1242:Do not pass non-read-only struct by read-only reference.", Justification = "ArraySegment non-read-only may be an oversight.")]
#pragma warning restore IDE0079 // Remove unnecessary suppression
	public bool Add(in ArraySegment<char> chars, out ArraySegment<char> remaining,
#if NULL_ANALYSIS
	[NotNullWhen(true)]
#endif
	out TRow? row)
	{
		var a = chars.Array;
		var len = chars.Count;
		var offset = chars.Offset;
		var end = offset + len;
		var span = chars.AsSpan();

		for (var i = 0; i < len; i++)
		{
			ref readonly char c = ref span[i];
			if (AddChar(in c, out row))
			{
				var n = offset + i + 1;
				remaining = n == end ? default : new ArraySegment<char>(a!, n, end - n);
				return true;
			}
		}

		row = default;
		remaining = default;
		return false;
	}

	/// <summary>
	/// Takes all the characters in an ReadOnlySpan and attemtps to add them to the row.
	/// </summary>
	/// <inheritdoc cref="Add(in ArraySegment{char}, out ArraySegment{char})"/>
	public bool Add(ReadOnlySpan<char> chars, out ReadOnlySpan<char> remaining,
#if NULL_ANALYSIS
	[NotNullWhen(true)]
#endif
	out TRow? row)
	{
		var len = chars.Length;
		for (var i = 0; i < len; i++)
		{
			ref readonly char c = ref chars[i];
			if (AddChar(in c, out row))
			{
				var n = i + 1;
				remaining = n == chars.Length ? ReadOnlySpan<char>.Empty : chars.Slice(n);
				return true;
			}
		}

		row = default;
		remaining = ReadOnlySpan<char>.Empty;
		return false;
	}

	/// <summary>
	/// Takes all the characters in an ReadOnlyMemory and attemtps to add them to the row.
	/// </summary>
	/// <inheritdoc cref="Add(in ArraySegment{char}, out ArraySegment{char})"/>
	public bool Add(ReadOnlyMemory<char> chars, out ReadOnlyMemory<char> remaining,
#if NULL_ANALYSIS
	[NotNullWhen(true)]
#endif
	out TRow? row)
	{
		var len = chars.Length;
		var span = chars.Span;
		for (var i = 0; i < len; i++)
		{
			ref readonly char c = ref span[i];
			if (AddChar(in c, out row))
			{
				var n = i + 1;
				remaining = n == chars.Length ? ReadOnlyMemory<char>.Empty : chars.Slice(n);
				return true;
			}
		}

		row = default;
		remaining = ReadOnlyMemory<char>.Empty;
		return false;
	}

	/// <summary>
	/// Takes all the characters in a string and attemtps to add them to the row.
	/// </summary>
	/// <inheritdoc cref="Add(in ArraySegment{char}, out ArraySegment{char})"/>
	public bool Add(string chars, out ReadOnlySpan<char> remaining,
#if NULL_ANALYSIS
	[NotNullWhen(true)]
#endif
	out TRow? row)
		=> Add(chars.AsSpan(), out remaining, out row);

	protected abstract void AddNextChar(in char c, bool ws = false);

	protected abstract void AddEntry();

	protected abstract bool Complete(
#if NULL_ANALYSIS
	[NotNullWhen(true)]
#endif
	out TRow? row);

	enum State
	{
		RowStart,
		BeforeField,
		InField,
		InQuotedField,
		Quote,
		AfterQuotedField,
		EndOfRow
	}
}
