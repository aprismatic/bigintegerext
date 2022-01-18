/************************************************************************************
 This library is an extension for the .NET implementation of BigInteger. It provides
 some of the missing functionality.

 This library is provided as-is and is covered by the MIT License [1].

 [1] The MIT License (MIT), website, (http://opensource.org/licenses/MIT)
 ************************************************************************************/

using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace Aprismatic
{
    public static class BigIntegerExt
    {
        // primes smaller than 2000 to test the generated prime number
        public static readonly ulong[] PrimesBelow2000 = {
           2,    3,    5,    7,   11,   13,   17,   19,   23,   29,   31,   37,   41,   43,   47,   53,   59,   61,   67,   71,
          73,   79,   83,   89,   97,  101,  103,  107,  109,  113,  127,  131,  137,  139,  149,  151,  157,  163,  167,  173,
         179,  181,  191,  193,  197,  199,  211,  223,  227,  229,  233,  239,  241,  251,  257,  263,  269,  271,  277,  281,
         283,  293,  307,  311,  313,  317,  331,  337,  347,  349,  353,  359,  367,  373,  379,  383,  389,  397,  401,  409,
         419,  421,  431,  433,  439,  443,  449,  457,  461,  463,  467,  479,  487,  491,  499,  503,  509,  521,  523,  541,
         547,  557,  563,  569,  571,  577,  587,  593,  599,  601,  607,  613,  617,  619,  631,  641,  643,  647,  653,  659,
         661,  673,  677,  683,  691,  701,  709,  719,  727,  733,  739,  743,  751,  757,  761,  769,  773,  787,  797,  809,
         811,  821,  823,  827,  829,  839,  853,  857,  859,  863,  877,  881,  883,  887,  907,  911,  919,  929,  937,  941,
         947,  953,  967,  971,  977,  983,  991,  997, 1009, 1013, 1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063, 1069,
        1087, 1091, 1093, 1097, 1103, 1109, 1117, 1123, 1129, 1151, 1153, 1163, 1171, 1181, 1187, 1193, 1201, 1213, 1217, 1223,
        1229, 1231, 1237, 1249, 1259, 1277, 1279, 1283, 1289, 1291, 1297, 1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373,
        1381, 1399, 1409, 1423, 1427, 1429, 1433, 1439, 1447, 1451, 1453, 1459, 1471, 1481, 1483, 1487, 1489, 1493, 1499, 1511,
        1523, 1531, 1543, 1549, 1553, 1559, 1567, 1571, 1579, 1583, 1597, 1601, 1607, 1609, 1613, 1619, 1621, 1627, 1637, 1657,
        1663, 1667, 1669, 1693, 1697, 1699, 1709, 1721, 1723, 1733, 1741, 1747, 1753, 1759, 1777, 1783, 1787, 1789, 1801, 1811,
        1823, 1831, 1847, 1861, 1867, 1871, 1873, 1877, 1879, 1889, 1901, 1907, 1913, 1931, 1933, 1949, 1951, 1973, 1979, 1987,
        1993, 1997, 1999 };

        public static readonly BigInteger[] PrimesBelow2000_BI = PrimesBelow2000.Select(p => (BigInteger)p).ToArray();


        /// <summary>
        /// Calculates the modulo inverse of `this`.
        /// </summary>
        /// <param name="mod">Modulo</param>
        /// <returns>Modulo inverse of `this`; or 1 if mod.inv. does not exist.</returns>
        public static BigInteger ModInverse(this BigInteger T, BigInteger mod)
        {
            BigInteger i = mod, v = BigInteger.Zero, d = BigInteger.One, t, x;

            while (T.Sign > 0)
            {
                x = T;
                t = BigInteger.DivRem(i, T, out T);
                i = x;
                x = d;
                d = v - t * x;
                v = x;
            }

            v %= mod; // TODO: this could be faster (?)
            if (v.Sign < 0)
                v = (v + mod) % mod; // TODO: this too (?)

            return v;
        }


        /// <summary>
        /// Returns the position of the most significant bit of the BigInteger's absolute value.
        /// </summary>
        /// <example>
        /// 1) The result is 1, if the value of BigInteger is 0...0000 0000
        /// 2) The result is 1, if the value of BigInteger is 0...0000 0001
        /// 3) The result is 2, if the value of BigInteger is 0...0000 0010
        /// 4) The result is 2, if the value of BigInteger is 0...0000 0011
        /// 5) The result is 5, if the value of BigInteger is 0...0001 0011
        /// </example>
        public static int BitCount(this BigInteger T)
        {
            var data = T.Sign >= 0 ? T.ToByteArray() : (-T).ToByteArray();
            var value = data[data.Length - 1];
            byte mask = 0x80;
            var bits = 8;

            while (bits > 0 && (value & mask) == 0)
            {
                bits--;
                mask >>= 1;
            }

            bits += (data.Length - 1) << 3;

            return bits == 0 ? 1 : bits;
        }


        /// <summary>
        /// Returns a random BigInteger that is within a specified range.
        /// The lower bound is inclusive, and the upper bound is exclusive.
        /// Code is based upon this StackOverflow answer: https://stackoverflow.com/a/68593532/664178
        /// </summary>
        /// <param name="minValue">Inclusive lower bound</param>
        /// <param name="maxValue">Exclusive upper bound</param>
        /// <param name="rng">Random number generator to use</param>
        public static BigInteger GenRandomBits(this BigInteger T, BigInteger minValue, BigInteger maxValue, RandomNumberGenerator rng)
        {
            if (minValue > maxValue) throw new ArgumentException($"{nameof(minValue)} must be less or equal to {nameof(maxValue)}");
            if (minValue == maxValue) return minValue;
            var zeroBasedUpperBound = maxValue - BigInteger.One - minValue; // Inclusive
            Debug.Assert(zeroBasedUpperBound.Sign >= 0);
            var bytes = zeroBasedUpperBound.ToByteArray();
            Debug.Assert(bytes.Length > 0);
            Debug.Assert((bytes[bytes.Length - 1] & 0b10000000) == 0);

            // Search for the most significant non-zero bit
            byte lastByteMask = 0b11111111;
            for (byte mask = 0b10000000; mask > 0; mask >>= 1, lastByteMask >>= 1)
            {
                if ((bytes[bytes.Length - 1] & mask) == mask) break; // We found it
            }

            while (true)
            {
                rng.GetBytes(bytes);
                bytes[bytes.Length - 1] &= lastByteMask;
                var result = new BigInteger(bytes);
                Debug.Assert(result.Sign >= 0);
                if (result <= zeroBasedUpperBound) return result + minValue;
            }
        }


        /// <summary>
        /// Returns a random BigInteger of exactly the specified bit length using the provided RNG
        /// </summary>
        /// <param name="bits">Bit length of random BigInteger to generate</param>
        /// <param name="rng">RandomNumberGenerator object</param>
        /// <exception cref="ArgumentOutOfRangeException">`bits` must be > 0</exception>
        public static BigInteger GenRandomBits(this BigInteger T, int bits, RandomNumberGenerator rng)
        {
            if (bits <= 0) throw new ArgumentOutOfRangeException(nameof(bits), bits, "Number of required bits must be greater than zero.");

            bits++; // add one for the sign bit

            var bytes = bits >> 3;
            var remBits = bits - (bytes << 3);

            if (remBits != 0)
                bytes++;

            var data = new byte[bytes];
            rng.GetBytes(data);

            if (remBits == 1)
            {
                // we know that value of `bytes` can't be `0` here because of `bits++`
                data[bytes - 1] = 0; // added byte set to 0 for positive sign
                data[bytes - 2] |= 0x80; // MSB set to 1
            }
            else if (remBits > 1)
            {
                var mask = (byte)(0x01 << (remBits - 2));
                data[bytes - 1] |= mask;

                mask = (byte)(0xFF >> (8 - remBits + 1));
                data[bytes - 1] &= mask; // set the sign bit to 0
            }
            else // remBits == 0
            {
                data[bytes - 1] |= 0x40; // MSB to 1
                data[bytes - 1] &= 0x7F; // Sign to 0
            }

            return new BigInteger(data);
        }


        /// <summary>
        /// Generates a random probable prime positive BigInteger of exactly the specified bit length using the provided RNG
        /// </summary>
        /// <param name="bits">Bit length of prime to generate; has to be greater than 1</param>
        /// <param name="confidence">Number of chosen bases</param>
        /// <param name="rng">RandomNumberGenerator object</param>
        /// <returns>A probably prime number</returns>
        /// <exception cref="ArgumentOutOfRangeException">`bits` must be >= 2</exception>
        public static BigInteger GenPseudoPrime(this BigInteger T, int bits, int confidence, RandomNumberGenerator rng)
        {
            if (bits < 2) throw new ArgumentOutOfRangeException(nameof(bits), bits, "GenPseudoPrime can only generate prime numbers of 2 bits or more");

            BigInteger result;

            do
            {
                result = BigInteger.Zero.GenRandomBits(bits, rng);
                result |= BigInteger.One; // make it odd
            } while (!result.IsProbablePrime(confidence, rng));

            return result;
        }


        /// <summary>
        /// Generates a random probable safe prime positive BigInteger of exactly the specified bit length using the provided RNG.
        /// Safe prime is a prime P such that P=2*Q+1, where Q is prime. Such Q is called a Sophie Germain prime.
        /// This method uses the Combined Sieve approach to improve performance as compared to naive algorithm.
        /// See Michael Wiener ``Safe Prime Generation with a Combined Sieve'', 2003 (https://eprint.iacr.org/2003/186)
        /// </summary>
        /// <param name="bits">Bit length of prime to generate</param>
        /// <param name="confidence">Number of chosen bases</param>
        /// <param name="rng">RandomNumberGenerator object</param>
        /// <returns>A probably prime number</returns>
        /// <exception cref="ArgumentOutOfRangeException">`bits` must be >= 3</exception>
        public static BigInteger GenSafePseudoPrime(this BigInteger T, int bits, int confidence, RandomNumberGenerator rng)
        {
            if (bits < 3) throw new ArgumentOutOfRangeException(nameof(bits), bits, "GenSafePseudoPrime can only generate prime numbers of 3 bits or more");

            BigInteger result;
            var two = new BigInteger(2);

            do
            {
                BigInteger q;
                var qbits = bits - 1;

                var done = false;

                while (!done)
                {
                    q = BigInteger.Zero.GenRandomBits(qbits, rng);
                    q |= BigInteger.One; // make it odd

                    var fail = false;

                    if (q <= UInt64.MaxValue)
                    {
                        var uival = (UInt64)q;

                        foreach (var curSmallPrime in PrimesBelow2000)
                        {
                            if (curSmallPrime >= uival)
                            {
                                fail = true; // not a fail but we skip Rabin-Miller test using this flag
                                done = true; // and exit the outer while loop
                                break;
                            }

                            var rem = uival % curSmallPrime;

                            // Sieve: if rem=0 then Q is composite;
                            //        if second condition is true, then P will be divisible by curSmallPrime
                            if (rem == 0 || rem == (curSmallPrime - 1) / 2)
                            {
                                fail = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var curSmallPrime in PrimesBelow2000_BI)
                        {
                            var rem = q % curSmallPrime;

                            // Sieve: if rem=0 then Q is composite;
                            //        if second condition is true, then P will be divisible by curSmallPrime
                            if (rem.IsZero || rem == (curSmallPrime - BigInteger.One) / 2)
                            {
                                fail = true;
                                break;
                            }
                        }
                    }

                    if (fail) continue; // try another Q

                    done = q.RabinMillerTest(confidence, rng); // returns true if Q is prime
                }

                result = two * q + BigInteger.One;
            } while (!result.RabinMillerTest(confidence, rng)); // no need to check divisibility by small primes, can go straight to Rabin-Miller

            return result;
        }


        /// <summary>
        /// Determines whether a number is probably prime using the Rabin-Miller's test
        /// </summary>
        /// <remarks>
        /// Before applying the test, the number is tested for divisibility by primes &lt; 2000
        /// </remarks>
        /// <param name="confidence">Number of chosen bases</param>
        /// <param name="rng">RandomNumberGenerator object</param>
        /// <returns>True if this is probably prime</returns>
        public static bool IsProbablePrime(this BigInteger T, int confidence, RandomNumberGenerator rng)
        {
            var thisVal = BigInteger.Abs(T);
            if (thisVal.IsZero || thisVal.IsOne) return false;

            if (thisVal <= UInt64.MaxValue)
            {
                var uival = (UInt64)thisVal;

                foreach (var smallPrime in PrimesBelow2000)
                {
                    if (smallPrime >= uival)
                        return true;

                    if (uival % smallPrime == 0)
                        return false;
                }
            }
            else
            {
                foreach (var smallPrime in PrimesBelow2000_BI)
                {
                    if ((thisVal % smallPrime).IsZero)
                        return false;
                }
            }

            return thisVal.RabinMillerTest(confidence, rng);
        }


        /// <summary>
        /// Probabilistic prime test based on Miller-Rabin's algorithm.
        /// Algorithm based on http://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.186-4.pdf (p. 72)
        /// This method REQUIRES that the BigInteger is positive
        /// </summary>
        /// <remarks>
        /// for any p &gt; 0 with p - 1 = 2^s * t
        ///
        /// p is probably prime (strong pseudoprime) if for any a &lt; p,
        /// 1) a^t mod p = 1 or
        /// 2) a^((2^j)*t) mod p = p-1 for some 0 &lt;= j &lt;= s-1
        ///
        /// Otherwise, p is composite.
        /// </remarks>
        /// <param name="confidence">Number of chosen bases</param>
        /// <returns>True if this is a strong pseudoprime to randomly chosen bases</returns>
        public static bool RabinMillerTest(this BigInteger w, int confidence, RandomNumberGenerator rng)
        {
            var wMinusOne = w - BigInteger.One;
            var m = wMinusOne;
            var a = 0;

            while (m.IsEven)
            {
                m >>= 1;
                a++;
            }

            var wlen = w.BitCount();
            BigInteger b;

            for (var i = 0; i < confidence; i++)
            {
                do
                {
                    b = BigInteger.Zero.GenRandomBits(wlen, rng);
                } while (b >= wMinusOne || b < 2);

                var z = BigInteger.ModPow(b, m, w);
                if (z.IsOne || z == wMinusOne)
                    continue;

                for (var j = 1; j < a; j++)
                {
                    z = BigInteger.ModPow(z, 2, w);
                    if (z.IsOne)
                        return false;
                    if (z == wMinusOne)
                        break;
                }

                if (z != wMinusOne)
                    return false;
            }

            return true;
        }
    }
}
