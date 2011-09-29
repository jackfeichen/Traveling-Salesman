using System;
using NUnit.Framework;

namespace Radex.Tsp.UnitTest
{
    [TestFixture]
    public class RouteTests
    {
        #region Helpers

        private static readonly Random rnd = new Random((int)DateTime.Now.ToBinary());

        public static void AssignRandomRoute(Route route, NodeList nodeList)
        {
            for (var i = 0; i < nodeList.Count; i++)
                route.Add(rnd.Next(nodeList.Count));
        }

        public static NodeList InitializeNodeList() { return InitializeNodeList(NodeTests.DEFAULT_CAPACITY); }
        public static NodeList InitializeNodeList(int capacity)
        {
            var nodeList = NodeTests.GenerateRandomNodes(capacity);
            nodeList.CalculateNodeRating(RatingType.Distance);

            return nodeList;
        }

        public static Route InitializeRoute() { return InitializeRoute(InitializeNodeList()); }
        public static Route InitializeRoute(int capacity) { return InitializeRoute(InitializeNodeList(capacity)); }
        public static Route InitializeRoute(NodeList nodeList)
        {
            var route = new Route(nodeList.Count, nodeList.IsCycle);
            AssignRandomRoute(route, nodeList);

            route.DetermineFitness(nodeList);
            return route;
        }

        public static Route GetRandomRoute() { return InitializeRoute(); }
        public static Route GetRandomRoute(int capacity) { return InitializeRoute(capacity); }
        public static Route GetRandomRoute(NodeList nodeList) { return InitializeRoute(nodeList); }

        #endregion

        [Test]
        public void AddNodeTest()
        {
            var capacity = 10;
            var route = new Route(capacity);

            for (var i = 0; i < capacity; i++)
                Assert.IsTrue(route.AddNode(rnd.Next(100)));

            Assert.IsFalse(route.AddNode(rnd.Next(100)));
        }

        [Test]
        public void DetermineFitnessTest()
        {
            var route = InitializeRoute();
            Assert.Greater(route.Fitness, 0);
        }

        [Test]
        public void StartEndTest()
        {
            var nodeList = InitializeNodeList();
            var route = InitializeRoute(nodeList);
            Assert.AreEqual(NodeTests.DEFAULT_CAPACITY, route.Count);

            route = new Route(nodeList.Count, nodeList.IsCycle);
            AssignRandomRoute(route, nodeList);
            route.DetermineFitness(nodeList);

            Assert.AreEqual(nodeList.Count, route.Count);
        }
    }
}
