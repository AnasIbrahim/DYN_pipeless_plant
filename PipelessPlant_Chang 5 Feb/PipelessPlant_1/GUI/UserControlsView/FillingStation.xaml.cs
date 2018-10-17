using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MULTIFORM_PCS.GUI.UserControlsView
{
    /// <summary>
    /// Interaction logic for Station.xaml
    /// </summary>
    public partial class FillingStation : UserControl
    {
        public int stationID;
        private LinearGradientBrush green;
        private LinearGradientBrush red;
        private SolidColorBrush[] basicColors = new SolidColorBrush[] { Brushes.Yellow, Brushes.Black, Brushes.Red, Brushes.Blue };

        public FillingStation(Datastructure.Model.Stations.FillingStation fill)
        {
            InitializeComponent();

            this.stationID = fill.theId;

            red = new LinearGradientBrush();
            red.EndPoint = new Point(1, 1);
            red.StartPoint = new Point(0.5, 0.5);
            GradientStop gs1 = new GradientStop(((System.Windows.Media.SolidColorBrush)Brushes.Red).Color, 0);
            GradientStop gs2 = new GradientStop(((System.Windows.Media.SolidColorBrush)Brushes.White).Color, 1);
            red.GradientStops.Add(gs1);
            red.GradientStops.Add(gs2);

            green = new LinearGradientBrush();
            green.EndPoint = new Point(1, 1);
            green.StartPoint = new Point(0.5, 0.5);
            GradientStop gs3 = new GradientStop(((System.Windows.Media.SolidColorBrush)Brushes.Green).Color, 0);
            GradientStop gs4 = new GradientStop(((System.Windows.Media.SolidColorBrush)Brushes.White).Color, 1);
            green.GradientStops.Add(gs3);
            green.GradientStops.Add(gs4);

            this.RenderTransform = new RotateTransform(-fill.theRotation + 270, 70.71 / 2, 70.71 / 2);

            LinearGradientBrush col1 = new LinearGradientBrush();
            col1.EndPoint = new Point(1,1);
            col1.StartPoint = new Point(0.5,0.5);
            col1.GradientStops = new GradientStopCollection();
            GradientStop color1 = new GradientStop(((System.Windows.Media.SolidColorBrush)basicColors[fill.theColorID[1]]).Color, 0);
            GradientStop white1 = new GradientStop(((System.Windows.Media.SolidColorBrush)Brushes.White).Color, 1);
            col1.GradientStops.Add(color1);
            col1.GradientStops.Add(white1);
            rectangleColor1.Fill = col1;

            LinearGradientBrush col2 = new LinearGradientBrush();
            col2.EndPoint = new Point(1, 1);
            col2.StartPoint = new Point(0.5, 0.5);
            col2.GradientStops = new GradientStopCollection();
            GradientStop color2 = new GradientStop(((System.Windows.Media.SolidColorBrush)basicColors[fill.theColorID[0]]).Color, 0);
            GradientStop white2 = new GradientStop(((System.Windows.Media.SolidColorBrush)Brushes.White).Color, 1);
            col2.GradientStops.Add(color2);
            col2.GradientStops.Add(white2);
            rectangleColor2.Fill = col2;

            updateUC(fill);
        }

        public void updateUC(Datastructure.Model.Stations.FillingStation fill)
        {
            if (fill.theFillRate_1 >= 0.01)
            {
                rectangleColor1CTRL.Fill = green;
            }
            else
            {
                rectangleColor1CTRL.Fill = red;
            }
            if (fill.theFillRate_2 >= 0.01)
            {
                rectangleColor2CTRL.Fill = green;
            }
            else
            {
                rectangleColor2CTRL.Fill = red;
            }
        }

        public void markSelection(bool selected)
        {
            if (selected)
            {
                l1.Stroke = Brushes.Red;
                l2.Stroke = Brushes.Red;
                ellipseDockingSpot.Stroke = Brushes.Red;
            }
            else
            {
                l1.Stroke = Brushes.Black;
                l2.Stroke = Brushes.Black;
                ellipseDockingSpot.Stroke = Brushes.LightGray;
            }
        }
    }
}
