using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AMath
{
    [Serializable]
    public struct Color
    {
        public double a, r, g, b;
        private const double byteToDouble = 1d / byte.MaxValue;

        public Color(double a, double r, double g, double b)
        {
            this.a = a;
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public Color(byte a, byte r, byte g, byte b)
        {
            this.a = a * byteToDouble;
            this.r = r * byteToDouble;
            this.g = g * byteToDouble;
            this.b = b * byteToDouble;
        }

        public byte A => ClampToByte(a);
        public byte R => ClampToByte(r);
        public byte G => ClampToByte(g);
        public byte B => ClampToByte(b);

        public double H => GetHue();
        public double S => Max(r, g, b) == 0 ? 0 : 1d - Min(r, g, b) / Max(r, g, b);
        public double V => Max(r, g, b);
        public double Luma => r * 0.2627 + g * 0.6780 + b * 0.0593;

        public static Color FromInt32(uint val)
        {
            byte A = (byte)(val >> 24);
            byte R = (byte)((val & 0xFF0000) >> 16);
            byte G = (byte)((val & 0xFF00) >> 8);
            byte B = (byte)(val & 0xFF);

            return FromArgb(A, R, G, B);
        }

        public static Color FromInt16(ushort val)
        {
            byte A = (byte)(val >> 12);
            byte R = (byte)((val & 0xF00) >> 8);
            byte G = (byte)((val & 0xF0) >> 4);
            byte B = (byte)(val & 0xF);

            A = (byte)((A << 4) + A);
            R = (byte)((R << 4) + R);
            G = (byte)((G << 4) + G);
            B = (byte)((B << 4) + B);

            return FromArgb(A, R, G, B);
        }

        public static byte HexToByte(string s) => s.Length switch
        {
            1 => HexToByte(s[0]),
            2 => HexToByte(s[0], s[1]),
            _ => 0
        };

        public static byte HexToByte(char a, char b) =>
            (byte)((HexToByte(a) << 4) | HexToByte(b));

        public static byte HexToByte(char c) => (byte)(
              c >= '0' && c <= '9' ? c - '0'
            : c >= 'A' && c <= 'F' ? c - 'A' + 10
            : c >= 'a' && c <= 'f' ? c - 'a' + 10 : 0x10);

        public static readonly Regex Hex = new(@"^#?([a-fA-F0-9]{8}|[a-fA-F0-9]{6}|[a-fA-F0-9]{4}|[a-fA-F0-9]{3})$");
        public static Color FromHex(string hex)
        {
            if (Hex.IsMatch(hex))
                return (hex = hex.Replace("#", string.Empty)).Length switch
                {
                    3 => FromRgb(HexToByte(hex[0], hex[0]),
                                 HexToByte(hex[1], hex[1]),
                                 HexToByte(hex[2], hex[2])),

                    4 => FromArgb(HexToByte(hex[0], hex[0]),
                                  HexToByte(hex[1], hex[1]),
                                  HexToByte(hex[2], hex[2]),
                                  HexToByte(hex[3], hex[3])),

                    6 => FromRgb(HexToByte(hex[0..2]),
                                 HexToByte(hex[2..4]),
                                 HexToByte(hex[4..6])),

                    8 => FromArgb(HexToByte(hex[0..2]),
                                  HexToByte(hex[2..4]),
                                  HexToByte(hex[4..6]),
                                  HexToByte(hex[6..8])),

                    _ => new()
                };
            return new();
        }

        public static Color FromRgb(double R, double G, double B) => new(1, R, G, B);
        public static Color FromArgb(double A, double R, double G, double B) => new(A, R, G, B);

        public static Color FromRgb(byte R, byte G, byte B) => new(255, R, G, B);
        public static Color FromArgb(byte A, byte R, byte G, byte B) => new(A, R, G, B);

        public static Color FromHSV(double Hue, double Saturation, double Value, double A = 1)
        {
            double C = (Value = Clamp(Value)) * Clamp(Saturation),
                   H = Hue % 360d / 60d,
                   X = C * (1d - Math.Abs(H % 2 - 1)),
                   m = Value - C + 0.5 * byteToDouble;

            C = Clamp(C + m);
            X = Clamp(X + m);

            return H switch
            {
                <= 1 => new(A, C, X, m),
                <= 2 => new(A, X, C, m),
                <= 3 => new(A, m, C, X),
                <= 4 => new(A, m, X, C),
                <= 5 => new(A, X, m, C),
                <= 6 => new(A, C, m, X),
                _ => new(A, m, m, m)
            };
        }

        public double GetHue()
        {
            double max = Max(r, g, b),
                   min = Min(r, g, b),
                   delta = 60d / (max - min);

            if (max == min) return 0;
            if (max == r) return (g - b) * delta + (g < b ? 360 : 0);
            if (max == g) return (b - r) * delta + 120;
            if (max == b) return (r - g) * delta + 240;

            return 0;
        }

        private static double Max(params double[] value) => value.Max();
        private static double Min(params double[] value) => value.Min();
        private static double Clamp(double value, double min = 0, double max = 1) => Min(Max(value, min), max);
        private static byte ClampToByte(double value) => (byte)Clamp(value * 255, 0, 255);

        public override int GetHashCode() => HashCode.Combine(a, r, g, b, nameof(Color));
        public override bool Equals(object obj) => obj is Color color && color.a == a && color.r == r && color.g == g && color.b == b;
        public override string ToString() => $"#{Convert.ToString((A << 24) + (R << 16) + (G << 8) + B, 16).ToUpper().PadLeft(8, '0')}";

        public static bool operator ==(Color left, Color right) => left.Equals(right);
        public static bool operator !=(Color left, Color right) => !left.Equals(right);
    }
}