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
        public List<NEAT> Members { get; private set; }

        public NEAT OriginalNEAT
        {
            get
            {
                return Members[0];
            }
        }

        public double TopFitness
        {
            get
            {
                return Members.Max(x => x.Fitness);
            }
        }

        public double AverageFitness
        {
            get
            {
                return Members.Average(x => x.Fitness);
            }
        }

        public Species()
        {
            Members = new List<NEAT>();
        }

        public Species(Genus genus)
        {
            Members = new List<NEAT>();
            Genus = genus;
        }

        public Species(Genus genus, NEAT neat)
        {
            Members = new List<NEAT> { neat };
            Genus = genus;
        }

        public bool IsCompatible(NEAT neat)
        {
            // TODO compatibility
            // delta = (c1*E)/N + (c2*D)/N + c3*WAvg
            //      c1, c2, c3 configurable coeficients
            //      N is the number of genes in the larger genome
            //      E is the number of excess genes
            //      D is the number of disjoint genes
            //      WAvg is the average differences of weight for shared genes
            // If delta > deltaThreshold for all species create a new species

            // TODO Look into adjusted fitness using species deltas, this is more complex

            throw new NotImplementedException();
        }
    }
}
