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
            var posDelta = Direction * Velocity;
            var newPos = Position + posDelta * deltaTime;

            if (newPos.X - Size < 0.0f || newPos.X + Size > Arena.Size.X)
            {
                Direction = new Vector2f(Direction.X * -1.0f, Direction.Y);
                newPos.X = newPos.X - Size < 0.0f ? Size : Arena.Size.X - Size;
            }

            if (newPos.Y - Size < 0.0f || newPos.Y + Size > Arena.Size.Y)
            {
                Direction = new Vector2f(Direction.X, Direction.Y * -1.0f);
                newPos.Y = newPos.Y - Size < 0.0f ? Size : Arena.Size.Y - Size;
            }

            Position = newPos;
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
