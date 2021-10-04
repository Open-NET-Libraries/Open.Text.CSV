using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using System.Threading.Tasks;

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

	[Params(1, 8192, 16384, 32768)]
	public int FileStreamBufferSize { get; set; } = 1;

	[Params(1024, 4096, 16384)]
	public int ByteBufferSize { get; set; } = 4096;

	[Params(true, false)]
	public bool UseAsync { get; set; } = false;

	FileStream GetStream() => GetStream(FileStreamBufferSize, UseAsync);

	protected FileStream Stream { get; private set; }
	protected byte[] ByteBuffer { get; private set; }

	protected Memory<byte> ByteBufferMemory { get; private set; }

	[IterationSetup]
	public virtual void Setup()
	{
		Stream = GetStream();
		ByteBuffer = new byte[ByteBufferSize]; // Get the exact size for the benchmark.
		ByteBufferMemory = new Memory<byte>(ByteBuffer);
	}

	[IterationCleanup]
	public virtual void Cleanup()
	{
		Stream.Dispose();
		Stream = null;
		ByteBuffer = null;
		ByteBufferMemory = Memory<byte>.Empty;
	}

	protected void Run(Action test)
	{
		Setup();
		test();
		Cleanup();
	}

	protected T Run<T>(Func<T> test)
	{
		Setup(); 
		try
		{
			return test();
		}
		finally
		{
			Cleanup();
		}
	}

	protected async Task RunAsync(Func<Task> test)
	{
		Setup();
		await test();
		Cleanup();
	}

}
