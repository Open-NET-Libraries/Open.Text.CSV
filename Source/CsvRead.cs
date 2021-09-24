using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Open.Text.CSV
{
	public static class CsvRead
	{
		public static IEnumerable<IReadOnlyList<string>> ReadAllRows(TextReader textReader, int rowBufferCount = 3)
		{
			if (rowBufferCount < 1) throw new ArgumentOutOfRangeException(nameof(rowBufferCount), rowBufferCount, "Must be at least 1.");
			var buffer = new BlockingCollection<ArraySegment<string>>();
			foreach(var row in buffer)
				yield return row;

			_ = Task.Run(() =>
			{

			});
		}
	}
}
