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
    /// Interaction logic for Vessel.xaml
    /// </summary>
    public partial class Vessel : UserControl
    {
        public int vesselID;
        public bool isChildOf;
        private SolidColorBrush[] basicColors = new SolidColorBrush[] { Brushes.Red, Brushes.Yellow, Brushes.Black, Brushes.Blue };

        public Vessel(Datastructure.Model.Vessel.Vessel v)
        {
            this.vesselID = v.theId;

            InitializeComponent();

            isChildOf = false;
        }

        public void markSelection(bool selected)
        {
            if (selected)
            {
                border1.BorderBrush = Brushes.Red;
            }
            else
            {
                border1.BorderBrush = Brushes.Black;
            }
        }
    }
}
