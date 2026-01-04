using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;


[SimpleJob(RuntimeMoniker.NativeAot90)]
[SimpleJob(RuntimeMoniker.Net90)]
public class Benchmarks
{
    [Benchmark]
    public void Loops1()
    {
        int u = 100000;
        var r = Random.Shared.Next(10_000);

        //Use span instead of array
        Span<int> a = stackalloc int[10_000];

        //using a.Length instead of 10_000 constant should give the compiler better optimization hints
        for (var i = 0; i < a.Length; i++)
        {
            for (var j = 0; j < 100_000; j++)
            {
                a[i] += j % u;
            }

            a[i] += r;
        }
        Console.WriteLine(a[r]);
    }


    [Benchmark]
    public void Loops2()
    {
        int u = 100000;
        var r = Random.Shared.Next(10_000);

        //Use span instead of array
        Span<int> a = stackalloc int[10_000];

        //using a.Length instead of 10_000 constant should give the compiler better optimization hints
        for (var i = 0; i < a.Length; i++)
        {
            for (var j = 0; j < 100_000; j++)
            {
                unchecked
                {
                    a[i] += j % u;
                }
            }

            unchecked
            {
                a[i] += r;
            }
        }
        Console.WriteLine(a[r]);
    }


}


public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<Benchmarks>();
    }
}

