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

namespace MULTIFORM_PCS.GUI
{
    /// <summary>
    /// Interaktionslogik für AutomaticExecution.xaml
    /// </summary>
    public partial class AutomaticExecution : Window
    {
        private bool[] accEmptyVessel = new bool[] { false, false, false, false, false, false };
        private int[] selectedAGVs = new int[] { -1, -1 };

        public AutomaticExecution()
        {
            InitializeComponent();
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender == el1)
            {
                accEmptyVessel[0] = !accEmptyVessel[0];
                if (accEmptyVessel[0])
                {
                    el1.Fill = (Brush)this.FindResource("LGBGreen");
                }
                else
                {
                    el1.Fill = (Brush)this.FindResource("LGBRed");
                }
            }
            else if (sender == el2)
            {
                accEmptyVessel[1] = !accEmptyVessel[1];
                if (accEmptyVessel[1])
                {
                    el2.Fill = (Brush)this.FindResource("LGBGreen");
                }
                else
                {
                    el2.Fill = (Brush)this.FindResource("LGBRed");
                }
            }
            else if (sender == el3)
            {
                accEmptyVessel[2] = !accEmptyVessel[2];
                if (accEmptyVessel[2])
                {
                    el3.Fill = (Brush)this.FindResource("LGBGreen");
                }
                else
                {
                    el3.Fill = (Brush)this.FindResource("LGBRed");
                }
            }
            else if (sender == el4)
            {
                accEmptyVessel[3] = !accEmptyVessel[3];
                if (accEmptyVessel[3])
                {
                    el4.Fill = (Brush)this.FindResource("LGBGreen");
                }
                else
                {
                    el4.Fill = (Brush)this.FindResource("LGBRed");
                }
            }
            else if (sender == el5)
            {
                accEmptyVessel[4] = !accEmptyVessel[4];
                if (accEmptyVessel[4])
                {
                    el5.Fill = (Brush)this.FindResource("LGBGreen");
                }
                else
                {
                    el5.Fill = (Brush)this.FindResource("LGBRed");
                }
            }
            else if (sender == el6)
            {
                accEmptyVessel[5] = !accEmptyVessel[5];
                if (accEmptyVessel[5])
                {
                    el6.Fill = (Brush)this.FindResource("LGBGreen");
                }
                else
                {
                    el6.Fill = (Brush)this.FindResource("LGBRed");
                }
            }
            e.Handled = true;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == cb1)
            {
                selectedAGVs[0] = cb1.SelectedIndex;
            }
            else if (sender == cb2)
            {
                selectedAGVs[1] = cb2.SelectedIndex;
            }
            if (selectedAGVs[0] != selectedAGVs[1] && selectedAGVs[0] != -1 && selectedAGVs[1] != -1)
            {
                elAGV.Fill = (Brush)this.FindResource("LGBGreen");
            }
            else
            {
                elAGV.Fill = (Brush)this.FindResource("LGBRed");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            tbStatus.Text = "";
            for (int i = 0; i < accEmptyVessel.Length; i++)
            {
                if (accEmptyVessel[i] == false)
                {
                    tbStatus.Text = "Not all vessels as empty acknowledged!";
                    return;
                }
            }
            if (selectedAGVs[0] == selectedAGVs[1] || selectedAGVs[0] == -1 || selectedAGVs[1] == -1)
            {
                tbStatus.Text = "Not two different AGVs selected!";
                return;
            }

            if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router != null || Gateway.CTRLModule.getInstance().Debug)
            {
                if (!Gateway.CTRLModule.getInstance().Debug)
                {
                    for (int i = 0; i < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; i++)
                    {
                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[i].RobotID == selectedAGVs[0])
                        {
                            if (!Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[i].IsConnected)
                            {
                                tbStatus.Text = "Selected AGV 1 not connected to PCS!";
                                return;
                            }
                        }
                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[i].RobotID == selectedAGVs[1])
                        {
                            if (!Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[i].IsConnected)
                            {
                                tbStatus.Text = "Selected AGV 2 not connected to PCS!";
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                tbStatus.Text = "iRobot server not started!";
                return;
            }

            Gateway.CTRLModule.getInstance().startExecution(selectedAGVs[0] ,selectedAGVs[1]);

            this.Close();
        }
    }
}
