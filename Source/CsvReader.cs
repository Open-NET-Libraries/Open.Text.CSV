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

		public IEnumerable<CsvRowEntry> ReadNextRow()
			=> GetNextRow(Source);

		public ValueTask<IEnumerable<CsvRowEntry>?> ReadNextRowAsync()
			=> GetNextRowAsync(Source);

		public IEnumerable<IEnumerable<CsvRowEntry>> ReadRows()
		{
			var s = _source;
			if (s is null)
				throw new ObjectDisposedException(GetType().ToString());
			Contract.EndContractBlock();

			while (TryGetNextRow(s, out var row))
				yield return row;
		}

		public static bool TryGetNextRow(StreamReader source, out IEnumerable<CsvRowEntry> row)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			Contract.EndContractBlock();

			if (!source.EndOfStream)
			{
				row = CsvUtility.GetRow(source.ReadLine());
				return true;
			}

			row = null!;
			return false;
		}

		public static ValueTask<IEnumerable<CsvRowEntry>?> GetNextRowAsync(StreamReader source)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			Contract.EndContractBlock();

			if (source.EndOfStream)
				return new ValueTask<IEnumerable<CsvRowEntry>?>(default(IEnumerable<CsvRowEntry>));

			return GetNextRowAsyncCore(source);
		}

		static async ValueTask<IEnumerable<CsvRowEntry>?> GetNextRowAsyncCore(StreamReader source)
			=> source.EndOfStream ? null : CsvUtility.GetRow(await source.ReadLineAsync().ConfigureAwait(false));

		public static IEnumerable<CsvRowEntry> GetNextRow(StreamReader source)
		{
			TryGetNextRow(source, out var row);
			return row;
		}

		public static IEnumerable<IEnumerable<CsvRowEntry>> GetRows(StreamReader source)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			Contract.EndContractBlock();

			while (TryGetNextRow(source, out var row))
				yield return row;
		}

#if NETSTANDARD2_1_OR_GREATER
		public static async IAsyncEnumerable<IEnumerable<CsvRowEntry>> GetRowsAsync(StreamReader source)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			Contract.EndContractBlock();

			if (source.EndOfStream) yield break;
			var last = source.ReadLineAsync().ConfigureAwait(false);

			while(!source.EndOfStream)
			{
				var next = source.ReadLineAsync().ConfigureAwait(false);
				yield return CsvUtility.GetRow(await last);
				last = next;
			}

			yield return CsvUtility.GetRow(await last);
		}

		public static async IAsyncEnumerable<IEnumerable<CsvRowEntry>> GetRowsFromFileAsync(string filepath)
		{
			if (filepath is null)
				throw new ArgumentNullException(nameof(filepath));
			if (string.IsNullOrWhiteSpace(filepath))
				throw new ArgumentException("Cannot be empty or only whitespace.", nameof(filepath));
			Contract.EndContractBlock();

			if (!File.Exists(filepath)) yield break;
			using var fs = new FileInfo(filepath).OpenRead();
			using var sr = new StreamReader(fs);
			await foreach (var line in GetRowsAsync(sr))
				yield return line;
		}
#endif

		public static IEnumerable<IEnumerable<CsvRowEntry>> GetRowsFromFile(string filepath)
		{
			if (filepath is null)
				throw new ArgumentNullException(nameof(filepath));
			if (string.IsNullOrWhiteSpace(filepath))
				throw new ArgumentException("Cannot be empty or only whitespace.", nameof(filepath));
			Contract.EndContractBlock();

			if (!File.Exists(filepath)) yield break;
			using var fs = new FileInfo(filepath).OpenRead();
			using var sr = new StreamReader(fs);
			foreach (var line in GetRows(sr))
				yield return line;
		}
	}
}
