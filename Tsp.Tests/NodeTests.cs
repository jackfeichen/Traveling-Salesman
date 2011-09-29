using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Radex.Tsp.UnitTest
{
    [TestFixture]
    public class NodeTests
    {
        private static readonly Random rnd = new Random((int)DateTime.Now.ToBinary());
        public static readonly int DEFAULT_CAPACITY = 5;

        public static NodeList GenerateRandomNodes() { return GenerateRandomNodes(DEFAULT_CAPACITY); }
        public static NodeList GenerateRandomNodes(int capacity)
        {
            var nodes = new List<Node>();
            for (var i = 0; i < capacity; i++)
                nodes.Add(new Node { Id = i, X = rnd.Next(1, 100), Y = rnd.Next(1, 100) });
            return new NodeList(nodes);
        }

        [Test]
        public void NodeList_Construction_With_RandomNodes()
        {
            var nodeList = GenerateRandomNodes();
            nodeList.ForEach(n =>
                {
                    Assert.Greater(n.X, 0);
                    Assert.Greater(n.Y, 0);
                    Assert.Less(n.X, 100);
                    Assert.Less(n.Y, 100);
                });
        }

        [Test]
        public void CalculateDistance_Then_Check_Rating()
        {
            var nodeList = GenerateRandomNodes();

            const int node1 = 0;
            const int node2 = 4;
            nodeList.CalculateNodeRating(RatingType.Distance);

            var distance = nodeList[node1].CalculateDistance(nodeList[node2]);
            Assert.AreEqual(distance, nodeList[node1].Ratings[node2]);
        }

        [Test]
        public void Load_Nodes_From_Xml_File()
        {
            var file = "Nodes.xml";
            Assert.IsTrue(File.Exists(file));
            var list = new NodeList(file);
            Assert.IsNotNull(list);
            Assert.Greater(list.Count, 0);
        }
    }
}
