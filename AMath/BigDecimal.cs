using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Numerics;

namespace AMath
{
    /// <summary> Represents large real number. </summary>
    [Serializable]
    public partial struct BigDecimal : IComparable<BigDecimal>, IConvertible, ICloneable, IEquatable<BigDecimal>, IFormattable
    {
        private BigInteger value;
        private int scale;

        public int Sign => value.Sign;
        private int Log10 => scale + (int)BigInteger.Log10(value);

        #region Constructors
        public BigDecimal(sbyte value) : this(value, 0) { }
        public BigDecimal(byte value) : this(value, 0) { }
        public BigDecimal(short value) : this(value, 0) { }
        public BigDecimal(ushort value) : this(value, 0) { }
        public BigDecimal(char value) : this(value, 0) { }
        public BigDecimal(int value) : this(value, 0) { }
        public BigDecimal(uint value) : this(value, 0) { }
        public BigDecimal(long value) : this(value, 0) { }
        public BigDecimal(ulong value) : this(value, 0) { }
        public BigDecimal(BigInteger value) : this(value, 0) { }

        private const int decimalScaleMask = 0x00ff0000,
                          decimalScaleOffset = 16;

        public BigDecimal(decimal value)
        {
            this.value = 0;
            uint[] bytes = decimal.GetBits(value).Select(i => unchecked((uint)i)).ToArray();
            for (int i = 2; i >= 0; i--) this.value = (this.value << 32) + bytes[i];
            if (bytes[3] >> 31 == 1) this.value = -this.value;
            this.scale = (int)(bytes[3] & decimalScaleMask) >> decimalScaleOffset;
            Normalize();
        }

        private const long doubleSignMask = unchecked((long)0x8000000000000000),
                           doubleScaleMask = 0x7ff0000000000000;
        private const int doubleScaleOffset = 52;

        public BigDecimal(float value) : this((double)value) { }
        public BigDecimal(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
                throw new ArgumentException($"Cannot convert NaN or Infinity to {typeof(BigDecimal)}", nameof(value));

            long bits = BitConverter.DoubleToInt64Bits(value);

            scale = (int)((bits & doubleScaleMask) >> doubleScaleOffset);
            this.value = bits & ~(doubleScaleMask | doubleSignMask);

            if (scale != 0) this.value |= 1L << doubleScaleOffset;
            else scale++;

            if ((bits & doubleSignMask) == doubleSignMask) this.value = -this.value;
            scale -= (int)(doubleScaleMask >> doubleScaleOffset + 1) + doubleScaleOffset;

            while ((this.value & 1) == 0 && scale < 0)
            {
                this.value >>= 1;
                scale++;
            }

            if (scale > 0)
            {
                this.value <<= scale;
                scale = 0;
            }

            this.value *= BigInteger.Pow(5, scale = -scale);
            Normalize();
        }

        private BigDecimal(BigInteger value, int scale)
        {
            this.value = value;
            this.scale = scale;
            Normalize();
        }
        #endregion

        #region Implicit convertion operators
        public static implicit operator BigDecimal(sbyte value) => new(value);
        public static implicit operator BigDecimal(byte value) => new(value);
        public static implicit operator BigDecimal(short value) => new(value);
        public static implicit operator BigDecimal(ushort value) => new(value);
        public static implicit operator BigDecimal(char value) => new(value);
        public static implicit operator BigDecimal(int value) => new(value);
        public static implicit operator BigDecimal(uint value) => new(value);
        public static implicit operator BigDecimal(long value) => new(value);
        public static implicit operator BigDecimal(ulong value) => new(value);
        public static implicit operator BigDecimal(float value) => new(value);
        public static implicit operator BigDecimal(double value) => new(value);
        public static implicit operator BigDecimal(decimal value) => new(value);
        public static implicit operator BigDecimal(BigInteger value) => new(value);
        #endregion

        #region Explicit convertion operators
        public static explicit operator BigInteger(BigDecimal value) => Truncate(value).value * BigInteger.Pow(10, Math.Max(0, -value.scale));
        public static explicit operator ulong(BigDecimal value) => (ulong)(BigInteger)value;
        public static explicit operator long(BigDecimal value) => (long)(BigInteger)value;
        public static explicit operator uint(BigDecimal value) => (uint)(BigInteger)value;
        public static explicit operator int(BigDecimal value) => (int)(BigInteger)value;
        public static explicit operator char(BigDecimal value) => (char)(BigInteger)value;
        public static explicit operator ushort(BigDecimal value) => (ushort)(BigInteger)value;
        public static explicit operator short(BigDecimal value) => (short)(BigInteger)value;
        public static explicit operator byte(BigDecimal value) => (byte)(BigInteger)value;
        public static explicit operator sbyte(BigDecimal value) => (sbyte)(BigInteger)value;
        public static explicit operator float(BigDecimal value) => (float)(double)value;
        public static explicit operator double(BigDecimal value) => ToDouble(value);
        public static explicit operator decimal(BigDecimal value) => ToDecimal(value);
        #endregion

        #region Boolean operators
        public static bool operator >(BigDecimal left, BigDecimal right) => (left - right).Sign == 1;
        public static bool operator <(BigDecimal left, BigDecimal right) => (right - left).Sign == 1;
        public static bool operator >=(BigDecimal left, BigDecimal right) => (right - left).Sign != 1;
        public static bool operator <=(BigDecimal left, BigDecimal right) => (left - right).Sign != 1;
        public static bool operator ==(BigDecimal left, BigDecimal right) => (left - right).Sign == 0;
        public static bool operator !=(BigDecimal left, BigDecimal right) => (left - right).Sign != 0;
        #endregion

        #region Arithmetic operators
        public static BigDecimal operator ++(BigDecimal value) => value + One;
        public static BigDecimal operator --(BigDecimal value) => value - One;
        public static BigDecimal operator +(BigDecimal value) => new(value.value, value.scale);
        public static BigDecimal operator -(BigDecimal value) => new(-value.value, value.scale);

        public static BigDecimal operator +(BigDecimal left, BigDecimal right) => left.scale > right.scale
            ? new(left.value + right.value * BigInteger.Pow(10, left.scale - right.scale), left.scale)
            : new(left.value * BigInteger.Pow(10, right.scale - left.scale) + right.value, right.scale);
        
        public static BigDecimal operator -(BigDecimal left, BigDecimal right) => left.scale > right.scale
            ? new(left.value - right.value * BigInteger.Pow(10, left.scale - right.scale), left.scale)
            : new(left.value * BigInteger.Pow(10, right.scale - left.scale) - right.value, right.scale);

        public static BigDecimal operator *(BigDecimal left, BigDecimal right) =>
            new(left.value * right.value, left.scale + right.scale);

        public static BigDecimal operator /(BigDecimal left, BigDecimal right) =>
            new(left.value * BigInteger.Pow(10, TotalPrecision) / right.value, left.scale - right.scale + TotalPrecision);

        public static BigDecimal operator %(BigDecimal left, BigDecimal right) =>
            left - Truncate(left / right) * right;
        #endregion

        #region String functions
        public static BigDecimal Parse(string input) => TryParse(input, out BigDecimal res) ? res : 
            throw new ArgumentException($"Failed to parse string: \"{input}\" to {nameof(BigDecimal)}", nameof(input));

        public static bool TryParse(string input, out BigDecimal output)
        {
            output = MinusOne;
            Regex rgx = new(@"([+-]?\d*)?[.,]?\d+([Ee][+-]?\d+)?");

            string[] split = (input = input.Replace(" ", "")).Split('E', 'e');
            if (rgx.IsMatch(input) && BigInteger.TryParse(split[0].Replace(".", "").Replace(",", ""), out BigInteger m))
            {
                output.value = m;
                int decimalPoint = split[0].Replace(',', '.').IndexOf('.');

                if (decimalPoint >= 0)
                    output.scale += split[0].Length - decimalPoint - 1;

                if (split.Length > 1 && int.TryParse(split[1], out int s))
                    output.scale -= s;
                output.Normalize();

                return true;
            }
            return false;
        }

        public string ToString(string format, IFormatProvider formatProvider) => ToString();
        public override string ToString()
        {
            BigDecimal value = Round(Abs(this), Precision);
            string result = value.value.ToString(),
                   Sign   = this.Sign < 0 ? "-" : "";

            if (value.scale > 0)
            {
                result = result.PadLeft(value.scale + 1, '0');
                result = (result.Insert(result.Length - value.scale, ".")
                    + new string('0', Precision))
                    .Remove(result.Length - value.scale + Precision);

                return Sign + result.TrimEnd('0').TrimEnd('.');
            }
            return Sign + result + new string('0', -value.scale);
        }
        #endregion

        public static double ToDouble(BigDecimal value)
        {
            if (value > double.MaxValue) return double.PositiveInfinity;
            if (value < double.MinValue) return double.NegativeInfinity;
            if (Abs(value) < double.Epsilon) return 0;
            if (double.TryParse(value.ToString().Replace('.', ','), out double d)) return d;
            return double.NaN;
        }

        private const decimal decimalEpsilon = 0.0000000000000000000000000001m;
        public static decimal ToDecimal(BigDecimal value)
        {
            BigDecimal abs = Truncate(Abs(value), 28);
            if (abs > decimal.MaxValue) throw new OverflowException();
            if (abs < decimalEpsilon) return decimal.Zero;

            int hi = unchecked((int)(uint)(abs.value >> 64)),
                mid = unchecked((int)(uint)(abs.value >> 32 & uint.MaxValue)),
                lo = unchecked((int)(uint)(abs.value & uint.MaxValue));

            return new(lo, mid, hi, value.Sign < 0, (byte)abs.scale);
        }

        private void Normalize()
        {
            if (TrimEndZeros)
            {
                BigInteger div = BigInteger.DivRem(value, 10, out BigInteger rem);
                while (rem == 0 && value > 0)
                {
                    value = div;
                    div = BigInteger.DivRem(value, 10, out rem);
                    scale--;
                }
            }
            if (scale > TotalPrecision)
            {
                value /= BigInteger.Pow(10, scale - TotalPrecision);
                scale = TotalPrecision;
            }
        }

        public bool Equals(BigDecimal other) => value == other.value && scale == other.scale; // IEquatable
        public override bool Equals(object obj) => obj is BigDecimal other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(value, scale, nameof(BigDecimal));
        public int CompareTo(BigDecimal other) => (this - other).Sign; //IComparable
        public object Clone() => new BigDecimal(value, scale); // ICloneable

        #region IConvertible
        public bool ToBoolean(IFormatProvider provider) => value != 0;
        
        public sbyte ToSByte(IFormatProvider provider) => (sbyte)this;
        public byte ToByte(IFormatProvider provider) => (byte)this;
        
        public short ToInt16(IFormatProvider provider) => (short)this;
        public ushort ToUInt16(IFormatProvider provider) => (ushort)this;
        public char ToChar(IFormatProvider provider) => (char)this;
        
        public int ToInt32(IFormatProvider provider) => (int)this;
        public uint ToUInt32(IFormatProvider provider) => (uint)this;
        
        public long ToInt64(IFormatProvider provider) => (long)this;
        public ulong ToUInt64(IFormatProvider provider) => (ulong)this;

        public float ToSingle(IFormatProvider provider) => (float)this;
        public double ToDouble(IFormatProvider provider) => (double)this;
        public decimal ToDecimal(IFormatProvider provider) => (decimal)this;

        public DateTime ToDateTime(IFormatProvider provider) => Convert.ToDateTime((double)this);
        public string ToString(IFormatProvider provider) => ToString();

        public TypeCode GetTypeCode() => TypeCode.Object;
        public object ToType(Type conversionType, IFormatProvider provider) => 
            Convert.ChangeType(this, conversionType, provider);
        #endregion
    }
}
