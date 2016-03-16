using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Microsoft.Samples.Kinect.BodyBasics
{
    public class ViewModel
    {
        private PlotModel plotModel;

        public ViewModel()
        {
            this.plotModel = new PlotModel();
            plotModel.LegendTitle = "Legend";
            plotModel.LegendOrientation = LegendOrientation.Horizontal;
            plotModel.LegendPlacement = LegendPlacement.Outside;
            plotModel.LegendPosition = LegendPosition.TopRight;
            plotModel.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            plotModel.LegendBorder = OxyColors.Black;
            this.Title = "Puls";
            this.Points = new List<DataPoint>();
        }

        public string Title { get; private set; }

        public IList<DataPoint> Points { get; private set; }

        public void AddDatapoint(double x, double y)
        {
            if(Points.Count < 10)
            {
                this.Points.Add(new DataPoint(x, y));
            }
            else
            {
                this.Points.RemoveAt(0);
                this.Points.Add(new DataPoint(x, y));
            }
        }
       
    }


}
