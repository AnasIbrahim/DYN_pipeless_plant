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
using System.Windows.Threading;
using MULTIFORM_PCS;

namespace MULTIFORM_PCS.GUI.UserControlsCTRL
{
    /// <summary>
    /// Interaction logic for AGVCTRL.xaml
    /// </summary>
    public partial class AGVCTRL : UserControl
    {
        public int agvID;
        public bool setNewGoal;

        public AGVCTRL(int id)
        {
            this.agvID = id;
            this.setNewGoal = false;

            InitializeComponent();

            chargeDock.MouseLeftButtonDown += (o, a) =>
            {
                Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().robotFromId(id).DockDemo();
            };
        }

        private void IsMouseDirectlyOverChanged_LeftButton(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderLeft.Background = Brushes.SteelBlue;
                borderLeft.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderLeft.Background = Brushes.LightSteelBlue;
                borderLeft.BorderBrush = Brushes.LightSteelBlue;
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
                            borderW.Background = Brushes.SteelBlue;
                            borderW.BorderBrush = Brushes.SteelBlue;
                        }
                        else
                        {
                            borderW.Background = Brushes.LightSteelBlue;
                            borderW.BorderBrush = Brushes.LightSteelBlue;
                        }
                        break;
                    }
                case 2:
                    {
                        if (on)
                        {
                            borderS.Background = Brushes.SteelBlue;
                            borderS.BorderBrush = Brushes.SteelBlue;
                        }
                        else
                        {
                            borderS.Background = Brushes.LightSteelBlue;
                            borderS.BorderBrush = Brushes.LightSteelBlue;
                        }
                        break;
                    }
                case 3:
                    {
                        if (on)
                        {
                            borderA.Background = Brushes.SteelBlue;
                            borderA.BorderBrush = Brushes.SteelBlue;
                        }
                        else
                        {
                            borderA.Background = Brushes.LightSteelBlue;
                            borderA.BorderBrush = Brushes.LightSteelBlue;
                        }
                        break;
                    }
                case 4:
                    {
                        if (on)
                        {
                            borderD.Background = Brushes.SteelBlue;
                            borderD.BorderBrush = Brushes.SteelBlue;
                        }
                        else
                        {
                            borderD.Background = Brushes.LightSteelBlue;
                            borderD.BorderBrush = Brushes.LightSteelBlue;
                        }
                        break;
                    }
            }
        }

        private void IsMouseDirectlyOverChanged_Forward(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderForward.Background = Brushes.SteelBlue;
                borderForward.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderForward.Background = Brushes.LightSteelBlue;
                borderForward.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void IsMouseDirectlyOverChanged_Backward(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderBackward.Background = Brushes.SteelBlue;
                borderBackward.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderBackward.Background = Brushes.LightSteelBlue;
                borderBackward.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void IsMouseDirectlyOverChanged_Right(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderRight.Background = Brushes.SteelBlue;
                borderRight.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderRight.Background = Brushes.LightSteelBlue;
                borderRight.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void forward_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
  
                Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(this.agvID).forward(25, 0, 0, 0);
            }
        }
        private void forward_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(this.agvID).forward(0, 0, 0, 0);
            }
        }
        private void left_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(this.agvID).turnLeft(25, 0, 0);
            }
        }
        private void left_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(this.agvID).turnLeft(0, 0, 0);
            }
        }
        private void backward_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(this.agvID).backward(25, 0, 0, 0);
            }
        }
        private void backward_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(this.agvID).backward(0, 0, 0, 0);
            }
        }
        private void right_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(this.agvID).turnRight(25, 0, 0);
            }
        }
        private void right_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(this.agvID).turnRight(0, 0, 0);
            }
        }

        private void IsMouseDirectlyOverChanged_Dock(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderDock.Background = Brushes.SteelBlue;
                borderDock.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderDock.Background = Brushes.LightSteelBlue;
                borderDock.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void IsMouseDirectlyOverChanged_UnDock(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderUnDock.Background = Brushes.SteelBlue;
                borderUnDock.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderUnDock.Background = Brushes.LightSteelBlue;
                borderUnDock.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void dock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(this.agvID).dock();
        }

        private void unDock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(this.agvID).unDock();
        }

        private void MoveToGoal_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            setNewGoal = true;
            borderMoveToGoal.Background = Brushes.White;
            borderMoveToGoal.BorderBrush = Brushes.White;
        }
        public void setGoalForRobot(int x, int y)
        {
            Gateway.ConnectionModule.iRobot.iRobotRouter.Robot rob = Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().robotFromId(agvID);
            if (rob != null)
            {
                rob.SetGoal(new Gateway.ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new Gateway.ConnectionModule.iRobot.iRobotVector(x * 10, y * 10), 0));
                Gateway.ObserverModule.getInstance().getCurrentPlant().getAGV(agvID).GoalRot = 0;
                Gateway.ObserverModule.getInstance().getCurrentPlant().getAGV(agvID).GoalX = x * 10;
                Gateway.ObserverModule.getInstance().getCurrentPlant().getAGV(agvID).GoalY = y * 10;
            }
            else
            {
                setNewGoal = false;
                borderMoveToGoal.Background = Brushes.LightSteelBlue;
                borderMoveToGoal.BorderBrush = Brushes.LightSteelBlue;
                PCSMainWindow.getInstance().postStatusMessage("Selected robot not connected to the PCS...");
            }
        }

        private void IsMouseDirectlyOverChanged_MoveToGoal(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderMoveToGoal.Background = Brushes.SteelBlue;
                borderMoveToGoal.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (setNewGoal)
                {
                    borderMoveToGoal.Background = Brushes.White;
                    borderMoveToGoal.BorderBrush = Brushes.White;
                }
                else
                {
                    borderMoveToGoal.Background = Brushes.LightSteelBlue;
                    borderMoveToGoal.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }
    }
}
