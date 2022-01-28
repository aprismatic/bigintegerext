using System;
using System.Numerics;
using System.Security.Cryptography;
using Aprismatic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;

namespace MyBenchmark
{
    [SimpleJob(runStrategy: RunStrategy.Throughput, invocationCount: 10000)]
    public class Bench
    {
        private readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        [Params(4,8,12,16)]
        public int confidence;

        [Params(256,512,1024,2048,4096)]
        public int bits;

        private BigInteger[] numbers;
        private const int amount = 10000;
        private int index;

        [GlobalSetup]
        public void Setup()
        {
            numbers = new BigInteger[amount];
            for (var i = 0; i < amount; i++)
            {
                numbers[i] = BigInteger.One.GenRandomBits(bits, rng);
                numbers[i] |= BigInteger.One;
            }
        }

        [IterationSetup]
        public void IterationSetup()
        {
            index = 0;
        }

        [Benchmark]
        public bool b2k() => numbers[index++].IsProbablePrime(confidence, rng);

        [Benchmark]
        public bool b50k() => numbers[index++].IsProbablePrime50k(confidence, rng);

        [Benchmark]
        public bool b100k() => numbers[index++].IsProbablePrime100k(confidence, rng);

        [Benchmark]
        public bool b200k() => numbers[index++].IsProbablePrime200k(confidence, rng);
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<Bench>();
        }
    }
}
