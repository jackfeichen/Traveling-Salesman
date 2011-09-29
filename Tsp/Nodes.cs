using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace Radex.Tsp
{
    [DataContract]
    public class Node
    {
        public int Id { get; set; }
        public int Seq { get; set; }
        public bool IsStart { get; set; }
        public bool IsEnd { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        private List<double> ratings;
        public List<double> Ratings
        {
            get
            {
                if (ratings == null)
                    ratings = new List<double>();
                return ratings;
            }
        }

        private List<int> closest;
        public List<int> Closest
        {
            get
            {
                if(closest == null)
                    closest = new List<int>();
                return closest;
            }
        }

        public void FindClosest(int number)
        {
            var shortestNode = 0;
            var dist = new double[Ratings.Count];
            Ratings.CopyTo(dist);

            if (number > Ratings.Count - 1)
            {
                number = Ratings.Count - 1;
            }

            Closest.Clear();

            for (int i = 0; i < number; i++)
            {
                double shortestDistance = Double.MaxValue;
                for (int cityNum = 0; cityNum < Ratings.Count; cityNum++)
                {
                    if (dist[cityNum] < shortestDistance)
                    {
                        shortestDistance = dist[cityNum];
                        shortestNode = cityNum;
                    }
                }
                closest.Add(shortestNode);
                dist[shortestNode] = Double.MaxValue;
            }
        }
    }

    /// <summary>
    /// Node concreate class allows for the creation of generic Nodes within the INodeList interface
    /// </summary>
    public class NodeList : IList<Node>
    {
        private static readonly int NUMBER_OF_CLOSEST = int.Parse(ConfigurationManager.AppSettings["NumberOfClosest"] ?? "5");

        private readonly List<Node> nodes = new List<Node>();

        public int StartNodeIndex { get; private set; }
        public int EndNodeIndex { get; private set; }
        public bool IsCycle { get; private set; }

        public NodeList(string file)
        {
            var doc = XDocument.Load(file);
            if (doc.Root == null) return;
            var items = doc.Root.Elements("Node")
                                .Select((e, i) => new Node
                                {
                                    Id = i,
                                    Seq = int.Parse(e.GetAttr("Seq", "-1")),
                                    IsEnd = bool.Parse(e.GetAttr("IsEnd", "false")),
                                    IsStart = bool.Parse(e.GetAttr("IsStart", "false")),
                                    X = double.Parse(e.GetAttr("X", "0")),
                                    Y = double.Parse(e.GetAttr("Y", "0"))
                                });
            Initialize(items);
        }
        public NodeList(IEnumerable<Node> items)
        {
            Initialize(items);
        }
        private void Initialize(IEnumerable<Node> items)
        {
            this.nodes.AddRange(items);
            this.CalculateNodeRating(RatingType.Distance, NUMBER_OF_CLOSEST);

            var start = this.nodes.FirstOrDefault(n => n.IsStart);
            var end = this.nodes.FirstOrDefault(n => n.IsEnd);
            SetStartAndEndNodes(start, end);
        }
        // default behavior of what an uninitialized start/end node means
        private void SetStartAndEndNodes(Node start, Node end)
        {
            this.IsCycle = true;
            if(start != null && end != null)
            {
                this.StartNodeIndex = start.Id;
                this.EndNodeIndex = end.Id;
                this.IsCycle = false;
            }
        }

        #region Implementation of IEnumerable

        public IEnumerator<Node> GetEnumerator()
        {
            return this.nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<Node>

        public void Add(Node item)
        {
            // todo: update ratings
            this.nodes.Add(item);
        }

        public void Clear()
        {
            // todo: update ratings
            this.nodes.Clear();
        }

        public bool Remove(Node item)
        {
            // todo: update ratings
            return this.nodes.Remove(item);
        }

        public bool Contains(Node item) { return this.nodes.Contains(item); }
        public void CopyTo(Node[] array, int arrayIndex) { this.nodes.CopyTo(array, arrayIndex); }
        public int Count { get { return this.nodes.Count; } }
        public bool IsReadOnly { get { return false; }}

        #endregion

        #region Implementation of IList<Node>

        public int IndexOf(Node item) { return this.nodes.IndexOf(item); }

        public void Insert(int index, Node item)
        {
            // todo: update ratings
            this.nodes.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            // todo: update ratings
            this.nodes.RemoveAt(index);
        }

        public Node this[int index]
        {
            get { return this.nodes[index]; }
            set
            {
                this.nodes[index] = value;
                // todo: recalculate ratings
            }
        }

        #endregion
    }

    /// <summary>
    /// Node static method feature will allow "splicing" in of functionality to all members that implement INodeList
    /// </summary>
    /// <remarks>Node class extender will carry the bulk of the actual implementation for INodeList</remarks>
    public static class NodeListExtenders
    {
        private static readonly int NUMBER_OF_CLOSEST = int.Parse(ConfigurationManager.AppSettings["NumberOfClosest"] ?? "5");

        public static void CalculateNodeRating(this NodeList list, RatingType type)
        {
            list.CalculateNodeRating(type, NUMBER_OF_CLOSEST);
        }

        public static void CalculateNodeRating(this NodeList list, RatingType type, int numberOfClosest)
        {
            if (type == RatingType.Distance)
            {
                list.ForEach(n =>
                    {
                        n.Ratings.Clear();
                        list.ForEach(p => n.Ratings.Add(n.CalculateDistance(p)));
                    });
                list.ForEach(n => n.FindClosest(numberOfClosest));
            }
            else
                throw new ArgumentException(string.Format("There is no rating type of {0}", type));
        }
    }
}
