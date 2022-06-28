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
        public static int LatestInnovation = 0;

        public int MAX_ACTIVATE_TRIES = 20;

        public double MaxWeight { get; set; } = 4.0;
        public double MaxWeightShift { get; set; } = 0.5;

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

        public NEAT(Genus genus) : this(genus.SensorCount, genus.OutputCount) { }

        /// <summary>
        /// Create a new NEAT with the same Genome.
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
            // TODO move all the innovation/connection mapping and alignment stuff in CompareGenome()
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

        #region Genome Comparison

        public struct GenomeComparisonResult
        {
            public SortedSet<int> Innovations;

            public Dictionary<int, Connection> InnovationConnectionLookupA;
            public Dictionary<int, Connection> InnovationConnectionLookupB;

            public List<int> ExcessInnovations;
            public List<int> DisjointInnovations;
            public List<int> SharedInnovations;
            public double WeightAverageDifference;

            public GenomeComparisonResult()
            {
                Innovations = new SortedSet<int>();

                InnovationConnectionLookupA = new Dictionary<int, Connection>();
                InnovationConnectionLookupB = new Dictionary<int, Connection>();

                ExcessInnovations = new List<int>();
                DisjointInnovations = new List<int>();
                SharedInnovations = new List<int>();
                WeightAverageDifference = 0.0;
            }
        }

        public GenomeComparisonResult CompareGenome(NEAT other)
        {
            // TODO go through both genomes and fill a new GenomeComparisonResult
            throw new NotImplementedException();
        }

        #endregion

        #region Mutations

        public void MutationRandom(Random random, int maxMutations = 1)
        {
            // TODO redo the probabilities, make them configurable
            // TODO Topological mutations
            // TODO Weights mutations
            // TODO Reenable mutation
            // TODO Toggle enable mutation
            // TODO make mutations more frequent on newer genes

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
            // TODO with time step activation this might not be an issue anymore, this rule can now be removed
            for (int i = SensorCount + OutputCount; i < Nodes.Count; i++)
            {
                for (int j = SensorCount; j < Nodes.Count; j++)
                {
                    if (i == j)
                        continue;

                    //var sourceNode = Nodes[i];
                    //var targetNode = Nodes[j];

                    //if (targetNode.Type == NodeType.Hidden)
                    //{
                    //    // To make sure we don't have any loops in the network hidden nodes can only connect to other hidden nodes where source_id < target_id.
                    //    // This is a bit limiting but I don't think it's a huge deal.
                    //    if (sourceNode.ID < targetNode.ID)
                    //        potentialConnections.Add(new Tuple<int, int>(i, j));
                    //}
                    //else
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
            var innov = LatestInnovation++;

            Connections.Add(new Connection(Nodes[chosenConnection.Item1], Nodes[chosenConnection.Item2], weigth, true, innov));
            RecalculateReverseLookupTable();
        }

        public void MutateAddNode(Random random)
        {
            var newNode = new Node(Nodes.Count, NodeType.Hidden);
            Nodes.Add(newNode);

            var chosenConnection = Connections[random.Next(Connections.Count)];
            chosenConnection.Enabled = false;

            var newConnection1 = new Connection(chosenConnection.Source, newNode, chosenConnection.Weight, true, LatestInnovation++);
            var newConnection2 = new Connection(newNode, chosenConnection.Target, 1.0, true, LatestInnovation++);

            Connections.Add(newConnection1);
            Connections.Add(newConnection2);

            RecalculateReverseLookupTable();
        }
        public void MutateWeight(Random random)
        {
            // TODO use gaussian distribution
            var weight = random.NextDouble() * MaxWeight * 2.0 - MaxWeight;
            Connections[random.Next(Connections.Count)].Weight = weight;

            RecalculateReverseLookupTable();
        }

        public void MutateShiftWeight(Random random)
        {
            // TODO use gaussian distribution
            var weightShift = random.NextDouble() * MaxWeightShift * 2.0 - MaxWeightShift;
            var con = Connections[random.Next(Connections.Count)];
            var newWeight = Math.Clamp(con.Weight + weightShift, -MaxWeight, MaxWeight);
            con.Weight = newWeight;

            RecalculateReverseLookupTable();
        }

        #endregion

        #region Inputs/Outputs

        /// <summary>
        /// This function compute one time step of the network.
        /// </summary>
        /// <returns><c>true</c> if activation was possible, <c>false</c> if no valid connections between sensors and outputs.</returns>
        public bool Activate()
        {
            //if (Connections.Count <= 0)
            //    return;

            //// Hidden layer first
            //// This works because connections between hidden nodes can only be source_id < target_id
            //for (int i = SensorCount + OutputCount; i < Nodes.Count; i++)
            //    Nodes[i].Value = ComputeNode(i);

            //// Output layer second
            //for (int i = SensorCount; i < SensorCount + OutputCount; i++)
            //    Nodes[i].Value = ComputeNode(i);

            var activateTries = 0;
            var alreadyActivated = false;
            while (!AtLeastOneOutputActivated())
            {
                activateTries++;
                if (activateTries >= MAX_ACTIVATE_TRIES)
                    return false;

                ActivateInternal();
                alreadyActivated = true;
            }

            if (!alreadyActivated)
                ActivateInternal();
            return true;
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
            return Math.Tanh(x*2);
        }

        private void ActivateInternal()
        {
            for (int nodeIndex = SensorCount; nodeIndex < Nodes.Count; nodeIndex++)
            {
                var node = Nodes[nodeIndex];
                var connections = _reverseLookupTable[node.ID];

                if (node.Activated || connections.Where(connection => connection.Source.Activated).Any())
                {
                    node.Activated = true;
                    node.OldValue = node.Value;

                    var value = 0.0;
                    foreach (var connection in connections)
                        value += connection.Source.Activated ? connection.Source.Value * connection.Weight : 0.0;

                    node.Value = Sigmoid(value);
                }
            }
        }

        private bool AtLeastOneOutputActivated()
        {
            for (int nodeIndex = SensorCount; nodeIndex < SensorCount + OutputCount; nodeIndex++)
                if (Nodes[nodeIndex].Activated)
                    return true;

            return false;
        }

        #endregion
    }
}
