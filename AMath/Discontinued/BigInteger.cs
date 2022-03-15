using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AMath_
{
    public partial struct BigInteger
    {
        private BitArray bits;
        private bool Negative;

        public string Bits => bits.ToString();
        public int Sign => BitArray.Eq(bits, BitArray.Empty) ? 0 : Negative ? -1 : 1;

        public static readonly BigInteger Zero = 0,
                                          One = 1,
                                          Ten = 10;

        public BigInteger(ulong value) : this(BitArray.Empty, false)
        {
            bool[] bits = new bool[sizeof(ulong) << 3];
            for (int i = 0; i < bits.Length && value > 0; i++)
            {
                bits[i] = (value & 1) == 1;
                value >>= 1;
            }
            this.bits = new(bits);
        }
        
        public BigInteger(long value) : this((ulong)(value < 0 ? -value : value)) { Negative = value < 0; }
        public BigInteger(uint value) : this((ulong)value) { }
        public BigInteger(int value) : this((long)value) { }
        public BigInteger(ushort value) : this((ulong)value) { }
        public BigInteger(short value) : this((long)value) { }
        public BigInteger(char value) : this((ulong)value) { }
        public BigInteger(sbyte value) : this((long)value) { }
        public BigInteger(byte value) : this((ulong)value) { }

        public BigInteger(BitArray bits, bool negative)
        {
            this.bits = bits;
            
            Negative = negative;
        }

        public static BigInteger Abs(BigInteger value) => new(value.bits, false);

        /// <summary> Addition </summary>
        /// <returns> <paramref name="left"/> + <paramref name="right"/> </returns>
        public static BigInteger Add(BigInteger left, BigInteger right) =>
            new(left.Negative != right.Negative ? SubPositive(left.bits, right.bits) : AddPositive(left.bits, right.bits), false);

        /// <summary> Subtraction </summary>
        /// <returns> <paramref name="left"/> - <paramref name="right"/> </returns>
        public static BigInteger Sub(BigInteger left, BigInteger right) =>
            new(left.Negative != right.Negative ? AddPositive(left.bits, right.bits) : SubPositive(Max(left, right).bits, Min(left, right).bits), Lt(left, right) != right.Negative);

        private static BitArray AddPositive(BitArray left, BitArray right)
        {
            BitArray carry, summand = left, addend = right;

            while (addend.Contains(true))
            {
                carry = summand & addend;
                summand ^= addend;
                addend = carry << 1;
            }

            return summand;
        }

        private static BitArray SubPositive(BitArray left, BitArray right)
        {
            if (right >= left) return BitArray.Empty;
            BitArray borrow, minuend = left, subtrahend = right;

            while (subtrahend.Contains(true))
            {
                borrow = ~minuend & subtrahend;
                minuend ^= subtrahend;
                subtrahend = borrow << 1;
            }

            return minuend;
        }

        /// <summary> multiplication </summary>
        /// <returns> <paramref name="left"/> * <paramref name="right"/> </returns>
        public static BigInteger Mul(BigInteger left, BigInteger right)
        {
            BitArray result = BitArray.Empty,
                min = left.bits.Length > right.bits.Length ? right.bits : left.bits,
                max = left.bits.Length > right.bits.Length ? left.bits : right.bits;

            for (int i = 0; i < min.Length; max <<= 1)
                if (min[i++]) result = AddPositive(result, max);

            return new(result, left.Negative != right.Negative);
        }

        /// <summary> Division </summary>
        /// <returns> <paramref name="left"/> / <paramref name="right"/> </returns>
        public static BigInteger Div(BigInteger left, BigInteger right) =>
            DivRem(left, right, out _);

        /// <summary> Remainder </summary>
        /// <returns> <paramref name="left"/> % <paramref name="right"/> </returns>
        public static BigInteger Rem(BigInteger left, BigInteger right)
        {
            _ = DivRem(left, right, out BigInteger rem);
            return rem;
        }

        /// <summary> Division with remainder </summary>
        /// <returns> <paramref name="left"/> / <paramref name="right"/> </returns>
        public static BigInteger DivRem(BigInteger left, BigInteger right, out BigInteger remainder)
        {
            if (Lt(left, right))
            {
                remainder = left;
                return Zero;
            }

            if (Eq(left, right))
            {
                remainder = Zero;
                return One;
            }

            BitArray dividend = left.bits, divisor = right.bits,
                quotient = BitArray.Empty;

            int d = dividend.Length - divisor.Length;
            divisor <<= dividend.Length - divisor.Length;

            while (d-- >= 0/*BitArray.Gte(dividend, right.bits)*/)
            {
                quotient <<= 1;
                if (BitArray.Gte(dividend, divisor))
                {
                    dividend = SubPositive(dividend, divisor);
                    quotient |= BitArray.Single;
                }
                divisor >>= 1;
            }

            remainder = new(dividend, false);

            return new(quotient, left.Negative != right.Negative);
        }

        /// <summary> Greater then </summary>
        /// <returns> <paramref name="left"/> > <paramref name="right"/> </returns>
        public static bool Gt(BigInteger left, BigInteger right) =>
            left.Negative != right.Negative? right.Negative : BitArray.Gt(left.bits, right.bits);

        /// <summary> Lower then </summary>
        /// <returns> <paramref name="left"/> < <paramref name="right"/> </returns>
        public static bool Lt(BigInteger left, BigInteger right) =>
            left.Negative != right.Negative ? left.Negative : BitArray.Lt(left.bits, right.bits);

        /// <summary> Greater then or equals </summary>
        /// <returns> <paramref name="left"/> >= <paramref name="right"/> </returns>
        public static bool Gte(BigInteger left, BigInteger right) => !Lt(left, right);

        /// <summary> Lower then or equals </summary>
        /// <returns> <paramref name="left"/> <= <paramref name="right"/> </returns>
        public static bool Lte(BigInteger left, BigInteger right) => !Gt(left, right);

        public static bool Eq(BigInteger left, BigInteger right) =>
            left.Negative == right.Negative && BitArray.Eq(left.bits, right.bits);

        public static BigInteger Max(params BigInteger[] input)
        {
            int maxIndex = 0;
            for (int i = 1; i < input.Length; i++)
                if (Gt(input[i], input[maxIndex]))
                    maxIndex = i;

            return input[maxIndex];
        }

        public static BigInteger Min(params BigInteger[] input)
        {
            int minIndex = 0;
            for (int i = 1; i < input.Length; i++)
                if (Lt(input[i], input[minIndex]))
                    minIndex = i;

            return input[minIndex];
        }

        public static BigInteger Pow(BigInteger value, int power)
        {
            BigInteger result = One;
            do {
                if ((power & 1) == 1)
                    result *= value;
                value *= value;
            } while ((power >>= 1) > 0);
            return result;
        }

        public int GetBitLength() => bits.Length;

        public static BigInteger operator +(BigInteger value) => new(value.bits, value.Negative);
        public static BigInteger operator -(BigInteger value) => new(value.bits, !value.Negative);
        public static BigInteger operator ~(BigInteger value) => new(BitArray.Not(value.bits), !value.Negative);

        public static BigInteger operator +(BigInteger left, BigInteger right) => Add(left, right);
        public static BigInteger operator -(BigInteger left, BigInteger right) => Sub(left, right);
        public static BigInteger operator *(BigInteger left, BigInteger right) => Mul(left, right);
        public static BigInteger operator /(BigInteger left, BigInteger right) => Div(left, right);
        public static BigInteger operator %(BigInteger left, BigInteger right) => Rem(left, right);

        public static bool operator ==(BigInteger left, BigInteger right) => Eq(left, right);
        public static bool operator !=(BigInteger left, BigInteger right) => !Eq(left, right);
        public static bool operator <=(BigInteger left, BigInteger right) => Lte(left, right);
        public static bool operator >=(BigInteger left, BigInteger right) => Gte(left, right);
        public static bool operator <(BigInteger left, BigInteger right) => Lt(left, right);
        public static bool operator >(BigInteger left, BigInteger right) => Gt(left, right);

        public static BigInteger operator &(BigInteger left, BigInteger right) => new(BitArray.And(left.bits, right.bits), left.Negative && right.Negative);
        public static BigInteger operator ^(BigInteger left, BigInteger right) => new(BitArray.Xor(left.bits, right.bits), left.Negative != right.Negative);
        public static BigInteger operator |(BigInteger left, BigInteger right) => new(BitArray.Or(left.bits, right.bits), left.Negative || right.Negative);

        public static BigInteger operator <<(BigInteger value, int offset) => new(BitArray.Shift(value.bits, -offset), value.Negative);
        public static BigInteger operator >>(BigInteger value, int offset) => new(BitArray.Shift(value.bits, offset), value.Negative);

        public static implicit operator BigInteger(ulong value) => new(value);
        public static implicit operator BigInteger(long value) => new(value);
        public static implicit operator BigInteger(uint value) => new(value);
        public static implicit operator BigInteger(int value) => new(value);
        public static implicit operator BigInteger(ushort value) => new(value);
        public static implicit operator BigInteger(short value) => new(value);
        public static implicit operator BigInteger(char value) => new(value);
        public static implicit operator BigInteger(byte value) => new(value);
        public static implicit operator BigInteger(sbyte value) => new(value);

        public static explicit operator ulong(BigInteger value)
        {
            if (value.bits.Length > sizeof(ulong) << 3 || value.Negative)
                throw new ArgumentOutOfRangeException(nameof(value));

            ulong res = 0;
            for (int i = value.bits.Length; i > 0;)
                res = (res << 1) + (value.bits[--i] ? 1ul : 0);
            
            return res;
        }

        public static explicit operator long(BigInteger value)
        {
            if (value.bits.Length > (sizeof(long) << 3) - 1)
                throw new ArgumentOutOfRangeException(nameof(value));

            long res = 0;
            for (int i = value.bits.Length; i > 0;)
                res = (res << 1) + (value.bits[--i] ? 1 : 0);
            
            return value.Negative ? -res : res;
        }

        public static explicit operator uint(BigInteger value) => (uint)(ulong)value;
        public static explicit operator int(BigInteger value) => (int)(long)value;
        public static explicit operator ushort(BigInteger value) => (ushort)(ulong)value;
        public static explicit operator short(BigInteger value) => (short)(long)value;
        public static explicit operator char(BigInteger value) => (char)(ulong)value;
        public static explicit operator byte(BigInteger value) => (byte)(ulong)value;
        public static explicit operator sbyte(BigInteger value) => (sbyte)(long)value;

        public static bool TryParse(string input, out BigInteger value)
        {
            Regex regex = new(@"[+-]?\d+");

            value = Zero;
            if (!regex.IsMatch(input)) return false;

            for (int i = 0; i < input.Length; i++)
            {
                value *= Ten;
                value += input[i] - '0';
            }

            return true;
        }

        public static BigInteger Parse(string input)
        {
            Regex regex = new(@"[+-]?\d+");
            if (!regex.IsMatch(input)) throw new ArgumentException(null, nameof(input));
            
            BigInteger value = Zero;
            for (int i = 0; i < input.Length; i++)
            {
                value *= Ten;
                value += input[i] - '0';
            }

            return value;
        }

        public override string ToString()
        {
            if (bits.Length == 1 && !bits[0]) return "0";

            BigInteger abs = Abs(this);
            
            StringBuilder sb = new();
            if (Negative) sb.Append('-');

            while (abs.bits.Contains(true))
            {
                abs = DivRem(abs, Ten, out BigInteger rem);
                sb.Insert(Negative ? 1 : 0, (char)('0' + (char)rem));
            }
            return sb.ToString();
        }
    }
}
