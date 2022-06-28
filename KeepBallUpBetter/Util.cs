using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeepBallUpBetter
{
    internal static class Util
    {
        public static bool Collides(float x1, float y1, float w1, float h1, float x2, float y2, float w2, float h2)
        {
            return x1 < x2 + w2 && x1 + w1 > x2 && y1 < y2 + h2 && y1 + h1 > y2;
        }

        public static bool Collides(Vector2f pos1, Vector2f size1, Vector2f pos2, Vector2f size2)
        {
            return Collides(pos1.X, pos1.Y, size1.X, size1.Y, pos2.X, pos2.Y, size2.X, size2.Y);
        }

        public static bool Collides(Rect box1, Rect box2)
        {
            return Collides(box1.Position, box1.Size, box2.Position, box2.Size);
        }

        public static Vector2f? LineIntersection(Vector2f line1Start, Vector2f line1End, Vector2f line2Start, Vector2f line2End)
        {
            return LineIntersection(new Line(line1Start, line1End), new Line(line2Start, line2End));
        }

        public static Vector2f? LineIntersection(Line line1, Line line2)
        {
            if (!Collides(line1.BoundingBox(), line2.BoundingBox()))
                return null;

            var denominator = (line2.End.Y - line2.Start.Y) * (line1.End.X - line1.Start.X) - (line2.End.X - line2.Start.X) * (line1.End.Y - line1.Start.Y);

            if (denominator == 0.0f)
                return null;

            var a = (line2.End.X - line2.Start.X) * (line1.Start.Y - line2.Start.Y) - (line2.End.Y - line2.Start.Y) * (line1.Start.X - line2.Start.X);
            var b = (line1.End.X - line1.Start.X) * (line1.Start.Y - line2.Start.Y) - (line1.End.Y - line1.Start.Y) * (line1.Start.X - line2.Start.X);

            a /= denominator;
            b /= denominator;

            if (a >= 0.0f && a <= 1.0f && b >= 0.0f && b <= 1.0f)
                return new Vector2f(line1.Start.X + a * (line1.End.X - line1.Start.X), line1.Start.Y + a * (line1.End.Y - line1.Start.Y));
            else
                return null;
        }
    }

    internal struct Line
    {
        public Vector2f Start;
        public Vector2f End;

        public bool PointedRight { get { return End.X - Start.X >= 0.0f; } }
        public bool PointedDown { get { return End.Y - Start.Y >= 0.0f; } }

        public Line(Vector2f start, Vector2f end)
        {
            Start = start;
            End = end;
        }

        public Rect BoundingBox()
        {
            var x = Math.Min(Start.X, End.X);
            var y = Math.Min(Start.Y, End.Y);
            var w = Math.Max(Start.X, End.X) - x;
            var h = Math.Max(Start.Y, End.Y) - y;

            return new Rect(x, y, w, h);
        }

        public Line Normalize()
        {
            var len = (float)Math.Sqrt(Math.Pow(End.X - Start.X, 2) + Math.Pow(End.Y - Start.Y, 2));
            return new Line(Start / len, End / len);
        }

        public Vector2f Direction()
        {
            var direction = End - Start;
            var len = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
            return direction / len;
        }

        public static Line operator +(Line l, float x)
        {
            var identityVector = new Vector2f(1.0f, 1.0f);
            return new Line(l.Start + identityVector * x, l.End + identityVector * x);
        }

        public static Line operator +(Line l, Vector2f v)
        {
            return new Line(l.Start + v, l.End + v);
        }

        public static Line operator *(Line l, float x)
        {
            var newEnd = (l.End - l.Start) * x;
            return new Line(l.Start, l.End + newEnd);
        }
    }

    internal struct Rect
    {
        public Vector2f Position;
        public Vector2f Size;

        public Rect(float x, float y, float w, float h) : this(new Vector2f(x, y), new Vector2f(w, h)) { }

        public Rect(Vector2f position, Vector2f size)
        {
            Position = position;
            Size = size;
        }

        public static Rect operator +(Rect r, Vector2f v)
        {
            return new Rect(r.Position + v, r.Size);
        }
    }
}
