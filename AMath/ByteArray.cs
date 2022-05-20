using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMath
{
    public struct ByteArray
    {
        private ulong[] data;
        private int Length => data.Count;
        public int ByteLength => Length * sizeof(ulong);
        public int BitLength => ByteLength << 3;
        private const int WordSize = sizeof(ulong) << 3;

        private ByteArray(ulong data) 
        {
            while (data.Count > 1 && data[^1] == 0)
                data = data[..^1];
            this.data = data;
        }

        public ByteArray(byte[] bytes)
        {
            data = new ulong[bytes.Length >> 3];
            Parallel.For(0, data.Length, i => data[i] = (ulong)bytes[(i << 3)..(i + 1 << 3)])
            // for (int i = 0; i < bytes.Length / sizeof(ulong); i++)
            // {
            //     ulong temp = 0;
            //     for (int t = 0; t < sizeof(ulong); t++)
            //         temp |= ((ulong)bytes[t]) << (t * sizeof(byte) * 8);
            //     data.Add(temp);
            // }
        }

        public static ByteArray And(ByteArray left, ByteArray right)
        {
            int Length = Math.Max(left.Length, right.Length);
            ulong[] data = new ulong[Length];
            
            Parallel.For(0, Length, i => data[i] = i < left.Length && i < right.Length ? left[i] & right[i] : 0);

            return new(data);
        }

        public static ByteArray Or(ByteArray left, ByteArray right)
        {
            int Length = Math.Max(left.Length, right.Length);
            ulong[] data = new ulong[Length];
            
            Parallel.For(0, Length, i => data[i] = i < left.Length && i < right.Length ? left[i] | right[i] : (i < left.Length ? left[i] : right[i]));

            return new(data);
        }

        public static ByteArray Xor(ByteArray left, ByteArray right)
        {
            int Length = Math.Max(left.Length, right.Length);
            ulong[] data = new ulong[Length];
            
            Parallel.For(0, Length, i => data[i] = i < left.Length && i < right.Length ? left[i] ^ right[i] : (i < left.Length ? left[i] : right[i]));

            return new(data);
        }
        
        public static ByteArray Not(ByteArray value)
        {
            ulong[] data = new ulong[value.Length];
            
            Parallel.For(0, value.Length, i => data[i] = ~value[i]);

            return new(data);
        }

        public static ByteArray ShiftLeft(ByteArray value, int offset)
        {
            int wordoffset = offset / WordSize;
            offset %= WordSize;
            ulong mask = ulong.MaxValue << WordSize - offset;

            ulong[] data = new ulong[value.Length + wordoffset + 1];

            Parallel.For(0, value.Length, i => {
                ulong temp = value[i] & mask,
                      word = value[i] & ~mask;

                data[i += wordoffset] = word << offset;
                data[i + 1] |= temp >> WordSize - offset;
            });

            return new(data);
        }

        public static ByteArray ShiftRight(ByteArray value, int offset)
        {
            int wordoffset = offset / WordSize;
            offset %= WordSize;
            ulong mask = ~(ulong.MaxValue << (WordSize - offset));

            ulong[] data = new ulong[value.Length + wordoffset];

            Parallel.For(0, data.Length, i => {
                ulong temp = value[i + wordoffset] & ~mask,
                      word = value[i + wordoffset] & mask;

                data[i] = word >> offset;
                if (i > 0) data[i - 1] |= temp << WordSize - offset;
            });

            return new(data);
        }

        public ulong this[int index] { get => data[index]; set => data[index] = value; }
        public ulong[] this[Range range] { get => data[range]; }

        public override string ToString() => ToString16();

        public string ToString2() =>
            string.Join(' ', data.Select(x => Convert.ToString(unchecked((long)x), 2).PadLeft(sizeof(long) * 8, '0')).Reverse()).TrimStart('0');

        public string ToString16() =>
            string.Join(' ', data.Select(x => Convert.ToString(unchecked((long)x), 16).PadLeft(sizeof(long) >> 1, '0')).Reverse()).TrimStart('0');
    }
}
diff --git a/AMath/Discontinued/ByteArray.cs b/AMath/Discontinued/ByteArray.cs
