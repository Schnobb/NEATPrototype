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
    internal class Paddle
    {
        private const bool USE_AI = false;
        private const float MAX_VELOCITY_GAIN = 650.0f;
        private const float DEFAULT_FRICTION = 0.35f;
        private const float MIN_VELOCITY_THRESHOLD = 0.01f;

        public Vector2f Position { get; set; }
        public Vector2f Size { get; set; }
        public Vector2f Direction { get; set; }
        public float Velocity { get; set; }
        public float Friction { get; set; }

        public Color Color { get; set; }

        public Arena Arena { get; set; }

        public Paddle(Vector2f position, Vector2f size, Color color, Arena arena)
        {
            Position = position;
            Size = size;
            Color = color;
            Arena = arena;

            Direction = new Vector2f(1.0f, 0.0f);
            Velocity = 0.0f;
            Friction = DEFAULT_FRICTION;
        }

        public void Update(float deltaTime)
        {
            if (USE_AI)
            {

            }
            else
            {
                if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
                    Velocity = MAX_VELOCITY_GAIN;// * deltaTime;
                if (Keyboard.IsKeyPressed(Keyboard.Key.Left))
                    Velocity = -MAX_VELOCITY_GAIN;// * deltaTime;
            }

            var posDelta = Direction * Velocity;
            var newPos = Position + posDelta * deltaTime;

            Velocity -= (1.0f/Friction) * Velocity * deltaTime;
            if (Math.Abs(Velocity) < MIN_VELOCITY_THRESHOLD)
                Velocity = 0.0f;

            if (newPos.X < 0.0f)
            {
                Velocity = 0.0f;
                newPos.X = 0.0f;
            }
            else if (newPos.X + Size.X > Arena.Size.X)
            {
                Velocity = 0.0f;
                newPos.X = Arena.Size.X - Size.X;
            }

            Position = newPos;
        }

        public void Draw(RenderWindow window, float deltaTime)
        {
            var shape = new RectangleShape(Size)
            {
                Position = Position + Arena.Position,
                FillColor = Color
            };

            window.Draw(shape);
        }
    }
}
