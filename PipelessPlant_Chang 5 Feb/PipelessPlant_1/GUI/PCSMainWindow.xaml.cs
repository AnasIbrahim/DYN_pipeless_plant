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
using System.Threading;

namespace MULTIFORM_PCS.GUI
{
    /// <summary>
    /// Interaktionslogik für PCSMainWindow.xaml
    /// </summary>
    public partial class PCSMainWindow : Window
    {
        public bool emergencyStop;
        public List<Ellipse> delayLED;
        // console
        public event Action<String[]> consoleCmd = null;
        private class ConsoleString
        {
            private string str;
            public ConsoleString(string str)
            {
                this.str = str;
            }
            public override string ToString()
            {
                return str;
            }
            /*        public override bool Equals(object obj) {
                      return this == obj;
                    }*/
        }
        private void consoleKeyDown(object s, KeyEventArgs a)
        {
            if (a.Key == Key.Return && textBoxConsole.Text != "")
            {
                listBoxConsole.Items.Add(new ConsoleString(textBoxConsole.Text));
                listBoxConsole.ScrollIntoView(listBoxConsole.Items[listBoxConsole.Items.Count - 1]);
                listBoxConsole.SelectedIndex = -1;
                if (consoleCmd != null)
                    consoleCmd(textBoxConsole.Text.Split(' '));
                textBoxConsole.Text = "";
            }
            else if (listBoxConsole.Items.Count > 0)
            {
                if (a.Key == Key.Up)
                {
                    if (listBoxConsole.SelectedIndex == -1 && listBoxConsole.Items.Count > 0)
                    {
                        listBoxConsole.SelectedIndex = listBoxConsole.Items.Count - 1;
                        listBoxConsole.ScrollIntoView(listBoxConsole.Items[listBoxConsole.SelectedIndex]);
                        textBoxConsole.Text = (listBoxConsole.Items[listBoxConsole.SelectedIndex] as ConsoleString).ToString();
                    }
                    else if (listBoxConsole.SelectedIndex > 0)
                    {
                        textBoxConsole.Text = (listBoxConsole.Items[--listBoxConsole.SelectedIndex] as ConsoleString).ToString();
                        listBoxConsole.ScrollIntoView(listBoxConsole.Items[listBoxConsole.SelectedIndex]);
                    }
                }
                else if (a.Key == Key.Down)
                {
                    if (listBoxConsole.SelectedIndex == -1)
                    {
                        listBoxConsole.SelectedIndex = 0;
                        listBoxConsole.ScrollIntoView(listBoxConsole.Items[listBoxConsole.SelectedIndex]);
                        textBoxConsole.Text = (listBoxConsole.Items[listBoxConsole.SelectedIndex] as ConsoleString).ToString();
                    }
                    else if (listBoxConsole.SelectedIndex < (listBoxConsole.Items.Count - 1))
                    {
                        textBoxConsole.Text = (listBoxConsole.Items[++listBoxConsole.SelectedIndex] as ConsoleString).ToString();
                        listBoxConsole.ScrollIntoView(listBoxConsole.Items[listBoxConsole.SelectedIndex]);
                    }
                }
            }
        }


        private RecipeConfiguration recipeConfig;

        private StreamWriter logWriter;
        private System.Windows.Threading.DispatcherTimer statusBoxUpdater;
        private List<int> countdownStatusBox;

        private int typeShown; //0-live, 1-sim, 2-segmentation, 3-detection, 4-scheduling
        public int TypeShown
        {
            get { return typeShown; }
        }
        private Image imageDrawCamera;
        private Canvas imageRed;
        private Image imagePattern;
        public CameraDetectionCTRL.SegmentationCTRL segmentationCTRL;
        public CameraDetectionCTRL.CameraCTRL cameraCTRL;
        public CameraDetectionCTRL.DetectionCTRL detectionCTRL;

        private GanttChart ganttChart;

        public List<Rectangle> routingGridView;

        List<AGVUserControls> allAGVs;
        List<ChargingStationUserControls> allChargingStations;
        List<FillingStationUserControls> allFillingStations;
        List<StorageUserControls> allStorageStations;
        List<MixingUserControls> allMixingStations;
        List<VesselUserControls> allVessels;

        // todo
        public UserControlsCTRL.AGVCTRL activeAGVCTRL;
        private UserControlsCTRL.ChargingCTRL activeChargingCTRL;
        private UserControlsCTRL.FillingCTRL activeFillingCTRL;
        private UserControlsCTRL.MixingCTRL activeMixingCTRL;
        private UserControlsCTRL.StorageCTRL activeStorageCTRL;

        private static PCSMainWindow main;
        public static PCSMainWindow getInstance()
        {
            if (main == null)
            {
                main = new PCSMainWindow();
            }
            return main;
        }

        public PCSMainWindow()
        {
            logWriter = new StreamWriter("PCS.log", false);
            logWriter.WriteLine("#####");
            logWriter.WriteLine("# MULTIFORM PCS v1.1 - Logfile - " + DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second);
            logWriter.WriteLine("#####");

            main = this;

            InitializeComponent();

            debugLED.Fill = (Brush)this.FindResource("LGBGreen");//DEBUG FORCED!

            segmentationCTRL = new CameraDetectionCTRL.SegmentationCTRL();
            segmentationCTRL.Visibility = System.Windows.Visibility.Hidden;
            leftGrid.Children.Add(segmentationCTRL);
            Grid.SetRow(segmentationCTRL, 2);

            cameraCTRL = new CameraDetectionCTRL.CameraCTRL();
            cameraCTRL.Visibility = System.Windows.Visibility.Hidden;
            leftGrid.Children.Add(cameraCTRL);
            Grid.SetRow(cameraCTRL, 2);

            detectionCTRL = new CameraDetectionCTRL.DetectionCTRL();
            detectionCTRL.Visibility = System.Windows.Visibility.Hidden;
            leftGrid.Children.Add(detectionCTRL);
            Grid.SetRow(detectionCTRL, 2);

            countdownStatusBox = new List<int>();
            statusBoxUpdater = new System.Windows.Threading.DispatcherTimer();
            statusBoxUpdater.Tick += new EventHandler(updateStatusBox);
            statusBoxUpdater.Interval = TimeSpan.FromSeconds(1.0d);
            statusBoxUpdater.Start();

            Gateway.ObserverModule observer = Gateway.ObserverModule.getInstance();
            observer.setCurrentPlant(new Gateway.PlantCreationModule().createNewPlant());
            observer.regVisual(this);
            Gateway.CTRLModule.getInstance().constructModules(observer.getCurrentPlant(), 0, 0);
            Gateway.LoadSaveModule.getInstance().writeModelToModelica(observer.getCurrentPlant().theName, false);
            Gateway.LoadSaveModule.getInstance().writeModelToModelica(observer.getCurrentPlant().theName, true);

            routingGridView = new List<Rectangle>();

            allAGVs = new List<AGVUserControls>();
            allChargingStations = new List<ChargingStationUserControls>();
            allFillingStations = new List<FillingStationUserControls>();
            allStorageStations = new List<StorageUserControls>();
            allMixingStations = new List<MixingUserControls>();
            allVessels = new List<VesselUserControls>();

            imageDrawCamera = new Image();
            imageDrawCamera.LayoutTransform = new RotateTransform(180);//Does this rotation of the camera image work?
            imageRed = new Canvas();
            imageRed.LayoutTransform = new RotateTransform(180);//Does this rotation of the camera image work?
            imagePattern = new Image();
            imagePattern.LayoutTransform = new RotateTransform(180);//Does this rotation of the camera image work?
            //plantImage.LayoutTransform = new RotateTransform(180);

            ganttChart = new GanttChart();

            typeShown = 0;
            border22.Background =  (Brush)this.FindResource("LGBWhite"); 
            border22.BorderBrush = Brushes.White;
            border24.Background =  (Brush)this.FindResource("LGBWhite"); 
            border24.BorderBrush = Brushes.White;

            try
            {
                TextReader dymolaEXEPathReader = new StreamReader("dymola.path");
                Gateway.CTRLModule.getInstance().pathToDymola = dymolaEXEPathReader.ReadLine();
                dymolaEXEPathReader.Close();
            }
            catch (Exception)
            {
                //Console.WriteLine(e.StackTrace);
                Gateway.CTRLModule.getInstance().pathToDymola = "NaN";
            }

            this.Focus();

            Datastructure.Model.Plant p = Gateway.ObserverModule.getInstance().getCurrentPlant();

            plantCanvas.Width = p.theSize.Width;
            plantCanvas.Height = p.theSize.Height;

            //GRID EINZEICHNEN
            Line line1 = new Line();
            line1.X1 = 0;
            line1.X2 = plantCanvas.Width;
            line1.Stroke = Brushes.Black;
            line1.StrokeThickness = 1;
            line1.Y1 = line1.Y2 = plantCanvas.Height / 3;
            Line line2 = new Line();
            line2.X1 = 0;
            line2.X2 = plantCanvas.Width;
            line2.StrokeThickness = 1;
            line2.Stroke = Brushes.Black;
            line2.Y1 = line2.Y2 = plantCanvas.Height * 2 / 3;
            Line line3 = new Line();
            line3.X1 = plantCanvas.Width / 2;
            line3.X2 = plantCanvas.Width / 2;
            line3.Stroke = Brushes.Black;
            line3.StrokeThickness = 1;
            line3.Y1 = 0;
            line3.Y2 = plantCanvas.Height;
            plantCanvas.Children.Add(line1);
            plantCanvas.Children.Add(line2);
            plantCanvas.Children.Add(line3);


            for (int i = 0; i < p.AllVessels.Count; i++)
            {
                allVessels.Add(new VesselUserControls(p.AllVessels[i].theId));
                allVessels[allVessels.Count - 1].vesView = new UserControlsView.Vessel(p.AllVessels[i]);
                allVessels[allVessels.Count - 1].vesSum = new UserControlsSummary.VesselSummary(p.AllVessels[i]);
                stackPanelVesselItems.Children.Add(allVessels[allVessels.Count - 1].vesSum);
            }

            for (int i = 0; i < p.AllStations.Count; i++)
            {
                if (p.AllStations[i].isFillingStation())
                {
                    Datastructure.Model.Stations.FillingStation fill = (Datastructure.Model.Stations.FillingStation)p.AllStations[i];
                    allFillingStations.Add(new FillingStationUserControls(fill.theId));

                    allFillingStations[allFillingStations.Count - 1].fillView = new UserControlsView.FillingStation(fill);
                    plantCanvas.Children.Add(allFillingStations[allFillingStations.Count - 1].fillView);
                    Canvas.SetBottom(allFillingStations[allFillingStations.Count - 1].fillView, p.AllStations[i].thePosition.Y - 70.71 / 2);
                    Canvas.SetLeft(allFillingStations[allFillingStations.Count - 1].fillView, p.AllStations[i].thePosition.X - 70.71 / 2);

                    allFillingStations[allFillingStations.Count - 1].fillSum = new UserControlsSummary.FillingStationSummary(fill);
                    stackPanelPlantItems.Children.Add(allFillingStations[allFillingStations.Count - 1].fillSum);

                    allFillingStations[allFillingStations.Count - 1].fillCTRL = new UserControlsCTRL.FillingCTRL(fill);
                }
                else if (p.AllStations[i].isMixingStation())
                {
                    Datastructure.Model.Stations.MixingStation mix = (Datastructure.Model.Stations.MixingStation)p.AllStations[i];
                    allMixingStations.Add(new MixingUserControls(mix.theId));

                    if (mix.theCurrentVessel != null)
                    {
                        allMixingStations[allMixingStations.Count - 1].mixView = new UserControlsView.MixingStation(mix, getVesselUCForVesselID(mix.theCurrentVessel.theId));
                    }
                    else
                    {
                        allMixingStations[allMixingStations.Count - 1].mixView = new UserControlsView.MixingStation(mix, null);
                    }
                    plantCanvas.Children.Add(allMixingStations[allMixingStations.Count - 1].mixView);
                    Canvas.SetBottom(allMixingStations[allMixingStations.Count - 1].mixView, p.AllStations[i].thePosition.Y - 70.71 / 2);
                    Canvas.SetLeft(allMixingStations[allMixingStations.Count - 1].mixView, p.AllStations[i].thePosition.X - 70.71 / 2);

                    allMixingStations[allMixingStations.Count - 1].mixSum = new UserControlsSummary.MixingStationSummary(mix);
                    stackPanelPlantItems.Children.Add(allMixingStations[allMixingStations.Count - 1].mixSum);

                    allMixingStations[allMixingStations.Count - 1].mixCTRL = new UserControlsCTRL.MixingCTRL(mix.theId);
                }
                else if (p.AllStations[i].isChargingStation())
                {
                    Datastructure.Model.Stations.ChargingStation cha = (Datastructure.Model.Stations.ChargingStation)p.AllStations[i];
                    allChargingStations.Add(new ChargingStationUserControls(cha.theId));

                    allChargingStations[allChargingStations.Count - 1].chargeView = new UserControlsView.ChargingStation(cha);
                    plantCanvas.Children.Add(allChargingStations[allChargingStations.Count - 1].chargeView);
                    Canvas.SetBottom(allChargingStations[allChargingStations.Count - 1].chargeView, p.AllStations[i].thePosition.Y - 45 / 2);
                    Canvas.SetLeft(allChargingStations[allChargingStations.Count - 1].chargeView, p.AllStations[i].thePosition.X - 45 / 2);

                    allChargingStations[allChargingStations.Count - 1].chargeSum = new UserControlsSummary.ChargingStationSummary(cha);
                    stackPanelPlantItems.Children.Add(allChargingStations[allChargingStations.Count - 1].chargeSum);

                    allChargingStations[allChargingStations.Count - 1].chargeCTRL = new UserControlsCTRL.ChargingCTRL(cha.theId);
                }
                else if (p.AllStations[i].isStorageStation())
                {
                    Datastructure.Model.Stations.StorageStation sto = (Datastructure.Model.Stations.StorageStation)p.AllStations[i];
                    allStorageStations.Add(new StorageUserControls(sto.theId));

                    List<UserControlsView.Vessel> storageVessel = new List<UserControlsView.Vessel>();
                    for (int j = 0; j < sto.theVessels.Length; j++)
                    {
                        if (sto.theVessels[j] == null)
                        {
                            storageVessel.Add(null);
                        }
                        else
                        {
                            storageVessel.Add(getVesselUCForVesselID(sto.theVessels[j].theId));
                        }
                    }
                    allStorageStations[allStorageStations.Count - 1].stoView = new UserControlsView.StorageStation(sto, storageVessel);
                    plantCanvas.Children.Add(allStorageStations[allStorageStations.Count - 1].stoView);
                    Canvas.SetBottom(allStorageStations[allStorageStations.Count - 1].stoView, p.AllStations[i].thePosition.Y - 150 / 2);
                    Canvas.SetLeft(allStorageStations[allStorageStations.Count - 1].stoView, p.AllStations[i].thePosition.X - 90 / 2);

                    allStorageStations[allStorageStations.Count - 1].stoSum = new UserControlsSummary.StorageStationSummary(sto);
                    stackPanelPlantItems.Children.Add(allStorageStations[allStorageStations.Count - 1].stoSum);

                    allStorageStations[allStorageStations.Count - 1].stoCTRL = new UserControlsCTRL.StorageCTRL(sto);
                }
            }

            for (int i = 0; i < p.AllAGVs.Count; i++)
            {
                allAGVs.Add(new AGVUserControls(p.AllAGVs[i].Id));

                if (p.AllAGVs[i].theVessel != null)
                {
                    allAGVs[allAGVs.Count - 1].agvView = new UserControlsView.AGV(p.AllAGVs[i], getVesselUCForVesselID(p.AllAGVs[i].theVessel.theId));
                }
                else
                {
                    allAGVs[allAGVs.Count - 1].agvView = new UserControlsView.AGV(p.AllAGVs[i], null);
                }
                plantCanvas.Children.Add(allAGVs[allAGVs.Count - 1].agvView);
                Canvas.SetBottom(allAGVs[allAGVs.Count - 1].agvView, p.AllAGVs[i].theCurPosition.Y - p.AllAGVs[i].Diameter / 2);
                Canvas.SetLeft(allAGVs[allAGVs.Count - 1].agvView, p.AllAGVs[i].theCurPosition.X - p.AllAGVs[i].Diameter / 2);

                allAGVs[allAGVs.Count - 1].shadowView = new UserControlsView.AGV(p.AllAGVs[i], null);
                plantCanvas.Children.Add(allAGVs[allAGVs.Count - 1].shadowView);
                Canvas.SetBottom(allAGVs[allAGVs.Count - 1].shadowView, p.AllAGVs[i].ShadowY - p.AllAGVs[i].Diameter / 2);
                Canvas.SetLeft(allAGVs[allAGVs.Count - 1].shadowView, p.AllAGVs[i].ShadowX - p.AllAGVs[i].Diameter / 2);
                allAGVs[allAGVs.Count - 1].shadowView.ellipseAGV.Stroke = Brushes.Green;

                allAGVs[allAGVs.Count - 1].agvSum = new UserControlsSummary.AGVSummary(p.AllAGVs[i]);
                stackPanelPlantItems.Children.Add(allAGVs[allAGVs.Count - 1].agvSum);

                allAGVs[allAGVs.Count - 1].agvCTRL = new UserControlsCTRL.AGVCTRL(p.AllAGVs[i].Id);
            }

            //Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().connectToPLC();
            //showRoutingGrid(true);

        }

        private void updateStatusBox(object sender, EventArgs e)
        {
            for (int i = countdownStatusBox.Count - 1; i >= 0; i--)
            {
                countdownStatusBox[i]--;
                if (countdownStatusBox[i] <= 0 && listBoxStatus.Items.Count > i)
                {
                    try
                    {
                        listBoxStatus.Items.RemoveAt(i);
                    }
                    catch (ArgumentNullException)
                    {
                        //WTF?
                    }
                }
            }
            int index = 0;
            while (index < countdownStatusBox.Count)
            {
                if (countdownStatusBox[index] <= 0)
                {
                    countdownStatusBox.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }
        }

        public void postStatusMessage(string message)
        {
            Dispatcher.Invoke(
             (Action)delegate()
            {
                listBoxStatus.Items.Add(message);
                Console.WriteLine(message);
                countdownStatusBox.Add(15);
                try
                {
                    logWriter.WriteLine(message);
                    logWriter.Flush();
                }
                catch (Exception)
                {
                }
            }
             );
        }

        public void unmarkRunningTAOpt()
        {
            Dispatcher.Invoke(
             (Action)delegate()
             {

                 GUI.PCSMainWindow.getInstance().borderRunTAOpt43.Background = System.Windows.Media.Brushes.LightSteelBlue;
                 GUI.PCSMainWindow.getInstance().borderRunTAOpt43.BorderBrush = System.Windows.Media.Brushes.LightSteelBlue;
             }
             );
        }

        private UserControlsView.Vessel getVesselUCForVesselID(int id)
        {
            for (int i = 0; i < allVessels.Count; i++)
            {
                if (allVessels[i].ID == id)
                {
                    return allVessels[i].vesView;
                }
            }
            return null;
        }

        private bool isDocked(Datastructure.Model.AGV.AGV robot)
        {
            Datastructure.Model.Plant arena = Gateway.ObserverModule.getInstance().getCurrentPlant();
            for (int i = 0; i < arena.AllStations.Count; i++)
            {
                if (robot.theCurPosition.X >= (arena.AllStations[i].theDockingPos.X - 5) && robot.theCurPosition.X <= (arena.AllStations[i].theDockingPos.X + 5)
                    && robot.theCurPosition.Y >= (arena.AllStations[i].theDockingPos.Y - 5) && robot.theCurPosition.Y <= (arena.AllStations[i].theDockingPos.Y + 5))
                {
                    if (arena.AllStations[i].theDockingRot < 5)
                    {
                        if (robot.theRotation >= 360 + arena.AllStations[i].theDockingRot - 5 || robot.theRotation <= arena.AllStations[i].theDockingRot + 5)
                        {
                            return true;
                        }
                    }
                    else if (arena.AllStations[i].theDockingRot > 355)
                    {
                        if (robot.theRotation >= arena.AllStations[i].theDockingRot - 5 || robot.theRotation <= arena.AllStations[i].theDockingRot - 360 + 5)
                        {
                            return true;
                        }
                    }
                    else if (robot.theRotation >= (arena.AllStations[i].theDockingRot - 5) && robot.theRotation <= (arena.AllStations[i].theDockingRot + 5))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void updatePlantView(Datastructure.Model.Plant p)
        {
            //FIRST REMOVE THE VESSELS SO THAT THEY CAN BE CHILDREN OF OTHER ELEMENTS
            for (int i = 0; i < allStorageStations.Count; i++)
            {
                allStorageStations[i].stoView.removeAllVessels(allVessels);
            }
            for (int i = 0; i < allMixingStations.Count; i++)
            {
                allMixingStations[i].mixView.removeAllVessels(allVessels);
            }
            for (int i = 0; i < allAGVs.Count; i++)
            {
                allAGVs[i].agvView.removeAllVessels(allVessels);
            }

            for (int i = 0; i < allVessels.Count; i++)
            {
                allVessels[i].vesSum.updateUC();
            }

            for (int i = 0; i < p.AllAGVs.Count; i++)
            {
                for (int j = 0; j < allAGVs.Count; j++)
                {
                    if (p.AllAGVs[i].Id == allAGVs[j].ID)
                    {
                        Canvas.SetBottom(allAGVs[j].agvView, p.AllAGVs[i].theCurPosition.Y - p.AllAGVs[i].Diameter / 2);
                        Canvas.SetLeft(allAGVs[j].agvView, p.AllAGVs[i].theCurPosition.X - p.AllAGVs[i].Diameter / 2);
                        allAGVs[j].agvView.RenderTransform = new RotateTransform(-p.AllAGVs[i].theRotation + 90, p.AllAGVs[i].Diameter / 2, p.AllAGVs[i].Diameter / 2);

                        Canvas.SetBottom(allAGVs[j].shadowView, p.AllAGVs[i].ShadowY - p.AllAGVs[i].Diameter / 2);
                        Canvas.SetLeft(allAGVs[j].shadowView, p.AllAGVs[i].ShadowX - p.AllAGVs[i].Diameter / 2);
                        allAGVs[j].shadowView.RenderTransform = new RotateTransform(-p.AllAGVs[i].ShadowRot + 90, p.AllAGVs[i].Diameter / 2, p.AllAGVs[i].Diameter / 2);

                        if (isDocked(p.AllAGVs[i]))
                        {
                            allAGVs[j].agvView.markDocking(true);
                        }
                        else
                        {
                            allAGVs[j].agvView.markDocking(false);
                        }
                        if (p.AllAGVs[i].theVessel != null)
                        {
                            UserControlsView.Vessel currentVesselOnAGV = getVesselUCForVesselID(p.AllAGVs[i].theVessel.theId);
                            if (currentVesselOnAGV.isChildOf == false)
                            {
                                allAGVs[j].agvView.mainGrid.Children.Add(currentVesselOnAGV);
                                currentVesselOnAGV.isChildOf = true;
                                currentVesselOnAGV.Margin = new Thickness(11.5, 9, 0, 0);
                            }
                        }
                        allAGVs[j].agvSum.updateUC();
                        break;
                    }
                }
            }
            for (int i = 0; i < p.AllStations.Count; i++)
            {
                if (p.AllStations[i].isChargingStation())
                {
                    for (int j = 0; j < allChargingStations.Count; j++)
                    {
                        if (allChargingStations[j].ID == p.AllStations[i].theId)
                        {
                            Datastructure.Model.Stations.ChargingStation cha = (Datastructure.Model.Stations.ChargingStation)p.AllStations[i];
                            allChargingStations[j].chargeView.updateUC(cha);
                            allChargingStations[j].chargeSum.updateUC(cha);
                            break;
                        }
                    }
                }
                else if (p.AllStations[i].isFillingStation())
                {
                    for (int j = 0; j < allFillingStations.Count; j++)
                    {
                        if (allFillingStations[j].ID == p.AllStations[i].theId)
                        {
                            Datastructure.Model.Stations.FillingStation fill = (Datastructure.Model.Stations.FillingStation)p.AllStations[i];
                            allFillingStations[j].fillView.updateUC(fill);
                            allFillingStations[j].fillSum.updateUC(fill);
                            break;
                        }
                    }
                }
                else if (p.AllStations[i].isMixingStation())
                {
                    for (int j = 0; j < allMixingStations.Count; j++)
                    {
                        if (allMixingStations[j].ID == p.AllStations[i].theId)
                        {
                            Datastructure.Model.Stations.MixingStation mix = (Datastructure.Model.Stations.MixingStation)p.AllStations[i];
                            if (mix.theCurrentVessel == null)
                            {
                                allMixingStations[j].mixView.updateUC(mix, null);
                            }
                            else
                            {
                                allMixingStations[j].mixView.updateUC(mix, getVesselUCForVesselID(mix.theCurrentVessel.theId));
                            }
                            allMixingStations[j].mixSum.updateUC(mix);
                            break;
                        }
                    }
                }
                else if (p.AllStations[i].isStorageStation())
                {
                    Datastructure.Model.Stations.StorageStation sto = (Datastructure.Model.Stations.StorageStation)p.AllStations[i];
                    for (int j = 0; j < allStorageStations.Count; j++)
                    {
                        if (allStorageStations[j].ID == p.AllStations[i].theId)
                        {
                            List<UserControlsView.Vessel> storageVessel = new List<UserControlsView.Vessel>();
                            for (int k = 0; k < sto.theVessels.Length; k++)
                            {
                                if (sto.theVessels[k] == null)
                                {
                                    storageVessel.Add(null);
                                }
                                else
                                {
                                    storageVessel.Add(getVesselUCForVesselID(sto.theVessels[k].theId));
                                }
                            }
                            allStorageStations[j].stoView.updateUC(sto, storageVessel);
                            allStorageStations[j].stoSum.updateUC(sto);
                            break;
                        }
                    }
                }
            }
            TimeSpan t = TimeSpan.FromSeconds(p.theCurSimTime);
            textBlockSimulationTime.Text = (t.Hours < 10 ? "0" + t.Hours.ToString() : t.Hours.ToString()) + ":" + (t.Minutes < 10 ? "0" + t.Minutes.ToString() : t.Minutes.ToString()) + ":" + (t.Seconds < 10 ? "0" + t.Seconds.ToString() : t.Seconds.ToString()) + ":" + (t.Milliseconds < 10 ? "00" + t.Milliseconds.ToString() : (t.Milliseconds < 100 ? "0" + t.Milliseconds.ToString() : t.Milliseconds.ToString()));
        }

        private void sliderZoomPlantView_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (textBlockZoom != null)
            {
                string zoom = sliderZoomPlantView.Value + "";
                zoom = zoom.Replace(',', '.');
                if (zoom.Length < 3)
                {
                    zoom += ".0";
                }
                else
                {
                    zoom = zoom.Substring(0, 3);
                }
                textBlockZoom.Text = zoom + "x";
            }
            plantCanvas.LayoutTransform = new ScaleTransform(sliderZoomPlantView.Value, sliderZoomPlantView.Value);
        }

        private void IsMouseDirectlyOverChanged_ConfigLoad(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border1.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border1.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border1.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border1.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_ConfigEdit(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border2.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border2.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border2.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border2.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_ConfigSave(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border3.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border3.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border3.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border3.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_ApplicationExit(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border4.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border4.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border4.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border4.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        /**private void IsMouseDirectlyOverChanged_AddRecipe(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border5.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                border5.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border5.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border5.BorderBrush = Brushes.LightSteelBlue;
            }
        }*/
        private void IsMouseDirectlyOverChanged_ChangeRecipe(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border6.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border6.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border6.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border6.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        /**private void IsMouseDirectlyOverChanged_RemoveRecipe(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border7.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                border7.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border7.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border7.BorderBrush = Brushes.LightSteelBlue;
            }
        }*/
        private void IsMouseDirectlyOverChanged_Create22Model(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border8.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border8.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border8.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border8.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_Create31Model(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border9.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border9.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border9.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border9.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_StartSocket(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border10.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border10.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().simulationSocketStarted)
                {
                    border10.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border10.BorderBrush = Brushes.White;
                }
                else
                {
                    border10.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border10.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }
        private void IsMouseDirectlyOverChanged_StartDymola6Sim(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border11.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border11.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border11.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border11.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_StartDymola7Sim(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border12.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border12.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border12.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border12.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_ShowSocketInfo(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border13.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border13.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border13.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border13.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_Help(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border14.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border14.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border14.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border14.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_About(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border15.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border15.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border15.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border15.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_MarkSelection(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderMarkSelection.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                borderMarkSelection.BorderBrush = Brushes.SteelBlue;
                border16.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border16.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderMarkSelection.Background =  (Brush)this.FindResource("LGBWhite"); 
                borderMarkSelection.BorderBrush = Brushes.White;
                border16.Background =  (Brush)this.FindResource("LGBWhite"); 
                border16.BorderBrush = Brushes.White;
            }
        }
        private void IsMouseDirectlyOverChanged_RoutingGrid(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderRoutingGrid.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                borderRoutingGrid.BorderBrush = Brushes.SteelBlue;
                border17.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border17.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderRoutingGrid.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderRoutingGrid.BorderBrush = Brushes.LightSteelBlue;
                border17.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border17.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_ShowCamera(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border19.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border19.BorderBrush = Brushes.SteelBlue;
                border30.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border30.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (typeShown == 1)
                {
                    border19.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border19.BorderBrush = Brushes.White;
                    border30.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border30.BorderBrush = Brushes.White;
                }
                else
                {
                    border19.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border19.BorderBrush = Brushes.LightSteelBlue;
                    border30.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border30.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }
        private void IsMouseDirectlyOverChanged_Zoom(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderZoom.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                borderZoom.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderZoom.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderZoom.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_SimTime(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderSimTime.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                borderSimTime.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderSimTime.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderSimTime.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void sliderZoomPlantView_MouseMove(object sender, MouseEventArgs e)
        {
            borderZoom.Background = (Brush)this.FindResource("LGBSteelBlue"); 
            borderZoom.BorderBrush = Brushes.SteelBlue;
        }

        #region keyEvents;
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Datastructure.Model.Plant p = Gateway.ObserverModule.getInstance().getCurrentPlant();
            if (activeAGVCTRL != null)
            {
                if (e.Key == Key.W)
                {
                    activeAGVCTRL.markKey(1, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(activeAGVCTRL.agvID).forward(200, 0, 0, 0);
                    }
                }
                else if (e.Key == Key.S)
                {
                    activeAGVCTRL.markKey(2, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(activeAGVCTRL.agvID).backward(200, 0, 0, 0);
                    }
                }
                else if (e.Key == Key.A)
                {
                    activeAGVCTRL.markKey(3, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(activeAGVCTRL.agvID).turnLeft(225, 0, 0);
                    }
                }
                else if (e.Key == Key.D)
                {
                    activeAGVCTRL.markKey(4, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(activeAGVCTRL.agvID).turnRight(225, 0, 0);
                    }
                }
            }
            else if (activeChargingCTRL != null)
            {
                if (e.Key == Key.C)
                {
                    activeChargingCTRL.markKey(1, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeChargingCTRL.stationID).startLoading();
                    }
                }
            }
            else if (activeFillingCTRL != null)
            {
                if (e.Key == Key.F)
                {
                    activeFillingCTRL.markKey(1, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeFillingCTRL.fill.theId).startFilling(0);
                    }
                }
                else if (e.Key == Key.G)
                {
                    activeFillingCTRL.markKey(2, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeFillingCTRL.fill.theId).startFilling(1);
                    }
                }
            }
            else if (activeMixingCTRL != null)
            {
                if (e.Key == Key.M)
                {
                    activeMixingCTRL.markKey(1, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeMixingCTRL.stationID).startMixing();
                    }
                }
                else if (e.Key == Key.F)
                {
                    activeMixingCTRL.markKey(2, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeMixingCTRL.stationID).startFilling(0);
                    }
                }
                else if (e.Key == Key.W)
                {
                    activeMixingCTRL.markKey(3, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        int toPos = (int)p.getMixingStations()[0].theAltitude + 1;
                        if (toPos > 5)
                        {
                            toPos = 5;
                        }
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeMixingCTRL.stationID).moveVArm(toPos, 10);
                    }
                }
                else if (e.Key == Key.S)
                {
                    activeMixingCTRL.markKey(4, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        int toPos = (int)p.getMixingStations()[0].theAltitude - 1;
                        if (toPos < 1)
                        {
                            toPos = 1;
                        }
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeMixingCTRL.stationID).moveVArm(toPos, 10);
                    }
                }
                else if (e.Key == Key.G)
                {
                    activeMixingCTRL.markKey(5, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeMixingCTRL.stationID).lockContainer();
                    }
                }
                else if (e.Key == Key.R)
                {
                    activeMixingCTRL.markKey(6, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeMixingCTRL.stationID).releaseContainer();
                    }
                }
            }
            else if (activeStorageCTRL != null)
            {
                if (e.Key == Key.D)
                {
                    activeStorageCTRL.markKey(1, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        int toPos = (int)activeStorageCTRL.sto.theTraverse - 1;
                        if (toPos < 1)
                        {
                            toPos = 1;
                        }
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeStorageCTRL.sto.theId).moveHArm(toPos, 10);
                    }
                }
                else if (e.Key == Key.A)
                {
                    activeStorageCTRL.markKey(2, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        int toPos = (int)activeStorageCTRL.sto.theTraverse + 1;
                        if (toPos > 7)
                        {
                            toPos = 7;
                        }
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeStorageCTRL.sto.theId).moveHArm(toPos, 10);
                    }
                }
                else if (e.Key == Key.W)
                {
                    activeStorageCTRL.markKey(3, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        int toPos = (int)activeStorageCTRL.sto.theAltitude + 1;
                        if (toPos > 5)
                        {
                            toPos = 5;
                        }
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeStorageCTRL.sto.theId).moveVArm(toPos, 10);
                    }
                }
                else if (e.Key == Key.S)
                {
                    activeStorageCTRL.markKey(4, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        int toPos = (int)activeStorageCTRL.sto.theAltitude - 1;
                        if (toPos < 1)
                        {
                            toPos = 1;
                        }
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeStorageCTRL.sto.theId).moveVArm(toPos, 10);
                    }
                }
                else if (e.Key == Key.G)
                {
                    activeStorageCTRL.markKey(5, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeStorageCTRL.sto.theId).lockContainer();
                    }
                }
                else if (e.Key == Key.R)
                {
                    activeStorageCTRL.markKey(6, true);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeStorageCTRL.sto.theId).releaseContainer();
                    }
                }
            }
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (activeAGVCTRL != null)
            {
                if (e.Key == Key.W)
                {
                    activeAGVCTRL.markKey(1, false);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(activeAGVCTRL.agvID).forward(0, 0, 0, 0);
                    }
                }
                else if (e.Key == Key.S)
                {
                    activeAGVCTRL.markKey(2, false);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(activeAGVCTRL.agvID).backward(0, 0, 0, 0);
                    }
                }
                else if (e.Key == Key.A)
                {
                    activeAGVCTRL.markKey(3, false);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(activeAGVCTRL.agvID).turnLeft(0, 0, 0);
                    }
                }
                else if (e.Key == Key.D)
                {
                    activeAGVCTRL.markKey(4, false);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(activeAGVCTRL.agvID).turnRight(0, 0, 0);
                    }
                }
            }
            else if (activeChargingCTRL != null)
            {
                if (e.Key == Key.C)
                {
                    activeChargingCTRL.markKey(1, false);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeChargingCTRL.stationID).stopLoading();
                    }
                }
            }
            else if (activeFillingCTRL != null)
            {
                if (e.Key == Key.F)
                {
                    activeFillingCTRL.markKey(1, false);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeFillingCTRL.fill.theId).stopFilling(0);
                    }
                }
                else if (e.Key == Key.G)
                {
                    activeFillingCTRL.markKey(2, false);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeFillingCTRL.fill.theId).stopFilling(1);
                    }
                }
            }
            else if (activeMixingCTRL != null)
            {
                if (e.Key == Key.M)
                {
                    activeMixingCTRL.markKey(1, false);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeMixingCTRL.stationID).stopMixing();
                    }
                }
                else if (e.Key == Key.F)
                {
                    activeMixingCTRL.markKey(2, false);
                    //if (Gateway.CTRLModule.getInstance().SimulationRunning)
                    {
                        Gateway.CTRLModule.getInstance().getStationCTRL(activeMixingCTRL.stationID).stopFilling(0);
                    }
                }
                else if (e.Key == Key.W)
                {
                    activeMixingCTRL.markKey(3, false);
                }
                else if (e.Key == Key.S)
                {
                    activeMixingCTRL.markKey(4, false);
                }
                else if (e.Key == Key.G)
                {
                    activeMixingCTRL.markKey(5, false);
                }
                else if (e.Key == Key.R)
                {
                    activeMixingCTRL.markKey(6, false);
                }
            }
            else if (activeStorageCTRL != null)
            {
                if (e.Key == Key.A)
                {
                    activeStorageCTRL.markKey(1, false);
                }
                else if (e.Key == Key.D)
                {
                    activeStorageCTRL.markKey(2, false);
                }
                else if (e.Key == Key.W)
                {
                    activeStorageCTRL.markKey(3, false);
                }
                else if (e.Key == Key.S)
                {
                    activeStorageCTRL.markKey(4, false);
                }
                else if (e.Key == Key.G)
                {
                    activeStorageCTRL.markKey(5, false);
                }
                else if (e.Key == Key.R)
                {
                    activeStorageCTRL.markKey(6, false);
                }
            }
        }
        #endregion;

        #region mouseClickOnSummaries;
        public void selectAGV(int id)
        {
            for (int j = 0; j < allAGVs.Count; j++)
            {
                if (allAGVs[j].ID == id)
                {
                    unselectAll();
                    allAGVs[j].agvView.markSelection(true);
                    borderCTRL.Child = allAGVs[j].agvCTRL;
                    activeAGVCTRL = allAGVs[j].agvCTRL;
                    allAGVs[j].agvSum.markSelection(true);
                    break;
                }
            }
        }
        public void selectMixingStation(int id)
        {
            for (int j = 0; j < allMixingStations.Count; j++)
            {
                if (allMixingStations[j].ID == id)
                {
                    unselectAll();
                    allMixingStations[j].mixView.markSelection(true);
                    borderCTRL.Child = allMixingStations[j].mixCTRL;
                    activeMixingCTRL = allMixingStations[j].mixCTRL;
                    allMixingStations[j].mixSum.markSelection(true);
                    return;
                }
            }
        }
        public void selectStorageStation(int id)
        {
            for (int j = 0; j < allStorageStations.Count; j++)
            {
                if (allStorageStations[j].ID == id)
                {
                    unselectAll();
                    allStorageStations[j].stoView.markSelection(true);
                    borderCTRL.Child = allStorageStations[j].stoCTRL;
                    activeStorageCTRL = allStorageStations[j].stoCTRL;
                    allStorageStations[j].stoSum.markSelection(true);
                    return;
                }
            }
        }
        public void selectChargingStation(int id)
        {
            for (int j = 0; j < allChargingStations.Count; j++)
            {
                if (allChargingStations[j].ID == id)
                {
                    unselectAll();
                    allChargingStations[j].chargeView.markSelection(true);
                    borderCTRL.Child = allChargingStations[j].chargeCTRL;
                    activeChargingCTRL = allChargingStations[j].chargeCTRL;
                    allChargingStations[j].chargeSum.markSelection(true);
                    return;
                }
            }
        }
        public void selectFillingStation(int id)
        {
            for (int j = 0; j < allFillingStations.Count; j++)
            {
                if (allFillingStations[j].ID == id)
                {
                    unselectAll();
                    allFillingStations[j].fillView.markSelection(true);
                    borderCTRL.Child = allFillingStations[j].fillCTRL;
                    activeFillingCTRL = allFillingStations[j].fillCTRL;
                    allFillingStations[j].fillSum.markSelection(true);
                    return;
                }
            }
        }
        public void selectVessel(int id)
        {
            for (int j = 0; j < allVessels.Count; j++)
            {
                if (allVessels[j].ID == id)
                {
                    unselectVessel();
                    allVessels[j].vesSum.markSelection(true);
                    allVessels[j].vesView.markSelection(true);
                    break;
                }
            }
        }
        #endregion;

        #region canvasClick;
        private void plantCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Datastructure.Model.Plant curPlant = Gateway.ObserverModule.getInstance().getCurrentPlant();
            Datastructure.Model.General.Position clickedPosition = new Datastructure.Model.General.Position(e.GetPosition(plantCanvas).X, curPlant.theSize.Height - e.GetPosition(plantCanvas).Y);

            //Add Vessels here.
            //??
            if (activeAGVCTRL != null && activeAGVCTRL.setNewGoal)
            {
                activeAGVCTRL.setNewGoal = false;
                activeAGVCTRL.setGoalForRobot((int)clickedPosition.X, (int)clickedPosition.Y);
                return;
            }


            for (int i = 0; i < curPlant.AllAGVs.Count; i++)
            {
                if (Math.Abs(curPlant.AllAGVs[i].theCurPosition.X - clickedPosition.X) <= curPlant.AllAGVs[i].Diameter / 2 &&
                    Math.Abs(curPlant.AllAGVs[i].theCurPosition.Y - clickedPosition.Y) <= curPlant.AllAGVs[i].Diameter / 2)
                {
                    for (int j = 0; j < allAGVs.Count; j++)
                    {
                        if (allAGVs[j].ID == curPlant.AllAGVs[i].Id)
                        {
                            unselectAll();
                            allAGVs[j].agvView.markSelection(true);
                            borderCTRL.Child = allAGVs[j].agvCTRL;
                            activeAGVCTRL = allAGVs[j].agvCTRL;
                            allAGVs[j].agvSum.markSelection(true);
                            scrollViewerSummaries.ScrollToVerticalOffset(stackPanelPlantItems.Children.IndexOf(allAGVs[j].agvSum) * 85);
                            return;
                        }
                    }
                }
            }

            for (int i = 0; i < curPlant.AllStations.Count; i++)
            {
                if (Math.Abs(curPlant.AllStations[i].thePosition.X - clickedPosition.X) <= curPlant.AllStations[i].theSize.Width / 2 &&
                    Math.Abs(curPlant.AllStations[i].thePosition.Y - clickedPosition.Y) <= curPlant.AllStations[i].theSize.Height / 2)
                {
                    if (curPlant.AllStations[i].isChargingStation())
                    {
                        for (int j = 0; j < allChargingStations.Count; j++)
                        {
                            if (allChargingStations[j].ID == curPlant.AllStations[i].theId)
                            {
                                unselectAll();
                                allChargingStations[j].chargeView.markSelection(true);
                                borderCTRL.Child = allChargingStations[j].chargeCTRL;
                                activeChargingCTRL = allChargingStations[j].chargeCTRL;
                                allChargingStations[j].chargeSum.markSelection(true);
                                scrollViewerSummaries.ScrollToVerticalOffset(stackPanelPlantItems.Children.IndexOf(allChargingStations[j].chargeSum) * 85);
                                return;
                            }
                        }
                    }
                    else if (curPlant.AllStations[i].isFillingStation())
                    {
                        for (int j = 0; j < allFillingStations.Count; j++)
                        {
                            if (allFillingStations[j].ID == curPlant.AllStations[i].theId)
                            {
                                unselectAll();
                                allFillingStations[j].fillView.markSelection(true);
                                borderCTRL.Child = allFillingStations[j].fillCTRL;
                                activeFillingCTRL = allFillingStations[j].fillCTRL;
                                allFillingStations[j].fillSum.markSelection(true);
                                scrollViewerSummaries.ScrollToVerticalOffset(stackPanelPlantItems.Children.IndexOf(allFillingStations[j].fillSum) * 85);
                                return;
                            }
                        }
                    }
                    else if (curPlant.AllStations[i].isMixingStation())
                    {
                        for (int j = 0; j < allMixingStations.Count; j++)
                        {
                            if (allMixingStations[j].ID == curPlant.AllStations[i].theId)
                            {
                                unselectAll();
                                allMixingStations[j].mixView.markSelection(true);
                                borderCTRL.Child = allMixingStations[j].mixCTRL;
                                activeMixingCTRL = allMixingStations[j].mixCTRL;
                                allMixingStations[j].mixSum.markSelection(true);
                                scrollViewerSummaries.ScrollToVerticalOffset(stackPanelPlantItems.Children.IndexOf(allMixingStations[j].mixSum) * 85);
                                return;
                            }
                        }
                    }
                    else if (curPlant.AllStations[i].isStorageStation())
                    {
                        for (int j = 0; j < allStorageStations.Count; j++)
                        {
                            if (allStorageStations[j].ID == curPlant.AllStations[i].theId)
                            {
                                unselectAll();
                                allStorageStations[j].stoView.markSelection(true);
                                borderCTRL.Child = allStorageStations[j].stoCTRL;
                                activeStorageCTRL = allStorageStations[j].stoCTRL;
                                allStorageStations[j].stoSum.markSelection(true);
                                scrollViewerSummaries.ScrollToVerticalOffset(stackPanelPlantItems.Children.IndexOf(allStorageStations[j].stoSum) * 85);
                                return;
                            }
                        }
                    }
                }
            }

            //Put unselect to right click?
            unselectAll();
        }
        private void unselectAll()
        {
            borderCTRL.Child = null;
            activeAGVCTRL = null;
            activeChargingCTRL = null;
            activeFillingCTRL = null;
            activeMixingCTRL = null;
            activeStorageCTRL = null;

            for (int i = 0; i < allAGVs.Count; i++)
            {
                allAGVs[i].agvView.markSelection(false);
                allAGVs[i].agvSum.markSelection(false);
            }
            for (int i = 0; i < allStorageStations.Count; i++)
            {
                allStorageStations[i].stoView.markSelection(false);
                allStorageStations[i].stoSum.markSelection(false);
            }
            for (int i = 0; i < allFillingStations.Count; i++)
            {
                allFillingStations[i].fillView.markSelection(false);
                allFillingStations[i].fillSum.markSelection(false);
            }
            for (int i = 0; i < allMixingStations.Count; i++)
            {
                allMixingStations[i].mixView.markSelection(false);
                allMixingStations[i].mixSum.markSelection(false);
            }
            for (int i = 0; i < allChargingStations.Count; i++)
            {
                allChargingStations[i].chargeView.markSelection(false);
                allChargingStations[i].chargeSum.markSelection(false);
            }
        }
        private void unselectVessel()
        {
            for (int i = 0; i < allVessels.Count; i++)
            {
                allVessels[i].vesSum.markSelection(false);
                allVessels[i].vesView.markSelection(false);
            }
        }
        #endregion;

        public void showRoutingGrid(bool selected)
        {
            if (selected)
            {
                int interpolation = 10;
                int[][] grid = Gateway.CTRLModule.getInstance().routingModule.getRoutingGrid(interpolation);
                routingGridView.Clear();
                for (int i = 0; i < grid.Length; i++)
                {
                    for (int j = 0; j < grid[i].Length; j++)
                    {
                        Rectangle r = new Rectangle();
                        r.Width = 1;
                        r.Height = 1;
                        r.Opacity = 0.5;
                        if (grid[i][j] == 0)
                        {
                            r.Fill = Brushes.Green;
                        }
                        else if (grid[i][j] == 1)
                        {
                            r.Fill = Brushes.Red;
                        }
                        else if (grid[i][j] == 2)
                        {
                            r.Fill = Brushes.Red;
                        }
                        else if (grid[i][j] == 3)
                        {
                            r.Fill = Brushes.Red;
                        }
                        else if (grid[i][j] == 4)
                        {
                            r.Fill = Brushes.Red;
                        }
                        else if (grid[i][j] == 5)
                        {
                            r.Fill = Brushes.Red;
                        }
                        routingGridView.Add(r);
                        plantCanvas.Children.Add(r);
                        Canvas.SetTop(r, Gateway.ObserverModule.getInstance().getCurrentPlant().theSize.Width - j * interpolation);
                        Canvas.SetLeft(r, i * interpolation);
                    }
                }
            }
            else
            {
                for (int i = 0; i < routingGridView.Count; i++)
                {
                    plantCanvas.Children.Remove(routingGridView[i]);
                }
            }
        }

        private void IsMouseDirectlyOverChanged_Segmentation(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border20.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border20.BorderBrush = Brushes.SteelBlue;
                border31.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border31.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (typeShown == 2)
                {
                    border20.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border20.BorderBrush = Brushes.White;
                    border31.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border31.BorderBrush = Brushes.White;
                }
                else
                {
                    border20.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border20.BorderBrush = Brushes.LightSteelBlue;
                    border31.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border31.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }

        private void IsMouseDirectlyOverChanged_Detection(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border21.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border21.BorderBrush = Brushes.SteelBlue;
                border32.Background = (Brush)this.FindResource("LGBSteelBlue"); 
                border32.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (typeShown == 3)
                {
                    border21.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border21.BorderBrush = Brushes.White;
                    border32.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border32.BorderBrush = Brushes.White;
                }
                else
                {
                    border21.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border21.BorderBrush = Brushes.LightSteelBlue;
                    border32.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border32.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }

        private void IsMouseDirectlyOverChanged_ShowSim(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border22.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                border22.BorderBrush = Brushes.SteelBlue;
                border24.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                border24.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (typeShown == 0)
                {
                    border22.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border22.BorderBrush = Brushes.White;
                    border24.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border24.BorderBrush = Brushes.White;
                }
                else
                {
                    border22.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border22.BorderBrush = Brushes.LightSteelBlue;
                    border24.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border24.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }

        private void IsMouseDirectlyOverChanged_CreateRTNModel(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border23.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                border23.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border23.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border23.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void menuButtonExit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //exiting();

            Application.Current.Shutdown(0);
            e.Handled = true;
        }
        private void exiting()
        {
            postStatusMessage("EXIT");

            Gateway.CTRLModule.getInstance().killDymolaSimulation();
            Gateway.CTRLModule.getInstance().camCtrl.stopCamera();
            Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().stopServer();
            Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().disconnectFromPLC();

            logWriter.WriteLine();
            logWriter.WriteLine();
            logWriter.Close();
        }

        private void menuItemSocketInfo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GUI.ModelicaInterfaceHelpWPF interfaceHelp = new GUI.ModelicaInterfaceHelpWPF();
            interfaceHelp.Show();
            e.Handled = true;
        }

        private void menuItemStartDYM6_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.CTRLModule.getInstance().startDymolaSimulation(false);
            if (Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().simulationSocketStarted)
            {
                border10.Background =  (Brush)this.FindResource("LGBWhite"); 
                border10.BorderBrush = Brushes.White;
            }
            else
            {
                border10.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border10.BorderBrush = Brushes.LightSteelBlue;
            }
            if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                border26.Background =  (Brush)this.FindResource("LGBWhite"); 
                border26.BorderBrush = Brushes.White;
            }
            else
            {
                border26.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border26.BorderBrush = Brushes.LightSteelBlue;
            }
            e.Handled = true;
        }
        private void menuItemStartDYM7_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.CTRLModule.getInstance().startDymolaSimulation(true);
            if (Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().simulationSocketStarted)
            {
                border10.Background =  (Brush)this.FindResource("LGBWhite"); 
                border10.BorderBrush = Brushes.White;
            }
            else
            {
                border10.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border10.BorderBrush = Brushes.LightSteelBlue;
            }
            if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                border26.Background =  (Brush)this.FindResource("LGBWhite"); 
                border26.BorderBrush = Brushes.White;
            }
            else
            {
                border26.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border26.BorderBrush = Brushes.LightSteelBlue;
            }
            e.Handled = true;
        }
        private void menuItemStartSocket_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().startSocketConnection();
            if (Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().simulationSocketStarted)
            {
                border10.Background =  (Brush)this.FindResource("LGBWhite"); 
                border10.BorderBrush = Brushes.White;
            }
            else
            {
                border10.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border10.BorderBrush = Brushes.LightSteelBlue;
            }
            e.Handled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            exiting();
        }

        private void pauseButton_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border25.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                border25.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (Gateway.CTRLModule.getInstance().SimulationRunning && Gateway.CTRLModule.getInstance().SimulationPaused)
                {
                    border25.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border25.BorderBrush = Brushes.White;
                }
                else
                {
                    border25.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border25.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }

        private void playButton_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border26.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                border26.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (Gateway.CTRLModule.getInstance().SimulationRunning && !Gateway.CTRLModule.getInstance().SimulationPaused)
                {
                    border26.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border26.BorderBrush = Brushes.White;
                }
                else
                {
                    border26.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border26.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }

        private void menuItemPauseSim_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Gateway.CTRLModule.getInstance().SimulationRunning && !Gateway.CTRLModule.getInstance().SimulationPaused)
            {
                Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().pauseSim();
                border25.Background =  (Brush)this.FindResource("LGBWhite"); 
                border25.BorderBrush = Brushes.White;
                border26.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border26.BorderBrush = Brushes.LightSteelBlue;
            }
            e.Handled = true;
        }

        private void menuItemResumeSim_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Gateway.CTRLModule.getInstance().SimulationRunning && Gateway.CTRLModule.getInstance().SimulationPaused)
            {
                Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().resumeSim();
                border25.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border25.BorderBrush = Brushes.LightSteelBlue;
                border26.Background =  (Brush)this.FindResource("LGBWhite"); 
                border26.BorderBrush = Brushes.White;
            }
            e.Handled = true;
        }

        private void showCameraImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            typeShown = 1;
            switchContent();
            Gateway.CTRLModule.getInstance().camCtrl.startCamera(imageDrawCamera, imageRed, imagePattern);
        }
        private void showSegmentation_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            typeShown = 2;
            switchContent();
            Gateway.CTRLModule.getInstance().camCtrl.startCamera(imageDrawCamera, imageRed, imagePattern);
        }
        private void showDetection_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            typeShown = 3;
            switchContent();
            Gateway.CTRLModule.getInstance().camCtrl.startCamera(imageDrawCamera, imageRed, imagePattern);
        }

        private void switchContent()
        {
            if (typeShown == 0)
            {
                border20.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border20.BorderBrush = Brushes.LightSteelBlue;
                border31.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border31.BorderBrush = Brushes.LightSteelBlue;
                border21.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border21.BorderBrush = Brushes.LightSteelBlue;
                border32.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border32.BorderBrush = Brushes.LightSteelBlue;
                border19.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border19.BorderBrush = Brushes.LightSteelBlue;
                border30.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border30.BorderBrush = Brushes.LightSteelBlue;
                border36.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border36.BorderBrush = Brushes.LightSteelBlue;

                scrollViewerSim.Visibility = System.Windows.Visibility.Visible;
                stackPanelSimCRTL.Visibility = System.Windows.Visibility.Visible;
                borderSimCTRL.IsEnabled = true;
                borderSimCTRL.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderSimCTRL.BorderBrush = Brushes.SteelBlue;
                border16.Background =  (Brush)this.FindResource("LGBWhite"); 
                border16.BorderBrush = Brushes.White;
                border17.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border17.BorderBrush = Brushes.LightSteelBlue;

                viewboxImages.Visibility = System.Windows.Visibility.Hidden;
                borderCamCTRL.IsEnabled = false;
                borderCamCTRL.Background = (Brush)this.FindResource("LGBGray"); 
                borderCamCTRL.BorderBrush = Brushes.Black;
                if (Gateway.CTRLModule.getInstance().camCtrl.Running)
                {
                    border33.Background =  (Brush)this.FindResource("LGBLightGray"); 
                    border33.BorderBrush = Brushes.LightGray;
                }
                else
                {
                    border33.Background =  (Brush)this.FindResource("LGBGray"); 
                    border33.BorderBrush = Brushes.Silver;
                }
                border34.Background =  (Brush)this.FindResource("LGBGray"); 
                border34.BorderBrush = Brushes.Silver;
                if (Gateway.CTRLModule.getInstance().camCtrl.Res800x600)
                {
                    border35.Background =  (Brush)this.FindResource("LGBLightGray"); 
                    border35.BorderBrush = Brushes.LightGray;
                }
                else
                {
                    border35.Background =  (Brush)this.FindResource("LGBGray"); 
                    border35.BorderBrush = Brushes.Silver;
                }
                if (Gateway.CTRLModule.getInstance().camCtrl.Running && Gateway.CTRLModule.getInstance().camCtrl.detectionAlgorithm.imagesStopped)
                {
                    border37.Background =  (Brush)this.FindResource("LGBLightGray"); 
                    border37.BorderBrush = Brushes.LightGray;
                }
                else
                {
                    border37.Background =  (Brush)this.FindResource("LGBGray"); 
                    border37.BorderBrush = Brushes.Silver;
                }

                segmentationCTRL.Visibility = System.Windows.Visibility.Hidden;
                cameraCTRL.Visibility = System.Windows.Visibility.Hidden;
                detectionCTRL.Visibility = System.Windows.Visibility.Hidden;
                ganttContainer.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (typeShown == 1)
            {
                border20.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border20.BorderBrush = Brushes.LightSteelBlue;
                border31.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border31.BorderBrush = Brushes.LightSteelBlue;
                border21.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border21.BorderBrush = Brushes.LightSteelBlue;
                border32.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border32.BorderBrush = Brushes.LightSteelBlue;
                border22.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border22.BorderBrush = Brushes.LightSteelBlue;
                border24.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border24.BorderBrush = Brushes.LightSteelBlue;
                border36.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border36.BorderBrush = Brushes.LightSteelBlue;

                scrollViewerSim.Visibility = System.Windows.Visibility.Hidden;
                stackPanelSimCRTL.Visibility = System.Windows.Visibility.Hidden;
                borderSimCTRL.IsEnabled = false;
                borderSimCTRL.Background =  (Brush)this.FindResource("LGBGray"); 
                borderSimCTRL.BorderBrush = Brushes.Black;
                border16.Background =  (Brush)this.FindResource("LGBLightGray"); 
                border16.BorderBrush = Brushes.LightGray;
                border17.Background =  (Brush)this.FindResource("LGBGray"); 
                border17.BorderBrush = Brushes.Silver;

                borderCamCTRL.IsEnabled = true;
                borderCamCTRL.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderCamCTRL.BorderBrush = Brushes.SteelBlue;
                viewboxImages.Visibility = System.Windows.Visibility.Visible;
                if (Gateway.CTRLModule.getInstance().camCtrl.Running)
                {
                    border33.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border33.BorderBrush = Brushes.White;
                }
                else
                {
                    border33.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border33.BorderBrush = Brushes.LightSteelBlue;
                }
                border34.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border34.BorderBrush = Brushes.LightSteelBlue;
                if (Gateway.CTRLModule.getInstance().camCtrl.Res800x600)
                {
                    border35.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border35.BorderBrush = Brushes.White;
                }
                else
                {
                    border35.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border35.BorderBrush = Brushes.LightSteelBlue;
                }
                if (Gateway.CTRLModule.getInstance().camCtrl.Running && Gateway.CTRLModule.getInstance().camCtrl.detectionAlgorithm.imagesStopped)
                {
                    border37.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border37.BorderBrush = Brushes.White;
                }
                else
                {
                    border37.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border37.BorderBrush = Brushes.LightSteelBlue;
                }

                if (viewboxImages.Child != imageDrawCamera)
                {
                    viewboxImages.Child = imageDrawCamera;
                }

                segmentationCTRL.Visibility = System.Windows.Visibility.Hidden;
                cameraCTRL.Visibility = System.Windows.Visibility.Visible;
                detectionCTRL.Visibility = System.Windows.Visibility.Hidden;
                ganttContainer.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (typeShown == 2)
            {
                border21.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border21.BorderBrush = Brushes.LightSteelBlue;
                border32.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border32.BorderBrush = Brushes.LightSteelBlue;
                border22.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border22.BorderBrush = Brushes.LightSteelBlue;
                border24.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border24.BorderBrush = Brushes.LightSteelBlue;
                border19.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border19.BorderBrush = Brushes.LightSteelBlue;
                border30.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border30.BorderBrush = Brushes.LightSteelBlue;
                border36.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border36.BorderBrush = Brushes.LightSteelBlue;

                scrollViewerSim.Visibility = System.Windows.Visibility.Hidden;
                stackPanelSimCRTL.Visibility = System.Windows.Visibility.Hidden;
                borderSimCTRL.IsEnabled = false;
                borderSimCTRL.Background =  (Brush)this.FindResource("LGBGray"); 
                borderSimCTRL.BorderBrush = Brushes.Black;
                border16.Background =  (Brush)this.FindResource("LGBLightGray"); 
                border16.BorderBrush = Brushes.LightGray;
                border17.Background =  (Brush)this.FindResource("LGBGray"); 
                border17.BorderBrush = Brushes.Silver;

                borderCamCTRL.IsEnabled = true;
                borderCamCTRL.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderCamCTRL.BorderBrush = Brushes.SteelBlue;
                viewboxImages.Visibility = System.Windows.Visibility.Visible;
                if (Gateway.CTRLModule.getInstance().camCtrl.Running)
                {
                    border33.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border33.BorderBrush = Brushes.White;
                }
                else
                {
                    border33.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border33.BorderBrush = Brushes.LightSteelBlue;
                }
                border34.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border34.BorderBrush = Brushes.LightSteelBlue;
                if (Gateway.CTRLModule.getInstance().camCtrl.Res800x600)
                {
                    border35.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border35.BorderBrush = Brushes.White;
                }
                else
                {
                    border35.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border35.BorderBrush = Brushes.LightSteelBlue;
                }
                if (Gateway.CTRLModule.getInstance().camCtrl.Running && Gateway.CTRLModule.getInstance().camCtrl.detectionAlgorithm.imagesStopped)
                {
                    border37.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border37.BorderBrush = Brushes.White;
                }
                else
                {
                    border37.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border37.BorderBrush = Brushes.LightSteelBlue;
                }

                if (viewboxImages.Child != imageRed)
                {
                    viewboxImages.Child = imageRed;
                }

                segmentationCTRL.Visibility = System.Windows.Visibility.Visible;
                cameraCTRL.Visibility = System.Windows.Visibility.Hidden;
                detectionCTRL.Visibility = System.Windows.Visibility.Hidden;
                ganttContainer.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (typeShown == 3)
            {
                border20.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border20.BorderBrush = Brushes.LightSteelBlue;
                border31.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border31.BorderBrush = Brushes.LightSteelBlue;
                border22.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border22.BorderBrush = Brushes.LightSteelBlue;
                border24.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border24.BorderBrush = Brushes.LightSteelBlue;
                border19.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border19.BorderBrush = Brushes.LightSteelBlue;
                border30.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border30.BorderBrush = Brushes.LightSteelBlue;
                border36.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border36.BorderBrush = Brushes.LightSteelBlue;

                scrollViewerSim.Visibility = System.Windows.Visibility.Hidden;
                stackPanelSimCRTL.Visibility = System.Windows.Visibility.Hidden;
                borderSimCTRL.IsEnabled = false;
                borderSimCTRL.Background =  (Brush)this.FindResource("LGBGray"); 
                borderSimCTRL.BorderBrush = Brushes.Black;
                border16.Background =  (Brush)this.FindResource("LGBLightGray"); 
                border16.BorderBrush = Brushes.LightGray;
                border17.Background =  (Brush)this.FindResource("LGBGray"); 
                border17.BorderBrush = Brushes.Silver;

                borderCamCTRL.IsEnabled = true;
                borderCamCTRL.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderCamCTRL.BorderBrush = Brushes.SteelBlue;
                viewboxImages.Visibility = System.Windows.Visibility.Visible;
                if (Gateway.CTRLModule.getInstance().camCtrl.Running)
                {
                    border33.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border33.BorderBrush = Brushes.White;
                }
                else
                {
                    border33.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border33.BorderBrush = Brushes.LightSteelBlue;
                }
                border34.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border34.BorderBrush = Brushes.LightSteelBlue;
                if (Gateway.CTRLModule.getInstance().camCtrl.Res800x600)
                {
                    border35.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border35.BorderBrush = Brushes.White;
                }
                else
                {
                    border35.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border35.BorderBrush = Brushes.LightSteelBlue;
                }
                if (Gateway.CTRLModule.getInstance().camCtrl.Running && Gateway.CTRLModule.getInstance().camCtrl.detectionAlgorithm.imagesStopped)
                {
                    border37.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border37.BorderBrush = Brushes.White;
                }
                else
                {
                    border37.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border37.BorderBrush = Brushes.LightSteelBlue;
                }

                if (viewboxImages.Child != imagePattern)
                {
                    viewboxImages.Child = imagePattern;
                }

                segmentationCTRL.Visibility = System.Windows.Visibility.Hidden;
                cameraCTRL.Visibility = System.Windows.Visibility.Hidden;
                detectionCTRL.Visibility = System.Windows.Visibility.Visible;
                ganttContainer.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (typeShown == 4)
            {
                border20.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border20.BorderBrush = Brushes.LightSteelBlue;
                border31.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border31.BorderBrush = Brushes.LightSteelBlue;
                border22.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border22.BorderBrush = Brushes.LightSteelBlue;
                border24.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border24.BorderBrush = Brushes.LightSteelBlue;
                border21.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border21.BorderBrush = Brushes.LightSteelBlue;
                border32.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border32.BorderBrush = Brushes.LightSteelBlue;
                border19.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border19.BorderBrush = Brushes.LightSteelBlue;
                border30.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border30.BorderBrush = Brushes.LightSteelBlue;
                border36.Background =  (Brush)this.FindResource("LGBWhite"); 
                border36.BorderBrush = Brushes.White;

                scrollViewerSim.Visibility = System.Windows.Visibility.Hidden;
                stackPanelSimCRTL.Visibility = System.Windows.Visibility.Hidden;
                borderSimCTRL.IsEnabled = false;
                borderSimCTRL.Background =  (Brush)this.FindResource("LGBGray"); 
                borderSimCTRL.BorderBrush = Brushes.Black;
                border16.Background =  (Brush)this.FindResource("LGBLightGray"); 
                border16.BorderBrush = Brushes.LightGray;
                border17.Background =  (Brush)this.FindResource("LGBGray"); 
                border17.BorderBrush = Brushes.Silver;

                viewboxImages.Visibility = System.Windows.Visibility.Hidden;
                borderCamCTRL.IsEnabled = false;
                borderCamCTRL.Background =  (Brush)this.FindResource("LGBGray"); 
                borderCamCTRL.BorderBrush = Brushes.Black;
                if (Gateway.CTRLModule.getInstance().camCtrl.Running)
                {
                    border33.Background =  (Brush)this.FindResource("LGBLightGray"); 
                    border33.BorderBrush = Brushes.LightGray;
                }
                else
                {
                    border33.Background =  (Brush)this.FindResource("LGBGray"); 
                    border33.BorderBrush = Brushes.Silver;
                }
                border34.Background =  (Brush)this.FindResource("LGBGray"); 
                border34.BorderBrush = Brushes.Silver;
                if (Gateway.CTRLModule.getInstance().camCtrl.Res800x600)
                {
                    border35.Background =  (Brush)this.FindResource("LGBLightGray"); 
                    border35.BorderBrush = Brushes.LightGray;
                }
                else
                {
                    border35.Background =  (Brush)this.FindResource("LGBGray"); 
                    border35.BorderBrush = Brushes.Silver;
                }
                if (Gateway.CTRLModule.getInstance().camCtrl.Running && Gateway.CTRLModule.getInstance().camCtrl.detectionAlgorithm.imagesStopped)
                {
                    border37.Background =  (Brush)this.FindResource("LGBLightGray"); 
                    border37.BorderBrush = Brushes.LightGray;
                }
                else
                {
                    border37.Background =  (Brush)this.FindResource("LGBGray"); 
                    border37.BorderBrush = Brushes.Silver;
                }

                segmentationCTRL.Visibility = System.Windows.Visibility.Hidden;
                cameraCTRL.Visibility = System.Windows.Visibility.Hidden;
                detectionCTRL.Visibility = System.Windows.Visibility.Hidden;

                if (ganttChart != null)
                {
                    ganttContainer.Visibility = System.Windows.Visibility.Visible;
                }

                if (!ganttContainer.Children.Contains(ganttChart))
                {
                    ganttContainer.Children.Add(ganttChart);
                }
            }
        }

        private void showSim_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            typeShown = 0;
            switchContent();
        }

        private void IsMouseDirectlyOverChanged_StartLive(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border33.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                border33.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (Gateway.CTRLModule.getInstance().camCtrl.Running)
                {
                    border33.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border33.BorderBrush = Brushes.White;
                }
                else
                {
                    border33.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border33.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }
        private void IsMouseDirectlyOverChanged_StopLive(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border34.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                border34.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border34.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border34.BorderBrush = Brushes.LightSteelBlue;
            }
        }
        private void IsMouseDirectlyOverChanged_SetLQ(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border35.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                border35.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (Gateway.CTRLModule.getInstance().camCtrl.Res800x600)
                {
                    border35.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border35.BorderBrush = Brushes.White;
                }
                else
                {
                    border35.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border35.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }
        private void startCam_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.CTRLModule.getInstance().camCtrl.startCamera(imageDrawCamera, imageRed, imagePattern);
        }
        private void stopCam_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.CTRLModule.getInstance().camCtrl.stopCamera();
            border33.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
            border33.BorderBrush = Brushes.LightSteelBlue;
        }
        private void camSetLQ_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.CTRLModule.getInstance().camCtrl.changeResolutionAndRestartCamera(imageDrawCamera, imageRed, imagePattern);
            if (Gateway.CTRLModule.getInstance().camCtrl.Running)
            {
                border33.Background =  (Brush)this.FindResource("LGBWhite"); 
                border33.BorderBrush = Brushes.White;
            }
            else
            {
                border33.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border33.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void IsMouseDirectlyOverChanged_PauseLive(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border37.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                border37.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (Gateway.CTRLModule.getInstance().camCtrl.Running && Gateway.CTRLModule.getInstance().camCtrl.detectionAlgorithm.imagesStopped)
                {
                    border37.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border37.BorderBrush = Brushes.White;
                }
                else
                {
                    border37.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border37.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }
        private void pauseCam_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.CTRLModule.getInstance().camCtrl.pauseCamera();
        }

        private void IsMouseDirectlyOverChanged_DYNLogo(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderDYN.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                borderDYN.BorderBrush = Brushes.SteelBlue;
                borderDYN3.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                borderDYN3.BorderBrush = Brushes.SteelBlue;
                borderDYN4.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                borderDYN4.BorderBrush = Brushes.SteelBlue;
                borderDYN5.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                borderDYN5.BorderBrush = Brushes.SteelBlue;
                borderDYN6.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                borderDYN6.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderDYN.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderDYN.BorderBrush = Brushes.LightSteelBlue;
                borderDYN3.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderDYN3.BorderBrush = Brushes.LightSteelBlue;
                borderDYN4.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderDYN4.BorderBrush = Brushes.LightSteelBlue;
                borderDYN5.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderDYN5.BorderBrush = Brushes.LightSteelBlue;
                borderDYN6.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderDYN6.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void border9_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.LoadSaveModule.getInstance().writeModelToModelica(Gateway.ObserverModule.getInstance().getCurrentPlant().theName, false);
        }

        private void border8_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.LoadSaveModule.getInstance().writeModelToModelica(Gateway.ObserverModule.getInstance().getCurrentPlant().theName, true);
        }

        private void IsMouseDirectlyOverChanged_Scheduling(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border36.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                border36.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (typeShown == 4)
                {
                    border36.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border36.BorderBrush = Brushes.White;
                }
                else
                {
                    border36.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border36.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }

        /**private void IsMouseDirectlyOverChanged_ConnectIRobot(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border38.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                border38.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border38.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border38.BorderBrush = Brushes.LightSteelBlue;
            }
        }*/

        private void IsMouseDirectlyOverChanged_StartRobotServer(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border40.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                border40.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                {
                    border40.Background =  (Brush)this.FindResource("LGBWhite"); 
                    border40.BorderBrush = Brushes.White;
                }
                else
                {
                    border40.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    border40.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }

        private void IsMouseDirectlyOverChanged_ConfigRobotServer(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border39.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                border39.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border39.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border39.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void IsMouseDirectlyOverChanged_StopRobotServer(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                border41.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                border41.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                border41.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border41.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void border40_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().startServer();
            e.Handled = true;
        }

        private void border41_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().stopServer();
            e.Handled = true;
            if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
            {
                border40.Background =  (Brush)this.FindResource("LGBWhite"); 
                border40.BorderBrush = Brushes.White;
            }
            else
            {
                border40.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                border40.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void openIRobotServerConfig_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IRobotServerConfiguration serverConfig = new IRobotServerConfiguration();
            serverConfig.ShowDialog();
        }

        private void IsMouseDirectlyOverChanged_ConnectToPLC(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderPLCTest.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                borderPLCTest.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().Connected)
                {
                    borderPLCTest.Background =  (Brush)this.FindResource("LGBWhite"); 
                    borderPLCTest.BorderBrush = Brushes.White;
                }
                else
                {
                    borderPLCTest.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    borderPLCTest.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }

        private void borderConnectToPLC_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().connectToPLC();
        }

        private void IsMouseDirectlyOverChanged_DisconnectPLC(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderPLCDisconnect.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                borderPLCDisconnect.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderPLCDisconnect.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderPLCDisconnect.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void borderDisconnectPLC_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().disconnectFromPLC();
        }

        private void buttonCalStoHor_Click(object sender, RoutedEventArgs e)
        {

            //moved
        }

        private void buttonCalStoVer_Click(object sender, RoutedEventArgs e)
        {
            //moved
        }

        private void buttonCalMixVer_Click(object sender, RoutedEventArgs e)
        {
            //moved to mixing ctrl
        }

        private void border6_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Gateway.CTRLModule.getInstance().AutomaticCTRLRunning)
            {
                if (!ControlModules.SchedulingModule.TAOptModule.getInstance().getRunning())
                {
                    recipeConfig = new RecipeConfiguration();
                    recipeConfig.ShowDialog();
                }
                else
                {
                    postStatusMessage("A change of the recipes is not possible while the optimization of TAOpt 4.3 is running!");
                }
            }
            else
            {
                postStatusMessage("NOT POSSIBLE, AUTOMATIC RUNNING...");
            }
        }

        private void border23_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Gateway.CTRLModule.getInstance().AutomaticCTRLRunning)
            {
                ControlModules.SchedulingModule.TAOptModel.TAOptModelBuilder.getInstance().buildModel(Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData, System.IO.Directory.GetCurrentDirectory());
            }
            else
            {
                postStatusMessage("NOT POSSIBLE, AUTOMATIC RUNNING...");
            }
        }

        private void IsMouseDirectlyOverChanged_RunTAOpt(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderRunTAOpt43.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                borderRunTAOpt43.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (ControlModules.SchedulingModule.TAOptModule.getInstance().getRunning())
                {
                    borderRunTAOpt43.Background =  (Brush)this.FindResource("LGBWhite"); 
                    borderRunTAOpt43.BorderBrush = Brushes.White;
                }
                else
                {
                    borderRunTAOpt43.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    borderRunTAOpt43.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }

        private void borderRunTAOpt43_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Gateway.CTRLModule.getInstance().AutomaticCTRLRunning)
            {
                ControlModules.SchedulingModule.TAOptModule.getInstance().scheduleRecipes();
                postStatusMessage("Starting scheduling of the recipes by TAOpt 4.3 ...");

                borderRunTAOpt43.Background =  (Brush)this.FindResource("LGBWhite"); 
                borderRunTAOpt43.BorderBrush = Brushes.White;
            }
            else
            {
                postStatusMessage("NOT POSSIBLE, AUTOMATIC RUNNING...");
            }
        }

        private void border36_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            typeShown = 4;
            switchContent();
        }

        public void drawGanttChart(Datastructure.Schedule.DetailedProductionPlan detailed, Datastructure.Schedule.Schedule s)
        {
            Dispatcher.Invoke(
             (Action)delegate()
             {
                 ganttChart.drawSchedule(detailed, s);
                 switchContent();

                 updateVesselSummaries();
             }
             );
        }

        public void updateVesselSummaries()
        {
            Dispatcher.Invoke(
            (Action)delegate()
            {
                //Farben auslesen
                Datastructure.Schedule.Schedule s = Gateway.CTRLModule.getInstance().CurrentSchedule;

                if (s != null)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            Datastructure.Model.Plant p = Gateway.ObserverModule.getInstance().getCurrentPlant();
                            if (s.RawData.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.YELLOW)
                            {
                                p.AllVessels[i].Colors[j] = 0;
                            }
                            else if (s.RawData.selectedRecipes[i].ElementAt(j) == Datastructure.Model.Recipes.RecipeData.RECIPE.BLACK)
                            {
                                p.AllVessels[i].Colors[j] = 1;
                            }
                            else if (s.RawData.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.RED)
                            {
                                p.AllVessels[i].Colors[j] = 2;
                            }
                            else if (s.RawData.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLUE)
                            {
                                p.AllVessels[i].Colors[j] = 3;
                            }
                            else if (s.RawData.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.PURPLE)
                            {
                                p.AllVessels[i].Colors[j] = 4;
                            }
                            else if (s.RawData.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.ORANGE)
                            {
                                p.AllVessels[i].Colors[j] = 5;
                            }
                            else if (s.RawData.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.GREEN)
                            {
                                p.AllVessels[i].Colors[j] = 6;
                            }
                        }
                    }

                    //hier VesselSummaries Updaten
                    foreach (VesselUserControls ves in allVessels)
                    {
                        ves.vesSum.updateUC();
                    }
                }
            });
        }

        private void IsMouseDirectlyOverChanged_ExecuteSchedule(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderExecuteSchedule.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                borderExecuteSchedule.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (Gateway.CTRLModule.getInstance().AutomaticCTRLRunning)
                {
                    borderExecuteSchedule.Background =  (Brush)this.FindResource("LGBWhite"); 
                    borderExecuteSchedule.BorderBrush = Brushes.White;
                }
                else
                {
                    borderExecuteSchedule.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    borderExecuteSchedule.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }

        public void setPLCLights(int status)
        {
            Dispatcher.Invoke(
             (Action)delegate()
             {
                 if (status == 0)
                 {
                     plcLight1.Fill = (Brush)this.FindResource("LGBRed");
                     plcLight2.Fill = Brushes.LightSteelBlue;
                 }
                 else if (status == 1)
                 {
                     plcLight1.Fill = (Brush)this.FindResource("LGBYellow");
                     plcLight2.Fill = Brushes.LightSteelBlue;
                 }
                 else if (status == 2)
                 {
                     if (plcLight1.Fill == (Brush)this.FindResource("LGBGreen"))
                     {
                         plcLight1.Fill = Brushes.LightSteelBlue;
                         plcLight2.Fill = (Brush)this.FindResource("LGBGreen");
                     }
                     else
                     {
                         plcLight1.Fill = (Brush)this.FindResource("LGBGreen");
                         plcLight2.Fill = Brushes.LightSteelBlue;
                     }
                 }
             }
             );
        }

        private void borderDYN_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //NOTAUS
            emergencyStop = true;
            this.IsEnabled = false;
            borderDYN.IsEnabled = true;

            Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(new byte[64]);

            //Roboter stop
            //IROBOT SERVER //CHANGE HERE TODO
            if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
            {
                for (int i = 0; i < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; i++)
                {
                    Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[i].connection.send(Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[i].protocol.writeDirectDrive(0, 0));
                }
            }
        }
        private void borderExecuteSchedule_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Gateway.CTRLModule.getInstance().AutomaticCTRLRunning)
            {
                AutomaticExecution aExeWindow = new AutomaticExecution();
                aExeWindow.ShowDialog();
            }
            else
            {
                postStatusMessage("NOT POSSIBLE, AUTOMATIC RUNNING...");
            }
        }

        public void updateGanttChartUI(TimeSpan t)
        {
            Dispatcher.Invoke(
            (Action)delegate()
           {
               ganttChart.updateExecutionTime(t);
           });
        }

        public void addDelayAlarmLED(string content)
        {
            if (delayLED == null)
            {
                delayLED = new List<Ellipse>();
            }

            Border delayAlarmLED = new Border();
            delayAlarmLED.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
            delayAlarmLED.CornerRadius = new CornerRadius(5);
            delayAlarmLED.Margin = new Thickness(0, 0, 5, 0);
            delayAlarmLED.BorderBrush = Brushes.Black;
            Grid g = new Grid();
            delayAlarmLED.Child = g;
            Ellipse e = new Ellipse();
            e.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            e.Margin = new Thickness(5, 5, 5, 5);
            e.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            e.Height = 20;
            e.Width = 20;
            e.Stroke = Brushes.Black;
            e.StrokeThickness = 2;
            e.Fill = Brushes.SteelBlue;
            g.Children.Add(e);
            TextBlock info = new TextBlock();
            info.Text = content;
            info.Margin = new Thickness(35, 5, 5, 5);
            info.FontSize = 14;
            info.Height = 20;
            info.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            info.TextAlignment = TextAlignment.Left;
            info.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            g.Children.Add(info);

            delayLED.Add(e);

            listViewDelayAlarmLED.Items.Add(delayAlarmLED);
        }
        public void setLEDAlarm(bool[] alarm)
        {
            for (int i = 0; i < alarm.Length; i++)
            {
                if (alarm[i])
                {
                    delayLED[i].Fill = (Brush)this.FindResource("LGBRed");
                }
                else
                {
                    delayLED[i].Fill = Brushes.SteelBlue;
                }
            }
        }

        public void setAGVLED(int led, bool active)
        {
            if (led == 0)
            {
                if (active)
                {
                    agv5LED.Fill = (Brush)this.FindResource("LGBGreen");
                }
                else
                {
                    agv5LED.Fill = (Brush)this.FindResource("LGBRed");
                }
            }
            else if (led == 1)
            {
                if (active)
                {
                    agv4LED.Fill = (Brush)this.FindResource("LGBGreen");
                }
                else
                {
                    agv4LED.Fill = (Brush)this.FindResource("LGBRed");
                }
            }
            else if (led == 2)
            {
                if (active)
                {
                    agv3LED.Fill = (Brush)this.FindResource("LGBGreen");
                }
                else
                {
                    agv3LED.Fill = (Brush)this.FindResource("LGBRed");
                }
            }
            else if (led == 3)
            {
                if (active)
                {
                    agv2LED.Fill = (Brush)this.FindResource("LGBGreen");
                }
                else
                {
                    agv2LED.Fill = (Brush)this.FindResource("LGBRed");
                }
            }
            else if (led == 4)
            {
                if (active)
                {
                    agv1LED.Fill = (Brush)this.FindResource("LGBGreen");
                }
                else
                {
                    agv1LED.Fill = (Brush)this.FindResource("LGBRed");
                }
            }
        }

        private void debugLED_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Gateway.CTRLModule.getInstance().AutomaticCTRLRunning)
            {
                //Gateway.CTRLModule.getInstance().Debug = !Gateway.CTRLModule.getInstance().Debug;
                if (Gateway.CTRLModule.getInstance().Debug)
                {
                    debugLED.Fill = (Brush)this.FindResource("LGBGreen");
                }
                else
                {
                    debugLED.Fill = Brushes.LightSteelBlue;
                }
            }
        }

        private void buttonDebugACK_Click(object sender, RoutedEventArgs e)
        {
            Gateway.CTRLModule.getInstance().Ack = true;
        }

        private void IsMouseDirectlyOverChanged_PauseSchedule(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderPauseExecution.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                borderPauseExecution.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                if (Gateway.CTRLModule.getInstance().Paused)
                {
                    borderPauseExecution.Background =  (Brush)this.FindResource("LGBWhite"); 
                    borderPauseExecution.BorderBrush = Brushes.White;
                }
                else
                {
                    borderPauseExecution.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                    borderPauseExecution.BorderBrush = Brushes.LightSteelBlue;
                }
            }
        }

        private void borderPauseExecution_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.CTRLModule.getInstance().Paused = !Gateway.CTRLModule.getInstance().Paused;
            if (Gateway.CTRLModule.getInstance().Paused)
            {
                borderPauseExecution.Background =  (Brush)this.FindResource("LGBWhite"); 
                borderPauseExecution.BorderBrush = Brushes.White;
            }
            else
            {
                borderPauseExecution.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderPauseExecution.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void IsMouseDirectlyOverChanged_StopSchedule(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderSTOPExecution.Background =  (Brush)this.FindResource("LGBSteelBlue"); 
                borderSTOPExecution.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderSTOPExecution.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
                borderSTOPExecution.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void borderStopExecution_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.CTRLModule.getInstance().ABORT_AUTOMATIC();
            Gateway.CTRLModule.getInstance().Paused = false;
            borderPauseExecution.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
            borderPauseExecution.BorderBrush = Brushes.LightSteelBlue;
            borderExecuteSchedule.Background =  (Brush)this.FindResource("LGBLightSteelBlue"); 
            borderExecuteSchedule.BorderBrush = Brushes.LightSteelBlue;
        }

        private void IsMouseDirectlyOverChanged_PrintGanttChart(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Boolean)e.NewValue == true)
            {
                borderPrintGanttChart.Background = (Brush)this.FindResource("LGBSteelBlue");
                borderPrintGanttChart.BorderBrush = Brushes.SteelBlue;
            }
            else
            {
                borderPrintGanttChart.Background = (Brush)this.FindResource("LGBLightSteelBlue");
                borderPrintGanttChart.BorderBrush = Brushes.LightSteelBlue;
            }
        }

        private void borderPrintGanttChart_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string printTitle = "MULTIFORM Pipeless Plant Schedule";
            double i = ganttChart.canvasDrawGantt.ActualWidth;
            double j = ganttChart.canvasDrawGantt.ActualHeight;
            TransformGroup printGroup = new TransformGroup();
            PrintDialog printDlg = new System.Windows.Controls.PrintDialog();

            double borderMargin = 20;

            if (printDlg.ShowDialog() == true)
            {
                //get selected printer capabilities
                System.Printing.PrintCapabilities capabilities = printDlg.PrintQueue.GetPrintCapabilities(printDlg.PrintTicket);
                //canvasDrawGantt.Width = capabilities.PageImageableArea.ExtentHeight - borderMargin * 2;
                //canvasDrawGantt.Height = capabilities.PageImageableArea.ExtentWidth - borderMargin * 2;
                //canvasDrawGantt.UpdateLayout();

                //get scale of the print wrt to screen of WPF visual
                double scale = Math.Min((capabilities.PageImageableArea.ExtentWidth - borderMargin * 2) / j, (capabilities.PageImageableArea.ExtentHeight - borderMargin * 2) / i);

                //Transform the Visual to scale
                printGroup.Children.Add(new ScaleTransform(scale, scale));
                //printGroup.Children.Add(new ScaleTransform((capabilities.PageImageableArea.ExtentWidth - borderMargin * 2) / i, (capabilities.PageImageableArea.ExtentHeight - borderMargin * 2) / j));

                printGroup.Children.Add(new RotateTransform(90));
                ganttChart.canvasDrawGantt.LayoutTransform = printGroup;
                //canvasDrawGantt.UpdateLayout();

                //get the size of the printer page
                Size sz = new Size(capabilities.PageImageableArea.ExtentWidth - borderMargin * 2, capabilities.PageImageableArea.ExtentHeight - borderMargin * 2);
                //update the layout of the visual to the printer page size.
                ganttChart.canvasDrawGantt.Measure(sz);
                ganttChart.canvasDrawGantt.Arrange(new Rect(new System.Windows.Point(capabilities.PageImageableArea.OriginWidth + borderMargin, capabilities.PageImageableArea.OriginHeight + borderMargin), sz));
                printDlg.PrintVisual(ganttChart.canvasDrawGantt, printTitle.Trim());

                ganttChart.canvasDrawGantt.Height = j;
                ganttChart.canvasDrawGantt.Width = i;
                ganttChart.canvasDrawGantt.LayoutTransform = null;
                //canvasDrawGantt.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                //canvasDrawGantt.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                ganttChart.canvasDrawGantt.UpdateLayout();
            }
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }
    }
}
