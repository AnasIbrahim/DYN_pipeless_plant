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

namespace MULTIFORM_PCS.GUI.UserControlsCTRL
{
    /// <summary>
    /// Interaction logic for ChargingCTRL.xaml
    /// </summary>
    public partial class ChargingCTRL : UserControl
    {
        public int stationID;

        public ChargingCTRL(int id)
        {
            this.stationID = id;

            InitializeComponent();
        }

        private void IsMouseDirectlyOverChanged_Charging(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderCharging.Background = Brushes.SteelBlue;
                borderCharging.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderCharging.Background = Brushes.LightSteelBlue;
                borderCharging.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        public void markKey(int key, bool on)
        {
            switch (key)
            {
                case 1:
                    {
                        if (on)
                        {
                            borderC.Background = Brushes.SteelBlue;
                            borderC.BorderBrush = Brushes.SteelBlue;
                        }
                        else
                        {
                            borderC.Background = Brushes.LightSteelBlue;
                            borderC.BorderBrush = Brushes.LightSteelBlue;
                        }
                        break;
                    }
            }
        }

        private void charging_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getStationCTRL(this.stationID).startLoading();
            }
        }
        private void charging_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getStationCTRL(this.stationID).stopLoading();
            }
        }
    }
}
