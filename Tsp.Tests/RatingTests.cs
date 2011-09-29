using System;
using NUnit.Framework;

namespace Radex.Tsp.UnitTest
{
    [TestFixture]
    public class RatingTests
    {
        [Test]
        public void CalculateRatingTest()
        {
            var n1 = new Node {X = -121.974015d, Y = 37.240532d};
            var n2 = new Node {X = -121.982876d, Y = 37.229842};
            Assert.AreEqual(0.013885d, Math.Round(n1.CalculateDistance(n2), 6));
        }
    }
}
