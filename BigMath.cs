using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AMath
{
    public static partial class BigMath
    {
        public static int Precision = 32;
        public static int SubPrecision = 16;

        public static BigDecimal Epsilon => new BigDecimal(1, Precision + (SubPrecision >> 1));
        public static BigDecimal PI => GetPI();
        public static BigDecimal E => Exp(1);

        private static readonly List<BigInteger> primes = new List<BigInteger>() { 2, 3, 5, 7 };
        public static BigInteger[] GetPrimes(BigInteger limit)
        {
            BigInteger lastPrime = primes[primes.Count - 1];

            for (BigInteger i = lastPrime + 10 - lastPrime % 10; i < limit; i += 10)
            {
                if (IsPrime(i + 1)) primes.Add(i + 1);
                if (IsPrime(i + 3)) primes.Add(i + 3);
                if (IsPrime(i + 7)) primes.Add(i + 7);
                if (IsPrime(i + 9)) primes.Add(i + 9);
            }

            return primes.TakeWhile(prime => prime < limit).ToArray();
        }

        public static bool IsPrime(BigInteger value)
        {
            BigInteger max = value;
            foreach (BigInteger prime in primes)
                if (prime > max) return true;
                else if (value % prime == 0) return false;
                else max = value / prime + 1;
            return true;
        }

        public static BigInteger Factorial(BigInteger value)
        {
            BigInteger result = value;
            while (--value > 1) result *= value;
            return result;
        }

        private static BigDecimal GetPI()
        {
            BigDecimal a, b, t, p;

            BigDecimal pa = 1m,
                       pb = 1m / Sqrt(2m),
                       pt = 0.25m,
                       pp = 1m;

            BigDecimal pi = 3, prev = 0;

            while (Abs(pi - prev) > Epsilon)
            {
                a = (pa + pb) * 0.5m;
                b = Sqrt(pa * pb);
                t = pt - pp * Pow(pa - a, 2);
                p = pp * 2;

                prev = pi;
                pi = Pow(a + b, 2) * 0.25m / t;

                (pa, pb, pt, pp) = (a, b, t, p);
            }

            return pi;
        }

        public static BigDecimal Sin(BigDecimal radians)
        {
            while (radians < -PI * 0.5) radians += PI;
            while (radians > PI * 0.5) radians -= PI;

            BigDecimal sin = radians, prev = 0;

            int i = 0;
            while (Abs(sin - prev) > Epsilon)
            {
                prev = sin;
                sin += ((i & 1) == 0 ? -1 : 1) * Pow(radians, 2 * ++i + 1) / Factorial(2 * i + 1);
            }

            return sin;
        }

        public static BigDecimal Cos(BigDecimal radians) =>
            Sin(PI * 0.5m - radians);

        public static BigDecimal Tg(BigDecimal radians) =>
            Sin(radians) / Cos(radians);

        public static BigDecimal Ctg(BigDecimal radians) =>
            Cos(radians) / Sin(radians);

        public static BigDecimal ArcSin(BigDecimal value)
        {
            if (Abs(value) > 1) return null;

            BigDecimal arcsin = value, prev = 0;

            int i = 0;
            while (Abs(arcsin - prev) > Epsilon)
            {
                prev = arcsin;
                arcsin += Factorial(2 * ++i) * Pow(value, 2 * i + 1) /
                    (((BigInteger)1 << 2 * i) * Pow(Factorial(i), 2) * (2 * i + 1));
            }

            return arcsin;
        }

        public static BigDecimal ArcCos(BigDecimal value) =>
            PI * 0.5m - ArcSin(value);

        public static BigDecimal Log(BigDecimal Value) =>
            Log(Value, 2);

        public static BigDecimal Lg(BigDecimal Value) =>
            Value == 0 ? null : Ln(Value) / Ln(10);

        public static int Log10(BigInteger value)
        {
            int result = 0;
			while (BigInteger.Abs(value) > 0)
			{
				result++;
			 	value /= 10;
			}
			return result;
        }

        public static BigDecimal Log(BigDecimal Value, BigDecimal Base) =>
            Value == 0 ? null : Ln(Value) / Ln(Base);

        public static BigDecimal Ln(BigDecimal value)
        {
			int log10 = Log10((BigInteger)value);
            if (log10 != 0)
				return log10 * LogE(10) + LogE(value / BigInteger.Pow(10, log10));
			return LogE(value);
        }

		private static BigDecimal LogE(BigDecimal value)
		{
			if (value <= 0) return null;
            BigDecimal z = (value - 1) / (value + 1);
            BigDecimal result = z, prev = 0;

            int i = 0;
            while (Abs(result - prev) > Epsilon)
            {
                prev = result;
                result += Pow(z, 2 * ++i + 1) / (2 * i + 1);
            }

            return 2 * result;
		}

        public static BigDecimal Exp(BigDecimal value)
        {
            BigDecimal result = 1, prev = 0;
            int i = 1, pow = 1;
            
            while (value > 2)
            {
                value *= 0.5m;
                pow <<= 1;
            }

            while (Abs(result - prev) > Epsilon)
            {
                prev = result;
                result += Pow(value, i) / Factorial(i++);
            }

            return Pow(result, pow);
        }

        public static BigDecimal Pow(BigDecimal value, BigDecimal power) =>
            power == 0 ? 1 :
            (Abs(power) > 1 ? Truncate(power, Precision).mantissa.IsEven
                            : Round(1 / power).mantissa.IsEven)
            ? (value.Sign > 0 ? Exp(power * Ln(value)) : null)
            : value.Sign * Exp(power * Ln(Abs(value)));

		
        private static BigDecimal Pow(BigDecimal value, BigInteger power)
        {
            BigDecimal result = 1;
            bool negative = power < 0;
            power = BigInteger.Abs(power);

            while (power > 0)
            {
                if ((power & 1) == 1)
                    result *= value;

                power >>= 1;
                value *= value;
            }

            return negative ? 1 / result : result;
        }

        public static BigDecimal Sqrt(BigDecimal value) =>
            new BigDecimal()
            {
                mantissa = Sqrt(value.mantissa * BigInteger.Pow(10, Precision * 2 + 2 * SubPrecision + (value.exponenta & 1))),
                exponenta = (value.exponenta >> 1) + Precision + SubPrecision + (value.exponenta & 1)
            }.Simplify();

        public static BigInteger Sqrt(BigInteger value)
        {
            if (value <= 4503599761588223UL)
            {
                if (value.Sign < 0) throw new ArgumentException("Negative argument.");
                return (ulong)Math.Sqrt((ulong)value);
            }

            BigInteger root;
            int byteLen = value.ToByteArray().Length;
            if (byteLen < 128) root = (BigInteger)Math.Sqrt((double)value);
            else root = (BigInteger)Math.Sqrt((double)(value >> (byteLen - 127) * 8)) << (byteLen - 127) * 4;

            while (true)
            {
                var root2 = value / root + root >> 1;
                if ((root2 == root || root2 == root + 1) && IsSqrt(value, root)) return root;
                root = value / root2 + root2 >> 1;
                if ((root == root2 || root == root2 + 1) && IsSqrt(value, root2)) return root2;
            }
        }

        private static bool IsSqrt(BigInteger value, BigInteger root)
        {
            var lowerBound = root * root;

            return value >= lowerBound && value <= lowerBound + (root << 1);
        }

        public static BigDecimal Abs(BigDecimal value) =>
            value.Sign * value;

        public static BigDecimal Round(BigDecimal value) =>
            Round(value, 0);

        public static BigDecimal Round(BigDecimal value, int precision) =>
            value.Sign * Truncate(Truncate(Abs(value), precision + 1) + new BigDecimal(5, precision + 1), precision);

        public static BigDecimal Truncate(BigDecimal value) =>
            Truncate(value, 0);

        public static BigDecimal Truncate(BigDecimal value, int precision) =>
            precision >= value.exponenta ? value : new BigDecimal()
            {
                mantissa = value.mantissa / BigInteger.Pow(10, value.exponenta - precision),
                exponenta = precision
            };
    }
}