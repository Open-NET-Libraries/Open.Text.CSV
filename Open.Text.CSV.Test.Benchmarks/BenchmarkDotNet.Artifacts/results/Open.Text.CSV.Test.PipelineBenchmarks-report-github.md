``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1237 (21H1/May2021Update)
AMD Ryzen 5 4500U with Radeon Graphics, 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.100-rc.1.21463.6
  [Host]     : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT
  Job-QYBRXV : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                    Method | BufferSize | UseAsync |      Mean |     Error |    StdDev |    Median | Ratio | RatioSD |
|-------------------------- |----------- |--------- |----------:|----------:|----------:|----------:|------:|--------:|
| **PipeReader_EnumerateAsync** |       **4096** |    **False** | **343.94 ms** |  **6.701 ms** | **10.233 ms** | **346.01 ms** |  **3.99** |    **1.45** |
|           FileStream_Read |       4096 |    False |  88.62 ms |  9.660 ms | 28.484 ms | 101.81 ms |  1.00 |    0.00 |
|      FileStream_ReadAsync |       4096 |    False |  70.00 ms |  0.991 ms |  0.878 ms |  69.94 ms |  0.70 |    0.22 |
|                           |            |          |           |           |           |           |       |         |
| **PipeReader_EnumerateAsync** |       **4096** |     **True** | **242.94 ms** |  **4.764 ms** |  **9.180 ms** | **243.31 ms** |  **0.90** |    **0.20** |
|           FileStream_Read |       4096 |     True | 249.27 ms | 21.865 ms | 64.468 ms | 267.99 ms |  1.00 |    0.00 |
|      FileStream_ReadAsync |       4096 |     True | 212.42 ms |  4.229 ms |  7.294 ms | 212.61 ms |  0.78 |    0.18 |
|                           |            |          |           |           |           |           |       |         |
| **PipeReader_EnumerateAsync** |      **16384** |    **False** | **128.58 ms** |  **8.079 ms** | **23.820 ms** | **126.84 ms** |  **3.34** |    **1.25** |
|           FileStream_Read |      16384 |    False |  41.74 ms |  3.632 ms | 10.710 ms |  43.46 ms |  1.00 |    0.00 |
|      FileStream_ReadAsync |      16384 |    False |  29.77 ms |  1.003 ms |  2.814 ms |  28.58 ms |  0.79 |    0.30 |
|                           |            |          |           |           |           |           |       |         |
| **PipeReader_EnumerateAsync** |      **16384** |     **True** | **161.93 ms** |  **3.505 ms** | **10.336 ms** | **156.88 ms** |  **4.14** |    **0.31** |
|           FileStream_Read |      16384 |     True |  41.63 ms |  0.650 ms |  0.507 ms |  41.63 ms |  1.00 |    0.00 |
|      FileStream_ReadAsync |      16384 |     True |  65.46 ms |  1.297 ms |  2.928 ms |  65.31 ms |  1.59 |    0.10 |
