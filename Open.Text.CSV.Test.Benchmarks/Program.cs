using BenchmarkDotNet.Running;
using Open.Text.CSV.Test;
using Open.Text.CSV.Test.Benchmarks;
using System;
using System.IO;
using System.Text;

Console.Write("Initializing...");

var sb = new StringBuilder();
var report = new Open.Diagnostics.BenchmarkConsoleReport<PipelineBenchmarks>(100, sb, SimpleBenchmark<PipelineBenchmarks>.Results);

var filePath = @"..\Tests\TestData.csv";
if (!File.Exists(filePath)) filePath = @"..\" + filePath;
if (!File.Exists(filePath)) filePath = @"..\" + filePath;
if (!File.Exists(filePath)) filePath = @"..\" + filePath;
if (!File.Exists(filePath)) throw new FileNotFoundException();

report.AddBenchmark(nameof(PipelineBenchmarks) + " UseAsync=false", count => new PipelineBenchmarks(filePath)
{
	UseAsync = false
});

report.AddBenchmark(nameof(PipelineBenchmarks) + " UseAsync=true", count => new PipelineBenchmarks(filePath)
{
	UseAsync = true
});

report.Pretest(10, 10); // Run once through first to scramble/warm-up initial conditions.
Console.SetCursorPosition(0, Console.CursorTop);
report.Test(4, 8);

//BenchmarkRunner.Run<StringBuilderVsMemoryOwner>();
//BenchmarkRunner.Run<CsvFileReadTests>();
//BenchmarkRunner.Run<TextReaderBenchmarks>();
//BenchmarkRunner.Run<CsvFileParallelReadTests>();
//BenchmarkRunner.Run<PipelineBenchmarks>();
Console.Beep();