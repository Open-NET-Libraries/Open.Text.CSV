﻿using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Open.Text.CSV.Test;

public abstract class FileReadBenchmarkBase
{
	protected const string TEST_FILE = "TestData.csv";
	protected static FileStream GetStream(int bufferSize, bool useAsync = false) => new(
		TEST_FILE,
		FileMode.Open,
		FileAccess.Read,
		FileShare.Read,
		bufferSize,
		useAsync);


	[Params(4096, 16384)]
	public int BufferSize { get; set; } = 4096;

	[Params(true, false)]
	public bool UseAsync { get; set; } = false;

	FileStream GetStream() => GetStream(BufferSize, UseAsync);

	protected FileStream Stream { get; private set; }
	protected byte[] ByteBuffer { get; private set; }

	[IterationSetup]
	public virtual void Setup()
	{
		Stream = GetStream();
		ByteBuffer = new byte[BufferSize];
	}

	[IterationCleanup]
	public virtual void Cleanup()
	{
		Stream.Dispose();
		Stream = null;
		ByteBuffer = null;
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
