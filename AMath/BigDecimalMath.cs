using System;
using System.Numerics;

namespace AMath
{
    public partial struct BigDecimal
    {
        public static int Precision { get; private set; } = 32;
        public static int SubPrecision { get; private set; } = 16;
        public static int TotalPrecision => Precision + SubPrecision;
        public static readonly BigDecimal Zero = 0, One = 1, MinusOne = -1, Half = 0.5m, Quarter = 0.25m;
        private static readonly BigDecimal Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Ten = 10;
        public static bool TrimEndZeros { get; set; } = false;

        /// <summary> BigDecimal is not infinite accuracy data type so it's need limit. </summary>
        /// <param name="Precision"> Number of visible decimal places after decimal sign. </param>
        /// <param name="SubPrecision"> Number of invisible decimal places after decimal sign. </param>
        public static void SetPrecision(int Precision, int SubPrecision = 16)
        {
            BigDecimal.Precision = Precision;
            BigDecimal.SubPrecision = SubPrecision;
            pi = Zero;
            e = Zero;
            gr = Zero;
        }

        /// <summary> Math constant π ≈ 3.14. Half turn on circle. </summary>
        public static BigDecimal PI => pi.Equals(Zero) ? pi = ComputePI() : pi;
        private static BigDecimal pi = Zero;

        /// <summary> Math constant τ ≈ 6.28. Full turn on circle </summary>
        public static BigDecimal Tau => Two * PI;

        /// <summary> Math constant e ≈ 2.72. Euler's number. </summary>
        public static BigDecimal E => e.Equals(Zero) ? e = Exp(One) : e;
        private static BigDecimal e = Zero;

        /// <summary> Math constant φ ≈ 1.62. Root of x² - x - 1 = 0 </summary>
        public static BigDecimal GoldenRatio => gr.Equals(Zero) ? gr = Sqrt(Five) * Half + Half : gr;
        private static BigDecimal gr = Zero;

        private static BigDecimal ComputePI()
        {   // Gauss-Legendre algorithm
            BigDecimal a = One, b = Sqrt(Half), t = Quarter, p = One, n;

            do {
                a = ((n = a) + b) * Half;
                b = Sqrt(n * b);
                t -= p * (n -= a) * n;
                p *= Two;
            } while (a != b);

            return a * b / t;
        }

        #region Exponental functions
        /// <summary> Integer exponentation function. </summary>
        /// <returns> <paramref name="value"/>^<paramref name="exponent"/>. </returns>
        private static BigDecimal Pow(BigDecimal value, int exponent)
        {
            bool negative = exponent < 0;
            exponent = Math.Abs(exponent);
            BigDecimal result = One;
            while (exponent > 0)
                if (((exponent >>= 1) & 1) == 1)
                    result *= value *= value;
                else result *= value;
            return negative ? One / result : result;
        }

        /// <summary> Exponentation function. </summary>
        /// <returns> <paramref name="value"/>^<paramref name="exponent"/>. </returns>
        public static BigDecimal Pow(BigDecimal value, BigDecimal exponent) =>
            Exp(Ln(value) * exponent);

        /// <summary> Exponental function e^x. </summary>
        /// <returns> e^<paramref name="exponent"/>. </returns>
        public static BigDecimal Exp(BigDecimal exponent)
        {
            BigDecimal delta, result = One, pow = One, fac = One, i = One;

            do result += delta = (pow *= exponent) / (fac *= i++);
            while (delta != Zero);

            return result;
        }

        /// <returns> Square root of <paramref name="value"/>. </returns>
        public static BigDecimal Sqrt(BigDecimal value) =>
            new(AMath.Sqrt(value.value * BigInteger.Pow(10, TotalPrecision * 2 + (value.scale & 1))), (value.scale + 1 >> 1) + TotalPrecision);
        #endregion

        #region Logarithm functions
        private static int Log2(BigInteger value)
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

        /// <summary> Logarithm function. </summary>
        /// <returns> Log_<paramref name="Base"/> <paramref name="value"/>. </returns>
        public static BigDecimal Log(BigDecimal value, BigDecimal Base) => Ln(value) / Ln(Base);

        /// <summary> Decimal logarithm function. </summary>
        /// <returns> Log_10 <paramref name="value"/>. </returns>
        public static BigDecimal Lg(BigDecimal value) => Ln(value) / Ln(10);

        /// <summary> Natural logarithm function. </summary>
        /// <returns> Log_e <paramref name="value"/>. </returns>
        public static BigDecimal Ln(BigDecimal value)
        {
            int log2 = Log2((BigInteger)value) - 1;
            if (log2 > 1) return log2 * LogE(Two) + LogE(value / ((BigInteger)1 << log2));
            return LogE(value);
        }
        
        /// <summary> Natural logarithm function, but slow at large <paramref name="value"/>. </summary>
        /// <returns> Log_e <paramref name="value"/>. </returns>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="value"/> must be greater than zero. </exception>
        private static BigDecimal LogE(BigDecimal value)
        {
            if (value <= Zero) throw new ArgumentOutOfRangeException(nameof(value), "must be greater than zero");
            BigDecimal z = (value - One) / (value + One), z2 = z * z, result = z, delta, i = One;

            do result += delta = (z *= z2) / (i += Two);
            while (delta != Zero);

            return Two * result;
        }
        #endregion

        #region Trigonometry functions
        /// <summary> Sinusoid function. </summary>
        /// <param name="radians"> Angle in radians. </param>
        /// <returns> Sine of specified angle. </returns>
        public static BigDecimal Sin(BigDecimal radians)
        {
            BigDecimal result = radians %= PI * Half, prev = Zero, i = One, pow = radians, fac = One;
            radians *= radians;

            while (result != prev)
            {
                prev = result;
                result -= (pow *= radians) / (fac *= ++i * ++i);
                result += (pow *= radians) / (fac *= ++i * ++i);
            }

            return result;
        }

        /// <summary> Cosinusoid function. </summary>
        /// <param name="radians"> Angle in radians. </param>
        /// <returns> Cosine of specified angle. </returns>
        public static BigDecimal Cos(BigDecimal radians)
        {
            BigDecimal result = One, prev = Zero, i = Zero, pow = One, fac = One;
            radians *= radians %= PI * Half;

            while (result != prev)
            {
                prev = result;
                result -= (pow *= radians) / (fac *= ++i * ++i);
                result += (pow *= radians) / (fac *= ++i * ++i);
            }

            return result;
        }

        /// <summary> Tangent function. </summary>
        /// <param name="radians"> Angle in radians. </param>
        /// <returns> Tangent of specified angle. </returns>
        public static BigDecimal Tg(BigDecimal radians) =>
            (radians %= PI) != PI * Half ? Sin(radians) / Cos(radians) : throw new DivideByZeroException();

        /// <summary> Cotangent function. </summary>
        /// <param name="radians"> Angle in radians. </param>
        /// <returns> Cotangent of specified angle. </returns>
        public static BigDecimal Ctg(BigDecimal radians) =>
            (radians %= PI) != PI ? Cos(radians) / Sin(radians) : throw new DivideByZeroException();

        [Obsolete("This function is very slow at values greater than 1/√2.")]
        public static BigDecimal ArcSin(BigDecimal value)
        {
            if (Abs(value) > One) throw new ArgumentOutOfRangeException(nameof(value));

            BigDecimal result = value, delta, val = value * value;
            BigInteger pow4 = BigInteger.One, fac1 = BigInteger.One, fac2 = BigInteger.One, i = BigInteger.Zero;

            do result += delta = (fac1 *= (++i << 1) * ((i << 1) - 1)) * (value *= val) /
                                 ((fac2 *= i) * fac2 * ((i << 1) + 1) * (pow4 <<= 2));
            while (delta != Zero);

            return result;
        }

        [Obsolete("This function is very slow at values lower than 1/√2.")]
        public static BigDecimal ArcCos(BigDecimal value) =>
            PI * Half - ArcSin(value);
        #endregion

        /// <summary> Absolute function. </summary>
        /// <returns> |<paramref name="value"/>|. </returns>
        public static BigDecimal Abs(BigDecimal value) => 
            new(BigInteger.Abs(value.value), value.scale);

        /// <summary> Round to lower. </summary>
        /// <returns> └ <paramref name="value"/> ┘. </returns>
        public static BigDecimal Truncate(BigDecimal value, int digits = 0) =>
            value.scale > digits ? new(value.value / BigInteger.Pow(10, value.scale - digits), digits) : value;
        
        /// <summary> Round to nearest. </summary>
        public static BigDecimal Round(BigDecimal value, int digits = 0) =>
            value.scale > digits ? Truncate(value + new BigDecimal(5, digits + 1), digits) : value;

        /// <summary> Arithmetic average. </summary>
        public static BigDecimal Average(params BigDecimal[] array)
        {
            BigDecimal average = array[0];
            for (int i = 1; i < array.Length; i++)
                average += array[i];
            return average / array.Length;
        }

        /// <returns> Maximum value of <paramref name="array"/> </returns>
        public static BigDecimal Max(params BigDecimal[] array)
        {
            int max = 0;
            for (int i = 0; i < array.Length; i++)
                if (array[i] > array[max]) max = i;
            return array[max];
        }
        
        /// <returns> Minimal value of <paramref name="array"/> </returns>
        public static BigDecimal Min(params BigDecimal[] array)
        {
            int min = 0;
            for (int i = 0; i < array.Length; i++)
                if (array[i] < array[min]) min = i;
            return array[min];
        }
    }
}
