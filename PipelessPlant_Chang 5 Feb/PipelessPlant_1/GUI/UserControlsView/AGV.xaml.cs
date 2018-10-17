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
    /// Interaktionslogik für AGV.xaml
    /// </summary>
    public partial class AGV : UserControl
    {
        public int agvID;
        private bool selected;

        public AGV(Datastructure.Model.AGV.AGV a, UserControlsView.Vessel vUC)
        {
            selected = false;
            this.agvID = a.Id;

            InitializeComponent();

            agvControl.Height = a.Diameter;
            agvControl.Width = a.Diameter;

            this.RenderTransform = new RotateTransform(-a.theRotation + 90, 33 / 2, 33 / 2);
        }

        public void markDocking(bool docked)
        {
            if (docked)
            {
                ellipseAGV.Stroke = Brushes.Green;
            }
            else
            {
                if (selected)
                {
                    ellipseAGV.Stroke = Brushes.Red;
                }
                else
                {
                    ellipseAGV.Stroke = Brushes.Black;
                }
            }
        }

        public void markSelection(bool selected)
        {
            if (selected)
            {
                ellipseAGV.Stroke = Brushes.Red;
                this.selected = true;
            }
            else
            {
                ellipseAGV.Stroke = Brushes.Black;
                this.selected = false;
            }
        }
        public void removeAllVessels(List<VesselUserControls> vessels)
        {
            for (int i = 0; i < vessels.Count; i++)
            {
                if (mainGrid.Children.Contains(vessels[i].vesView))
                {
                    mainGrid.Children.Remove(vessels[i].vesView);
                    vessels[i].vesView.isChildOf = false;
                }
            }
        }
    }
}
