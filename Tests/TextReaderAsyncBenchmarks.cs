using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open.Text.CSV.Test;

[MemoryDiagnoser]
public class TextReaderAsyncBenchmarks : TextReaderAsyncBenchmarkBase
{
	public TextReaderAsyncBenchmarks(string path = null) : base(path)
	{
	}


}
