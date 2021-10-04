``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1237 (21H1/May2021Update)
AMD Ryzen 5 4500U with Radeon Graphics, 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.100-rc.1.21463.6
  [Host]     : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT
  Job-LZETXR : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                             Method | BufferSize | UseAsync |     Mean |    Error |   StdDev |   Median | Ratio | RatioSD |       Gen 0 |      Gen 1 |      Gen 2 | Allocated |
|----------------------------------- |----------- |--------- |---------:|---------:|---------:|---------:|------:|--------:|------------:|-----------:|-----------:|----------:|
|       **CsvReader_GetAllRowsFromFile** |      **16384** |    **False** |  **5.159 s** | **0.3866 s** | **1.1399 s** |  **5.096 s** |  **1.00** |    **0.00** | **287000.0000** |          **-** |          **-** |    **574 MB** |
| CsvMemoryReader_GetAllRowsFromFile |      16384 |    False |  4.795 s | 0.3512 s | 1.0243 s |  4.593 s |  0.98 |    0.32 | 252000.0000 |          - |          - |    505 MB |
|    CsvReader_ReadRowsBufferedAsync |      16384 |    False | 11.131 s | 0.5835 s | 1.7206 s | 11.841 s |  2.25 |    0.55 |  97000.0000 | 33000.0000 |  1000.0000 |    591 MB |
|            CsvReader_PipeRowsAsync |      16384 |    False |  9.853 s | 0.4253 s | 1.2540 s | 10.296 s |  2.02 |    0.59 | 130000.0000 | 65000.0000 | 19000.0000 |    643 MB |
|                                    |            |          |          |          |          |          |       |         |             |            |            |           |
|       **CsvReader_GetAllRowsFromFile** |      **16384** |     **True** |  **6.021 s** | **0.3870 s** | **1.1411 s** |  **6.222 s** |  **1.00** |    **0.00** | **287000.0000** |          **-** |          **-** |    **575 MB** |
| CsvMemoryReader_GetAllRowsFromFile |      16384 |     True |  7.223 s | 0.3959 s | 1.1674 s |  7.369 s |  1.25 |    0.32 | 253000.0000 |          - |          - |    506 MB |
|    CsvReader_ReadRowsBufferedAsync |      16384 |     True | 11.603 s | 0.5664 s | 1.6702 s | 12.203 s |  2.00 |    0.50 |  97000.0000 | 33000.0000 |  1000.0000 |    591 MB |
|            CsvReader_PipeRowsAsync |      16384 |     True |  8.974 s | 0.6146 s | 1.8120 s |  9.260 s |  1.54 |    0.42 | 122000.0000 | 61000.0000 | 20000.0000 |    640 MB |
|                                    |            |          |          |          |          |          |       |         |             |            |            |           |
|       **CsvReader_GetAllRowsFromFile** |      **32768** |    **False** |  **3.747 s** | **0.2340 s** | **0.6900 s** |  **3.712 s** |  **1.00** |    **0.00** | **286000.0000** |          **-** |          **-** |    **574 MB** |
| CsvMemoryReader_GetAllRowsFromFile |      32768 |    False |  3.740 s | 0.2027 s | 0.5913 s |  3.527 s |  1.03 |    0.25 | 252000.0000 |          - |          - |    505 MB |
|    CsvReader_ReadRowsBufferedAsync |      32768 |    False |  6.614 s | 0.3223 s | 0.9452 s |  6.605 s |  1.82 |    0.43 |  97000.0000 | 34000.0000 |  1000.0000 |    590 MB |
|            CsvReader_PipeRowsAsync |      32768 |    False |  7.160 s | 0.3483 s | 1.0270 s |  7.351 s |  1.96 |    0.40 | 129000.0000 | 64000.0000 | 18000.0000 |    641 MB |
|                                    |            |          |          |          |          |          |       |         |             |            |            |           |
|       **CsvReader_GetAllRowsFromFile** |      **32768** |     **True** |  **3.482 s** | **0.1803 s** | **0.5316 s** |  **3.333 s** |  **1.00** |    **0.00** | **287000.0000** |          **-** |          **-** |    **574 MB** |
| CsvMemoryReader_GetAllRowsFromFile |      32768 |     True |  3.756 s | 0.1437 s | 0.4170 s |  3.672 s |  1.11 |    0.19 | 252000.0000 |          - |          - |    505 MB |
|    CsvReader_ReadRowsBufferedAsync |      32768 |     True |  6.899 s | 0.3092 s | 0.9116 s |  6.712 s |  2.02 |    0.40 |  97000.0000 | 33000.0000 |  1000.0000 |    590 MB |
|            CsvReader_PipeRowsAsync |      32768 |     True |  6.057 s | 0.1566 s | 0.4594 s |  6.095 s |  1.78 |    0.28 | 121000.0000 | 60000.0000 | 19000.0000 |    639 MB |