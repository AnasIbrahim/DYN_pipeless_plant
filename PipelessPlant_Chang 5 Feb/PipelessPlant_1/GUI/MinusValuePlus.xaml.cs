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
    /// Interaction logic for MinusValuePlus.xaml
    /// </summary>
    public partial class MinusValuePlus : UserControl
    {
        private int value;
        public int Value
        {
            get { return this.value; }
        }
        private int maxValue;

        public MinusValuePlus()
        {
            InitializeComponent();
        }

        private void ButtonPlus_Click(object sender, RoutedEventArgs e)
        {
            if (value < maxValue - 1)
            {
                value++;
                labelValue.Content = "" + value;
            }
        }
        private void ButtonMinus_Click(object sender, RoutedEventArgs e)
        {
            if (value > 0)
            {
                value--;
                labelValue.Content = "" + value;
            }
        }
    }
}
