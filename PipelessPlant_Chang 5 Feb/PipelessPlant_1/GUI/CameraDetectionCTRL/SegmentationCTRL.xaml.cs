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

namespace MULTIFORM_PCS.GUI.CameraDetectionCTRL
{
    /// <summary>
    /// Interaktionslogik für SegmentationCTRL.xaml
    /// </summary>
    public partial class SegmentationCTRL : UserControl
    {
        public SegmentationCTRL()
        {
            InitializeComponent();
        }

        private void sliderRedMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.redChanMin = (int)sliderRedMin.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderRedMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.redChanMax = (int)sliderRedMax.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderGreenMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.greenChanMin = (int)sliderGreenMin.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderGreenMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.greenChanMax = (int)sliderGreenMax.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderBlueMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.blueChanMax = (int)sliderBlueMax.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderBlueMin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.blueChanMin = (int)sliderBlueMin.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderParticleDistance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
              lock (ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm)
              {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.maxParticleDistanceForCluster = (int)sliderParticleDistance.Value;
              }
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderMinParticleCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.minClusterParticleCount = (int)sliderMinParticleCount.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderMaxClusterDistance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.maxClusterDistanceForMerging = (int)sliderMaxClusterDistance.Value;
            }
            else
            {
                e.Handled = true;
            }
        }


        private void checkBoxShowClustering_Click(object sender, RoutedEventArgs e)
        {
          ControlModules.CameraModule.CameraControl.getInstance().DrawCluster = (bool)checkBoxShowClustering.IsChecked;
        }

    }
}
