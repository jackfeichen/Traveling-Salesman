using System;

namespace Radex.Tsp
{
    public enum RatingType
    {
        Distance = 0
    }

    public static class RatingExtensions
    {
        public static double CalculateDistance(this Node n1, Node n2)
        {
            // no exception-check needed, as BaseNode has the data sufficient for calculation
            double xd = n1.X - n2.X, yd = n1.Y - n2.Y;
            return Math.Sqrt(xd * xd + yd * yd);
        }
    }
}
