using System;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;

namespace Module_Graphs
{
    /// <summary>
    /// Interaction logic for YTGraphWPFUC.xaml
    /// </summary>
    public partial class YTGraphWPFUC : UserControl
    {
        public YTGraphWPFUC()
        {
            InitializeComponent();

            SetupChartProperties();
        }



        private void SetupChartProperties()
        {
            //customize the X-Axis to properly display Time 
            //chart1.Customize += chart1_Customize;
            chart1.Series.Clear(); //first remove all series completely

            //// Enable all elements
            //chart1.ChartAreas[0].AxisX.MinorGrid.Enabled = true;
            //chart1.ChartAreas[0].AxisX.MinorTickMark.Enabled = true;
            //chart1.ChartAreas[0].AxisX.MinorTickMark.Interval = 15;

            // Set Grid lines and tick marks interval
            //chart1.ChartAreas[0].AxisX.MajorGrid.Interval = 30;
            //chart1.ChartAreas[0].AxisX.MajorTickMark.Interval = 30;
            //chart1.ChartAreas[0].AxisX.MajorGrid.IntervalOffsetType = DateTimeIntervalType.Seconds;
            //chart1.ChartAreas[0].AxisX.MinorGrid.Interval = 5;
            ///lchart1.ChartAreas[0].AxisX.MinorTickMark.Interval = 5;

            //// Set Line Color
            //chart1.ChartAreas[0].AxisX.MinorGrid.LineColor = Color.Blue;

            //// Set Line Style
            //chart1.ChartAreas[0].AxisX.MajorTickMark.LineDashStyle = ChartDashStyle.Solid;

            //// Set Line Width
            //chart1.ChartAreas[0].AxisX.MajorGrid.LineWidth = 1;    

            
            chart1.ChartAreas[0].AxisX.Interval = 600; //let's show a minute of data
            chart1.ChartAreas[0].AxisX.IsStartedFromZero = true;
            chart1.ChartAreas[0].AxisX.Minimum = 0;

            chart1.ChartAreas[0].AxisY.IsStartedFromZero = false;
            chart1.BackColor = Color.GhostWhite;

            //set legend position and properties as required
            chart1.Legends[0].LegendStyle = LegendStyle.Table;

            // Set table style if legend style is Table
            chart1.Legends[0].TableStyle = LegendTableStyle.Auto;

            // Set legend docking
            chart1.Legends[0].Docking = Docking.Top;

            // Set legend alignment
            chart1.Legends[0].Alignment = StringAlignment.Center;

            // Set Antialiasing mode
            //this can be set lower if there are any performance issues!
            chart1.AntiAliasing = AntiAliasingStyles.All;
            chart1.TextAntiAliasingQuality = TextAntiAliasingQuality.High; 
        }


        public void CheckAndAddSeriesToGraph(string strPinDescription, string strUnit)
        {
            foreach (Series se in chart1.Series)
            {
                if (se.Name == strPinDescription)
                {
                    return; //already exists
                }
            }
            if (strUnit == "marker")
            {
                Series s = chart1.Series.Add(strPinDescription);
                s.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                s.MarkerStyle = MarkerStyle.Cross;
                s.Color = System.Drawing.Color.FromArgb(255, 150, 0, 150);
                s.BorderWidth = 3;
            }
            else
            {
                Series s = chart1.Series.Add(strPinDescription);
                s.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
                s.BorderColor = System.Drawing.Color.FromArgb(180, 26, 59, 105);
                s.BorderWidth = 2; // show a THICK line for high visibility, can be reduced for high volume data points to be better visible
                s.ShadowOffset = 1;
                s.IsVisibleInLegend = true;
                //s.IsValueShownAsLabel = true;                       
                s.LegendText = strPinDescription + " (" + strUnit + ")";
                s.LegendToolTip = strPinDescription + " (" + strUnit + ")";

                if (strPinDescription == "Breath")
                {
                    s.Color = System.Drawing.Color.FromArgb(255, 0, 0, 255);
                }
                else
                {
                    s.Color = System.Drawing.Color.FromArgb(255, 255, 0, 0);
                }
            }
        }

        internal void ClearSeriesFromGraph()
        {
            //remove the series curve itself
            chart1.Series.Clear();
        }

        internal void ClearFirstValueFromGraph(string strPinName)
        {
            //remove the first value from the curve
            
        }

        internal void ClearCurveDataPointsFromGraph()
        {
            //clear only DATA points from curve, keeping it as is.
            foreach (Series s in chart1.Series)
            {
                s.Points.Clear();
            }
        }

        public void AddPointToLine(string strPinName, double dValueY, double dValueX)
        {
            // we don't want series to be drawn while adding points to it.
            //this can reduce flicker.
            chart1.Series.SuspendUpdates();

            chart1.Series[strPinName].Points.AddXY(dValueX, dValueY);
            chart1.Series.ResumeUpdates();
        }

        private void chart1_Customize(object sender, EventArgs e)
        {
            //make the X-axis show up with days, hours, minutes, seconds properly.
            //this is not very well documented anywhere but copied from an untraceable blog article.
            CustomLabelsCollection xAxisLabels = ((Chart)sender).ChartAreas[0].AxisX.CustomLabels;
            for (int cnt = 0; cnt < xAxisLabels.Count; cnt++)
            {
                TimeSpan ts = TimeSpan.FromSeconds(double.Parse(xAxisLabels[cnt].Text));
                if (ts.Days > 0)
                    xAxisLabels[cnt].Text = ts.Days.ToString("00") + ":" + ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
                else
                {
                    if (ts.Hours > 0)
                        xAxisLabels[cnt].Text = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
                    else
                        xAxisLabels[cnt].Text = ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
                }
            }

        }

        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            chart1.Width = (int)e.NewSize.Width;
            chart1.Height = (int) e.NewSize.Height;
        }      
    }
}
