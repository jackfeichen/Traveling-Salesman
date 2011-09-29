using System;
using System.Collections.Generic;
using System.Linq;

namespace Radex.Tsp
{
    public class Route : List<int>
    {
        public bool IsCycle { get; private set; }
        public double Fitness { get; set; }
        public Random Randomizer { get; set; }

        public Route(IEnumerable<int> nodes, bool isCycle) :
            this(nodes.Count(), isCycle)
        {
            this.AddRange(nodes);
        }
        public Route(int capacity) : 
            this(capacity, true, new Random((int)DateTime.Now.ToBinary())) { }
        public Route(int capacity, bool isCycle) : 
            this(capacity, isCycle, new Random((int)DateTime.Now.ToBinary())) { }
        public Route(int capacity, bool isCycle, Random randomizer)
        {
            Fitness = -1;
            Capacity = capacity;
            Randomizer = randomizer;
            IsCycle = isCycle;
        }

        public void DetermineFitness(NodeList nodeList)
        {
            Fitness = 0;
            for (var i = 0; i < Count - 1; i++)
            {
                var thisNode = this[i];
                var nextNode = this[i + 1];
                Fitness += nodeList[thisNode].Ratings[nextNode];
            }
            if(IsCycle)
            {
                var thisNode = this[Count - 1];
                var nextNode = this[0];
                Fitness += nodeList[thisNode].Ratings[nextNode];
            }
        }

        public bool AddNode(Node node)
        {
            if (Count == Capacity)
                return false;
            Add(node.Id);
            return true;
        }

        public bool AddNode(int nodeIndex)
        {
            if (Count == Capacity)
                return false;
            Add(nodeIndex);
            return true;
        }
    }
}
