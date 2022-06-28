using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeepBallUpBetter
{
    internal static class Extensions
    {
        public static float Dot(this Vector2f v1, Vector2f v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        public static float Magnitude(this Vector2f v)
        {
            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
        }

        /// <summary>
        /// Rotates a Vector2f clockwise. Angle is in radians.
        /// </summary>
        /// <param name="v">Vector2f to rotate.</param>
        /// <param name="rad">Rotation angle in radians.</param>
        /// <returns>New rotated Vector2f.</returns>
        public static Vector2f Rotate(this Vector2f v, float rad)
        {
            return new Vector2f(
                (float)(v.X * Math.Cos(rad) - v.Y * Math.Sin(rad)),
                (float)(v.X * Math.Sin(rad) + v.Y * Math.Cos(rad))
            );
        }
    }
}
