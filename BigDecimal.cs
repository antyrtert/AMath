using System;
using System.Linq;
using System.Numerics;

namespace AMath
{
    public static partial class BigMath
    {
        public class BigDecimal : IComparable
        {
            public BigInteger mantissa;
            /// <summary>
            /// Power of 10
            /// </summary>
            public int exponent = 0;

            public int Sign => mantissa.Sign;

            public BigDecimal(BigInteger mantissa, int exponent)
            {
                this.mantissa = mantissa;
                this.exponent = exponent;
            }

            public static BigDecimal operator ++(BigDecimal value) =>
                value + 1;

            public static BigDecimal operator --(BigDecimal value) =>
                value - 1;

            public static BigDecimal operator +(BigDecimal left, BigDecimal right) =>
                ReferenceEquals(left, null) || ReferenceEquals(right, null) ? null :
                new BigDecimal(
                    left.exponent > right.exponent 
                        ? left.mantissa + right.mantissa * BigInteger.Pow(10, left.exponent - right.exponent)
                        : left.mantissa * BigInteger.Pow(10, right.exponent - left.exponent) + right.mantissa,
                    left.exponent > right.exponent ? left.exponent : right.exponent
                ).Simplify();

            public static BigDecimal operator -(BigDecimal left, BigDecimal right) =>
                ReferenceEquals(left, null) || ReferenceEquals(right, null) ? null :
                new BigDecimal(
                    left.exponent > right.exponent 
                        ? left.mantissa - right.mantissa * BigInteger.Pow(10, left.exponent - right.exponent)
                        : left.mantissa * BigInteger.Pow(10, right.exponent - left.exponent) - right.mantissa,
                    left.exponent > right.exponent ? left.exponent : right.exponent
                ).Simplify();

            public static BigDecimal operator *(BigDecimal left, BigDecimal right) =>
                ReferenceEquals(left, null) || ReferenceEquals(right, null) ? null :
                new BigDecimal(
                    left.mantissa * right.mantissa,
                    left.exponent + right.exponent
                ).Simplify();

            public static BigDecimal operator /(BigDecimal left, BigDecimal right) =>
                ReferenceEquals(left, null) || ReferenceEquals(right, null) ? null :
                new BigDecimal(
                    left.mantissa * BigInteger.Pow(10, Precision * 2 + SubPrecision * 2) / right.mantissa,
                    left.exponent - right.exponent + Precision * 2 + SubPrecision * 2
                ).Simplify();
                
            public static BigDecimal operator %(BigDecimal left, BigDecimal right) =>
                ReferenceEquals(left, null) || ReferenceEquals(right, null) ? null :
                left - right * Truncate(left / right);

            public static bool operator <(BigDecimal left, BigDecimal right) =>
                ReferenceEquals(left, null) || ReferenceEquals(right, null) ? false :
                (left - right).Sign < 0;

            public static bool operator <=(BigDecimal left, BigDecimal right) =>
                ReferenceEquals(left, null) || ReferenceEquals(right, null) ? false :
                (left - right).Sign <= 0 || left == right;

            public static bool operator >(BigDecimal left, BigDecimal right) =>
                ReferenceEquals(left, null) || ReferenceEquals(right, null) ? false :
                (left - right).Sign > 0;

            public static bool operator >=(BigDecimal left, BigDecimal right) =>
                ReferenceEquals(left, null) || ReferenceEquals(right, null) ? false :
                (left - right).Sign >= 0 || left == right;

            public static bool operator ==(BigDecimal left, BigDecimal right) =>
                ReferenceEquals(left, null) || ReferenceEquals(right, null) ? false :
                !(Abs(left - right) > Epsilon);

            public static bool operator !=(BigDecimal left, BigDecimal right) =>
                ReferenceEquals(left, null) || ReferenceEquals(right, null) ? true :
                Abs(left - right) > Epsilon;

            public static BigDecimal operator -(BigDecimal value) =>
                ReferenceEquals(value, null) ? null :
                -1 * value;
            
            public static explicit operator BigInteger(BigDecimal value) =>
                ReferenceEquals(value, null) ? new BigInteger() :
                value.exponent > 0
                ? value.mantissa / BigInteger.Pow(10, value.exponent)
                : value.mantissa * BigInteger.Pow(10, -value.exponent);

            public static implicit operator BigDecimal(int value) =>
                new BigDecimal(value, 0).Simplify();

            public static implicit operator BigDecimal(BigInteger value) =>
                new BigDecimal(value, 0).Simplify();

            public static implicit operator BigDecimal(double value)
            {
                if (double.IsInfinity(value) || double.IsNaN(value)) return null;
                long bits = BitConverter.DoubleToInt64Bits(value);

                bool negative = (bits & (1L << 63)) != 0;
                int exponent = (int)((bits >> 52) & 0x7ffL);
                BigInteger mantissa = bits & 0xfffffffffffffL;

                if (mantissa == 0) return negative ? -0 : 0;
                if (exponent == 0) exponent++;
                else mantissa |= 1L << 52;
                exponent -= 1075;

                while ((mantissa & 1) == 0)
                {
                    mantissa >>= 1;
                    exponent++;
                }

                if (exponent > 0)
                {
                    mantissa <<= exponent;
                    exponent = 0;
                }

                return new BigDecimal(negative ? -mantissa : mantissa, 0) / ((BigInteger)1 << -exponent);
            }

            public static implicit operator BigDecimal(decimal value)
            {
                int[] bits = decimal.GetBits(value);
                BigInteger mantissa = (bits[2] << 64) + (bits[1] << 32) + bits[0];
                if ((bits[3] & 0x80000000) == 0x80000000) mantissa = -mantissa;
                return new BigDecimal(mantissa, bits[3] << 1 >> 17).Simplify();
            }

            public BigDecimal Simplify()
            {
                BigDecimal v = Truncate(this, Precision + SubPrecision);

                if (mantissa.IsZero)
                    exponent = 0;
                else
                {
                    BigInteger shortened = BigInteger.DivRem(mantissa, 10, out BigInteger remainder);
                    while (remainder == 0)
                    {
                        mantissa = shortened;
                        exponent--;
                        shortened = BigInteger.DivRem(mantissa, 10, out remainder);
                    }
                }

                return v;
            }

            public static BigDecimal Parse(string input)
            {
                input = input.Replace(',', '.');
                if (input.Contains('.'))
                    if (input.Length - Precision - SubPrecision > input.IndexOf('.'))
                        input = input.Remove(input.IndexOf('.') + Precision + SubPrecision);

                return BigInteger.TryParse(input.Replace(".", ""), out BigInteger mantissa) ?
                new BigDecimal(
                    mantissa, input.Contains('.') ? input.Length - input.IndexOf('.') - 1 : 0
                ).Simplify() : throw new ArgumentException();
            }

            public string ToFullString()
            {
                BigDecimal value = this;
                string result = value.mantissa.ToString(),
                       Sign = (this.Sign < 0 ? "-" : "");
                if (value.exponent > 0)
                {
                    result = result.PadLeft(value.exponent + 1, '0');
                    result = (result.Insert(result.Length - value.exponent, ".")
                        + new string('0', Precision));

                    return Sign + result.TrimEnd('0').TrimEnd('.');
                }
                return Sign + result + new string('0', -value.exponent);
            }

            public override string ToString()
            {
                BigDecimal value = Round(Abs(this), Precision);
                string result = value.mantissa.ToString(),
                       Sign = (this.Sign < 0 ? "-" : "");
                if (value.exponent > 0)
                {
                    result = result.PadLeft(value.exponent + 1, '0');
                    result = (result.Insert(result.Length - value.exponent, ".")
                        + new string('0', Precision))
                        .Remove(result.Length - value.exponent + Precision);

                    return Sign + result.TrimEnd('0').TrimEnd('.');
                }
                return Sign + result + new string('0', -value.exponent);
            }

            public override bool Equals(object obj) =>
                !ReferenceEquals(obj, null)
                && obj is BigDecimal other 
                && mantissa == other.mantissa 
                && exponent == other.exponent;

            public override int GetHashCode() =>
                base.GetHashCode();

            public int CompareTo(object obj) =>
                obj is BigDecimal other ? (this - other).Sign 
                : throw new ArgumentException($"Unable to compare {GetType().Name} to {obj.GetType().Name}.");
        }

    }
}