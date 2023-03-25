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

var report = new Open.Diagnostics.BenchmarkConsoleReport<TextReaderFileBenchmarks>(10, sb, SimpleBenchmark<TextReaderFileBenchmarks>.Results);

report.AddBenchmark(nameof(TextReaderFileBenchmarks) + " UseAsync=false", _ => new TextReaderFileBenchmarks(filePath)
{
	UseAsync = false
});

report.AddBenchmark(nameof(TextReaderFileBenchmarks) + " UseAsync=true", _ => new TextReaderFileBenchmarks(filePath)
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