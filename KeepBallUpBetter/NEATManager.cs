using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeepBallUpBetter
{
    internal class NEATManager
    {
        // TODO we'll probably want to handle multiple NEATs at the same time, this is written to handle one NEAT at a time
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

        public NEAT.Genus Genus { get; private set; }

        public int BatchSize { get; set; } = 30;
        public List<NEAT.NEAT> CurrentBatch { get; private set; }
        public NEAT.NEAT CurrentNEAT { get { return CurrentBatch[_currentNEATIndex]; } }

        public List<NEAT.NEAT> History { get; private set; }
        public List<NEAT.NEAT> HallOfFame { get; private set; }

        private int _currentNEATIndex;
        private double _loopbackValue;

        public NEATManager()
        {
            Genus = new NEAT.Genus(BatchSize, Enum.GetNames(typeof(Sensor)).Length, Enum.GetNames(typeof(Output)).Length);
            CurrentBatch = Genus.GenerateNewBatch(RandomManager.GetRandomInstance());
            _currentNEATIndex = 0;
            History = new List<NEAT.NEAT>();
        }

        public NEAT.NEAT NextNEAT(double fitness)
        {
            CurrentNEAT.Fitness = fitness;
            _loopbackValue = 0;
            _currentNEATIndex++;

            if (_currentNEATIndex >= CurrentBatch.Count())
            {
                _currentNEATIndex = 0;
                CurrentBatch = CurrentBatch.Where(x => x.Fitness >= 0.0).ToList();
                History.Concat(CurrentBatch);
                CurrentBatch = Genus.CrossoverAllSpecies(RandomManager.GetRandomInstance());
            }

            return CurrentNEAT;
        }

        public List<NEAT.NEAT> GetHallOfFame()
        {
            // Copy list
            HallOfFame = History.Where(x => true).ToList();
            // Sort descending
            HallOfFame.Sort((n1, n2) => -(n1.Fitness.CompareTo(n2.Fitness)));
            return HallOfFame;
        }

        #region Inputs / Outputs

        public void SetSensorValues(float paddleX, float paddleDX, float ballRelX, float ballRelY, float ballDX, float ballDY)
        {
            CurrentNEAT.SetSensorValues(paddleX, paddleDX, ballRelX, ballRelY, ballDX, ballDY, 1.0, _loopbackValue);
        }

        public void Activate()
        {
            if (!CurrentNEAT.Activate())
            {
                // TODO should it be 0? I think invalid NEATs should probably be eliminated. -1 would make sense.
                NextNEAT(-1.0);
                return;
            }

            _loopbackValue = CurrentNEAT.GetOutputValues()[(int)Output.Loopback];
        }

        public List<double> GetOutputs()
        {
            return CurrentNEAT.GetOutputValues();
        }

        #endregion
    }
}
