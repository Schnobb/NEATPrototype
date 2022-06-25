using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace KeepBallUp
{
    public class KeepBallUpGame : Game
    {
        private const int WIDTH = 800;
        private const int HEIGHT = 480;
        private const float PADDLE_VELOCITY = 8.0f;
        private const float INIT_BALL_VELOCITY = 10.0f;
        private const bool ENABLE_RANDOM_DIRECTION_CHANGE = true;
        private const float RANDOM_DIRECTION_CHANGE_CHANCE = 0.001f;
        private const float DIRECTION_OUTPUT_THRESHOLD = 0.1f;
        private const int NEAT_BATCH_SIZE = 20;
        private const float SLOW_DY_THRESHOLD = 3.0f;

        private const bool AI_ENABLED = true;
        private const bool AI_ENABLE_ANALOG_CONTROLS = true;

        private int? _seed = 1337;

        private enum Sensor
        {
            PaddleX,
            PaddleDX,
            BallRelX,
            BallRelY,
            BallDX,
            BallDY
        }

        private enum Output
        {
            Direction
        }

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private FrameCounter _frameCounter;

        private bool _dead;
        private int _score;
        private Ball _ball;
        private Paddle _paddle;
        private SpriteFont _defaultFont;
        private float _aiDirection;
        private int _slowTimer = 0;
        private int _maxSlowTimer = 60;

        private NEATManager _neatManager;

        private Random _random;

        public KeepBallUpGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = WIDTH;
            _graphics.PreferredBackBufferHeight = HEIGHT;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _frameCounter = new FrameCounter();

            if (_seed.HasValue)
                _random = new Random(_seed.Value);
            else
                _random = new Random();

            _neatManager = new NEATManager(_random, Enum.GetNames(typeof(Sensor)).Length, Enum.GetNames(typeof(Output)).Length, NEAT_BATCH_SIZE);
        }

        protected override void Initialize()
        {
            _dead = false;
            _score = 0;

            _ball = new Ball(WIDTH / 2.0f, HEIGHT / 2.0f, WIDTH, HEIGHT);
            _paddle = new Paddle(WIDTH / 2.0f, HEIGHT - HEIGHT / 16.0f, WIDTH);
            _ball.paddle = _paddle;

            BallDirectionChange();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _ball.Sprite = Content.Load<Texture2D>("ball");
            _paddle.Sprite = Content.Load<Texture2D>("paddle");
            _defaultFont = Content.Load<SpriteFont>("default");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (_dead)
                Reset();

            _aiDirection = 0.0f;
            if (AI_ENABLED)
            {
                SetSensorValues();
                _neatManager.GetCurrentNEAT().ComputeValues();
                var values = _neatManager.GetCurrentNEAT().GetOutputValues();
                var direction = values[(int)Output.Direction];
                _aiDirection = (float)direction;

                if (Math.Abs(direction) > DIRECTION_OUTPUT_THRESHOLD)
                {
                    if (AI_ENABLE_ANALOG_CONTROLS)
                        _paddle.DX = (float)direction * PADDLE_VELOCITY;
                    else
                        _paddle.DX = Math.Sign(direction) * PADDLE_VELOCITY;
                }
            }
            else
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Right))
                    _paddle.DX = PADDLE_VELOCITY;

                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                    _paddle.DX = -PADDLE_VELOCITY;
            }

            if (ENABLE_RANDOM_DIRECTION_CHANGE && (float)_random.NextDouble() < RANDOM_DIRECTION_CHANGE_CHANCE)
                BallDirectionChange();

            if (_slowTimer > 0 || Math.Abs(_ball.DY) <= SLOW_DY_THRESHOLD)
            {
                if (Math.Abs(_ball.DY) > SLOW_DY_THRESHOLD)
                    _slowTimer = 0;
                else
                {
                    _slowTimer++;
                    if (_slowTimer > _maxSlowTimer)
                    {
                        _slowTimer = 0;
                        BallDirectionChange();
                    }
                }
            }

            _paddle.Update(gameTime);
            _ball.Update(gameTime);

            _dead = _ball.Dead;

            if (_ball.Scored)
            {
                _ball.Scored = false;
                _score += 1;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _ball.Draw(gameTime, _spriteBatch);
            _paddle.Draw(gameTime, _spriteBatch);

            _spriteBatch.DrawString(_defaultFont, $"Score: {_score}", new Vector2(8.0f, 24.0f), Color.Indigo);
            _spriteBatch.DrawString(_defaultFont, $"Current batch: {_neatManager.CurrentIndex + 1}/{_neatManager.BatchSize}", new Vector2(8.0f, 40.0f), Color.Indigo);
            _spriteBatch.DrawString(_defaultFont, $"Current direction: {_aiDirection}", new Vector2(8.0f, 56.0f), Color.Indigo);
            _spriteBatch.DrawString(_defaultFont, $"Current DY: {_ball.DY}", new Vector2(8.0f, 56f+16f), Color.Indigo);

            var highScores = "High Scores:\n";
            for (int i = 0; i < 10; i++)
            {
                if (_neatManager.DeadNEATs.Count - 1 - i < 0)
                    highScores += $"  {i + 1}.\n";
                else
                {
                    var score = _neatManager.DeadNEATs.Values[_neatManager.DeadNEATs.Count - 1 - i].Fitness;
                    highScores += $"  {i + 1}. {score}\n";
                }
            }

            _spriteBatch.DrawString(_defaultFont, highScores, new Vector2(WIDTH - WIDTH / 4, 8.0f), Color.DarkOrange);

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter.Update(deltaTime);
            var fps = $"{(int)Math.Round(_frameCounter.AverageFramesPerSecond)}fps";
            _spriteBatch.DrawString(_defaultFont, fps, new Vector2(0, 0), Color.Black);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        public void Reset()
        {
            _neatManager.GetCurrentNEAT().Fitness = _score;
            _neatManager.NextNEAT();
            Initialize();
        }

        private void BallDirectionChange()
        {
            var randomRad = _random.NextDouble() * Math.PI;
            _ball.DX = (float)-Math.Sin(randomRad) * INIT_BALL_VELOCITY;
            _ball.DY = (float)Math.Cos(randomRad) * INIT_BALL_VELOCITY;
        }

        private void SetSensorValues()
        {
            var paddleX = (_paddle.X * 2.0f - WIDTH) / WIDTH;
            var paddleDX = _paddle.DX / PADDLE_VELOCITY;

            var ballRelX = (_ball.X - _paddle.X) / WIDTH;
            var ballRelY = (_ball.Y - _paddle.Y) / HEIGHT;

            var ballDX = _ball.DX / INIT_BALL_VELOCITY;
            var ballDY = _ball.DY / INIT_BALL_VELOCITY;

            _neatManager.GetCurrentNEAT().SetSensorValues(paddleX, paddleDX, ballRelX, ballRelY, ballDX, ballDY);
        }
    }

    class Ball
    {
        public float X;
        public float Y;
        public float DX;
        public float DY;
        public float Size;
        public bool Dead;
        public bool Scored;

        public float MaxX;
        public float MaxY;

        public Paddle paddle;

        private Texture2D _sprite;
        public Texture2D Sprite
        {
            get
            {
                return _sprite;
            }
            set
            {
                _sprite = value;
                Size = _sprite.Width;
            }
        }
        public Ball(float x, float y, float maxX, float maxY)
        {
            X = x;
            Y = y;
            MaxX = maxX;
            MaxY = maxY;
            Dead = false;
            Scored = false;
        }

        public void Update(GameTime gameTime)
        {
            var tempX = X + DX;
            var tempY = Y + DY;

            if (tempY + Size > MaxY)
                Dead = true;

            if (tempX - Size / 2.0f < 0 || tempX + Size / 2.0f > MaxX)
            {
                DX *= -1.0f;
                tempX = tempX - Size / 2.0f < 0 ? Size / 2.0f : MaxX - Size / 2.0f;
            }

            if (tempY - Size / 2.0f < 0 || tempY + Size / 2.0f > MaxY)
            {
                DY *= -1.0f;
                tempY = tempY - Size / 2.0f < 0 ? Size / 2.0f : MaxY - Size / 2.0f;
            }

            if (Collides(tempX, tempY, Size, Size, paddle.X, paddle.Y, paddle.Width, paddle.Height))
            {
                DY *= -1.0f;
                tempY = paddle.Y - Size;
                Scored = true;
            }

            X = tempX;
            Y = tempY;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite, new Vector2(X - Size / 2.0f, Y - Size / 2.0f), Color.White);
        }

        private bool Collides(float x1, float y1, float w1, float h1, float x2, float y2, float w2, float h2)
        {
            return x1 < x2 + w2 && x1 + w1 > x2 && y1 < y2 + h2 && y1 + h1 > y2;
        }
    }

    class Paddle
    {
        public const float Friction = 0.92f;

        public float X;
        public float Y;
        public float DX;
        public float DY;
        public float Width;
        public float Height;

        public float MaxX;

        private Texture2D _sprite;
        public Texture2D Sprite
        {
            get
            {
                return _sprite;
            }
            set
            {
                _sprite = value;
                Width = _sprite.Width;
                Height = _sprite.Height;
            }
        }

        public Paddle(float x, float y, float maxX)
        {
            X = x;
            Y = y;
            MaxX = maxX;
        }

        public void Update(GameTime gameTime)
        {
            var tempX = X + DX;
            var tempY = Y + DY;

            if (tempX - Width / 2.0f < 0 || tempX + Width / 2.0f > MaxX)
            {
                DX = 0;
                tempX = tempX - Width / 2.0f < 0 ? Width / 2.0f : MaxX - Width / 2.0f;
            }

            DX *= Friction;

            if (Math.Abs(DX) < 0.01f)
                DX = 0.0f;

            X = tempX;
            Y = tempY;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite, new Vector2(X - Width / 2.0f, Y - Height / 2.0f), Color.White);
        }
    }
}
