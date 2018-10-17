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

namespace MULTIFORM_PCS.GUI.UserControlsView
{
    /// <summary>
    /// Interaction logic for Station.xaml
    /// </summary>
    public partial class StorageStation : UserControl
    {
        private double aMin;
        private double aMax;
        public int stationID;
        private LinearGradientBrush green;
        private LinearGradientBrush red;

        public StorageStation(Datastructure.Model.Stations.StorageStation sto, List<UserControlsView.Vessel> vesselsUC)
        {
            this.aMin = sto.theVArmPositions[0];
            this.aMax = sto.theVArmPositions[4];
            this.stationID = sto.theId;

            InitializeComponent();

            red = new LinearGradientBrush();
            red.EndPoint = new Point(1, 1);
            red.StartPoint = new Point(0.5, 0.5);
            GradientStop gs1 = new GradientStop(((System.Windows.Media.SolidColorBrush)Brushes.Red).Color, 0);
            GradientStop gs2 = new GradientStop(((System.Windows.Media.SolidColorBrush)Brushes.White).Color, 1);
            red.GradientStops.Add(gs1);
            red.GradientStops.Add(gs2);

            green = new LinearGradientBrush();
            green.EndPoint = new Point(1, 1);
            green.StartPoint = new Point(0.5, 0.5);
            GradientStop gs3 = new GradientStop(((System.Windows.Media.SolidColorBrush)Brushes.Green).Color, 0);
            GradientStop gs4 = new GradientStop(((System.Windows.Media.SolidColorBrush)Brushes.White).Color, 1);
            green.GradientStops.Add(gs3);
            green.GradientStops.Add(gs4);

            this.RenderTransform = new RotateTransform(-sto.theRotation + 270, 90 / 2, 90 / 2);

            updateUC(sto, vesselsUC);
        }

        public void removeAllVessels(List<VesselUserControls> vessels)
        {
            for(int i = 0; i < vessels.Count; i++)
            {
                if(mainGrid.Children.Contains(vessels[i].vesView))
                {
                    mainGrid.Children.Remove(vessels[i].vesView);
                    vessels[i].vesView.isChildOf = false;
                }
            }
        }
        public void updateUC(Datastructure.Model.Stations.StorageStation sto, List<UserControlsView.Vessel> vesselsUC)
        {
            /**
             * 5-40
             */
            if (sto.theAltitude == 1)
            {
                labelVArmPos.Content = "D";
            }
            else if(sto.theAltitude == 3)
            {
                labelVArmPos.Content = "C";
            }
            else if (sto.theAltitude == 5)
            {
                labelVArmPos.Content = "U";
            }
            rectangleHArm.Margin = new Thickness(26.5, 27.5, 0, 0);
            spot1.Margin = new Thickness(36.5, 21.5, 0, 0);
            spot2.Margin = new Thickness(47, 21.5, 0, 0);
            spot3.Margin = new Thickness(57.5, 21.5, 0, 0);
            spot4.Margin = new Thickness(82.5, 21.5, 0, 0);
            spot5.Margin = new Thickness(93, 21.5, 0, 0);
            spot6.Margin = new Thickness(103.5, 21.5, 0, 0);
            for (int i = 0; i < vesselsUC.Count; i++)
            {
                if (vesselsUC[i] == null || vesselsUC[i].isChildOf == true)
                {
                    continue;
                }
                else
                {
                    switch (i)
                    {
                        case 0:
                            {
                                vesselsUC[i].Margin = new Thickness(70.495, 20, 0, 0);
                                if (!mainGrid.Children.Contains(vesselsUC[i]))
                                {
                                    mainGrid.Children.Add(vesselsUC[i]);
                                }
                                break;
                            }
                        case 1:
                            {
                                vesselsUC[i].Margin = spot1.Margin;
                                if (!mainGrid.Children.Contains(vesselsUC[i]))
                                {
                                    mainGrid.Children.Add(vesselsUC[i]);
                                }
                                break;
                            }
                        case 2:
                            {
                                vesselsUC[i].Margin = spot2.Margin;
                                if (!mainGrid.Children.Contains(vesselsUC[i]))
                                {
                                    mainGrid.Children.Add(vesselsUC[i]);
                                }
                                break;
                            }
                        case 3:
                            {
                                vesselsUC[i].Margin = spot3.Margin;
                                if (!mainGrid.Children.Contains(vesselsUC[i]))
                                {
                                    mainGrid.Children.Add(vesselsUC[i]);
                                }
                                break;
                            }
                        case 4:
                            {
                                vesselsUC[i].Margin = spot4.Margin;
                                if (!mainGrid.Children.Contains(vesselsUC[i]))
                                {
                                    mainGrid.Children.Add(vesselsUC[i]);
                                }
                                break;
                            }
                        case 5:
                            {
                                vesselsUC[i].Margin = spot5.Margin;
                                if (!mainGrid.Children.Contains(vesselsUC[i]))
                                {
                                    mainGrid.Children.Add(vesselsUC[i]);
                                }
                                break;
                            }
                        case 6:
                            {
                                vesselsUC[i].Margin = spot6.Margin;
                                if (!mainGrid.Children.Contains(vesselsUC[i]))
                                {
                                    mainGrid.Children.Add(vesselsUC[i]);
                                }
                                break;
                            }
                    }
                }
            }
        }

        public void markSelection(bool selected)
        {
            if (selected)
            {
                l1.Stroke = Brushes.Red;
                l2.Stroke = Brushes.Red;
                ellipseDockingSpot.Stroke = Brushes.Red;
            }
            else
            {
                l1.Stroke = Brushes.Black;
                l2.Stroke = Brushes.Black;
                ellipseDockingSpot.Stroke = Brushes.LightGray;
            }
        }
    }
}
