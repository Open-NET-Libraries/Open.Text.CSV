using System;
using System.IO;
using System.Text;

namespace Open.Text.CSV
{
	public abstract class CsvReaderBase : IDisposable
	{
		protected CsvReaderBase(TextReader source)
		{
			_source = source ?? throw new ArgumentNullException(nameof(source));
		}

		protected const string CORRUPT_FIELD = "Corrupt field found. A double quote is not escaped or there is extra data after a quoted field.";
		TextReader? _source;
		protected TextReader Source => _source ?? throw new ObjectDisposedException(GetType().ToString());

		public virtual void Dispose()
		{
			_source = null; // The intention here is if this object is disposed, then prevent further reading.
		}

		protected enum State
		{
			BeforeField,
			InField,
			InQuotedField,
			Quote,
			AfterQuotedField,
			EndOfRow,
		}

		protected StringBuilder? FieldBuffer;

	}
}
