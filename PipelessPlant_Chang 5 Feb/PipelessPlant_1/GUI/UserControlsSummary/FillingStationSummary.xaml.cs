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
    /// Interaction logic for FillingStationSummary.xaml
    /// </summary>
    public partial class FillingStationSummary : UserControl
    {
        public int stationID;

        public FillingStationSummary(Datastructure.Model.Stations.FillingStation fill)
        {
            this.stationID = fill.theId;

            InitializeComponent();

            updateUC(fill);
        }

        public void updateUC(Datastructure.Model.Stations.FillingStation fill)
        {
            labelID.Content = "ID: " + fill.theId;
            labelRotation.Content = "Orientation: " + fill.theRotation;
            labelPosX.Content = "Position x: " + fill.thePosition.X;
            labelPosY.Content = "Position y: " + fill.thePosition.Y;
            labelFilling.Content = "Filling: " + (fill.theFillRate_1 >= 0.01 ? "Yes" : "No") + " and " + (fill.theFillRate_2 >= 0.01 ? "Yes" : "No");
            labelColorID.Content = "Colors: " + Datastructure.Model.MyColors.getInstance().getName(fill.theColorID[0]) + " and " + Datastructure.Model.MyColors.getInstance().getName(fill.theColorID[1]);
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
            Gateway.ObserverModule.getInstance().theVisual.selectFillingStation(stationID);
        }
    }
}
