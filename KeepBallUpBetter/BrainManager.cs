using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeepBallUpBetter
{
    internal class BrainManager
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
        public List<NEAT.Genome> CurrentBatch { get; private set; }
        public int CurrentGenomeIndex { get; private set; }
        public NEAT.Genome CurrentGenome { get { return CurrentBatch[CurrentGenomeIndex]; } }

        public List<NEAT.Genome> History { get; private set; }
        public List<NEAT.Genome> HallOfFame { get; private set; }

        private double _loopbackValue;

        public BrainManager()
        {
            Genus = new NEAT.Genus(BatchSize, Enum.GetNames(typeof(Sensor)).Length, Enum.GetNames(typeof(Output)).Length);
            CurrentBatch = Genus.GenerateNewBatch(RandomManager.GetRandomInstance());
            CurrentGenomeIndex = 0;
            History = new List<NEAT.Genome>();
        }

        public NEAT.Genome NextGenome(double fitness)
        {
            CurrentGenome.Fitness = fitness;
            _loopbackValue = 0;
            CurrentGenomeIndex++;

            if (CurrentGenomeIndex >= CurrentBatch.Count())
            {
                CurrentGenomeIndex = 0;
                CurrentBatch = CurrentBatch.Where(x => x.Fitness >= 0.0).ToList();
                History.Concat(CurrentBatch);
                CurrentBatch = Genus.CrossoverAllSpecies(RandomManager.GetRandomInstance());
            }

            return CurrentGenome;
        }

        public List<NEAT.Genome> GetHallOfFame()
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
            CurrentGenome.SetSensorValues(paddleX, paddleDX, ballRelX, ballRelY, ballDX, ballDY, 1.0, _loopbackValue);
        }

        public void Activate()
        {
            if (!CurrentGenome.Activate())
            {
                // TODO should it be 0? I think invalid NEATs should probably be eliminated. -1 would make sense.
                NextGenome(-1.0);
                return;
            }

            _loopbackValue = CurrentGenome.GetOutputValues()[(int)Output.Loopback];
        }

        public List<double> GetOutputs()
        {
            return CurrentGenome.GetOutputValues();
        }

        #endregion
    }
}
