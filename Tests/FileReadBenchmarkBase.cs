using BenchmarkDotNet.Attributes;
using System.IO;

namespace Open.Text.CSV.Test;

public abstract class FileReadBenchmarkBase
{
	protected const string TEST_FILE = "TestData.csv";
	protected static FileStream GetStream(int bufferSize, bool useAsync = false)
		=> new(TEST_FILE, new FileStreamOptions
		{
			Access = FileAccess.Read,
			BufferSize = bufferSize,
			Mode = FileMode.Open,
			Options = (useAsync ? FileOptions.Asynchronous : FileOptions.None) | FileOptions.SequentialScan,
			Share = FileShare.Read,
		});

	[Params(1, 8192, 32768)]
	public int FileStreamBufferSize { get; set; } = 8192;

	[Params(4092, 16384, 65536)]
	public int ByteBufferSize { get; set; } = 16384;

	[Params(true, false)]
	public bool UseAsync { get; set; } = false;

	protected FileStream GetStream() => GetStream(FileStreamBufferSize, UseAsync);

}
