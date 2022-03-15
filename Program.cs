using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static AMath.BigMath;

namespace AMath
{
    class Program
    {
        static void Main(string[] args)
        {
            Precision = 64;
            SubPrecision = 16;


            Console.Title = "String evaluator";
            Console.Write(">");
            string s = Console.ReadLine();

            do {
                switch (s.ToLower())
                {
                    case "clear":
                        Console.Clear();
                        Console.Write(">");
                        continue;
                    case "precision":
                        Console.Write(">");
                        if (int.TryParse(Console.ReadLine(), out int p))
                            Precision = p;
                        Console.Write(">");
                        continue;
                    case "subprecision":
                        Console.Write(">");
                        if (int.TryParse(Console.ReadLine(), out int sp))
                            SubPrecision = sp;
                        Console.Write(">");
                        continue;
                }
                
                try { Console.WriteLine(" = " + Evaluate(s)); }
                catch (Exception e) { Console.WriteLine(e); }

                Console.Write("\nWrite new expression or \"exit\" to stop\n>");
            } while ((s = Console.ReadLine()).ToLower() != "exit");

            //PrintLog(100);
        }

        /// <summary>
        /// Returns the index of first brace and the index of its pair.
        /// </summary>
        /// <returns>
        /// If no braces (-1, -1),
        /// if no closing brace (index of first brace, -1),
        /// if found unexpected closing brace (-1, index of closing brace).
        /// </returns>
        public static (int, int) PairBracket(string input)
        {
            int start = input.IndexOfAny("({[".ToCharArray());
            if (start == -1) return (-1, input.IndexOfAny(")}]".ToCharArray()));

            Stack<int> stack = new Stack<int>();
            for (int i = start; i < input.Length; i++)
                if ("({[".Contains(input[i])) stack.Push("({[".IndexOf(input[i]));
                else if (")}]".Contains(input[i]))
                    if (input[i] != ")}]"[stack.Pop()]) return (-1, i);
                    else if (stack.Count == 0) return (start, i);

            return (start, -1);
        }

        /// <summary>
        /// Returns value of expression.
        /// </summary>
        public static BigDecimal Evaluate(string expression)
        {
            {
                int i = 0;
                foreach (Match m in Regex.Matches(expression, @"\d\("))
                    expression = expression.Insert(m.Index + ++i, "*");
                
                i = 0;
                foreach (Match m in Regex.Matches(expression, @"\)(\d|\()"))
                    expression = expression.Insert(m.Index + ++i, "*");

                i = 0;
                foreach (Match m in Regex.Matches(expression, @"[*/+\-^%]-"))
                    expression = expression.Insert(m.Index + ++i, "0");
            }

            int start, end;
            (start, end) = PairBracket(expression);
            while (start != -1)
            {
                if ((end = end > 0 ? end : expression.Length) == -1)
                    expression += ")";

                expression = expression.Remove(start) +
                    Evaluate(expression.Substring(start + 1, end - start - 1)).ToFullString() +
                    expression.Substring(end < expression.Length ? end + 1 : end);

                (start, end) = PairBracket(expression);
            }
            if (end != -1) throw 
                new Exception($"Unexpected close brace: '{expression[end]}', index={end}.");

            List<string> operands = new List<string>();
            while (expression.Length > 0)
            {
                string value = Regex.Match(expression, @"((\d+)?\.?\d+)|[*/+\-^%]|\w+").Value;
                operands.Add(value);
                expression = expression.Substring(value.Length);
            }

            // Console.Write("\n  > " + string.Join(' ', operands));

            for (int i = 0; i < operands.Count; i++)
            {
                if (operands[i].ToUpper() == "PI")
                    operands[i] = PI.ToFullString();
                else if (operands[i].ToUpper() == "E")
                    operands[i] = E.ToFullString();
            }

            string[][] opers = 
            {
                new string[] { "*", "/", "^", "!" },
                new string[] { "%", "^" },
                new string[] { "+", "-" }
            };
            for (int l = 0; l < 4; l++)
            {
                int operandsCount = operands.Count;
                for (int i = 1; i < operandsCount; i++)
                    foreach (string oper in opers[l])
                        if (operands[i] == oper)
                        {
                            operands[i] = EvaluateSimple(operands[i - 1], operands[i + 1], oper).ToFullString();
                            operands.RemoveAt(i - 1);
                            operands.RemoveAt(i--);
                            operandsCount -= 2;
                        }
            }

            if (operands.Count == 1) 
                return BigDecimal.Parse(operands[0]);
            else
                throw new Exception("Some error");
        }

        public static BigDecimal EvaluateSimple(string left, string right, string oper)
        {
            switch (oper)
            {
                case "*":
                    return BigDecimal.Parse(left) * BigDecimal.Parse(right);
                case "^":
                    return Pow(BigDecimal.Parse(left), BigDecimal.Parse(right));
                case "/":
                    return BigDecimal.Parse(left) / BigDecimal.Parse(right);
                case "%":
                    return BigDecimal.Parse(left) % BigDecimal.Parse(right);
                case "+":
                    return BigDecimal.Parse(left) + BigDecimal.Parse(right);
                case "-":
                    return BigDecimal.Parse(left) - BigDecimal.Parse(right);
                case "!":
                    return Factorial((BigInteger)BigDecimal.Parse(left));
            }
            throw new ArgumentException("Unable to evaluate '" + oper + "'.");
        }

        public static void PrintLog(int width)
        {
            string separator = "\n" + new string('─', width) + "\n";

            Console.WriteLine("\n" + new string(' ', (separator.Length - 7) >> 1) + "antyrtert\n" + separator);

            Precision = separator.Length - 47;

            // Log accuracy
            Console.WriteLine("Quality test\n");
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
            Console.WriteLine("Speed test");
            Console.WriteLine("PI                                        = " + PI);
            LogAverageTime(() => DoNothing(PI));
            Console.WriteLine("e                                         = " + E);
            LogAverageTime(() => DoNothing(E));
            Console.WriteLine("Sqrt(2)                                   = " + Sqrt(2m));
            LogAverageTime(() => DoNothing(Sqrt(2m)));
            Console.WriteLine("Pow(2, 1 / 3)                             = " + Pow(2m, (BigDecimal)1 / 3));
            LogAverageTime(() => DoNothing(Pow(2m, (BigDecimal)1 / 3)));
            Console.WriteLine("Sin(PI / 3)                               = " + Sin(PI / 3));
            LogAverageTime(() => DoNothing(Sin(PI / 3)));
            Console.WriteLine("Cos(PI / 3)                               = " + Cos(PI / 3));
            LogAverageTime(() => DoNothing(Cos(PI / 3)));
            Console.WriteLine("Tg(PI / 6)                                = " + Tg(PI / 6));
            LogAverageTime(() => DoNothing(Tg(PI / 6)));
            Console.WriteLine("Ctg(PI / 6)                               = " + Ctg(PI / 6));
            LogAverageTime(() => DoNothing(Ctg(PI / 6)));
            Console.WriteLine("ArcSin(0.5)                               = " + ArcSin(0.5m));
            LogAverageTime(() => DoNothing(ArcSin(0.5m)));
            Console.WriteLine("ArcCos(0.5)                               = " + ArcCos(0.5m));
            LogAverageTime(() => DoNothing(ArcCos(0.5m)));
            Console.WriteLine("Log(0.5, 2)                               = " + Log(0.5m, 2));
            LogAverageTime(() => DoNothing(Log(0.5m, 2)));
            Console.WriteLine("Lg(0.5)                                   = " + Lg(0.5m));
            LogAverageTime(() => DoNothing(Lg(0.5m)));
            Console.WriteLine("Ln(0.5)                                   = " + Ln(0.5m));
            LogAverageTime(() => DoNothing(Ln(0.5m)));
            LogTime(() => Console.WriteLine("GetPrimes(1000000)                        = " + GetPrimes(1000000).Length));

            Console.WriteLine(separator);
        }

        private static void DoNothing(object obj) { }
        
        public static void LogAverageTime(Action action, int iterations = 1000)
        {
            double time = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
               stopwatch.Restart();
               action.Invoke();
               time += stopwatch.Elapsed.TotalMilliseconds;
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