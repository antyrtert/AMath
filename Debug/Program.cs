using System;
using System.Diagnostics;

using AMath;

namespace Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: BigNatural, BigInteger, -BigRational-, --BigDecimal--, BigFloat, ?BigReal, BigComplex, BigQuaternion
            //       Expression evaluator, Math renderer, ?Expression solver
            //       Approximate evaluation               Symbolic evaluation

            // BigRational.Precision = 512;
            // Console.WriteLine(BigRational.PI);
            BigDecimalLog();
            //BigDecimalLog();
        }

        static void BigDecimalLog()
        {
            Console.WindowWidth = 122;
            BigDecimal.SetPrecision(116);

            Console.WriteLine("Constants:");
            Console.WriteLine($"π = {BigDecimal.PI}");
            Console.WriteLine($"e = {BigDecimal.E}");
            Console.WriteLine($"φ = {BigDecimal.GoldenRatio}");

            BigDecimal.SetPrecision(88);
            BigDecimal x = BigDecimal.Exp(BigDecimal.E);

            Console.WriteLine($"\nFunctions:");
            Console.WriteLine($"x              = {x}");
            
            Console.WriteLine($"Function name  |                                           Value                                           | Average time");
            Console.WriteLine($"Ln(x)          | {BigDecimal.Ln(x).ToString().PadRight(2, '.').PadRight(90, '0').Remove(89),89} | {MeasureTime(() => BigDecimal.Ln(x)).TotalMilliseconds,10:0.000}ms");
            Console.WriteLine($"Ln(Ln(x))      | {BigDecimal.Ln(BigDecimal.Ln(x)).ToString().PadRight(2, '.').PadRight(90, '0').Remove(89),89} | {MeasureTime(() => BigDecimal.Ln(BigDecimal.Ln(x))).TotalMilliseconds,10:0.000}ms");
            Console.WriteLine($"Exp(x)         | {BigDecimal.Exp(x).ToString().PadRight(2, '.').PadRight(90, '0').Remove(89),89} | {MeasureTime(() => BigDecimal.Exp(x)).TotalMilliseconds,10:0.000}ms");
            Console.WriteLine($"Pow(x, x)      | {BigDecimal.Pow(x, x).ToString().PadRight(2, '.').PadRight(90, '0').Remove(89),89} | {MeasureTime(() => BigDecimal.Pow(x, x)).TotalMilliseconds,10:0.000}ms");
            
            x = BigDecimal.PI / 4;
            Console.WriteLine($"\nx              = {x}");

            Console.WriteLine($"Function name  |                                           Value                                           | Average time");
            Console.WriteLine($"Sin(x)         | {BigDecimal.Sin(x).ToString().PadRight(2, '.').PadRight(90, '0').Remove(89),89} | {MeasureTime(() => BigDecimal.Sin(x)).TotalMilliseconds,10:0.000}ms");
            Console.WriteLine($"Cos(x)         | {BigDecimal.Cos(x).ToString().PadRight(2, '.').PadRight(90, '0').Remove(89),89} | {MeasureTime(() => BigDecimal.Cos(x)).TotalMilliseconds,10:0.000}ms");
            Console.WriteLine($"Tg(x)          | {BigDecimal.Tg(x).ToString().PadRight(2, '.').PadRight(90, '0').Remove(89),89} | {MeasureTime(() => BigDecimal.Tg(x)).TotalMilliseconds,10:0.000}ms");
            Console.WriteLine($"Ctg(x)         | {BigDecimal.Ctg(x).ToString().PadRight(2, '.').PadRight(90, '0').Remove(89),89} | {MeasureTime(() => BigDecimal.Ctg(x)).TotalMilliseconds,10:0.000}ms");
            Console.WriteLine($"Arcsin(sin(x)) | {BigDecimal.ArcSin(BigDecimal.Sin(x)).ToString().PadRight(2, '.').PadRight(90, '0').Remove(89),89} | {MeasureTime(() => BigDecimal.ArcSin(BigDecimal.Sin(x))).TotalMilliseconds,10:0.000}ms");
            Console.WriteLine($"Arccos(cos(x)) | {BigDecimal.ArcCos(BigDecimal.Cos(x)).ToString().PadRight(2, '.').PadRight(90, '0').Remove(89),89} | {MeasureTime(() => BigDecimal.ArcCos(BigDecimal.Cos(x))).TotalMilliseconds,10:0.000}ms");
            
            decimal[] Decimals = new decimal[7] 
            {
                decimal.MinusOne,
                decimal.Zero,
                decimal.One,
                decimal.MinValue,
                decimal.MaxValue,
                new decimal(1, 0, 0, true, 28),
                new decimal(1, 0, 0, false, 28)
            };

            BigDecimal.SetPrecision(32);
            Console.WriteLine($"\n{"Convertation:",-15}| {typeof(decimal),-32} |  {typeof(BigDecimal),-32} |  {typeof(decimal),-32}");
            foreach (decimal d in Decimals) Console.WriteLine(BigDecimalConvertLog(d));

            double[] Doubles = new double[5]
            {
                -1d, 0d, 1d,
                // double.MinValue,  // No support for scientific notation yet
                // double.MaxValue,  // Too long values
                -double.Epsilon,
                double.Epsilon
            };

            Console.WriteLine($"\n{"Convertation:",-15}| {typeof(double),-32} |  {typeof(BigDecimal),-32} |  {typeof(double),-32}");
            foreach (double d in Doubles) Console.WriteLine(BigDecimalConvertLog(d));
        }

        static string BigDecimalConvertLog(object d) => d switch
        {
            double => $"{typeof(double),-15}| {d,32} |  {new BigDecimal((double)d),32} |  {BigDecimal.ToDouble((double)d),32}",
            decimal => $"{typeof(decimal),-15}| {d,32} |  {new BigDecimal((decimal)d),32} |  {BigDecimal.ToDecimal((decimal)d),32}",
            _ => "Unsupported"
        };

        static TimeSpan MeasureTime(Action action, int samples = 1000)
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < samples; i++)
                action.Invoke();
            return sw.Elapsed / samples;
        }

        static void ClearConsole()
        {
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);
            Console.Write(new string(' ', Console.WindowHeight * Console.WindowWidth));
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = true;
        }
    }
}
