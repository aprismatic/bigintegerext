# BigIntegerExt

![Test .NET (Windows)](https://github.com/aprismatic/bigintegerext/workflows/Test%20.NET%20(Windows)/badge.svg?branch=master)

A set of extension methods for the .NET `System.Numerics.BigInteger` class, including methods like `BitCount`, `ModInverse`, `GenPseudoPrime`, `GenSafePseudoPrime`, `GenRandomBits` (using cryptographically strong RNG), `IsProbablePrime` (implemented using Rabin-Miller test).

### Installation

`Aprismatic.BigIntegerExt` library is available in GitHub package repo as well as on NuGet.org: <https://www.nuget.org/packages/Aprismatic.BigIntegerExt/>.

You can install it through NuGet package manager in Visual Studion or Rider or run `dotnet add package Aprismatic.BigIntegerExt`.

### Usage

```csharp
using Aprismatic;
using System.Security.Cryptography; // for RandomNumberGenerator

// ...

var bi = new BigInteger(123);
var bi2 = new BigInteger(345);
var rng = RandomNumberGenerator.Create();

var bitCount = bi.BitCount(); // returns the position (int) of the most significant bit
                              // of the BigInteger's absolute value.

var r1 = bi.ModInverse(bi2); // returns BigInteger x such that (bi * x) % bi2 == 1
var r2 = bi.IsProbablePrime(8, rng); // returns true if bi is a probable prime, false otherwise
                                     // confidence parameter 8 is the number of iterations to
                                     // use in the Rabin-Miller (RM) test

// C# currently doesn't support static extension methods, so we need to use the instance
// method instead. We can use a pre-defined instance of the BigInteger class such as
// `BigInteger.One` or any other instance that is already defined, in our case - `bi` or `bi2`

var r3 = bi.GenPseudoPrime(128, 8, rng); // returns a BigInteger probable prime of 128 bits
                                         // tested with 8 loops of RM test; doesn't change `bi`

var r4 = bi.GenSafePseudoPrime(128, 8, rng); // same but returns a safe prime (i.e., a prime x
                                             // such that (x-1)/2 is also prime)

var r5 = bi.GenRandomBits(128, rng); // returns a BigInteger x with exaxctly 128 random bits,
                                     // i.e., such that x.BitCount() == 128
var r6 = bi.GenRandomBits(bi, bi2, rng); // return a random BigInteger x such that bi ≤ x < bi2
```

### Licensing & Contributions

This library is provided free of charge under the MIT license. See the LICENSE file for more details. Contributions are welcome.
