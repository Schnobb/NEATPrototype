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
        // TODO velocity variation?

        public Vector2f Position { get; set; }
        public Vector2f Size { get; set; }
        public Color Color { get; set; } = new Color(32, 32, 32);

        public Ball Ball { get; set; }
        public Paddle Paddle { get; set; }

        public float Gravity { get; set; }
        public float MaxGravity { get; set; }
        public float GravityTimer { get; set; }
        public float MaxGravityTimer { get; set; }
        public float GravityTimerThreshold { get; set; }

        public float RandomChangeTimer { get; set; }
        public float CurrentMaxRandomChangeTimer { get; set; }
        public float BaseMaxRandomChangeTimer { get; set; } = 4.0f;
        public float MaxRandomChangeTimerVariation { get; set; } = 0.5f;

        public float RoundTime { get; set; }

        public int Lives { get; set; }
        public int MaxLives { get; set; }

        // Probably not a good idea, but fuck it for now. I need some functions from it
        public KeepBallUpGame Parent { get; set; }

        public int Score { get; set; }

        public Arena(KeepBallUpGame parent, Vector2f position, Vector2f size)
        {
            Position = position;
            Size = size;
            Parent = parent;

            MaxGravity = 9.8f * 60.0f;
            MaxGravityTimer = 16.0f;
            GravityTimerThreshold = 4.0f;
            MaxLives = 2;

            ResetAll();
        }

        public void ResetAll()
        {
            if (Parent.BrainManager != null)
                Parent.BrainManager.NextGenome(Score);

            Reset();
            Score = 0;
            Lives = MaxLives;
        }

        public void Reset()
        {
            Gravity = 0;
            GravityTimer = 0;
            RoundTime = 0;
            ResetRandomChangeTimer();

            var ballDir = new Vector2f(0.0f, 1.0f);
            //ballDir = ballDir.Rotate((float)Math.PI / 4.5f);
            Ball = new Ball(Size / 2.0f, ballDir, 600.0f, 8.0f, Color.Red, this);
            Ball.RandomizeDirection();

            Paddle = new Paddle(new Vector2f(Size.X / 2.0f - 40.0f, Size.Y - Size.Y / 10.0f - 8.0f), new Vector2f(80.0f, 16.0f), new Color(0, 175, 0), this);
        }

        public void ResetRandomChangeTimer()
        {
            RandomChangeTimer = 0;
            CurrentMaxRandomChangeTimer = BaseMaxRandomChangeTimer + MaxRandomChangeTimerVariation * BaseMaxRandomChangeTimer * RandomManager.GetNextFullFloat();
        }

        public void Dead()
        {
            Lives--;
            if (Lives < 0)
                ResetAll();
            else
                Reset();
        }

        public void Update(float deltaTime)
        {
            RoundTime += deltaTime;
            GravityTimer += deltaTime;
            if (GravityTimer > GravityTimerThreshold)
                Gravity = Math.Clamp(((GravityTimer - GravityTimerThreshold) / MaxGravityTimer) * MaxGravity, 0.0f, MaxGravity);
            else
                Gravity = 0;

            RandomChangeTimer += deltaTime;
            if (RandomChangeTimer > CurrentMaxRandomChangeTimer)
            {
                Ball.RandomizeDirection();
                ResetRandomChangeTimer();
            }

            Paddle.Update(deltaTime);
            Ball.Update(deltaTime);
        }

        public void Draw(RenderWindow window, float deltaTime)
        {
            var shape = new RectangleShape(Size)
            {
                Position = Position,
                FillColor = Color
            };
            window.Draw(shape);

            Ball.Draw(window, deltaTime);
            Paddle.Draw(window, deltaTime);

            var hud = $"SCORE: {Score}\nLIVES: {Lives}\nTIME: {RoundTime:0.00}";
            if (KeepBallUpGame.ENABLE_AI)
                hud += $"\nGENOME BATCH: {Parent.BrainManager.CurrentGenomeIndex + 1}/{Parent.BrainManager.BatchSize}";

            Parent.Print(hud, Position.X + 8.0f, Position.Y + 8.0f, 14);
            //Parent.Print($"Gravity Timer: {GravityTimer}\nCurrent Gravity: {Gravity}", Position.X + 8.0f, Position.Y + 32.0f, 12);
        }
    }
}
