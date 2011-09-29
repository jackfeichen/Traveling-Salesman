using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Radex.Tsp.UnitTest
{
    [TestFixture]
    public class UtilityTests
    {
        [Test]
        public void RandomizeTest()
        {
            var list = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            foreach (var i in list.Randomize())
                Console.Write("{0} | ", i);
        }
    }
}
