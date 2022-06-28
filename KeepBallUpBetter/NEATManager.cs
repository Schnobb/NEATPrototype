using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeepBallUpBetter
{
    internal class NEATManager
    {
        public enum Sensor
        {
            PaddleX,
            PaddleDX,
            BallRelX,
            BallRelY,
            BallDX,
            BallDY,
            Constant,
            Loopback
        }

        public enum Output
        {
            DirectionX,
            Loopback
        }
    }
}
