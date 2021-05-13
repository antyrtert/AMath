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
                if (prime > (max = BigInteger.DivRem(value, prime, out BigInteger rem)) + 1)
                    return true;
                else if (rem == 0)
                    return false;
            return true;
        }

        public static BigInteger Factorial(BigInteger n)
        {
            BigInteger sum = n, result = n;
            for (BigInteger i = n - 2; i > 1; i -= 2)
                result *= (sum += i);

            if ((n & 1) == 1) result *= (n >> 1) + 1;
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

        public static BigDecimal DegreesToRadians(BigDecimal degrees)
        {
            while (degrees < -180) degrees += 180;
            while (degrees > 180) degrees -= 180;

            return PI * degrees / 180;
        }

        public static BigDecimal RadiansToDegrees(BigDecimal radians)
        {
            BigDecimal pi = PI * 0.5m;
            while (radians < -pi) radians += pi;
            while (radians > pi) radians -= pi;

            return 180 / (radians * PI);
        }

        public static BigDecimal Sin(BigDecimal radians)
        {
            BigDecimal pi = PI * 0.5m;
            while (radians < -pi) radians += pi;
            while (radians > pi) radians -= pi;

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
            radians % (PI * 0.5m) != 0 ? Sin(radians) / Cos(radians) : null;

        public static BigDecimal Ctg(BigDecimal radians) =>
            radians % PI != 0 ? Cos(radians) / Sin(radians) : null;

        public static BigDecimal ArcSin(BigDecimal value)
        {
            if (Abs(value) > 1) return null;
            BigDecimal result = value, prev = 0;

            int i = 0;
            while (Abs(result - prev) > Epsilon)
            {
                prev = result;
                result += Factorial(2 * ++i) * Pow(value, 2 * i + 1) /
                    (((BigInteger)1 << 2 * i) * Pow(Factorial(i), 2) * (2 * i + 1));
            }

            return result;
        }

        public static BigDecimal ArcCos(BigDecimal value) =>
            PI * 0.5m - ArcSin(value);

        public static BigDecimal Log(BigDecimal Value) =>
            Log(Value, 2);

        public static BigDecimal Lg(BigDecimal Value) =>
            Value == 0 ? null : Ln(Value) / Ln(10);

        public static int Log2(BigInteger value)
        {
            value = BigInteger.Abs(value);
            int result = value.ToByteArray().Length * 8 - 8;
            if (result < 0) result = 0;
            value >>= result;
            while (value != 0)
            {
                result++;
                value >>= 1;
            }
            return result;
        }

        public static int Log10(BigInteger value) =>
            (int)BigInteger.Log10(BigInteger.Abs(value));

        public static BigDecimal Log(BigDecimal Value, BigDecimal Base) =>
            Value == 0 ? null : Ln(Value) / Ln(Base);

        public static BigDecimal Ln(BigDecimal value)
        {
			int log2 = Log2((BigInteger)value) - 1;
            if (log2 > 0)
				return log2 * LogE(2) + LogE(value / ((BigInteger)1 << log2));
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

		
        public static BigDecimal Pow(BigDecimal value, BigInteger power)
        {
            BigDecimal result = 1;
            bool negative = power < 0;
            power = BigInteger.Abs(power) << 1;

            while ((power >>= 1) > 0)
            {
                if ((power & 1) == 1)
                    result *= value;
                value *= value;
            }

            return negative ? 1 / result : result;
        }

        public static BigDecimal Sqrt(BigDecimal value) =>
            value < 0 ? null : new BigDecimal(
                Sqrt(value.mantissa * BigInteger.Pow(10, Precision * 2 + 2 * SubPrecision + (value.exponenta & 1))),
                (value.exponenta >> 1) + Precision + SubPrecision + (value.exponenta & 1)
            ).Simplify();

        public static BigInteger Sqrt(BigInteger value)
        {
            if (value.Sign < 0) return -1;
            if (value <= 4503599761588223UL)
                return (ulong)Math.Sqrt((ulong)value);

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

        private static bool IsSqrt(BigInteger value, BigInteger root) =>
            value >= root * root && value <= root * root + (root << 1);

        public static BigDecimal Abs(BigDecimal value) =>
            value.Sign * value;

        public static BigDecimal Round(BigDecimal value) =>
            Round(value, 0);

        public static BigDecimal Round(BigDecimal value, int precision) =>
            value.Sign * Truncate(Truncate(Abs(value), precision + 1) + new BigDecimal(5, precision + 1), precision);

        public static BigDecimal Ceil(BigDecimal value) =>
            Ceil(value, 0);

        public static BigDecimal Ceil(BigDecimal value, int precision) =>
            Truncate(Truncate(value, precision + 1) + new BigDecimal(5, precision + 1), precision);

        public static BigDecimal Floor(BigDecimal value) =>
            Floor(value, 0);

        public static BigDecimal Floor(BigDecimal value, int precision) =>
            Truncate(Truncate(value, precision + 1) - new BigDecimal(5, precision + 1), precision);

        public static BigDecimal Truncate(BigDecimal value) =>
            Truncate(value, 0);

        public static BigDecimal Truncate(BigDecimal value, int precision) =>
            precision >= value.exponenta ? value : new BigDecimal(
                value.mantissa / BigInteger.Pow(10, value.exponenta - precision), precision);
    }
}