using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;

namespace Radex.Tsp
{
    public class MessageEventArgs : EventArgs { public string Message { get; set; } }

    public enum AlgorithmType
    {
        GeneticAlgorithmByCutpoint,
        GeneticAlgorithmByClosest
    }

    public abstract class Algorithm : List<Route>, INotifyPropertyChanged
    {
        private Route _bestRoute;
        public Route BestRoute 
        {
            get
            {
                return _bestRoute;
            }
            set
            {
                _bestRoute = value;
                OnPropertyChanged("BestRoute");
            }
        }

        public abstract int Id { get; }
        public bool Stop { get; set; }

        public abstract void InitializePopulation(int populationSize, NodeList nodeList);
        public abstract void Run(NodeList nodeList);

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null) {handler(this, new PropertyChangedEventArgs(name));}
        }

        #endregion

        #region Other Events

        public event EventHandler Initialized;
        protected virtual void OnInitialized()
        {
            var handler = Initialized;
            if (handler != null) { handler(this, new EventArgs()); }
        }

        public event EventHandler<MessageEventArgs> Completed;
        protected virtual void OnCompleted(string message)
        {
            var handler = Completed;
            if (handler != null) { handler(this, new MessageEventArgs { Message = message }); }
        }

        #endregion

        #region Factory Methods

        public static Algorithm Create(AlgorithmType type)
        {
            if (type == AlgorithmType.GeneticAlgorithmByCutpoint)
                return new GeneticAlgorithmByCutpoint();
            if (type == AlgorithmType.GeneticAlgorithmByClosest)
                return new GeneticAlgorithmByClosest();
            throw new ArgumentException("There is no implementation for algorithm of type " + type);
        }

        #endregion
    }

    #region Genetic Algorithm Base

    internal abstract class GeneticAlgorithm : Algorithm
    {
        protected static readonly int MAX_GEN = int.Parse(ConfigurationManager.AppSettings["MaxGen"] ?? "2500");
        protected static readonly int GROUP_SIZE = int.Parse(ConfigurationManager.AppSettings["GroupSize"] ?? "25");
        protected static readonly int MUTATE_CHANCE = int.Parse(ConfigurationManager.AppSettings["MutateChance"] ?? "5");

        public Random Randomizer { get; set; }
        public int MaxGeneration { get; set; }
        public int GroupSize { get; set; }
        public int MutateChance { get; set; }

        public override int Id { get { return this.BestRouteGeneration; } }
        public int BestRouteGeneration { get; set; }

        protected GeneticAlgorithm(Random randomizer, int maxGen, int groupSize, int mutateChance)
        {
            Randomizer = randomizer;
            MaxGeneration = maxGen;
            GroupSize = groupSize;
            MutateChance = mutateChance;
        }

        public override void Run(NodeList nodeList)
        {
            var watch = new Stopwatch();
            watch.Start();
            // controls generation and algorithm run-time loop invariant
            for (var generation = 1; generation <= MaxGeneration; generation++)
            {
                if (Stop) break;
                // initiate crossover
                CrossOver(nodeList, generation);
            }
            watch.Stop();
            var msg = watch.Elapsed.TotalSeconds.ToString("#.##") + " s";
            OnCompleted(msg);
        }

        internal virtual void CrossOver(NodeList nodeList, int generation)
        {
            // Isolate group members for cross-over
            var parents = new Tuple<Route, Route>();
            var group = SetGroupMembers(nodeList, parents);

            // enforce cutpoints on both parents and inherit nodes between the cutpoints to the children.
            var children = FilterInheritedGenes(parents, nodeList);

            // get genetic sequence for use within children
            var seq = GetSequence(parents);

            ExecuteCrossover(children, seq, nodeList.Count);
            if (Randomizer.Next(100) <= MutateChance) { Mutate(children, nodeList.Count); }

            CheckNewBestRoute(nodeList, children, generation);

            // replace loosers with children
            ReplaceLoosers(group, children);
        }

        internal virtual IList<int> SetGroupMembers(NodeList nodeList, Tuple<Route, Route> parents)
        {
            var size = this.GroupSize < this.Count ? this.GroupSize : this.Count;

            // grab x random route entries where x is GroupSize, then order by Fitness Level, fittest at the top
            var group = new int[size];
            for (var i = 0; i < size; i++) { group[i] = Randomizer.Next(Count); }
            var sorted = group.OrderBy(i => this[i].Fitness).ToList();

            // select parents
            parents.One = this[sorted[0]];
            parents.Two = this[sorted[1]];
            return sorted;
        }

        internal abstract Tuple<Route, Route> FilterInheritedGenes(Tuple<Route, Route> parents, NodeList nodeList);

        internal abstract Tuple<IList<int>, IList<int>> GetSequence(Tuple<Route, Route> parents);

        internal virtual void ExecuteCrossover(Tuple<Route, Route> children, Tuple<IList<int>, IList<int>> sequence,
                                                int capacity)
        {
            Action<IList<int>, Route> copy =
                (seq, child) =>
                {
                    var iter = seq.Except(child).GetEnumerator();
                    iter.MoveNext();
                    for (var i = 0; i < capacity; i++)
                        if (child[i] == -1)
                        {
                            child[i] = iter.Current;
                            iter.MoveNext();
                        }
                };

            // copy nodal sequence from parent2 to child1
            copy(sequence.Two, children.One);
            // copy nodal sequence from parent1 to child2
            copy(sequence.One, children.Two);
        }

        internal virtual void Mutate(Tuple<Route, Route> children, int capacity)
        {
            // do some mutation...
        }

        internal virtual void CheckNewBestRoute(NodeList nodeList, Tuple<Route, Route> children, int generation)
        {
            children.One.DetermineFitness(nodeList);
            children.Two.DetermineFitness(nodeList);
            var fittestChild = children.One.Fitness < children.Two.Fitness ? children.One : children.Two;
            if (fittestChild.Fitness >= BestRoute.Fitness) return;
            BestRoute = fittestChild;
            BestRouteGeneration = generation;
        }

        internal virtual void ReplaceLoosers(IList<int> group, Tuple<Route, Route> children)
        {
            this[group[group.Count - 1]] = children.One;
            this[group[group.Count - 2]] = children.Two;
        }

    }

    #endregion

    #region Genetic Algorithm By Cutpoint

    internal class GeneticAlgorithmByCutpoint : GeneticAlgorithm
    {
        public int CutpointOne { get; set; }
        public int CutpointTwo { get; set; }

        public GeneticAlgorithmByCutpoint() : this(-1, -1, new Random((int)DateTime.Now.ToBinary())) { }
        public GeneticAlgorithmByCutpoint(Random randomizer) : this(-1, -1, randomizer) { }
        public GeneticAlgorithmByCutpoint(int cutpointOne, int cutpointTwo, Random randomizer)
            : this(cutpointOne, cutpointTwo, randomizer, MAX_GEN, GROUP_SIZE, MUTATE_CHANCE) { }
        public GeneticAlgorithmByCutpoint(int cutpointOne, int cutpointTwo, Random randomizer, int maxGen, int groupSize, int mutateChance)
            : base(randomizer, maxGen, groupSize, mutateChance)
        {
            CutpointOne = cutpointOne;
            CutpointTwo = cutpointTwo;
        }

        public override void InitializePopulation(int populationSize, NodeList nodeList)
        {
            for (var i = 0; i < populationSize; i++)
            {
                var route = new Route(nodeList.Count, nodeList.IsCycle, Randomizer);
                var randomList = nodeList.Shuffle(Randomizer);

                foreach (var n in randomList) { route.AddNode(n); }

                route.DetermineFitness(nodeList);
                Add(route);

                if (BestRoute == null || route.Fitness < BestRoute.Fitness)
                    BestRoute = route;
            }

            // random cut points if not explicitly set
            InitializeCutpoints(nodeList);
            OnInitialized();
        }

        // InitializeCutpoints
        #region Cutpoint Initialization

        internal void InitializeCutpoints(NodeList nodeList)
        {
            if (CutpointOne == -1)
                CutpointOne = new Random().Next(GetCutpointOneRange(nodeList.Count).One, GetCutpointOneRange(nodeList.Count).Two);
            if (CutpointTwo == -1)
                CutpointTwo = new Random().Next(Math.Max(GetCutpointTwoRange(nodeList.Count).One, CutpointOne), GetCutpointTwoRange(nodeList.Count).Two);
        }

        internal static Tuple<int, int> GetCutpointOneRange(int count)
        {
            var result = new Tuple<int, int>
            {
                One = (int)Math.Floor(count * .1),
                Two = (int)Math.Ceiling(count * .25)
            };
            return result;
        }

        internal static Tuple<int, int> GetCutpointTwoRange(int count)
        {
            var result = new Tuple<int, int>
            {
                One = (int)Math.Floor(count * .75),
                Two = (int)Math.Ceiling(count * .9)
            };
            return result;
        }

        #endregion

        internal override Tuple<Route, Route> FilterInheritedGenes(Tuple<Route, Route> parents, NodeList nodeList)
        {
            var children = new Tuple<Route, Route>(new Route(nodeList.Count, nodeList.IsCycle, Randomizer),
                                                   new Route(nodeList.Count, nodeList.IsCycle, Randomizer));
            Action<Route, Route> filter = 
                (parent, child) =>
                {
                    for (var i = 0; i < parent.Count; i++)
                        child.Add((i < CutpointOne || i > CutpointTwo) &&
                                  !(nodeList[i].IsStart || nodeList[i].IsEnd) ?
                                  -1 : parent[i]);
                };

            filter(parents.One, children.One);
            filter(parents.Two, children.Two);
            return children;
        }

        internal override Tuple<IList<int>, IList<int>> GetSequence(Tuple<Route, Route> parents)
        {
            var result = new Tuple<IList<int>, IList<int>> { One = new List<int>(), Two = new List<int>() };

            // start from CutpointTwo then add the parent nodes into the new code-list and rotate through the beginning of the list.
            // GetRange is inclusive, where we want an exclusive range, hence the additional math.
            ((List<int>)result.One).AddRange(parents.One.GetRange(CutpointTwo + 1, parents.One.Count - 1 - CutpointTwo));
            ((List<int>)result.One).AddRange(parents.One.GetRange(0, CutpointTwo + 1));

            ((List<int>)result.Two).AddRange(parents.Two.GetRange(CutpointTwo + 1, parents.Two.Count - 1 - CutpointTwo));
            ((List<int>)result.Two).AddRange(parents.Two.GetRange(0, CutpointTwo + 1));

            return result;
        }
    }

    #endregion

    #region Genetic Algorithm By Closest

    internal class GeneticAlgorithmByClosest : GeneticAlgorithm
    {
        private static readonly int CHANCE_FOR_CLOSEST = int.Parse(ConfigurationManager.AppSettings["ChanceForClosest"] ?? "75");
        private static readonly int MUTATE_SEGMENT_SIZE = int.Parse(ConfigurationManager.AppSettings["MutateSegmentSize"] ?? "4");

        public int ChanceForClosest { get; set; }
        public int MutateSegmentSize { get; set; }

        public GeneticAlgorithmByClosest()
            : base(new Random((int)DateTime.Now.Ticks), MAX_GEN, GROUP_SIZE, MUTATE_CHANCE)
        {
            ChanceForClosest = CHANCE_FOR_CLOSEST;
            MutateSegmentSize = MUTATE_SEGMENT_SIZE;
        }

        public override void InitializePopulation(int populationSize, NodeList nodeList)
        {
            Route bestRoute = null;
            for(var i=0; i<populationSize; i++)
            {
                var route = new Route(nodeList.Count, nodeList.IsCycle, Randomizer);

                // constrain subset of options
                var options = nodeList.Where(n => !n.IsStart && !n.IsEnd)
                                      .Select(n => n.Id).ToList();

                // initialize the starting point
                var start = nodeList.IsCycle ? Randomizer.Next(nodeList.Count) : nodeList.StartNodeIndex;
                var current = nodeList[start];
                route.AddNode(start);
                options.Remove(start);

                // while there are available items in the option subset
                while(route.Count < (route.IsCycle ? nodeList.Count : nodeList.Count - 1))
                {
                    var next = options[Randomizer.Next(options.Count)];
                    if (Randomizer.Next(100) < ChanceForClosest)
                    {
                        var closest = current.Closest.Except(route).ToList();
                        if (closest.Count > 0) { next = closest[Randomizer.Next(closest.Count)]; }
                    }

                    route.AddNode(next);
                    options.Remove(next);
                    current = nodeList[next];
                }

                // add the end node, will be automatically excluded if start == end
                route.AddNode(nodeList.EndNodeIndex);

                route.DetermineFitness(nodeList);
                Add(route);

                if (bestRoute == null || route.Fitness < bestRoute.Fitness)
                    bestRoute = route;
            }
            BestRoute = bestRoute;
            OnInitialized();
        }

        internal override Tuple<Route, Route> FilterInheritedGenes(Tuple<Route, Route> parents, NodeList nodeList)
        {
            var result = new Tuple<Route, Route>();

            // take matching links from both parents
            Func<Route, Route, Route> matcher =
                (p1, p2) =>
                    {
                        var child = new Route(nodeList.Count, nodeList.IsCycle, Randomizer);
                        child.AddRange(Enumerable.Repeat(-1, nodeList.Count).ToArray());
                        // 1st pass - matches p1 going forward with p2 going forward
                        for (var i = 0; i < nodeList.Count - 1; i++ )
                        {
                            var item = p1[i];
                            var j = p2.IndexOf(item);
                            // matching tuple
                            if(j<nodeList.Count-1 && (p1[i+1] == p2[j+1]))
                            {
                                child[i] = p1[i];
                                child[i + 1] = p1[i + 1];
                                i++;
                            }
                        }
                        // 2nd pass - matches p1 going backward with p2 going forward
                        for (var i = nodeList.Count-1; i > 0; i--)
                        {
                            var item = p1[i];
                            var j = p2.IndexOf(item);
                            if(j<nodeList.Count-1 && (p1[i-1] == p2[j+1]))
                            {
                                child[i] = p1[i];
                                child[i - 1] = p1[i - 1];
                                i--;
                            }
                        }
                        return child;
                    };

            result.One = matcher(parents.One, parents.Two);
            result.Two = matcher(parents.Two, parents.One);

            // always ensure that the last node is the EndNode (unless it's a cycle)
            if(!nodeList.IsCycle)
            {
                var final = parents.One[nodeList.Count - 1];
                result.One[nodeList.Count - 1] = final;
                result.Two[nodeList.Count - 1] = final;
            }

            // return matched nodes
            return result;
        }

        internal override Tuple<IList<int>, IList<int>> GetSequence(Tuple<Route, Route> parents)
        {
            // identify pairing sequence 
            return new Tuple<IList<int>, IList<int>>(parents.One.ToList(), parents.Two.ToList());
        }

        internal override void  Mutate(Tuple<Route,Route> children, int capacity)
        {
            // randomly changes a segment of this tour
            var index = Randomizer.Next(capacity - MutateSegmentSize);
            Action<Route, IList<int>> mutater =
                (route, segment) =>
                    {
                        int i = index;
                        segment.Randomize().ForEach(
                                s =>
                                    {
                                        route[i] = s;
                                        i++;
                                    }
                            );
                    };

            mutater(children.One, children.One.GetRange(index, MutateSegmentSize));
            mutater(children.Two, children.Two.GetRange(index, MutateSegmentSize));
        }
    }

    #endregion
}
