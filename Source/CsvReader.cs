using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading;

namespace Open.Text.CSV
{
	public class CsvReader : IDisposable
	{
		public CsvReader(TextReader source)
		{
			_source = source ?? throw new ArgumentNullException(nameof(source));
		}

		private const string CORRUPT_FIELD = "Corrupt field found. A double quote is not escaped or there is extra data after a quoted field.";
		TextReader? _source;
		TextReader Source => _source ?? throw new ObjectDisposedException(GetType().ToString());

		public void Dispose()
		{
			_source = null; // The intention here is if this object is disposed, then prevent further reading.
		}

		private enum State
		{
			BeforeField,
			InField,
			InQuotedField,
			Quote,
			AfterQuotedField,
			EndOfRow,
		}

		StringBuilder? _fieldBuffer;

		private bool ReadNextRowCore(ref IList<string>? rowBuffer)
		{
			var rb = rowBuffer;
			var s = Source;
			var state = State.BeforeField;
			int c;
			int len = 0;

			var fb = Interlocked.Exchange(ref _fieldBuffer, null) ?? new StringBuilder();

			void AddChar(int c, bool ws = false)
			{
				fb.Append((char)c);
				if (!ws) len = fb.Length;
			}

			void AddEntry()
			{
				rb ??= new List<string>();
				if (len == 0)
				{
					rb.Add(string.Empty);
					if (fb.Length != 0) fb.Clear();
					return;
				}

				if (len < fb.Length) fb.Length = len;
				rb.Add(fb.ToString());
				fb.Clear();
				len = 0;
			}


			// Since ReadNextRow will only be done through synchronous methods, this should ensure thread safety.

			{
				// Should never be acquired without a clear buffer but an exception may have been trapped and state not reset.
				var fbEmpty = fb.Length == 0;
				Debug.Assert(fb.Length == 0);
				if (!fbEmpty) fb.Clear();
			}

			try
			{


			loop:

				c = s.Read();
				if (c == -1)
				{
					rowBuffer = rb;

					switch (state)
					{
						case State.InField:
						case State.Quote:
							AddEntry();
							break;

						case State.InQuotedField:
							fb.Clear();
							throw new InvalidDataException("When the line ends with a quoted field, the last character should be an unescaped double quote.");
					}

					return rb is not null && rb.Count != 0;
				}

				switch (state)
				{
					case State.BeforeField:
						switch (c)
						{
							case ' ':
							case '\t':
								break; // ignore leading white-space.

							case '"':
								state = State.InQuotedField;
								break;

							case ',':
								AddEntry();
								break;

							case '\r':
							case '\n':
								state = State.EndOfRow;
								break;

							default:
								state = State.InField;
								AddChar(c);
								break;
						}
						break;

					case State.InField:
						switch (c)
						{
							case ' ':
							case '\t':
								AddChar(c, true);
								break;

							case ',':
								AddEntry();
								state = State.BeforeField;
								break;

							case '\r':
							case '\n':
								AddEntry();
								state = State.EndOfRow;
								break;

							default:
								AddChar(c);
								break;
						}
						break;

					case State.InQuotedField:
						if (c == '"') state = State.Quote;
						else AddChar(c);
						break;

					case State.Quote:
						switch (c)
						{
							case ' ':
							case '\t':
								AddEntry();
								state = State.AfterQuotedField;
								break;

							case '"':
								AddChar(c);
								state = State.InQuotedField;
								break;

							case ',':
								AddEntry();
								state = State.BeforeField;
								break;

							case '\r':
							case '\n':
								AddEntry();
								state = State.EndOfRow;
								break;

							default:
								fb.Clear();
								throw new InvalidDataException(CORRUPT_FIELD);
						}
						break;

					case State.AfterQuotedField:
						switch (c)
						{
							case ' ':
							case '\t':
								break;

							case ',':
								state = State.BeforeField;
								break;

							case '\r':
							case '\n':
								state = State.EndOfRow;
								break;

							default:
								throw new InvalidDataException(CORRUPT_FIELD);
						}
						break;
				}

				if (state == State.EndOfRow)
				{
					rowBuffer = rb;
					switch (c)
					{
						case '\r':
							var n = s.Peek();
							if (n == -1) return rb is not null && rb.Count != 0;
							if (n != '\n') throw new InvalidDataException("Expected new-line character after carrage return.");
							c = s.Read();
							goto default;

						default:
							Debug.Assert(c == '\n');
							return rb is not null && rb.Count != 0 || s.Peek() != -1;
					}
				}

				goto loop;
			}
			finally
			{
				Interlocked.CompareExchange(ref _fieldBuffer, fb, null);
			}
		}

		public bool ReadNextRow(IList<string> rowBuffer)
		{
			if (rowBuffer is null)
				throw new ArgumentNullException(nameof(rowBuffer));

			return ReadNextRowCore(ref rowBuffer!);
		}

		public IList<string>? ReadNextRow()
		{
			IList<string>? list = null;
			var result = ReadNextRowCore(ref list);
			Debug.Assert(result == (list is not null));
			return list;
		}

		public IEnumerable<IList<string>> ReadRows(IList<string> rowBuffer)
		{
			if (rowBuffer is null)
				throw new ArgumentNullException(nameof(rowBuffer));

			var s = _source;
			if (s is null) throw new ObjectDisposedException(GetType().ToString());
			Contract.EndContractBlock();

			while (ReadNextRow(rowBuffer))
				yield return rowBuffer;
		}

		public IEnumerable<IList<string>> ReadRows()
		{
			var s = _source;
			if (s is null) throw new ObjectDisposedException(GetType().ToString());
			Contract.EndContractBlock();

			IList<string>? row;
			while ((row = ReadNextRow()) is not null)
				yield return row;
		}

		public static List<IList<string>> GetRowsFromFile(string filepath)
		{
			if (filepath is null)
				throw new ArgumentNullException(nameof(filepath));
			if (string.IsNullOrWhiteSpace(filepath))
				throw new ArgumentException("Cannot be empty or only whitespace.", nameof(filepath));
			Contract.EndContractBlock();

			using var fs = new FileInfo(filepath).OpenRead();
			using var sr = new StreamReader(fs);
			using var csv = new CsvReader(sr);
			var list = new List<IList<string>>();
			foreach (var row in csv.ReadRows()) list.Add(row);
			return list;
		}

		public static List<IList<string>> GetRowsFromText(string csvText)
		{
			if (csvText is null)
				throw new ArgumentNullException(nameof(csvText));
			Contract.EndContractBlock();

			using var sr = new StringReader(csvText);
			using var csv = new CsvReader(sr);
			var list = new List<IList<string>>();
			foreach (var row in csv.ReadRows()) list.Add(row);
			return list;
		}
	}
}
