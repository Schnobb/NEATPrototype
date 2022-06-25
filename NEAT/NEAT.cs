using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEAT
{
    public class NEAT
    {
        // This is a global counter of new genes (connections)
        // Innovation is used in crossover gene alignment
        public static int LatestInnovation;

        // TODO: these should be configurable
        public const double DEFAULT_MAX_WEIGHT = 4.0;
        public const double DEFAULT_MAX_WEIGHT_SHIFT = 0.5;
        public const double DEFAULT_C1 = 1.0;
        public const double DEFAULT_C2 = 1.0;
        public const double DEFAULT_C3 = 1.0;
        public const double DEFAULT_DELTA_THRESHOLD = 1.0;

        public int SensorCount;
        public int OutputCount;

        // GENOME
        public List<Node> Nodes;
        public List<Connection> Connections;
        // ---

        public double Fitness;

        private Dictionary<int, List<Connection>> _reverseLookupTable;

        #region Init

        public NEAT()
        {
            SensorCount = 0;
            OutputCount = 0;
            Fitness = 0;

            Nodes = new List<Node>();
            Connections = new List<Connection>();
            _reverseLookupTable = new Dictionary<int, List<Connection>>();
        }

        public NEAT(int sensorCount, int outputCount)
        {
            SensorCount = sensorCount;
            OutputCount = outputCount;
            Fitness = 0;

            Nodes = new List<Node>();
            Connections = new List<Connection>();
            _reverseLookupTable = new Dictionary<int, List<Connection>>();

            for (int i = 0; i < SensorCount; i++)
                Nodes.Add(new Node(i, NodeType.Sensor));

            for (int i = SensorCount; i < SensorCount + OutputCount; i++)
                Nodes.Add(new Node(i, NodeType.Output));
        }

        /// <summary>
        /// Create a new NEAT with the same Genome
        /// </summary>
        public NEAT CopyGenome()
        {
            var newNEAT = new NEAT
            {
                SensorCount = SensorCount,
                OutputCount = OutputCount
            };

            foreach (var node in Nodes)
                newNEAT.Nodes.Add(node);

            foreach (var connection in Connections)
                newNEAT.Connections.Add(connection);

            newNEAT.RecalculateReverseLookupTable();

            return newNEAT;
        }

        public NEAT Crossover(NEAT other)
        {
            // TODO Speciation using compatibility distance delta
            // delta = (c1*E)/N + (c2*D)/N + c3*WAvg
            //      c1, c2, c3 configurable coeficients
            //      N is the number of genes in the larger genome
            //      E is the number of excess genes
            //      D is the number of disjoint genes
            //      WAvg is the average differences of weight for shared genes
            // setup a deltaThreshold. If delta > deltaThreshold crossover is not compatible
            // this will require the crossover function to return a success flag instead of a NEAT

            // TODO Look into adjusted fitness using species deltas, this is more complex

            var newConnections = new List<Connection>();
            var newNodes = new HashSet<Node>();

            foreach (var node in Nodes)
                if (node.Type != NodeType.Hidden)
                    newNodes.Add(node);

            var innovationListAll = new SortedSet<int>();
            var innovationDictSelf = new Dictionary<int, Connection>();
            var innovationDictOther = new Dictionary<int, Connection>();

            int maxInnovSelf = -1;
            int maxInnovOther = -1;

            foreach (var connection in Connections)
            {
                innovationListAll.Add(connection.Innovation);
                innovationDictSelf.Add(connection.Innovation, connection);
                if (connection.Innovation > maxInnovSelf)
                    maxInnovSelf = connection.Innovation;
            }

            foreach (var connection in other.Connections)
            {
                innovationListAll.Add(connection.Innovation);
                innovationDictOther.Add(connection.Innovation, connection);
                if (connection.Innovation > maxInnovOther)
                    maxInnovOther = connection.Innovation;
            }

            // TODO newConnections is populated here
            foreach (var innovation in innovationListAll)
            {
                if (innovation > maxInnovSelf)
                {
                    // TODO excess genes, check fitness
                }
                else if (innovation > maxInnovOther)
                {
                    // TODO excess genes, check fitness
                }
                else if (innovationDictSelf.ContainsKey(innovation) && innovationDictOther.ContainsKey(innovation))
                {
                    // TODO shared genes, check fitness
                }
                else
                {
                    // TODO disjoint genes, check fitness
                }
            }

            // Add hidden nodes to newNodes
            foreach (var connection in newConnections)
            {
                if (connection.Source.Type == NodeType.Hidden)
                    newNodes.Add(connection.Source);
                if (connection.Target.Type == NodeType.Hidden)
                    newNodes.Add(connection.Target);
            }

            var newNEAT = new NEAT()
            {
                SensorCount = SensorCount,
                OutputCount = OutputCount,
                Nodes = newNodes.ToList(),
                Connections = newConnections
            };

            newNEAT.RecalculateReverseLookupTable();
            return newNEAT;
        }

        #endregion

        #region Mutations

        public void MutationRandom(Random random, int maxMutations = 1)
        {
            for (int i = 0; i < maxMutations; i++)
            {
                if (Connections.Count > 0)
                {
                    var rand = random.NextDouble();

                    if (rand < 0.45)
                        MutateAddConnection(random);
                    else if (rand < 0.9)
                        MutateShiftWeight(random);
                    else if (rand < 0.98)
                        MutateWeight(random);
                    else
                        MutateAddNode(random);
                }
                else
                {
                    MutateAddConnection(random);
                }
            }

            RecalculateReverseLookupTable();
        }

        public void MutateAddConnection(Random random)
        {
            List<Tuple<int, int>> potentialConnections = new List<Tuple<int, int>>();

            // Sensors
            //  Can form connections to hidden layer nodes or output layer nodes with no restrictions
            for (int i = 0; i < SensorCount; i++)
                for (int j = SensorCount; j < Nodes.Count; j++)
                    potentialConnections.Add(new Tuple<int, int>(i, j));

            // Hidden nodes
            //  Can form connections to output layer nodes without restrictions and can form connections to hidden layer nodes as long as source_id < target_id
            for (int i = SensorCount + OutputCount; i < Nodes.Count; i++)
            {
                for (int j = SensorCount; j < Nodes.Count; j++)
                {
                    if (i == j)
                        continue;

                    var sourceNode = Nodes[i];
                    var targetNode = Nodes[j];

                    if (targetNode.Type == NodeType.Hidden)
                    {
                        // To make sure we don't have any loops in the network hidden nodes can only connect to other hidden nodes where source_id < target_id.
                        // This is a bit limiting but I don't think it's a huge deal.
                        if (sourceNode.ID < targetNode.ID)
                            potentialConnections.Add(new Tuple<int, int>(i, j));
                    }
                    else
                        potentialConnections.Add(new Tuple<int, int>(i, j));
                }
            }

            // Remove potential connections that already exist
            var potentialIndexesToRemove = new List<int>();
            for (int i = 0; i < potentialConnections.Count; i++)
                foreach (var con in Connections)
                    if (con.Equivalent(potentialConnections[i].Item1, potentialConnections[i].Item2))
                        potentialIndexesToRemove.Add(i);

            for (int i = potentialIndexesToRemove.Count - 1; i >= 0; i--)
                potentialConnections.RemoveAt(i);

            if (potentialConnections.Count <= 0)
                return;

            var chosenConnection = potentialConnections[random.Next(0, potentialConnections.Count)];
            var weigth = 1.0;
            var innov = LatestInnovation;
            LatestInnovation++;

            Connections.Add(new Connection(Nodes[chosenConnection.Item1], Nodes[chosenConnection.Item2], weigth, true, innov));
            RecalculateReverseLookupTable();
        }

        public void MutateAddNode(Random random)
        {
            var newNode = new Node(Nodes.Count, NodeType.Hidden);
            Nodes.Add(newNode);

            var chosenConnection = Connections[random.Next(Connections.Count)];
            chosenConnection.Enabled = false;

            var newConnection1 = new Connection(chosenConnection.Source, newNode, chosenConnection.Weight, true, LatestInnovation);
            LatestInnovation++;
            var newConnection2 = new Connection(newNode, chosenConnection.Target, 1.0, true, LatestInnovation);
            LatestInnovation++;

            Connections.Add(newConnection1);
            Connections.Add(newConnection2);

            RecalculateReverseLookupTable();
        }
        public void MutateWeight(Random random)
        {
            var weight = random.NextDouble() * DEFAULT_MAX_WEIGHT * 2.0 - DEFAULT_MAX_WEIGHT;
            Connections[random.Next(Connections.Count)].Weight = weight;

            RecalculateReverseLookupTable();
        }

        public void MutateShiftWeight(Random random)
        {
            var weightShift = random.NextDouble() * DEFAULT_MAX_WEIGHT_SHIFT * 2.0 - DEFAULT_MAX_WEIGHT_SHIFT;
            var con = Connections[random.Next(Connections.Count)];
            var newWeight = Math.Clamp(con.Weight + weightShift, -DEFAULT_MAX_WEIGHT, DEFAULT_MAX_WEIGHT);
            con.Weight = newWeight;

            RecalculateReverseLookupTable();
        }

        #endregion

        #region Inputs/Outputs

        /// <summary>
        /// This function computes and set all non-sensor node values.
        /// </summary>
        public void ComputeValues()
        {
            if (Connections.Count <= 0)
                return;

            // Hidden layer first
            // This works because connections between hidden nodes can only be source_id < target_id
            for (int i = SensorCount + OutputCount; i < Nodes.Count; i++)
                Nodes[i].Value = ComputeNode(i);

            // Output layer second
            for (int i = SensorCount; i < SensorCount + OutputCount; i++)
                Nodes[i].Value = ComputeNode(i);
        }

        public void SetSensorValues(params double[] values)
        {
            if (values.Length != SensorCount)
                throw new ArgumentException("values length should be equal to SensorCount.");

            for (var i = 0; i < SensorCount; i++)
                Nodes[i].Value = values[i];
        }

        public List<double> GetOutputValues()
        {
            var values = new List<double>();
            for (int i = SensorCount; i < SensorCount + OutputCount; i++)
                values.Add(Nodes[i].Value);

            return values;
        }

        #endregion

        #region Privates

        private void RecalculateReverseLookupTable()
        {
            _reverseLookupTable = new Dictionary<int, List<Connection>>();

            if (Connections.Count <= 0)
                return;

            for (int i = SensorCount; i < Nodes.Count; i++)
            {
                var currentNode = Nodes[i];
                var currentNodeConnections = new List<Connection>();

                foreach (var connection in Connections)
                    if (connection.Enabled && connection.Target.ID == currentNode.ID)
                        currentNodeConnections.Add(connection);

                _reverseLookupTable.Add(currentNode.ID, currentNodeConnections);
            }
        }

        private double ComputeNode(int nodeIndex)
        {
            var currentNode = Nodes[nodeIndex];
            double value = 0.0;

            foreach (var connection in _reverseLookupTable[currentNode.ID])
                value += connection.Source.Value * connection.Weight;

            return Sigmoid(value);
        }

        private double Sigmoid(double x)
        {
            //var res = 1.0f / (1.0f + Math.Exp(-x));
            //return res * 2.0f - 1.0f;
            return Math.Tanh(x);
        }

        #endregion
    }
}
