using System;
using System.Linq;
using System.Numerics;

namespace AMath
{
    public static partial class BigMath
    {
        public class BigDecimal
        {
            public BigInteger mantissa;
            public int exponenta = 0;

            public int Sign => mantissa.Sign;

            public BigDecimal() { }
            public BigDecimal(BigInteger mantissa, int exponenta)
            {
                this.mantissa = mantissa;
                this.exponenta = exponenta;
            }

            public static BigDecimal operator ++(BigDecimal value) =>
                value + 1;

            public static BigDecimal operator --(BigDecimal value) =>
                value - 1;

            public static BigDecimal operator +(BigDecimal left, BigDecimal right) =>
                new BigDecimal()
                {
                    mantissa = left.exponenta > right.exponenta ?
                        left.mantissa + right.mantissa * BigInteger.Pow(10, left.exponenta - right.exponenta) :
                        left.mantissa * BigInteger.Pow(10, right.exponenta - left.exponenta) + right.mantissa,
                    exponenta = left.exponenta > right.exponenta ? left.exponenta : right.exponenta
                }.Simplify();

            public static BigDecimal operator -(BigDecimal left, BigDecimal right) =>
                new BigDecimal()
                {
                    mantissa = left.exponenta > right.exponenta ?
                        left.mantissa - right.mantissa * BigInteger.Pow(10, left.exponenta - right.exponenta) :
                        left.mantissa * BigInteger.Pow(10, right.exponenta - left.exponenta) - right.mantissa,
                    exponenta = left.exponenta > right.exponenta ? left.exponenta : right.exponenta
                }.Simplify();

            public static BigDecimal operator *(BigDecimal left, BigDecimal right) =>
                new BigDecimal()
                {
                    mantissa = left.mantissa * right.mantissa,
                    exponenta = left.exponenta + right.exponenta
                }.Simplify();

            public static BigDecimal operator /(BigDecimal left, BigDecimal right) =>
                new BigDecimal()
                {
                    mantissa = left.mantissa * BigInteger.Pow(10, Precision * 2 + SubPrecision * 2) / right.mantissa,
                    exponenta = left.exponenta - right.exponenta + Precision * 2 + SubPrecision * 2
                }.Simplify();
                
            public static BigDecimal operator %(BigDecimal left, BigDecimal right) =>
                left - right * Truncate(left / right);

            public static bool operator <(BigDecimal left, BigDecimal right) =>
                (left - right).Sign < 0;

            public static bool operator <=(BigDecimal left, BigDecimal right) =>
                (left - right).Sign <= 0 || left == right;

            public static bool operator >(BigDecimal left, BigDecimal right) =>
                (left - right).Sign > 0;

            public static bool operator >=(BigDecimal left, BigDecimal right) =>
                (left - right).Sign >= 0 || left == right;

            public static bool operator ==(BigDecimal left, BigDecimal right) =>
                !(left - right > Epsilon);

            public static bool operator !=(BigDecimal left, BigDecimal right) =>
                left - right > Epsilon;

            public static BigDecimal operator -(BigDecimal value) =>
                -1 * value;
            
            public static explicit operator BigInteger(BigDecimal value) =>
                value.exponenta > 0 ?
                 value.mantissa / BigInteger.Pow(10, value.exponenta) :
                 value.mantissa * BigInteger.Pow(10, -value.exponenta);

            public static implicit operator BigDecimal(int value) =>
                new BigDecimal() { mantissa = value }.Simplify();

            public static implicit operator BigDecimal(BigInteger value) =>
                new BigDecimal() { mantissa = value }.Simplify();

            public static implicit operator BigDecimal(double value)
            {
                if (double.IsInfinity(value) || double.IsNaN(value)) return 0;
                long bits = BitConverter.DoubleToInt64Bits(value);

                bool negative = (bits & (1L << 63)) != 0;
                int exponent = (int)((bits >> 52) & 0x7ffL);
                BigInteger mantissa = bits & 0xfffffffffffffL;

                if (exponent == 0)
                    exponent++;
                else mantissa |= 1L << 52;

                exponent -= 1075;

                if (mantissa == 0)
                    return negative ? -0 : 0;

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

                return new BigDecimal()
                {
                    mantissa = negative ? -mantissa : mantissa
                } / ((BigInteger)1 << -exponent);
            }

            public static implicit operator BigDecimal(decimal value)
            {
                int[] bits = decimal.GetBits(value);

                BigInteger mantissa = (bits[2] << 64) + (bits[1] << 32) + bits[0];

                if ((bits[3] & 0x80000000) == 0x80000000)
                    mantissa = -mantissa;

                return new BigDecimal()
                {
                    mantissa = mantissa,
                    exponenta = bits[3] << 1 >> 17
                }.Simplify();
            }

            public BigDecimal Simplify()
            {
                BigDecimal v = Truncate(this, Precision + SubPrecision);

                if (v.mantissa == 0) return new BigDecimal()
                {
                    mantissa = 0,
                    exponenta = 0
                };

                while (v.mantissa % 10 == 0)
                {
                    v.mantissa /= 10;
                    v.exponenta--;
                }

                return v;
            }

            public static BigDecimal Parse(string input)
            {
                input = input.Replace(',', '.');
                if (input.Contains('.'))
                    if (input.Length - Precision - SubPrecision > input.IndexOf('.'))
                        input = input.Remove(input.IndexOf('.') + Precision + SubPrecision);

                return new BigDecimal()
                {
                    mantissa = BigInteger.Parse(input.Replace(".", "")),
                    exponenta = input.Contains('.') ? input.Length - input.IndexOf('.') - 1 : 0
                }.Simplify();
            }

            public override string ToString()
            {
                BigDecimal value = Round(Abs(this), Precision + 1);
                string result = value.mantissa.ToString();
                if (value.exponenta > 0)
                {
                    result = result.PadLeft(value.exponenta + 1, '0');
                    result = result.Insert(result.Length - value.exponenta, ".")
                        .PadRight(result.Length - value.exponenta + 1, '0');

                    // if (result.Length - value.exponenta + 1 > Precision)
                    //     result = result.Remove(result.Length - value.exponenta + Precision + 1);

                    return (Sign < 0 ? "-" : "") + result
                        .TrimEnd('0').TrimEnd('.');
                }
                return (Sign < 0 ? "-" : "") + result
                    .PadRight(result.Length - value.exponenta, '0');
            }

            public override bool Equals(object obj) =>
                base.Equals(obj);

            public override int GetHashCode() =>
                base.GetHashCode();
        }

    }
}