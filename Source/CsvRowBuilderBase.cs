using System;
using System.Diagnostics;
using System.IO;

namespace Open.Text.CSV;

/// <summary>
/// Receives characters in a CSV sequence and translates them into values in a row.
/// </summary>
public abstract class CsvRowBuilderBase
{
	const string CORRUPT_FIELD = "Corrupt field found. A double quote is not escaped or there is extra data after a quoted field.";

	State _state = State.BeforeField;
	protected int FieldLen = 0;
	protected int MaxFields = 0;

	/// <summary>
	/// Resets the row builder to the beginning of a row.
	/// </summary>
	public virtual void Reset()
	{
		_state = State.BeforeField;
		ResetFieldBuffer();
		FieldLen = 0;
	}

	protected abstract void ResetFieldBuffer();

	/// <summary>
	/// Attempts to finalize a row.
	/// </summary>
	/// <returns>true if a new row is emitted; otherwise false.</returns>
	/// <exception cref="InvalidDataException">If called in the middle of a quoted field.</exception>
	public bool EndRow()
	{
		switch (_state)
		{
			case State.InField:
			case State.Quote:
				AddEntry();
				break;

			case State.InQuotedField:
				ResetFieldBuffer();
				throw new InvalidDataException("When the line ends with a quoted field, the last character should be an unescaped double quote.");
		}

		return Complete();
	}

	private bool AddChar(in char c)
	{
		switch (_state)
		{
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
						return false;

					case '\r':
						_state = State.EndOfRow;
						return false;

					case '\n':
						break;

					default:
						_state = State.InField;
						AddNextChar(in c);
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

		Debug.Assert(c == -1 || c == '\n');
		return Complete();
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
	public bool Add(in ArraySegment<char> chars, out ArraySegment<char> remaining)
	{
		var a = chars.Array;
		var len = chars.Count;
		var offset = chars.Offset;
		var end = offset + len;
		var span = chars.AsSpan();

		for (var i = 0; i < len; i++)
		{
			ref readonly char c = ref span[i];
			if (AddChar(in c))
			{
				var n = offset + i + 1;
				remaining = n == end ? default : new ArraySegment<char>(a, n, end - n);
				return true;
			}
		}

		remaining = default;
		return false;
	}

	/// <summary>
	/// Takes all the characters in an ReadOnlySpan and attemtps to add them to the row.
	/// </summary>
	/// <inheritdoc cref="Add(in ArraySegment{char}, out ArraySegment{char})"/>
	public bool Add(in ReadOnlySpan<char> chars, out ReadOnlySpan<char> remaining)
	{
		var len = chars.Length;
		for (var i = 0; i < len; i++)
		{
			ref readonly char c = ref chars[i];
			if (AddChar(in c))
			{
				var n = i + 1;
				remaining = n == chars.Length ? ReadOnlySpan<char>.Empty : chars.Slice(n);
				return true;
			}
		}

		remaining = ReadOnlySpan<char>.Empty;
		return false;
	}

	/// <summary>
	/// Takes all the characters in an ReadOnlyMemory and attemtps to add them to the row.
	/// </summary>
	/// <inheritdoc cref="Add(in ArraySegment{char}, out ArraySegment{char})"/>
	public bool Add(in ReadOnlyMemory<char> chars, out ReadOnlyMemory<char> remaining)
	{
		var len = chars.Length;
		var span = chars.Span;
		for (var i = 0; i < len; i++)
		{
			ref readonly char c = ref span[i];
			if (AddChar(in c))
			{
				var n = i + 1;
				remaining = n == chars.Length ? ReadOnlyMemory<char>.Empty : chars.Slice(n);
				return true;
			}
		}

		remaining = ReadOnlyMemory<char>.Empty;
		return false;
	}

	/// <summary>
	/// Takes all the characters in a string and attemtps to add them to the row.
	/// </summary>
	/// <inheritdoc cref="Add(in ArraySegment{char}, out ArraySegment{char})"/>
	public bool Add(string chars, out ReadOnlySpan<char> remaining)
		=> Add(chars.AsSpan(), out remaining);

	protected abstract void AddNextChar(in char c, bool ws = false);

	protected abstract void AddEntry();

	protected abstract bool Complete();

	enum State
	{
		BeforeField,
		InField,
		InQuotedField,
		Quote,
		AfterQuotedField,
		EndOfRow
	}
}
