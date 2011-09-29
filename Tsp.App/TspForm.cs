using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Radex.Tsp.UI
{
    public partial class TspForm : Form
    {
        private readonly Algorithm alg;
        private readonly NodeList nodes;
        private readonly GuiFactors guiFactors;
        private Image cityImage;
        private Graphics cityGraphics;
        private bool isRunning;

        public TspForm()
        {
            InitializeComponent();
            this.alg = Algorithm.Create(AlgorithmType.GeneticAlgorithmByClosest);
            this.nodes = new NodeList("Nodes.xml");
            this.guiFactors = new GuiFactors(this.nodes);
            this.DrawCityList();

            Action<Action> safeInvoker = 
                act =>
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(act);
                        return;
                    }
                    act();
                };

            this.alg.PropertyChanged += 
                (o, pe) =>
                {
                    if (pe.PropertyName == "BestRoute")
                    {
                        safeInvoker(() => this.DrawTour(alg.BestRoute, alg.Id));
                    }
                };
            this.alg.Initialized += (o, ce) => safeInvoker(() => lblStatus.Text = "Running");
            this.alg.Completed += (o, ce) => safeInvoker(() =>
            {
                lblStatus.Text = ce.Message;
                lastIterationValue.Text = alg.Id.ToString();
                btnStart.Text = "Start";
                isRunning = false;
            });
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            int population;
            if (!int.TryParse(txtPopulation.Text, out population)) { population = 10000; }
            if (isRunning)
            {
                btnStart.Text = "Start";
                isRunning = false;
                alg.Stop = true;
                return;
            }
            btnStart.Text = "Stop";
            lblStatus.Text = "Initializing";
            isRunning = true; 
            ThreadPool.QueueUserWorkItem(
                o =>
                {
                    this.alg.InitializePopulation(population, nodes);
                    this.DrawTour(alg.BestRoute, alg.Id);
                    this.alg.Run(this.nodes);
                });
        }

        public void DrawTour(Route route, int id)
        {
            this.lastFitnessValue.Text = Math.Round(route.Fitness, 2).ToString(CultureInfo.CurrentCulture);
            this.lastIterationValue.Text = id.ToString();

            if (this.cityImage == null)
            {
                this.cityImage = new Bitmap(tourDiagram.Width, tourDiagram.Height);
                this.cityGraphics = Graphics.FromImage(cityImage);
            }

            cityGraphics.FillRectangle(Brushes.White, 0, 0, cityImage.Width, cityImage.Height);

            Action<Node, Node> draw = 
                (prev, curr) =>
                {
                    // Draw a circle for the city.
                    var xValue = Convert.ToInt32((curr.X + guiFactors.XOffset) / guiFactors.XFactor);
                    var yValue = Convert.ToInt32((curr.Y + guiFactors.YOffset) / guiFactors.YFactor);
                    cityGraphics.DrawEllipse(Pens.Black, xValue - 2, Math.Abs(400 - yValue) - 2, 5, 5);
                    if (curr.IsStart) { cityGraphics.DrawEllipse(Pens.Green, xValue - 2, Math.Abs(400 - yValue) - 2, 8, 8); }
                    if (curr.IsEnd) { cityGraphics.DrawEllipse(Pens.Red, xValue - 2, Math.Abs(400 - yValue) - 2, 9, 9); }

                    // Draw the line connecting the city.
                    var ptLastCity = new Point(
                        Convert.ToInt32((prev.X + guiFactors.XOffset) / guiFactors.XFactor),
                        Math.Abs(400 - Convert.ToInt32((prev.Y + guiFactors.YOffset) / guiFactors.YFactor))
                    );

                    var ptCurrentCity = new Point(
                        Convert.ToInt32((curr.X + guiFactors.XOffset) / guiFactors.XFactor),
                        Math.Abs(400 - Convert.ToInt32((curr.Y + guiFactors.YOffset) / guiFactors.YFactor))
                    );

                    cityGraphics.DrawLine(Pens.Black, ptLastCity, ptCurrentCity);
                };

            for(var i=0; i<route.Count; i++)
            {
                var current = this.nodes[route[i]];
                var previous = i > 0 ? this.nodes[route[i - 1]] : current;
                draw(previous, current);
            }
            if(route.IsCycle)
            {
                draw(this.nodes[route.Last()], this.nodes[route.First()]);
            }
            this.tourDiagram.Image = cityImage;
        }

        /// <summary>
        /// Draw just the list of cities.
        /// </summary>
        private void DrawCityList()
        {
            var city = new Bitmap(tourDiagram.Width, tourDiagram.Height);
            var graphics = Graphics.FromImage(city);

            foreach (var node in this.nodes)
            {
                // Draw a circle for the city.
                var xValue = Convert.ToInt32((node.X + guiFactors.XOffset) / guiFactors.XFactor);
                var yValue = Convert.ToInt32((node.Y + guiFactors.YOffset) / guiFactors.YFactor);
                graphics.DrawEllipse(Pens.Black, xValue - 2, Math.Abs(400 - yValue) - 2, 5, 5);
                if (node.IsStart) { graphics.DrawEllipse(Pens.Green, xValue - 2, Math.Abs(400 - yValue) - 2, 7, 7); }
                if (node.IsEnd) { graphics.DrawEllipse(Pens.Red, xValue - 2, Math.Abs(400 - yValue) - 2, 9, 9); }
            }

            this.tourDiagram.Image = city;
        }
    }
}
