using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.IO;
using System.Threading;
using MULTIFORM_PCS.ControlModules.SchedulingModule;
using MULTIFORM_PCS.ControlModules.FeedForwadModule;
using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.DataTypes;
using MULTIFORM_PCS.ControlModules.CameraModule.CameraForm;
using MULTIFORM_PCS.ControlModules.CameraControl.CameraControlClass;
using System.Windows.Threading;
using System.Diagnostics; // Process
using System.Globalization;
using Emgu.CV.WPF;



namespace MULTIFORM_PCS.ControlModules.MPCModule
{
    public class NMPC


    {
        //public static runRobots sample;
        Thread MPCThread;
        bool MPCrunning = false;
        public double dist1Odometer = 0;
        public double angle1Odometer = 0;
        public double dist2Odometer = 0;
        public double angle2Odometer = 0;
        public double dist3Odometer = 0;
        public double angle3Odometer = 0;
        int[] Agvinfo = { 1, 0, 0 };
       // public int[] odometerValues = { 0, 0, 0, 0, 0, 0 };
        public double getDist10doometer()
        {
            return this.dist1Odometer;
        }


        public void values(string message)
        {

            string[] words = message.Split(' ');
            if ((message.Contains("192.168.1.101")))
            {
                if (words[2] == "distance")
                {
                    double deltaDist = Convert.ToDouble(words[3]);
                    this.dist1Odometer += deltaDist;
                }
                else if (words[2] == "angle")
                {
                    double deltaAngle = Convert.ToDouble(words[3]);
                    this.angle1Odometer += deltaAngle;
                }

            }

            if ((message.Contains("192.168.1.102")))
            {
                if (words[2] == "distance")
                {
                    double deltaDist = Convert.ToDouble(words[3]);
                    this.dist2Odometer += deltaDist;
                }
                else if (words[2] == "angle")
                {
                    double deltaAngle = Convert.ToDouble(words[3]);
                    this.angle2Odometer += deltaAngle;
                }

            }
            if ((message.Contains("192.168.1.104")))
            {
                if (words[2] == "distance")
                {
                    double deltaDist = Convert.ToDouble(words[3]);
                    this.dist3Odometer += deltaDist;
                }
                else if (words[2] == "angle")
                {
                    double deltaAngle = Convert.ToDouble(words[3]);
                    this.angle3Odometer += deltaAngle;
                }


            }
            //Console.WriteLine("11  : " + dist1Odometer + " 22:" + dist3Odometer);
        }

        /// <summary>
        /// Function to run one Robot
        /// </summary>
        /// <param name="currentRobot"></param>
        /// <param name="robot1Final"></param>
        /// <param name="dock"></param>

        public void oneRobot(int currentRobot, double[] robot1Final, bool dock)
        {
            int[] RobotAssingment = new int[] { 0, 1, 3 }; // assingment of LEDs to IDs
            double[] delta_distance = new double[] { 0, 0, 0 };
            double[] delta_theta = new double[] { 0, 0, 0 };
            Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
            RobotDiscription[] RobotArray = new RobotDiscription[] { Gateway.CTRLModule.getInstance().camCtrl.RobotA, Gateway.CTRLModule.getInstance().camCtrl.RobotB, 
                                                                    Gateway.CTRLModule.getInstance().camCtrl.RobotC };
            //Arrays to save current position of Robot
            double[] Current_X = new double[RobotArray.Length];
            double[] Current_Y = new double[RobotArray.Length];
            double[] Current_Angle = new double[RobotArray.Length];
            //Update Current Image to get current Position of Robots
            Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
            if (Gateway.CTRLModule.getInstance().camCtrl.MyImage2 != null && Gateway.CTRLModule.getInstance().camCtrl.PlotLEDsAndUpdateStrings == true)
            {
                MULTIFORM_PCS.GUI.PCSMainWindow.getInstance().image1.Source = BitmapSourceConvert.ToBitmapSource(Gateway.CTRLModule.getInstance().camCtrl.MyImage2);
                Gateway.CTRLModule.getInstance().camCtrl.MyImage2.Dispose();
                Gateway.CTRLModule.getInstance().camCtrl.imOriginal.Dispose();
            }
            //get Current Robot1 Postion            
            Current_X[currentRobot] = RobotArray[currentRobot].Apex.X;
            Current_Y[currentRobot] = RobotArray[currentRobot].Apex.Y;
            Current_Angle[currentRobot] = RobotArray[currentRobot].Angle;
            
            //to scale angle from -pi to pi
            if (Current_Angle[currentRobot] > 180) { Current_Angle[currentRobot] = Current_Angle[currentRobot] - 360; }
            else if (Current_Angle[currentRobot] < -180) { Current_Angle[currentRobot] = Current_Angle[currentRobot] + 360; }
            
            //Robot1 current states and final position send to python                    
            double x1Int = Current_X[currentRobot]/1000.0;
            double y1Int = Current_Y[currentRobot]/1000.0;
            
            Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
            RobotArray = new RobotDiscription[] { Gateway.CTRLModule.getInstance().camCtrl.RobotA, Gateway.CTRLModule.getInstance().camCtrl.RobotB, 
                                                                    Gateway.CTRLModule.getInstance().camCtrl.RobotC };
            //delete Gaurd files if exist to execute C# and python parallel(should be created by python).Gaurd files are used to run c# or python to run synchronously
            // Flag to check python have started or not
            FileInfo startFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\pythonStarted.log");
            if (startFlag.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\pythonStarted.log"); }
            // Flag to check velocity is written or not
            FileInfo velFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\velocityWritten.log");
            if (velFlag.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\velocityWritten.log"); }
            //for setting orientation as sampling time will be changed
            FileInfo changeSamTime = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\changeSampleTime.log");
            if (changeSamTime.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\changeSampleTime.log"); }
            //Gaurd File pythonStarted.log is used to check when python will Exit.
            FileInfo pythonExit = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\pythonExit.log");
            if (pythonExit.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\pythonExit.log"); }
            FileInfo robot1moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\robot1moved.log");
            if (robot1moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\robot1moved.log"); }
            FileInfo robot2moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\robot2moved.log");
            if (robot2moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\robot2moved.log"); }

            if (MPCrunning == false)
            {
                MPCrunning = false;
                //this.threadInterrupt.Reset();
                this.MPCThread = new Thread(() =>
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                    //MPCThread.Join();
                    //Console.WriteLine(this.MPCThread);












                    // create new process (the python program) which will run parallel to c#
                    Process p = new Process();
                    p.StartInfo.FileName = "python.exe";
                    p.StartInfo.Arguments = AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\mainFb.py " + x1Int + " " + y1Int +
                                            " " + Current_Angle[currentRobot] + " " + robot1Final[0] + " " + robot1Final[1] ;
                    p.Start();
                    //Gaurd File pythonStarted.log is used to check when python will start.
                    startFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\pythonStarted.log");
                    //waiting for Python to start
                    while (!startFlag.Exists) { Thread.Sleep(10); startFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\pythonStarted.log"); }
                    //initializing velocities of left and right wheels of the robot
                    short left1 = 0; short right1 = 0;
                    //file containing the  time step for which the robot should run and also mpc Total steps so that c# and python exit at same time
                    StreamReader InitialValuesPython = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\IntStepSize.log", false);
                    List<string> linesOfIntValue = new List<string>();
                    List<string[]> IntValue = new List<string[]>();
                    while (!InitialValuesPython.EndOfStream)
                    {
                        string line = InitialValuesPython.ReadLine();
                        if (line != "")
                        {
                            linesOfIntValue.Add(line);
                            IntValue.Add(line.Split('\t', ' '));
                        }
                    }
                    InitialValuesPython.Close();
                    int SleepTime = Convert.ToInt32(IntValue[0][0]);
                    int TotalSteps = Convert.ToInt32(IntValue[0][1]);
                    int odowait = 0;
                    Encoder v = new Encoder();



                    dist1Odometer = 0;
                    dist2Odometer = 0;
                    dist3Odometer = 0;
                    angle1Odometer = 0;
                    angle2Odometer = 0;
                    angle3Odometer = 0;


                    //main loop to transfer robots
                    for (int i = 0; i < 500; i++)
                    {   //For encoders
                        changeSamTime = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\changeSampleTime.log");
                        Console.WriteLine("dist1Odometer:" + dist1Odometer);
                       
                            if (currentRobot == 0)
                            {
                                //delta_theta[robLoop] = Convert.ToDouble(Math.PI / 180) * (angle1Odometer);
                                delta_theta[currentRobot] = angle1Odometer;
                                delta_distance[currentRobot] = dist1Odometer;
                            }
                            if (currentRobot == 1)
                            {
                                //delta_theta[robLoop] = Convert.ToDouble(Math.PI / 180) * angle2Odometer;
                                delta_theta[currentRobot] = angle2Odometer;
                                delta_distance[currentRobot] = dist2Odometer;

                            }
                            if (currentRobot == 2)
                            {
                                //delta_theta[robLoop] = Convert.ToDouble(Math.PI / 180) * angle3Odometer;
                                delta_theta[currentRobot] = angle3Odometer;
                                delta_distance[currentRobot] = dist3Odometer;

                            }


                            Console.WriteLine("odo"+dist1Odometer);
                        //Update GUI and Camera Position of robot
                        Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
                        RobotArray = new RobotDiscription[] { Gateway.CTRLModule.getInstance().camCtrl.RobotA, Gateway.CTRLModule.getInstance().camCtrl.RobotB, Gateway.CTRLModule.getInstance().camCtrl.RobotC };
                        Controller[] ControllerArray = new Controller[] { Gateway.CTRLModule.getInstance().Controller_Robot1, Gateway.CTRLModule.getInstance().Controller_Robot2, Gateway.CTRLModule.getInstance().Controller_Robot3 };
                        //int[] RobotAssingment = new int[] { 0, 1, 3 }; // assingment of LEDs to IDs
                        int[,] Destinations = new[,] { { 3510, 400 }, { 600, 600 }, { 0, 600 } };
                        int[] Orientations = new int[] { 180, 180, 285 };
                        int[] DirectionControllerOutput = new int[RobotArray.Length];
                        int[] Robot_Phase = new int[RobotArray.Length];
                        if (Gateway.CTRLModule.getInstance().camCtrl.MyImage2 != null && Gateway.CTRLModule.getInstance().camCtrl.PlotLEDsAndUpdateStrings == true)
                        {
                            MULTIFORM_PCS.GUI.PCSMainWindow.getInstance().image1.Source = BitmapSourceConvert.ToBitmapSource(Gateway.CTRLModule.getInstance().camCtrl.MyImage2);
                            Gateway.CTRLModule.getInstance().camCtrl.MyImage2.Dispose();
                            Gateway.CTRLModule.getInstance().camCtrl.imOriginal.Dispose();
                        }
                        //initializing velocity matrix to be given to all robots  
                        double[][] velocity1 = new double[RobotArray.Length][];
                        //Robot1 Cam Pos                   
                        Current_X[currentRobot] = RobotArray[currentRobot].Apex.X;
                        Current_Y[currentRobot] = RobotArray[currentRobot].Apex.Y;
                        Current_Angle[currentRobot] = RobotArray[currentRobot].Angle;
                        
                        //to scale angle from -pi to pi
                        if (Current_Angle[currentRobot] > 180) { Current_Angle[currentRobot] = Current_Angle[currentRobot] - 360; }
                        else if (Current_Angle[currentRobot] < -180) { Current_Angle[currentRobot] = Current_Angle[currentRobot] + 360; }
                        
                        //writing current position to posData.log file to be read by python                  
                        StreamWriter posData = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\posData.log", false);
                        posData.WriteLine(Current_X[currentRobot] / 1000.0 + "\t" + Current_Y[currentRobot] / 1000.0 + "\t" + Current_Angle[currentRobot]) ;
                        posData.Close();
                        //do not read current position 1st time
                        if (i > 0)
                        {
                            //file to know if robot have moved or not from Encoders
                            if (Math.Abs(delta_distance[0]) > 0 || Math.Abs(delta_theta[0]) > 0)
                            { StreamWriter Robot1move = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\robot1moved.log", false); Robot1move.Close(); }
                            
                            //Gaurd file to be written after current position is written python will wait for this file to be written
                            StreamWriter posFlag = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\posWritten.log", false);
                            posFlag.Close();
                        }
                        //to read left and left wheel velocities from python                    
                        StreamReader velocityReader;
                        List<string[]> velocitySeq = new List<string[]>();
                        //gaurd file for the velocity to be written
                        velFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\velocityWritten.log");
                        //check if python has exits and exits loop after stopping robots
                        pythonExit = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\pythonExit.log");
                        if (pythonExit.Exists)
                        {
                            velocity1[currentRobot] = new double[] { 0, 0 };
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot]).forward(velocity1[currentRobot], 0, 0, 0);
                            
                            break;
                        }
                        // for setting orientation at the target position new sampling time
                        if (changeSamTime.Exists) { SleepTime = 50; }
                        //wait for python to write velocities
                        while (!velFlag.Exists) { Thread.Sleep(1); velFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\velocityWritten.log"); }
                        for (int j = 0; j <= 50; j++)
                        {
                            try
                            {
                                velocityReader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\wheel.log", false);
                                List<string> linesOfVelocity = new List<string>();
                                while (!velocityReader.EndOfStream)
                                {
                                    string line = velocityReader.ReadLine();
                                    if (line != "")
                                    {
                                        linesOfVelocity.Add(line);
                                        velocitySeq.Add(line.Split('\t', ' '));
                                    }
                                }
                                velocityReader.Close();
                                double vLeft1 = Convert.ToDouble(velocitySeq[0][0])*1000.0;
                                double vRight1 = Convert.ToDouble(velocitySeq[0][1])*1000.0;
                                
                                left1 = Convert.ToInt16(vLeft1);
                                right1 = Convert.ToInt16(vRight1);
                                
                                Console.WriteLine("read");
                                break;
                            }
                            catch (IOException)
                            {
                                left1 = 0;
                                right1 = 0;
                                if (j == 50) {
                                    velocity1[currentRobot] = new double[] { 0, 0 };
                                    Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot]).forward(velocity1[currentRobot], 0, 0, 0);

                                    break;
                                }
                                
                            }
                        }
                        velocity1[currentRobot] = new double[] { left1, right1 };
                        
                        Console.WriteLine(right1 + "  " + left1);
                        
                        //only give velocities from controller after 3rd iteration
                        if (i > 2)
                        {
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot]).forward(velocity1[currentRobot], 0, 0, 0);
                           
                        }
                        // check if total number of loops in python hae reached then stop the robots and exit 
                        if (i == TotalSteps)
                        {
                            velocity1[currentRobot] = new double[] { 0, 0 };
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot]).forward(velocity1[currentRobot], 0, 0, 0);
                            
                            break;
                        }
                        //Time to remove values from Encoders(150 ms delay from router to recieve packets)
                        //if (SleepTime > 150 & odowait == 0)
                        //{
                        //    Thread.Sleep(100);
                        //    SleepTime = SleepTime - 100;
                        //    odowait = 1;
                        //}
                       
                        //reseting values for the next measurement
                        dist1Odometer = 0;
                        dist2Odometer = 0;
                        dist3Odometer = 0;
                        angle1Odometer = 0;
                        angle2Odometer = 0;
                        angle3Odometer = 0;
                        robot1moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\robot1moved.log");
                        if (robot1moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\robot1moved.log"); }
                        robot2moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\robot2moved.log");
                        if (robot2moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\robot2moved.log"); }
                        //sampling time for current velocities so that robot will run till sampling time
                        //Thread.Sleep(SleepTime);
                        Console.WriteLine(SleepTime);
                        //reinintializing velocity seq array so that new velocities can be written
                        velocitySeq = new List<string[]>();
                        //try to delete  velocity gaurd file so that in next loop wait for new velocities to be written
                        for (int l = 0; l <= 100; l++)
                        {
                            try
                            {
                                if (velFlag.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\velocityWritten.log"); }
                                velFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\velocityWritten.log"); break;
                            }
                            catch (IOException) { Thread.Sleep(10); }
                        }
                    }
                    //deleting temporary gaurd files 
                    pythonExit = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\pythonExit.log");
                   // if (pythonExit.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\pythonExit.log"); }
                    startFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\pythonStarted.log");
                    if (startFlag.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\pythonStarted.log"); }
                    p.WaitForExit();
                    //docking if required
                    if (currentRobot == 2) { currentRobot = 3; }
                    else if (currentRobot == 2) { currentRobot = 3; }
                    if (dock == true) { Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(currentRobot).dock(); }
                    
                    changeSamTime = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\changeSampleTime.log");
                    if (changeSamTime.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\changeSampleTime.log"); }
                    robot1moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\robot1moved.log");
                    if (robot1moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\robot1moved.log"); }
                    robot2moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\robot2moved.log");
                    if (robot2moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_1robot\\robot2moved.log"); }


                    Thread.Sleep(20);
                    MPCrunning = false;
                    //threadInterrupt.Set();
                    this.MPCThread = null;
                });
                this.MPCThread.Start();
                //MPCThread.Join();
                //MPCrunning = false;
            }
        }


        

        public void oneRobot_MS(int[] currentRobot, double[] robot1Final, bool dock)
        {

            int[] RobotAssingment = new int[] { 0, 1, 3 }; // assingment of LEDs to IDs
            double[] delta_distance = new double[] { 0, 0, 0 };
            double[] delta_theta = new double[] { 0, 0, 0 };
            Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
            RobotDiscription[] RobotArray = new RobotDiscription[] { Gateway.CTRLModule.getInstance().camCtrl.RobotA, Gateway.CTRLModule.getInstance().camCtrl.RobotB, 
                                                                    Gateway.CTRLModule.getInstance().camCtrl.RobotC };
            //Arrays to save current position of Robot
            double[] Current_X = new double[RobotArray.Length];
            double[] Current_Y = new double[RobotArray.Length];
            double[] Current_Angle = new double[RobotArray.Length];
            //Update Current Image to get current Position of Robots
            Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
            if (Gateway.CTRLModule.getInstance().camCtrl.MyImage2 != null && Gateway.CTRLModule.getInstance().camCtrl.PlotLEDsAndUpdateStrings == true)
            {
                MULTIFORM_PCS.GUI.PCSMainWindow.getInstance().image1.Source = BitmapSourceConvert.ToBitmapSource(Gateway.CTRLModule.getInstance().camCtrl.MyImage2);
                Gateway.CTRLModule.getInstance().camCtrl.MyImage2.Dispose();
                Gateway.CTRLModule.getInstance().camCtrl.imOriginal.Dispose();
            }
            //get Current Robot1 Postion            
            Current_X[currentRobot[0]] = RobotArray[currentRobot[0]].Apex.X;  //robot to be controlled
            Current_Y[currentRobot[0]] = RobotArray[currentRobot[0]].Apex.Y;
            Current_Angle[currentRobot[0]] = RobotArray[currentRobot[0]].Angle;
            //get Current Robot2 Position     
            Current_X[currentRobot[1]] = RobotArray[currentRobot[1]].Apex.X;       //dynamic Obstacle
            Current_Y[currentRobot[1]] = RobotArray[currentRobot[1]].Apex.Y;
            Current_Angle[currentRobot[1]] = RobotArray[currentRobot[1]].Angle;
            //to scale angle from -pi to pi
            if (Current_Angle[currentRobot[0]] > 180) { Current_Angle[currentRobot[0]] = Current_Angle[currentRobot[0]] - 360; }
            else if (Current_Angle[currentRobot[0]] < -180) { Current_Angle[currentRobot[0]] = Current_Angle[currentRobot[0]] + 360; }
            if (Current_Angle[currentRobot[1]] > 180) { Current_Angle[currentRobot[1]] = Current_Angle[currentRobot[1]] - 360; }
            else if (Current_Angle[currentRobot[1]] < -180) { Current_Angle[currentRobot[1]] = Current_Angle[currentRobot[1]] + 360; }
            //Robot1 current states and final position send to python                    
            double x1Int = Current_X[currentRobot[0]] / 1000.0;
            double y1Int = Current_Y[currentRobot[0]] / 1000.0;
            //Robot2 current states and final position send to python
            double x2Int = Current_X[currentRobot[1]] / 1000.0;
            double y2Int = Current_Y[currentRobot[1]] / 1000.0;
            Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
            RobotArray = new RobotDiscription[] { Gateway.CTRLModule.getInstance().camCtrl.RobotA, Gateway.CTRLModule.getInstance().camCtrl.RobotB, 
                                                                    Gateway.CTRLModule.getInstance().camCtrl.RobotC };
            //delete Gaurd files if exist to execute C# and python parallel(should be created by python).Gaurd files are used to run c# or python to run synchronously
            // Flag to check python have started or not
            FileInfo startFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\pythonStarted.log");
            if (startFlag.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\pythonStarted.log"); }
            // Flag to check velocity is written or not
            FileInfo velFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\velocityWritten.log");
            if (velFlag.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\velocityWritten.log"); }
            //for setting orientation as sampling time will be changed
            FileInfo changeSamTime = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\changeSampleTime.log");
            if (changeSamTime.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\changeSampleTime.log"); }
            //Gaurd File pythonStarted.log is used to check when python will Exit.
            FileInfo pythonExit = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\pythonExit.log");
            if (pythonExit.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\pythonExit.log"); }
            FileInfo robot1moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\robot1moved.log");
            if (robot1moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\robot1moved.log"); }
            FileInfo robot2moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\robot2moved.log");
            if (robot2moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\robot2moved.log"); }

            if (MPCrunning == false)
            {
                MPCrunning = true;
                //this.threadInterrupt.Reset();
                this.MPCThread = new Thread(() =>
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                    //MPCThread.Join();
                    //Console.WriteLine(this.MPCThread);
                    





                    // create new process (the python program) which will run parallel to c#
                    Process p = new Process();
                    p.StartInfo.FileName = "python.exe";
                    p.StartInfo.Arguments = AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\mainCSharp.py " + x1Int + " " + y1Int +
                                            " " + Current_Angle[currentRobot[0]] + " " + robot1Final[0] + " " + robot1Final[1] + " " + x2Int +
                                            " " + y2Int + " " + Current_Angle[currentRobot[1]] + " " + robot1Final[0] + " " + robot1Final[1];
                    p.Start();
                    //Gaurd File pythonStarted.log is used to check when python will start.
                    startFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\pythonStarted.log");
                    //waiting for Python to start
                    while (!startFlag.Exists) { Thread.Sleep(10); startFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\pythonStarted.log"); }
                    //initializing velocities of left and right wheels of the robot
                    short left1 = 0; short right1 = 0; short left2 = 0; short right2 = 0;
                    //file containing the  time step for which the robot should run and also mpc Total steps so that c# and python exit at same time
                    StreamReader InitialValuesPython = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\IntStepSize.log", false);
                    List<string> linesOfIntValue = new List<string>();
                    List<string[]> IntValue = new List<string[]>();
                    while (!InitialValuesPython.EndOfStream)
                    {
                        string line = InitialValuesPython.ReadLine();
                        if (line != "")
                        {
                            linesOfIntValue.Add(line);
                            IntValue.Add(line.Split('\t', ' '));
                        }
                    }
                    InitialValuesPython.Close();
                    int SleepTime = Convert.ToInt32(IntValue[0][0]);
                    int TotalSteps = Convert.ToInt32(IntValue[0][1]);
                    int odowait = 0;
                    Encoder v = new Encoder();



                    dist1Odometer = 0;
                    dist2Odometer = 0;
                    dist3Odometer = 0;
                    angle1Odometer = 0;
                    angle2Odometer = 0;
                    angle3Odometer = 0;


                    //main loop to transfer robots
                    for (int i = 0; i < 300; i++)
                    {   //For encoders
                        changeSamTime = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\changeSampleTime.log");
                        Console.WriteLine("dist1Odometer:" + dist1Odometer);
                        for (int robLoop = 0; robLoop < currentRobot.Length; robLoop++)
                        {
                            if (currentRobot[robLoop] == 0)
                            {
                                //delta_theta[robLoop] = Convert.ToDouble(Math.PI / 180) * (angle1Odometer);
                                delta_theta[robLoop] = angle1Odometer;
                                delta_distance[robLoop] = dist1Odometer;
                            }
                            if (currentRobot[robLoop] == 1)
                            {
                                //delta_theta[robLoop] = Convert.ToDouble(Math.PI / 180) * angle2Odometer;
                                delta_theta[robLoop] = angle2Odometer;
                                delta_distance[robLoop] = dist2Odometer;

                            }
                            if (currentRobot[robLoop] == 2)
                            {
                                //delta_theta[robLoop] = Convert.ToDouble(Math.PI / 180) * angle3Odometer;
                                delta_theta[robLoop] = angle3Odometer;
                                delta_distance[robLoop] = dist3Odometer;

                            }
                        }


                        //Update GUI and Camera Position of robot
                        Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
                        RobotArray = new RobotDiscription[] { Gateway.CTRLModule.getInstance().camCtrl.RobotA, Gateway.CTRLModule.getInstance().camCtrl.RobotB, Gateway.CTRLModule.getInstance().camCtrl.RobotC };
                        Controller[] ControllerArray = new Controller[] { Gateway.CTRLModule.getInstance().Controller_Robot1, Gateway.CTRLModule.getInstance().Controller_Robot2, Gateway.CTRLModule.getInstance().Controller_Robot3 };
                        //int[] RobotAssingment = new int[] { 0, 1, 3 }; // assingment of LEDs to IDs
                        int[,] Destinations = new[,] { { 3510, 400 }, { 600, 600 }, { 0, 600 } };
                        int[] Orientations = new int[] { 180, 180, 285 };
                        int[] DirectionControllerOutput = new int[RobotArray.Length];
                        int[] Robot_Phase = new int[RobotArray.Length];
                        if (Gateway.CTRLModule.getInstance().camCtrl.MyImage2 != null && Gateway.CTRLModule.getInstance().camCtrl.PlotLEDsAndUpdateStrings == true)
                        {
                            MULTIFORM_PCS.GUI.PCSMainWindow.getInstance().image1.Source = BitmapSourceConvert.ToBitmapSource(Gateway.CTRLModule.getInstance().camCtrl.MyImage2);
                            Gateway.CTRLModule.getInstance().camCtrl.MyImage2.Dispose();
                            Gateway.CTRLModule.getInstance().camCtrl.imOriginal.Dispose();
                        }
                        //initializing velocity matrix to be given to all robots  
                        double[][] velocity1 = new double[RobotArray.Length][];
                        //Robot1 Cam Pos                   
                        Current_X[currentRobot[0]] = RobotArray[currentRobot[0]].Apex.X;
                        Current_Y[currentRobot[0]] = RobotArray[currentRobot[0]].Apex.Y;
                        Current_Angle[currentRobot[0]] = RobotArray[currentRobot[0]].Angle;
                        //Robot2 Cam Pos                    
                        Current_X[currentRobot[1]] = RobotArray[currentRobot[1]].Apex.X;
                        Current_Y[currentRobot[1]] = RobotArray[currentRobot[1]].Apex.Y;
                        Current_Angle[currentRobot[1]] = RobotArray[currentRobot[1]].Angle;
                        //to scale angle from -pi to pi
                        if (Current_Angle[currentRobot[0]] > 180) { Current_Angle[currentRobot[0]] = Current_Angle[currentRobot[0]] - 360; }
                        else if (Current_Angle[currentRobot[0]] < -180) { Current_Angle[currentRobot[0]] = Current_Angle[currentRobot[0]] + 360; }
                        if (Current_Angle[currentRobot[1]] > 180) { Current_Angle[currentRobot[1]] = Current_Angle[currentRobot[1]] - 360; }
                        else if (Current_Angle[currentRobot[1]] < -180) { Current_Angle[currentRobot[1]] = Current_Angle[currentRobot[1]] + 360; }
                        //writing current position to posData.log file to be read by python                  
                        StreamWriter posData = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\posData.log", false);
                        posData.WriteLine(Current_X[currentRobot[0]] / 1000.0 + "\t" + Current_Y[currentRobot[0]] / 1000.0 + "\t" + Current_Angle[currentRobot[0]] + "\t" +
                                          Current_X[currentRobot[1]] / 1000.0 + "\t" + Current_Y[currentRobot[1]] / 1000.0 + "\t" + Current_Angle[currentRobot[1]]);
                        posData.Close();
                        //do not read current position 1st time
                        if (i > 0)
                        {
                            //file to know if robot have moved or not from Encoders
                            if (Math.Abs(delta_distance[0]) > 0 || Math.Abs(delta_theta[0]) > 0)
                            { StreamWriter Robot1move = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\robot1moved.log", false); Robot1move.Close(); }
                            if (Math.Abs(delta_distance[1]) > 0 || Math.Abs(delta_theta[1]) > 0)
                            { StreamWriter Robot2move = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\robot2moved.log", false); Robot2move.Close(); }
                            //Gaurd file to be written after current position is written python will wait for this file to be written
                            StreamWriter posFlag = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\posWritten.log", false);
                            posFlag.Close();
                        }
                        //to read left and left wheel velocities from python                    
                        StreamReader velocityReader;
                        List<string[]> velocitySeq = new List<string[]>();
                        //gaurd file for the velocity to be written
                        velFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\velocityWritten.log");
                        //check if python has exits and exits loop after stopping robots
                        pythonExit = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\pythonExit.log");
                        if (pythonExit.Exists)
                        {
                            velocity1[currentRobot[0]] = new double[] { 0, 0 };
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[0]]).forward(velocity1[currentRobot[0]], 0, 0, 0);
                            velocity1[currentRobot[1]] = new double[] { 0, 0 };
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[1]]).forward(velocity1[currentRobot[1]], 0, 0, 0);
                            break;
                        }
                        // for setting orientation at the target position new sampling time
                        if (changeSamTime.Exists) { SleepTime = 50; }
                        //wait for python to write velocities
                        while (!velFlag.Exists) { Thread.Sleep(1); velFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\velocityWritten.log"); }
                        for (int j = 0; j <= 50; j++)
                        {
                            try
                            {
                                velocityReader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\wheel.log", false);
                                List<string> linesOfVelocity = new List<string>();
                                while (!velocityReader.EndOfStream)
                                {
                                    string line = velocityReader.ReadLine();
                                    if (line != "")
                                    {
                                        linesOfVelocity.Add(line);
                                        velocitySeq.Add(line.Split('\t', ' '));
                                    }
                                }
                                velocityReader.Close();
                                double vLeft1 = Convert.ToDouble(velocitySeq[0][0]) * 1000.0;
                                double vRight1 = Convert.ToDouble(velocitySeq[0][1]) * 1000.0;
                                //double vLeft2 = Convert.ToDouble(velocitySeq[0][2]) * 1000.0;
                                //double vRight2 = Convert.ToDouble(velocitySeq[0][3]) * 1000.0;
                                left1 = Convert.ToInt16(vLeft1);
                                right1 = Convert.ToInt16(vRight1);
                                //left2 = Convert.ToInt16(vLeft2);
                                //right2 = Convert.ToInt16(vRight2);
                                Console.WriteLine("read");
                                break;
                            }
                            catch (IOException)
                            {
                                left1 = 0;
                                right1 = 0;
                                left2 = 0;
                                right2 = 0;
                            }
                        }
                        velocity1[currentRobot[0]] = new double[] { left1, right1 };
                        velocity1[currentRobot[1]] = new double[] { left2, right2 };
                        Console.WriteLine(right1 + "  " + left1);
                        Console.WriteLine(right2 + "  " + left2);
                        //only give velocities from controller after 3rd iteration
                        if (i > 2)
                        {
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[0]]).forward(velocity1[currentRobot[0]], 0, 0, 0);
                            //Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[1]]).forward(velocity1[currentRobot[1]], 0, 0, 0);
                            
                            /*Setting a constant velocity to the second robot to use it as a dynamic obstacle*/
                            if (i < 75)
                            {
                                if(i<29)
                                    {
                                velocity1[currentRobot[1]] = new double[] { 140, 140 }; //
                                    }
                                else
                                {
                                    velocity1[currentRobot[1]] = new double[] { 140, 140 }; //
                                    if (i > 35) {
                                        velocity1[currentRobot[1]] = new double[] { 140, 140 }; //
                                    }
                                }
                            }
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[1]]).forward(velocity1[currentRobot[1]], 0, 0, 0);
                        }
                        // check if total number of loops in python hae reached then stop the robots and exit 
                        if (i == TotalSteps)
                        {
                            velocity1[currentRobot[0]] = new double[] { 0, 0 };
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[0]]).forward(velocity1[currentRobot[0]], 0, 0, 0);
                            /*Stop the dynamic obstacle after the loop is exited */
                            velocity1[currentRobot[1]] = new double[] { 0, 0 };
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[1]]).forward(velocity1[currentRobot[1]], 0, 0, 0);
                            break;
                        }
                        //Time to remove values from Encoders(150 ms delay from router to recieve packets)
                        if (SleepTime > 150 & odowait == 0)
                        {
                            //Thread.Sleep(100);
                            SleepTime = SleepTime - 100;
                            odowait = 1;
                        }
                        //Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI(); 
                        //if (Gateway.CTRLModule.getInstance().camCtrl.MyImage2 != null && Gateway.CTRLModule.getInstance().camCtrl.PlotLEDsAndUpdateStrings == true)
                        //{
                        //    image1.Source = BitmapSourceConvert.ToBitmapSource(Gateway.CTRLModule.getInstance().camCtrl.MyImage2);
                        //    Gateway.CTRLModule.getInstance().camCtrl.MyImage2.Dispose();
                        //    Gateway.CTRLModule.getInstance().camCtrl.imOriginal.Dispose();
                        //} 
                        //reseting values for the next measurement
                        dist1Odometer = 0;
                        dist2Odometer = 0;
                        dist3Odometer = 0;
                        angle1Odometer = 0;
                        angle2Odometer = 0;
                        angle3Odometer = 0;
                        robot1moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\robot1moved.log");
                        if (robot1moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\robot1moved.log"); }
                        robot2moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\robot2moved.log");
                        if (robot2moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\robot2moved.log"); }
                        //sampling time for current velocities so that robot will run till sampling time
                        //Thread.Sleep(SleepTime);
                        Console.WriteLine(SleepTime);
                        //reinintializing velocity seq array so that new velocities can be written
                        velocitySeq = new List<string[]>();
                        //try to delete  velocity gaurd file so that in next loop wait for new velocities to be written
                        for (int l = 0; l <= 100; l++)
                        {
                            try
                            {
                                if (velFlag.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\velocityWritten.log"); }
                                velFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\velocityWritten.log"); break;
                            }
                            catch (IOException) { Thread.Sleep(10); }
                        }
                    }
                    //deleting temporary gaurd files 
                    pythonExit = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\pythonExit.log");
                    if (pythonExit.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\pythonExit.log"); }
                    startFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\pythonStarted.log");
                    if (startFlag.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\pythonStarted.log"); }
                    p.WaitForExit();
                    //docking if required
                    if (currentRobot[0] == 2) { currentRobot[0] = 3; }
                    else if (currentRobot[1] == 2) { currentRobot[1] = 3; }
                    if (dock == true) { Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(currentRobot[0]).dock(); }
                    //if (dock[1] == true) { Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(currentRobot[1]).dock(); }
                    changeSamTime = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\changeSampleTime.log");
                    if (changeSamTime.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\changeSampleTime.log"); }
                    robot1moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\robot1moved.log");
                    if (robot1moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\robot1moved.log"); }
                    robot2moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\robot2moved.log");
                    if (robot2moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\MS1Robot\\robot2moved.log"); }


                    Thread.Sleep(100);
                    MPCrunning = false;
                    //threadInterrupt.Set();
                    this.MPCThread = null;
                });
                this.MPCThread.Start();
                //MPCThread.Join();
                //MPCrunning = false;
            }

        }
















        /// <summary>
        /// Function to run two Robots
        /// </summary>
        /// <param name="currentRobot"></param>
        /// <param name="robot1Final"></param>
        /// <param name="robot2Final"></param>
        /// <param name="dock"></param>
        public void twoRobots(int[] currentRobot,double[] robot1Final,double[] robot2Final,bool[] dock)
        {
            int[] RobotAssingment = new int[] { 0, 1, 3 }; // assingment of LEDs to IDs
            double[] delta_distance = new double[] { 0, 0, 0 };
            double[] delta_theta = new double[] { 0, 0, 0 };
            Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
            RobotDiscription[] RobotArray = new RobotDiscription[] { Gateway.CTRLModule.getInstance().camCtrl.RobotA, Gateway.CTRLModule.getInstance().camCtrl.RobotB, 
                                                                    Gateway.CTRLModule.getInstance().camCtrl.RobotC };
            //Arrays to save current position of Robot
            double[] Current_X = new double[RobotArray.Length];
            double[] Current_Y = new double[RobotArray.Length];
            double[] Current_Angle = new double[RobotArray.Length];
            //Update Current Image to get current Position of Robots
            Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
            if (Gateway.CTRLModule.getInstance().camCtrl.MyImage2 != null && Gateway.CTRLModule.getInstance().camCtrl.PlotLEDsAndUpdateStrings == true)
            {
                MULTIFORM_PCS.GUI.PCSMainWindow.getInstance().image1.Source = BitmapSourceConvert.ToBitmapSource(Gateway.CTRLModule.getInstance().camCtrl.MyImage2);
                Gateway.CTRLModule.getInstance().camCtrl.MyImage2.Dispose();
                Gateway.CTRLModule.getInstance().camCtrl.imOriginal.Dispose();
            }
            //get Current Robot1 Postion            
            Current_X[currentRobot[0]] = RobotArray[currentRobot[0]].Apex.X;
            Current_Y[currentRobot[0]] = RobotArray[currentRobot[0]].Apex.Y;
            Current_Angle[currentRobot[0]] = RobotArray[currentRobot[0]].Angle;
            //get Current Robot2 Position     
            Current_X[currentRobot[1]] = RobotArray[currentRobot[1]].Apex.X;
            Current_Y[currentRobot[1]] = RobotArray[currentRobot[1]].Apex.Y;
            Current_Angle[currentRobot[1]] = RobotArray[currentRobot[1]].Angle;
            //to scale angle from -pi to pi
            if (Current_Angle[currentRobot[0]] > 180) { Current_Angle[currentRobot[0]] = Current_Angle[currentRobot[0]] - 360; }
            else if (Current_Angle[currentRobot[0]] < -180) { Current_Angle[currentRobot[0]] = Current_Angle[currentRobot[0]] + 360; }
            if (Current_Angle[currentRobot[1]] > 180) { Current_Angle[currentRobot[1]] = Current_Angle[currentRobot[1]] - 360; }
            else if (Current_Angle[currentRobot[1]] < -180) { Current_Angle[currentRobot[1]] = Current_Angle[currentRobot[1]] + 360; }
            //Robot1 current states and final position send to python                    
            double x1Int = Current_X[currentRobot[0]] / 1000.0;
            double y1Int = Current_Y[currentRobot[0]]/1000.0;
            //Robot2 current states and final position send to python
            double x2Int = Current_X[currentRobot[1]] / 1000.0;
            double y2Int = Current_Y[currentRobot[1]]/1000.0;
            Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
            RobotArray = new RobotDiscription[] { Gateway.CTRLModule.getInstance().camCtrl.RobotA, Gateway.CTRLModule.getInstance().camCtrl.RobotB, 
                                                                    Gateway.CTRLModule.getInstance().camCtrl.RobotC }; 
            //delete Gaurd files if exist to execute C# and python parallel(should be created by python).Gaurd files are used to run c# or python to run synchronously
            // Flag to check python have started or not
            FileInfo startFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\pythonStarted.log");
            if (startFlag.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\pythonStarted.log"); }
            // Flag to check velocity is written or not
            FileInfo velFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\velocityWritten.log");
            if (velFlag.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\velocityWritten.log"); }
            //for setting orientation as sampling time will be changed
            FileInfo changeSamTime = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\changeSampleTime.log");
            if (changeSamTime.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\changeSampleTime.log"); }
            //Gaurd File pythonStarted.log is used to check when python will Exit.
            FileInfo pythonExit = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\pythonExit.log");
            if (pythonExit.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\pythonExit.log"); }
            FileInfo robot1moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\robot1moved.log");
            if (robot1moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\robot1moved.log"); }
            FileInfo robot2moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\robot2moved.log");
            if (robot2moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\robot2moved.log"); }

            if (MPCrunning == false)
            {
                MPCrunning = true;
                //this.threadInterrupt.Reset();
                this.MPCThread = new Thread(() =>
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                    //MPCThread.Join();
                    //Console.WriteLine(this.MPCThread);


                    









            // create new process (the python program) which will run parallel to c#
            Process p = new Process();
            p.StartInfo.FileName = "python.exe";
            p.StartInfo.Arguments = AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\mainFb.py " + x1Int + " " + y1Int +
                                    " " + Current_Angle[currentRobot[0]] + " " + robot1Final[0] + " " + robot1Final[1] + " " + x2Int +
                                    " " + y2Int + " " + Current_Angle[currentRobot[1]] + " " + robot2Final[0] + " " + robot2Final[1];

            //p.StartInfo.Arguments ="C:\\Users\\Pipelessplant\\Desktop\\Pipeless_AutoCalibration - 17.08\\PipelessPlant_1\\bin\\Debug\\pythonfiles\\python_2robots\\mainFb.py " + x1Int + " " + y1Int +
            //                " " + Current_Angle[currentRobot[0]] + " " + robot1Final[0] + " " + robot1Final[1] + " " + x2Int +
            //                " " + y2Int + " " + Current_Angle[currentRobot[1]] + " " + robot2Final[0] + " " + robot2Final[1];
            p.Start();
            //Gaurd File pythonStarted.log is used to check when python will start.
            startFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\pythonStarted.log");
            //waiting for Python to start
            while (!startFlag.Exists) { Thread.Sleep(10); startFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\pythonStarted.log"); }
            //initializing velocities of left and right wheels of the robot
            short left1 = 0; short right1 = 0; short left2 = 0; short right2 = 0;
            //file containing the  time step for which the robot should run and also mpc Total steps so that c# and python exit at same time
            StreamReader InitialValuesPython = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\IntStepSize.log", false);
            List<string> linesOfIntValue = new List<string>();
            List<string[]> IntValue = new List<string[]>();
            while (!InitialValuesPython.EndOfStream)
            {
                string line = InitialValuesPython.ReadLine();
                if (line != "")
                {
                    linesOfIntValue.Add(line);
                    IntValue.Add(line.Split('\t', ' '));
                }
            }
            InitialValuesPython.Close();
            int SleepTime = Convert.ToInt32(IntValue[0][0]);
            int TotalSteps = Convert.ToInt32(IntValue[0][1]);
            int odowait = 0;
            Encoder v=new Encoder();

            
            
            dist1Odometer = 0;
            dist2Odometer = 0;
            dist3Odometer = 0;
            angle1Odometer = 0;
            angle2Odometer = 0;
            angle3Odometer = 0;
            

            //main loop to transfer robots
            for (int i = 0; i < 300; i++)
            {   //For encoders
                changeSamTime = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\changeSampleTime.log");
                Console.WriteLine("dist1Odometer:"+ dist1Odometer);
                for (int robLoop = 0; robLoop < currentRobot.Length; robLoop++)
                {
                    if (currentRobot[robLoop] == 0)
                    {
                        //delta_theta[robLoop] = Convert.ToDouble(Math.PI / 180) * (angle1Odometer);
                        delta_theta[robLoop] = angle1Odometer;
                        delta_distance[robLoop] = dist1Odometer;
                    }
                    if (currentRobot[robLoop] == 1)
                    {
                        //delta_theta[robLoop] = Convert.ToDouble(Math.PI / 180) * angle2Odometer;
                        delta_theta[robLoop] = angle2Odometer;
                        delta_distance[robLoop] = dist2Odometer;

                    }
                    if (currentRobot[robLoop] == 2)
                    {
                        //delta_theta[robLoop] = Convert.ToDouble(Math.PI / 180) * angle3Odometer;
                        delta_theta[robLoop] = angle3Odometer;
                        delta_distance[robLoop] = dist3Odometer;

                    }
                }


                //Update GUI and Camera Position of robot
                Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
                RobotArray = new RobotDiscription[] { Gateway.CTRLModule.getInstance().camCtrl.RobotA, Gateway.CTRLModule.getInstance().camCtrl.RobotB, Gateway.CTRLModule.getInstance().camCtrl.RobotC };
                Controller[] ControllerArray = new Controller[] { Gateway.CTRLModule.getInstance().Controller_Robot1, Gateway.CTRLModule.getInstance().Controller_Robot2, Gateway.CTRLModule.getInstance().Controller_Robot3 };
                //int[] RobotAssingment = new int[] { 0, 1, 3 }; // assingment of LEDs to IDs
                int[,] Destinations = new[,] { { 3510, 400 }, { 600, 600 }, { 0, 600 } };
                int[] Orientations = new int[] { 180, 180, 285 };
                int[] DirectionControllerOutput = new int[RobotArray.Length];
                int[] Robot_Phase = new int[RobotArray.Length];
                if (Gateway.CTRLModule.getInstance().camCtrl.MyImage2 != null && Gateway.CTRLModule.getInstance().camCtrl.PlotLEDsAndUpdateStrings == true)
                {
                    MULTIFORM_PCS.GUI.PCSMainWindow.getInstance().image1.Source = BitmapSourceConvert.ToBitmapSource(Gateway.CTRLModule.getInstance().camCtrl.MyImage2);
                    Gateway.CTRLModule.getInstance().camCtrl.MyImage2.Dispose();
                    Gateway.CTRLModule.getInstance().camCtrl.imOriginal.Dispose();
                }
                //initializing velocity matrix to be given to all robots  
                double[][] velocity1 = new double[RobotArray.Length][];
                //Robot1 Cam Pos                   
                Current_X[currentRobot[0]] = RobotArray[currentRobot[0]].Apex.X;
                Current_Y[currentRobot[0]] = RobotArray[currentRobot[0]].Apex.Y;
                Current_Angle[currentRobot[0]] = RobotArray[currentRobot[0]].Angle;
                //Robot2 Cam Pos                    
                Current_X[currentRobot[1]] = RobotArray[currentRobot[1]].Apex.X;
                Current_Y[currentRobot[1]] = RobotArray[currentRobot[1]].Apex.Y;
                Current_Angle[currentRobot[1]] = RobotArray[currentRobot[1]].Angle;
                //to scale angle from -pi to pi
                if (Current_Angle[currentRobot[0]] > 180) { Current_Angle[currentRobot[0]] = Current_Angle[currentRobot[0]] - 360; }
                else if (Current_Angle[currentRobot[0]] < -180) { Current_Angle[currentRobot[0]] = Current_Angle[currentRobot[0]] + 360; }
                if (Current_Angle[currentRobot[1]] > 180) { Current_Angle[currentRobot[1]] = Current_Angle[currentRobot[1]] - 360; }
                else if (Current_Angle[currentRobot[1]] < -180) { Current_Angle[currentRobot[1]] = Current_Angle[currentRobot[1]] + 360; }
                //writing current position to posData.log file to be read by python                  
                StreamWriter posData = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\posData.log", false);
                posData.WriteLine(Current_X[currentRobot[0]] / 1000.0 + "\t" + Current_Y[currentRobot[0]] / 1000.0 + "\t" + Current_Angle[currentRobot[0]] + "\t" +
                                  Current_X[currentRobot[1]] / 1000.0 + "\t" + Current_Y[currentRobot[1]] / 1000.0 + "\t" + Current_Angle[currentRobot[1]]);
                posData.Close();
                //do not read current position 1st time
                if (i > 0)
                {
                    //file to know if robot have moved or not from Encoders
                    if (Math.Abs(delta_distance[0]) > 0 || Math.Abs(delta_theta[0]) > 0)
                    { StreamWriter Robot1move = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\robot1moved.log", false); Robot1move.Close(); }
                    if (Math.Abs(delta_distance[1]) > 0 || Math.Abs(delta_theta[1]) > 0)
                    { StreamWriter Robot2move = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\robot2moved.log", false); Robot2move.Close(); }
                    //Gaurd file to be written after current position is written python will wait for this file to be written
                    StreamWriter posFlag = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\posWritten.log", false);
                    posFlag.Close();
                }
                //to read left and left wheel velocities from python                    
                StreamReader velocityReader;
                List<string[]> velocitySeq = new List<string[]>();
                //gaurd file for the velocity to be written
                velFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\velocityWritten.log");
                //check if python has exits and exits loop after stopping robots
                pythonExit = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\pythonExit.log");
                if (pythonExit.Exists)
                {
                    velocity1[currentRobot[0]] = new double[] { 0, 0 };
                    Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[0]]).forward(velocity1[currentRobot[0]], 0, 0, 0);
                    velocity1[currentRobot[1]] = new double[] { 0, 0 };
                    Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[1]]).forward(velocity1[currentRobot[1]], 0, 0, 0);
                    break;
                }
                // for setting orientation at the target position new sampling time
                if (changeSamTime.Exists) { SleepTime = 50; }
                //wait for python to write velocities
                while (!velFlag.Exists) { Thread.Sleep(1); velFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\velocityWritten.log"); }
                for (int j = 0; j <= 50; j++)
                {
                    try
                    {
                        velocityReader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\wheel.log", false);
                        List<string> linesOfVelocity = new List<string>();
                        while (!velocityReader.EndOfStream)
                        {
                            string line = velocityReader.ReadLine();
                            if (line != "")
                            {
                                linesOfVelocity.Add(line);
                                velocitySeq.Add(line.Split('\t', ' '));
                            }
                        }
                        velocityReader.Close();
                        double vLeft1 = Convert.ToDouble(velocitySeq[0][0])*1000.0;
                        double vRight1 = Convert.ToDouble(velocitySeq[0][1]) * 1000.0;
                        double vLeft2 = Convert.ToDouble(velocitySeq[0][2]) * 1000.0;
                        double vRight2 = Convert.ToDouble(velocitySeq[0][3]) * 1000.0;
                        left1 = Convert.ToInt16(vLeft1);
                        right1 = Convert.ToInt16(vRight1);
                        left2 = Convert.ToInt16(vLeft2);
                        right2 = Convert.ToInt16(vRight2);
                        Console.WriteLine("read");
                        break;
                    }
                    catch (IOException)
                    {
                        left1 = 0;
                        right1 = 0;
                        left2 = 0;
                        right2 = 0;
                    }
                }
                velocity1[currentRobot[0]] = new double[] { left1, right1 };
                velocity1[currentRobot[1]] = new double[] { left2, right2 };
                Console.WriteLine(right1 + "  " + left1);
                Console.WriteLine(right2 + "  " + left2);
                //only give velocities from controller after 3rd iteration
                if (i > 2)
                {
                    Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[0]]).forward(velocity1[currentRobot[0]], 0, 0, 0);
                    Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[1]]).forward(velocity1[currentRobot[1]], 0, 0, 0);
                }
                // check if total number of loops in python hae reached then stop the robots and exit 
                if (i == TotalSteps)
                {
                    velocity1[currentRobot[0]] = new double[] { 0, 0 };
                    Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[0]]).forward(velocity1[currentRobot[0]], 0, 0, 0);
                    velocity1[currentRobot[1]] = new double[] { 0, 0 };
                    Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[1]]).forward(velocity1[currentRobot[1]], 0, 0, 0);
                    break;
                }
                //Time to remove values from Encoders(150 ms delay from router to recieve packets)
                if (SleepTime > 150 & odowait == 0)
                {
                    //Thread.Sleep(100);
                    //SleepTime = SleepTime - 100;
                    odowait = 1;
                }
                //Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI(); 
                //if (Gateway.CTRLModule.getInstance().camCtrl.MyImage2 != null && Gateway.CTRLModule.getInstance().camCtrl.PlotLEDsAndUpdateStrings == true)
                //{
                //    image1.Source = BitmapSourceConvert.ToBitmapSource(Gateway.CTRLModule.getInstance().camCtrl.MyImage2);
                //    Gateway.CTRLModule.getInstance().camCtrl.MyImage2.Dispose();
                //    Gateway.CTRLModule.getInstance().camCtrl.imOriginal.Dispose();
                //} 
                //reseting values for the next measurement
                dist1Odometer = 0;
                dist2Odometer = 0;
                dist3Odometer = 0;
                angle1Odometer = 0;
                angle2Odometer = 0;
                angle3Odometer = 0;
                robot1moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\robot1moved.log");
                if (robot1moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\robot1moved.log"); }
                robot2moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\robot2moved.log");
                if (robot2moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\robot2moved.log"); }
                //sampling time for current velocities so that robot will run till sampling time
                //Thread.Sleep(SleepTime);
                Console.WriteLine(SleepTime);
                //reinintializing velocity seq array so that new velocities can be written
                velocitySeq = new List<string[]>();
                //try to delete  velocity gaurd file so that in next loop wait for new velocities to be written
                for (int l = 0; l <= 100; l++)
                {
                    try
                    {
                        if (velFlag.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\velocityWritten.log"); }
                        velFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\velocityWritten.log"); break;
                    }
                    catch (IOException) { Thread.Sleep(10); }
                }
            }
            //deleting temporary gaurd files 
            pythonExit = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\pythonExit.log");
            if (pythonExit.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\pythonExit.log"); }
            startFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\pythonStarted.log");
            if (startFlag.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\pythonStarted.log"); }
            p.WaitForExit();
            //docking if required
            if (currentRobot[0] == 2) { currentRobot[0] = 3; }
            else if (currentRobot[1] == 2) { currentRobot[1] = 3; }
            if (dock[0] == true) { Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(currentRobot[0]).dock(); }
            if (dock[1] == true) { Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(currentRobot[1]).dock(); }
            changeSamTime = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\changeSampleTime.log");
            if (changeSamTime.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\changeSampleTime.log"); }
            robot1moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\robot1moved.log");
            if (robot1moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\robot1moved.log"); }
            robot2moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\robot2moved.log");
            if (robot2moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_2robots\\robot2moved.log"); }


            Thread.Sleep(100);
            MPCrunning = false;
            //threadInterrupt.Set();
            this.MPCThread = null;
                });
                this.MPCThread.Start();
                //MPCThread.Join();
                //MPCrunning = false;
            }


        }


        /// <summary>
        /// Function to run 3 robots
        /// </summary>
        /// <param name="currentRobot"></param>
        /// <param name="robot1Final"></param>
        /// <param name="robot2Final"></param>
        /// <param name="robot3Final"></param>
        /// <param name="dock"></param>
        public void threeRobots(int[] currentRobot, double[] robot1Final, double[] robot2Final,double[] robot3Final, bool[] dock)
        {
            int[] RobotAssingment = new int[] { 0, 1, 3 }; // assingment of LEDs to IDs
            double[] delta_distance = new double[] { 0, 0, 0 };
            double[] delta_theta = new double[] { 0, 0, 0 };
            Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
            RobotDiscription[] RobotArray = new RobotDiscription[] { Gateway.CTRLModule.getInstance().camCtrl.RobotA, Gateway.CTRLModule.getInstance().camCtrl.RobotB, 
                                                                    Gateway.CTRLModule.getInstance().camCtrl.RobotC };
            //Arrays to save current position of Robot
            double[] Current_X = new double[RobotArray.Length];
            double[] Current_Y = new double[RobotArray.Length];
            double[] Current_Angle = new double[RobotArray.Length];
            //Update Current Image to get current Position of Robots
            Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
            if (Gateway.CTRLModule.getInstance().camCtrl.MyImage2 != null && Gateway.CTRLModule.getInstance().camCtrl.PlotLEDsAndUpdateStrings == true)
            {
                MULTIFORM_PCS.GUI.PCSMainWindow.getInstance().image1.Source = BitmapSourceConvert.ToBitmapSource(Gateway.CTRLModule.getInstance().camCtrl.MyImage2);
                Gateway.CTRLModule.getInstance().camCtrl.MyImage2.Dispose();
                Gateway.CTRLModule.getInstance().camCtrl.imOriginal.Dispose();
            }
            //get Current Robot1 Postion            
            Current_X[currentRobot[0]] = RobotArray[currentRobot[0]].Apex.X;
            Current_Y[currentRobot[0]] = RobotArray[currentRobot[0]].Apex.Y;
            Current_Angle[currentRobot[0]] = RobotArray[currentRobot[0]].Angle;
            //get Current Robot2 Position     
            Current_X[currentRobot[1]] = RobotArray[currentRobot[1]].Apex.X;
            Current_Y[currentRobot[1]] = RobotArray[currentRobot[1]].Apex.Y;
            Current_Angle[currentRobot[1]] = RobotArray[currentRobot[1]].Angle;
            //get Current Robot3 Position     
            Current_X[currentRobot[2]] = RobotArray[currentRobot[2]].Apex.X;
            Current_Y[currentRobot[2]] = RobotArray[currentRobot[2]].Apex.Y;
            Current_Angle[currentRobot[2]] = RobotArray[currentRobot[2]].Angle;
            //to scale angle from -pi to pi
            if (Current_Angle[currentRobot[0]] > 180) { Current_Angle[currentRobot[0]] = Current_Angle[currentRobot[0]] - 360; }
            else if (Current_Angle[currentRobot[0]] < -180) { Current_Angle[currentRobot[0]] = Current_Angle[currentRobot[0]] + 360; }
            if (Current_Angle[currentRobot[1]] > 180) { Current_Angle[currentRobot[1]] = Current_Angle[currentRobot[1]] - 360; }
            else if (Current_Angle[currentRobot[1]] < -180) { Current_Angle[currentRobot[1]] = Current_Angle[currentRobot[1]] + 360; }
            if (Current_Angle[currentRobot[2]] > 180) { Current_Angle[currentRobot[2]] = Current_Angle[currentRobot[2]] - 360; }
            else if (Current_Angle[currentRobot[2]] < -180) { Current_Angle[currentRobot[2]] = Current_Angle[currentRobot[2]] + 360; }
            //Robot1 current states and final position send to python                    
            double x1Int = Current_X[currentRobot[0]]/1000.0;
            double y1Int = Current_Y[currentRobot[0]] / 1000.0;
            //Robot2 current states and final position send to python
            double x2Int = Current_X[currentRobot[1]] / 1000.0;
            double y2Int = Current_Y[currentRobot[1]] / 1000.0;
            //Robot2 current states and final position send to python
            double x3Int = Current_X[currentRobot[2]]/1000.0;
            double y3Int = Current_Y[currentRobot[2]] / 1000.0;
            Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
            RobotArray = new RobotDiscription[] { Gateway.CTRLModule.getInstance().camCtrl.RobotA, Gateway.CTRLModule.getInstance().camCtrl.RobotB, 
                                                                    Gateway.CTRLModule.getInstance().camCtrl.RobotC };
            //delete Gaurd files if exist to execute C# and python parallel(should be created by python).Gaurd files are used to run c# or python to run synchronously
            // Flag to check python have started or not
            FileInfo startFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\pythonStarted.log");
            if (startFlag.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\pythonStarted.log"); }
            // Flag to check velocity is written or not
            FileInfo velFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\velocityWritten.log");
            if (velFlag.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\velocityWritten.log"); }
            //for setting orientation as sampling time will be changed
            FileInfo changeSamTime = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\changeSampleTime.log");
            if (changeSamTime.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\changeSampleTime.log"); }
            //Gaurd File pythonStarted.log is used to check when python will Exit.
            FileInfo pythonExit = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\pythonExit.log");
            if (pythonExit.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\pythonExit.log"); }
            FileInfo robot1moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\robot1moved.log");
            if (robot1moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\robot1moved.log"); }
            FileInfo robot2moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\robot2moved.log");
            if (robot2moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\robot2moved.log"); }

            if (MPCrunning == false)
            {
                MPCrunning = true;
                //this.threadInterrupt.Reset();
                this.MPCThread = new Thread(() =>
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                    //MPCThread.Join();
                    //Console.WriteLine(this.MPCThread);












                    // create new process (the python program) which will run parallel to c#
                    Process p = new Process();
                    p.StartInfo.FileName = "python.exe";
                    p.StartInfo.Arguments = AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\mainFb.py " + x1Int + " " + y1Int +
                                            " " + Current_Angle[currentRobot[0]] + " " + robot1Final[0] + " " + robot1Final[1] + " " + x2Int +
                                            " " + y2Int + " " + Current_Angle[currentRobot[1]] + " " + robot2Final[0] + " " + robot2Final[1] + " " + x3Int +
                                            " " + y3Int + " " + Current_Angle[currentRobot[2]] + " " + robot3Final[0] + " " + robot3Final[1];
                    p.Start();
                    //Gaurd File pythonStarted.log is used to check when python will start.
                    startFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\pythonStarted.log");
                    //waiting for Python to start
                    while (!startFlag.Exists) { Thread.Sleep(10); startFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\pythonStarted.log"); }
                    //initializing velocities of left and right wheels of the robot
                    short left1 = 0; short right1 = 0; short left2 = 0; short right2 = 0; short left3 = 0; short right3 = 0;
                    //file containing the  time step for which the robot should run and also mpc Total steps so that c# and python exit at same time
                    StreamReader InitialValuesPython = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\IntStepSize.log", false);
                    List<string> linesOfIntValue = new List<string>();
                    List<string[]> IntValue = new List<string[]>();
                    while (!InitialValuesPython.EndOfStream)
                    {
                        string line = InitialValuesPython.ReadLine();
                        if (line != "")
                        {
                            linesOfIntValue.Add(line);
                            IntValue.Add(line.Split('\t', ' '));
                        }
                    }
                    InitialValuesPython.Close();
                    int SleepTime = Convert.ToInt32(IntValue[0][0]);
                    int TotalSteps = Convert.ToInt32(IntValue[0][1]);
                    int odowait = 0;
                    Encoder v = new Encoder();



                    dist1Odometer = 0;
                    dist2Odometer = 0;
                    dist3Odometer = 0;
                    angle1Odometer = 0;
                    angle2Odometer = 0;
                    angle3Odometer = 0;


                    //main loop to transfer robots
                    for (int i = 0; i < 500; i++)
                    {   //For encoders
                        changeSamTime = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\changeSampleTime.log");
                        Console.WriteLine("dist1Odometer:" + dist1Odometer);
                        for (int robLoop = 0; robLoop < currentRobot.Length; robLoop++)
                        {
                            if (currentRobot[robLoop] == 0)
                            {
                                //delta_theta[robLoop] = Convert.ToDouble(Math.PI / 180) * (angle1Odometer);
                                delta_theta[robLoop] = angle1Odometer;
                                delta_distance[robLoop] = dist1Odometer;
                            }
                            if (currentRobot[robLoop] == 1)
                            {
                                //delta_theta[robLoop] = Convert.ToDouble(Math.PI / 180) * angle2Odometer;
                                delta_theta[robLoop] = angle2Odometer;
                                delta_distance[robLoop] = dist2Odometer;

                            }
                            if (currentRobot[robLoop] == 2)
                            {
                                //delta_theta[robLoop] = Convert.ToDouble(Math.PI / 180) * angle3Odometer;
                                delta_theta[robLoop] = angle3Odometer;
                                delta_distance[robLoop] = dist3Odometer;

                            }
                        }


                        //Update GUI and Camera Position of robot
                        Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
                        RobotArray = new RobotDiscription[] { Gateway.CTRLModule.getInstance().camCtrl.RobotA, Gateway.CTRLModule.getInstance().camCtrl.RobotB, Gateway.CTRLModule.getInstance().camCtrl.RobotC };
                        Controller[] ControllerArray = new Controller[] { Gateway.CTRLModule.getInstance().Controller_Robot1, Gateway.CTRLModule.getInstance().Controller_Robot2, Gateway.CTRLModule.getInstance().Controller_Robot3 };
                        //int[] RobotAssingment = new int[] { 0, 1, 3 }; // assingment of LEDs to IDs
                        int[,] Destinations = new[,] { { 3510, 400 }, { 600, 600 }, { 0, 600 } };
                        int[] Orientations = new int[] { 180, 180, 285 };
                        int[] DirectionControllerOutput = new int[RobotArray.Length];
                        int[] Robot_Phase = new int[RobotArray.Length];
                        if (Gateway.CTRLModule.getInstance().camCtrl.MyImage2 != null && Gateway.CTRLModule.getInstance().camCtrl.PlotLEDsAndUpdateStrings == true)
                        {
                            MULTIFORM_PCS.GUI.PCSMainWindow.getInstance().image1.Source = BitmapSourceConvert.ToBitmapSource(Gateway.CTRLModule.getInstance().camCtrl.MyImage2);
                            Gateway.CTRLModule.getInstance().camCtrl.MyImage2.Dispose();
                            Gateway.CTRLModule.getInstance().camCtrl.imOriginal.Dispose();
                        }
                        //initializing velocity matrix to be given to all robots  
                        double[][] velocity1 = new double[RobotArray.Length][];
                        //Robot1 Cam Pos                   
                        Current_X[currentRobot[0]] = RobotArray[currentRobot[0]].Apex.X;
                        Current_Y[currentRobot[0]] = RobotArray[currentRobot[0]].Apex.Y;
                        Current_Angle[currentRobot[0]] = RobotArray[currentRobot[0]].Angle;
                        //Robot2 Cam Pos                    
                        Current_X[currentRobot[1]] = RobotArray[currentRobot[1]].Apex.X;
                        Current_Y[currentRobot[1]] = RobotArray[currentRobot[1]].Apex.Y;
                        Current_Angle[currentRobot[1]] = RobotArray[currentRobot[1]].Angle;
                        //Robot3 Cam Pos                    
                        Current_X[currentRobot[2]] = RobotArray[currentRobot[2]].Apex.X;
                        Current_Y[currentRobot[2]] = RobotArray[currentRobot[2]].Apex.Y;
                        Current_Angle[currentRobot[2]] = RobotArray[currentRobot[2]].Angle;
                        //to scale angle from -pi to pi
                        if (Current_Angle[currentRobot[0]] > 180) { Current_Angle[currentRobot[0]] = Current_Angle[currentRobot[0]] - 360; }
                        else if (Current_Angle[currentRobot[0]] < -180) { Current_Angle[currentRobot[0]] = Current_Angle[currentRobot[0]] + 360; }
                        if (Current_Angle[currentRobot[1]] > 180) { Current_Angle[currentRobot[1]] = Current_Angle[currentRobot[1]] - 360; }
                        else if (Current_Angle[currentRobot[1]] < -180) { Current_Angle[currentRobot[1]] = Current_Angle[currentRobot[1]] + 360; }
                        if (Current_Angle[currentRobot[2]] > 180) { Current_Angle[currentRobot[2]] = Current_Angle[currentRobot[2]] - 360; }
                        else if (Current_Angle[currentRobot[2]] < -180) { Current_Angle[currentRobot[2]] = Current_Angle[currentRobot[2]] + 360; }
                        //writing current position to posData.log file to be read by python                  
                        StreamWriter posData = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\posData.log", false);
                        posData.WriteLine(Current_X[currentRobot[0]] / 1000.0 + "\t" + Current_Y[currentRobot[0]] / 1000.0 + "\t" + Current_Angle[currentRobot[0]] + "\t" +
                                          Current_X[currentRobot[1]] / 1000.0 + "\t" + Current_Y[currentRobot[1]] / 1000.0 + "\t" + Current_Angle[currentRobot[1]]
                                          + "\t" + Current_X[currentRobot[2]] / 1000.0 + "\t" + Current_Y[currentRobot[2]] / 1000.0 + "\t" + Current_Angle[currentRobot[2]]);
                        posData.Close();
                        //do not read current position 1st time
                        if (i > 0)
                        {
                            //file to know if robot have moved or not from Encoders
                            if (Math.Abs(delta_distance[0]) > 0 || Math.Abs(delta_theta[0]) > 0)
                            { StreamWriter Robot1move = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\robot1moved.log", false); Robot1move.Close(); }
                            if (Math.Abs(delta_distance[1]) > 0 || Math.Abs(delta_theta[1]) > 0)
                            { StreamWriter Robot2move = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\robot2moved.log", false); Robot2move.Close(); }
                            if (Math.Abs(delta_distance[2]) > 0 || Math.Abs(delta_theta[2]) > 0)
                            { StreamWriter Robot2move = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\robot3moved.log", false); Robot2move.Close(); }
                            //Gaurd file to be written after current position is written python will wait for this file to be written
                            StreamWriter posFlag = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\posWritten.log", false);
                            posFlag.Close();
                        }
                        //to read left and left wheel velocities from python                    
                        StreamReader velocityReader;
                        List<string[]> velocitySeq = new List<string[]>();
                        //gaurd file for the velocity to be written
                        velFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\velocityWritten.log");
                        //check if python has exits and exits loop after stopping robots
                        pythonExit = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\pythonExit.log");
                        if (pythonExit.Exists)
                        {
                            velocity1[currentRobot[0]] = new double[] { 0, 0 };
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[0]]).forward(velocity1[currentRobot[0]], 0, 0, 0);
                            velocity1[currentRobot[1]] = new double[] { 0, 0 };
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[1]]).forward(velocity1[currentRobot[1]], 0, 0, 0);
                            velocity1[currentRobot[2]] = new double[] { 0, 0 };
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[2]]).forward(velocity1[currentRobot[2]], 0, 0, 0);
                            break;
                        }
                        // for setting orientation at the target position new sampling time
                        if (changeSamTime.Exists) { SleepTime = 50; }
                        //wait for python to write velocities
                        while (!velFlag.Exists) { Thread.Sleep(1); velFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\velocityWritten.log"); }
                        for (int j = 0; j <= 50; j++)
                        {
                            try
                            {
                                velocityReader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\wheel.log", false);
                                List<string> linesOfVelocity = new List<string>();
                                while (!velocityReader.EndOfStream)
                                {
                                    string line = velocityReader.ReadLine();
                                    if (line != "")
                                    {
                                        linesOfVelocity.Add(line);
                                        velocitySeq.Add(line.Split('\t', ' '));
                                    }
                                }
                                velocityReader.Close();
                                double vLeft1 = Convert.ToDouble(velocitySeq[0][0])*1000;
                                double vRight1 = Convert.ToDouble(velocitySeq[0][1])*1000;
                                double vLeft2 = Convert.ToDouble(velocitySeq[0][2])*1000;
                                double vRight2 = Convert.ToDouble(velocitySeq[0][3])*1000;
                                double vLeft3 = Convert.ToDouble(velocitySeq[0][4])*1000;
                                double vRight3 = Convert.ToDouble(velocitySeq[0][5])*1000;
                                left1 = Convert.ToInt16(vLeft1);
                                right1 = Convert.ToInt16(vRight1);
                                left2 = Convert.ToInt16(vLeft2);
                                right2 = Convert.ToInt16(vRight2);
                                left3 = Convert.ToInt16(vLeft3);
                                right3 = Convert.ToInt16(vRight3);
                                Console.WriteLine("read");
                                break;
                            }
                            catch (IOException)
                            {  if (j==50){
                            }
                                left1 = 0;
                                right1 = 0;
                                left2 = 0;
                                right2 = 0;
                                left3 = 0;
                                right3 = 0;
                                if (j == 50)
                                {
                                    velocity1[currentRobot[0]] = new double[] { 0, 0 };
                                    Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[0]]).forward(velocity1[currentRobot[0]], 0, 0, 0);
                                    velocity1[currentRobot[1]] = new double[] { 0, 0 };
                                    Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[1]]).forward(velocity1[currentRobot[1]], 0, 0, 0);
                                    velocity1[currentRobot[2]] = new double[] { 0, 0 };
                                    Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[2]]).forward(velocity1[currentRobot[2]], 0, 0, 0);
                                    break;

                                }
                            }
                        }
                        velocity1[currentRobot[0]] = new double[] { left1, right1 };
                        velocity1[currentRobot[1]] = new double[] { left2, right2 };
                        velocity1[currentRobot[2]] = new double[] { left3, right3 };
                        Console.WriteLine(right1 + "  " + left1);
                        Console.WriteLine(right2 + "  " + left2);
                        Console.WriteLine(right3 + "  " + left3);
                        //only give velocities from controller after 3rd iteration
                        if (i > 2)
                        {
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[0]]).forward(velocity1[currentRobot[0]], 0, 0, 0);
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[1]]).forward(velocity1[currentRobot[1]], 0, 0, 0);
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[2]]).forward(velocity1[currentRobot[2]], 0, 0, 0);
                        }
                        // check if total number of loops in python hae reached then stop the robots and exit 
                        if (i == TotalSteps)
                        {
                            velocity1[currentRobot[0]] = new double[] { 0, 0 };
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[0]]).forward(velocity1[currentRobot[0]], 0, 0, 0);
                            velocity1[currentRobot[1]] = new double[] { 0, 0 };
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[1]]).forward(velocity1[currentRobot[1]], 0, 0, 0);
                            velocity1[currentRobot[2]] = new double[] { 0, 0 };
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[currentRobot[2]]).forward(velocity1[currentRobot[2]], 0, 0, 0);
                            break;
                        }
                        //Time to remove values from Encoders(150 ms delay from router to recieve packets)
                        //if (SleepTime > 150 & odowait == 0)
                        //{
                        //    Thread.Sleep(100);
                        //    SleepTime = SleepTime - 100;
                        //    odowait = 1;
                        //}
                        //Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI(); 
                        //if (Gateway.CTRLModule.getInstance().camCtrl.MyImage2 != null && Gateway.CTRLModule.getInstance().camCtrl.PlotLEDsAndUpdateStrings == true)
                        //{
                        //    image1.Source = BitmapSourceConvert.ToBitmapSource(Gateway.CTRLModule.getInstance().camCtrl.MyImage2);
                        //    Gateway.CTRLModule.getInstance().camCtrl.MyImage2.Dispose();
                        //    Gateway.CTRLModule.getInstance().camCtrl.imOriginal.Dispose();
                        //} 
                        //reseting values for the next measurement
                        dist1Odometer = 0;
                        dist2Odometer = 0;
                        dist3Odometer = 0;
                        angle1Odometer = 0;
                        angle2Odometer = 0;
                        angle3Odometer = 0;
                        robot1moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\robot1moved.log");
                        if (robot1moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\robot1moved.log"); }
                        robot2moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\robot2moved.log");
                        if (robot2moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\robot2moved.log"); }
                        //sampling time for current velocities so that robot will run till sampling time
                        //Thread.Sleep(SleepTime);
                       // Thread.Sleep(50);
                        Console.WriteLine(SleepTime);
                        //reinintializing velocity seq array so that new velocities can be written
                        velocitySeq = new List<string[]>();
                        //try to delete  velocity gaurd file so that in next loop wait for new velocities to be written
                        for (int l = 0; l <= 100; l++)
                        {
                            try
                            {
                                if (velFlag.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\velocityWritten.log"); }
                                velFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\velocityWritten.log"); break;
                            }
                            catch (IOException) { Thread.Sleep(10); }
                        }
                    }
                    //deleting temporary gaurd files 
                    pythonExit = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\pythonExit.log");
                    if (pythonExit.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\pythonExit.log"); }
                    startFlag = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\pythonStarted.log");
                    if (startFlag.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\pythonStarted.log"); }
                    p.WaitForExit();
                    //docking if required
                    if (currentRobot[0] == 2) { currentRobot[0] = 3; }
                    else if (currentRobot[1] == 2) { currentRobot[1] = 3; }
                    else if (currentRobot[2] == 2) { currentRobot[2] = 3; }
                    if (dock[0] == true) { Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(currentRobot[0]).dock(); }
                    if (dock[1] == true) { Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(currentRobot[1]).dock(); }
                    if (dock[2] == true) { Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(currentRobot[2]).dock(); }
                    changeSamTime = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\changeSampleTime.log");
                    if (changeSamTime.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\changeSampleTime.log"); }
                    robot1moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\robot1moved.log");
                    if (robot1moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\robot1moved.log"); }
                    robot2moved = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\robot2moved.log");
                    if (robot2moved.Exists) { File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\pythonfiles\\python_3robots\\robot2moved.log"); }


                    Thread.Sleep(100);
                    MPCrunning = false;
                    //threadInterrupt.Set();
                    this.MPCThread = null;
                });
                this.MPCThread.Start();
                //MPCThread.Join();
                //MPCrunning = false;
            }


        }
        /// <summary>
        /// read the agv_info.log file to run the robots according to schedule
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public int[] getinfo(int group)
        {
            StreamReader agvInfo = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "agv_info.log", false);
            List<string> scheduleList = new List<string>();
            string[] lines;
            string[] scheduleArray;
            string line;
            int movementGroups = 1;
            int oldtime = 0;
            int maxTime = 0;
            while ((line = agvInfo.ReadLine()) != null) { scheduleList.Add(line); }
            agvInfo.Close();
            lines = scheduleList.ToArray();
            Tuple<int, string, int, int, int[], int[]>[] sTasks = new Tuple<int, string, int, int, int[], int[]>[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                scheduleArray = lines[i].Split('\t', ' ');
                int taskID = Convert.ToInt32(scheduleArray[0]) / 10000;
                string robotID = scheduleArray[1] + scheduleArray[2];
                int intTime = Convert.ToInt32(scheduleArray[3]);
                int endTime = Convert.ToInt32(scheduleArray[4]);
                int[] startPos = returnPosition(scheduleArray[5]);
                int[] endPos = returnPosition(scheduleArray[6]);
                sTasks[i] = new Tuple<int, string, int, int, int[], int[]>(taskID, robotID, intTime, endTime, startPos, endPos);
                if (sTasks[i].Item4 > maxTime) { maxTime = sTasks[i].Item4; }
                if (oldtime < sTasks[i].Item3) { movementGroups++; }
                oldtime = sTasks[i].Item3;
                //Console.WriteLine(sTasks[i]);               
            }
            Console.WriteLine("Maximum Time:" + maxTime);
            int currentTasks = 0;
            int currentLine = 0;
            int startTime = sTasks[currentTasks].Item3;
            int[,] tasksInGroup = new int[movementGroups, 2];
            //Console.WriteLine(tasksInGroup.Length);
            for (int i = 0; i < movementGroups; i++)
            {
                startTime = sTasks[currentLine].Item3;
                while (startTime == sTasks[currentLine].Item3)
                {
                    currentLine++;
                    currentTasks++;
                    if (currentLine == sTasks.Length) { break; }
                }

                tasksInGroup[i, 0] = currentTasks;
                tasksInGroup[i, 1] = currentLine - currentTasks;
                currentTasks = 0;
            }
            int[] values = { movementGroups, tasksInGroup[group, 0], tasksInGroup[group, 1] };
            return values;
        }

        public int[] getGroupOneRobot(int startLine)
        {
            StreamReader agvInfo = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "agv_info.log", false);
            int[] initialPosition = new int[2];
            int[] finalPosition = new int[2];
            int robotid1;
            List<string> scheduleList = new List<string>();
            string[] lines;
            string[] scheduleArray;
            string line;
            while ((line = agvInfo.ReadLine()) != null) { scheduleList.Add(line); }
            agvInfo.Close();
            lines = scheduleList.ToArray();
            Tuple<int, string, int, int, int[], int[]> sTasks;
            scheduleArray = lines[startLine].Split('\t', ' ');
            int taskID = Convert.ToInt32(scheduleArray[0]) / 10000;
            string robot = scheduleArray[1] + scheduleArray[2];
            int intTime = Convert.ToInt32(scheduleArray[3]);
            int endTime = Convert.ToInt32(scheduleArray[4]);
            int[] startPos = returnPosition(scheduleArray[5]);
            int[] endPos = returnPosition(scheduleArray[6]);
            sTasks = new Tuple<int, string, int, int, int[], int[]>(taskID, robot, intTime, endTime, startPos, endPos);
            robotid1 = robotID(sTasks.Item2);
            initialPosition = sTasks.Item5;
            finalPosition = sTasks.Item6;
            int[] values = { robotid1, initialPosition[0], initialPosition[1], finalPosition[0], finalPosition[1] };
            return values;
        }

        public int[][] getGroupTwoRobot(int startLine)
        {
            StreamReader agvInfo = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "agv_info.log", false);
            int[][] initialPosition = new int[2][];
            int[][] finalPosition = new int[2][];
            int[] robotid = new int[2];
            List<string> scheduleList = new List<string>();
            string[] lines;
            string[] scheduleArray;
            string line;
            while ((line = agvInfo.ReadLine()) != null) { scheduleList.Add(line); }
            agvInfo.Close();
            lines = scheduleList.ToArray();
            Tuple<int, string, int, int, int[], int[]>[] sTasks = new Tuple<int, string, int, int, int[], int[]>[2];

            scheduleArray = lines[startLine].Split('\t', ' ');
            int taskID = Convert.ToInt32(scheduleArray[0]) / 10000;
            string robot = scheduleArray[1] + scheduleArray[2];
            int intTime = Convert.ToInt32(scheduleArray[3]);
            int endTime = Convert.ToInt32(scheduleArray[4]);
            int[] startPos = returnPosition(scheduleArray[5]);
            int[] endPos = returnPosition(scheduleArray[6]);
            sTasks[0] = new Tuple<int, string, int, int, int[], int[]>(taskID, robot, intTime, endTime, startPos, endPos);
            robotid[0] = robotID(sTasks[0].Item2);
            initialPosition[0] = sTasks[0].Item5;
            finalPosition[0] = sTasks[0].Item6;
            scheduleArray = lines[startLine + 1].Split('\t', ' ');
            taskID = Convert.ToInt32(scheduleArray[0]) / 10000;
            robot = scheduleArray[1] + scheduleArray[2];
            intTime = Convert.ToInt32(scheduleArray[3]);
            endTime = Convert.ToInt32(scheduleArray[4]);
            startPos = returnPosition(scheduleArray[5]);
            endPos = returnPosition(scheduleArray[6]);
            sTasks[1] = new Tuple<int, string, int, int, int[], int[]>(taskID, robot, intTime, endTime, startPos, endPos);
            robotid[1] = robotID(sTasks[1].Item2);
            initialPosition[1] = sTasks[1].Item5;
            finalPosition[1] = sTasks[1].Item6;
            int[][] values = { robotid, initialPosition[0], initialPosition[1], finalPosition[0], finalPosition[1] };
            return values;
        }

        public int[][] getGroupThreeRobot(int startLine)
        {
            StreamReader agvInfo = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "agv_info.log", false);
            int[][] initialPosition = new int[3][];
            int[][] finalPosition = new int[3][];
            int[] robotid = new int[3];
            List<string> scheduleList = new List<string>();
            string[] lines;
            string[] scheduleArray;
            string line;
            while ((line = agvInfo.ReadLine()) != null) { scheduleList.Add(line); }
            agvInfo.Close();
            lines = scheduleList.ToArray();
            Tuple<int, string, int, int, int[], int[]>[] sTasks = new Tuple<int, string, int, int, int[], int[]>[3];
            scheduleArray = lines[startLine].Split('\t', ' ');
            int taskID = Convert.ToInt32(scheduleArray[0]) / 10000;
            string robot = scheduleArray[1] + scheduleArray[2];
            int intTime = Convert.ToInt32(scheduleArray[3]);
            int endTime = Convert.ToInt32(scheduleArray[4]);
            int[] startPos = returnPosition(scheduleArray[5]);
            int[] endPos = returnPosition(scheduleArray[6]);
            sTasks[0] = new Tuple<int, string, int, int, int[], int[]>(taskID, robot, intTime, endTime, startPos, endPos);
            robotid[0] = robotID(sTasks[0].Item2);
            initialPosition[0] = sTasks[0].Item5;
            finalPosition[0] = sTasks[0].Item6;
            scheduleArray = lines[startLine + 1].Split('\t', ' ');
            taskID = Convert.ToInt32(scheduleArray[0]) / 10000;
            robot = scheduleArray[1] + scheduleArray[2];
            intTime = Convert.ToInt32(scheduleArray[3]);
            endTime = Convert.ToInt32(scheduleArray[4]);
            startPos = returnPosition(scheduleArray[5]);
            endPos = returnPosition(scheduleArray[6]);
            sTasks[1] = new Tuple<int, string, int, int, int[], int[]>(taskID, robot, intTime, endTime, startPos, endPos);
            robotid[1] = robotID(sTasks[1].Item2);
            initialPosition[1] = sTasks[1].Item5;
            finalPosition[1] = sTasks[1].Item6;
            scheduleArray = lines[startLine + 2].Split('\t', ' ');
            taskID = Convert.ToInt32(scheduleArray[0]) / 10000;
            robot = scheduleArray[1] + scheduleArray[2];
            intTime = Convert.ToInt32(scheduleArray[3]);
            endTime = Convert.ToInt32(scheduleArray[4]);
            startPos = returnPosition(scheduleArray[5]);
            endPos = returnPosition(scheduleArray[6]);
            sTasks[2] = new Tuple<int, string, int, int, int[], int[]>(taskID, robot, intTime, endTime, startPos, endPos);
            robotid[2] = robotID(sTasks[2].Item2);
            initialPosition[2] = sTasks[2].Item5;
            finalPosition[2] = sTasks[2].Item6;
            int[][] values = { robotid, initialPosition[0], initialPosition[1], initialPosition[2], finalPosition[0], finalPosition[1], finalPosition[2] };
            return values;
        }
        /// <summary>
        /// return robot IDs
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>

        public int robotID(string name)
        {
            switch (name)
            {
                case ("AGV1"): { return 0; }
                case ("AGV2"): { return 1; }
                case ("AGV4"): { return 2; }
                case ("AGV3"): { return 2; }
                default:
                    {
                        Console.WriteLine("robot not recoginized");
                        return 4;
                    }
            }

        }
        public int[] returnPosition(string station)
        {
            int[] storagePosition = { 2100, 2100 };
            int[] fill1Position = { 1100, 1520 };
            int[] fill2Position = { 3000, 1520 };
            int[] mixingPosition = { 2100, 900 };
            int[] agv1IntPosition = { 3200, 700 };
            int[] agv2IntPosition = { 2750, 700 };
            int[] agv4IntPosition = { 1100, 2100 };
            int[] agv3IntPosition = { 1100, 2100 };
            int[] charging1Position = { 720, 2300 };
            int[] charging2Position = { 3280, 2300 };
            int[] defaultPos = { 0, 0 };
            switch (station)
            {
                case ("INIT1"): { return agv1IntPosition; }
                case ("INIT2"): { return agv2IntPosition; }
                case ("INIT3"): { return agv4IntPosition; }
                case ("INIT"): { return agv3IntPosition; }
                case ("STORAGE"): { return storagePosition; }
                case ("COL1"): { return fill1Position; }
                case ("COL2"): { return fill2Position; }
                case ("MIX"): { return mixingPosition; }
                case ("CHARGING1"): { return charging1Position; }
                case ("CHARGING2"): { return charging1Position; }
                default: { Console.WriteLine("No such Place found!!!"); return defaultPos; }
            }
        }
        public bool dockIfStation(int[] position)
        {
            int[] storagePosition = returnPosition("STORAGE");
            int[] fill1Position = returnPosition("COL1");
            int[] fill2Position = returnPosition("COL2");
            int[] mixingPosition = returnPosition("MIX");
            int[] agv1IntPosition = returnPosition("INIT1");
            int[] agv2IntPosition = returnPosition("INIT2");
            int[] agv4IntPosition = returnPosition("INIT3");
            int[] charging1Position = returnPosition("CHARGING1");
            int[] charging2Position = returnPosition("CHARGING2");
            if (position.SequenceEqual(storagePosition))
            {
                Console.WriteLine("station");
                return true;
            }
            else if (position.SequenceEqual(fill1Position))
            {
                Console.WriteLine("station");
                return true;
            }
            else if (position.SequenceEqual(fill2Position))
            {
                Console.WriteLine("station");
                return true;
            }
            else if (position.SequenceEqual(mixingPosition))
            {
                Console.WriteLine("station");
                return true;
            }
            else { return false; }


        }
        /// <summary>
        /// run robots according to schedule
        /// </summary>
        /// <param name="groupAgvInfo"></param>
        /// <param name="dockingTest"></param>
        /// <returns></returns>
        public bool[] runBySchedule(int groupAgvInfo, bool[] dockingTest)
        {
            if (groupAgvInfo < Agvinfo[0])
            {
                Agvinfo = getinfo(groupAgvInfo);
                if (Agvinfo[1] == 1)
                {
                    int[] one = getGroupOneRobot(Agvinfo[2]);
                    //Console.WriteLine("robotID:" + one[0] + " initial position Robot:" + one[1] + "," + one[2] +
                    //" Final Position:" + one[3] + "," + one[4]);
                    int currentRobot = new int();
                    currentRobot = one[0];
                    int[] robot1Final = { one[3], one[4] };
                    double[] robot1FinalMeter = { Convert.ToDouble(one[3]) / 1000.0, Convert.ToDouble(one[4]) / 1000.0 };
                    int[] RobotAssingment = new int[] { 0, 1, 3 }; // assingment of LEDs to IDs
                    int dockId = new int();
                    dockId = currentRobot;
                    if (currentRobot == 2) { dockId = 3; }
                    if (dockingTest[currentRobot] == true)
                    {
                        Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(dockId).unDock();
                        Thread.Sleep(3500);
                        dockingTest[currentRobot] = false;
                    }
                    Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
                    RobotDiscription[] RobotArray = new RobotDiscription[] { Gateway.CTRLModule.getInstance().camCtrl.RobotA, Gateway.CTRLModule.getInstance().camCtrl.RobotB, 
                                                                    Gateway.CTRLModule.getInstance().camCtrl.RobotC };
                    //Arrays to save current position of Robot
                    double[] Current_X = new double[RobotArray.Length];
                    double[] Current_Y = new double[RobotArray.Length];
                    double[] Current_Angle = new double[RobotArray.Length];
                    bool dockAtStation;
                    //Update Current Image to get current Position of Robots
                    Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
                    if (Gateway.CTRLModule.getInstance().camCtrl.MyImage2 != null && Gateway.CTRLModule.getInstance().camCtrl.PlotLEDsAndUpdateStrings == true)
                    {
                        MULTIFORM_PCS.GUI.PCSMainWindow.getInstance().image1.Source = BitmapSourceConvert.ToBitmapSource(Gateway.CTRLModule.getInstance().camCtrl.MyImage2);
                        Gateway.CTRLModule.getInstance().camCtrl.MyImage2.Dispose();
                        Gateway.CTRLModule.getInstance().camCtrl.imOriginal.Dispose();
                    }
                    //get Current Robot1 Postion            
                    Current_X[currentRobot] = RobotArray[currentRobot].Apex.X;
                    Current_Y[currentRobot] = RobotArray[currentRobot].Apex.Y;
                    Current_Angle[currentRobot] = RobotArray[currentRobot].Angle;
                    dockAtStation = dockIfStation(robot1Final);
                    dockingTest[currentRobot] = dockAtStation;
                    //Console.WriteLine("Current robot Position:" + Current_X[currentRobot] + ":" + Current_Y[currentRobot]);

                    //Console.WriteLine(Math.Sqrt((Math.Pow(Current_X[currentRobot] - one[1], 2)) + (Math.Pow(Current_Y[currentRobot] - one[2], 2))));
                    if (Math.Sqrt((Math.Pow(Current_X[currentRobot] - one[1], 2)) + (Math.Pow(Current_Y[currentRobot] - one[2], 2))) <=      500)
                    {
                        Console.WriteLine("current Robot: " + currentRobot + " robot1FinalMeter:" + robot1FinalMeter[0] + " " + robot1FinalMeter[1] + " dock at station:" + dockAtStation);
                       oneRobot(currentRobot, robot1FinalMeter, dockAtStation);
                        dockingTest[3] = true;
                    }
                    else { Console.WriteLine("robots not at desired initial Position"); }

                }
                else if (Agvinfo[1] == 2)
                {
                    int[][] two = getGroupTwoRobot(Agvinfo[2]);
                    //Console.WriteLine("robotID:" + two[0][0] + " initial position Robot:" + two[1][0] + "," + two[1][1] +
                    //    " Final Position:" + two[3][0] + "," + two[3][1]);
                    //Console.WriteLine("robotID:" + two[0][1] + " initial position Robot:" + two[2][0] + "," + two[2][1] +
                    //    " Final Position:" + two[4][0] + "," + two[4][1]);
                    int[] currentRobot = two[0];
                    int[] robot1Final = two[3];
                    int[] robot2Final = two[4];
                    double[] robot1FinalMeter = { Convert.ToDouble(two[3][0]) / 1000.0, Convert.ToDouble(two[3][1]) / 1000.0 };
                    double[] robot2FinalMeter = { Convert.ToDouble(two[4][0]) / 1000.0, Convert.ToDouble(two[4][1]) / 1000.0 };
                    bool[] dockAtStation = new bool[2];

                    int[] RobotAssingment = new int[] { 0, 1, 3 }; // assingment of LEDs to IDs
                    //if (Dock1.Text == "1") { dock[0] = 1; } if (Dock2.Text == "1") { dock[1] = 1; }
                    currentRobot[0] = two[0][0];
                    currentRobot[1] = two[0][1];
                    int[] dockId = new int[2];
                    dockId[0] = currentRobot[0];
                    dockId[1] = currentRobot[1];
                    if (currentRobot[0] == 2)
                    { dockId[0] = 3; }
                    if (currentRobot[1] == 2)
                    { dockId[1] = 3; }
                    //if undocking assigned then undock
                    if (dockingTest[currentRobot[0]] == true || dockingTest[currentRobot[1]] == true)
                    {
                        if (dockingTest[currentRobot[0]] == true)
                        {
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(dockId[0]).unDock();
                            dockingTest[currentRobot[0]] = false;
                        }

                        if (dockingTest[currentRobot[1]] == true)
                        {
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(dockId[1]).unDock();
                            dockingTest[currentRobot[1]] = false;
                        }
                        //time to undock
                        Thread.Sleep(3500);
                    }
                    currentRobot[0] = two[0][0];
                    currentRobot[1] = two[0][1];
                    //Console.WriteLine(" :" + currentRobot[0] + " : " + currentRobot[1]);
                    Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
                    RobotDiscription[] RobotArray = new RobotDiscription[] { Gateway.CTRLModule.getInstance().camCtrl.RobotA, Gateway.CTRLModule.getInstance().camCtrl.RobotB, 
                                                                    Gateway.CTRLModule.getInstance().camCtrl.RobotC };
                    //Arrays to save current position of Robot
                    double[] Current_X = new double[RobotArray.Length];
                    double[] Current_Y = new double[RobotArray.Length];
                    double[] Current_Angle = new double[RobotArray.Length];
                    //Update Current Image to get current Position of Robots
                    Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
                    if (Gateway.CTRLModule.getInstance().camCtrl.MyImage2 != null && Gateway.CTRLModule.getInstance().camCtrl.PlotLEDsAndUpdateStrings == true)
                    {
                        MULTIFORM_PCS.GUI.PCSMainWindow.getInstance().image1.Source = BitmapSourceConvert.ToBitmapSource(Gateway.CTRLModule.getInstance().camCtrl.MyImage2);
                        Gateway.CTRLModule.getInstance().camCtrl.MyImage2.Dispose();
                        Gateway.CTRLModule.getInstance().camCtrl.imOriginal.Dispose();
                    }
                    //get Current Robot1 Postion            
                    Current_X[currentRobot[0]] = RobotArray[currentRobot[0]].Apex.X;
                    Current_Y[currentRobot[0]] = RobotArray[currentRobot[0]].Apex.Y;
                    Current_Angle[currentRobot[0]] = RobotArray[currentRobot[0]].Angle;
                    //get Current Robot2 Position 
                    //Console.WriteLine(":"+currentRobot[1]);
                    Current_X[currentRobot[1]] = RobotArray[currentRobot[1]].Apex.X;
                    Current_Y[currentRobot[1]] = RobotArray[currentRobot[1]].Apex.Y;
                    Current_Angle[currentRobot[1]] = RobotArray[currentRobot[1]].Angle;
                    dockAtStation[0] = dockIfStation(robot1Final);
                    dockingTest[currentRobot[0]] = dockAtStation[0];
                    dockAtStation[1] = dockIfStation(robot2Final);
                    dockingTest[currentRobot[1]] = dockAtStation[1];
                    Console.WriteLine("current Robot " + currentRobot[0] + " :" + currentRobot[1] + " robot1Final" + robot1FinalMeter[0] + " " + robot1FinalMeter[1] + " robot2Final" + robot2FinalMeter[0] + " " + robot2FinalMeter[1] + " dock at station:" + dockAtStation[0] + " " + dockAtStation[1]);
                    if (Math.Sqrt((Math.Pow(Current_X[currentRobot[0]] - two[1][0], 2))
                        + (Math.Pow(Current_Y[currentRobot[0]] - two[1][1], 2))) <= 500)
                    {
                        if (Math.Sqrt((Math.Pow(Current_X[currentRobot[1]] - two[2][0], 2))
                            + (Math.Pow(Current_Y[currentRobot[1]] - two[2][1], 2))) <= 500)
                        {
                            twoRobots(currentRobot, robot1FinalMeter, robot2FinalMeter, dockAtStation);
                            dockingTest[3] = true;
                        }
                        else { Console.WriteLine("This robot is not at desired initial Position:" + currentRobot[1]); }
                    }
                    else { Console.WriteLine("This robot is not at desired initial Position:" + currentRobot[0]); }

                }
                else if (Agvinfo[1] == 3)
                {
                    int[][] three = getGroupThreeRobot(Agvinfo[2]);
                    Console.WriteLine("robotID:" + three[0][0] + " initial position Robot:" + three[1][0] + "," + three[1][1] +
                        " Final Position:" + three[4][0] + "," + three[4][1]);
                    Console.WriteLine("robotID:" + three[0][1] + " initial position Robot:" + three[2][0] + "," + three[2][1]
                        + " Final Position:" + three[5][0] + "," + three[5][1]);
                    Console.WriteLine("robotID:" + three[0][2] + " initial position Robot:" + three[3][0] + "," + three[3][1]
                        + " Final Position:" + three[6][0] + "," + three[6][1]);

                    int[] currentRobot = three[0];
                    int[] robot1Final = three[4];
                    int[] robot2Final = three[5];
                    int[] robot3Final = three[6];
                    double[] robot1FinalMeter = { Convert.ToDouble(three[4][0]) / 1000.0, Convert.ToDouble(three[4][1]) / 1000.0 };
                    double[] robot2FinalMeter = { Convert.ToDouble(three[5][0]) / 1000.0, Convert.ToDouble(three[5][1]) / 1000.0 };
                    double[] robot3FinalMeter = { Convert.ToDouble(three[6][0]) / 1000.0, Convert.ToDouble(three[6][1]) / 1000.0 };
                    bool[] dockAtStation = new bool[3];

                    int[] RobotAssingment = new int[] { 0, 1, 3 }; // assingment of LEDs to IDs
                    //if (Dock1.Text == "1") { dock[0] = 1; } if (Dock2.Text == "1") { dock[1] = 1; }
                    currentRobot[0] = three[0][0];
                    currentRobot[1] = three[0][1];
                    currentRobot[2] = three[0][2];
                    int[] dockId = new int[3];
                    dockId[0] = currentRobot[0];
                    dockId[1] = currentRobot[1];
                    dockId[2] = currentRobot[2];
                    if (currentRobot[0] == 2)
                    { dockId[0] = 3; }
                    if (currentRobot[1] == 2)
                    { dockId[1] = 3; }
                    if (currentRobot[2] == 2)
                    { dockId[2] = 3; }
                    //if undocking assigned then undock
                    if (dockingTest[currentRobot[0]] == true || dockingTest[currentRobot[1]] == true || dockingTest[currentRobot[2]] == true)
                    {
                        if (dockingTest[currentRobot[0]] == true)
                        {
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(dockId[0]).unDock();
                            dockingTest[currentRobot[0]] = false;
                        }

                        if (dockingTest[currentRobot[1]] == true)
                        {
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(dockId[1]).unDock();
                            dockingTest[currentRobot[1]] = false;
                        }
                        if (dockingTest[currentRobot[2]] == true)
                        {
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(dockId[2]).unDock();
                            dockingTest[currentRobot[2]] = false;
                        }

                        //time to undock
                        Thread.Sleep(6500);
                    }
                    currentRobot[0] = three[0][0];
                    currentRobot[1] = three[0][1];
                    currentRobot[2] = three[0][2];
                    //Console.WriteLine(" :" + currentRobot[0] + " : " + currentRobot[1]);
                    Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
                    RobotDiscription[] RobotArray = new RobotDiscription[] { Gateway.CTRLModule.getInstance().camCtrl.RobotA, Gateway.CTRLModule.getInstance().camCtrl.RobotB, 
                                                                    Gateway.CTRLModule.getInstance().camCtrl.RobotC };
                    //Arrays to save current position of Robot
                    double[] Current_X = new double[RobotArray.Length];
                    double[] Current_Y = new double[RobotArray.Length];
                    double[] Current_Angle = new double[RobotArray.Length];
                    //Update Current Image to get current Position of Robots
                    Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
                    if (Gateway.CTRLModule.getInstance().camCtrl.MyImage2 != null && Gateway.CTRLModule.getInstance().camCtrl.PlotLEDsAndUpdateStrings == true)
                    {
                        MULTIFORM_PCS.GUI.PCSMainWindow.getInstance().image1.Source = BitmapSourceConvert.ToBitmapSource(Gateway.CTRLModule.getInstance().camCtrl.MyImage2);
                        Gateway.CTRLModule.getInstance().camCtrl.MyImage2.Dispose();
                        Gateway.CTRLModule.getInstance().camCtrl.imOriginal.Dispose();
                    }
                    //get Current Robot1 Postion            
                    Current_X[currentRobot[0]] = RobotArray[currentRobot[0]].Apex.X;
                    Current_Y[currentRobot[0]] = RobotArray[currentRobot[0]].Apex.Y;
                    Current_Angle[currentRobot[0]] = RobotArray[currentRobot[0]].Angle;
                    //get Current Robot2 Position 
                    //Console.WriteLine(":"+currentRobot[1]);
                    Current_X[currentRobot[1]] = RobotArray[currentRobot[1]].Apex.X;
                    Current_Y[currentRobot[1]] = RobotArray[currentRobot[1]].Apex.Y;
                    Current_Angle[currentRobot[1]] = RobotArray[currentRobot[1]].Angle;
                    Current_X[currentRobot[2]] = RobotArray[currentRobot[2]].Apex.X;
                    Current_Y[currentRobot[2]] = RobotArray[currentRobot[2]].Apex.Y;
                    Current_Angle[currentRobot[2]] = RobotArray[currentRobot[2]].Angle;
                    dockAtStation[0] = dockIfStation(robot1Final);
                    dockingTest[currentRobot[0]] = dockAtStation[0];
                    dockAtStation[1] = dockIfStation(robot2Final);
                    dockingTest[currentRobot[1]] = dockAtStation[1];
                    dockAtStation[2] = dockIfStation(robot3Final);
                    dockingTest[currentRobot[2]] = dockAtStation[2];
                    Console.WriteLine("current Robot " + currentRobot + " robot1Final" + robot1Final[0] + " " + robot1Final[1] + " robot2Final" + robot2Final[0] + " " + robot2Final[1] + " robot3Final" + robot3Final[0] + " " + robot3Final[1] + " dock at station:" + dockAtStation[0] + " " + dockAtStation[1] + " " + dockAtStation[2]);
                    if (Math.Sqrt((Math.Pow(Current_X[currentRobot[0]] - three[1][0], 2))
                        + (Math.Pow(Current_Y[currentRobot[0]] - three[1][1], 2))) <= 500)
                    {
                        if (Math.Sqrt((Math.Pow(Current_X[currentRobot[1]] - three[2][0], 2))
                            + (Math.Pow(Current_Y[currentRobot[1]] - three[2][1], 2))) <= 500)
                        {
                            if (Math.Sqrt((Math.Pow(Current_X[currentRobot[2]] - three[3][0], 2))
                            + (Math.Pow(Current_Y[currentRobot[2]] - three[3][1], 2))) <= 500)
                            {

                                threeRobots(currentRobot, robot1FinalMeter, robot2FinalMeter,robot3FinalMeter,dockAtStation);
                                dockingTest[3] = true;
                            }
                            else { Console.WriteLine("This robot is not at desired initial Position:" + currentRobot[2] + "current position:" + Current_X[currentRobot[2]] + ":" + Current_Y[currentRobot[2]] + " desired position" + three[3][0] + ": " + three[3][1]); }
                        }
                        else { Console.WriteLine("This robot is not at desired initial Position:" + currentRobot[1] + "current position:" + Current_X[currentRobot[1]] + ":" + Current_Y[currentRobot[1]] + " desired position" + three[2][0] + ": " + three[2][1]); }
                    }
                    else { Console.WriteLine("This robot is not at desired initial Position:" + currentRobot[0] + "current position:" + Current_X[currentRobot[0]] + ":" + Current_Y[currentRobot[0]] + " desired position" + three[1][0] + ": " + three[1][1]); }



                }
                Console.WriteLine("Total no. of Groups:" + Agvinfo[0] + " current group:" + groupAgvInfo + " Total tasks in Group:" + Agvinfo[1] + " Start line:" + Agvinfo[2]);


            }
            else { Console.WriteLine("no more groups"); }
            groupAgvInfo++;
            //dockingTest[3] = true;
            // Console.WriteLine(i);
            return dockingTest;
        }
        public int[] timeInfo(int groupAgvInfo)
        {



            int[] val = { 0, 0, 0 };
            StreamReader agvInfo = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "agv_info.log", false);
            List<string> scheduleList = new List<string>();
            string[] lines;
            string[] scheduleArray;
            string line;
            int movementGroups = 1;

            int oldtime = 0;
            int maxTime = 0;
            while ((line = agvInfo.ReadLine()) != null) { scheduleList.Add(line); }
            agvInfo.Close();
            lines = scheduleList.ToArray();
            Tuple<int, string, int, int, int[], int[]>[] sTasks = new Tuple<int, string, int, int, int[], int[]>[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                scheduleArray = lines[i].Split('\t', ' ');
                int taskID = Convert.ToInt32(scheduleArray[0]) / 10000;
                string robotID = scheduleArray[1] + scheduleArray[2];
                int intTime = Convert.ToInt32(scheduleArray[3]);
                int endTime = Convert.ToInt32(scheduleArray[4]);
                int[] startPos = returnPosition(scheduleArray[5]);
                int[] endPos = returnPosition(scheduleArray[6]);
                sTasks[i] = new Tuple<int, string, int, int, int[], int[]>(taskID, robotID, intTime, endTime, startPos, endPos);
                if (sTasks[i].Item4 > maxTime) { maxTime = sTasks[i].Item4; }
                if (oldtime < sTasks[i].Item3) { movementGroups++; }
                oldtime = sTasks[i].Item3;
                //Console.WriteLine(sTasks[i]);               
            }
            Console.WriteLine("Maximum Time:" + maxTime);
            int currentTasks = 0;
            int currentLine = 0;
            int startTime = sTasks[currentTasks].Item3;
            int[] groupStartTime = new int[movementGroups];
            int[] groupEndTime = new int[movementGroups];
            int[] nextGroupStartTime = new int[movementGroups];

            int[,] tasksInGroup = new int[movementGroups, 2];
            //Console.WriteLine(tasksInGroup.Length);
            for (int i = 0; i < movementGroups; i++)
            {
                startTime = sTasks[currentLine].Item3;
                groupStartTime[i] = startTime;
                groupEndTime[i] = sTasks[currentLine].Item4;
                while (startTime == sTasks[currentLine].Item3)
                {
                    currentLine++;
                    currentTasks++;
                    if (currentLine == sTasks.Length) { break; }
                }
                if (currentLine < sTasks.Length)
                { nextGroupStartTime[i] = sTasks[currentLine].Item3; }


                tasksInGroup[i, 0] = currentTasks;
                tasksInGroup[i, 1] = currentLine - currentTasks;
                currentTasks = 0;
            }
            if (groupAgvInfo < movementGroups)
            {
                val[0] = groupStartTime[groupAgvInfo];
                val[1] = groupEndTime[groupAgvInfo];
                val[2] = nextGroupStartTime[groupAgvInfo];
            }

            return val;
        }

        
    }
}
