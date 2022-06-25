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
    internal class KeepBallUpGame : Game
    {
        private const uint DEFAULT_FONT_SIZE = 12;
        private const float ARENA_BORDER_SIZE = 32.0f;

        private Font _defaultFont;
        private float _fps;

        private Arena _arena;

        public KeepBallUpGame(RenderWindow window) : base(window) { }
        public KeepBallUpGame(RenderWindow window, uint targetFPS) : base(window, targetFPS) { }

        public override void Initialize()
        {
            _arena = new Arena(new Vector2f(ARENA_BORDER_SIZE, ARENA_BORDER_SIZE), GameWindow.DefaultView.Size - new Vector2f(ARENA_BORDER_SIZE, ARENA_BORDER_SIZE) * 2.0f);

            Console.WriteLine("Initialized");
        }

        public override void LoadContent()
        {
            _defaultFont = new Font("Content/CascadiaMono.ttf");

            Console.WriteLine("Content Loaded\n");
        }

        public override void Update(float deltaTime)
        {
            GameWindow.DispatchEvents();
            _fps = 1.0f / deltaTime;

            if (Keyboard.IsKeyPressed(Keyboard.Key.Escape))
                IsRunning = false;

            _arena.Update(deltaTime);
        }

        public override void Draw(float deltaTime)
        {
            GameWindow.Clear();

            _arena.Draw(GameWindow, deltaTime);

            Print($"{_fps:0.00}fps", 4.0f, 4.0f, 8);
            var mousePos = GameWindow.MapPixelToCoords(Mouse.GetPosition(GameWindow));
            Print($"[{mousePos.X:0};{mousePos.Y:0}]", 8.0f, 16.0f);
            GameWindow.Display();
        }

        private void Print(string str, float x, float y, uint size = DEFAULT_FONT_SIZE)
        {
            Print(str, x, y, _defaultFont, size);
        }

        private void Print(string str, float x, float y, Font font, uint size = DEFAULT_FONT_SIZE)
        {
            var text = new Text(str, font, size)
            {
                Position = new Vector2f(x, y)
            };

            GameWindow.Draw(text);
        }
    }    
}
