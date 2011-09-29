using System;
using NUnit.Framework;

namespace Radex.Tsp.UnitTest
{
    [TestFixture]
    public class TspTests
    {
        [Test]
        public void Run_GeneticAlgorithm_Against_Nodes()
        {
            var file = "Nodes.xml";
            var list = new NodeList(file);

            Algorithm alg = new GeneticAlgorithmByCutpoint();

            alg.InitializePopulation(1000, list);
            var originalFitness = alg.BestRoute.Fitness;
            Assert.Greater(originalFitness, 0);

            Console.WriteLine("Intial best route fitness: " + alg.BestRoute.Fitness);

            alg.PropertyChanged += (o, e) =>
                                   {
                                       if (e.PropertyName == "BestRoute")
                                       {
                                           Console.WriteLine("new best route found with fitness level: {0}",
                                                             alg.BestRoute.Fitness);
                                           Assert.Less(alg.BestRoute.Fitness, originalFitness);
                                       }
                                   };
            
            alg.Run(list);
            var finalFitness = alg.BestRoute.Fitness;
            Assert.LessOrEqual(finalFitness, originalFitness);

            Console.WriteLine("Final best route fitness: " + alg.BestRoute.Fitness);
        }
    }
}
