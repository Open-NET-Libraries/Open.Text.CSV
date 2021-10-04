``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1237 (21H1/May2021Update)
AMD Ryzen 5 4500U with Radeon Graphics, 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.100-rc.1.21463.6
  [Host]     : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT
  Job-WHMBDW : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                    Method | FileStreamBufferSize | UseAsync |      Mean |     Error |     StdDev |    Median |
|-------------------------- |--------------------- |--------- |----------:|----------:|-----------:|----------:|
|         **StreamReader_Read** |                    **1** |    **False** | **338.03 ms** | **24.284 ms** |  **71.603 ms** | **362.52 ms** |
|    StreamReader_ReadAsync |                    1 |    False | 214.59 ms |  3.599 ms |   3.534 ms | 214.32 ms |
| PipeReader_EnumerateAsync |                    1 |    False | 249.96 ms | 32.718 ms |  96.470 ms | 273.43 ms |
|         **StreamReader_Read** |                    **1** |     **True** | **808.59 ms** | **76.520 ms** | **225.620 ms** | **819.66 ms** |
|    StreamReader_ReadAsync |                    1 |     True | 707.02 ms | 10.281 ms |   9.617 ms | 707.26 ms |
| PipeReader_EnumerateAsync |                    1 |     True | 243.91 ms |  4.198 ms |   3.278 ms | 244.74 ms |
|         **StreamReader_Read** |                 **8192** |    **False** |  **95.53 ms** |  **5.364 ms** |  **15.814 ms** | **104.74 ms** |
|    StreamReader_ReadAsync |                 8192 |    False | 173.54 ms |  2.968 ms |   3.048 ms | 174.78 ms |
| PipeReader_EnumerateAsync |                 8192 |    False | 296.57 ms |  5.889 ms |   7.232 ms | 297.80 ms |
|         **StreamReader_Read** |                 **8192** |     **True** | **180.47 ms** | **10.311 ms** |  **30.401 ms** | **179.91 ms** |
|    StreamReader_ReadAsync |                 8192 |     True | 279.05 ms | 19.941 ms |  58.798 ms | 300.55 ms |
| PipeReader_EnumerateAsync |                 8192 |     True | 362.36 ms |  4.478 ms |   3.970 ms | 362.26 ms |
|         **StreamReader_Read** |                **16384** |    **False** |  **78.67 ms** |  **0.421 ms** |   **0.328 ms** |  **78.61 ms** |
|    StreamReader_ReadAsync |                16384 |    False | 146.88 ms |  2.512 ms |   2.350 ms | 145.99 ms |
| PipeReader_EnumerateAsync |                16384 |    False |  82.30 ms |  1.643 ms |   3.774 ms |  81.33 ms |
|         **StreamReader_Read** |                **16384** |     **True** | **102.43 ms** | **10.899 ms** |  **32.136 ms** | **111.68 ms** |
|    StreamReader_ReadAsync |                16384 |     True | 197.16 ms |  3.174 ms |   2.969 ms | 197.66 ms |
| PipeReader_EnumerateAsync |                16384 |     True | 212.70 ms |  4.239 ms |   7.198 ms | 213.62 ms |
|         **StreamReader_Read** |                **32768** |    **False** |  **64.58 ms** |  **0.905 ms** |   **0.803 ms** |  **64.27 ms** |
|    StreamReader_ReadAsync |                32768 |    False |  92.26 ms |  0.880 ms |   0.687 ms |  92.39 ms |
| PipeReader_EnumerateAsync |                32768 |    False | 111.04 ms |  2.189 ms |   3.472 ms | 111.84 ms |
|         **StreamReader_Read** |                **32768** |     **True** |  **84.82 ms** |  **4.015 ms** |  **11.840 ms** |  **89.76 ms** |
|    StreamReader_ReadAsync |                32768 |     True | 129.97 ms |  2.563 ms |   3.422 ms | 129.70 ms |
| PipeReader_EnumerateAsync |                32768 |     True | 148.35 ms |  1.999 ms |   1.772 ms | 148.42 ms |
