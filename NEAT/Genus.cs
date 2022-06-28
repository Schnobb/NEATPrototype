using System;
using System.Collections.Generic;
using System.Text;

namespace NEAT
{
    // Cute name for a SpeciesManager lol
    public class Genus
    {
        public int GenerateMaxMutations { get; set; } = 10;

        public List<Species> Species { get; set; }

        public int SensorCount { get; set; }
        public int OutputCount { get; set; }

        public Genus() : this(0, 0) { }

        public Genus(int sensorCount, int outputCount)
        {
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

        public void Generate(Random random, int neatCount = 1)
        {
            for (int i = 0; i < neatCount; i++)
            {
                var neat = new NEAT(this);
                neat.MutationRandom(random, GenerateMaxMutations);
                AddToSpecies(neat);
            }
        }

        public void CrossoverSpecies(Species species, int top = 3, int offspringPerCross = 3)
        {
            // TODO elect a superchamp from the top and clone it a couple times, keep one original and mutate the other clones
            // TODO cross top x together and mutate
            // TODO what to do when species is not big enough? clone and mutate?
            // TODO AddToSpecies()
            throw new NotImplementedException();
        }

        public void CrossoverAllSpecies(int top = 3, int offspringPerCross = 3)
        {
            // TODO chance to crossover across species
            foreach (var species in Species)
                CrossoverSpecies(species, top, offspringPerCross);
        }
    }
}
