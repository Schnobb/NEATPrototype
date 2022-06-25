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
    public static class Util
    {
        public static bool Collides(float x1, float y1, float w1, float h1, float x2, float y2, float w2, float h2)
        {
            return x1 < x2 + w2 && x1 + w1 > x2 && y1 < y2 + h2 && y1 + h1 > y2;
        }

        public static bool Collides(Vector2f pos1, Vector2f size1, Vector2f pos2, Vector2f size2)
        {
            return pos1.X < pos2.X + size2.X && pos1.X + size1.X > pos2.X && pos1.Y < pos2.Y + size2.Y && pos1.Y + size1.Y > pos2.Y;
        }

        public static Vector2f? lineIntersection(Vector2f line1Start, Vector2f line1End, Vector2f line2Start, Vector2f line2End)
        {
            //var denominator = (line1Start.X - line1End.X) * (line2Start.Y - line2End.Y) - (line1Start.Y - line1End.Y) * (line2Start.X - line2End.X);
            var denominator = (line2End.Y - line2Start.Y) * (line1End.X - line1Start.X) - (line2End.X - line2Start.X) * (line1End.Y - line1Start.Y);

            if (denominator == 0.0f)
                return null;

            var a = (line2End.X - line2Start.X) * (line1Start.Y - line2Start.Y) - (line2End.Y - line2Start.Y) * (line1Start.X - line2Start.X);
            var b = (line1End.X - line1Start.X) * (line1Start.Y - line2Start.Y) - (line1End.Y - line1Start.Y) * (line1Start.X - line2Start.X);

            a /= denominator;
            b /= denominator;

            if (a >= 0.0f && a <= 1.0f && b >= 0.0f && b <= 1.0f)
                // TODO: verify that they should both use "a"
                return new Vector2f(line1Start.X + a * (line1End.X - line1Start.X), line1Start.Y + a * (line1End.Y - line1Start.Y));
            else
                return null;
        }

        /// <summary>
        /// Rotates a Vector2f. Angle is in radians
        /// </summary>
        /// <param name="vec">Vector2f to rotate</param>
        /// <param name="rad">Rotation angle in radians</param>
        /// <returns>New rotated Vector2f</returns>
        public static Vector2f RotateVector2f(Vector2f vec, float rad)
        {
            return new Vector2f(
                (float)(vec.X * Math.Cos(rad) - vec.Y * Math.Sin(rad)),
                (float)(vec.X * Math.Sin(rad) + vec.Y * Math.Cos(rad))
            );
        }
    }
}
