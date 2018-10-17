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
using System.IO;

namespace MULTIFORM_PCS.GUI.CameraDetectionCTRL
{
    /// <summary>
    /// Interaktionslogik für CameraCTRL.xaml
    /// </summary>
    public partial class CameraCTRL : UserControl
    {
        public CameraCTRL()
        {
            InitializeComponent();
        }

        private void sliderGamma_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.gamma = (int)sliderGamma.Value;
                ControlModules.CameraModule.CameraControl.getInstance().liveReader.setGamma(ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.gamma);
            }
            else
            {
                e.Handled = true;
            }
        }

        private void IsMouseDirectlyOverChanged_LoadDefault(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderLoadDefaultConfig.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                borderLoadDefaultConfig.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderLoadDefaultConfig.Background = (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderLoadDefaultConfig.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void IsMouseDirectlyOverChanged_SaveConfig(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderSaveConfig.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                borderSaveConfig.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderSaveConfig.Background = (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderSaveConfig.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void IsMouseDirectlyOverChanged_LoadConfig(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderLoadConfig.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                borderLoadConfig.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderLoadConfig.Background = (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderLoadConfig.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void borderLoadConfig_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog loadCameraSettingsDialog = new Microsoft.Win32.OpenFileDialog();
            loadCameraSettingsDialog.InitialDirectory = Directory.GetCurrentDirectory();
            loadCameraSettingsDialog.Filter = "Camera Detection Settings | *.sav";
            loadCameraSettingsDialog.Title = "Select Camera Detection Settings...";
            loadCameraSettingsDialog.ShowDialog();
            if (loadCameraSettingsDialog.FileName != null && loadCameraSettingsDialog.FileName != "")
            {
                FileInfo f = new FileInfo(loadCameraSettingsDialog.FileName);
                string pathToSaveFile = f.FullName;
                ControlModules.CameraModule.CameraControl.getInstance().loadSettingsFromFile(pathToSaveFile);
            }
        }

        private void borderSaveConfig_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveCameraSettingsDialog = new Microsoft.Win32.SaveFileDialog();
            saveCameraSettingsDialog.InitialDirectory = Directory.GetCurrentDirectory();
            saveCameraSettingsDialog.Filter = "Camera Detection Settings | *.sav";
            saveCameraSettingsDialog.Title = "Select Camera Detection Settings...";
            saveCameraSettingsDialog.ShowDialog();
            if (saveCameraSettingsDialog.FileName != null && saveCameraSettingsDialog.FileName != "")
            {
                FileInfo f = new FileInfo(saveCameraSettingsDialog.FileName);
                ControlModules.CameraModule.CameraControl.getInstance().saveSettingsToFile(f);
            }
        }

        private void borderLoadDefaultConfig_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ControlModules.CameraModule.CameraControl.getInstance().loadDefaultSettings();
        }

        private void sliderContrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.contrast = (int)sliderContrast.Value;
                ControlModules.CameraModule.CameraControl.getInstance().liveReader.setContrast(ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.contrast);
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ControlModules.CameraModule.CameraControl.getInstance().Running)
            {
                ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.brightness = (int)sliderBrigthness.Value;
                ControlModules.CameraModule.CameraControl.getInstance().liveReader.setBrightness(ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.brightness);
            }
            else
            {
                e.Handled = true;
            }
        }

        private void sliderSkipMinX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
          if (ControlModules.CameraModule.CameraControl.getInstance().Running)
          {
            ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.skipMinX = (int)sliderSkipMinX.Value;
          }
          else
          {
            e.Handled = true;
          }
        }

        private void sliderSkipMinY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
          if (ControlModules.CameraModule.CameraControl.getInstance().Running)
          {
            ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.skipMinY = (int)sliderSkipMinY.Value;
          }
          else
          {
            e.Handled = true;
          }
        }

        private void sliderSkipMaxX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
          if (ControlModules.CameraModule.CameraControl.getInstance().Running)
          {
            ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.skipMaxX = (int)sliderSkipMaxX.Value;
          }
          else
          {
            e.Handled = true;
          }
        }

        private void sliderSkipMaxY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
          if (ControlModules.CameraModule.CameraControl.getInstance().Running)
          {
            ControlModules.CameraModule.CameraControl.getInstance().detectionAlgorithm.skipMaxY = (int)sliderSkipMaxY.Value;
          }
          else
          {
            e.Handled = true;
          }
        }
    }
}
