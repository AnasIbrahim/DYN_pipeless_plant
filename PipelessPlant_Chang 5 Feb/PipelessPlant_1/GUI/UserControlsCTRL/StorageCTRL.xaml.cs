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
    /// Interaction logic for StorageCTRL.xaml
    /// </summary>
    public partial class StorageCTRL : UserControl
    {
        public Datastructure.Model.Stations.StorageStation sto;

        public StorageCTRL(Datastructure.Model.Stations.StorageStation stat)
        {
            this.sto = stat;

            InitializeComponent();
        }

        private void IsMouseDirectlyOverChanged_HLeft(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderHLeft.Background = Brushes.SteelBlue;
                borderHLeft.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderHLeft.Background = Brushes.LightSteelBlue;
                borderHLeft.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_HRight(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderHRight.Background = Brushes.SteelBlue;
                borderHRight.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderHRight.Background = Brushes.LightSteelBlue;
                borderHRight.BorderBrush = Brushes.LightSteelBlue;
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
                            borderLeft.Background = Brushes.SteelBlue;
                            borderLeft.BorderBrush = Brushes.SteelBlue;
                        }
                        else
                        {
                            borderLeft.Background = Brushes.LightSteelBlue;
                            borderLeft.BorderBrush = Brushes.LightSteelBlue;
                        }
                        break;
                    }
                case 2:
                    {
                        if (on)
                        {
                            borderRight.Background = Brushes.SteelBlue;
                            borderRight.BorderBrush = Brushes.SteelBlue;
                        }
                        else
                        {
                            borderRight.Background = Brushes.LightSteelBlue;
                            borderRight.BorderBrush = Brushes.LightSteelBlue;
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

        private void right_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            //{
            //    int toPos = (int)this.sto.theTraverse - 1;
            //    if (toPos < 1)
            //    {
            //        toPos = 1;
            //    }
            //    Gateway.CTRLModule.getInstance().getStationCTRL(this.sto.theId).moveHArm(toPos, 10);
            //}
          if (Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().Connected)
          {
            GUI.PCSMainWindow.getInstance().postStatusMessage("Horizontal arm of the StorageStation to Center");

            byte[] signals = new byte[64];

            signals[34] = 0;
            signals[35] = 0;
            signals[36] = 0;
            signals[37] = 1;
            signals[38] = 0;
            signals[39] = 0;
            signals[40] = 0;
            signals[41] = 1;
            signals[42] = 0;
            signals[43] = 0;

            Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);

            signals[41] = 0;
            signals[37] = 0;

            Thread.Sleep(4000);

            Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);
          }
        }
        private void left_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //int toPos = (int)this.sto.theTraverse + 1;
            //if (toPos > 7)
            //{
            //    toPos = 7;
            //}
            //Gateway.CTRLModule.getInstance().getStationCTRL(this.sto.theId).moveHArm(toPos, 10);
          //if (Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().Connected)
          //{
          //  GUI.PCSMainWindow.getInstance().postStatusMessage("Horizontal arm of the StorageStation to Pos 6");

          //  byte[] signals = new byte[64];

          //  signals[34] = 0;
          //  signals[35] = 0;
          //  signals[36] = 0;
          //  signals[37] = 0;
          //  signals[38] = 0;
          //  signals[39] = 0;
          //  signals[40] = 1;
          //  signals[41] = 1;
          //  signals[42] = 0;
          //  signals[43] = 0;

          //  Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);

          //  signals[41] = 0;
          //  signals[40] = 0;

          //  Thread.Sleep(4000);

          //  Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);
          //}
        }
        private void up_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            //{
            //    int toPos = (int)this.sto.theAltitude + 1;
            //    if (toPos > 5)
            //    {
            //        toPos = 5;
            //    }
            //    Gateway.CTRLModule.getInstance().getStationCTRL(this.sto.theId).moveVArm(toPos, 10);
            //}

          if (Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().Connected)
          {
            GUI.PCSMainWindow.getInstance().postStatusMessage("Vertical arm of the StorageStation Up");

            byte[] signals = new byte[64];

            signals[28] = 0;
            signals[29] = 0;
            signals[30] = 1;
            signals[31] = 1;
            signals[32] = 0;
            signals[33] = 0;

            Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);

            signals[31] = 0;
            signals[30] = 0;

            Thread.Sleep(6000);

            Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);
          }
          
        }
        private void down_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            //{
            //    int toPos = (int)this.sto.theAltitude - 1;
            //    if (toPos < 1)
            //    {
            //        toPos = 1;
            //    }
            //    Gateway.CTRLModule.getInstance().getStationCTRL(this.sto.theId).moveVArm(toPos, 10);
            //}
          if (Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().Connected)
          {
            GUI.PCSMainWindow.getInstance().postStatusMessage("Vertical arm of the StorageStation to Center");

            byte[] signals = new byte[64];

            signals[28] = 0;
            signals[29] = 1;
            signals[30] = 0;
            signals[31] = 1;
            signals[32] = 0;
            signals[33] = 0;

            Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);

            signals[31] = 0;
            signals[29] = 0;

            Thread.Sleep(6000);

            Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);
          }
        }
        private void grab_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getStationCTRL(this.sto.theId).lockContainer();
            }
        }
        private void release_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getStationCTRL(this.sto.theId).releaseContainer();
            }
        }
        private void ref_Hor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().Connected)
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("Calibrating horizontal arm of the StorageStation...");

                byte[] signals = new byte[64];

                signals[34] = 0;
                signals[35] = 0;
                signals[36] = 0;
                signals[37] = 0;
                signals[38] = 0;
                signals[39] = 0;
                signals[40] = 0;
                signals[41] = 1;
                signals[42] = 0;
                signals[43] = 1;

                Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);

                signals[41] = 0;
                signals[43] = 0;

                Thread.Sleep(10000);

                Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);
            }
            e.Handled = true;
        }
        private void ref_Vert_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().Connected)
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("Calibrating vertical arm of the StorageStation...");

                byte[] signals = new byte[64];

                signals[28] = 0;
                signals[29] = 0;
                signals[30] = 0;
                signals[31] = 1;
                signals[32] = 0;
                signals[33] = 1;

                Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);

                signals[31] = 0;
                signals[33] = 0;

                Thread.Sleep(10000);

                Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);
            }
            e.Handled = true;
        }
        private void stop_Hor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().Connected)
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("Stopping horizontal arm of the StorageStation...");

                byte[] signals = new byte[64];

                signals[34] = 0;
                signals[35] = 0;
                signals[36] = 0;
                signals[37] = 0;
                signals[38] = 0;
                signals[39] = 0;
                signals[40] = 0;
                signals[41] = 0;
                signals[42] = 1;
                signals[43] = 0;

                Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);

                signals[42] = 0;

                Thread.Sleep(250);

                Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);
            }
            e.Handled = true;
        }
        private void stop_Vert_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().Connected)
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("Stopping vertical arm of the StorageStation...");

                byte[] signals = new byte[64];

                signals[28] = 0;
                signals[29] = 0;
                signals[30] = 0;
                signals[31] = 0;
                signals[32] = 1;
                signals[33] = 0;

                Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);

                signals[31] = 0;

                Thread.Sleep(250);

                Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);
            }
            e.Handled = true;
        }

        private void borderVertRef_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderVertRef.Background = Brushes.SteelBlue;
                borderVertRef.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderVertRef.Background = Brushes.LightSteelBlue;
                borderVertRef.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void borderHorRef_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderHorRef.Background = Brushes.SteelBlue;
                borderHorRef.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderHorRef.Background = Brushes.LightSteelBlue;
                borderHorRef.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void borderVertStop_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderVertStop.Background = Brushes.SteelBlue;
                borderVertStop.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderVertStop.Background = Brushes.LightSteelBlue;
                borderVertStop.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void borderHorStop_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderHorStop.Background = Brushes.SteelBlue;
                borderHorStop.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderHorStop.Background = Brushes.LightSteelBlue;
                borderHorStop.BorderBrush = Brushes.LightSteelBlue;
            }
        }
    }
}
