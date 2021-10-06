using BenchmarkDotNet.Running;
using Open.Text.CSV.Test;
using Open.Text.CSV.Test.Benchmarks;
using System;
using System.IO;
using System.Text;

var filePath = @"..\Tests\TestData.csv";
if (!File.Exists(filePath)) filePath = @"..\" + filePath;
if (!File.Exists(filePath)) filePath = @"..\" + filePath;
if (!File.Exists(filePath)) filePath = @"..\" + filePath;
if (!File.Exists(filePath)) throw new FileNotFoundException();

Console.Write("Initializing...");
var sb = new StringBuilder();

var report = new Open.Diagnostics.BenchmarkConsoleReport<CsvFileReadBenchmarks>(1500, sb, SimpleBenchmark<CsvFileReadBenchmarks>.Results);

report.AddBenchmark(nameof(CsvFileReadBenchmarks) + " UseAsync=false", count => new CsvFileReadBenchmarks(filePath)
{
	UseAsync = false
});

report.AddBenchmark(nameof(CsvFileReadBenchmarks) + " UseAsync=true", count => new CsvFileReadBenchmarks(filePath)
{
	UseAsync = true
});

report.Pretest(4, 4); // Run once through first to scramble/warm-up initial conditions.
Console.SetCursorPosition(0, Console.CursorTop);
report.Test(4, 8);

//BenchmarkRunner.Run<StringBuilderVsMemoryOwner>();
//BenchmarkRunner.Run<CsvFileReadTests>();
//BenchmarkRunner.Run<TextReaderBenchmarks>();
//BenchmarkRunner.Run<CsvFileParallelReadTests>();
//BenchmarkRunner.Run<PipelineBenchmarks>();
Console.Beep();