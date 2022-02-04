using System.Numerics;
using System.Security.Cryptography;
using Aprismatic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Running;

namespace MyBenchmark
{
    [SimpleJob(runStrategy: RunStrategy.Throughput, invocationCount: 1)]
    public class Bench
    {
        private readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        [Params(128,256,512/*,1024,2048,4096,8192*/)]
        public int bit;

        [Params(1,2,5,10,20/*,30,40,50,75,100,125,150,200,250,300,350,400,500,600,700,800,900,1000*/)]
        public ulong prims;

        private BigInteger[] numbers;
        private const int amount = 10000;
        private int index;

        [GlobalSetup]
        public void Setup()
        {
            numbers = new BigInteger[amount];
            for (var i = 0; i < amount; i++)
            {
                do
                {
                    numbers[i] = BigInteger.One.GenRandomBits(bit, rng);
                } while (numbers[i] % 2 == 0 ||
                         numbers[i] % 3 == 0 ||
                         numbers[i] % 5 == 0 ||
                         numbers[i] % 7 == 0 ||
                         numbers[i] % 11 == 0);
            }
        }

        [IterationSetup]
        public void IterationSetup()
        {
            BigIntegerExt.PrimesBelow2000 = BigIntegerExt.PrimesBelow1M.Where(x => x < prims * 1000).OrderBy(x => x).ToArray();
            BigIntegerExt.PrimesBelow2000_BI = BigIntegerExt.PrimesBelow2000.Select(p => (BigInteger) p).ToArray();

            if (BigIntegerExt.PrimesBelow2000[^1] >= prims*1000) throw new Exception("selection");
            if (BigIntegerExt.PrimesBelow2000.Length != BigIntegerExt.PrimesBelow2000_BI.Length) throw new Exception("lengths");
            for (var i = 0; i < BigIntegerExt.PrimesBelow2000.Length; i++)
                if (BigIntegerExt.PrimesBelow2000[i] != BigIntegerExt.PrimesBelow2000_BI[i]) throw new Exception("values");

            index = 0;
        }

        [Benchmark]
        public bool c4() => numbers[index++].IsProbablePrime(4, rng);

        [Benchmark]
        public bool c10() => numbers[index++].IsProbablePrime(10, rng);

        [Benchmark]
        public bool c16() => numbers[index++].IsProbablePrime(16, rng);
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<Bench>(
                DefaultConfig.Instance
                    .AddExporter(CsvMeasurementsExporter.Default)
                    .AddExporter(RPlotExporter.Default)
                    );
        }
    }
}
