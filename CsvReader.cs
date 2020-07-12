using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;

namespace Open.Text.CSV
{
	public class CsvReader : IDisposable
	{
		public CsvReader(StreamReader source)
		{
			_source = source ?? throw new ArgumentNullException(nameof(source));
		}

		StreamReader? _source;
		StreamReader Source => _source ?? throw new ObjectDisposedException(GetType().ToString());

		public void Dispose()
		{
			_source = null; // The intention here is if this object is disposed, then prevent further reading.
		}

		public IEnumerable<string> ReadNextRow()
			=> GetNextRow(Source);

		public ValueTask<IEnumerable<string>?> ReadNextRowAsync()
			=> GetNextRowAsync(Source);

		public IEnumerable<IEnumerable<string>> ReadRows()
		{
			var s = _source;
			if (s is null)
				throw new ObjectDisposedException(GetType().ToString());
			Contract.EndContractBlock();

			while (TryGetNextRow(s, out var row))
				yield return row;
		}

		public static bool TryGetNextRow(StreamReader source, out IEnumerable<string> row)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			Contract.EndContractBlock();

			if (!source.EndOfStream)
			{
				row = CsvUtility.GetLine(source.ReadLine());
				return true;
			}

			row = null!;
			return false;
		}

		public static ValueTask<IEnumerable<string>?> GetNextRowAsync(StreamReader source)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			Contract.EndContractBlock();

			if (source.EndOfStream)
				return new ValueTask<IEnumerable<string>?>(default(IEnumerable<string>));

			return GetNextRowAsyncCore(source);
		}

		static async ValueTask<IEnumerable<string>?> GetNextRowAsyncCore(StreamReader source)
			=> source.EndOfStream ? null : CsvUtility.GetLine(await source.ReadLineAsync().ConfigureAwait(false));

		public static IEnumerable<string> GetNextRow(StreamReader source)
		{
			TryGetNextRow(source, out var row);
			return row;
		}

		public static IEnumerable<IEnumerable<string>> GetRows(StreamReader source)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			Contract.EndContractBlock();

			while (TryGetNextRow(source, out var row))
				yield return row;
		}

		public static IEnumerable<IEnumerable<string>> GetRowsFromFile(string filepath)
		{
			if (filepath is null)
				throw new ArgumentNullException(nameof(filepath));
			if (string.IsNullOrWhiteSpace(filepath))
				throw new ArgumentException("Cannot be empty or only whitespace.", nameof(filepath));
			Contract.EndContractBlock();

			if (!File.Exists(filepath)) yield break;
			using var s = new StreamReader(new FileInfo(filepath).OpenRead());
			foreach (var line in GetRows(s))
				yield return line;
		}
	}
}
