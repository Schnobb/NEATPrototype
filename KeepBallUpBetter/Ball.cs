using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeepBallUpBetter
{
    internal class Ball
    {
        private const bool DISABLE_RANDOM_DIRECTION = false;
        private const float RANDOM_ANGLE_VARIATION_ON_BOUNCE = (float)Math.PI / 16.0f;
        private const uint BALL_COLLISION_RESOLUTION = 16;
        private const bool DEBUG_DISABLE_MOVEMENT = false;
        private const bool SHOW_DEBUG_LINES = false;

        public Vector2f Position { get; set; }
        public Vector2f Direction { get; set; }
        public float Velocity { get; set; }

        public float Size { get; set; }
        public Color Color { get; set; }

        public Arena Arena { get; set; }

        private volatile ConcurrentBag<Line> _debugLines;
        private CircleShape _shape;

        public Ball(Vector2f position, Vector2f direction, float velocity, float size, Color color, Arena arena)
        {
            Position = position;
            Direction = direction;
            Velocity = velocity;
            Size = size;
            Color = color;
            Arena = arena;

            _debugLines = new ConcurrentBag<Line>();

            _shape = new CircleShape(Size)
            {
                Origin = new Vector2f(Size, Size)
            };
        }

        #region Update/Draw

        public void Update(float deltaTime)
        {
            _debugLines.Clear();

            if (SHOW_DEBUG_LINES)
                AddDebugLines(deltaTime);

            var posDelta = Direction * Velocity + new Vector2f(0.0f, Arena.Gravity);
            var newPos = Position + posDelta * deltaTime;

            var collided = false;

            if (newPos.X - Size < 0.0f || newPos.X + Size > Arena.Size.X)
            {
                Direction = new Vector2f(-Direction.X, Direction.Y);
                newPos.X = newPos.X - Size < 0.0f ? Size : Arena.Size.X - Size;
                collided = true;
            }

            if (newPos.Y - Size < 0.0f)
            {
                Direction = new Vector2f(Direction.X, -Direction.Y);
                newPos.Y = newPos.Y - Size < 0.0f ? Size : Arena.Size.Y - Size;
                collided = true;
            }

            if (newPos.Y + Size > Arena.Size.Y)
                Arena.Dead();

            var paddleCollision = HandlePaddleCollisions(ref newPos);

            if (paddleCollision)
                Arena.GravityTimer = 0.0f;

            collided = collided || paddleCollision;

            if (collided)
                Direction = Direction.Rotate((RandomManager.GetNextFloat() * 2.0f - 1.0f) * RANDOM_ANGLE_VARIATION_ON_BOUNCE);

            if (!DEBUG_DISABLE_MOVEMENT)
                Position = newPos;

            //Direction = Util.RotateVector2f(Direction, (float)Math.PI / 40.0f);
        }

        public void Draw(RenderWindow window, float deltaTime)
        {
            _shape.Position = Position + Arena.Position;
            _shape.FillColor = Color;
            window.Draw(_shape);

            foreach (var line in _debugLines)
            {
                var newLine = new Line(line.Start + Arena.Position, line.End + Arena.Position);
                Arena.Parent.DrawLine(newLine);
            }
        }

        #endregion

        public void RandomizeDirection()
        {
            if (DISABLE_RANDOM_DIRECTION)
                return;

            Direction = new Vector2f(0.0f, 1.0f).Rotate(RandomManager.GetNextFloat() * 2.0f * (float)Math.PI);
        }

        #region Privates

        private void AddDebugLines(float deltaTime)
        {
            var visualAidMultiplier = 16.0f;
            var baseLine = new Line(Position, Position + Direction * Velocity * deltaTime * visualAidMultiplier);

            var lines = new List<Line>();
            for (int i = 0; i < BALL_COLLISION_RESOLUTION; i++)
            {
                var currentAngle = (float)(Math.PI / 2.0) / (float)(BALL_COLLISION_RESOLUTION - 1);
                currentAngle *= i;

                var translationDirection = Direction.Rotate(currentAngle);
                var translation = translationDirection * Size;
                lines.Add(baseLine + translation);

                if (i > 0)
                {
                    translationDirection = Direction.Rotate(-currentAngle);
                    translation = translationDirection * Size;
                    lines.Add(baseLine + translation);
                }
            }

            foreach (var line in lines)
                _debugLines.Add(line);
        }

        private bool HandlePaddleCollisions(ref Vector2f newPos)
        {
            var baseLine = new Line(Position, newPos);
            var direction = -baseLine.Direction();

            var collisionLines = new List<Line>();
            for (int i = 0; i < BALL_COLLISION_RESOLUTION; i++)
            {
                var currentAngle = (float)(Math.PI / 2.0) / (float)(BALL_COLLISION_RESOLUTION - 1);
                currentAngle *= i;

                var translationDirection = direction.Rotate(currentAngle);
                var translation = translationDirection * Size;

                var t = translationDirection.Dot(direction);
                var collisionLine = new Line(Position, newPos + -direction * t * Size);
                collisionLines.Add(collisionLine + translation);

                if (i > 0)
                {
                    translationDirection = direction.Rotate(-currentAngle);
                    translation = translationDirection * Size;
                    collisionLine = new Line(Position, newPos + -direction * t * Size);
                    collisionLines.Add(collisionLine + translation);
                }
            }

            //foreach (var line in collisionLines)
            //    _debugLines.Add(line * 1.0f);

            var collisions = new List<Bounce>();
            foreach (var collisionLine in collisionLines)
            {
                var bounce = TestLineCollision(collisionLine);
                if (bounce.HasValue)
                    collisions.Add(bounce.Value);
                //{
                //    var offsetMagnitude = bounce.Value.BallOffset.Magnitude();
                //    //collisions.Add(offsetMagnitude, bounce.Value);
                //}
            }

            if (collisions.Count <= 0)
                return false;

            collisions.Sort((c1, c2) => c1.BallOffset.Magnitude().CompareTo(c2.BallOffset.Magnitude()));
            var highestBounce = collisions.Last();
            newPos = highestBounce.Intersection - highestBounce.BallOffset;

            switch (highestBounce.CollisionSide)
            {
                case Bounce.Side.Top:
                    Direction = new Vector2f(Direction.X, -Math.Abs(Direction.Y));
                    break;
                case Bounce.Side.Right:
                    Direction = new Vector2f(Math.Abs(Direction.X), -Math.Abs(Direction.Y));
                    break;
                case Bounce.Side.Bottom:
                    Direction = new Vector2f(Direction.X, Math.Abs(Direction.Y));
                    break;
                case Bounce.Side.Left:
                    Direction = new Vector2f(-Math.Abs(Direction.X), -Math.Abs(Direction.Y));
                    break;
            }

            Arena.Score++;
            return true;

            //foreach (var collisionLine in collisionLines)
            //{
            //    var bounce = TestLineCollision(collisionLine);
            //    if (bounce.HasValue)
            //    {
            //        newPos = bounce.Value.Intersection - bounce.Value.BallOffset;

            //        switch (bounce.Value.CollisionSide)
            //        {
            //            case Bounce.Side.Top:
            //                Direction = new Vector2f(Direction.X, -Math.Abs(Direction.Y));
            //                break;
            //            case Bounce.Side.Right:
            //                Direction = new Vector2f(Math.Abs(Direction.X), -Math.Abs(Direction.Y));
            //                break;
            //            case Bounce.Side.Bottom:
            //                Direction = new Vector2f(Direction.X, Math.Abs(Direction.Y));
            //                break;
            //            case Bounce.Side.Left:
            //                Direction = new Vector2f(-Math.Abs(Direction.X), -Math.Abs(Direction.Y));
            //                break;
            //        }

            //        Arena.Score++;
            //        return true;
            //    }
            //}

            //return false;
        }

        private struct Bounce
        {
            public enum Side
            {
                Top,
                Right,
                Bottom,
                Left
            }

            public Vector2f Intersection;
            public Vector2f BallOffset;
            public Side CollisionSide;

            public Bounce(Vector2f intersection, Vector2f ballOffset, Side side)
            {
                Intersection = intersection;
                BallOffset = ballOffset;
                CollisionSide = side;
            }
        }

        private Bounce? TestLineCollision(Line line)
        {
            Line paddleLine;

            if (line.PointedRight)
                paddleLine = new Line(Arena.Paddle.Position, Arena.Paddle.Position + new Vector2f(0.0f, Arena.Paddle.Size.Y));
            else
                paddleLine = new Line(Arena.Paddle.Position + new Vector2f(Arena.Paddle.Size.X, 0.0f), Arena.Paddle.Position + Arena.Paddle.Size);

            var intersection = Util.LineIntersection(line, paddleLine);
            if (intersection.HasValue)
            {
                if (line.PointedRight)
                    return new Bounce(intersection.Value, line.Start - Position, Bounce.Side.Left);
                else
                    return new Bounce(intersection.Value, line.End - Position, Bounce.Side.Right);
            }

            if (line.PointedDown)
                paddleLine = new Line(Arena.Paddle.Position, Arena.Paddle.Position + new Vector2f(Arena.Paddle.Size.X, 0.0f));
            else
                paddleLine = new Line(Arena.Paddle.Position + new Vector2f(0.0f, Arena.Paddle.Size.Y), Arena.Paddle.Position + Arena.Paddle.Size);

            intersection = Util.LineIntersection(line, paddleLine);
            if (intersection.HasValue)
            {
                if (line.PointedDown)
                    return new Bounce(intersection.Value, line.Start - Position, Bounce.Side.Top);
                else
                    return new Bounce(intersection.Value, line.End - Position, Bounce.Side.Bottom);
            }

            return null;
        }

        #endregion
    }
}
