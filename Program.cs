using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Diagnostics;
using static AMath.BigMath;

namespace AMath
{
    class Program
    {
        static void Main(string[] args)
        {
            Precision = 64;
            SubPrecision = 16;


            Console.WriteLine(PI);

            // PrintLog(100);
        }
        
        public static void DoNothing(BigDecimal value)
        {
            
        }

        public static void PrintLog(int width)
        {
            string separator = "\n" + new string('─', width) + "\n";

            Console.WriteLine("\n" + new string(' ', (separator.Length - 7) >> 1) + "antyrtert\n" + separator);

            Precision = separator.Length - 47;

            // Log accuracy
            Console.WriteLine("e                                         = " + E);
            Console.WriteLine("PI                                        = " + PI);
            Console.WriteLine("Pow(Sqrt(3), 2)                           = " + Pow(Sqrt(3m), 2m));
            Console.WriteLine("Pow(Pow(3, 1 / 3), 3)                     = " + Pow(Pow(3m, (BigDecimal)1 / 3), 3m));
            Console.WriteLine("ArcSin(Sin(PI / 3))                       = " + ArcSin(Sin(PI / 3)));
            Console.WriteLine("ArcCos(Cos(PI / 3))                       = " + ArcCos(Cos(PI / 3)));
            Console.WriteLine("Pow(Sin(PI / 3), 2) + Pow(Cos(PI / 3), 2) = " + (Pow(Sin(PI / 3), 2m) + Pow(Cos(PI / 3), 2m)));
            Console.WriteLine("Tg(Pi / 3) * Ctg(PI / 3)                  = " + Tg(PI / 3) * Ctg(PI / 3));
            Console.WriteLine("Pow(3, Log(27, 3))                        = " + Pow(3m, Log(27m, 3m)));
            Console.WriteLine("Pow(10, Lg(1000)                          = " + Pow(10m, Lg(1000m)));
            Console.WriteLine("Ln(Exp(3))                                = " + Ln(Exp(3)));

            Console.WriteLine(separator);

            // Log speed
            Console.WriteLine("PI                                        = " + PI); // PI
            LogAverageTime(() => DoNothing(PI));
            Console.WriteLine("e                                         = " + E);  // E
            LogAverageTime(() => DoNothing(E));
            Console.WriteLine("Sqrt(2)                                   = " + Sqrt(2m)); // Sqrt(2)
            LogAverageTime(() => DoNothing(Sqrt(2m)));
            Console.WriteLine("Pow(2, 1/3)                               = " + Pow(2m, (BigDecimal)1 / 3)); // Pow(2, 1/3)
            LogAverageTime(() => DoNothing(Pow(2m, (BigDecimal)1 / 3)));
            Console.WriteLine("Sin(PI / 3)                               = " + Sin(PI / 3)); // Sin(PI / 3)
            LogAverageTime(() => DoNothing(Sin(PI / 3)));
            Console.WriteLine("Cos(PI / 3)                               = " + Cos(PI / 3)); // Cos(PI / 3)
            LogAverageTime(() => DoNothing(Cos(PI / 3)));
            Console.WriteLine("Tg(PI / 6)                                = " + Tg(PI / 6)); // Tg(PI / 6)
            LogAverageTime(() => DoNothing(Tg(PI / 6)));
            Console.WriteLine("Ctg(PI / 6)                               = " + Ctg(PI / 6)); // Ctg(PI / 6)
            LogAverageTime(() => DoNothing(Ctg(PI / 6)));
            Console.WriteLine("ArcSin(0.5)                               = " + ArcSin(0.5m)); // ArcSin(0.5)
            LogAverageTime(() => DoNothing(ArcSin(0.5m)));
            Console.WriteLine("ArcCos(0.5)                               = " + ArcCos(0.5m)); // ArcCos(0.5)
            LogAverageTime(() => DoNothing(ArcCos(0.5m)));
            Console.WriteLine("Log[2](0.5)                               = " + Log(0.5m, 2)); // Log[2](0.5)
            LogAverageTime(() => DoNothing(Log(0.5m, 2)));
            Console.WriteLine("Lg(0.5)                                   = " + Lg(0.5m)); // Lg(0.5)
            LogAverageTime(() => DoNothing(Lg(0.5m)));
            Console.WriteLine("Ln(0.5)                                   = " + Ln(0.5m)); // Ln(0.5)
            LogAverageTime(() => DoNothing(Ln(0.5m)));
            LogTime(() => Console.WriteLine("GetPrimes(1000000)          = " + GetPrimes(1000000).Length));

            Console.WriteLine(separator);
        }
        
        public static void LogAverageTime(Action action, int iterations = 1000)
        {
            double time = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
               action.Invoke();
               time += stopwatch.Elapsed.TotalMilliseconds;
               stopwatch.Restart();
            }
            Console.WriteLine("└─ average time: " + (time / iterations).ToString("0.00000").PadLeft(9) + " ms");
        }

        public static void LogTime(Action action)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            action.Invoke();
            stopwatch.Stop();
            Console.WriteLine("└─ elapsed time: " + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00000").PadLeft(9) + " ms");
        }
    }
}