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

namespace MULTIFORM_PCS.ControlModules.CameraModule.Visualization
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class CameraControlWindow : UserControl
    {
        private System.Windows.Threading.DispatcherTimer infoBoxUpdater;
        private List<int> countdown;

        private Algorithm.PatternDetectionAlgorithm detectionAlgorithm;
        public ImageReader.LiveImageReader liveReader;
        public Algorithm.SimpleDewarping dewarping;
       

        public CameraControlWindow()
        {
            detectionAlgorithm = new Algorithm.PatternDetectionAlgorithm();

            InitializeComponent();

            detectionAlgorithm.setDefaultParameterValues();

            countdown = new List<int>();
            infoBoxUpdater = new System.Windows.Threading.DispatcherTimer();
            infoBoxUpdater.Tick += new EventHandler(updateInfoBox);
            infoBoxUpdater.Interval = TimeSpan.FromSeconds(1.0d);
            infoBoxUpdater.Start();

            loadCurrentValuesToUI();

            try
            {
                bool res800x600 = false;
                if (res800x600)
                {
                    //this.Title = "Tracking Algorithm (800x600)";
                }
                else
                {
                    //this.Title = "Tracking Algorithm (1600x1200)";
                }
                liveReader = new ImageReader.LiveImageReader(detectionAlgorithm, res800x600);
                liveReader.startCam();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                //detectionAlgorithm.useLiveImages = false;
            }

            //detectionAlgorithm.startTracking(this);
        }

        private void loadCurrentValuesToUI()
        {
            sliderBlue.Value = detectionAlgorithm.redChanMin;
            sliderGreen.Value = detectionAlgorithm.moreThanGreen;
            sliderRed.Value = detectionAlgorithm.moreThanBlue;

            sliderMaxClusterDistance.Value = detectionAlgorithm.maxClusterDistanceForMerging;
            sliderMinParticleCount.Value = detectionAlgorithm.minClusterParticleCount;
            sliderParticleDistance.Value = detectionAlgorithm.maxParticleDistanceForCluster;

            sliderMaxXOutside.Value = detectionAlgorithm.regionOutsideMaxX;
            sliderMinXOutside.Value = detectionAlgorithm.regionOutsideMinX;
            sliderMaxYOutside.Value = detectionAlgorithm.regionOutsideMaxY;
            sliderMinYOutside.Value = detectionAlgorithm.regionOutsideMinY;
            sliderMinXCenter.Value = detectionAlgorithm.regionCenterMinX;
            sliderMaxXCenter.Value = detectionAlgorithm.regionCenterMaxX;
            sliderMinYCenter.Value = detectionAlgorithm.regionCenterMinY;
            sliderMaxYCenter.Value = detectionAlgorithm.regionCenterMaxY;

            sliderAPEXBASEMin_Center.Value = detectionAlgorithm.distanceAPEXtoBASEMin_CENTER;
            sliderAPEXBASEMax_Center.Value = detectionAlgorithm.distanceAPEXtoBASEMax_CENTER;
            sliderBASEBASE_Center.Value = detectionAlgorithm.distanceBASEMax_CENTER;
            sliderAPEXBASETol_Center.Value = detectionAlgorithm.APEXtoBASEtolerance_CENTER;
            sliderAPEXBASEMin_Middle.Value = detectionAlgorithm.distanceAPEXtoBASEMin_MIDDLE;
            sliderAPEXBASEMax_Middle.Value = detectionAlgorithm.distanceAPEXtoBASEMax_MIDDLE;
            sliderBASEBASE_Middle.Value = detectionAlgorithm.distanceBASEMax_MIDDLE;
            sliderAPEXBASETol_Middle.Value = detectionAlgorithm.APEXtoBASEtolerance_MIDDLE;
            sliderAPEXBASEMin_Outside.Value = detectionAlgorithm.distanceAPEXtoBASEMin_OUTSIDE;
            sliderAPEXBASEMax_Outside.Value = detectionAlgorithm.distanceAPEXtoBASEMax_OUTSIDE;
            sliderBASEBASE_Outside.Value = detectionAlgorithm.distanceBASEMax_OUTSIDE;
            sliderAPEXBASETol_Outside.Value = detectionAlgorithm.APEXtoBASEtolerance_OUTSIDE;
        }

        private void updateInfoBox(object sender, EventArgs e)
        {
            for (int i = countdown.Count - 1; i >= 0; i--)
            {
                countdown[i]--;
                if (countdown[i] <= 0 && listBoxInfo.Items.Count > i)
                {
                    listBoxInfo.Items.RemoveAt(i);
                }
            }
            int index = 0;
            while (index < countdown.Count)
            {
                if (countdown[index] <= 0)
                {
                    countdown.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }
        }

        public void postMessage(string message)
        {
            listBoxInfo.Items.Add(message);
            countdown.Add(5);
        }

        private void sliderBlue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.redChanMin = (int)sliderBlue.Value;
        }

        private void buttonLoadDefaultValues_Click(object sender, RoutedEventArgs e)
        {
            detectionAlgorithm.setDefaultParameterValues();
            loadCurrentValuesToUI();
        }

        private void sliderRed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.moreThanBlue= (int)sliderRed.Value;
        }

        private void sliderGreen_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.moreThanGreen = (int)sliderGreen.Value;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (detectionAlgorithm.imagesStopped)
                {
                    detectionAlgorithm.imagesStopped = false;
                }
                else
                {
                    detectionAlgorithm.imagesStopped = true;
                }
            }
        }

        private void sliderParticleDistance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.maxParticleDistanceForCluster = (int)sliderParticleDistance.Value;
        }

        private void sliderMinParticleCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.minClusterParticleCount = (int)sliderMinParticleCount.Value;
        }

        private void sliderMaxClusterDistance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.maxClusterDistanceForMerging = (int)sliderMaxClusterDistance.Value;
        }

        private void sliderMaxXOutside_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.regionOutsideMaxX = (int)sliderMaxXOutside.Value;
        }

        private void sliderMinXOutside_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.regionOutsideMinX = (int)sliderMinXOutside.Value;
        }

        private void sliderMaxYOutside_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.regionOutsideMaxY = (int)sliderMaxYOutside.Value;
        }

        private void sliderMinYOutside_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.regionOutsideMinY = (int)sliderMinYOutside.Value;
        }

        private void sliderMinXCenter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.regionCenterMinX = (int)sliderMinXCenter.Value;
        }

        private void sliderMaxXCenter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.regionCenterMaxX = (int)sliderMaxXCenter.Value;
        }

        private void sliderMinYCenter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.regionCenterMinY = (int)sliderMinYCenter.Value;
        }

        private void sliderMaxYCenter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.regionCenterMaxY = (int)sliderMaxYCenter.Value;
        }

        private void sliderAPEXBASEMin_Center_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.distanceAPEXtoBASEMin_CENTER = (int)sliderAPEXBASEMin_Center.Value;
        }

        private void sliderAPEXBASEMax_Center_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.distanceAPEXtoBASEMax_CENTER = (int)sliderAPEXBASEMax_Center.Value;
        }

        private void sliderBASEBASE_Center_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.distanceBASEMax_CENTER = (int)sliderBASEBASE_Center.Value;
        }

        private void sliderAPEXBASETol_Center_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.APEXtoBASEtolerance_CENTER = (int)sliderAPEXBASETol_Center.Value;
        }

        private void sliderAPEXBASEMin_Middle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.distanceAPEXtoBASEMin_MIDDLE = (int)sliderAPEXBASEMin_Middle.Value;
        }

        private void sliderAPEXBASEMax_Middle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.distanceAPEXtoBASEMax_MIDDLE = (int)sliderAPEXBASEMax_Middle.Value;
        }

        private void sliderBASEBASE_Middle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.distanceBASEMax_MIDDLE = (int)sliderBASEBASE_Middle.Value;
        }

        private void sliderAPEXBASETol_Middle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.APEXtoBASEtolerance_MIDDLE = (int)sliderAPEXBASETol_Middle.Value;
        }

        private void sliderAPEXBASEMin_Outside_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.distanceAPEXtoBASEMin_OUTSIDE= (int)sliderAPEXBASEMin_Outside.Value;
        }

        private void sliderAPEXBASEMax_Outside_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.distanceAPEXtoBASEMax_OUTSIDE = (int)sliderAPEXBASEMax_Outside.Value;
        }

        private void sliderBASEBASE_Outside_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.distanceBASEMax_OUTSIDE = (int)sliderBASEBASE_Outside.Value;
        }

        private void sliderAPEXBASETol_Outside_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            detectionAlgorithm.APEXtoBASEtolerance_OUTSIDE = (int)sliderAPEXBASETol_Outside.Value;
        }

        private void buttonSaveCurrentValues_Click(object sender, RoutedEventArgs e)
        {
            StreamWriter configValues = new StreamWriter("config.sav", false);
            configValues.WriteLine("#PARAMETER SEGMENTATION");
            configValues.WriteLine(detectionAlgorithm.redChanMin);
            configValues.WriteLine(detectionAlgorithm.moreThanGreen);
            configValues.WriteLine(detectionAlgorithm.moreThanBlue);

            configValues.WriteLine("#PARAMETER CLUSTERING");
            configValues.WriteLine(detectionAlgorithm.maxParticleDistanceForCluster);
            configValues.WriteLine(detectionAlgorithm.minClusterParticleCount);
            configValues.WriteLine(detectionAlgorithm.maxClusterDistanceForMerging);

            configValues.WriteLine("#PARAMETER DETECTION (REGIONS: Outside, Middle, Center)");
            configValues.WriteLine(detectionAlgorithm.regionOutsideMaxX);
            configValues.WriteLine(detectionAlgorithm.regionOutsideMinX);
            configValues.WriteLine(detectionAlgorithm.regionOutsideMaxY);
            configValues.WriteLine(detectionAlgorithm.regionOutsideMinY);
            configValues.WriteLine(detectionAlgorithm.regionCenterMinX);
            configValues.WriteLine(detectionAlgorithm.regionCenterMaxX);
            configValues.WriteLine(detectionAlgorithm.regionCenterMinY);
            configValues.WriteLine(detectionAlgorithm.regionCenterMaxY);

            configValues.WriteLine("#PARAMETER DETECTION PER REGION");
            configValues.WriteLine(detectionAlgorithm.distanceAPEXtoBASEMin_CENTER);
            configValues.WriteLine(detectionAlgorithm.distanceAPEXtoBASEMax_CENTER);
            configValues.WriteLine(detectionAlgorithm.distanceBASEMax_CENTER);
            configValues.WriteLine(detectionAlgorithm.APEXtoBASEtolerance_CENTER);

            configValues.WriteLine(detectionAlgorithm.distanceAPEXtoBASEMin_MIDDLE);
            configValues.WriteLine(detectionAlgorithm.distanceAPEXtoBASEMax_MIDDLE);
            configValues.WriteLine(detectionAlgorithm.distanceBASEMax_MIDDLE);
            configValues.WriteLine(detectionAlgorithm.APEXtoBASEtolerance_MIDDLE);

            configValues.WriteLine(detectionAlgorithm.distanceAPEXtoBASEMin_OUTSIDE);
            configValues.WriteLine(detectionAlgorithm.distanceAPEXtoBASEMax_OUTSIDE);
            configValues.WriteLine(detectionAlgorithm.distanceBASEMax_OUTSIDE);
            configValues.WriteLine(detectionAlgorithm.APEXtoBASEtolerance_OUTSIDE);

            configValues.Flush();
            configValues.Close();

            Console.WriteLine("Config saved.");
        }

        private void buttonLoadSavedValues_Click(object sender, RoutedEventArgs e)
        {
            bool found = false;
            DirectoryInfo dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            foreach (FileInfo f in dir.EnumerateFiles())
            {
                if (f.Name == "config.sav")
                {
                    found = true;
                    break;
                }
            }
            if (found)
            {
                StreamReader configReader = new StreamReader("config.sav");
                configReader.ReadLine();
                detectionAlgorithm.redChanMin = int.Parse(configReader.ReadLine());
                detectionAlgorithm.moreThanGreen = int.Parse(configReader.ReadLine());
                detectionAlgorithm.moreThanBlue = int.Parse(configReader.ReadLine());

                configReader.ReadLine();
                detectionAlgorithm.maxParticleDistanceForCluster = int.Parse(configReader.ReadLine());
                detectionAlgorithm.minClusterParticleCount = int.Parse(configReader.ReadLine());
                detectionAlgorithm.maxClusterDistanceForMerging = int.Parse(configReader.ReadLine());

                configReader.ReadLine();
                detectionAlgorithm.regionOutsideMaxX = int.Parse(configReader.ReadLine());
                detectionAlgorithm.regionOutsideMinX = int.Parse(configReader.ReadLine());
                detectionAlgorithm.regionOutsideMaxY = int.Parse(configReader.ReadLine());
                detectionAlgorithm.regionOutsideMinY = int.Parse(configReader.ReadLine());
                detectionAlgorithm.regionCenterMinX = int.Parse(configReader.ReadLine());
                detectionAlgorithm.regionCenterMaxX = int.Parse(configReader.ReadLine());
                detectionAlgorithm.regionCenterMinY = int.Parse(configReader.ReadLine());
                detectionAlgorithm.regionCenterMaxY = int.Parse(configReader.ReadLine());

                configReader.ReadLine();
                detectionAlgorithm.distanceAPEXtoBASEMin_CENTER = int.Parse(configReader.ReadLine());
                detectionAlgorithm.distanceAPEXtoBASEMax_CENTER = int.Parse(configReader.ReadLine());
                detectionAlgorithm.distanceBASEMax_CENTER = int.Parse(configReader.ReadLine());
                detectionAlgorithm.APEXtoBASEtolerance_CENTER = int.Parse(configReader.ReadLine());

                detectionAlgorithm.distanceAPEXtoBASEMin_MIDDLE = int.Parse(configReader.ReadLine());
                detectionAlgorithm.distanceAPEXtoBASEMax_MIDDLE = int.Parse(configReader.ReadLine());
                detectionAlgorithm.distanceBASEMax_MIDDLE = int.Parse(configReader.ReadLine());
                detectionAlgorithm.APEXtoBASEtolerance_MIDDLE = int.Parse(configReader.ReadLine());

                detectionAlgorithm.distanceAPEXtoBASEMin_OUTSIDE = int.Parse(configReader.ReadLine());
                detectionAlgorithm.distanceAPEXtoBASEMax_OUTSIDE = int.Parse(configReader.ReadLine());
                detectionAlgorithm.distanceBASEMax_OUTSIDE = int.Parse(configReader.ReadLine());
                detectionAlgorithm.APEXtoBASEtolerance_OUTSIDE = int.Parse(configReader.ReadLine());

                configReader.Close();

                Console.WriteLine("Config loaded.");

                loadCurrentValuesToUI();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (detectionAlgorithm.useLiveImages)
            {
                liveReader.stopCam();
            }
        }
    }
}
