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
        public ViewModel()
        {
            this.Title = "Puls";
            this.Points = new List<DataPoint>();
        }

        public string Title { get; private set; }

        public IList<DataPoint> Points { get; private set; }

        public void AddDatapoint(double x, double y)
        {
            this.Points.Add(new DataPoint(x, y));
        }
       
    }


}
