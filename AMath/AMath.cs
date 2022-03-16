using System;
using System.Collections.Generic;
using System.Numerics;

namespace AMath
{
    public class Prime
    {
        int index = -1;
        private List<BigInteger> primes = new() { 1, 2, 3, 5, 7 };
        public BigInteger this[int index] => primes[index];

        public BigInteger Next()
        {
            if (primes.Count > ++index) return primes[index];

            BigInteger next = primes[^1] + 2;

            while (!IsPrime(next)) next += 2;

            primes.Add(next);
            return next;
        }

        public bool IsPrime(BigInteger value)
        {
            BigInteger limit = value;
            for (int i = 1; i < primes.Count; i++)
            {
                if (primes[i] > limit) return true;
                limit = BigInteger.DivRem(value, primes[i], out var rem) + 1;
                if (rem == 0) return false;
            }
            return false;
        }
    }

    public static class AMath
    {
        /// <summary> BigInteger does not have implemented Sqrt function, so it is implemented here. </summary>
        /// <returns> Square root of <paramref name="value"/>. </returns>
        public static BigInteger Sqrt(BigInteger value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            if (value < 2) return value;
            BigInteger result, next = value >> (int)(value.GetBitLength() >> 1);

            do
            {
                result = next;
                next = (next + value / next) >> 1;
            } while (result != next);

            return result;
        }
    }
}
