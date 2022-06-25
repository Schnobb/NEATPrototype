using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeepBallUpBetter
{
    internal class Arena
    {
        public Vector2f Position { get; set; }
        public Vector2f Size { get; set; }
        public Ball Ball { get; set; }
        public Paddle Paddle { get; set; }

        public Arena(Vector2f position, Vector2f size)
        {
            Position = position;
            Size = size;

            Ball = new Ball(Size / 2.0f, new Vector2f(0.0f, 1.0f), 600.0f, 8.0f, Color.Red, this);
            Paddle = new Paddle(new Vector2f(Size.X / 2.0f - 40.0f, Size.Y - Size.Y / 10.0f - 8.0f), new Vector2f(80.0f, 16.0f), new Color(0, 175, 0), this);
        }

        public void Update(float deltaTime)
        {
            Paddle.Update(deltaTime);
            Ball.Update(deltaTime);
        }

        public void Draw(RenderWindow window, float deltaTime)
        {
            var shape = new RectangleShape(Size)
            {
                Position = Position,
                FillColor = new Color(32, 32, 32)
            };
            window.Draw(shape);

            Ball.Draw(window, deltaTime);
            Paddle.Draw(window, deltaTime);
        }
    }
}
