using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Radex.Tsp.UnitTest
{
    [TestFixture]
    public class AlgorithmTests
    {
        [Test]
        public void Get_Random_Routes_Then_Set_FilterGene()
        {
            // need alg, cutpoints, and nodelist
            var alg = new GeneticAlgorithmByCutpoint { CutpointOne = 2, CutpointTwo = 6 };
            var nodeList = RouteTests.InitializeNodeList(10);
            alg.Add(RouteTests.GetRandomRoute(nodeList));
            alg.Add(RouteTests.GetRandomRoute(nodeList));

            alg.InitializeCutpoints(nodeList);

            Console.WriteLine("CutpointOne: {0}\t\tCutpointTwo: {1}\n", alg.CutpointOne, alg.CutpointTwo);

            var children = alg.FilterInheritedGenes(new Tuple<Route, Route>(alg[0], alg[1]), nodeList);
            children.One.ForEach(n => Console.Write(" {0} |", n));
            Console.WriteLine();
            children.Two.ForEach(n => Console.Write(" {0} |", n));

            // TODO: Assert statements
        }

        [Test]
        public void Get_Random_Routes_Then_GetSequence()
        {
            var alg = new GeneticAlgorithmByCutpoint { CutpointOne = 2, CutpointTwo = 6 };
            var nodeList = RouteTests.InitializeNodeList(10);
            alg.Add(RouteTests.GetRandomRoute(nodeList));
            alg.Add(RouteTests.GetRandomRoute(nodeList));

            alg.InitializeCutpoints(nodeList);

            Console.WriteLine("CutpointOne: {0}\t\tCutpointTwo: {1}\n", alg.CutpointOne, alg.CutpointTwo);

            var seq = alg.GetSequence(new Tuple<Route, Route>(alg[0], alg[1]));

            alg[0].ForEach(n => Console.Write(" {0} |", n));
            Console.WriteLine();
            seq.One.ForEach(n => Console.Write(" {0} |", n));
            Console.WriteLine();
            Console.WriteLine();
            alg[1].ForEach(n => Console.Write(" {0} |", n));
            Console.WriteLine();
            seq.Two.ForEach(n => Console.Write(" {0} |", n));

            // TODO: Assert statements
        }

        [Test]
        public void Get_Algorithm_Then_Initialize_Popoulation()
        {
            var popsize = 15;
            var nodeList = RouteTests.InitializeNodeList(15);
            var alg = new GeneticAlgorithmByCutpoint(5, 11, new Random());

            alg.InitializePopulation(popsize, nodeList);
            alg.ForEach(r =>
                {
                    r.ForEach(n => Console.Write("{0} | ", n));
                    Console.WriteLine();
                });
        }

        [Test]
        public void Set_Explicit_Routes_Then_ExecuteCrossover()
        {
            var alg = new GeneticAlgorithmByCutpoint(2, 6, new Random());
            var children = new Tuple<Route, Route>(
                new Route(10) { -1, -1, 3, 5, 9, 4, 7, -1, -1, -1 },
                new Route(10) { -1, -1, 1, 2, 6, 8, 10, -1, -1, -1 }
            );
            var seq = new Tuple<IList<int>, IList<int>>(
                new List<int>(10) { 8, 10, 6, 2, 1, 3, 5, 9, 4, 7 },
                new List<int>(10) { 9, 4, 3, 7, 5, 1, 2, 6, 8, 10 }
            );
            alg.ExecuteCrossover(children, seq, 10);

            children.One.ForEach(item => Console.Write("{0} | ", item));
            Console.WriteLine();

            children.Two.ForEach(item => Console.Write("{0} | ", item));
            Console.WriteLine();
        }

        [Test]
        public void Set_Explicit_Routes_Then_Run_Replacement()
        {
            var alg = new GeneticAlgorithmByCutpoint();
            var r1 = new Route(10) { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            alg.Add(r1);
            
            for (var i = 0; i < 10; i++)
                Assert.AreEqual(r1[i], alg[0][i]);

            var r2 = new Route(10) { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };

            for (var i = 0; i < 10; i++)
                Assert.AreNotEqual(r2[i], alg[0][i]);

            alg[0] = r2;

            for (var i = 0; i < 10; i++)
                Assert.AreEqual(r2[i], alg[0][i]);

            for (var i = 0; i < 10; i++)
                Assert.AreNotEqual(r1[i], alg[0][i]);
        }

        [Test]
        public void Set_Explicit_Routes_Then_Test_PairAbstraction()
        {
            var alg = new GeneticAlgorithmByCutpoint();
            var r1 = new Route(10) { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var r2 = new Route(10) { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
            alg.Add(r1);
            alg.Add(r2);

            for (var i = 0; i < 10; i++)
            {
                Assert.AreEqual(r1[i], alg[0][i]);
                Assert.AreEqual(r2[i], alg[1][i]);
            }

            for (var i = 0; i < 10; i++)
            {
                Assert.AreNotEqual(r2[i], alg[0][i]);
                Assert.AreNotEqual(r1[i], alg[1][i]);
            }
        }

        [Test]
        public void Set_Explicit_Routes_Then_Check_Index()
        {
            var alg = new GeneticAlgorithmByCutpoint();
            var r1 = new Route(10) { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var r2 = new Route(10) { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
            alg.Add(r1);
            alg.Add(r2);

            for (var i = 0; i < 10; i++)
            {
                Assert.AreEqual(r1[i], alg[0][i]);
                Assert.AreEqual(r2[i], alg[1][i]);
            }

            var pair = new Tuple<Route, Route>(r2, r1);

            Assert.AreEqual(0, alg.IndexOf(r1));
            Assert.AreEqual(1, alg.IndexOf(r2));

            Assert.AreEqual(1, alg.IndexOf(pair.One));
            Assert.AreEqual(0, alg.IndexOf(pair.Two));
        }

        [Test]
        public void Initialize_Random_Routes_Then_SetGroupMembers()
        {
            //var nodeList = RouteTests.InitializeNodeList(10);
            //var alg = new GeneticAlgorithmByCutpoint();
            //alg.InitializePopulation(6, nodeList);

            //var parents = new Tuple<Route, Route>();
            //var group = alg.SetGroupMembers(nodeList, parents);

            //IList<Route> routes = alg.OrderBy(r => r.Fitness).ToList();

            //Assert.AreSame(routes[0], parents.One);
            //Assert.AreSame(routes[1], parents.Two);
        }

        [Test]
        public void Initialize_Routes_Then_ReplaceLoosers()
        {
            //var nodeList = RouteTests.InitializeNodeList(10);
            //var alg = new GeneticAlgorithmByCutpoint();
            //alg.InitializePopulation(6, nodeList);

            //var children = new Tuple<Route, Route>(
            //    new Route(10) { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
            //    new Route(10) { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, }
            //);

            //var loosers = new Tuple<Route, Route>(alg[4], alg[5]);

            //Assert.AreNotSame(alg[4], children.One);
            //Assert.AreNotSame(alg[5], children.Two);

            //alg.ReplaceLoosers(loosers, children);

            //Assert.AreNotSame(loosers.One, children.One);
            //Assert.AreNotSame(loosers.Two, children.Two);

            //Assert.AreSame(alg[4], children.One);
            //Assert.AreSame(alg[5], children.Two);
        }

        [Test]
        public void Initialize_Route_And_Test_Filter_For_ClosestAlgorithm()
        {
            var nodeList = new NodeList("Nodes2.xml");
            var p1 = new Route(new[] { 9, 7, 0, 11, 6, 8, 3, 1, 4, 10, 2, 5 }, nodeList.IsCycle);
            var p2 = new Route(new[] { 3, 8, 11, 6, 0, 9, 7, 5, 4, 10, 1, 2 }, nodeList.IsCycle);

            var alg = Algorithm.Create(AlgorithmType.GeneticAlgorithmByClosest) as GeneticAlgorithmByClosest;
            var children = alg.FilterInheritedGenes(new Tuple<Route, Route>(p1, p2), nodeList);

            // should have all but 4 matches...
            Assert.AreEqual(4, children.One.Where(c => c == -1).Count());
            Assert.AreEqual(4, children.Two.Where(c => c == -1).Count());
        }

        [Test]
        public void Mutater_Randomize_Test()
        {
            var nodes = RouteTests.InitializeNodeList(10);
            var alg = Algorithm.Create(AlgorithmType.GeneticAlgorithmByClosest) as GeneticAlgorithm;
            var r1 = new Route(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, nodes.IsCycle);
            var r2 = new Route(new[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 }, nodes.IsCycle);

            var seq1 = r1.ToList();
            var seq2 = r2.ToList();

            var children = Tuple.Create(r1, r2);

            Assert.AreEqual(seq1, children.One.ToList());
            Assert.AreEqual(seq2, children.Two.ToList());

            alg.Mutate(children, nodes.Count);

            Assert.AreNotEqual(seq1, children.One.ToList());
            Assert.AreNotEqual(seq2, children.Two.ToList());
        }
    }
}
