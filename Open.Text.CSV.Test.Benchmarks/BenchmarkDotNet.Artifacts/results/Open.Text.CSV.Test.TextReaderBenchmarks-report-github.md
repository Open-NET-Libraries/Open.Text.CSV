``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1237 (21H1/May2021Update)
AMD Ryzen 5 4500U with Radeon Graphics, 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.100-rc.1.21463.6
  [Host]     : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT
  Job-GSLIBR : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                    Method | BufferSize | UseAsync |      Mean |     Error |    StdDev |    Median | Ratio | RatioSD |
|-------------------------- |----------- |--------- |----------:|----------:|----------:|----------:|------:|--------:|
|         **StreamReader_Read** |       **4096** |    **False** | **103.12 ms** | **12.815 ms** | **37.786 ms** | **115.82 ms** |  **1.40** |    **0.79** |
|    StreamReader_ReadAsync |       4096 |    False | 390.25 ms |  4.908 ms |  4.591 ms | 389.94 ms |  5.55 |    2.07 |
| PipeReader_EnumerateAsync |       4096 |    False | 530.73 ms |  4.008 ms |  3.749 ms | 531.56 ms |  7.54 |    2.79 |
|           FileStream_Read |       4096 |    False |  86.45 ms | 10.064 ms | 29.673 ms | 101.79 ms |  1.00 |    0.00 |
|      FileStream_ReadAsync |       4096 |    False | 164.51 ms |  2.416 ms |  2.142 ms | 164.89 ms |  2.27 |    0.84 |
|                           |            |          |           |           |           |           |       |         |
|         **StreamReader_Read** |       **4096** |     **True** | **270.00 ms** | **25.665 ms** | **75.675 ms** | **286.30 ms** |  **1.22** |    **0.55** |
|    StreamReader_ReadAsync |       4096 |     True | 236.19 ms |  2.834 ms |  5.253 ms | 235.98 ms |  0.94 |    0.30 |
| PipeReader_EnumerateAsync |       4096 |     True | 251.74 ms |  4.943 ms |  4.623 ms | 252.17 ms |  0.89 |    0.14 |
|           FileStream_Read |       4096 |     True | 241.45 ms | 21.703 ms | 63.992 ms | 251.60 ms |  1.00 |    0.00 |
|      FileStream_ReadAsync |       4096 |     True | 208.57 ms |  4.081 ms |  4.191 ms | 208.95 ms |  0.75 |    0.12 |
|                           |            |          |           |           |           |           |       |         |
|         **StreamReader_Read** |      **16384** |    **False** |  **76.13 ms** |  **0.941 ms** |  **0.834 ms** |  **76.32 ms** |  **1.48** |    **0.02** |
|    StreamReader_ReadAsync |      16384 |    False | 140.35 ms |  1.059 ms |  0.990 ms | 140.50 ms |  2.73 |    0.02 |
| PipeReader_EnumerateAsync |      16384 |    False | 204.74 ms |  1.106 ms |  1.034 ms | 204.74 ms |  3.98 |    0.04 |
|           FileStream_Read |      16384 |    False |  51.42 ms |  0.396 ms |  0.371 ms |  51.36 ms |  1.00 |    0.00 |
|      FileStream_ReadAsync |      16384 |    False |  59.45 ms |  1.135 ms |  3.030 ms |  58.62 ms |  1.26 |    0.03 |
|                           |            |          |           |           |           |           |       |         |
|         **StreamReader_Read** |      **16384** |     **True** | **101.33 ms** |  **9.239 ms** | **27.240 ms** | **105.53 ms** |  **1.35** |    **0.57** |
|    StreamReader_ReadAsync |      16384 |     True | 192.67 ms |  3.809 ms |  5.214 ms | 193.90 ms |  2.10 |    0.37 |
| PipeReader_EnumerateAsync |      16384 |     True | 215.07 ms |  3.238 ms |  2.870 ms | 215.89 ms |  2.27 |    0.18 |
|           FileStream_Read |      16384 |     True |  81.16 ms |  7.250 ms | 21.376 ms |  83.35 ms |  1.00 |    0.00 |
|      FileStream_ReadAsync |      16384 |     True |  66.95 ms |  1.323 ms |  3.065 ms |  67.99 ms |  0.75 |    0.12 |
