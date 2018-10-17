using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.IO;

using MULTIFORM_PCS.ControlModules.CameraModule.Algorithm;
using MULTIFORM_PCS.ControlModules.CameraModule.ImageReader;

namespace MULTIFORM_PCS.ControlModules.CameraModule
{
    class CameraControl
    {
        private bool block;
        private bool res800x600;
        public bool Res800x600
        {
            get { return res800x600; }
        }
        private bool running;
        public bool Running
        {
            get { return running; }
        }
        private bool segmentation;
        public bool Segmentation
        {
            get { return segmentation; }
            set { segmentation = value; }
        }
        private bool patternDetection;
        public bool PatternDetection
        {
            get { return patternDetection; }
            set { patternDetection = value; }
        }
        private bool drawCluster;
        public bool DrawCluster
        {
          get { return drawCluster; }
          set { drawCluster = value; }
        }

        public PatternDetectionAlgorithm detectionAlgorithm;
        public LiveImageReader liveReader;
        public SimpleDewarping dewarping;

        #region singleton_pattern;
        private static CameraControl cam;
        public static CameraControl getInstance()
        {
            if (cam == null)
            {
                cam = new CameraControl();
            }
            return cam;
        }
        private CameraControl()
        {
            res800x600 = true;
            running = false;
            segmentation = false;
            patternDetection = false;
            drawCluster = false;
            block = false;
        }
        #endregion;

        public void startCamera(Image drawCam, Canvas red, Image pattern)
        {
            if (!running)
            {
              if (!block)
              {
                block = true;
                detectionAlgorithm = Algorithm.PatternDetectionAlgorithm.getInstance();

                loadDefaultSettings();

                try
                {
                  liveReader = new ImageReader.LiveImageReader(detectionAlgorithm, res800x600);
                  liveReader.startCam();
                  liveReader.setGamma(detectionAlgorithm.gamma);
                  liveReader.setContrast(detectionAlgorithm.contrast);
                  liveReader.setBrightness(detectionAlgorithm.brightness);
                  running = true;
                  block = false;
                }
                catch (Exception e)
                {
                  Console.WriteLine(e.StackTrace);
                  GUI.PCSMainWindow.getInstance().postStatusMessage("Cannot connect to Camera! uEye-API.dll (or uEye-API_64.dll) not found or camera not connected to the PCS.");
                  running = false;
                  block = false;
                  return;
                }

                detectionAlgorithm.startTracking(drawCam, red, pattern);
              }
              else
              {
                //SKIP!
              }
            }
            else
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("Camera already running.");
            }
        }
        public void changeResolutionAndRestartCamera(Image drawCam, Canvas red, Image pattern)
        {
            if (running)
            {
                stopCamera();
            }
            if (res800x600)
            {
                res800x600 = false;
            }
            else
            {
                res800x600 = true;
            }
            startCamera(drawCam, red, pattern);
        }
        public void stopCamera()
        {
            if (running)
            {
                running = false;
                detectionAlgorithm.stopTracking();
                liveReader.stopCam();
            }
            else
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("No camera running, cannot stop camera.");
            }
        }
        public void pauseCamera()
        {
            if (running)
            {
                detectionAlgorithm.imagesStopped = !detectionAlgorithm.imagesStopped;
            }
            else
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("No camera running, cannot pause the camera image.");
            }
        }

        public void loadDefaultSettings()
        {
            detectionAlgorithm.setDefaultParameterValues();

            loadCurrentValuesToUI();
        }
        public void loadSettingsFromFile(string filename)
        {
          try
          {
            StreamReader configReader = new StreamReader(filename);
            configReader.ReadLine();
            detectionAlgorithm.gamma = int.Parse(configReader.ReadLine());
            detectionAlgorithm.contrast = int.Parse(configReader.ReadLine());
            detectionAlgorithm.brightness = int.Parse(configReader.ReadLine());

            configReader.ReadLine();
            detectionAlgorithm.redChanMin = int.Parse(configReader.ReadLine());
            detectionAlgorithm.redChanMax = int.Parse(configReader.ReadLine());
            detectionAlgorithm.greenChanMin = int.Parse(configReader.ReadLine());
            detectionAlgorithm.greenChanMax = int.Parse(configReader.ReadLine());
            detectionAlgorithm.blueChanMin = int.Parse(configReader.ReadLine());
            detectionAlgorithm.blueChanMax = int.Parse(configReader.ReadLine());

            configReader.ReadLine();
            detectionAlgorithm.skipMinX = int.Parse(configReader.ReadLine());
            detectionAlgorithm.skipMaxX = int.Parse(configReader.ReadLine());
            detectionAlgorithm.skipMinY = int.Parse(configReader.ReadLine());
            detectionAlgorithm.skipMaxY = int.Parse(configReader.ReadLine());

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

            //detectionAlgorithm.distanceAPEXtoBASEMin_MIDDLE = int.Parse(configReader.ReadLine());
            //detectionAlgorithm.distanceAPEXtoBASEMax_MIDDLE = int.Parse(configReader.ReadLine());
            //detectionAlgorithm.distanceBASEMax_MIDDLE = int.Parse(configReader.ReadLine());
            //detectionAlgorithm.APEXtoBASEtolerance_MIDDLE = int.Parse(configReader.ReadLine());

            detectionAlgorithm.maxDistanceMarker_CENTER = int.Parse(configReader.ReadLine());

            detectionAlgorithm.distanceAPEXtoBASEMin_OUTSIDE = int.Parse(configReader.ReadLine());
            detectionAlgorithm.distanceAPEXtoBASEMax_OUTSIDE = int.Parse(configReader.ReadLine());
            detectionAlgorithm.distanceBASEMax_OUTSIDE = int.Parse(configReader.ReadLine());
            detectionAlgorithm.APEXtoBASEtolerance_OUTSIDE = int.Parse(configReader.ReadLine());

            configReader.Close();

            GUI.PCSMainWindow.getInstance().postStatusMessage("Config loaded from file \"" + filename + "\".");

            loadCurrentValuesToUI();
          }
          catch (Exception)
          {
            GUI.PCSMainWindow.getInstance().postStatusMessage("Wrong config format. Please use the new one.");
          }
        }
        public void saveSettingsToFile(FileInfo saveFile)
        {
            StreamWriter configValues = new StreamWriter(saveFile.FullName, false);
            configValues.WriteLine("#PARAMETER CAMERA");
            configValues.WriteLine(detectionAlgorithm.gamma);
            configValues.WriteLine(detectionAlgorithm.contrast);
            configValues.WriteLine(detectionAlgorithm.brightness);

            configValues.WriteLine("#PARAMETER SEGMENTATION");
            configValues.WriteLine(detectionAlgorithm.redChanMin);
            configValues.WriteLine(detectionAlgorithm.redChanMax);
            configValues.WriteLine(detectionAlgorithm.greenChanMin);
            configValues.WriteLine(detectionAlgorithm.greenChanMax);
            configValues.WriteLine(detectionAlgorithm.blueChanMin);
            configValues.WriteLine(detectionAlgorithm.blueChanMax);

            configValues.WriteLine("#PARAMETER ROI");
            configValues.WriteLine(detectionAlgorithm.skipMinX);
            configValues.WriteLine(detectionAlgorithm.skipMaxX);
            configValues.WriteLine(detectionAlgorithm.skipMinY);
            configValues.WriteLine(detectionAlgorithm.skipMaxY);

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

            //configValues.WriteLine(detectionAlgorithm.distanceAPEXtoBASEMin_MIDDLE);
            //configValues.WriteLine(detectionAlgorithm.distanceAPEXtoBASEMax_MIDDLE);
            //configValues.WriteLine(detectionAlgorithm.distanceBASEMax_MIDDLE);
            //configValues.WriteLine(detectionAlgorithm.APEXtoBASEtolerance_MIDDLE);

            configValues.WriteLine(detectionAlgorithm.maxDistanceMarker_CENTER);

            configValues.WriteLine(detectionAlgorithm.distanceAPEXtoBASEMin_OUTSIDE);
            configValues.WriteLine(detectionAlgorithm.distanceAPEXtoBASEMax_OUTSIDE);
            configValues.WriteLine(detectionAlgorithm.distanceBASEMax_OUTSIDE);
            configValues.WriteLine(detectionAlgorithm.APEXtoBASEtolerance_OUTSIDE);

            configValues.Flush();
            configValues.Close();

            GUI.PCSMainWindow.getInstance().postStatusMessage("Config saved to file \""+saveFile.Name+"\".");
        }
        private void loadCurrentValuesToUI()
        {
            GUI.PCSMainWindow.getInstance().cameraCTRL.sliderGamma.Value = detectionAlgorithm.gamma;
            GUI.PCSMainWindow.getInstance().cameraCTRL.sliderContrast.Value = detectionAlgorithm.contrast;
            GUI.PCSMainWindow.getInstance().cameraCTRL.sliderBrigthness.Value = detectionAlgorithm.brightness;

            GUI.PCSMainWindow.getInstance().segmentationCTRL.sliderRedMin.Value = detectionAlgorithm.redChanMin;
            GUI.PCSMainWindow.getInstance().segmentationCTRL.sliderRedMax.Value = detectionAlgorithm.redChanMax;
            GUI.PCSMainWindow.getInstance().segmentationCTRL.sliderGreenMin.Value = detectionAlgorithm.greenChanMin;
            GUI.PCSMainWindow.getInstance().segmentationCTRL.sliderGreenMax.Value = detectionAlgorithm.greenChanMax;
            GUI.PCSMainWindow.getInstance().segmentationCTRL.sliderBlueMin.Value = detectionAlgorithm.blueChanMin;
            GUI.PCSMainWindow.getInstance().segmentationCTRL.sliderBlueMax.Value = detectionAlgorithm.blueChanMax;

            GUI.PCSMainWindow.getInstance().segmentationCTRL.sliderMaxClusterDistance.Value = detectionAlgorithm.maxClusterDistanceForMerging;
            GUI.PCSMainWindow.getInstance().segmentationCTRL.sliderMinParticleCount.Value = detectionAlgorithm.minClusterParticleCount;
            GUI.PCSMainWindow.getInstance().segmentationCTRL.sliderParticleDistance.Value = detectionAlgorithm.maxParticleDistanceForCluster;

            GUI.PCSMainWindow.getInstance().cameraCTRL.sliderSkipMinX.Value = detectionAlgorithm.skipMinX;
            GUI.PCSMainWindow.getInstance().cameraCTRL.sliderSkipMaxX.Value = detectionAlgorithm.skipMaxX;
            GUI.PCSMainWindow.getInstance().cameraCTRL.sliderSkipMinY.Value = detectionAlgorithm.skipMinY;
            GUI.PCSMainWindow.getInstance().cameraCTRL.sliderSkipMaxY.Value = detectionAlgorithm.skipMaxY;

            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderMaxXOutside.Value = detectionAlgorithm.regionOutsideMaxX;
            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderMinXOutside.Value = detectionAlgorithm.regionOutsideMinX;
            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderMaxYOutside.Value = detectionAlgorithm.regionOutsideMaxY;
            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderMinYOutside.Value = detectionAlgorithm.regionOutsideMinY;
            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderMinXCenter.Value = detectionAlgorithm.regionCenterMinX;
            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderMaxXCenter.Value = detectionAlgorithm.regionCenterMaxX;
            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderMinYCenter.Value = detectionAlgorithm.regionCenterMinY;
            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderMaxYCenter.Value = detectionAlgorithm.regionCenterMaxY;

            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderAPEXBASEMin_Center.Value = detectionAlgorithm.distanceAPEXtoBASEMin_CENTER;
            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderAPEXBASEMax_Center.Value = detectionAlgorithm.distanceAPEXtoBASEMax_CENTER;
            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderBASEBASE_Center.Value = detectionAlgorithm.distanceBASEMax_CENTER;
            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderAPEXBASETol_Center.Value = detectionAlgorithm.APEXtoBASEtolerance_CENTER;
            //GUI.PCSMainWindow.getInstance().detectionCTRL.sliderAPEXBASEMin_Middle.Value = detectionAlgorithm.distanceAPEXtoBASEMin_MIDDLE;
            //GUI.PCSMainWindow.getInstance().detectionCTRL.sliderAPEXBASEMax_Middle.Value = detectionAlgorithm.distanceAPEXtoBASEMax_MIDDLE;
            //GUI.PCSMainWindow.getInstance().detectionCTRL.sliderBASEBASE_Middle.Value = detectionAlgorithm.distanceBASEMax_MIDDLE;
            //GUI.PCSMainWindow.getInstance().detectionCTRL.sliderAPEXBASETol_Middle.Value = detectionAlgorithm.APEXtoBASEtolerance_MIDDLE;
            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderMaxDistanceMarker_Center.Value = detectionAlgorithm.maxDistanceMarker_CENTER;
            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderAPEXBASEMin_Outside.Value = detectionAlgorithm.distanceAPEXtoBASEMin_OUTSIDE;
            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderAPEXBASEMax_Outside.Value = detectionAlgorithm.distanceAPEXtoBASEMax_OUTSIDE;
            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderBASEBASE_Outside.Value = detectionAlgorithm.distanceBASEMax_OUTSIDE;
            GUI.PCSMainWindow.getInstance().detectionCTRL.sliderAPEXBASETol_Outside.Value = detectionAlgorithm.APEXtoBASEtolerance_OUTSIDE;
        }
    }
}
