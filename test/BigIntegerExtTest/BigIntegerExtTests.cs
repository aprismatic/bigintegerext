using Aprismatic;
using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Xunit;

namespace BigIntegerExtTest
{
    public class BigIntegerExtTests
    {
        [Fact(DisplayName = "BitCount")]
        public void TestBitCount()
        {
            Assert.Equal(1, BigInteger.Zero.BitCount());
            Assert.Equal(1, BigInteger.One.BitCount());
            Assert.Equal(1, BigInteger.MinusOne.BitCount());

            var b1 = new BigInteger(Int32.MaxValue);
            var b2 = new BigInteger(Int32.MinValue);
            Assert.Equal(31, b1.BitCount());
            Assert.Equal(32, b2.BitCount());
            Assert.Equal(32, (b1 + 1).BitCount());

            var b3 = new BigInteger(UInt32.MaxValue);
            Assert.Equal(32, b3.BitCount());

            var b4 = new BigInteger(7);
            var b5 = new BigInteger(8);
            Assert.Equal(3, b4.BitCount());
            Assert.Equal(4, b5.BitCount());
        }

        [Fact(DisplayName = "ModInverse")]
        public void TestModInverse()
        {
            {
                var a = new BigInteger[] { 0, 0, 1, 3, 7, 25, 2, 13, 19, 31, 3 };
                var m = new BigInteger[]
                    { 1000000007, 1999, 2, 6, 87, 87, 91, 91, 1212393831, 73714876143, 73714876143 };
                var r = new BigInteger[] { 736445995, 1814, 1, 1, 25, 7, 46, 1, 701912218, 45180085378, 1 };

                var t = new BigInteger();
                BigInteger.TryParse(
                    "470782681346529800216759025446747092045188631141622615445464429840250748896490263346676188477401449398784352124574498378830506322639352584202116605974693692194824763263949618703029846313252400361025245824301828641617858127932941468016666971398736792667282916657805322080902778987073711188483372360907612588995664533157503380846449774089269965646418521613225981431666593065726252482995754339317299670566915780168",
                    out t);
                a[0] = t;
                a[1] = t;

                Assert.Equal(a.Length, m.Length);
                Assert.Equal(m.Length, r.Length);

                for (var i = 0; i < a.Length; i++)
                {
                    Assert.Equal(r[i], a[i].ModInverse(m[i]));
                }
            }

            var rng = RandomNumberGenerator.Create();
            var rnd = new Random();

            for (var i = 0; i < 9999; i++)
            {
                var bi = new BigInteger();
                var mod = new BigInteger();
                var j = 9999;
                while (BigInteger.GreatestCommonDivisor(bi, mod) != 1 || mod <= 1)
                {
                    if (++j > 1000)
                    {
                        bi = bi.GenRandomBits(rnd.Next(1, 1024), rng);
                        j = 0;
                    }

                    mod = mod.GenRandomBits(rnd.Next(1, 128), rng);
                }

                var inv = bi.ModInverse(mod);

                Assert.True((bi != 0 ? 1 : 0) == bi * inv % mod,
                    $"{Environment.NewLine}bi:  {bi}{Environment.NewLine}mod: {mod}{Environment.NewLine}inv: {inv}");
            }
        }

        [Fact(DisplayName = "IsProbablePrime")]
        public void TestIsProbablePrime()
        {
            var rng = RandomNumberGenerator.Create();

            Assert.False(BigInteger.Zero.IsProbablePrime(10, rng));
            Assert.False(BigInteger.One.IsProbablePrime(10, rng));

            for (var i = 2UL; i < 2000; i++) // since we have an array of primes below 2000 that we can check against
            {
                var res = new BigInteger(i).IsProbablePrime(10, rng);
                Assert.True(BigIntegerExt.PrimesBelow2000.Contains(i) == res,
                    $"{i} is prime is {BigIntegerExt.PrimesBelow2000.Contains(i)} but was evaluated as {res}");
            }

            foreach (var p in new[]
                { 633910111, 838041647, 15485863, 452930477, 28122569887267, 29996224275833, 571245373823500631 })
            {
                Assert.True(new BigInteger(p).IsProbablePrime(10, rng));
            }

            foreach (var p in new[] { 398012025725459, 60030484763, 571245373823500630, 239812014798221 })
            {
                Assert.False(new BigInteger(p).IsProbablePrime(50, rng));
            }
        }

        [Fact(DisplayName = "GenPseudoPrime")]
        public void TestGenPseudoPrime()
        {
            var bi = new BigInteger();
            var rng = RandomNumberGenerator.Create();
            var rand = new Random();

            // Test throw
            Assert.Throws<ArgumentOutOfRangeException>(() => BigInteger.Zero.GenSafePseudoPrime(-1, 4, rng));
            Assert.Throws<ArgumentOutOfRangeException>(() => BigInteger.Zero.GenSafePseudoPrime(0, 4, rng));
            Assert.Throws<ArgumentOutOfRangeException>(() => BigInteger.Zero.GenSafePseudoPrime(1, 4, rng));

            // Test small numbers thoroughly
            for (var i = 0; i < 100; i++)
            {
                var prime = bi.GenPseudoPrime(rand.Next(2, 9), 4, rng);
                Assert.Contains(prime, BigIntegerExt.PrimesBelow2000_BI);
            }

            // Test arbitrary values
            for (var i = 0; i < 200; i++)
            {
                var prime = bi.GenPseudoPrime(rand.Next(2, 768), 4, rng);

                foreach (var pr in BigIntegerExt.PrimesBelow2000)
                {
                    Assert.True(prime == pr || prime % pr != 0,
                                $"prime: {prime}{Environment.NewLine}" +
                                $"pr:    {pr}");
                }

                Assert.True(prime.IsProbablePrime(4, rng));
            }
        }

        [Fact(DisplayName = "GenSafePseudoPrime")]
        public void TestGenSafePseudoPrime()
        {
            var rng = RandomNumberGenerator.Create();
            var rand = new Random();

            // Test throw
            Assert.Throws<ArgumentOutOfRangeException>(() => BigInteger.Zero.GenSafePseudoPrime(-1, 4, rng));
            Assert.Throws<ArgumentOutOfRangeException>(() => BigInteger.Zero.GenSafePseudoPrime(0, 4, rng));
            Assert.Throws<ArgumentOutOfRangeException>(() => BigInteger.Zero.GenSafePseudoPrime(1, 4, rng));
            Assert.Throws<ArgumentOutOfRangeException>(() => BigInteger.Zero.GenSafePseudoPrime(2, 4, rng));

            // Test small numbers thoroughly
            for (var i = 0; i < 100; i++)
            {
                var prime = BigInteger.Zero.GenSafePseudoPrime(rand.Next(3, 9), 4, rng);
                Assert.Contains(prime, BigIntegerExt.PrimesBelow2000_BI);
                Assert.Contains(((prime - 1) / 2), BigIntegerExt.PrimesBelow2000_BI);
            }

            // Test arbitrary values
            for (var i = 0; i < 20; i++)
            {
                var prime = BigInteger.Zero.GenSafePseudoPrime(rand.Next(3, 768), 4, rng);

                foreach (var pr in BigIntegerExt.PrimesBelow2000)
                {
                    Assert.True(prime == pr || prime % pr != 0,
                        $"prime: {prime}{Environment.NewLine}" +
                        $"pr:    {pr}");
                }

                Assert.True(prime.IsProbablePrime(4, rng));
                Assert.True(((prime - 1) / 2).IsProbablePrime(4, rng));
            }
        }

        [Fact(DisplayName = "GenRandomBits")]
        public void TestGenRandomBits()
        {
            var rng = RandomNumberGenerator.Create();
            var rand = new Random();

            // Test with `bits`
            {
                var bi = BigInteger.Zero;

                for (var i = 0; i < 9999; i++) // Test < 32 bits
                {
                    bi = bi.GenRandomBits(rand.Next(1, 33), rng);
                    Assert.True(bi < BigInteger.Pow(2, 32));
                }

                for (var i = 0; i < 9999; i++) // Test on random number of bits
                {
                    var bits = rand.Next(1, 70 * 32 + 1);
                    bi = bi.GenRandomBits(bits, rng);
                    Assert.True(bits == bi.BitCount());
                    Assert.True(bi >= 0);
                }

                for (var i = 0; i < 9999; i++) // Test lower boundary value
                {
                    bi = bi.GenRandomBits(1, rng);
                    Assert.True(bi.ToByteArray()[0] == 1 || bi.ToByteArray()[0] == 0);
                }

                for (var i = 0; i < 9999; i++) // some edge cases
                {
                    var bi255 = bi.GenRandomBits(255, rng);
                    Assert.Equal(255, bi255.BitCount());

                    var bi256 = bi.GenRandomBits(256, rng);
                    Assert.Equal(256, bi256.BitCount());

                    var bi257 = bi.GenRandomBits(257, rng);
                    Assert.Equal(257, bi257.BitCount());
                }
            }

            // Test with upper/lower bounds
            {
                var bi = new BigInteger();

                for (var i = 0; i < 999; i++)
                {
                    var boundary = new BigInteger(rand.Next());

                    bi = bi.GenRandomBits(boundary, boundary, rng);

                    Assert.Equal(boundary, bi);
                }

                for (var i = 0; i < 9999; i++)
                {
                    var low = rand.Next();
                    var top = rand.Next();
                    low *= rand.Next() % 2 == 0 ? 1 : -1;
                    top *= rand.Next() % 2 == 0 ? 1 : -1;
                    if (top == low)
                        low--;
                    if (low > top)
                        (low, top) = (top, low);

                    bi = bi.GenRandomBits(low, top, rng);

                    Assert.True(bi >= low);
                    Assert.True(bi < top);
                }
            }
        }
    }
}
