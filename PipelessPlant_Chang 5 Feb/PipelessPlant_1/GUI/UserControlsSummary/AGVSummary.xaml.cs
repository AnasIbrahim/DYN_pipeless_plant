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
    /// Interaction logic for AGVCTRL.xaml
    /// </summary>
    public partial class AGVSummary : UserControl
    {
        private Datastructure.Model.AGV.AGV displayedAGV;

        public AGVSummary(Datastructure.Model.AGV.AGV agv)
        {
            this.displayedAGV = agv;

            InitializeComponent();

            updateUC();
        }

        public void updateUC()
        {
            labelID.Content = "ID: " + displayedAGV.Id;
            labelRotation.Content = "Orientation: " + displayedAGV.theRotation;
            labelPosX.Content = "Position x: " + displayedAGV.theCurPosition.X;
            labelPosY.Content = "Position y: " + displayedAGV.theCurPosition.Y;
            labelVessel.Content = "Vessel-ID: " + ((displayedAGV.theVessel == null) ? "null" : "" + displayedAGV.theVessel.theId);
            labelBattery.Content = "Battery Status: " + displayedAGV.theBatteryLoad;
            labelCharging.Content = "Charging Status: " + displayedAGV.theChargingStatus;
            if (displayedAGV.GoalX != -1 && displayedAGV.GoalY != -1)
            {
                labelGoal.Content = "Goal: (" + displayedAGV.GoalX + "/" + displayedAGV.GoalY + ")";
            }
            else
            {
                labelGoal.Content = "Goal: -";
            }
            if (displayedAGV.GoalRot != -1)
            {
                labelGoal.Content += " " + displayedAGV.GoalRot;
            }

            if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router != null)
            {
                for (int i = 0; i < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; i++)
                {
                    if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[i].RobotID == displayedAGV.Id)
                    {
                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[i].IsConnected)
                        {
                            labelConected.Content = "Connected: true";
                            PCSMainWindow.getInstance().setAGVLED(displayedAGV.Id, true);
                        }
                        else
                        {
                            labelConected.Content = "Connected: false";
                            PCSMainWindow.getInstance().setAGVLED(displayedAGV.Id, false);
                        }
                        labelIP.Content = "Adress: " + Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[i].Ip;
                        labelPort.Content = "Port: " + Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[i].Port;
                        return;
                    }
                }
            }
            labelConected.Content = "Connected: false";
            labelIP.Content = "Adress: -";
            labelPort.Content = "Port: -";
            PCSMainWindow.getInstance().setAGVLED(displayedAGV.Id, false);
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
            Gateway.ObserverModule.getInstance().theVisual.selectAGV(displayedAGV.Id);
        }
    }
}
