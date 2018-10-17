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
    /// Interaction logic for ChargingStation.xaml
    /// </summary>
    public partial class ChargingStation : UserControl
    {
        public int stationID;
        private LinearGradientBrush red;
        private LinearGradientBrush green;

        public ChargingStation(Datastructure.Model.Stations.ChargingStation cha)
        {
            InitializeComponent();

            this.stationID = cha.theId;

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

            this.RenderTransform = new RotateTransform(-cha.theRotation + 270, 45 / 2, 45 / 2);

            updateUC(cha);
        }

        public void updateUC(Datastructure.Model.Stations.ChargingStation cha)
        {
            if (cha.theLoadRate >= 0.01)
            {
                rectangleChargingCTRL.Fill = green;
            }
            else
            {
                rectangleChargingCTRL.Fill = red;
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
