using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AMath
{
    public partial struct BigRational
    {
        public static readonly BigRational One = new(1, 1), Half = new(1, 2), Quarter = new(1, 4), Zero = new(0, 1);

        // private static BigRational pi;
        public static BigRational PI => ComputePI(); // { get => pi; private set => pi = value; }

        private static int precision = 32;
        public static int Precision { get => precision; set => precision = Math.Max(value, 0); }

        public static BigRational Sqrt(BigRational value) =>
            new(AMath.Sqrt(value.numerator * BigInteger.Pow(100, precision)), AMath.Sqrt(value.denominator * BigInteger.Pow(100, precision)));

        private static BigRational ComputePI()
        {
            BigRational a = One, b = Sqrt(Half), t = Quarter, p = One, n;

            for (int i = 2; i <= precision; i <<= 1)
            {
                a = ((n = a) + b) * Half;
                b = Sqrt(n * b);
                t -= p * (n -= a) * n;
                p *= 2;
            }

            return a * b / t;
        }
    }
}
