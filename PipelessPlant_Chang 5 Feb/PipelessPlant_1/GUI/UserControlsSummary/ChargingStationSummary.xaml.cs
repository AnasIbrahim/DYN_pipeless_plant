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
    /// Interaction logic for ChargingStationCTRL.xaml
    /// </summary>
    public partial class ChargingStationSummary : UserControl
    {
        public int stationID;

        public ChargingStationSummary(Datastructure.Model.Stations.ChargingStation cha)
        {
            this.stationID = cha.theId;

            InitializeComponent();

            updateUC(cha);
        }

        public void updateUC(Datastructure.Model.Stations.ChargingStation cha)
        {
            labelID.Content = "ID: " + cha.theId;
            labelRotation.Content = "Orientation: " + cha.theRotation;
            labelPosX.Content = "Position x: " + cha.thePosition.X;
            labelPosY.Content = "Position y: " + cha.thePosition.Y;
            labelCharging.Content = "Charging: " + (cha.theLoadRate >= 0.01 ? "Yes" : "No");
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
            Gateway.ObserverModule.getInstance().theVisual.selectChargingStation(stationID);
        }
    }
}
