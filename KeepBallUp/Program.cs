using System;

namespace KeepBallUp
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new KeepBallUpGame())
                game.Run();
        }
    }
}
