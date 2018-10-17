


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.IO;
using System.Threading;

namespace MULTIFORM_PCS.ControlModules.CameraModule.Algorithm
{
    public class PatternDetectionAlgorithm
    {

        public StreamWriter log = new StreamWriter("C:\\Users\\Pipelessplant\\Desktop\\log.txt");        
        private System.Windows.Threading.DispatcherTimer imageAquire;
        private BitmapSource[] imageSources;
        private int cur_img_idx = 0;

        private BufferFilter[] datastructure;

        public List<Robot> robots = new List<Robot>(); //*******************

        private Stopwatch stopWatch;

        private int imageWidth = 1600;
        private int imageHeight = 1200;

        private double epsBaseAngle = 30; //Tolerance (epsilon) +/- 90 degree between base line and apex line

        private bool skipFrame = false;
        private bool firstImage = true;
        public bool imagesStopped;

        private Image imageDrawCamera;
        private Canvas imageRed;
        private Image imagePattern;

        //PARAMETER CAMERA
        public int gamma;
        public int contrast;
        public int brightness;

        //PARAMETER SEGMENTATION
        public int redChanMin;
        public int redChanMax;
        public int greenChanMin;
        public int greenChanMax;
        public int blueChanMin;
        public int blueChanMax;

        //PARAMETER ROI
        public int skipMinX;
        public int skipMaxX;
        public int skipMinY;
        public int skipMaxY;

        //PARAMETER CLUSTERING
        public int maxParticleDistanceForCluster;
        public int minClusterParticleCount;
        public int maxClusterDistanceForMerging;

        //PARAMETER DETECTION (REGIONS: Outside, Center)
        public int regionOutsideMaxX;
        public int regionOutsideMinX;
        public int regionOutsideMaxY;
        public int regionOutsideMinY;
        public int regionCenterMinX;
        public int regionCenterMaxX;
        public int regionCenterMinY;
        public int regionCenterMaxY;

        //PARAMETER DETECTION PER REGION
        public int distanceAPEXtoBASEMin_CENTER;
        public int distanceAPEXtoBASEMax_CENTER;
        public int distanceBASEMax_CENTER;
        public int APEXtoBASEtolerance_CENTER;

        public int maxDistanceMarker_CENTER;

        //public int distanceAPEXtoBASEMin_MIDDLE;
        //public int distanceAPEXtoBASEMax_MIDDLE;
        //public int distanceBASEMax_MIDDLE;
        //public int APEXtoBASEtolerance_MIDDLE;

        public int distanceAPEXtoBASEMin_OUTSIDE;
        public int distanceAPEXtoBASEMax_OUTSIDE;
        public int distanceBASEMax_OUTSIDE;
        public int APEXtoBASEtolerance_OUTSIDE;

        public static PatternDetectionAlgorithm detectionAlgorithm;
        public static PatternDetectionAlgorithm getInstance()
        {
          if (detectionAlgorithm == null)
          {
            detectionAlgorithm = new PatternDetectionAlgorithm();
          } return detectionAlgorithm;
        }
        private PatternDetectionAlgorithm()
        {
            setDefaultParameterValues();
            datastructure = new BufferFilter[Gateway.ObserverModule.getInstance().getCurrentPlant().AllAGVs.Count];
            for (int i = 0; i < Gateway.ObserverModule.getInstance().getCurrentPlant().AllAGVs.Count; i++)
            {
                datastructure[i] = new BufferFilter(Gateway.ObserverModule.getInstance().getCurrentPlant().AllAGVs[i].Id, 5);
            }
        }

        public void setDefaultParameterValues()
        {
            gamma = 73;  //default was 120
            contrast = 250;//default 250
            brightness = 6000; //2916; //default was 2000

            redChanMin = 22; //
            redChanMax =188;//
            blueChanMin = 42;// 42
            blueChanMax = 158;// 158
            greenChanMin = 22;//
            greenChanMax = 188;//
            skipMinX = 50;
            skipMaxX = 750;
            skipMinY = 25;
            skipMaxY = 575;

            maxParticleDistanceForCluster = 3;
            minClusterParticleCount = 1;
            maxClusterDistanceForMerging = 5;

            regionOutsideMaxX = 0;
            regionOutsideMinX = 1600;
            regionOutsideMaxY = 0;
            regionOutsideMinY = 1200;
            regionCenterMinX = 150;
            regionCenterMaxX = 650;
            regionCenterMinY = 100;
            regionCenterMaxY = 500;

            distanceAPEXtoBASEMin_CENTER = 50;
            distanceAPEXtoBASEMax_CENTER = 60;
            distanceBASEMax_CENTER = 20;
            APEXtoBASEtolerance_CENTER = 5;

            maxDistanceMarker_CENTER = 20;

            //distanceAPEXtoBASEMin_MIDDLE = 40;
            //distanceAPEXtoBASEMax_MIDDLE = 50;
            //distanceBASEMax_MIDDLE = 15;
            //APEXtoBASEtolerance_MIDDLE = 5;

            distanceAPEXtoBASEMin_OUTSIDE = 40;
            distanceAPEXtoBASEMax_OUTSIDE = 60;
            distanceBASEMax_OUTSIDE = 15;
            APEXtoBASEtolerance_OUTSIDE = 5;
        }

        public void startTracking(Image imageDrawCamera, Canvas imageRed, Image imagePattern)
        {
            this.imageDrawCamera = imageDrawCamera;
            this.imageRed = imageRed;
            this.imagePattern = imagePattern;

            imagesStopped = false;

            imageAquire = new System.Windows.Threading.DispatcherTimer();
            imageAquire.Tick += new EventHandler(setNewImage);
            imageAquire.Interval = TimeSpan.FromSeconds(0.01d);

            imageSources = new BitmapSource[1];

            stopWatch = new Stopwatch();

            imageAquire.Start();
        }

        public void stopTracking()
        {
          imageAquire.Stop();
        }

        private void setNewImage(object sender, EventArgs e)
        {
            if (!skipFrame)
            {
                skipFrame = true;
                if (!imagesStopped)
                {
                    stopWatch.Start();
                    cur_img_idx = 0;

                    int[] pixelDatanewImage = CameraControl.getInstance().liveReader.getCurrentImage();
                    imageHeight = CameraControl.getInstance().liveReader.getHeight();
                    imageWidth = CameraControl.getInstance().liveReader.getWidth();
                    int h = imageHeight;
                    int w = imageWidth;
                    int widthInByte = 4 * w;

                        WriteableBitmap modifiedImage = new WriteableBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Bgra32, null);

                  if (Algorithm.PatternDetectionAlgorithm.getInstance().skipMinX >= imageWidth -1)
                  {
                    Algorithm.PatternDetectionAlgorithm.getInstance().skipMinX = imageWidth-2;
                  }
                  if (Algorithm.PatternDetectionAlgorithm.getInstance().skipMaxX >= imageWidth - 1)
                  {
                    Algorithm.PatternDetectionAlgorithm.getInstance().skipMaxX = imageWidth-2;
                  }
                  if (Algorithm.PatternDetectionAlgorithm.getInstance().skipMinY >= imageHeight - 1)
                  {
                    Algorithm.PatternDetectionAlgorithm.getInstance().skipMinY = imageHeight-2;
                  }
                  if (Algorithm.PatternDetectionAlgorithm.getInstance().skipMaxY >= imageHeight- 1)
                  {
                    Algorithm.PatternDetectionAlgorithm.getInstance().skipMaxY = imageHeight-2;
                  }

                  for (int i = Algorithm.PatternDetectionAlgorithm.getInstance().skipMinX; i < Algorithm.PatternDetectionAlgorithm.getInstance().skipMaxX; i++)
                  {
                      pixelDatanewImage[Algorithm.PatternDetectionAlgorithm.getInstance().skipMinY * imageWidth + i] = System.Drawing.Color.Yellow.ToArgb();
                      pixelDatanewImage[(Algorithm.PatternDetectionAlgorithm.getInstance().skipMinY + 1) * imageWidth + i] = System.Drawing.Color.Yellow.ToArgb();
                      pixelDatanewImage[Algorithm.PatternDetectionAlgorithm.getInstance().skipMaxY * imageWidth + i] = System.Drawing.Color.Yellow.ToArgb();
                      pixelDatanewImage[(Algorithm.PatternDetectionAlgorithm.getInstance().skipMaxY + 1) * imageWidth + i] = System.Drawing.Color.Yellow.ToArgb();
                  }
                  for (int i = Algorithm.PatternDetectionAlgorithm.getInstance().skipMinY; i < Algorithm.PatternDetectionAlgorithm.getInstance().skipMaxY; i++)
                  {
                      pixelDatanewImage[i * imageWidth + Algorithm.PatternDetectionAlgorithm.getInstance().skipMinX] = System.Drawing.Color.Yellow.ToArgb();
                      pixelDatanewImage[i * imageWidth + Algorithm.PatternDetectionAlgorithm.getInstance().skipMinX + 1] = System.Drawing.Color.Yellow.ToArgb();
                      pixelDatanewImage[i * imageWidth + Algorithm.PatternDetectionAlgorithm.getInstance().skipMaxX] = System.Drawing.Color.Yellow.ToArgb();
                      pixelDatanewImage[i * imageWidth + Algorithm.PatternDetectionAlgorithm.getInstance().skipMaxX + 1] = System.Drawing.Color.Yellow.ToArgb();
                  }

                    modifiedImage.WritePixels(new Int32Rect(0, 0, w, h), pixelDatanewImage, widthInByte, 0);

                    imageSources[cur_img_idx] = modifiedImage;
                }
                imageDrawCamera.Source = imageSources[cur_img_idx];

                if (firstImage)
                {
                    CameraControl.getInstance().dewarping = new SimpleDewarping(imageWidth, imageHeight);
                    // todo: frede debug code
                    {
                        imageHeight = CameraControl.getInstance().liveReader.getHeight();
                        imageWidth = CameraControl.getInstance().liveReader.getWidth();
                        //int h = imageHeight;
                        //int w = imageWidth;
                        //WriteableBitmap wb = new WriteableBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Bgra32, null);
                        //var pixelDatanewImage = new int[w * h];

                        //var di = CameraControl.getInstance().dewarping.simpleDefishImage(CameraControl.getInstance().liveReader.getCurrentImage(), imageWidth, imageHeight);

                        int x1 = 55;
                        int x2 = 65;
                        int y1 = 35;
                        int y2 = 55;
                        int new_width = imageWidth - (x1 + x2);
                        int new_height = imageHeight - (y1 + y2);
                        int widthInByte = 4 * new_width;
                        int[] imageCut = new int[new_width * new_height];
                        for (int i = x1; i < (imageWidth - x2); i++)
                        {
                            for (int j = y1; j < (imageHeight - y2); j++)
                            {
                               // imageCut[(j - y1) * new_width + (i - x1)] = di[j * imageWidth + i];
                            }
                        }

                        WriteableBitmap wb = new WriteableBitmap(new_width, new_height, 96, 96, PixelFormats.Bgra32, null);
                        wb.WritePixels(new Int32Rect(0, 0, new_width, new_height), imageCut, widthInByte, 0);
                        //GUI.PCSMainWindow.getInstance().plantImage.Source = wb;

                        var middle = CameraControl.getInstance().dewarping.simpleDefishPoint(imageWidth / 2, imageHeight / 2);
                        Gateway.ObserverModule.getInstance().getCurrentPlant().AllAGVs[0].ShadowX = middle.X;
                        Gateway.ObserverModule.getInstance().getCurrentPlant().AllAGVs[0].ShadowY = middle.Y;
                        Gateway.ObserverModule.getInstance().modelChanged();
                    }
                    firstImage = false;
                }

                List<Point> perzeptrons;
                perzeptrons = CameraControl.getInstance().liveReader.getPerceptrons();
                //bool showSegmentation = GUI.PCSMainWindow.getInstance().TypeShown == 2;
                if (GUI.PCSMainWindow.getInstance().TypeShown == 2 && !CameraControl.getInstance().DrawCluster)
                {
                    int h = imageHeight;
                    int w = imageWidth;
                    imageRed.Children.Clear();
                    imageRed.Width = w;
                    imageRed.Height = h;
                    imageRed.Background = Brushes.Black;
                    /**int[] segmentedImagePixels = new int[h * w];
                    Parallel.For(0, h * w, delegate(int i)
                    {
                        segmentedImagePixels[i] = System.Drawing.Color.Black.ToArgb();
                    });*/
                    //Parallel.For(0, perzeptrons.Count, delegate(int i)
                    //{
                    for (int i = 0; i < perzeptrons.Count; i++)
                    {
                      Rectangle r = new Rectangle();
                      r.Fill = Brushes.Red;
                      r.Height = 1;
                      r.Width = 1;
                      imageRed.Children.Add(r);
                      Canvas.SetTop(r, (int)perzeptrons[i].X);
                      Canvas.SetLeft(r, (int)perzeptrons[i].Y);
                      //int index = (int)perzeptrons[i].X * imageWidth + (int)perzeptrons[i].Y;
                      //segmentedImagePixels[index] = System.Drawing.Color.Red.ToArgb();
                    }//);
                    /*int widthInByte = 4 * w;
                    WriteableBitmap segmentedImage = new WriteableBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Bgra32, null);
                    segmentedImage.WritePixels(new Int32Rect(0, 0, w, h), segmentedImagePixels, widthInByte, 0);
                    imageRed.Source = segmentedImage;*/
                }

                //stopWatch.Stop();
                //GUI.PCSMainWindow.getInstance().postStatusMessage("Image aquired and segmented in " + stopWatch.ElapsedMilliseconds + "ms");
                //stopWatch.Reset();

                if (perzeptrons.Count > 10000) // 5000
                {
                    GUI.PCSMainWindow.getInstance().postStatusMessage("Frame skipped due to bad segmentation.");
                    brightness = brightness - 500;
                    GUI.PCSMainWindow.getInstance().cameraCTRL.sliderBrigthness.Value = detectionAlgorithm.brightness;
                    GUI.PCSMainWindow.getInstance().postStatusMessage("brightness" + brightness);
                    skipFrame = false;
                }
                else
                {
                    detectPattern(imageSources[cur_img_idx], perzeptrons);
                    stopWatch.Stop();
                    GUI.PCSMainWindow.getInstance().postStatusMessage("Image aquired and segmented in " + stopWatch.ElapsedMilliseconds + "ms");
                    stopWatch.Reset();
                }
            }
            else
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("Frame skipped");
            }
        }

        //private List<Point> createRedImage(BitmapSource curImg)
        //{

        //    List<Point> perzeptrons = new List<Point>();

        //    stopWatch.Start();
        //    //BitmapSource bitmapSource = new FormatConvertedBitmap(curImg, PixelFormats.Bgra32, null, 0);
        //    WriteableBitmap modifiedImage = new WriteableBitmap(curImg);

        //    int h = imageHeight;
        //    int w = imageWidth;
        //    int[] pixelData = new int[w * h];
        //    int widthInByte = 4 * w;

        //    modifiedImage.CopyPixels(pixelData, widthInByte, 0);

        //    Parallel.For(0, pixelData.Length, delegate(int i)
        //    {
        //        System.Drawing.Color pixelColor = System.Drawing.Color.FromArgb(pixelData[i]);
        //        if (pixelColor.R > redChanMin && pixelColor.R > pixelColor.B + moreThanBlue && pixelColor.R > pixelColor.G + moreThanGreen)
        //        {
        //            pixelData[i] = System.Drawing.Color.Red.ToArgb();
        //            lock (perzeptrons)
        //            {
        //                perzeptrons.Add(new Point(i / imageWidth, i % imageWidth));
        //            }
        //        }
        //        else
        //        {
        //            pixelData[i] = System.Drawing.Color.Black.ToArgb();
        //        }
        //    });
        //    stopWatch.Stop();

        //    modifiedImage.WritePixels(new Int32Rect(0, 0, w, h), pixelData, widthInByte, 0);

        //    imageRed.Source = modifiedImage;
        //    //mainPointer.postMessage("Image segmented in " + stopWatch.ElapsedMilliseconds + "ms");
        //    stopWatch.Reset();

        //    return perzeptrons;
        //}

        /**public readonly double offsetX = 100.0d;//focus on the needed area of the dewarped image 1600x1200
        public readonly double offsetY = 50.0d;//focus on the needed area of the dewarped image 1600x1200
        public readonly double scale = 0.85d;//focus on the needed area of the dewarped image 1600x1200*/

        public readonly double offsetX = 130.0d;//focus on the needed area of the dewarped image 800x600
        public readonly double offsetY = 120.0d;//focus on the needed area of the dewarped image 800x600
        public readonly double scale = 0.85d;//focus on the needed area of the dewarped image 800x600


        


        public void detectPattern(BitmapSource curImg, List<Point> perzeptrons)
        {
            /*Stopwatch detectRobotsWatch = new Stopwatch();
            detectRobotsWatch.Stop();
            
            detectRobotsWatch.Start();

            
            //stopWatch.Start();*/
            //CLUSTERING!
            List<List<Point>> clouds = new List<List<Point>>();
            for (int i = 0; i < perzeptrons.Count; i++)
            {
                if (i == 0)
                {
                    clouds.Add(new List<Point>());
                    clouds[clouds.Count - 1].Add(perzeptrons[i]);
                }
                else
                {
                    bool insertToCluster = false;
                    for (int j = 0; j < clouds.Count; j++)
                    {
                        //if (euclidianDistance(coulds[j][0], perzeptrons[i]) <= maxParticleDistanceForCluster)
                        if (euclidianDistance(calculateCentroid(clouds[j]), perzeptrons[i]) <= maxParticleDistanceForCluster)
                        {
                            clouds[j].Add(perzeptrons[i]);
                            insertToCluster = true;
                            break;
                        }
                    }
                    if (!insertToCluster)
                    {
                        clouds.Add(new List<Point>());
                        clouds[clouds.Count - 1].Add(perzeptrons[i]);
                    }
                }
            }

            clouds.RemoveAll(c => c.Count < minClusterParticleCount);
            List<Point> cluster = new List<Point>();
            for (int i = 0; i < clouds.Count; i++)
            {
                Point centerOfCloud = calculateCentroid(clouds[i]);
                if (centerOfCloud.X != -1337 && centerOfCloud.Y != -1337)
                {
                    cluster.Add(calculateCentroid(clouds[i]));
                }
            }
            int index = 0;
            while (index < cluster.Count)
            {
                bool merged = false;
                for (int j = index + 1; j < cluster.Count; j++)
                {
                    if (euclidianDistance(cluster[index], cluster[j]) <= maxClusterDistanceForMerging)
                    {
                        merged = true;
                        cluster[index] = calculateCenter(cluster[index], cluster[j]);
                        cluster.RemoveAt(j);
                        break;
                    }
                }
                if (!merged)
                {
                    index++;
                }
            }

            if (CameraControl.getInstance().DrawCluster && (GUI.PCSMainWindow.getInstance().TypeShown == 2))
            {
                imageRed.Children.Clear();
                imageRed.Width = imageWidth;
                imageRed.Height = imageHeight;
                imageRed.Background = Brushes.Black;
                for (int i = 0; i < cluster.Count; i++)
                {
                    Ellipse r = new Ellipse();
                    r.Fill = Brushes.LightSteelBlue;
                    r.Height = 5.0d;
                    r.Width = 5.0d;
                    imageRed.Children.Add(r);
                    Canvas.SetTop(r, (int)cluster[i].X - 2.5d);
                    Canvas.SetLeft(r, (int)cluster[i].Y - 2.5d);
                    // to give out position of found cluster:
                    GUI.PCSMainWindow.getInstance().postStatusMessage("Cluster center at (Image)" + (int)cluster[i].X + ", " + (int)cluster[i].Y);
                    //log.WriteLine("Cluster center at (Image)" + (int)cluster[i].X + ", " + (int)cluster[i].Y);

                    //  Point robotPositionDewarpedImageCoords = CameraControl.getInstance().dewarping.simpleDefishPoint((int)cluster[i].X, (int)cluster[i].Y);

                    // GUI.PCSMainWindow.getInstance().postStatusMessage("Cluster center at (Real World)" + (int)robotPositionDewarpedImageCoords.X + ", " + (int)robotPositionDewarpedImageCoords.Y);
                }
                /** WriteableBitmap drawClusterImage = new WriteableBitmap((BitmapSource)imageRed.Source);
                 int[] clusterPixelData = new int[imageHeight * imageWidth];
                 int widthInByteCluster = 4 * imageWidth;

                 drawClusterImage.CopyPixels(clusterPixelData, widthInByteCluster, 0);

                 Parallel.For(0, cluster.Count, delegate(int i)
                 {
                     int indexCluster = (int)cluster[i].X * 1600 + (int)cluster[i].Y;
                     clusterPixelData[index] = System.Drawing.Color.YellowGreen.ToArgb();
                 });

                 drawClusterImage.WritePixels(new Int32Rect(0, 0, imageWidth, imageHeight), clusterPixelData, widthInByteCluster, 0);
                 imageRed.Source = drawClusterImage;*/
            }
            else
            {
                for (int i = 0; i < cluster.Count; i++)
                {
                    Console.WriteLine("Point: (" + cluster[i].X + "/" + cluster[i].Y + ")");
                }
            }


            List<Robot> robots = detectRobots(cluster);  //**********************************

            //stopWatch.Start();

            //WriteableBitmap modifiedImage = new WriteableBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Bgra32, null);
            //BitmapSource bitmapSource = new FormatConvertedBitmap(curImg, PixelFormats.Bgra32, null, 0);
            WriteableBitmap modifiedImage = new WriteableBitmap(curImg);

            int h = imageHeight;
            int w = imageWidth;
            int[] pixelData = new int[w * h];
            int widthInByte = 4 * w;

            modifiedImage.CopyPixels(pixelData, widthInByte, 0);

            for (int i = 0; i < robots.Count; i++)
            {
                //COORDIANTENUMRECHNUNG VON BILD WARPED ZU WORLD DEWARPED!
                Point robotPositionImageCoords = new Point(robots[i].getRobotPosition().Y, robots[i].getRobotPosition().X);
                //NEXT: ROBOT WARPED zu ROBOT DEWARPED (pixel coords)
                Point robotPositionDewarpedImageCoords = CameraControl.getInstance().dewarping.simpleDefishPoint((int)robotPositionImageCoords.X, (int)robotPositionImageCoords.Y);

                if (robotPositionDewarpedImageCoords.X != -666 && robotPositionDewarpedImageCoords.Y != -666)//else robot out of image, or no world coordinate stored
                {
                    double x1 = 55.0d;
                    double x2 = 65.0d;
                    double y1 = 35.0d;
                    double y2 = 55.0d;
                    //NEXT: ROBOT DEWARPED (pixel coords) to ROBOT DEWARPED (world coords)
                    if (robotPositionDewarpedImageCoords.X < x1 || robotPositionDewarpedImageCoords.X > (imageWidth - x2) //focus on the needed area of the camera image
                    || robotPositionDewarpedImageCoords.Y < y1 || robotPositionDewarpedImageCoords.Y > (imageHeight - y2))
                    {
                        //   GUI.PCSMainWindow.getInstance().postStatusMessage("ROBOT #" + robots[i].id + " out of image!");//Robot can be seen in the warped image, but out of the real plant area, especially out of the white boards
                    }
                    else
                    {
                        double realX = 400 / (imageWidth - (x1 + x2)) * (robotPositionDewarpedImageCoords.X - x1);
                        double realY = 300 / (imageHeight - (y1 + y2)) * (robotPositionDewarpedImageCoords.Y - y1);


                        //double realX = robotPositionDewarpedImageCoords.X - (offsetX * scale);
                        //double realY = robotPositionDewarpedImageCoords.Y - (offsetY * scale);
                        //realX = realX / (scale * imageWidth) * 400.0d;
                        //realY = realY / (scale * imageHeight) * 300.0d;


                        Datastructure.Model.Plant p = Gateway.ObserverModule.getInstance().getCurrentPlant();
                        for (int j = 0; j < p.AllAGVs.Count; j++)
                        {
                            if (p.AllAGVs[j].Id == robots[i].id-1)
                            {
                                datastructure[j].insert((float)(p.theSize.Width - realX), (float)(/**p.theSize.Height -*/ realY), robots[i].getRobotAngle(), DateTime.Now);//insert new robot position into buffer filter (jumps of more than 100 pixel are permitted)

                                //p.AllAGVs[j].theRotation = Math.Round(datastructure[j].getFilteredRotation() + 180, 2);//coordinaten umdrehen
                                //p.AllAGVs[j].theCurPosition.X = Math.Round(/**400 -*/ datastructure[j].getFilteredPosition().X, 2); //coordinaten umdrehen
                                //p.AllAGVs[j].theCurPosition.Y = Math.Round(/**300 - */datastructure[j].getFilteredPosition().Y, 2); //coordinaten umdrehen
                                //    //MAGED
                                    p.AllAGVs[j].theCurPosition.X = Math.Round(robots[i].getRealCenter().X, 2);//in cm
                                    p.AllAGVs[j].theCurPosition.Y = Math.Round(robots[i].getRealCenter().Y, 2); //in cm
                                    p.AllAGVs[j].theRotation = Math.Round(robots[i].angle, 2); //angle is in degrees
                                p.AllAGVs[j].LastUpdateCam = datastructure[i].getFilteredTimeStamp();

                                if (NetworkFeeder.singleton != null)
                                    NetworkFeeder.singleton.sendUpdatePos(new NetworkFeeder.RobotPos[] { new NetworkFeeder.RobotPos(p.AllAGVs[j].Id, p.AllAGVs[j].theCurPosition.X, p.AllAGVs[j].theCurPosition.Y, p.AllAGVs[j].theRotation) } );

                                //  GUI.PCSMainWindow.getInstance().postStatusMessage("Cluster center at (Real World)" + (int)p.AllAGVs[j].theCurPosition.X + ", " + (int)p.AllAGVs[j].theCurPosition.Y);

                                if (!p.AllAGVs[j].Seen)
                                {
                                    if (p.AllAGVs[j].firstSeen != null)
                                        p.AllAGVs[j].firstSeen();
                                    p.AllAGVs[j].Seen = true;
                                }
                                break;
                            }
                        }
                    }
                }

                Gateway.ObserverModule.getInstance().modelChanged();
                //ENDE UMRECHNUNG

                for (int j = (int)robots[i].apex.X - 3; j < (int)robots[i].apex.X + 3; j++)
                {
                    for (int l = (int)robots[i].apex.Y - 3; l < (int)robots[i].apex.Y + 3; l++)
                    {
                        if (j < 0 || l < 0 || l >= imageWidth || j >= imageHeight)
                        {
                            continue;
                        }
                        else
                        {
                            pixelData[j * imageWidth + l] = System.Drawing.Color.LimeGreen.ToArgb();
                        }
                    }
                }
                for (int j = (int)robots[i].base1.X - 3; j < (int)robots[i].base1.X + 3; j++)
                {
                    for (int l = (int)robots[i].base1.Y - 3; l < (int)robots[i].base1.Y + 3; l++)
                    {
                        if (j < 0 || l < 0 || l >= imageWidth || j >= imageHeight)
                        {
                            continue;
                        }
                        else
                        {
                            pixelData[j * imageWidth + l] = System.Drawing.Color.LimeGreen.ToArgb();
                        }
                    }
                }
                for (int j = (int)robots[i].base2.X - 3; j < (int)robots[i].base2.X + 3; j++)
                {
                    for (int l = (int)robots[i].base2.Y - 3; l < (int)robots[i].base2.Y + 3; l++)
                    {
                        if (j < 0 || l < 0 || l >= imageWidth || j >= imageHeight)
                        {
                            continue;
                        }
                        else
                        {
                            pixelData[j * imageWidth + l] = System.Drawing.Color.LimeGreen.ToArgb();
                        }
                    }
                }

                int posx = (int)robots[i].position.X;
                int posy = (int)robots[i].position.Y;
                int id = robots[i].id;
                for (int j = posx - 3; j < posx + 3; j++)
                {
                    for (int l = posy - 3; l < posy + 3; l++)
                    {
                        if (j < 0 || l < 0 || l >= imageWidth || j >= imageHeight || (j - posx) * (j - posx) + (l - posy) * (l - posy) > 4)
                        {
                            continue;
                        }
                        else
                        {
                            if (id == 1)
                                pixelData[j * imageWidth + l] = System.Drawing.Color.Red.ToArgb();
                            else if (id == 2)
                                pixelData[j * imageWidth + l] = System.Drawing.Color.White.ToArgb();
                            else if (id ==3)
                                pixelData[j * imageWidth + l] = System.Drawing.Color.Yellow.ToArgb();
                            else if (id == 4)
                                pixelData[j * imageWidth + l] = System.Drawing.Color.Brown.ToArgb();
                            else if (id == 5)
                                pixelData[j * imageWidth + l] = System.Drawing.Color.Fuchsia.ToArgb();

                        }
                    }
                }

                /*for (int j = 0; j < robots[i].idMarkerLeft.Count; j++)
                {
                    for (int k = (int)robots[i].idMarkerLeft[j].X - 5; k < (int)robots[i].idMarkerLeft[j].X + 5; k++)
                    {
                        for (int l = (int)robots[i].idMarkerLeft[j].Y - 5; l < (int)robots[i].idMarkerLeft[j].Y + 5; l++)
                        {
                            if (k < 0 || l < 0 || l >= imageWidth || k >= imageHeight)
                            {
                                continue;
                            }
                            else
                            {
                                pixelData[k * imageWidth + l] = System.Drawing.Color.OrangeRed.ToArgb();
                            }
                        }
                    }
                }
                for (int j = 0; j < robots[i].idMarkerRight.Count; j++)
                {
                    for (int k = (int)robots[i].idMarkerRight[j].X - 5; k < (int)robots[i].idMarkerRight[j].X + 5; k++)
                    {
                        for (int l = (int)robots[i].idMarkerRight[j].Y - 5; l < (int)robots[i].idMarkerRight[j].Y + 5; l++)
                        {
                            if (k < 0 || l < 0 || l >= imageWidth || k >= imageHeight)
                            {
                                continue;
                            }
                            else
                            {
                                pixelData[k * imageWidth + l] = System.Drawing.Color.OrangeRed.ToArgb();
                            }
                        }
                    }
                }
                for (int k = (int)robots[i].getRobotPosition().X - 5; k < (int)robots[i].getRobotPosition().X + 5; k++) {
                  for (int l = (int)robots[i].getRobotPosition().Y - 5; l < (int)robots[i].getRobotPosition().Y + 5; l++) {
                    //pixelData[k * imageWidth + l] = System.Drawing.Color.Magenta.ToArgb();
                  }
                }
               */

            }
            //stopWatch.Stop();

            modifiedImage.WritePixels(new Int32Rect(0, 0, w, h), pixelData, widthInByte, 0);
            imagePattern.Source = modifiedImage;

            /*GUI.PCSMainWindow.getInstance().postStatusMessage("Perzeptrons analyzed in " + stopWatch.ElapsedMilliseconds + "ms");
            stopWatch.Reset();
            GUI.PCSMainWindow.getInstance().postStatusMessage(robots.Count + " Pattern recognized in " + detectRobotsWatch.ElapsedMilliseconds + "ms");
            log.WriteLine(robots.Count + " Pattern recognized in " + detectRobotsWatch.ElapsedMilliseconds + "ms");
            detectRobotsWatch.Reset();*/

            skipFrame = false;
        }

        public class BasicPoints
        {
            public Point apex;// = new Point(-888, -888);
            public Point base1;// = new Point(-888, -888);
            public Point base2;//= new Point(-888, -888);

            public BasicPoints(Point a, Point b, Point c)
            {
                apex = a;
                base1 = b;
                base2 = c;
            }
            public BasicPoints()
            {
                apex = new Point(-888, -888);
                base1 = apex;
                base2 = apex;
            }
            public string bpMesg()
            {
                string bp = " Apex: (" + apex.X + ", " + apex.Y + ") and Bases: (" + base1.X + ", " + base1.Y + ") , (" + base2.X + ", " + base2.Y + ")";
                return bp;
            }
        }  //what I require




        private BasicPoints getBasic(List<Point> clusters)
        {
            if (clusters.Count() == 3)
            {
                List<double> dis = new List<double> { 0, 0, 0 };
                for (int i = 0; i < clusters.Count(); i++)
                {
                    for (int j = 0; j < clusters.Count(); j++)
                    {
                        double distance = Math.Sqrt(Math.Pow((clusters[j].X - clusters[i].X), 2) + Math.Pow((clusters[j].Y - clusters[i].Y), 2));
                        dis[i] = dis[i] + distance;
                    }
                }
                int to = dis.FindIndex(p => p == dis.Max());
                Point apex = clusters[to];
                clusters.RemoveAt(to);
                BasicPoints ro1 = new BasicPoints(apex, clusters[0], clusters[1]);
                return ro1;
            }
            else if (clusters.Count() > 3)
            {
                Point apex = notInLine(clusters);
                clusters.Remove(apex);
                List<double> dis = new List<double> { };
                for (int i = 0; i < clusters.Count(); i++)
                {
                    dis.Add(Math.Sqrt(Math.Pow((apex.X - clusters[i].X), 2) + Math.Pow((apex.Y - clusters[i].Y), 2)));
                }
                int bas = dis.FindIndex(p => p == dis.Min());
                Point b1 = clusters[bas];
                clusters.RemoveAt(bas);
                dis.RemoveAt(bas);
                bas = dis.FindIndex(p => p == dis.Min());
                Point b2 = clusters[bas];
                BasicPoints ro2 = new BasicPoints(apex, b1, b2);
                return ro2;//, 4);
            }
            else
            {
                Point a = new Point(-888, -888);
                return new BasicPoints(a, a, a);
            }
        }

        private List<Robot> detectRobots(List<Point> clusters)
        {
           
            double sizeOfRobot = 60;
            BasicPoints def = new BasicPoints();
            List<Robot> robots = new List<Robot>();
            for (int i = 1; i <= 5; i++)  //set default position, as not found!
            {
                Robot ro = new Robot(def, i);
                robots.Add(ro);
            }

            while (clusters.Count() > 2)
            {
                List<Point> temp = new List<Point>();
                temp.Add(clusters[0]);
                log.WriteLine("Clusters at:\r\n   (" + (int)clusters[0].X + " , " + (int)clusters[0].Y + ");");
                GUI.PCSMainWindow.getInstance().postStatusMessage("Clusters at:\r\n   (" + (int)clusters[0].X + " , " + (int)clusters[0].Y + ");");
                clusters.RemoveAt(0);
                int count = clusters.Count();
                int i = 0;
                while (count > 0)
                {
                    double dis = Math.Sqrt(Math.Pow((temp[0].X - clusters[i].X), 2) + Math.Pow((temp[0].Y - clusters[i].Y), 2));
                    if (dis < sizeOfRobot)
                    {
                        temp.Add(clusters[i]);
                        log.WriteLine("   (" + (int)clusters[i].X + " , " + (int)clusters[i].Y + ");");
                        GUI.PCSMainWindow.getInstance().postStatusMessage("   (" + (int)clusters[i].X + " , " + (int)clusters[i].Y + ");");
                        clusters.RemoveAt(i);
                        count--;
                    }
                    else
                    {
                        i++;
                        count--;
                    }
                }

                Robot temprobot = new Robot(new BasicPoints(), 1);
                switch (temp.Count())
                {
                    case 3:
                        {
                            temprobot= new Robot(getBasic(temp), 1);
                            break;
                        }
                    case 5:
                        {
                            
                            temprobot = new Robot(getBasic(temp), 5);
                            
                            break;
                        }
                    case 4:
                        {
                            
                            
                            bool flag;
                            BasicPoints baro;
                            testRobot2(temp, out flag, out baro);
                            if (flag)
                            {
                                temprobot = new Robot(baro, 2);
                                break;
                            }
                            else
                            {
                                temprobot = new Robot(baro, 3);
                                break;
                            }
                        }
                    default:
                        break;
                }
                int num = robots.FindIndex(p => p.id ==temprobot.id);
                robots.RemoveAt(num);
                robots.Add(temprobot);
                string roms = temprobot.robotMsg();
                GUI.PCSMainWindow.getInstance().postStatusMessage(roms + "\n");
                log.Write(roms + "\r\n\r\n");

            }
            
            DateTime dtt = new DateTime();
            dtt = System.DateTime.Now;
            string ss = dtt.ToString("dd/MM/yyy HH:mm:ss:fff");
            log.WriteLine(ss);
            log.WriteLine("==============================\r\n");
            GUI.PCSMainWindow.getInstance().postStatusMessage(ss);
            GUI.PCSMainWindow.getInstance().postStatusMessage("============================== \n");
            return robots;
        }



        public bool inLine(Point a, Point b, Point c, double error)
        {
            double area = (a.X - c.X) * (b.Y - c.Y) - (b.X - c.X) * (a.Y - c.Y);
            area = Math.Abs(area) / 2;
            //  Console.WriteLine(area);
            if (area < error)
                return true;
            return false;
        }

        private Point notInLine(List<Point> clusters)
        {
            double error = 40;
            for (int i = 0; i < clusters.Count(); i++)
            {
                int flag = 0;
                for (int j = 0; j < clusters.Count(); j++)
                {
                    int flagJ = 0;
                    if (i == j)
                        continue;
                    else
                    {
                        for (int k = 0; k < clusters.Count(); k++)
                        {
                            if (k == j || k == i)
                                continue;
                            else
                            {
                                if (inLine(clusters[i], clusters[j], clusters[k], error))
                                {
                                    flagJ = 1;
                                    flag = 0;
                                    break;
                                }
                                else
                                    flag = 1;
                            }
                        }
                    }
                    if (flagJ == 1)
                        break;
                }
                if (flag == 1)
                    return clusters[i];
            }
            return new Point(-888, -888);
        }

        private void testRobot2(List<Point> clusters, out bool flag, out BasicPoints robot)
        {
            flag = true;
            //robot = new BasicPoints();

            Point top = notInLine(clusters);
            clusters.Remove(top);
            List<double> dis = new List<double> { };
            for (int i = 0; i < clusters.Count(); i++)
            {
                dis.Add(Math.Sqrt(Math.Pow((top.X - clusters[i].X), 2) + Math.Pow((top.Y - clusters[i].Y), 2)));
            }
            int ex = dis.FindIndex(p => p == dis.Max());
            Point extra = clusters[ex];
            clusters.RemoveAt(ex);
            Point top_extra = new Point((extra.X - top.X), (extra.Y - top.Y));
            Point top_base = new Point((clusters[0].X - top.X), (clusters[0].Y - top.Y));
            double cross = top_extra.X * top_base.Y - top_extra.Y * top_base.X;
            if (cross < 0)  //robot 2. Counterclockwise. 
            {
                flag = true;
                robot = new BasicPoints(top, clusters[0], clusters[1]);//, 2);
            }
            else //For robot it is clockwise, the cross should be greater than 0
            {
                flag = false;
                robot = new BasicPoints(top, clusters[0], clusters[1]);//, 3);
            }
        }

        private double euclidianDistance(Point p1, Point p2)
        {
            double dist = Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            dist = Math.Round(dist, 2);
            return dist;
        }
        private Point calculateCenter(Point p1, Point p2)
        {
            double x = p1.X + (p2.X - p1.X) / 2;
            double y = p1.Y + (p2.Y - p1.Y) / 2;
            return new Point(Math.Round(x, 2), Math.Round(y, 2));
        }

        private Point calculateCentroid(List<Point> cluster)
        {
            if (cluster.Count > 0)
            {
                double minx = cluster[0].X;
                double maxx = cluster[0].X;
                double miny = cluster[0].Y;
                double maxy = cluster[0].Y;
                for (int i = 1; i < cluster.Count; i++)
                {
                    if (cluster[i].X < minx)
                    {
                        minx = cluster[i].X;
                    }
                    if (cluster[i].X > maxx)
                    {
                        maxx = cluster[i].X;
                    }
                    if (cluster[i].Y < miny)
                    {
                        miny = cluster[i].Y;
                    }
                    if (cluster[i].Y > maxy)
                    {
                        maxy = cluster[i].Y;
                    }
                }
                return calculateCenter(new Point(minx, miny), new Point(maxx, maxy));
                //cluster.Clear();
                //cluster.Add(calculateCenter(new Point(minx, miny), new Point(maxx, maxy)));
            }
            GUI.PCSMainWindow.getInstance().postStatusMessage("Cannot calculate centroid");
            return new Point(-1337, -1337); // jaja, Mr. 5ch0ppm3y3r
        }


  
        public class Robot
        {
            public Point apex;
            public Point base1;
            public Point base2;
            public Point mid;
            public Point position;
            public double angle;
            public Point realCenter;
            public int id = -1;
            //public List<Point> idMarkerRight = new List<Point>();
            // public List<Point> idMarkerLeft = new List<Point>();

            public Robot(BasicPoints basic, int i)
            {
                apex = basic.apex;
                base1 = basic.base1;
                base2 = basic.base2;
                mid = calculateCenter(basic.base1, basic.base2);
                id = i;

                SimpleDewarping de = new SimpleDewarping(600, 800);
                double ratio = 1;   //ration=(center of robot to LED bottom middle)/(middle to LED apex)
                double robotX = ratio * (mid.X - apex.X) + mid.X;
                double robotY = ratio * (mid.Y - apex.Y) + mid.Y;
                position = new Point(Math.Round(robotX, 2), Math.Round(robotY, 2));

                angle = Math.Atan((mid.Y - apex.Y) / (mid.X - apex.X)); // 0 ~360°
                angle = Math.Round(angle * 180 / Math.PI, 2);
                if (mid.X > apex.X || mid.X == apex.X) //&& mid.Y > apex.Y) || (mid.Y == apex.Y && mid.X > apex.X))
                    angle = 90 + angle;
                else
                    angle = 270 + angle;
                realCenter = de.simpleDefishPoint(position.X, position.Y);
            }

            public void printToConsole()
            {
                Console.WriteLine("Apex: (" + apex.X + ", " + apex.Y + ")");
                Console.WriteLine("Base1: (" + base1.X + ", " + base1.Y + ")");
                Console.WriteLine("Base2: (" + base2.X + ", " + base2.Y + ")\n");
            }

            public Point getRobotPosition()
            {
                return position;
            }

            public double getRobotAngle() //0 ~360°
            {
                return angle;
            }

            public Point getRealCenter()
            {
                return realCenter;
            }

            public string robotMsg()
            {
                string val;

                //MAGED only added the following line
               // Datastructure.Model.Plant p = Gateway.ObserverModule.getInstance().getCurrentPlant();
                if (apex.X > 0 && apex.X < 600 && apex.Y > 0 && apex.Y < 800 && base1.X > 0 && base1.X < 600 && base1.Y > 0 && base1.Y < 800
                    && base2.X > 0 && base2.X < 600 && base2.Y > 0 && base2.Y < 800)
                {
                    //Point center = getRobotPosition();
                    //double angle = getRobotAngle();

                    val = "ROBOT ( ID-" + id + " )  AT: (" + position.X + ", " + position.Y + ") WITH ANGLE: " + angle + "\r\nApex: (" + apex.X + ", " + apex.Y + ")" + "  Base1: (" + base1.X + ", " + base1.Y + ")" + "  Base2: (" + base2.X + ", " + base2.Y + ")\r\n";

                   
                //    //MAGED
                //    p.AllAGVs[id - 1].theCurPosition.X = Math.Round(realCenter.X, 2);//in cm
                //    p.AllAGVs[id - 1].theCurPosition.Y = Math.Round(realCenter.Y, 2); //in cm
                //    p.AllAGVs[id - 1].theRotation = Math.Round(angle, 2); //angle is in degrees
                    //==================end MAGED
                    val = val + "In the real plant robot is at: (" + (int)realCenter.X + ", " + (int)realCenter.Y + ")";
                }
                else
                    val = "ROBOT ( ID-" + id + " )  Not Found! \n";
               
                return val;
            }

            private Point calculateCenter(Point p1, Point p2)
            {
                double x = (p1.X + p2.X) / 2;
                double y = (p1.Y + p2.Y) / 2;
                return new Point(Math.Round(x, 2), Math.Round(y, 2));
            }
        }
    }
}
