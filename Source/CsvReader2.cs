using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Open.Text.CSV;

public sealed class CsvReader2 : CsvReaderBase<IMemoryOwner<string>>
{
	public CsvReader2(int maxFields, TextReader source) : base(source, new CsvRowBuilder2(maxFields))
	{
	}
}
