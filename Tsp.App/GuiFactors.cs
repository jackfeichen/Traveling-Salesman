namespace Radex.Tsp.UI
{
    internal class GuiFactors
    {
        public GuiFactors(NodeList nodeList)
        {
            this.nodeList = nodeList;
            CalculateFactors();
        }

        private readonly NodeList nodeList;

        public double XOffset { get; set; }
        public double YOffset { get; set; }
        public double XFactor { get; set; }
        public double YFactor { get; set; }

        private void CalculateFactors()
        {
            var minLat = nodeList[0].Y;
            var maxLat = nodeList[0].Y;
            var minLon = nodeList[0].X;
            var maxLon = nodeList[0].X;

            foreach (var node in nodeList)
            {
                if (node.X < minLon) { minLon = node.X; }
                if (node.X > maxLon) { maxLon = node.X; }
                if (node.Y < minLat) { minLat = node.Y; }
                if (node.Y > maxLat) { maxLat = node.Y; }
            }

            XOffset = 0 - minLon;
            YOffset = 0 - minLat;

            XFactor = (maxLon - minLon) / 400;
            YFactor = (maxLat - minLat) / 400;
        }
    }
}
