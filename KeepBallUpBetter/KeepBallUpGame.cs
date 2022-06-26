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
        // TODO this should be an amount of arena instead
        private const bool SPLIT_SCREEN = false;

        private const int DEFAULT_SEED = 1337;

        private Font _defaultFont;

        private Arena _arena;
        private Arena _arena2;

        private Vector2f _mousePos;
        private bool _mouseLeftPressed;
        private bool _mouseRightPressed;

        private Vector2f? _line1Start;
        private Vector2f? _line1End;

        private Vector2f? _line2Start;
        private Vector2f? _line2End;

        private Vector2f? _lineCollision;

        public KeepBallUpGame(RenderWindow window) : base(window) { }
        public KeepBallUpGame(RenderWindow window, uint targetFPS, uint targetTPS) : base(window, targetFPS, targetTPS) { }

        public override void Initialize()
        {
            if (SPLIT_SCREEN)
            {
                _arena = new Arena(this, new Vector2f(ARENA_BORDER_SIZE, ARENA_BORDER_SIZE / 2.0f), new Vector2f(GameWindow.DefaultView.Size.X, GameWindow.DefaultView.Size.Y / 2.0f) - new Vector2f(ARENA_BORDER_SIZE, ARENA_BORDER_SIZE / 2.0f) * 2.0f);
                _arena2 = new Arena(this, new Vector2f(ARENA_BORDER_SIZE, ARENA_BORDER_SIZE / 2.0f + GameWindow.DefaultView.Size.Y / 2.0f), new Vector2f(GameWindow.DefaultView.Size.X, GameWindow.DefaultView.Size.Y / 2.0f) - new Vector2f(ARENA_BORDER_SIZE, ARENA_BORDER_SIZE / 2.0f) * 2.0f);
            }
            else
                _arena = new Arena(this, new Vector2f(ARENA_BORDER_SIZE, ARENA_BORDER_SIZE), GameWindow.DefaultView.Size - new Vector2f(ARENA_BORDER_SIZE, ARENA_BORDER_SIZE) * 2.0f);

            RandomManager.Seed = DEFAULT_SEED;

            //TimeMultiplier = 0.1f;

            Console.WriteLine("Initialized");
        }

        public override void LoadContent()
        {
            _defaultFont = new Font("Content/CascadiaMono.ttf");

            Console.WriteLine("Content Loaded\n");
        }

        public override void Update(float deltaTime)
        {
            _mousePos = GameWindow.MapPixelToCoords(Mouse.GetPosition(GameWindow));

            if (Keyboard.IsKeyPressed(Keyboard.Key.Escape))
                IsRunning = false;

            if (Keyboard.IsKeyPressed(Keyboard.Key.Space))
                _arena.ResetAll();

            UpdateDebugCollisionLines();
            _arena.Update(deltaTime);

            if (_arena2 != null)
                _arena2.Update(deltaTime);

            _mouseLeftPressed = Mouse.IsButtonPressed(Mouse.Button.Left);
            _mouseRightPressed = Mouse.IsButtonPressed(Mouse.Button.Right);
        }

        public override void Draw(float deltaTime)
        {
            GameWindow.Clear();

            _arena.Draw(GameWindow, deltaTime);
            if (_arena2 != null)
                _arena2.Draw(GameWindow, deltaTime);
            DrawDebugCollisionLines();

            Print($"{FPS:0.00}fps\n{TPS:0.00}tps", 4.0f, 4.0f, 8);
            //Print($"GameTime: {GameTime:0.00}", 4.0f, 16.0f);
            //Print($"[{_mousePos.X:0};{_mousePos.Y:0}]", 8.0f, 16.0f);
            GameWindow.Display();
        }

        #region Debug stuff

        private void UpdateDebugCollisionLines()
        {
            if(Util.Collides(_mousePos, new Vector2f(1.0f, 1.0f), _arena.Position, _arena.Size))
            {
                if (_mouseLeftPressed && !Mouse.IsButtonPressed(Mouse.Button.Left))
                {
                    if (!_line1Start.HasValue)
                        _line1Start = _mousePos;
                    else if (!_line1End.HasValue)
                        _line1End = _mousePos;
                    else
                    {
                        _line1Start = _mousePos;
                        _line1End = null;
                    }
                }

                if (_mouseRightPressed && !Mouse.IsButtonPressed(Mouse.Button.Right))
                {
                    if (!_line2Start.HasValue)
                        _line2Start = _mousePos;
                    else if (!_line2End.HasValue)
                        _line2End = _mousePos;
                    else
                    {
                        _line2Start = _mousePos;
                        _line2End = null;
                    }
                }
            }

            if (!_line1Start.HasValue || !_line1End.HasValue || !_line2Start.HasValue || !_line2End.HasValue)
            {
                _lineCollision = null;
                return;
            }

            _lineCollision = Util.LineIntersection(_line1Start.Value, _line1End.Value, _line2Start.Value, _line2End.Value);
        }

        private void DrawDebugCollisionLines()
        {
            if (_line1Start.HasValue && _line1End.HasValue)
                DrawLine(_line1Start.Value, _line1End.Value);

            if (_line2Start.HasValue && _line2End.HasValue)
                DrawLine(_line2Start.Value, _line2End.Value);

            var debugCircleSize = 2.0f;
            if (_line1Start.HasValue)
            {
                var shape = new CircleShape(debugCircleSize)
                {
                    Position = _line1Start.Value,
                    FillColor = Color.Green,
                    Origin = new Vector2f(debugCircleSize, debugCircleSize)
                };
                GameWindow.Draw(shape);
            }

            if (_line1End.HasValue)
            {
                var shape = new CircleShape(debugCircleSize)
                {
                    Position = _line1End.Value,
                    FillColor = Color.Green,
                    Origin = new Vector2f(debugCircleSize, debugCircleSize)
                };
                GameWindow.Draw(shape);
            }

            if (_line2Start.HasValue)
            {
                var shape = new CircleShape(debugCircleSize)
                {
                    Position = _line2Start.Value,
                    FillColor = Color.Red,
                    Origin = new Vector2f(debugCircleSize, debugCircleSize)
                };
                GameWindow.Draw(shape);
            }

            if (_line2End.HasValue)
            {
                var shape = new CircleShape(debugCircleSize)
                {
                    Position = _line2End.Value,
                    FillColor = Color.Red,
                    Origin = new Vector2f(debugCircleSize, debugCircleSize)
                };
                GameWindow.Draw(shape);
            }

            if (_lineCollision.HasValue)
            {
                var shape = new CircleShape(debugCircleSize)
                {
                    Position = _lineCollision.Value,
                    FillColor = Color.Magenta,
                    Origin = new Vector2f(debugCircleSize, debugCircleSize)
                };
                GameWindow.Draw(shape);
            }
        }

        #endregion

        public void Print(string str, float x, float y, uint size = DEFAULT_FONT_SIZE, Color? color = null)
        {
            Print(str, x, y, _defaultFont, size, color);
        }

        public void Print(string str, float x, float y, Font font, uint size = DEFAULT_FONT_SIZE, Color? color = null)
        {
            var text = new Text(str, font, size)
            {
                Position = new Vector2f(x, y),
                FillColor = color.HasValue ? color.Value : Color.White
            };

            GameWindow.Draw(text);
        }

        public void DrawLine(Vector2f start, Vector2f end, Color? color = null)
        {
            Vertex[] line =
            {
                new Vertex(start),
                new Vertex(end)
            };

            if (color.HasValue)
                for (int i = 0; i < line.Length; i++)
                    line[i].Color = color.Value;

            GameWindow.Draw(line, PrimitiveType.Lines);
        }

        public void DrawLine(Line line, Color? color = null)
        {
            DrawLine(line.Start, line.End, color);
        }
    }
}
