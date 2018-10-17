using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization;
using System.IO;
using System.Diagnostics;

namespace MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.DataTypes
{
    class Plot
    {
        public int size;
        public void PlotSplineSolution(string title, Decimal[] xs, Decimal[] ys, string path) //Decimal[] x, Decimal[] y, string path)
        {
            List<DataPoint> points = new List<DataPoint>();
            var chart = new Chart();
            chart.Size = new System.Drawing.Size(600, 400);
            chart.Titles.Add(title);
            //string legendName = "Legend";
            //chart.Legends.Add(new Legend(legendName));

            ChartArea ca = new ChartArea("DefaultChartArea");
            ca.AxisX.Title = "time (sec)"; //"X(cm)";
            ca.AxisY.Title = "velocity (cm/sec)"; //"Y(cm)";
            chart.ChartAreas.Add(ca);

            size = 5;
            Series s1 = CreateSeries(chart, "", CreateDataPoints(xs, ys), Color.Blue, MarkerStyle.Circle);

            chart.Series.Add(s1);
            //chart.Series.Add(s2);

            ca.RecalculateAxesScale();
            ca.AxisX.Minimum = Math.Round(ca.AxisX.Minimum);  //260;
            ca.AxisX.Maximum = Math.Round(ca.AxisX.Maximum);  //282;

            //ca.AxisY.Minimum = 150;
            //ca.AxisY.Maximum = 200;

            int nIntervals = 40;//(xs.Length - 1);
            nIntervals = Math.Max(4, nIntervals);
            ca.AxisX.Interval = (ca.AxisX.Maximum - ca.AxisX.Minimum) / nIntervals;

            // Save
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (FileStream fs = new FileStream(path, FileMode.CreateNew))
            {
                chart.SaveImage(fs, ChartImageFormat.Png);
            }
        }

        public Series CreateSeries(Chart chart, string seriesName, IEnumerable<DataPoint> points, Color color, MarkerStyle markerStyle = MarkerStyle.None)
        {
            var s = new Series()
            {
                XValueType = ChartValueType.Double,
                YValueType = ChartValueType.Double,
                //Legend = chart.Legends[0].Name,
                IsVisibleInLegend = true,
                ChartType = SeriesChartType.Line,
                //Name = seriesName,
                //ChartArea = chart.ChartAreas[0].Name,
                MarkerStyle = markerStyle,
                Color = color,
                MarkerSize = size
            };

            foreach (var p in points)
            {
                s.Points.Add(p);
            }
            return s;
        }

        public static List<DataPoint> CreateDataPoints(Decimal[] x, Decimal[] y)
        {
            Debug.Assert(x.Length == y.Length);
            List<DataPoint> points = new List<DataPoint>();

            for (int i = 0; i < x.Length; i++)
            {
                points.Add(new DataPoint((double)x[i], (double)y[i]));
            }

            return points;
        }
    }
}
