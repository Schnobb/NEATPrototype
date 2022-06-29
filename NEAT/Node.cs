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
        public double OldValue;
        public bool Activated;

        public Node() : this(-1, NodeType.Hidden) { }
        public Node(Node node) : this(node.ID, node.Type) { }

        public Node(int id, NodeType type)
        {
            ID = id;
            Type = type;
            Value = 0.0;
            OldValue = 0.0;
            Activated = type == NodeType.Sensor;
        }

        public override string ToString()
        {
            return $"{ID}: {Value} [{Type}]";
        }
    }
}
