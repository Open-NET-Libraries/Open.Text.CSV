``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1237 (21H1/May2021Update)
AMD Ryzen 5 4500U with Radeon Graphics, 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.100-rc.1.21463.6
  [Host]     : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.45113), X64 RyuJIT


```
|                                 Method | FileStreamBufferSize | ByteBufferSize | UseAsync |      Mean |     Error |     StdDev |    Median | Ratio | RatioSD |     Gen 0 | Allocated |
|--------------------------------------- |--------------------- |--------------- |--------- |----------:|----------:|-----------:|----------:|------:|--------:|----------:|----------:|
|              **PipeReader_EnumerateAsync** |                    **1** |           **4092** |    **False** | **105.81 ms** |  **1.233 ms** |   **1.990 ms** | **106.11 ms** |  **2.10** |    **0.03** | **2000.0000** |  **4,241 KB** |
|                        FileStream_Read |                    1 |           4092 |    False |  50.49 ms |  0.175 ms |   0.164 ms |  50.45 ms |  1.00 |    0.00 |         - |      5 KB |
|                   FileStream_ReadAsync |                    1 |           4092 |    False |  65.53 ms |  0.342 ms |   0.303 ms |  65.47 ms |  1.30 |    0.01 |         - |      5 KB |
|       FileStream_SingleBufferReadAsync |                    1 |           4092 |    False | 315.25 ms |  6.259 ms |  16.815 ms | 320.58 ms |  5.80 |    0.60 |         - |      2 KB |
|              FileStream_EnumerateAsync |                    1 |           4092 |    False | 341.66 ms |  6.645 ms |   7.911 ms | 342.05 ms |  6.72 |    0.15 |         - |      4 KB |
|      FileStream_EnumerateAsync_Yielded |                    1 |           4092 |    False | 349.74 ms |  4.217 ms |   3.945 ms | 349.64 ms |  6.93 |    0.09 |         - |      3 KB |
|         FileStream_DualBufferReadAsync |                    1 |           4092 |    False |  73.78 ms |  0.899 ms |   0.751 ms |  73.81 ms |  1.46 |    0.02 |         - |      2 KB |
| FileStream_DualBufferReadAsync_Yielded |                    1 |           4092 |    False |  92.08 ms |  0.394 ms |   0.369 ms |  92.09 ms |  1.82 |    0.01 |         - |      2 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                    **1** |           **4092** |     **True** | **232.76 ms** |  **4.394 ms** |   **6.302 ms** | **234.00 ms** |  **1.91** |    **0.06** | **2000.0000** |  **4,236 KB** |
|                        FileStream_Read |                    1 |           4092 |     True | 121.98 ms |  0.723 ms |   1.104 ms | 121.79 ms |  1.00 |    0.00 | 2000.0000 |  4,244 KB |
|                   FileStream_ReadAsync |                    1 |           4092 |     True | 182.95 ms |  3.656 ms |   9.172 ms | 181.62 ms |  1.50 |    0.07 |         - |      6 KB |
|       FileStream_SingleBufferReadAsync |                    1 |           4092 |     True | 221.96 ms |  4.390 ms |   6.704 ms | 221.25 ms |  1.82 |    0.06 |         - |      2 KB |
|              FileStream_EnumerateAsync |                    1 |           4092 |     True | 215.02 ms |  3.014 ms |   2.517 ms | 214.88 ms |  1.75 |    0.03 |         - |      5 KB |
|      FileStream_EnumerateAsync_Yielded |                    1 |           4092 |     True | 428.22 ms | 37.967 ms | 111.947 ms | 464.69 ms |  3.88 |    0.52 |         - |      3 KB |
|         FileStream_DualBufferReadAsync |                    1 |           4092 |     True | 227.09 ms |  4.698 ms |  13.629 ms | 231.37 ms |  1.76 |    0.13 |         - |      3 KB |
| FileStream_DualBufferReadAsync_Yielded |                    1 |           4092 |     True | 554.14 ms |  3.689 ms |   3.270 ms | 554.90 ms |  4.52 |    0.05 |         - |      7 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                    **1** |          **16384** |    **False** |  **52.40 ms** |  **7.542 ms** |  **22.001 ms** |  **40.10 ms** |  **3.79** |    **0.72** |  **500.0000** |  **1,077 KB** |
|                        FileStream_Read |                    1 |          16384 |    False |  22.52 ms |  0.081 ms |   0.076 ms |  22.54 ms |  1.00 |    0.00 |         - |     17 KB |
|                   FileStream_ReadAsync |                    1 |          16384 |    False |  27.92 ms |  0.423 ms |   0.396 ms |  27.80 ms |  1.24 |    0.02 |         - |     18 KB |
|       FileStream_SingleBufferReadAsync |                    1 |          16384 |    False |  44.80 ms |  6.095 ms |  17.972 ms |  33.80 ms |  1.48 |    0.04 |         - |      1 KB |
|              FileStream_EnumerateAsync |                    1 |          16384 |    False |  34.55 ms |  0.137 ms |   0.122 ms |  34.59 ms |  1.53 |    0.01 |         - |      1 KB |
|      FileStream_EnumerateAsync_Yielded |                    1 |          16384 |    False | 115.90 ms |  1.772 ms |   1.657 ms | 115.42 ms |  5.15 |    0.08 |         - |      2 KB |
|         FileStream_DualBufferReadAsync |                    1 |          16384 |    False |  31.82 ms |  0.620 ms |   1.347 ms |  31.37 ms |  1.41 |    0.10 |         - |      2 KB |
| FileStream_DualBufferReadAsync_Yielded |                    1 |          16384 |    False |  35.95 ms |  0.061 ms |   0.054 ms |  35.93 ms |  1.60 |    0.01 |         - |      1 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                    **1** |          **16384** |     **True** |  **72.70 ms** |  **1.212 ms** |   **1.923 ms** |  **72.87 ms** |  **1.75** |    **0.07** |  **500.0000** |  **1,076 KB** |
|                        FileStream_Read |                    1 |          16384 |     True |  41.01 ms |  0.193 ms |   0.171 ms |  40.99 ms |  1.00 |    0.00 |  461.5385 |  1,076 KB |
|                   FileStream_ReadAsync |                    1 |          16384 |     True |  68.69 ms |  0.790 ms |   0.739 ms |  68.79 ms |  1.67 |    0.02 |         - |     19 KB |
|       FileStream_SingleBufferReadAsync |                    1 |          16384 |     True |  63.39 ms |  1.244 ms |   1.573 ms |  63.41 ms |  1.53 |    0.04 |         - |      2 KB |
|              FileStream_EnumerateAsync |                    1 |          16384 |     True | 160.05 ms |  3.125 ms |   4.773 ms | 162.03 ms |  3.84 |    0.15 |         - |      2 KB |
|      FileStream_EnumerateAsync_Yielded |                    1 |          16384 |     True | 191.87 ms |  6.885 ms |  20.301 ms | 201.08 ms |  4.01 |    0.53 |         - |      2 KB |
|         FileStream_DualBufferReadAsync |                    1 |          16384 |     True |  69.83 ms |  7.737 ms |  21.048 ms |  61.61 ms |  2.73 |    0.58 |         - |      2 KB |
| FileStream_DualBufferReadAsync_Yielded |                    1 |          16384 |     True | 159.14 ms |  0.564 ms |   0.527 ms | 159.08 ms |  3.88 |    0.02 |         - |      3 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                    **1** |          **65536** |    **False** |  **55.89 ms** |  **0.832 ms** |   **0.778 ms** |  **55.88 ms** |  **3.77** |    **0.05** |  **111.1111** |    **333 KB** |
|                        FileStream_Read |                    1 |          65536 |    False |  14.81 ms |  0.027 ms |   0.025 ms |  14.81 ms |  1.00 |    0.00 |   15.6250 |     65 KB |
|                   FileStream_ReadAsync |                    1 |          65536 |    False |  17.12 ms |  0.064 ms |   0.060 ms |  17.14 ms |  1.16 |    0.00 |         - |     65 KB |
|       FileStream_SingleBufferReadAsync |                    1 |          65536 |    False |  27.12 ms |  3.813 ms |  11.242 ms |  19.85 ms |  3.07 |    0.13 |         - |      1 KB |
|              FileStream_EnumerateAsync |                    1 |          65536 |    False |  51.09 ms |  0.305 ms |   0.286 ms |  51.14 ms |  3.45 |    0.02 |         - |      1 KB |
|      FileStream_EnumerateAsync_Yielded |                    1 |          65536 |    False |  74.25 ms |  0.626 ms |   0.585 ms |  74.17 ms |  5.01 |    0.04 |         - |      2 KB |
|         FileStream_DualBufferReadAsync |                    1 |          65536 |    False |  52.26 ms |  0.190 ms |   0.178 ms |  52.27 ms |  3.53 |    0.01 |         - |      1 KB |
| FileStream_DualBufferReadAsync_Yielded |                    1 |          65536 |    False |  50.27 ms |  0.181 ms |   0.169 ms |  50.31 ms |  3.39 |    0.01 |         - |      1 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                    **1** |          **65536** |     **True** |  **56.17 ms** |  **2.169 ms** |   **6.396 ms** |  **56.93 ms** |  **2.05** |    **0.70** |  **100.0000** |    **330 KB** |
|                        FileStream_Read |                    1 |          65536 |     True |  30.37 ms |  3.147 ms |   9.278 ms |  31.36 ms |  1.00 |    0.00 |   83.3333 |    330 KB |
|                   FileStream_ReadAsync |                    1 |          65536 |     True |  51.55 ms |  4.744 ms |  13.988 ms |  58.27 ms |  1.82 |    0.69 |         - |     65 KB |
|       FileStream_SingleBufferReadAsync |                    1 |          65536 |     True |  50.53 ms |  3.904 ms |  11.512 ms |  54.41 ms |  1.78 |    0.59 |         - |      2 KB |
|              FileStream_EnumerateAsync |                    1 |          65536 |     True |  59.59 ms |  3.718 ms |  10.963 ms |  64.62 ms |  2.17 |    0.82 |         - |      2 KB |
|      FileStream_EnumerateAsync_Yielded |                    1 |          65536 |     True |  70.06 ms |  1.105 ms |   1.034 ms |  69.95 ms |  2.30 |    0.75 |         - |      2 KB |
|         FileStream_DualBufferReadAsync |                    1 |          65536 |     True |  56.10 ms |  4.463 ms |  13.158 ms |  62.17 ms |  2.03 |    0.77 |         - |      2 KB |
| FileStream_DualBufferReadAsync_Yielded |                    1 |          65536 |     True |  59.29 ms |  0.308 ms |   0.273 ms |  59.34 ms |  1.96 |    0.63 |         - |      2 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                 **8192** |           **4092** |    **False** | **165.98 ms** | **25.339 ms** |  **74.713 ms** | **205.75 ms** |  **5.75** |    **0.30** | **1000.0000** |  **2,133 KB** |
|                        FileStream_Read |                 8192 |           4092 |    False |  39.28 ms |  0.110 ms |   0.103 ms |  39.32 ms |  1.00 |    0.00 |         - |     13 KB |
|                   FileStream_ReadAsync |                 8192 |           4092 |    False | 114.34 ms |  9.211 ms |  24.585 ms | 128.98 ms |  2.88 |    0.45 |         - |     13 KB |
|       FileStream_SingleBufferReadAsync |                 8192 |           4092 |    False |  55.52 ms |  1.107 ms |   2.285 ms |  55.24 ms |  1.43 |    0.07 |         - |     10 KB |
|              FileStream_EnumerateAsync |                 8192 |           4092 |    False |  58.82 ms |  1.167 ms |   2.383 ms |  58.29 ms |  1.49 |    0.07 |         - |     10 KB |
|      FileStream_EnumerateAsync_Yielded |                 8192 |           4092 |    False | 245.99 ms |  4.865 ms |   5.206 ms | 246.94 ms |  6.24 |    0.14 |         - |    192 KB |
|         FileStream_DualBufferReadAsync |                 8192 |           4092 |    False |  53.17 ms |  0.929 ms |   1.979 ms |  52.85 ms |  1.34 |    0.07 |         - |     10 KB |
| FileStream_DualBufferReadAsync_Yielded |                 8192 |           4092 |    False |  81.54 ms |  0.875 ms |   0.818 ms |  81.51 ms |  2.08 |    0.02 |         - |    193 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                 **8192** |           **4092** |     **True** | **284.04 ms** | **19.231 ms** |  **56.704 ms** | **310.10 ms** |  **3.55** |    **0.33** | **1000.0000** |  **2,125 KB** |
|                        FileStream_Read |                 8192 |           4092 |     True |  87.79 ms |  0.841 ms |   0.787 ms |  88.18 ms |  1.00 |    0.00 | 1333.3333 |  2,839 KB |
|                   FileStream_ReadAsync |                 8192 |           4092 |     True | 117.45 ms |  2.283 ms |   2.718 ms | 117.32 ms |  1.33 |    0.03 |         - |     14 KB |
|       FileStream_SingleBufferReadAsync |                 8192 |           4092 |     True | 120.22 ms |  2.239 ms |   2.095 ms | 120.96 ms |  1.37 |    0.02 |         - |     11 KB |
|              FileStream_EnumerateAsync |                 8192 |           4092 |     True | 128.68 ms |  2.530 ms |   3.864 ms | 128.90 ms |  1.44 |    0.04 |         - |     10 KB |
|      FileStream_EnumerateAsync_Yielded |                 8192 |           4092 |     True | 353.76 ms |  5.350 ms |   5.004 ms | 353.80 ms |  4.03 |    0.06 |         - |     73 KB |
|         FileStream_DualBufferReadAsync |                 8192 |           4092 |     True | 302.86 ms |  4.883 ms |   4.567 ms | 302.32 ms |  3.45 |    0.05 |         - |     11 KB |
| FileStream_DualBufferReadAsync_Yielded |                 8192 |           4092 |     True | 359.21 ms |  5.926 ms |   5.543 ms | 359.26 ms |  4.09 |    0.08 |         - |    253 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                 **8192** |          **16384** |    **False** | **139.68 ms** |  **0.790 ms** |   **0.739 ms** | **139.86 ms** |  **6.21** |    **0.04** |  **500.0000** |  **1,077 KB** |
|                        FileStream_Read |                 8192 |          16384 |    False |  22.51 ms |  0.049 ms |   0.046 ms |  22.51 ms |  1.00 |    0.00 |         - |     17 KB |
|                   FileStream_ReadAsync |                 8192 |          16384 |    False |  29.32 ms |  0.152 ms |   0.119 ms |  29.38 ms |  1.30 |    0.01 |         - |     17 KB |
|       FileStream_SingleBufferReadAsync |                 8192 |          16384 |    False |  89.98 ms |  1.309 ms |   2.292 ms |  90.20 ms |  4.00 |    0.16 |         - |      2 KB |
|              FileStream_EnumerateAsync |                 8192 |          16384 |    False | 118.35 ms |  1.306 ms |   1.158 ms | 118.41 ms |  5.26 |    0.05 |         - |      6 KB |
|      FileStream_EnumerateAsync_Yielded |                 8192 |          16384 |    False | 143.05 ms |  1.422 ms |   1.330 ms | 143.26 ms |  6.36 |    0.06 |         - |      2 KB |
|         FileStream_DualBufferReadAsync |                 8192 |          16384 |    False |  37.72 ms |  3.601 ms |  10.274 ms |  32.42 ms |  2.53 |    0.38 |         - |      2 KB |
| FileStream_DualBufferReadAsync_Yielded |                 8192 |          16384 |    False |  37.58 ms |  0.073 ms |   0.068 ms |  37.57 ms |  1.67 |    0.00 |         - |      2 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                 **8192** |          **16384** |     **True** |  **72.54 ms** |  **0.917 ms** |   **0.813 ms** |  **72.43 ms** |  **1.76** |    **0.02** |  **400.0000** |  **1,078 KB** |
|                        FileStream_Read |                 8192 |          16384 |     True |  41.29 ms |  0.185 ms |   0.154 ms |  41.26 ms |  1.00 |    0.00 |  461.5385 |  1,076 KB |
|                   FileStream_ReadAsync |                 8192 |          16384 |     True |  68.58 ms |  0.574 ms |   0.537 ms |  68.67 ms |  1.66 |    0.01 |         - |     17 KB |
|       FileStream_SingleBufferReadAsync |                 8192 |          16384 |     True |  63.08 ms |  1.258 ms |   1.883 ms |  62.70 ms |  1.52 |    0.06 |         - |      2 KB |
|              FileStream_EnumerateAsync |                 8192 |          16384 |     True |  66.36 ms |  1.301 ms |   2.378 ms |  66.13 ms |  1.59 |    0.05 |         - |      2 KB |
|      FileStream_EnumerateAsync_Yielded |                 8192 |          16384 |     True | 151.30 ms | 18.953 ms |  55.882 ms | 186.43 ms |  3.94 |    0.98 |         - |      2 KB |
|         FileStream_DualBufferReadAsync |                 8192 |          16384 |     True | 134.66 ms |  8.433 ms |  24.865 ms | 144.66 ms |  3.83 |    0.18 |         - |      2 KB |
| FileStream_DualBufferReadAsync_Yielded |                 8192 |          16384 |     True | 160.42 ms |  0.335 ms |   0.297 ms | 160.49 ms |  3.88 |    0.02 |         - |      2 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                 **8192** |          **65536** |    **False** |  **56.68 ms** |  **0.626 ms** |   **0.585 ms** |  **56.37 ms** |  **3.82** |    **0.04** |  **111.1111** |    **331 KB** |
|                        FileStream_Read |                 8192 |          65536 |    False |  14.85 ms |  0.022 ms |   0.020 ms |  14.86 ms |  1.00 |    0.00 |   15.6250 |     65 KB |
|                   FileStream_ReadAsync |                 8192 |          65536 |    False |  17.13 ms |  0.188 ms |   0.176 ms |  17.21 ms |  1.15 |    0.01 |         - |     65 KB |
|       FileStream_SingleBufferReadAsync |                 8192 |          65536 |    False |  48.50 ms |  0.268 ms |   0.250 ms |  48.39 ms |  3.27 |    0.02 |         - |      2 KB |
|              FileStream_EnumerateAsync |                 8192 |          65536 |    False |  51.71 ms |  0.701 ms |   0.655 ms |  51.61 ms |  3.48 |    0.04 |         - |      2 KB |
|      FileStream_EnumerateAsync_Yielded |                 8192 |          65536 |    False |  63.93 ms |  0.814 ms |   0.761 ms |  63.61 ms |  4.30 |    0.05 |         - |      2 KB |
|         FileStream_DualBufferReadAsync |                 8192 |          65536 |    False |  52.15 ms |  0.223 ms |   0.209 ms |  52.07 ms |  3.51 |    0.01 |         - |      8 KB |
| FileStream_DualBufferReadAsync_Yielded |                 8192 |          65536 |    False |  44.06 ms |  0.119 ms |   0.106 ms |  44.03 ms |  2.97 |    0.01 |         - |      2 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                 **8192** |          **65536** |     **True** |  **61.30 ms** |  **2.699 ms** |   **7.957 ms** |  **63.91 ms** |  **2.30** |    **0.78** |  **100.0000** |    **331 KB** |
|                        FileStream_Read |                 8192 |          65536 |     True |  29.42 ms |  3.128 ms |   9.224 ms |  28.14 ms |  1.00 |    0.00 |  142.8571 |    330 KB |
|                   FileStream_ReadAsync |                 8192 |          65536 |     True |  56.38 ms |  1.800 ms |   5.307 ms |  58.16 ms |  2.11 |    0.67 |         - |     65 KB |
|       FileStream_SingleBufferReadAsync |                 8192 |          65536 |     True |  52.94 ms |  3.776 ms |  11.133 ms |  56.90 ms |  1.99 |    0.75 |         - |      2 KB |
|              FileStream_EnumerateAsync |                 8192 |          65536 |     True |  56.95 ms |  5.358 ms |  15.797 ms |  63.74 ms |  2.12 |    0.86 |         - |      2 KB |
|      FileStream_EnumerateAsync_Yielded |                 8192 |          65536 |     True |  53.34 ms |  6.448 ms |  19.013 ms |  45.70 ms |  2.01 |    1.00 |         - |      2 KB |
|         FileStream_DualBufferReadAsync |                 8192 |          65536 |     True |  62.91 ms |  2.388 ms |   7.041 ms |  63.73 ms |  2.35 |    0.75 |         - |      2 KB |
| FileStream_DualBufferReadAsync_Yielded |                 8192 |          65536 |     True |  52.27 ms |  0.677 ms |   0.633 ms |  52.46 ms |  2.16 |    0.59 |         - |      2 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                **32768** |           **4092** |    **False** | **105.03 ms** |  **0.738 ms** |   **0.690 ms** | **104.87 ms** |  **2.06** |    **0.22** |  **200.0000** |    **571 KB** |
|                        FileStream_Read |                32768 |           4092 |    False |  48.25 ms |  2.841 ms |   8.377 ms |  50.97 ms |  1.00 |    0.00 |         - |     37 KB |
|                   FileStream_ReadAsync |                32768 |           4092 |    False |  68.93 ms |  1.372 ms |   1.580 ms |  69.35 ms |  1.43 |    0.39 |         - |     37 KB |
|       FileStream_SingleBufferReadAsync |                32768 |           4092 |    False |  32.71 ms |  1.297 ms |   3.574 ms |  30.79 ms |  0.71 |    0.20 |         - |     34 KB |
|              FileStream_EnumerateAsync |                32768 |           4092 |    False |  97.98 ms |  1.380 ms |   1.291 ms |  98.36 ms |  1.92 |    0.20 |         - |     37 KB |
|      FileStream_EnumerateAsync_Yielded |                32768 |           4092 |    False |  68.37 ms |  3.010 ms |   8.875 ms |  65.77 ms |  1.48 |    0.41 |         - |    157 KB |
|         FileStream_DualBufferReadAsync |                32768 |           4092 |    False |  92.38 ms |  0.367 ms |   0.343 ms |  92.47 ms |  1.81 |    0.19 |         - |     34 KB |
| FileStream_DualBufferReadAsync_Yielded |                32768 |           4092 |    False |  60.12 ms |  1.198 ms |   2.002 ms |  60.12 ms |  1.29 |    0.37 |         - |    182 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                **32768** |           **4092** |     **True** | **118.87 ms** |  **1.137 ms** |   **1.063 ms** | **119.13 ms** |  **1.25** |    **0.06** |  **200.0000** |    **568 KB** |
|                        FileStream_Read |                32768 |           4092 |     True |  89.47 ms |  5.407 ms |  15.942 ms |  95.38 ms |  1.00 |    0.00 |  333.3333 |    979 KB |
|                   FileStream_ReadAsync |                32768 |           4092 |     True | 104.58 ms |  2.069 ms |   3.399 ms | 104.35 ms |  1.11 |    0.14 |         - |     38 KB |
|       FileStream_SingleBufferReadAsync |                32768 |           4092 |     True |  88.43 ms |  5.936 ms |  17.503 ms |  84.27 ms |  1.03 |    0.29 |         - |     34 KB |
|              FileStream_EnumerateAsync |                32768 |           4092 |     True |  95.73 ms |  5.590 ms |  16.482 ms | 101.25 ms |  1.12 |    0.33 |         - |     34 KB |
|      FileStream_EnumerateAsync_Yielded |                32768 |           4092 |     True |  87.63 ms |  1.619 ms |   3.344 ms |  87.43 ms |  1.03 |    0.30 |         - |     67 KB |
|         FileStream_DualBufferReadAsync |                32768 |           4092 |     True |  93.34 ms |  4.680 ms |  13.799 ms |  93.30 ms |  1.09 |    0.33 |         - |     34 KB |
| FileStream_DualBufferReadAsync_Yielded |                32768 |           4092 |     True |  79.13 ms |  1.578 ms |   2.052 ms |  79.62 ms |  0.85 |    0.15 |         - |     96 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                **32768** |          **16384** |    **False** |  **53.59 ms** |  **3.190 ms** |   **9.406 ms** |  **48.90 ms** |  **1.53** |    **0.54** |  **285.7143** |    **580 KB** |
|                        FileStream_Read |                32768 |          16384 |    False |  37.42 ms |  2.679 ms |   7.900 ms |  40.64 ms |  1.00 |    0.00 |         - |     49 KB |
|                   FileStream_ReadAsync |                32768 |          16384 |    False |  57.11 ms |  4.396 ms |  12.961 ms |  62.97 ms |  1.64 |    0.67 |         - |     49 KB |
|       FileStream_SingleBufferReadAsync |                32768 |          16384 |    False |  34.66 ms |  1.393 ms |   3.812 ms |  33.77 ms |  1.00 |    0.34 |         - |     34 KB |
|              FileStream_EnumerateAsync |                32768 |          16384 |    False |  47.71 ms |  3.745 ms |  11.041 ms |  43.24 ms |  1.36 |    0.49 |         - |     34 KB |
|      FileStream_EnumerateAsync_Yielded |                32768 |          16384 |    False |  37.09 ms |  0.465 ms |   0.388 ms |  36.95 ms |  1.06 |    0.36 |         - |     59 KB |
|         FileStream_DualBufferReadAsync |                32768 |          16384 |    False |  68.30 ms |  3.138 ms |   9.253 ms |  71.62 ms |  1.95 |    0.68 |         - |     34 KB |
| FileStream_DualBufferReadAsync_Yielded |                32768 |          16384 |    False |  42.92 ms |  0.484 ms |   0.453 ms |  42.98 ms |  1.23 |    0.39 |         - |     51 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                **32768** |          **16384** |     **True** |  **96.18 ms** |  **1.917 ms** |   **4.593 ms** |  **97.96 ms** |  **1.96** |    **0.70** |  **200.0000** |    **579 KB** |
|                        FileStream_Read |                32768 |          16384 |     True |  46.71 ms |  5.744 ms |  16.937 ms |  42.85 ms |  1.00 |    0.00 |  142.8571 |    578 KB |
|                   FileStream_ReadAsync |                32768 |          16384 |     True |  94.61 ms |  6.457 ms |  19.038 ms | 105.65 ms |  2.30 |    0.92 |         - |     50 KB |
|       FileStream_SingleBufferReadAsync |                32768 |          16384 |     True |  98.48 ms |  1.842 ms |   1.891 ms |  98.05 ms |  1.50 |    0.11 |         - |     34 KB |
|              FileStream_EnumerateAsync |                32768 |          16384 |     True | 103.24 ms |  2.061 ms |   3.663 ms | 103.65 ms |  1.95 |    0.70 |         - |     34 KB |
|      FileStream_EnumerateAsync_Yielded |                32768 |          16384 |     True | 162.72 ms |  1.355 ms |   1.268 ms | 162.69 ms |  2.49 |    0.20 |         - |     37 KB |
|         FileStream_DualBufferReadAsync |                32768 |          16384 |     True |  99.40 ms |  0.750 ms |   0.701 ms |  99.38 ms |  1.52 |    0.12 |         - |     34 KB |
| FileStream_DualBufferReadAsync_Yielded |                32768 |          16384 |     True | 110.28 ms |  1.449 ms |   1.355 ms | 110.14 ms |  1.69 |    0.12 |         - |     90 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                **32768** |          **65536** |    **False** |  **57.44 ms** |  **0.980 ms** |   **0.917 ms** |  **57.48 ms** |  **3.87** |    **0.06** |  **111.1111** |    **331 KB** |
|                        FileStream_Read |                32768 |          65536 |    False |  14.82 ms |  0.027 ms |   0.025 ms |  14.83 ms |  1.00 |    0.00 |   15.6250 |     65 KB |
|                   FileStream_ReadAsync |                32768 |          65536 |    False |  17.56 ms |  0.280 ms |   0.344 ms |  17.49 ms |  1.19 |    0.03 |         - |     65 KB |
|       FileStream_SingleBufferReadAsync |                32768 |          65536 |    False |  38.93 ms |  1.473 ms |   4.105 ms |  40.77 ms |  2.27 |    0.39 |         - |      1 KB |
|              FileStream_EnumerateAsync |                32768 |          65536 |    False |  42.52 ms |  0.358 ms |   0.477 ms |  42.55 ms |  2.86 |    0.04 |         - |      2 KB |
|      FileStream_EnumerateAsync_Yielded |                32768 |          65536 |    False |  30.87 ms |  0.330 ms |   0.309 ms |  31.01 ms |  2.08 |    0.02 |         - |      2 KB |
|         FileStream_DualBufferReadAsync |                32768 |          65536 |    False |  39.04 ms |  0.311 ms |   0.276 ms |  39.06 ms |  2.63 |    0.02 |         - |      2 KB |
| FileStream_DualBufferReadAsync_Yielded |                32768 |          65536 |    False |  23.10 ms |  0.425 ms |   1.089 ms |  22.79 ms |  1.65 |    0.12 |         - |      2 KB |
|                                        |                      |                |          |           |           |            |           |       |         |           |           |
|              **PipeReader_EnumerateAsync** |                **32768** |          **65536** |     **True** |  **67.75 ms** |  **0.548 ms** |   **0.513 ms** |  **67.71 ms** |  **1.78** |    **0.58** |  **125.0000** |    **329 KB** |
|                        FileStream_Read |                32768 |          65536 |     True |  43.10 ms |  2.104 ms |   6.203 ms |  44.86 ms |  1.00 |    0.00 |   90.9091 |    330 KB |
|                   FileStream_ReadAsync |                32768 |          65536 |     True |  61.58 ms |  1.223 ms |   1.359 ms |  61.67 ms |  1.57 |    0.48 |         - |     65 KB |
|       FileStream_SingleBufferReadAsync |                32768 |          65536 |     True |  59.05 ms |  2.035 ms |   6.001 ms |  61.42 ms |  1.41 |    0.32 |         - |      2 KB |
|              FileStream_EnumerateAsync |                32768 |          65536 |     True |  60.08 ms |  3.674 ms |  10.834 ms |  63.38 ms |  1.45 |    0.46 |         - |      2 KB |
|      FileStream_EnumerateAsync_Yielded |                32768 |          65536 |     True |  65.62 ms |  5.541 ms |  16.338 ms |  74.18 ms |  1.56 |    0.49 |         - |      2 KB |
|         FileStream_DualBufferReadAsync |                32768 |          65536 |     True |  59.30 ms |  1.044 ms |   0.976 ms |  58.98 ms |  1.55 |    0.50 |         - |      2 KB |
| FileStream_DualBufferReadAsync_Yielded |                32768 |          65536 |     True |  42.01 ms |  5.044 ms |  14.873 ms |  30.85 ms |  1.01 |    0.44 |         - |      2 KB |
