using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Open.Text.CSV
{
	public class CsvRowBuilder
	{
		const string CORRUPT_FIELD = "Corrupt field found. A double quote is not escaped or there is extra data after a quoted field.";

		List<string>? _fields;
		readonly StringBuilder _fb = new();

		State _state = State.BeforeField;
		int _len = 0;

		private readonly Action<List<string>> _rowHandler;

		public CsvRowBuilder(Action<List<string>> rowHandler)
		{
			_rowHandler = rowHandler ?? throw new ArgumentNullException(nameof(rowHandler));
		}

		public bool EndRow()
		{
			switch (_state)
			{
				case State.InField:
				case State.Quote:
					AddEntry();
					break;

				case State.InQuotedField:
					_fb.Clear();
					throw new InvalidDataException("When the line ends with a quoted field, the last character should be an unescaped double quote.");
			}

			return Complete();
		}

		private bool Complete()
		{
			_state = State.BeforeField;
			_fb.Clear();
			var f = _fields;
			_fields = null;
			if (f is null || f.Count == 0) return false;
			_rowHandler(f);

			return true;
		}

		public bool AddChar(int c)
		{
			if (c == -1) return EndRow();

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
							_fb.Clear();
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

		public bool AddNextChars(in ReadOnlySpan<char> chars, out ReadOnlySpan<char> remaining)
		{
			var len = chars.Length;
			for (var i = 0; i < len; i++)
			{
				if (AddChar(chars[i]))
				{
					var n = i + 1;
					remaining = n == chars.Length ? ReadOnlySpan<char>.Empty : chars.Slice(n);
					return true;
				}
			}

			remaining = ReadOnlySpan<char>.Empty;
			return false;
		}

		public bool AddNextChars(string chars, out ReadOnlySpan<char> remaining)
			=> AddNextChars(chars.AsSpan(), out remaining);

		void AddNextChar(in int c, bool ws = false)
		{
			_fb.Append((char)c);
			if (!ws) _len = _fb.Length;
		}

		void AddEntry()
		{
			_fields ??= new List<string>();
			if (_len == 0)
			{
				_fields.Add(string.Empty);
				if (_fb.Length != 0) _fb.Clear();
				return;
			}

			if (_len < _fb.Length) _fb.Length = _len;
			_fields.Add(_fb.ToString());
			_fb.Clear();
			_len = 0;
		}

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
}
