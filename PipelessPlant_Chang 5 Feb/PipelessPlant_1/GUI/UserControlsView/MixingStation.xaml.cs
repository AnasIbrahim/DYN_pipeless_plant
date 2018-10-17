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
    public partial class MixingStation : UserControl
    {
        private double aMin;
        private double aMax;
        public int stationID;
        private LinearGradientBrush green;
        private LinearGradientBrush red;

        public MixingStation(Datastructure.Model.Stations.MixingStation mix, UserControlsView.Vessel vUC)
        {
            this.aMin = mix.theArmpositions[0];
            this.aMax = mix.theArmpositions[4];

            this.stationID = mix.theId;

            InitializeComponent();

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

            this.RenderTransform = new RotateTransform(-mix.theRotation + 270, 70.71 / 2, 70.71 / 2);

            updateUC(mix, vUC);
        }

        public void updateUC(Datastructure.Model.Stations.MixingStation mix, UserControlsView.Vessel vUC)
        {
            /**
             * 5-40
             */
            if (mix.theAltitude == 1)
            {
                labelVArmPosition.Content = "D";
            }
            else
            {
                labelVArmPosition.Content = "U";
            }
            if (vUC != null && vUC.isChildOf != true)
            {
                mainGrid.Children.Add(vUC);
                vUC.isChildOf = true;
                vUC.Margin = new Thickness(30.85, 50.35, 0, 0);
            }
            if (mix.theRotating)
            {
                rectangleMixingCTRL.Fill = green;
            }
            else
            {
                rectangleMixingCTRL.Fill = red;
            }
            if (mix.theFillRate >= 0.01)
            {
                rectangleGipsumCTRL.Fill = green;
            }
            else
            {
                rectangleGipsumCTRL.Fill = red;
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

        public void removeAllVessels(List<VesselUserControls> vessels)
        {
            for (int i = 0; i < vessels.Count; i++)
            {
                if (mainGrid.Children.Contains(vessels[i].vesView))
                {
                    mainGrid.Children.Remove(vessels[i].vesView);
                    vessels[i].vesView.isChildOf = false;
                }
            }
        }
    }
}
