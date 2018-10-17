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
    /// Interaction logic for FillingCTRL.xaml
    /// </summary>
    public partial class FillingCTRL : UserControl
    {
        public Datastructure.Model.Stations.FillingStation fill;
        private SolidColorBrush[] basicColors = new SolidColorBrush[] { Brushes.Yellow, Brushes.Black, Brushes.Red, Brushes.Blue };

        public FillingCTRL(Datastructure.Model.Stations.FillingStation stat)
        {
            this.fill = stat;

            InitializeComponent();

            borderFillColor1.BorderBrush = basicColors[fill.theColorID[0]];
            borderFillColor2.BorderBrush = basicColors[fill.theColorID[1]];
            borderF.BorderBrush = basicColors[fill.theColorID[0]];
            borderG.BorderBrush = basicColors[fill.theColorID[1]];
        }

        private void IsMouseDirectlyOverChanged_FillColor1(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderFillColor1.Background = Brushes.SteelBlue;
                //borderFillColor1.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderFillColor1.Background = Brushes.LightSteelBlue;
                //borderFillColor1.BorderBrush = basicColors[fill.theColorID[0]];
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
                            borderF.Background = Brushes.SteelBlue;
                            //borderF.BorderBrush = Brushes.SteelBlue;
                        }
                        else
                        {
                            borderF.Background = Brushes.LightSteelBlue;
                            //borderF.BorderBrush = basicColors[fill.theColorID[0]];
                        }
                        break;
                    }
                case 2:
                    {
                        if (on)
                        {
                            borderG.Background = Brushes.SteelBlue;
                            //borderG.BorderBrush = Brushes.SteelBlue;
                        }
                        else
                        {
                            borderG.Background = Brushes.LightSteelBlue;
                            //borderG.BorderBrush = basicColors[fill.theColorID[1]];
                        }
                        break;
                    }
            }
        }

        private void IsMouseDirectlyOverChanged_FillColor2(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderFillColor2.Background = Brushes.SteelBlue;
                //borderFillColor2.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderFillColor2.Background = Brushes.LightSteelBlue;
                //borderFillColor2.BorderBrush = basicColors[fill.theColorID[1]];
            }
        }

        private void fillColor1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getStationCTRL(this.fill.theId).startFilling(0);
            }
        }
        private void fillColor1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getStationCTRL(this.fill.theId).stopFilling(0);
            }
        }
        private void fillColor2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getStationCTRL(this.fill.theId).startFilling(1);
            }
        }
        private void fillColor2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getStationCTRL(this.fill.theId).stopFilling(1);
            }
        }
    }
}
