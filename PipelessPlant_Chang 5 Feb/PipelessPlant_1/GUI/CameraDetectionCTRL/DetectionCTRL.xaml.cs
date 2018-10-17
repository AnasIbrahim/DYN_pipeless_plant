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
    /// Interaktionslogik für DetectionCTRL.xaml
    /// </summary>
    public partial class DetectionCTRL : UserControl
    {
        public DetectionCTRL()
        {
            InitializeComponent();
        }

        private void sliderMaxXOutside_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.regionOutsideMaxX = (int)sliderMaxXOutside.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderMinXOutside_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.regionOutsideMinX = (int)sliderMinXOutside.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderMaxYOutside_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.regionOutsideMaxY = (int)sliderMaxYOutside.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderMinYOutside_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.regionOutsideMinY = (int)sliderMinYOutside.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderMinXCenter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.regionCenterMinX = (int)sliderMinXCenter.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderMaxXCenter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.regionCenterMaxX = (int)sliderMaxXCenter.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderMinYCenter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.regionCenterMinY = (int)sliderMinYCenter.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderMaxYCenter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.regionCenterMaxY = (int)sliderMaxYCenter.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderAPEXBASEMin_Center_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.distanceAPEXtoBASEMin_CENTER = (int)sliderAPEXBASEMin_Center.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderAPEXBASEMax_Center_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.distanceAPEXtoBASEMax_CENTER = (int)sliderAPEXBASEMax_Center.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderBASEBASE_Center_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.distanceBASEMax_CENTER = (int)sliderBASEBASE_Center.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderAPEXBASETol_Center_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.APEXtoBASEtolerance_CENTER = (int)sliderAPEXBASETol_Center.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        //private void sliderAPEXBASEMin_Middle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    if (ControlModules.CameraModule.CameraControl.getInstance().Running)
        //    {
        //        ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.distanceAPEXtoBASEMin_MIDDLE = (int)sliderAPEXBASEMin_Middle.Value;
        //    }
        //    else
        //    {
        //        e.Handled = true;
        //    }
        //}

        //private void sliderAPEXBASEMax_Middle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    if (ControlModules.CameraModule.CameraControl.getInstance().Running)
        //    {
        //        ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.distanceAPEXtoBASEMax_MIDDLE = (int)sliderAPEXBASEMax_Middle.Value;
        //    }
        //    else
        //    {
        //        e.Handled = true;
        //    }
        //}

        //private void sliderBASEBASE_Middle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    if (ControlModules.CameraModule.CameraControl.getInstance().Running)
        //    {
        //        ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.distanceBASEMax_MIDDLE = (int)sliderBASEBASE_Middle.Value;
        //    }
        //    else
        //    {
        //        e.Handled = true;
        //    }
        //}

        //private void sliderAPEXBASETol_Middle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    if (ControlModules.CameraModule.CameraControl.getInstance().Running)
        //    {
        //        ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.APEXtoBASEtolerance_MIDDLE = (int)sliderAPEXBASETol_Middle.Value;
        //    }
        //    else
        //    {
        //        e.Handled = true;
        //    }
        //}

        private void sliderMaxDistanceMarker_Center_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
          if (ControlModules.CameraModule.CameraControl.getInstance().Running)
          {
            ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.maxDistanceMarker_CENTER = (int)sliderMaxDistanceMarker_Center.Value;
          }
          else
          {
            e.Handled = true;
          }
        }

        private void sliderAPEXBASEMin_Outside_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.distanceAPEXtoBASEMin_OUTSIDE = (int)sliderAPEXBASEMin_Outside.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderAPEXBASEMax_Outside_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.distanceAPEXtoBASEMax_OUTSIDE = (int)sliderAPEXBASEMax_Outside.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderBASEBASE_Outside_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.distanceBASEMax_OUTSIDE = (int)sliderBASEBASE_Outside.Value;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderAPEXBASETol_Outside_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.APEXtoBASEtolerance_OUTSIDE = (int)sliderAPEXBASETol_Outside.Value;
            }
            else
            {
                e.Handled = true;
            }
        }
    }
}
