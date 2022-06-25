using System;
using System.Collections.Generic;
using System.Text;

namespace NEAT
{
    public enum NodeType
    {
        Sensor,
        Hidden,
        Output
    }

    public class Node
    {
        public int ID;
        public NodeType Type;
        public double Value;

        public Node()
        {
            ID = -1;
            Type = NodeType.Hidden;
            Value = 0.0;
        }

        public Node(int id, NodeType type)
        {
            ID = id;
            Type = type;
            Value = 0.0;
        }

        public override string ToString()
        {
            return $"{ID}: {Value} [{Type}]";
        }
    }
}
