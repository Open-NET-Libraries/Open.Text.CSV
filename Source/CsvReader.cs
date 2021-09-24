using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Open.Text.CSV
{
	public class CsvReader : CsvReaderBase
	{
		public CsvReader(TextReader source) : base(source)
		{
			_rowBuilder = new(row => _nextRow = row);
		}

		readonly CsvRowBuilder _rowBuilder;

		List<string>? _nextRow;

		public bool ReadNextRow(out IList<string>? rowBuffer)
		{
			var s = Source;
			int c;

		loop:
			c = s.Read();

			if (c == -1)
			{
				var rowReady = _rowBuilder.EndRow();
				rowBuffer = _nextRow;
				return rowReady;
			}

			if (_rowBuilder.AddChar(c))
			{
				rowBuffer = _nextRow;
				return true;

			}

			goto loop;
		}

		public IList<string>? ReadNextRow()
		{
			var result = ReadNextRow(out var list);
			Debug.Assert(result == (list is not null));
			return list;
		}

		public IEnumerable<IList<string>> ReadRows()
		{
			while (ReadNextRow(out var rowBuffer))
			{
				Debug.Assert(rowBuffer is not null);
				yield return rowBuffer!;
			}
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

		public static IEnumerable<IList<string>> ReadAllRows(TextReader source)
		{
			using var csv = new CsvReader(source);
			foreach (var row in csv.ReadRows()) yield return row;
		}

		public static ChannelReader<List<string>> ReadAllRowsAsync(TextReader source, int rowBufferCount = 3, int bufferSize = 4095)
		{
			if (rowBufferCount < 1) throw new ArgumentOutOfRangeException(nameof(rowBufferCount), rowBufferCount, "Must be at least 1.");
			var	rowBuffer = Channel.CreateBounded<List<string>>(new BoundedChannelOptions(rowBufferCount)
			{
				SingleWriter = true,
				AllowSynchronousContinuations = true
			});

			_ = Task.Run(async () =>
			{
				var writer = rowBuffer.Writer;
				List<string>? nextRow = null;
				var rowBuilder = new CsvRowBuilder(row => nextRow = row);
				var pool = ArrayPool<char>.Shared;
				var cNext = pool.Rent(bufferSize);
				var cCurrent = pool.Rent(bufferSize);

				try
				{
#if NETSTANDARD2_1_OR_GREATER
					var next = source.ReadAsync(cNext);
#else
					var next = source.ReadAsync(cNext, 0, cNext.Length);
#endif
				loop:
					var n = await next.ConfigureAwait(false);
					if (n == 0)
					{
						writer.Complete();
						return;
					}

					// Preemptive request.
#if NETSTANDARD2_1_OR_GREATER
					var current = source.ReadAsync(cCurrent);
#else
					var current = source.ReadAsync(cCurrent, 0, cCurrent.Length);
#endif
					if (rowBuilder.Add(cNext.AsMemory(0, n), out var remaining))
					{
						do 
						{
							Debug.Assert(nextRow != null);
							await writer.WriteAsync(nextRow!).ConfigureAwait(false);
						}
						while (rowBuilder.Add(remaining, out remaining));
					}

					var swap = cNext;
					cNext = cCurrent;
					cCurrent = swap;
					next = current;

					goto loop;
				}
				catch (Exception ex)
				{
					writer.Complete(ex);
				}
				finally
				{
					pool.Return(cNext);
					pool.Return(cCurrent);
				}
			});

			return rowBuffer.Reader;
		}
	}
}
