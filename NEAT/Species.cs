using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEAT
{
    public class Species
    {
        // Compatibility coefficients and threshold
        public double C1 { get; set; } = 1.0;               // always 1.0 in original implementation
        public double C2 { get; set; } = 1.0;               // always 1.0 in original implementation
        public double C3 { get; set; } = 3.0;               // between 0.4 and 7.0 in original implementation
        public double DeltaThreshold { get; set; } = 4.0;   // between 3.0 and 9.0 in original implementation

        public Genus Genus { get; set; }
        public List<Genome> Members { get; private set; }

        public Genome OriginalGenome { get { return Members[0]; } }
        public double TopFitness { get { return Members.Max(x => x.Fitness); } }
        public double AverageFitness { get { return Members.Average(x => x.Fitness); } }

        public Species()
        {
            Members = new List<Genome>();
        }

        public Species(Genus genus)
        {
            Members = new List<Genome>();
            Genus = genus;
        }

        public Species(Genus genus, Genome genome)
        {
            Members = new List<Genome> { genome };
            Genus = genus;
        }

        public bool IsCompatible(Genome genome)
        {
            // delta = (c1*E)/N + (c2*D)/N + c3*WAvg
            //      c1, c2, c3 configurable coeficients
            //      N is the number of genes in the larger genome
            //      E is the number of excess genes
            //      D is the number of disjoint genes
            //      WAvg is the average differences of weight for shared genes
            // If delta > deltaThreshold species is not compatible

            // TODO Look into adjusted fitness using species deltas, this is more complex

            var genomeCompareResults = OriginalGenome.CompareGenes(genome);

            var e = genomeCompareResults.ExcessInnovations.Count();
            var d = genomeCompareResults.DisjointInnovations.Count();
            //var m = genomeCompareResults.MatchingInnovations.Count();
            var wavg = genomeCompareResults.WeightAverageDifference;

            // In the paper N is mentionned to be the size of the larger genome and is used to divide the E and D factors in the formula.
            // This is supposed to normalize for genome size, however in the actual implementation it is not used.
            //var n = Math.Max(genomeCompareResults.InnovationConnectionLookupA.Count(), genomeCompareResults.InnovationConnectionLookupB.Count());

            //return (C1 * e + C2 * d) / n + C3 * wavg < DeltaThreshold;
            return C1 * e + C2 * d + C3 * wavg < DeltaThreshold;
        }
    }
}
