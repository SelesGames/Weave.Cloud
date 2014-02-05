using System.Drawing;

namespace SelesGames.Drawing.Fill
{
    internal struct ColorAggregate
    {
        public int A { get; set; }
        public int R { get; set; }
        public int B { get; set; }
        public int G { get; set; }

        public static ColorAggregate operator +(ColorAggregate c1, Color c2)
        {
            c1.Add(c2.A, c2.R, c2.G, c2.B);
            return c1;
        }

        public static ColorAggregate operator +(ColorAggregate c1, ColorAggregate c2)
        {
            c1.Add(c2.A, c2.R, c2.G, c2.B);
            return c1;
        }

        public static ColorAggregate operator /(ColorAggregate c1, int div)
        {
            c1.Divide(div);
            return c1;
        }

        public void Add(int alpha, int red, int green, int blue)
        {
            A += alpha;
            R += red;
            G += green;
            B += blue;
        }

        public void Divide(int div)
        {
            A = A / div;
            R = R / div;
            G = G / div;
            B = B / div;
        }
    }
}
