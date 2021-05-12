using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using static AMath.BigMath;

namespace AMath
{
    class Program
    {
        static void Main(string[] args)
        {
            Precision = Console.WindowWidth / 2 - 2;
            SubPrecision = 16;
            
            Console.WriteLine("\n" + new string(' ', (Console.WindowWidth - 9) / 2) + "antyrtert\n");
            
            Console.WriteLine("PI          = " + PI); 
            LogAverageTime(() => DoNothing(PI));
            Console.WriteLine("e           = " + E);
            LogAverageTime(() => DoNothing(E));
            Console.WriteLine("Sqrt(2)     = " + Sqrt(2m));
            LogAverageTime(() => DoNothing(Sqrt(2m)));
            Console.WriteLine("Pow(2, 1/3) = " + Pow(2m, (BigDecimal)1 / 3)); 
            LogAverageTime(() => DoNothing(Pow(2m, (BigDecimal)1 / 3)));
            Console.WriteLine("Sin(PI / 3) = " + Sin(PI / 3)); 
            LogAverageTime(() => DoNothing(Sin(PI / 3)));
            Console.WriteLine("Cos(PI / 4) = " + Cos(PI / 4)); 
            LogAverageTime(() => DoNothing(Cos(PI / 3)));

            //LogTime(() => Console.WriteLine("e           = " + E));
            //LogTime(() => Console.WriteLine("PI          = " + PI));
            //LogTime(() => Console.WriteLine("Sqrt(2)     = " + Sqrt(2m)));
            //LogTime(() => Console.WriteLine("Pow(2, 1/2) = " + Pow(2, 0.5m)));
            //LogTime(() => Console.WriteLine("Sin(PI / 4) = " + Sin(PI * 0.25m)));
        }
        
        public static void DoNothing(BigDecimal value)
        {
            
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
            Console.WriteLine("└─ average time: " + (time / iterations).ToString("0.00000") + " ms");
        }

        public static void LogTime(Action action)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            action.Invoke();
            stopwatch.Stop();
            Console.WriteLine("└─ elapsed time: " + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00000") + " ms");
        }
    }
}