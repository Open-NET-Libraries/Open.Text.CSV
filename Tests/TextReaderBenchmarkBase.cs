using System.IO;

namespace Open.Text.CSV.Test;

public class TextReaderBenchmarkBase : FileReadBenchmarkBase
{
	protected StreamReader Reader { get; private set; }
	protected char[] CharBuffer { get; private set; }

	public override void Setup()
	{
		base.Setup();
		Reader = new StreamReader(Stream);
		CharBuffer = new char[ByteBufferSize];
	}

	public override void Cleanup()
	{
		Reader.Dispose();
		CharBuffer = null;
		base.Cleanup();
	}
}
