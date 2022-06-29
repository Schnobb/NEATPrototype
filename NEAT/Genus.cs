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

        public Species AddToSpecies(Genome genome)
        {
            foreach (var species in Species)
            {
                if (species.IsCompatible(genome))
                {
                    species.Members.Add(genome);
                    return species;
                }
            }

            var newSpecies = new Species(this, genome);
            Species.Add(newSpecies);
            return newSpecies;
        }

        public List<Genome> GenerateNewBatch(Random random)
        {
            var newBatch = new List<Genome>();
            for (int i = 0; i < BatchSize; i++)
            {
                var genome = new Genome(this);
                genome.MutationRandom(random, GenerateMaxMutations);
                genome.Species = AddToSpecies(genome);
                newBatch.Add(genome);
            }

            return newBatch;
        }

        public List<Genome> CrossoverSpecies(Random random, Species species)
        {
            // TODO figure out what we'll cross together
            // TODO create BatchSize offsprings. NO -> that should be handled in CrossoverAllSpecies probably
            // TODO remove all NEATs that are not yet evaluated (fitness < 0)
            // TODO elect a superchamp from the top and clone it a couple times, keep one original and mutate the other clones
            // TODO cross top x together and mutate
            // TODO what to do when species is not big enough? clone and mutate?
            // TODO AddToSpecies()
            // TODO assign species and parent(s) species to Genome
            throw new NotImplementedException();
        }

        public List<Genome> CrossoverAllSpecies(Random random)
        {
            // TODO chance to crossover across species
            var newBatch = new List<Genome>();
            foreach (var species in Species)
                newBatch.Concat(CrossoverSpecies(random, species));

            return newBatch;
        }
    }
}
