using System.Numerics;
using System;
using System.Diagnostics;
using AMath;
using static AMath.BigMath;

namespace VSCODE
{
    class Program
    {
        static void Main(string[] args)
        {
            Precision = 64;
            SubPrecision = 16;

            LogTime(() => Console.WriteLine("e           = " + E));
            LogTime(() => Console.WriteLine("pi          = " + PI));
            LogTime(() => Console.WriteLine("sqrt(2)     = " + Sqrt(2m)));
            LogTime(() => Console.WriteLine("pow(2, 1/2) = " + Pow(2, 0.5m)));
            LogTime(() => Console.WriteLine("sin(pi / 4) = " + Sin(PI * 0.25m)));
        }

        public static void LogTime(Action action)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            action.Invoke();
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed.TotalMilliseconds + " ms");
        }
    }
}
