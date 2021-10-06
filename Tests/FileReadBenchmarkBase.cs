using BenchmarkDotNet.Attributes;
using System;
using System.IO;

namespace Open.Text.CSV.Test;

public abstract class FileReadBenchmarkBase
{
	protected const string TEST_FILE = "TestData.csv";
	protected FileReadBenchmarkBase(string testFile = null) => TestFile = testFile ?? TEST_FILE;

	protected readonly string TestFile;

	protected FileStream GetStream(int bufferSize, bool useAsync = false)
		=> new(TestFile, new FileStreamOptions
		{
			Access = FileAccess.Read,
			BufferSize = bufferSize,
			Mode = FileMode.Open,
			Options = (useAsync ? FileOptions.Asynchronous : FileOptions.None) | FileOptions.SequentialScan,
			Share = FileShare.Read,
		});

	//[Params(1, 8192, 32768)]
	public int FileStreamBufferSize { get; set; } = 1;

	//[Params(4092, 16384, 65536)]
	public int ByteBufferSize { get; set; } = 65536;

	[Params(true, false)]
	public bool UseAsync { get; set; } = false;

	protected FileStream GetStream() => GetStream(FileStreamBufferSize, UseAsync);

}
