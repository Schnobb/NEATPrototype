using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeepBallUpBetter
{
    internal class Ball
    {
        private const float RANDOM_ANGLE_VARIATION_ON_BOUNCE = (float)Math.PI / 16.0f;

        public Vector2f Position { get; set; }
        public Vector2f Direction { get; set; }
        public float Velocity { get; set; }

        public float Size { get; set; }
        public Color Color { get; set; }

        public Arena Arena { get; set; }

        public Ball(Vector2f position, Vector2f direction, float velocity, float size, Color color, Arena arena)
        {
            Position = position;
            Direction = direction;
            Velocity = velocity;
            Size = size;
            Color = color;
            Arena = arena;
        }

        public void Update(float deltaTime)
        {
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
                Direction = Util.RotateVector2f(Direction, (RandomManager.GetNextFloat() * 2.0f - 1.0f) * RANDOM_ANGLE_VARIATION_ON_BOUNCE);

            Position = newPos;
        }

        private bool HandlePaddleCollisions(ref Vector2f newPos)
        {
            // TODO this need to be done twice. Right now I check a ray from the middle of the ball
            // I need to check a ray from each side of the ball

            var testLeft = newPos.X - Position.X >= 0.0f;
            var testTop = newPos.Y - Position.Y >= 0.0f;

            Vector2f paddleLineStart;
            Vector2f paddleLineEnd;

            if (testLeft)
            {
                paddleLineStart = Arena.Paddle.Position;
                paddleLineEnd = Arena.Paddle.Position + new Vector2f(0.0f, Arena.Paddle.Size.Y);
            }
            else
            {
                paddleLineStart = Arena.Paddle.Position + new Vector2f(Arena.Paddle.Size.X, 0.0f);
                paddleLineEnd = Arena.Paddle.Position + Arena.Paddle.Size;
            }

            var intersection = Util.lineIntersection(Position, newPos, paddleLineStart, paddleLineEnd);
            if (intersection.HasValue)
            {
                Direction = new Vector2f(-Direction.X, Direction.Y);
                newPos = testLeft ? intersection.Value - new Vector2f(Size, 0.0f) : intersection.Value + new Vector2f(Size, 0.0f);
                Arena.Score++;
                return true;
            }

            if (testTop)
            {
                paddleLineStart = Arena.Paddle.Position;
                paddleLineEnd = Arena.Paddle.Position + new Vector2f(Arena.Paddle.Size.X, 0.0f);
            }
            else
            {
                paddleLineStart = Arena.Paddle.Position + new Vector2f(0.0f, Arena.Paddle.Size.Y);
                paddleLineEnd = Arena.Paddle.Position + Arena.Paddle.Size;
            }

            intersection = Util.lineIntersection(Position, newPos, paddleLineStart, paddleLineEnd);
            if (intersection.HasValue)
            {
                Direction = new Vector2f(Direction.X, -Direction.Y);
                newPos = testTop ? intersection.Value - new Vector2f(0.0f, Size) : intersection.Value + new Vector2f(0.0f, Size);
                Arena.Score++;
                return true;
            }

            return false;
        }

        public void Draw(RenderWindow window, float deltaTime)
        {
            var shape = new CircleShape(Size)
            {
                Position = Position + Arena.Position,
                FillColor = Color,
                Origin = new Vector2f(Size, Size)
            };

            window.Draw(shape);
        }
    }
}
