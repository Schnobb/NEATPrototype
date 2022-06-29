using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEAT
{
    public class Genome
    {
        private const int MAX_ACTIVATE_TRIES = 20;

        // This is a global counter of new genes (connections)
        // Innovation is used in crossover gene alignment
        public static int LatestInnovation = 0;

        public double MaxWeight { get; set; } = 4.0;
        public double MaxWeightShift { get; set; } = 0.5;
        public double SigmoidCoefficient { get; set; } = 2.0;

        public int SensorCount { get; set; }
        public int OutputCount { get; set; }

        #region Genes
        // TODO traits?
        public List<Node> Nodes { get; set; }
        public List<Connection> Connections { get; set; }
        #endregion

        #region Genealogy shit
        public Species Species { get; set; }
        public List<Species> ParentsSpecies { get; set; }
        #endregion

        public double Fitness { get; set; }

        private Dictionary<int, List<Connection>> _reverseLookupTable;

        #region Init

        public Genome(int sensorCount, int outputCount)
        {
            SensorCount = sensorCount;
            OutputCount = outputCount;
            Fitness = -1.0;

            Nodes = new List<Node>();
            Connections = new List<Connection>();
            ParentsSpecies = new List<Species>();
            _reverseLookupTable = new Dictionary<int, List<Connection>>();

            for (int i = 0; i < SensorCount; i++)
                Nodes.Add(new Node(i, NodeType.Sensor));

            for (int i = SensorCount; i < SensorCount + OutputCount; i++)
                Nodes.Add(new Node(i, NodeType.Output));
        }

        public Genome(Genus genus) : this(genus.SensorCount, genus.OutputCount) { }
        public Genome() : this(0, 0) { }

        /// <summary>
        /// Create a new Genome with the same genes.
        /// </summary>
        public Genome CopyGenes()
        {
            var newGenome = new Genome
            {
                SensorCount = SensorCount,
                OutputCount = OutputCount
            };

            foreach (var node in Nodes)
                newGenome.Nodes.Add(new Node(node));

            foreach (var connection in Connections)
                newGenome.Connections.Add(connection);

            newGenome.RecalculateReverseLookupTable();

            return newGenome;
        }

        #endregion

        #region Genome Comparison

        public struct GenomeComparisonResult
        {
            public Dictionary<int, Connection> InnovationConnectionLookupA;
            public Dictionary<int, Connection> InnovationConnectionLookupB;

            public List<int> ExcessInnovations;
            public List<int> DisjointInnovations;
            public List<int> MatchingInnovations;
            public double WeightAverageDifference;

            public GenomeComparisonResult()
            {
                InnovationConnectionLookupA = new Dictionary<int, Connection>();
                InnovationConnectionLookupB = new Dictionary<int, Connection>();

                ExcessInnovations = new List<int>();
                DisjointInnovations = new List<int>();
                MatchingInnovations = new List<int>();
                WeightAverageDifference = 0.0;
            }
        }

        public GenomeComparisonResult CompareGenes(Genome other)
        {
            var results = new GenomeComparisonResult();

            var innovationListAll = new SortedSet<int>();

            int maxInnovSelf = -1;
            int maxInnovOther = -1;

            foreach (var connection in Connections)
            {
                innovationListAll.Add(connection.Innovation);
                results.InnovationConnectionLookupA.Add(connection.Innovation, connection);
                if (connection.Innovation > maxInnovSelf)
                    maxInnovSelf = connection.Innovation;
            }

            foreach (var connection in other.Connections)
            {
                innovationListAll.Add(connection.Innovation);
                results.InnovationConnectionLookupB.Add(connection.Innovation, connection);
                if (connection.Innovation > maxInnovOther)
                    maxInnovOther = connection.Innovation;
            }

            foreach (var innovation in innovationListAll)
            {
                if (innovation > maxInnovSelf || innovation > maxInnovOther)
                    results.ExcessInnovations.Add(innovation);
                else if (results.InnovationConnectionLookupA.ContainsKey(innovation) && results.InnovationConnectionLookupB.ContainsKey(innovation))
                    results.MatchingInnovations.Add(innovation);
                else
                    results.DisjointInnovations.Add(innovation);
            }

            return results;
        }

        public Genome Crossover(Random random, Genome other)
        {
            var newConnections = new List<Connection>();
            var newNodes = new HashSet<Node>();

            foreach (var node in Nodes)
                if (node.Type != NodeType.Hidden)
                    newNodes.Add(new Node(node));

            var compareResults = CompareGenes(other);

            foreach (var innovation in compareResults.MatchingInnovations)
            {
                // Matching genes are taken from either parents randomly
                var potentialGeneA = compareResults.InnovationConnectionLookupA[innovation];
                var potentialGeneB = compareResults.InnovationConnectionLookupB[innovation];
                newConnections.Add(Util.Choose(random, potentialGeneA, potentialGeneB));
            }

            foreach (var innovation in compareResults.DisjointInnovations.Concat(compareResults.ExcessInnovations))
            {
                // Disjoint and excess genes are taken from the fittest parent, if both parents have the same fitness choose at random for each genes
                compareResults.InnovationConnectionLookupA.TryGetValue(innovation, out Connection potentialGeneA);
                compareResults.InnovationConnectionLookupB.TryGetValue(innovation, out Connection potentialGeneB);

                if (Fitness > other.Fitness)
                {
                    if (potentialGeneA != null)
                        newConnections.Add(potentialGeneA);
                } 
                else if (Fitness < other.Fitness)
                {
                    if (potentialGeneB != null)
                        newConnections.Add(potentialGeneB);
                }
                else
                {
                    var connection = potentialGeneA ?? potentialGeneB;
                    if (random.NextDouble() < 0.5)
                        newConnections.Add(connection);
                }
            }

            // Add hidden nodes to newNodes
            foreach (var connection in newConnections)
            {
                if (connection.Source.Type == NodeType.Hidden)
                    newNodes.Add(new Node(connection.Source));
                if (connection.Target.Type == NodeType.Hidden)
                    newNodes.Add(new Node(connection.Target));
            }

            newConnections.Sort((c1, c2) => c1.Innovation.CompareTo(c2.Innovation));
            var newGenome = new Genome()
            {
                SensorCount = SensorCount,
                OutputCount = OutputCount,
                Nodes = newNodes.ToList(),
                Connections = newConnections
            };

            newGenome.RecalculateReverseLookupTable();
            return newGenome;
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
                        MutateNewWeight(random);
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

        public void MutateWeights(Random random)
        {
            // TODO go through all connections and either don't touch them, do a MutateShiftWeight(), or MutateNewWeight()
            throw new NotImplementedException();
        }

        public void MutateNewWeight(Random random, Connection connection = null)
        {
            if (connection == null)
                connection = Connections[random.Next(Connections.Count)];

            // TODO use gaussian distribution?
            var weight = random.NextDouble() * MaxWeight * 2.0 - MaxWeight;
            connection.Weight = weight;
        }

        public void MutateShiftWeight(Random random, Connection connection = null)
        {
            if (connection == null)
                connection = Connections[random.Next(Connections.Count)];

            // TODO use gaussian distribution?
            var weightShift = random.NextDouble() * MaxWeightShift * 2.0 - MaxWeightShift;
            var newWeight = Math.Clamp(connection.Weight + weightShift, -MaxWeight, MaxWeight);
            connection.Weight = newWeight;
        }

        #endregion

        #region Inputs/Outputs

        /// <summary>
        /// This function compute one time step of the network.
        /// </summary>
        /// <returns><c>true</c> if activation was possible, <c>false</c> if no valid connections between sensors and outputs.</returns>
        public bool Activate()
        {
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

        private double Sigmoid(double x)
        {
            //var res = 1.0f / (1.0f + Math.Exp(-x));
            //return res * 2.0f - 1.0f;
            return Math.Tanh(x * SigmoidCoefficient);
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
