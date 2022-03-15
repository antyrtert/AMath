using ComputeSharp;

using System;
using System.Collections.Generic;
using System.Linq;

namespace AMath_Test
{
    public partial struct BigInteger : IDisposable
    {
        private ReadWriteBuffer<bool> Bits;
        private bool IsNegative;

        public int Sign => Bits.Length > 0 ? (!Bits.ToArray().ToList().Any(b => b) ? 0 : (IsNegative ? -1 : 1)) : 0;

        public BigInteger(sbyte value) : this((long)value) { }
        public BigInteger(byte value) : this((ulong)value) { }
        public BigInteger(char value) : this((ulong)value) { }
        public BigInteger(short value) : this((long)value) { }
        public BigInteger(ushort value) : this((ulong)value) { }
        public BigInteger(int value) : this((long)value) { }
        public BigInteger(uint value) : this((ulong)value) { }

        public BigInteger(ulong value)
        {
            IsNegative = false;
            List<bool> bits = new();
            do { bits.Add((value & 1) == 1); } while ((value >>= 1) > 0);
            Bits = Gpu.Default.AllocateReadWriteBuffer(bits.ToArray());
        }

        public BigInteger(long value)
        {
            if (IsNegative = value < 0) value = ~value + 1;
            List<bool> bits = new();
            do { bits.Add((value & 1) == 1); } while ((value >>= 1) > 0);
            Bits = Gpu.Default.AllocateReadWriteBuffer(bits.ToArray());
        }

        private BigInteger(ReadWriteBuffer<bool> bits, bool isNegative) : this(bits.ToArray(), isNegative) => bits.Dispose();
        private BigInteger(bool[] bits, bool isNegative)
        {
            if (bits.Length > 0) Bits = Gpu.Default.AllocateReadWriteBuffer(bits);
            else Bits = Gpu.Default.AllocateReadWriteBuffer(new bool[] { false });
            IsNegative = isNegative;
            Normalize();
        }

        public static BigInteger Not(BigInteger value)
        {
            bool[] Bits = value.Bits.ToArray();
            for (int i = 0; i < Bits.Length; i++) Bits[i] = !Bits[i];
            return new(Gpu.Default.AllocateReadWriteBuffer(value.Bits), !value.IsNegative);
        }

        private static ReadWriteBuffer<bool> CallCompareShader(ReadWriteBuffer<bool> left, ReadWriteBuffer<bool> right, Func<Bool2, bool> func)
        {
            CompareShader shader = new(left, right, func);
            Gpu.Default.For(shader.ResultBuffer.Length, shader);
            return shader.ResultBuffer;
        }

        public static BigInteger Or(BigInteger left, BigInteger right) => 
            new(CallCompareShader(left.Bits, right.Bits, CompareShader.Or), left.IsNegative || right.IsNegative);
        
        public static BigInteger Xor(BigInteger left, BigInteger right) =>
            new(CallCompareShader(left.Bits, right.Bits, CompareShader.Xor), left.IsNegative ^ right.IsNegative);
        
        public static BigInteger And(BigInteger left, BigInteger right) =>
            new(CallCompareShader(left.Bits, right.Bits, CompareShader.And), left.IsNegative && right.IsNegative);

        public static BigInteger Shift(BigInteger value, int offset)
        {
            List<bool> result = value.Bits.ToArray().ToList();
            
            if (offset > 0) result.RemoveRange(0, offset);
            else result.RemoveRange(result.Count - offset, offset);

            if (result.Count > 0)
                return new(result.ToArray(), value.IsNegative);
            else return new(0);
        }

        public static BigInteger Add(BigInteger left, BigInteger right)
        {
            BigInteger result = left ^ right, carry = (left & right) << 1, temp;

            while (carry > 0)
            {
                temp = result;
                result ^= carry;
                carry = (temp & carry) << 1;
                Console.WriteLine(result.ToString());
            }

            return result;
        }

        public static BigInteger Sub(BigInteger left, BigInteger right)
        {
            BigInteger result = left ^ right, borrow = (~left & right) << 1;

            while (borrow > 0)
            {
                result ^= borrow &= ~result;
                borrow <<= 1;
            }

            return result;
        }

        public static implicit operator BigInteger(sbyte value) => new(value);
        public static implicit operator BigInteger(byte value) => new(value);
        public static implicit operator BigInteger(char value) => new(value);
        public static implicit operator BigInteger(short value) => new(value);
        public static implicit operator BigInteger(ushort value) => new(value);
        public static implicit operator BigInteger(int value) => new(value);
        public static implicit operator BigInteger(uint value) => new(value);
        public static implicit operator BigInteger(long value) => new(value);
        public static implicit operator BigInteger(ulong value) => new(value);

        public static BigInteger operator +(BigInteger value) => new(value.Bits, value.IsNegative);
        public static BigInteger operator -(BigInteger value) => new(value.Bits, !value.IsNegative);

        public static BigInteger operator ~(BigInteger value) => Not(value);
        public static BigInteger operator |(BigInteger left, BigInteger right) => Or(left, right);
        public static BigInteger operator ^(BigInteger left, BigInteger right) => Xor(left, right);
        public static BigInteger operator &(BigInteger left, BigInteger right) => And(left, right);
        public static BigInteger operator <<(BigInteger value, int offset) => Shift(value, offset);
        public static BigInteger operator >>(BigInteger value, int offset) => Shift(value, offset);

        public static bool operator >(BigInteger left, BigInteger right)
        {
            if (left.Bits.Length != right.Bits.Length)
                return left.Bits.Length > right.Bits.Length;
            if (left.IsNegative != right.IsNegative) return right.IsNegative;
            bool[] lbits = left.Bits.ToArray(), rbits = right.Bits.ToArray();
            for (int i = left.Bits.Length - 1; i > -1; i--)
                if (rbits[i] && !lbits[i]) return false;
            return true;
        }
        
        public static bool operator <(BigInteger left, BigInteger right)
        {
            if (left.Bits.Length != right.Bits.Length)
                return left.Bits.Length < right.Bits.Length;
            if (left.IsNegative != right.IsNegative) return left.IsNegative;
            bool[] lbits = left.Bits.ToArray(), rbits = right.Bits.ToArray();
            for (int i = left.Bits.Length - 1; i > -1; i--)
                if (lbits[i] && !rbits[i]) return false;
            return true;
        }
        
        public static bool operator ==(BigInteger left, BigInteger right)
        {
            if (left.Bits.Length != right.Bits.Length) return false;
            if (left.IsNegative != right.IsNegative) return left.Bits.Length == 0;
            bool[] lbits = left.Bits.ToArray(), rbits = right.Bits.ToArray();
            for (int i = left.Bits.Length - 1; i > -1; i--)
                if (lbits[i] != rbits[i]) return false;
            return true;
        }
        
        public static bool operator !=(BigInteger left, BigInteger right)
        {
            if (left.Bits.Length != right.Bits.Length) return true;
            if (left.IsNegative != right.IsNegative) return left.Bits.Length > 0;
            bool[] lbits = left.Bits.ToArray(), rbits = right.Bits.ToArray();
            for (int i = left.Bits.Length - 1; i > -1; i--)
                if (lbits[i] != rbits[i]) return true;
            return false;
        }

        private void Normalize()
        {
            int i = Bits.Length;
            List<bool> bits = Bits.ToArray().ToList();
            while (--i > 0 && !bits[i]) bits.RemoveAt(i);
            if (i > 0) Bits = Gpu.Default.AllocateReadWriteBuffer(bits.ToArray());
            else Bits = Gpu.Default.AllocateReadWriteBuffer<bool>(1);
        }

        public void Dispose() => Bits.Dispose();

        public override string ToString()
        {
            System.Text.StringBuilder sb = new();
            bool[] Bits = this.Bits.ToArray();
            for (int i = Bits.Length; i > 0;)
                sb.Append(Bits[--i] ? '1' : '0');
            return sb.ToString();
        }
    }

    [AutoConstructor]
    public readonly partial struct CompareShader : IComputeShader
    {
        public readonly ReadWriteBuffer<bool> LeftBuffer;
        public readonly ReadWriteBuffer<bool> RightBuffer;
        public readonly ReadWriteBuffer<bool> ResultBuffer;
        public readonly Func<Bool2, bool> Function;

        public CompareShader(ReadWriteBuffer<bool> LeftBuffer, ReadWriteBuffer<bool> RightBuffer, Func<Bool2, bool> Function)
        {
            this.LeftBuffer = LeftBuffer;
            this.RightBuffer = RightBuffer;
            this.ResultBuffer = Gpu.Default.AllocateReadWriteBuffer<bool>(Math.Max(LeftBuffer.Length, RightBuffer.Length));
            this.Function = Function;
        }

        public void Execute()
        {
            ResultBuffer[ThreadIds.X] = Function(new Bool2(LeftBuffer[ThreadIds.X], RightBuffer[ThreadIds.X]));
        }

        [ShaderMethod]
        public static bool Or(Bool2 input) => input.X || input.Y;

        [ShaderMethod]
        public static bool Xor(Bool2 input) => input.X ^ input.Y;

        [ShaderMethod]
        public static bool And(Bool2 input) => input.X && input.Y;
    }
}
