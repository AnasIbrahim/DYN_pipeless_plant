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

namespace MULTIFORM_PCS.GUI.UserControlsSummary
{
    /// <summary>
    /// Interaction logic for MixingStationSummary.xaml
    /// </summary>
    public partial class MixingStationSummary : UserControl
    {
        public int stationID;

        public MixingStationSummary(Datastructure.Model.Stations.MixingStation mix)
        {
            this.stationID = mix.theId;

            InitializeComponent();

            updateUC(mix);
        }

        public void updateUC(Datastructure.Model.Stations.MixingStation mix)
        {
            labelID.Content = "ID: " + mix.theId;
            labelRotation.Content = "Orientation: " + mix.theRotation;
            labelPosX.Content = "Position x: " + mix.thePosition.X;
            labelPosY.Content = "Position y: " + mix.thePosition.Y;
            labelVessel.Content = "Vessel-ID: " + (mix.theCurrentVessel == null ? "null" : ("" + mix.theCurrentVessel.theId));
            labelAltitude.Content = "Altitude: " + mix.theAltitude;
            labelFilling.Content = "Filling: " + (mix.theFillRate >= 0.01 ? "Yes" : "No");
            labelMixing.Content = "Mixing: " + (mix.theRotating ? "Yes" : "No");
        }

        public void markSelection(bool selected)
        {
            if (selected)
            {
                border1.Background = (Brush)this.FindResource("LGBGray");
                borderIcon.Background = Brushes.LightSteelBlue;
            }
            else
            {
                border1.Background = (Brush)this.FindResource("LGBSteelBlue");
                borderIcon.Background = Brushes.SteelBlue;
            }
        }

        private void UserControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().theVisual.selectMixingStation(stationID);
        }
    }
}
