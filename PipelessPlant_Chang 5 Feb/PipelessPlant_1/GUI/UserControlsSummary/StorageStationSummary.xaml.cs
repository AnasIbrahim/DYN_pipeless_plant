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
    /// Interaction logic for StorageStationSummary.xaml
    /// </summary>
    public partial class StorageStationSummary : UserControl
    {
        public int stationID;

        public StorageStationSummary(Datastructure.Model.Stations.StorageStation sto)
        {
            this.stationID = sto.theId;

            InitializeComponent();

            updateUC(sto);
        }

        public void updateUC(Datastructure.Model.Stations.StorageStation sto)
        {
            labelID.Content = "ID: " + sto.theId;
            labelRotation.Content = "Orientation: " + sto.theRotation;
            labelPosX.Content = "Position x: " + sto.thePosition.X;
            labelPosY.Content = "Position y: " + sto.thePosition.Y;
            labelVessel.Content = "Vessel-ID: " + (sto.theVessels[0] == null ? "null" : ("" + sto.theVessels[0].theId));
            labelAltitude.Content = "Altitude: " + sto.theAltitude;
            labelTraverse.Content = "Traverse: " + sto.theTraverse;
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
            Gateway.ObserverModule.getInstance().theVisual.selectStorageStation(stationID);
        }
    }
}
