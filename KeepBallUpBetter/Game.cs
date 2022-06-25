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
    public abstract class Game
    {
        public const uint DEFAULT_TARGET_FPS = 0;
        public RenderWindow GameWindow { get; protected set; }
        public Clock GameClock { get; protected set; }
        public float GameTime { get { return GameClock.ElapsedTime.AsSeconds(); } }
        public bool IsRunning { get; set; }

        private float _lastGameTime;

        public Game(RenderWindow window) : this(window, DEFAULT_TARGET_FPS) { }

        public Game(RenderWindow window, uint targetFPS)
        {
            GameClock = new Clock();
            IsRunning = false;
            GameWindow = window;
            GameWindow.SetFramerateLimit(targetFPS);
        }

        public void Run()
        {
            GameClock.Restart();

            Initialize();
            LoadContent();

            IsRunning = true;
            _lastGameTime = GameTime;

            while (IsRunning)
            {
                var delta = GameTime - _lastGameTime;
                _lastGameTime = GameTime;

                Update(delta);
                Draw(delta);
            }
        }

        public abstract void Initialize();
        public abstract void LoadContent();
        public abstract void Update(float deltaTime);
        public abstract void Draw(float deltaTime);
    }
}
