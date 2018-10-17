using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Threading;
using MULTIFORM_PCS.ControlModules.SchedulingModule;
using MULTIFORM_PCS.ControlModules.FeedForwadModule;
using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.DataTypes;
using MULTIFORM_PCS.ControlModules.CameraModule.CameraForm;
using MULTIFORM_PCS.ControlModules.CameraControl.CameraControlClass;

using System.Diagnostics; // Process
using System.Globalization;
using Emgu.CV.WPF;


namespace MULTIFORM_PCS.ControlModules.MPCModule
{
    class AGVInfo
    {
        int[] Agvinfo = { 1, 0, 0 };
        public NMPC runR;
        //public int[] v;

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
            int[] storagePosition = { 1850, 2200 };
            int[] fill1Position = { 1100, 1520 };
            int[] fill2Position = { 3000, 1520 };
            int[] mixingPosition = { 2150, 800 };
            int[] agv1IntPosition = { 3200, 700 };
            int[] agv2IntPosition = { 2750, 700 };
            int[] agv4IntPosition = { 1150, 2300 };
            int[] agv3IntPosition = { 1150, 2300 };
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
                return true; }
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
                    if (dockingTest[currentRobot] == true) { Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(dockId).unDock();
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
                    if (Math.Sqrt((Math.Pow(Current_X[currentRobot] - one[1], 2)) + (Math.Pow(Current_Y[currentRobot] - one[2], 2))) <= 180)
                    {
                        Console.WriteLine("current Robot: " + currentRobot + " robot1FinalMeter:" + robot1FinalMeter[0] + " " + robot1FinalMeter[1] + " dock at station:" + dockAtStation);
                         runR.oneRobot(currentRobot, robot1FinalMeter, dockAtStation);
                        dockingTest[3] = true;
                    }
                    else { Console.WriteLine("robots not at desired initial Position"); }

                }
                else if (Agvinfo[1] == 2)
                {
                    int[][] two =getGroupTwoRobot(Agvinfo[2]);
                    Console.WriteLine("robotID:" + two[0][0] + " initial position Robot:" + two[1][0] + "," + two[1][1] +
                        " Final Position:" + two[3][0] + "," + two[3][1]);
                    Console.WriteLine("robotID:" + two[0][1] + " initial position Robot:" + two[2][0] + "," + two[2][1] +
                        " Final Position:" + two[4][0] + "," + two[4][1]);
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
                        if (dockingTest[currentRobot[0]] == true) { Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(dockId[0]).unDock();
                        dockingTest[currentRobot[0]] = false;
                        }

                        if (dockingTest[currentRobot[1]] == true) { Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(dockId[1]).unDock();
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
                    Console.WriteLine("current Robot " + currentRobot + " robot1Final" + robot1Final[0] + " " + robot1Final[1] + " robot2Final" + robot2Final[0] + " " + robot2Final[1] + " dock at station:" + dockAtStation[0] + " " + dockAtStation[1]);
                    if (Math.Sqrt((Math.Pow(Current_X[currentRobot[0]] - two[1][0], 2))
                        + (Math.Pow(Current_Y[currentRobot[0]] - two[1][1], 2))) <= 80)
                    {
                        if (Math.Sqrt((Math.Pow(Current_X[currentRobot[1]] - two[2][0], 2))
                            + (Math.Pow(Current_Y[currentRobot[1]] - two[2][1], 2))) <= 80)
                        { 
                            //runR.twoRobots(currentRobot, robot1FinalMeter, robot2FinalMeter, dockAtStation);
                            dockingTest[3] = true;
                        }
                        else { Console.WriteLine("robots not at desired initial Position"); }
                    }
                    else { Console.WriteLine("robots not at desired initial Position"); }

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
                        if (dockingTest[currentRobot[0]] == true) { Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(dockId[0]).unDock();
                        dockingTest[currentRobot[0]] = false;
                        }

                        if (dockingTest[currentRobot[1]] == true) { Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(dockId[1]).unDock();
                        dockingTest[currentRobot[1]] = false;
                        }
                        if (dockingTest[currentRobot[2]] == true) { Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(dockId[2]).unDock();
                        dockingTest[currentRobot[2]] = false;
                        }

                        //time to undock
                        Thread.Sleep(3500);
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
                    dockAtStation[0] = dockIfStation(robot1Final);
                    dockingTest[currentRobot[0]] = dockAtStation[0];
                    dockAtStation[1] = dockIfStation(robot2Final);
                    dockingTest[currentRobot[1]] = dockAtStation[1];
                    dockAtStation[2] = dockIfStation(robot3Final);
                    dockingTest[currentRobot[2]] = dockAtStation[2];
                    Console.WriteLine("current Robot " + currentRobot + " robot1Final" + robot1Final[0] + " " + robot1Final[1] + " robot2Final" + robot2Final[0] + " " + robot2Final[1] + " robot3Final" + robot3Final[0] + " " + robot3Final[1] + " dock at station:" + dockAtStation[0] + " " + dockAtStation[1] + " " + dockAtStation[2]);
                    if (Math.Sqrt((Math.Pow(Current_X[currentRobot[0]] - three[1][0], 2))
                        + (Math.Pow(Current_Y[currentRobot[0]] - three[1][1], 2))) <= 80)
                    {
                        if (Math.Sqrt((Math.Pow(Current_X[currentRobot[1]] - three[2][0], 2))
                            + (Math.Pow(Current_Y[currentRobot[1]] - three[2][1], 2))) <= 80)
                        {
                            if (Math.Sqrt((Math.Pow(Current_X[currentRobot[2]] - three[3][0], 2))
                            + (Math.Pow(Current_Y[currentRobot[2]] - three[3][1], 2))) <= 80)
                            {

                                //runR.threeRobots(currentRobot, robot1FinalMeter, robot2FinalMeter,robot3FinalMeter,dockAtStation);
                                dockingTest[3] = true;
                            }
                            else { Console.WriteLine("robots not at desired initial Position"); }
                        }
                        else { Console.WriteLine("robots not at desired initial Position"); }
                    }
                    else { Console.WriteLine("robots not at desired initial Position"); }



                }
                Console.WriteLine("Total no. of Groups:" + Agvinfo[0] + " current group:" + groupAgvInfo + " Total tasks in Group:" + Agvinfo[1] + " Start line:" + Agvinfo[2]);
                

            }
            else { Console.WriteLine("no more groups"); }
            groupAgvInfo++;
            dockingTest[3] = true;
            // Console.WriteLine(i);
            return dockingTest;
        }
        



    }
}
