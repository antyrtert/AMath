using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AMath
{
    [Serializable]
    public partial struct BigRational
    {
        public BigInteger numerator, denominator;

        public int Sign => numerator.Sign * denominator.Sign;

        public BigRational(BigInteger numerator) : this(numerator, 1) { }
        public BigRational(BigInteger numerator, BigInteger denominator)
        {
            this.numerator = numerator;
            this.denominator = denominator;
            Normalize();
        }

        public static implicit operator BigRational(BigInteger value) => new(value);
        public static implicit operator BigRational(ulong value) => new(value);
        public static implicit operator BigRational(long value) => new(value);
        public static implicit operator BigRational(uint value) => new(value);
        public static implicit operator BigRational(int value) => new(value);
        public static implicit operator BigRational(char value) => new(value);
        public static implicit operator BigRational(ushort value) => new(value);
        public static implicit operator BigRational(short value) => new(value);
        public static implicit operator BigRational(byte value) => new(value);
        public static implicit operator BigRational(sbyte value) => new(value);

        public static explicit operator BigInteger(BigRational value) => value.numerator / value.denominator;
        public static explicit operator ulong(BigRational value) => (ulong)(BigInteger)value;
        public static explicit operator long(BigRational value) => (long)(BigInteger)value;
        public static explicit operator uint(BigRational value) => (uint)(BigInteger)value;
        public static explicit operator int(BigRational value) => (int)(BigInteger)value;
        public static explicit operator char(BigRational value) => (char)(BigInteger)value;
        public static explicit operator ushort(BigRational value) => (ushort)(BigInteger)value;
        public static explicit operator short(BigRational value) => (short)(BigInteger)value;
        public static explicit operator byte(BigRational value) => (byte)(BigInteger)value;
        public static explicit operator sbyte(BigRational value) => (sbyte)(BigInteger)value;

        public static BigRational operator +(BigRational left, BigRational right) =>
            new(left.numerator * right.denominator + right.numerator * left.denominator, left.denominator * right.denominator);
        public static BigRational operator -(BigRational left, BigRational right) =>
            new(left.numerator * right.denominator - right.numerator * left.denominator, left.denominator * right.denominator);
        public static BigRational operator *(BigRational left, BigRational right) =>
            new(left.numerator * right.numerator, left.denominator * right.denominator);
        public static BigRational operator /(BigRational left, BigRational right) =>
            new(left.numerator * right.denominator, left.denominator * right.numerator);

        public static bool operator ==(BigRational left, BigRational right) =>
            left.numerator == right.numerator && left.denominator == right.denominator;
        public static bool operator !=(BigRational left, BigRational right) =>
            left.numerator != right.numerator || left.denominator != right.denominator;
        public static bool operator >(BigRational left, BigRational right) => (left - right).Sign == 1;
        public static bool operator <(BigRational left, BigRational right) => (right - left).Sign == 1;
        public static bool operator >=(BigRational left, BigRational right) => (right - left).Sign != 1;
        public static bool operator <=(BigRational left, BigRational right) => (left - right).Sign != 1;

        private void Normalize()
        {
            int max = (int)Math.Max(BigInteger.Log10(numerator), BigInteger.Log10(denominator));
            if (max > precision + 16)
            {
                BigInteger div = BigInteger.Pow(10, max - precision - 16);
                numerator /= div;
                denominator /= div;
            }
            BigInteger GCD = BigInteger.GreatestCommonDivisor(numerator, denominator);
            while (GCD > 1)
            {
                numerator /= GCD;
                denominator /= GCD;
                GCD = BigInteger.GreatestCommonDivisor(numerator, denominator);
            }
        }

        public override string ToString()
        {
            if (precision == 0) return (numerator / denominator).ToString();
            string output = (numerator * BigInteger.Pow(10, precision) / denominator).ToString();
            return output.PadLeft(precision + 1, '0').Insert(output.Length - precision, ".").TrimEnd('0').TrimEnd('.');
        }

        public override bool Equals(object obj) => obj is BigRational other && other == this;
        public override int GetHashCode() => HashCode.Combine(numerator, denominator, nameof(BigRational));
    }
}
