using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMath_
{
    public partial struct BitArray
    {
        private bool[] bits;
        public int Length => bits.Length;
        public static readonly BitArray Empty = new(new bool[] { false });
        public static readonly BitArray Single = new(new bool[] { true });

        public BitArray(bool[] bits)
        {
            int len = bits.Length;
            while (len-- > 0) if (bits[len]) break;

            if (len < 0) this.bits = bits[..1];
            else this.bits = bits[..++len];
        }

        public bool this[int index] { get => bits[index]; set => bits[index] = value; }
        public bool[] this[Range range] { get => bits[range]; }

        public static BitArray Or(BitArray left, BitArray right)
        {
            int len = Math.Max(left.Length, right.Length);
            bool[] bits = new bool[len];

            Parallel.For(0, len, i => bits[i] = i < left.Length && left.bits[i] || i < right.Length && right.bits[i]);

            return new(bits);
        }

        public static BitArray Xor(BitArray left, BitArray right)
        {
            int len = Math.Max(left.Length, right.Length);
            bool[] bits = new bool[len];

            Parallel.For(0, len, i => bits[i] = (i < left.Length && left.bits[i]) != (i < right.Length && right.bits[i]));

            return new(bits);
        }

        public static BitArray And(BitArray left, BitArray right)
        {
            int len = Math.Max(left.Length, right.Length);
            bool[] bits = new bool[len];

            Parallel.For(0, len, i => bits[i] = i < left.Length && left.bits[i] && i < right.Length && right.bits[i]);

            return new(bits);
        }

        public static BitArray Not(BitArray value)
        {
            bool[] bits = new bool[value.Length];

            Parallel.For(0, value.Length, i => bits[i] = !value.bits[i]);

            return new(bits);
        }

        public static BitArray Shift(BitArray value, int offset)
        {
            int len = value.Length - offset;
            if (len < 1) return Empty;
            bool[] bits = new bool[len];

            Parallel.For(0, len, i => bits[i] = (i += offset) >= 0 && i < value.Length && value.bits[i]);

            return new(bits);
        }

        /// <summary> Greater then </summary>
        /// <returns> <paramref name="left"/> > <paramref name="right"/> </returns>
        public static bool Gt(BitArray left, BitArray right)
        {
            if (left.Length != right.Length)
                return left.Length > right.Length;

            for (int i = left.Length - 1; i >= 0; i--)
                if (right.bits[i] != left.bits[i]) return left.bits[i];
            
            return false;
        }

        /// <summary> Lower then </summary>
        /// <returns> <paramref name="left"/> < <paramref name="right"/> </returns>
        public static bool Lt(BitArray left, BitArray right)
        {
            if (left.Length != right.Length)
                return left.Length < right.Length;
            
            for (int i = left.Length - 1; i >= 0; i--)
                if (left.bits[i] != right.bits[i]) return right.bits[i];
            
            return false;
        }

        /// <summary> Greater then or equals </summary>
        /// <returns> <paramref name="left"/> >= <paramref name="right"/> </returns>
        public static bool Gte(BitArray left, BitArray right) => !Lt(left, right);

        /// <summary> Lower then or equals </summary>
        /// <returns> <paramref name="left"/> <= <paramref name="right"/> </returns>
        public static bool Lte(BitArray left, BitArray right) => !Gt(left, right);

        /// <summary> Equals </summary>
        /// <returns> <paramref name="left"/> == <paramref name="right"/> </returns>
        public static bool Eq(BitArray left, BitArray right)
        {
            if (left.Length != right.Length) return false;

            for (int i = 0; i < left.Length; i++)
                if (left.bits[i] != right.bits[i]) return false;

            return true;
        }

        public static BitArray Min(params BitArray[] array) => array.Min();
        public static BitArray Max(params BitArray[] array) => array.Max();

        public static BitArray operator ~(BitArray value) => Not(value);
        public static BitArray operator |(BitArray left, BitArray right) => Or(left, right);
        public static BitArray operator ^(BitArray left, BitArray right) => Xor(left, right);
        public static BitArray operator &(BitArray left, BitArray right) => And(left, right);
        public static BitArray operator <<(BitArray value, int offset) => Shift(value, -offset);
        public static BitArray operator >>(BitArray value, int offset) => Shift(value, offset);

        public static bool operator <(BitArray left, BitArray right) => Lt(left, right);
        public static bool operator >(BitArray left, BitArray right) => Gt(left, right);
        public static bool operator <=(BitArray left, BitArray right) => Lte(left, right);
        public static bool operator >=(BitArray left, BitArray right) => Gte(left, right);
        public static bool operator ==(BitArray left, BitArray right) => Eq(left, right);
        public static bool operator !=(BitArray left, BitArray right) => !Eq(left, right);

        public bool Contains(bool value) => bits.Contains(value);
        public static bool Contains(BitArray array, bool value) => array.bits.Contains(value);
        public static BitArray Reverse(BitArray value) => new(value.bits.Reverse().ToArray());

        public static implicit operator BitArray(bool[] array) => new(array);
        public static explicit operator bool[](BitArray array) => array.bits;
        public static explicit operator bool(BitArray array) => array.Contains(true);

        public override string ToString()
        {
            StringBuilder sb = new();
            
            for (int i = Length - 1; i >= 0; i--)
                sb.Append(bits[i] ? '1' : '0');

            return sb.ToString();
        }

        public override bool Equals(object obj) =>
            obj is BitArray array && Eq(this, array);

        public override int GetHashCode() =>
            HashCode.Combine(bits);
    }
}
