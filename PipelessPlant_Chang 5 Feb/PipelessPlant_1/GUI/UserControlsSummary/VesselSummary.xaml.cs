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
    public partial class VesselSummary : UserControl
    {
        private Datastructure.Model.Vessel.Vessel displayedVessel;

        public VesselSummary(Datastructure.Model.Vessel.Vessel vessel)
        {
            this.displayedVessel = vessel;

            InitializeComponent();

            updateUC();
        }

        public void updateUC()
        {
            if (this.displayedVessel.Finished[0])
            {
                color1.Opacity = 1;
            }
            else
            {
                color1.Opacity = 0.3;
            }
            if (this.displayedVessel.Finished[1])
            {
                color2.Opacity = 1;
            }
            else
            {
                color2.Opacity = 0.3;
            }
            if (this.displayedVessel.Finished[2])
            {
                color3.Opacity = 1;
            }
            else
            {
                color3.Opacity = 0.3;
            }
            labelID.Content = "ID: " + displayedVessel.theId;
            //labelOverallVolume.Content = "Overall Volume: " + displayedVessel.theCurFillWater;            
            //layer1color = displayedVessel.Colors.ElementAt(0);
            //layer2color = displayedVessel.Colors.ElementAt(1);
            //layer3color = displayedVessel.Colors.ElementAt(2);
            switch (displayedVessel.Colors[0])
            {
                case 0:
                    color1.Background = (Brush)this.FindResource("LGBYellow");                      
                    break;
                case 1:
                    color1.Background = (Brush)this.FindResource("LGBBlack"); 
                    break;
                case 2:
                    color1.Background = (Brush)this.FindResource("LGBRed");                    
                    break;
                case 3:
                    color1.Background = (Brush)this.FindResource("LGBBlue"); 
                    break;

                case 4:
                    color1.Background = (Brush)this.FindResource("LGBPurple"); 

                    break;
                case 5:
                    color1.Background = (Brush)this.FindResource("LGBOrange");
                    break;
                case 6:
                    color1.Background = (Brush)this.FindResource("LGBGreen"); 
                    break;
                case -1:
                    color1.Background = (Brush)this.FindResource("LGBLightGray");
                    break;
            }
            switch (displayedVessel.Colors[1])
            {
                case 0:
                    color2.Background = (Brush)this.FindResource("LGBYellow");
                    break;
                case 1:
                    color2.Background = (Brush)this.FindResource("LGBBlack");
                    break;
                case 2:
                    color2.Background = (Brush)this.FindResource("LGBRed");
                    break;
                case 3:
                    color2.Background = (Brush)this.FindResource("LGBBlue");
                    break;

                case 4:
                    color2.Background = (Brush)this.FindResource("LGBPurple");

                    break;
                case 5:
                    color2.Background = (Brush)this.FindResource("LGBOrange");
                    break;
                case 6:
                    color2.Background = (Brush)this.FindResource("LGBGreen");
                    break;
                case -1:
                    color1.Background = (Brush)this.FindResource("LGBLightGray");
                    break;
            }
            switch (displayedVessel.Colors[2])
            {
                case 0:
                    color3.Background = (Brush)this.FindResource("LGBYellow");
                    break;
                case 1:
                    color3.Background = (Brush)this.FindResource("LGBBlack");
                    break;
                case 2:
                    color3.Background = (Brush)this.FindResource("LGBRed");
                    break;
                case 3:
                    color3.Background = (Brush)this.FindResource("LGBBlue");
                    break;

                case 4:
                    color3.Background = (Brush)this.FindResource("LGBPurple");

                    break;
                case 5:
                    color3.Background = (Brush)this.FindResource("LGBOrange");
                    break;
                case 6:
                    color3.Background = (Brush)this.FindResource("LGBGreen");
                    break;
                case -1:
                    color1.Background = (Brush)this.FindResource("LGBLightGray");
                    break;
            }
            color1.UpdateLayout();
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
            Gateway.ObserverModule.getInstance().theVisual.selectVessel(displayedVessel.theId);
        }
    }
}
