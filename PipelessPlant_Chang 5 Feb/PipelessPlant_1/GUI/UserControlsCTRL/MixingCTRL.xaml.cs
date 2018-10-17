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
using System.Threading;


namespace MULTIFORM_PCS.GUI.UserControlsCTRL
{
    /// <summary>
    /// Interaction logic for MixingCTRL.xaml
    /// </summary>
    public partial class MixingCTRL : UserControl
    {
        public int stationID;

        public MixingCTRL(int id)
        {
            this.stationID = id;

            InitializeComponent();
        }

        private void IsMouseDirectlyOverChanged_Mix(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderMix.Background = Brushes.SteelBlue;
                borderMix.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderMix.Background = Brushes.LightSteelBlue;
                borderMix.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_Fill(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderFill.Background = Brushes.SteelBlue;
                borderFill.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderFill.Background = Brushes.LightSteelBlue;
                borderFill.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_Up(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderArmUp.Background = Brushes.SteelBlue;
                borderArmUp.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderArmUp.Background = Brushes.LightSteelBlue;
                borderArmUp.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_Down(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderArmDown.Background = Brushes.SteelBlue;
                borderArmDown.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderArmDown.Background = Brushes.LightSteelBlue;
                borderArmDown.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_Grab(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderArmGrab.Background = Brushes.SteelBlue;
                borderArmGrab.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderArmGrab.Background = Brushes.LightSteelBlue;
                borderArmGrab.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_Release(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderArmRelease.Background = Brushes.SteelBlue;
                borderArmRelease.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderArmRelease.Background = Brushes.LightSteelBlue;
                borderArmRelease.BorderBrush = Brushes.LightSteelBlue;
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
                            borderM.Background = Brushes.SteelBlue;
                            borderM.BorderBrush = Brushes.SteelBlue;
                        }
                        else
                        {
                            borderM.Background = Brushes.LightSteelBlue;
                            borderM.BorderBrush = Brushes.LightSteelBlue;
                        }
                        break;
                    }
                case 2:
                    {
                        if (on)
                        {
                            borderF.Background = Brushes.SteelBlue;
                            borderF.BorderBrush = Brushes.SteelBlue;
                        }
                        else
                        {
                            borderF.Background = Brushes.LightSteelBlue;
                            borderF.BorderBrush = Brushes.LightSteelBlue;
                        }
                        break;
                    }
                case 3:
                    {
                        if (on)
                        {
                            borderUp.Background = Brushes.SteelBlue;
                            borderUp.BorderBrush = Brushes.SteelBlue;
                        }
                        else
                        {
                            borderUp.Background = Brushes.LightSteelBlue;
                            borderUp.BorderBrush = Brushes.LightSteelBlue;
                        }
                        break;
                    }
                case 4:
                    {
                        if (on)
                        {
                            borderDown.Background = Brushes.SteelBlue;
                            borderDown.BorderBrush = Brushes.SteelBlue;
                        }
                        else
                        {
                            borderDown.Background = Brushes.LightSteelBlue;
                            borderDown.BorderBrush = Brushes.LightSteelBlue;
                        }
                        break;
                    }
                case 5:
                    {
                        if (on)
                        {
                            borderG.Background = Brushes.SteelBlue;
                            borderG.BorderBrush = Brushes.SteelBlue;
                        }
                        else
                        {
                            borderG.Background = Brushes.LightSteelBlue;
                            borderG.BorderBrush = Brushes.LightSteelBlue;
                        }
                        break;
                    }
                case 6:
                    {
                        if (on)
                        {
                            borderR.Background = Brushes.SteelBlue;
                            borderR.BorderBrush = Brushes.SteelBlue;
                        }
                        else
                        {
                            borderR.Background = Brushes.LightSteelBlue;
                            borderR.BorderBrush = Brushes.LightSteelBlue;
                        }
                        break;
                    }
            }
        }

        private void mix_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getStationCTRL(this.stationID).startMixing();
            }
        }
        private void mix_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getStationCTRL(this.stationID).stopMixing();
            }
        }
        private void fill_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getStationCTRL(this.stationID).startFilling(0);
            }
        }
        private void fill_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getStationCTRL(this.stationID).stopFilling(0);
            }
        }
        private void up_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ////if (Gateway.CTRLModule.getInstance().SimulationRunning)
            //{
            //    Gateway.CTRLModule.getInstance().getStationCTRL(this.stationID).moveVArm(5, 10);
            //}
            if (Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().Connected)
            {
              GUI.PCSMainWindow.getInstance().postStatusMessage("Vertical arm of the MixingStation to Pos1");

              byte[] signals = new byte[64];

              signals[47] = 0;
              signals[48] = 1;
              signals[49] = 0;
              signals[50] = 0;

              signals[55] = 1;
              signals[56] = 0;
              signals[57] = 0;
              signals[58] = 0;

              Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);

              signals[55] = 0;
              signals[48] = 0;

              Thread.Sleep(6000);

              Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);
            }
        }
        private void down_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            //{
            //    Gateway.CTRLModule.getInstance().getStationCTRL(this.stationID).moveVArm(1, 10);
            //}
          if (Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().Connected)
          {
            GUI.PCSMainWindow.getInstance().postStatusMessage("Vertical arm of the MixingStation Down");

            byte[] signals = new byte[64];

            signals[47] = 1;
            signals[48] = 0;
            signals[49] = 0;
            signals[50] = 0;

            signals[55] = 1;
            signals[56] = 0;
            signals[57] = 0;
            signals[58] = 0;

            Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);

            Thread.Sleep(1000);

            signals[55] = 0;
            signals[47] = 0;

            Thread.Sleep(6000);

            Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);
          }
        }
        private void grab_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getStationCTRL(this.stationID).lockContainer();
            }
        }
        private void release_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getStationCTRL(this.stationID).releaseContainer();
            }
        }

        private void ref_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().Connected)
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("Calibrating vertical arm of the MixingStation...");

                byte[] signals = new byte[64];

                signals[47] = 0;
                signals[48] = 0;
                signals[49] = 0;
                signals[50] = 0;

                signals[55] = 1;
                signals[56] = 0;
                signals[57] = 1;
                signals[58] = 0;

                Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);

                Thread.Sleep(1000);

                signals[55] = 0;
                signals[57] = 0;

                Thread.Sleep(20000);

                Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);            
            }
            e.Handled = true;
        }
        private void stop_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().Connected)
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("Calibrating vertical arm of the MixingStation...");

                byte[] signals = new byte[64];

                signals[47] = 0;
                signals[48] = 0;
                signals[49] = 0;
                signals[50] = 0;
                signals[51] = 0;
                signals[52] = 0;
                signals[53] = 0;
                signals[54] = 0;
                signals[55] = 0;
                signals[56] = 1;
                signals[57] = 0;

                Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);

                signals[55] = 0;

                Thread.Sleep(250);

                Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);
            }
            e.Handled = true;
        }

        private void IsMouseDirectlyOverChanged_Stop(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderArmStop.Background = Brushes.SteelBlue;
                borderArmStop.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderArmStop.Background = Brushes.LightSteelBlue;
                borderArmStop.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void IsMouseDirectlyOverChanged_Reference(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderArmRef.Background = Brushes.SteelBlue;
                borderArmRef.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderArmRef.Background = Brushes.LightSteelBlue;
                borderArmRef.BorderBrush = Brushes.LightSteelBlue;
            }
        }

    }
}
