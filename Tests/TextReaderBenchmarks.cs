﻿using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Xunit;

namespace Open.Text.CSV.Test;

public class TextReaderBenchmarks : TextReaderBenchmarkBase
{
	[Benchmark]
	public int StreamReader_Read()
	{
		var count = 0;
		int next;
		while ((next = Reader.Read(CharBuffer)) is not 0)
			count += next;
		return count;
	}

	[Benchmark]
	public async Task<int> StreamReader_ReadAsync()
	{
		var count = 0;
		int next;
		while ((next = await Reader.ReadAsync(CharBuffer).ConfigureAwait(false)) is not 0)
			count += next;
		return count;
	}

	[Benchmark]
	public async Task<long> PipeReader_EnumerateAsync()
	{
		long count = 0;
		await foreach (var buffer in PipeReader
			.Create(Stream)
			.EnumerateAsync()
			.DecodeAsync())
		{
			count += buffer.WrittenMemory.Length;
		}

		return count;
	}

	//[Benchmark]
	public int StreamReader_ReadLine()
	{
		var count = 0;
		while (Reader.ReadLine() is not null)
			count++;
		return count;
	}

	//[Benchmark]
	public async Task<int> StreamReader_ReadLineAsync()
	{
		var count = 0;
		while (await Reader.ReadLineAsync().ConfigureAwait(false) is not null)
			count++;
		return count;
	}

	//[Benchmark]
	public async Task<int> StreamReader_SingleBufferReadAsync()
	{
		var count = 0;
		await foreach (var buffer in Reader.SingleBufferReadAsync(BufferSize))
			count += buffer.Length;
		return count;
	}

	//[Benchmark]
	public async Task<int> StreamReader_DualBufferReadAsync()
	{
		var count = 0;
		await foreach (var buffer in Reader.DualBufferReadAsync(BufferSize))
			count += buffer.Length;
		return count;
	}

	//[Benchmark]
	public async Task<int> StreamReader_PreemptiveReadLineAsync()
	{
		var count = 0;
		await foreach (var buffer in Reader.PreemptiveReadLineAsync())
			count++;
		return count;
	}
}

public class FileReadMethodTests : TextReaderBenchmarks
{
	static readonly int ExpectedCharacterCount = GetExpectedCharacterCount();

	static int GetExpectedCharacterCount()
	{
		var buffer = new char[4096];
		using var sr = new StreamReader(TEST_FILE);
		var count = 0;
		int next;
		while ((next = sr.Read(buffer)) is not 0)
		{
			count += next;
		}

		return count;
	}

	[Fact]
	public Task StreamReader_SingleBufferReadAsyncTest() => RunAsync(
		async () => Assert.Equal(
			ExpectedCharacterCount,
			await StreamReader_SingleBufferReadAsync()));

	[Fact]
	public Task StreamReader_DualBufferReadAsyncTest() => RunAsync(
		async () => Assert.Equal(
			ExpectedCharacterCount,
			await StreamReader_DualBufferReadAsync()));

	[Fact]
	public Task StreamReader_PreemptiveReadLineAsyncTest() => RunAsync(
		async () => Assert.Equal(
			CsvFileReadTests.ExpectedLineCount,
			await StreamReader_PreemptiveReadLineAsync()));
}