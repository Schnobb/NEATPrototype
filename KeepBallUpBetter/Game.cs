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
    internal abstract class Game
    {
        public const uint DEFAULT_TARGET_FPS = 0;
        public const uint DEFAULT_TARGET_TPS = 0;
        public const float DEFAULT_TIME_MULTIPLIER = 1.0f;

        public RenderWindow GameWindow { get; protected set; }
        public Clock GameClock { get; protected set; }
        public float GameTime { get { return GameClock.ElapsedTime.AsSeconds() * TimeMultiplier; } }
        public bool IsRunning { get; set; }
        public float TimeMultiplier { get; set; }
        public uint TargetTPS { get; set; }
        private uint _targetFPS;
        public uint TargetFPS
        {
            get
            {
                return _targetFPS;
            }

            set
            {
                _targetFPS = value;
                GameWindow.SetFramerateLimit(_targetFPS);
            }
        }

        public float FPS { get; set; }
        public float TPS { get; set; }

        private float _lastDrawTime;
        private float _lastUpdateTime;

        public Game(RenderWindow window) : this(window, DEFAULT_TARGET_FPS, DEFAULT_TARGET_TPS) { }

        public Game(RenderWindow window, uint targetFPS, uint targetTPS)
        {
            GameClock = new Clock();
            IsRunning = false;
            GameWindow = window;
            TargetFPS = targetFPS;
            TargetTPS = targetTPS > 0 ? targetTPS : uint.MaxValue;
            TimeMultiplier = DEFAULT_TIME_MULTIPLIER;
        }

        public void Run()
        {
            GameClock.Restart();

            Initialize();
            LoadContent();

            IsRunning = true;

            var updateTask = Task.Run(() =>
            {
                var millisecondsPerUpdates = 1000.0f / (float)TargetTPS;
                _lastUpdateTime = GameTime;

                while (IsRunning)
                {
                    var delta = GameTime - _lastUpdateTime;
                    if (delta < millisecondsPerUpdates)
                    {
                        Thread.Sleep((int)(millisecondsPerUpdates - delta));
                        delta = GameTime - _lastUpdateTime;
                    }
                    _lastUpdateTime = GameTime;

                    TPS = 1.0f / (delta / TimeMultiplier);
                    Update(delta);
                }
            });

            _lastDrawTime = GameTime;

            while (IsRunning)
            {
                GameWindow.DispatchEvents();
                var delta = GameTime - _lastDrawTime;
                _lastDrawTime = GameTime;

                FPS = 1.0f / (delta / TimeMultiplier);
                Draw(delta);
            }

            Task.WaitAll(updateTask);
        }

        public abstract void Initialize();
        public abstract void LoadContent();
        public abstract void Update(float deltaTime);
        public abstract void Draw(float deltaTime);
    }
}
