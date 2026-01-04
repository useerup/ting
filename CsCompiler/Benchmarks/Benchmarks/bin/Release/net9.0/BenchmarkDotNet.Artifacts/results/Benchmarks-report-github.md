```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2605)
Intel Core i9-10900 CPU 2.80GHz, 1 CPU, 20 logical and 10 physical cores
.NET SDK 9.0.100
  [Host]   : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  .NET 9.0 : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2


```
| Method | Job           | Runtime       | Mean    | Error    | StdDev   |
|------- |-------------- |-------------- |--------:|---------:|---------:|
| Loops1 | .NET 9.0      | .NET 9.0      | 1.041 s | 0.0203 s | 0.0234 s |
| Loops2 | .NET 9.0      | .NET 9.0      | 1.021 s | 0.0197 s | 0.0219 s |
| Loops1 | NativeAOT 9.0 | NativeAOT 9.0 |      NA |       NA |       NA |
| Loops2 | NativeAOT 9.0 | NativeAOT 9.0 |      NA |       NA |       NA |

Benchmarks with issues:
  Benchmarks.Loops1: NativeAOT 9.0(Runtime=NativeAOT 9.0)
  Benchmarks.Loops2: NativeAOT 9.0(Runtime=NativeAOT 9.0)
