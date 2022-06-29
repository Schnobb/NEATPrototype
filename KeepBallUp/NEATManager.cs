using NEAT;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KeepBallUp
{
    class NEATManager
    {
        public int CurrentIndex;
        public List<Genome> CurrentNEATs;
        public SortedList<double, Genome> DeadNEATs;
        public int BatchSize;

        public int SensorCount;
        public int OutputCount;

        public int NewNEATMutationCount = 30;
        public int ExistingNEATMutationCount = 5;

        private Random _random;

        /// <summary>
        /// Create a new NEATManager.
        /// </summary>
        /// <param name="batchSize">Should be > 6.</param>
        public NEATManager(Random random, int sensorCount, int outputCount, int batchSize)
        {
            if (batchSize < 7)
                throw new Exception("batchSize should be > 6");

            CurrentNEATs = new List<Genome>();
            DeadNEATs = new SortedList<double, Genome>(new DuplicateKeyComparer<double>());
            SensorCount = sensorCount;
            OutputCount = outputCount;
            BatchSize = batchSize;
            _random = random;

            CurrentIndex = 0;
            GenerateNewBatch();
        }

        public Genome GetCurrentNEAT()
        {
            return CurrentNEATs[CurrentIndex];
        }

        public void NextNEAT()
        {
            CurrentIndex += 1;

            if (CurrentIndex >= BatchSize)
            {
                CurrentIndex = 0;
                GenerateNewBatch();
            }
        }

        public void GenerateNewBatch()
        {
            var topAllTime = new List<Genome>();
            var topLastBatch = new List<Genome>();
            var newBatch = new List<Genome>();

            if (DeadNEATs.Count > 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var newNEAT = DeadNEATs.Values[DeadNEATs.Count - 1 - i].CopyGenes();
                        newNEAT.MutationRandom(_random, ExistingNEATMutationCount);
                        topAllTime.Add(newNEAT);
                    }
                }
            }

            var rankedCurrentBatch = RetireNEATs();
            if (rankedCurrentBatch.Count > 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var newNEAT = rankedCurrentBatch.Values[rankedCurrentBatch.Count - 1 - i].CopyGenes();
                        newNEAT.MutationRandom(_random, ExistingNEATMutationCount);
                        topLastBatch.Add(newNEAT);
                    }
                }
            }

            for (int i = 0; i < BatchSize - topAllTime.Count - topLastBatch.Count; i++)
            {
                var newNEAT = new Genome(SensorCount, OutputCount);
                newNEAT.MutationRandom(_random, NewNEATMutationCount);
                newBatch.Add(newNEAT);
            }

            CurrentNEATs = newBatch.Concat(topLastBatch.Concat(topAllTime)).ToList();
        }

        private SortedList<double, Genome> RetireNEATs()
        {
            var rankedCurrentBatch = new SortedList<double, Genome>(new DuplicateKeyComparer<double>());
            foreach (var neat in CurrentNEATs)
            {
                DeadNEATs.Add(neat.Fitness, neat);
                rankedCurrentBatch.Add(neat.Fitness, neat);
            }

            CurrentNEATs.Clear();
            return rankedCurrentBatch;
        }
    }

    /// <summary>
    /// Comparer for comparing two keys, handling equality as beeing greater.
    /// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    class DuplicateKeyComparer<TKey>
                    :
                 IComparer<TKey> where TKey : IComparable
    {
        #region IComparer<TKey> Members

        public int Compare(TKey x, TKey y)
        {
            int result = x.CompareTo(y);

            if (result == 0)
                return 1; // Handle equality as being greater. Note: this will break Remove(key) or
            else          // IndexOfKey(key) since the comparer never returns 0 to signal key equality
                return result;
        }

        #endregion
    }
}
