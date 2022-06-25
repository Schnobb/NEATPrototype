using System;
using System.Collections.Generic;
using System.Text;

namespace NEAT
{
    public class Connection
    {
        public Node Source;
        public Node Target;
        public double Weight;
        public bool Enabled;
        public int Innovation;

        public Connection()
        {
            Source = new Node();
            Target = new Node();
            Weight = 0;
            Enabled = false;
            Innovation = -1;
        }

        public Connection(Node source, Node target, double weight, bool enabled, int innovation)
        {
            Source = source;
            Target = target;
            Weight = weight;
            Enabled = enabled;
            Innovation = innovation;
        }

        public bool Equivalent(Connection other)
        {
            return Source.ID == other.Source.ID && Target.ID == other.Target.ID;
        }

        public bool Equivalent(int otherSourceID, int otherTargetID)
        {
            return Source.ID == otherSourceID && Target.ID == otherTargetID;
        }

        public override string ToString()
        {
            return $"{Source.ID} -> {Target.ID} [{Weight}]";
        }
    }
}
