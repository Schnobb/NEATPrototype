using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NEAT
{
    // Cute name for a SpeciesManager lol
    public class Genus
    {
        public int GenerateMaxMutations { get; set; } = 10;

        public List<Species> Species { get; set; }

        public int BatchSize { get; set; }
        public int SensorCount { get; set; }
        public int OutputCount { get; set; }

        public Genus() : this(0, 0, 0) { }

        public Genus(int batchSize, int sensorCount, int outputCount)
        {
            BatchSize = batchSize;
            SensorCount = sensorCount;
            OutputCount = outputCount;
            Species = new List<Species>();
        }

        public void AddToSpecies(NEAT neat)
        {
            foreach (var species in Species)
            {
                if (species.IsCompatible(neat))
                {
                    species.Members.Add(neat);
                    return;
                }
            }

            Species.Add(new Species(this, neat));
        }

        public List<NEAT> GenerateNewBatch(Random random)
        {
            var newBatch = new List<NEAT>();
            for (int i = 0; i < BatchSize; i++)
            {
                var neat = new NEAT(this);
                neat.MutationRandom(random, GenerateMaxMutations);
                AddToSpecies(neat);
                newBatch.Add(neat);
            }

            return newBatch;
        }

        public List<NEAT> CrossoverSpecies(Random random, Species species, int top = 3, int offspringPerCross = 3)
        {
            // TODO instead of offspringPerCross it should be based on BatchSize
            // TODO remove all NEATs that are not yet evaluated (fitness < 0)
            // TODO elect a superchamp from the top and clone it a couple times, keep one original and mutate the other clones
            // TODO cross top x together and mutate
            // TODO what to do when species is not big enough? clone and mutate?
            // TODO AddToSpecies()
            throw new NotImplementedException();
        }

        public List<NEAT> CrossoverAllSpecies(Random random, int top = 3, int offspringPerCross = 3)
        {
            // TODO chance to crossover across species
            var newBatch = new List<NEAT>();
            foreach (var species in Species)
                newBatch.Concat(CrossoverSpecies(random, species, top, offspringPerCross));

            return newBatch;
        }
    }
}
