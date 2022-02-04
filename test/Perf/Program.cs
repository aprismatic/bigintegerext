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
    [CsvExporter]
    [CsvMeasurementsExporter]
    [SimpleJob(runStrategy: RunStrategy.Throughput, invocationCount: 3000)]
    public class Bench
    {
        private readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        [Params(4,10,16)]
        public int confidence;

        [Params(128,256,512,1024,2048,4096,8192)]
        public int bits;

        [Params(1,2,5,10,20,30,40,50,75,100,125,150,200,250,300,350,400,500,600,700,800,900,1000)]
        public ulong primes;

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
                    numbers[i] = BigInteger.One.GenRandomBits(bits, rng);
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
            BigIntegerExt.PrimesBelow2000 = BigIntegerExt.PrimesBelow1M.Where(x => x < primes * 1000).OrderBy(x => x).ToArray();
            BigIntegerExt.PrimesBelow2000_BI = BigIntegerExt.PrimesBelow2000.Select(p => (BigInteger) p).ToArray();

            if (BigIntegerExt.PrimesBelow2000[^1] >= primes*1000) throw new Exception("selection");
            if (BigIntegerExt.PrimesBelow2000.Length != BigIntegerExt.PrimesBelow2000_BI.Length) throw new Exception("lengths");
            for (var i = 0; i < BigIntegerExt.PrimesBelow2000.Length; i++)
                if (BigIntegerExt.PrimesBelow2000[i] != BigIntegerExt.PrimesBelow2000_BI[i]) throw new Exception("values");

            index = 0;
        }

        [Benchmark]
        public bool bench() => numbers[index++].IsProbablePrime(confidence, rng);
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<Bench>();
        }
    }
}
