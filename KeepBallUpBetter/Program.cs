using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace KeepBallUpBetter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var window = new RenderWindow(new VideoMode(960, 540), "Keep The Ball Up!");
            var game = new KeepBallUpGame(window, 144);
            window.Closed += (sender, e) => { game.IsRunning = false; window.Close(); };
            //window.KeyPressed += (sender, e) => { if (e.Code == Keyboard.Key.Escape) { window.Close(); } };

            game.Run();
        }
    }
}