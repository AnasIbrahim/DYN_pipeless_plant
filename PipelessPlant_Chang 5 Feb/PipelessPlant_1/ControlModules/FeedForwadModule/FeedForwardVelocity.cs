using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using MULTIFORM_PCS.ControlModules.SchedulingModule;
using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.DataTypes;
using MULTIFORM_PCS.ControlModules.CameraModule.CameraForm;
using MULTIFORM_PCS.ControlModules.CameraControl.CameraControlClass;
using Emgu.CV.WPF;

namespace MULTIFORM_PCS.ControlModules.FeedForwadModule
{
    class FeedForwardVelocity
    {
        #region Internal variables
        CollisionInfoForPath[][] RobotInformationArray; // the array of all AGVs with there coresponding velocites
        int[] ControledRobotIDs;
        decimal[] PreviousVelocities;
        int SamplingTime;
        int TimeSlotGlobal;
        double timeGlobal_Seconds;
        int TimeSlotOfCurrentGroup;
        int NumberOfGroups;
        int CurrentGroup;
        int Controller_Velocity_Flag;
        bool[] NotFirstMovement;
        #endregion
        public FeedForwardVelocity()
        {

        }
        public void ConfigureFeedForwardVelocity(int[] RobotsToControl)
        {
            #region ConfigurationParametersFeedForward
            SamplingTime = 100; // in ms
            #endregion
            //RobotInformationArray = Gateway.CTRLModule.getInstance().SchedulingPathplanning.GetFinalVelocityProfileStructures(); // initalaize the class
            //NumberOfGroups = RobotInformationArray.Length;
            //timeGlobal_Seconds = 0;
            //TimeSlotGlobal = 0; // pointer to the array locations
            //TimeSlotOfCurrentGroup = 0;
            //CurrentGroup = 0;
            //ControledRobotIDs = RobotsToControl;
            //ControledRobotIDs[2] = 3;
            //PreviousVelocities = new decimal[ControledRobotIDs.Length];
            RobotInformationArray = Gateway.CTRLModule.getInstance().SchedulingPathplanning.GetFinalVelocityProfileStructures(); // initalaize the class
            NumberOfGroups = RobotInformationArray.Length;
            CurrentGroup = 0;
            TimeSlotOfCurrentGroup = 0;
            timeGlobal_Seconds = Convert.ToDouble(RobotInformationArray[CurrentGroup][0].velocityProfile[TimeSlotOfCurrentGroup, 0]);
            TimeSlotGlobal = Convert.ToInt32(timeGlobal_Seconds /1000 * SamplingTime); // pointer to the array locations
            ControledRobotIDs = RobotsToControl;
            PreviousVelocities = new decimal[ControledRobotIDs.Length];
            NotFirstMovement = new bool[ControledRobotIDs.Length];

        }
        public void FeedForwardVelocities()
        {
            #region ConfigurationParametersFeedForward2
            bool FastOperation = true; // bypass the wait times between groups
            double SpeedMultiplier = 10;//130/20;//170 / 20 ;  // used to sacale the FeedForward Calculated from the velocity profiles
            double[,] RobotsInitialPosition = new double[,] { { (287) * 10, 40 * 10 }, { (72 + 20) * 10, (230 - 20) * 10 }, { (328 - 20) * 10, (230 - 20) * 10 } }; // initial Positions of the  "3" robots
            double[] RobotsInitialAngle = new double[] { 90, -45, 225 };
            int mmtoCm = 10;
            #endregion


            timeGlobal_Seconds = (SamplingTime * TimeSlotGlobal) / 1000.0; // changes the time to real seconds
            Console.WriteLine("Time now is : " + timeGlobal_Seconds.ToString() + " Seconds.");
           
            if (CurrentGroup < NumberOfGroups) // have we finished the Schedule?
            {
                bool[] RobotsThatRequierControl = new bool[ControledRobotIDs.Length]; // flags if a robot has changed orientation
                
                #region FirstIteration
                if (CurrentGroup == 0 && TimeSlotOfCurrentGroup == 0) // this is the first run of the feedforward controller thus we need to correct orientation
                {
                    //double[,] RobotsInitialPosition = new double[ControledRobotIDs.Length, 2];
                    
                    for (int IitialIds = 0; IitialIds < ControledRobotIDs.Length; IitialIds++)
                    {
                        //RobotsInitialPosition[IitialIds, 0] = Convert.ToDouble(RobotInformationArray[0][IitialIds].path[0].X);
                        //RobotsInitialPosition[IitialIds, 1] = Convert.ToDouble(RobotInformationArray[0][IitialIds].path[0].Y);
                        //RobotsInitialAngle[IitialIds] = Convert.ToDouble(RobotInformationArray[0][IitialIds].allOrientations[0]);
                        RobotsThatRequierControl[IitialIds] = true;
                        if (RobotInformationArray[0][IitialIds].path != null && RobotInformationArray[0][IitialIds].allOrientations != null)
                        {
                            RobotsInitialPosition[IitialIds, 0] = Convert.ToDouble(RobotInformationArray[CurrentGroup][IitialIds].path[0].X * mmtoCm);
                            RobotsInitialPosition[IitialIds, 1] = Convert.ToDouble(RobotInformationArray[CurrentGroup][IitialIds].path[0].Y * mmtoCm);
                            RobotsInitialAngle[IitialIds] = Convert.ToDouble(RobotInformationArray[CurrentGroup][IitialIds].allOrientations[0]);
                            RobotsThatRequierControl[IitialIds] = true;
                        }
                        
                    }
                    // Controller run for all 3 robots can use both rotation and translation
                    controllerLoop(RobotsThatRequierControl, RobotsInitialAngle, RobotsInitialPosition, 1);
                }
                #endregion

                if (TimeSlotOfCurrentGroup >= RobotInformationArray[CurrentGroup][0].velocityProfile.GetLength(0)) // Have we finished the current group 
                {// yes we have Thus controller needed for all 3 robots to correct the still current groups Orientation and Position

                    // safety stop for all robots between groups
                    for (int i = 0; i < ControledRobotIDs.Length; i++)
                    {
                        double[] velocity= new  double[] {0,0};
                        Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(ControledRobotIDs[i]).forward(velocity, 0, 0,0);
                    }
                    // Add controller for all 3 robot here both rotation and translation using the still current groups Orientations 
                    double[,] RobotsCorrectionPosition = new double[ControledRobotIDs.Length, 2];
                    double[] RobotsCorrectionAngle = new double[ControledRobotIDs.Length];
                    RobotsThatRequierControl = new bool[ControledRobotIDs.Length];
                    for (int IitialId = 0; IitialId < ControledRobotIDs.Length; IitialId++)
                    {
                        if (RobotInformationArray[CurrentGroup][IitialId].path != null && RobotInformationArray[CurrentGroup][IitialId].allOrientations != null)
                        {
                            RobotsCorrectionPosition[IitialId, 0] = Convert.ToDouble(RobotInformationArray[CurrentGroup][IitialId].path[TimeSlotOfCurrentGroup - 1].X * mmtoCm);
                            RobotsCorrectionPosition[IitialId, 1] = Convert.ToDouble(RobotInformationArray[CurrentGroup][IitialId].path[TimeSlotOfCurrentGroup - 1].Y * mmtoCm);
                            RobotsCorrectionAngle[IitialId] = Convert.ToDouble(RobotInformationArray[CurrentGroup][IitialId].allOrientations[TimeSlotOfCurrentGroup - 1]);
                            RobotsThatRequierControl[IitialId] = true;
                        }
                    }
                    // Controller run for all 3 robots can use both rotation and translation (IDEA"// we can have the controller send back the time needed to control and thus update the global time (think about this later).)
                    controllerLoop(RobotsThatRequierControl, RobotsCorrectionAngle, RobotsCorrectionPosition, 1);
                    for (int DockRobot = 0; DockRobot < ControledRobotIDs.Length; DockRobot++) // Each robot that requiers control has been moved and thus ends up at a station == docking needed
                    {
                        if (RobotsThatRequierControl[DockRobot])
                        {
                            Thread.Sleep(50);
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(ControledRobotIDs[DockRobot]).dock();
                            NotFirstMovement[DockRobot] = true;
                        }
                    }
                    if (FastOperation)
                    {
                        Thread.Sleep(7000);
                    }

                    // run controller again to make sure the next group is starting correctly 
                    CurrentGroup++;
                    TimeSlotOfCurrentGroup = 0;
                    RobotsThatRequierControl = new bool[ControledRobotIDs.Length];
                    for (int IitialId = 0; IitialId < ControledRobotIDs.Length; IitialId++)
                    {
                        if (RobotInformationArray[CurrentGroup][IitialId].path != null && RobotInformationArray[CurrentGroup][IitialId].allOrientations != null)
                        {
                        RobotsCorrectionPosition[IitialId, 0] = Convert.ToDouble(RobotInformationArray[CurrentGroup][IitialId].path[TimeSlotOfCurrentGroup].X * mmtoCm);
                        RobotsCorrectionPosition[IitialId, 1] = Convert.ToDouble(RobotInformationArray[CurrentGroup][IitialId].path[TimeSlotOfCurrentGroup].Y * mmtoCm);
                        RobotsCorrectionAngle[IitialId] = Convert.ToDouble(RobotInformationArray[CurrentGroup][IitialId].allOrientations[TimeSlotOfCurrentGroup]);
                        RobotsThatRequierControl[IitialId] = true;
                        }
                    }
                    for (int UnDockRobot = 0; UnDockRobot < ControledRobotIDs.Length; UnDockRobot++) // Each robot that requiers control will be moved and thus starts at a station == undocking needed
                    {
                        if (RobotsThatRequierControl[UnDockRobot] && NotFirstMovement[UnDockRobot])
                        {
                            Thread.Sleep(50);
                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(ControledRobotIDs[UnDockRobot]).unDock();
                        }
                        /*else
                        {
                            NotFirstMovement[UnDockRobot] = true;
                        }*/
                    }
                    // Sleep the thread for 5 seconds (time needed for undock)
                    Thread.Sleep(6000);
                    controllerLoop(RobotsThatRequierControl, RobotsCorrectionAngle, RobotsCorrectionPosition, 1);
                }


                if (FastOperation || timeGlobal_Seconds >= Convert.ToDouble(RobotInformationArray[CurrentGroup][0].velocityProfile[TimeSlotOfCurrentGroup, 0]))// check if it is time to feed forward the group (time gap between moves for groups)
                {
                    if (TimeSlotOfCurrentGroup != 0) // not the first iteration of the group since that is taken care of when the group changes
                    {
                        
                        double[] RobotsDesiredAngle=new double[ControledRobotIDs.Length];
                        double[,] RobotsDesiredPosition=new double[ControledRobotIDs.Length,2];

                        
                        for (int RobotOrientationFlag = 0; RobotOrientationFlag < ControledRobotIDs.Length; RobotOrientationFlag++) // check all robot orientations relative to previous.
                        {

                            if (RobotInformationArray[CurrentGroup][RobotOrientationFlag].allOrientations != null) // check if the AGV has info and is not just extended
                            {
                                // flag is true if we have diffrent orientations
                                RobotsThatRequierControl[RobotOrientationFlag] = RobotInformationArray[CurrentGroup][RobotOrientationFlag].allOrientations[TimeSlotOfCurrentGroup] != RobotInformationArray[CurrentGroup][RobotOrientationFlag].allOrientations[TimeSlotOfCurrentGroup - 1];
                                RobotsDesiredAngle[RobotOrientationFlag]=Convert.ToDouble(RobotInformationArray[CurrentGroup][RobotOrientationFlag].allOrientations[TimeSlotOfCurrentGroup]);
                                RobotsDesiredPosition[RobotOrientationFlag,0]=Convert.ToDouble(RobotInformationArray[CurrentGroup][RobotOrientationFlag].path[TimeSlotOfCurrentGroup].X);
                                RobotsDesiredPosition[RobotOrientationFlag,1]=Convert.ToDouble(RobotInformationArray[CurrentGroup][RobotOrientationFlag].path[TimeSlotOfCurrentGroup].Y);

                            }
                            else
                            {
                                RobotsThatRequierControl[RobotOrientationFlag] = false;
                                    
                                RobotsDesiredAngle[RobotOrientationFlag]=0;
                                RobotsDesiredPosition[RobotOrientationFlag,0]=0;
                                RobotsDesiredPosition[RobotOrientationFlag,1]=0;
                            }

                        }
                        if (RobotsThatRequierControl.Contains(true))
                        {
                            controllerLoop(RobotsThatRequierControl, RobotsDesiredAngle, RobotsDesiredPosition, 3);
                            Controller_Velocity_Flag = 1; // Send The Velocity after the control This is used to resend the velocity even if it was the same as previous since the controller overrrid the velocity
                        }

                    }
                    double Velocity;
                    for (int i = 0; i < ControledRobotIDs.Length; i++)
                    {
                        
                        //Check if Orientation Changed
                        // if first slot in group send to all if not check if the V to be sent is the same as previous if so do NOT send
                        if (TimeSlotOfCurrentGroup == 0 || RobotInformationArray[CurrentGroup][i].velocityProfile[TimeSlotOfCurrentGroup - 1, 1] != RobotInformationArray[CurrentGroup][i].velocityProfile[TimeSlotOfCurrentGroup, 1] || Controller_Velocity_Flag==1)
                        {
                            double[] Send_Velocity = new double[2];
                            Velocity = Convert.ToDouble(RobotInformationArray[CurrentGroup][i].velocityProfile[TimeSlotOfCurrentGroup, 1]);
                            int xxxx = ControledRobotIDs[i]; // debug variable

                            Send_Velocity[0] = Velocity * SpeedMultiplier;
                            Send_Velocity[1] = Velocity * SpeedMultiplier;

                            Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(ControledRobotIDs[i]).forward(Send_Velocity, 0, 0, 0);
                            Thread.Sleep(5);
                        }
                    }
                    Controller_Velocity_Flag = 0; // moved from inside the forloop above
                    TimeSlotOfCurrentGroup++;

                }
                TimeSlotGlobal++;
            }
            else
            {
                decimal TotalTimeNeeded = RobotInformationArray[CurrentGroup][0].velocityProfile[RobotInformationArray[CurrentGroup][0].velocityProfile.Length - 1, 0];
                MessageBox.Show("The FeedForward control has Finished and took: " + TotalTimeNeeded + " Sec. Covering " + CurrentGroup + " groups");
            }

        }
        private void controllerLoop(bool[] RobotsToBeControlled,double[] DesiredAngleArray,double[,] DesiredPositions,int TypeOfControl)
        {
            #region Parameters
            int[] RobotAssingment = ControledRobotIDs;       // assingment of LEDs to IDs A,B,C
            //int[] RobotAssingment = new int[] { 0, 1, 3 }; // assingment of LEDs to IDs A,B,C
            Boolean Manual = false;
            int IterationsLimit = 2000; // Approx 1 min
            #endregion
            Controller[] ControllerArray = new Controller[] { Gateway.CTRLModule.getInstance().Controller_Robot1, Gateway.CTRLModule.getInstance().Controller_Robot2, Gateway.CTRLModule.getInstance().Controller_Robot3 };
            double[,] Destinations = DesiredPositions;
            double[] Orientations = DesiredAngleArray;
            int[] DirectionControllerOutput = new int[RobotsToBeControlled.Length];
            int[] Robot_Phase = new int[RobotsToBeControlled.Length];

         

            for (int i = 0; i < RobotsToBeControlled.Length; i++)
            {
                if (RobotsToBeControlled[i] == true)
                {
                    Robot_Phase[i] = TypeOfControl; // Set Controller Phase
                    ControllerArray[i].SetPhase(TypeOfControl); // set phase to inital phase (1) set final rotation phase(3)

                }
                else
                {
                    double[] veloStop = new double[] { 0, 0 };
                    Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(ControledRobotIDs[i]).forward(veloStop, 0, 0, 0);
                    Robot_Phase[i] = 5;
                    ControllerArray[i].SetPhase(5); // set final phase
                }
            }
            int WatchDogCounter = 0;
            // Declare RobotPhases
            while (!((Robot_Phase[0] == 5) && (Robot_Phase[1] == 5) && (Robot_Phase[2] == 5)) && WatchDogCounter <= IterationsLimit) // ends Only When All Robots Are finished
            {
                // Execute Controller as fast as possible
                WatchDogCounter++;
                Gateway.CTRLModule.getInstance().camCtrl.processFrameAndUpdateGUI();
                RobotDiscription[] RobotArray = new RobotDiscription[] { Gateway.CTRLModule.getInstance().camCtrl.RobotA, Gateway.CTRLModule.getInstance().camCtrl.RobotB, Gateway.CTRLModule.getInstance().camCtrl.RobotC };
                

                if (Gateway.CTRLModule.getInstance().camCtrl.MyImage2 != null && Gateway.CTRLModule.getInstance().camCtrl.PlotLEDsAndUpdateStrings == true)
                {
                   MULTIFORM_PCS.GUI.PCSMainWindow.getInstance().image1.Source = BitmapSourceConvert.ToBitmapSource(Gateway.CTRLModule.getInstance().camCtrl.MyImage2);
                }
                double[] Current_X = new double[RobotArray.Length];
                double[] Current_Y = new double[RobotArray.Length];
                double[] Current_Angle = new double[RobotArray.Length];
                double[] velocity = new double[RobotArray.Length];
                double[][] velocity2 = new double[RobotArray.Length][];
                int[] Phase_Robot = new int[RobotArray.Length];
                for (int CurrentRobot = 0; CurrentRobot < RobotArray.Length; CurrentRobot++)
                {
                    if (RobotsToBeControlled[CurrentRobot] == true)
                    {
                        Current_X[CurrentRobot] = RobotArray[CurrentRobot].Apex.X;
                        Current_Y[CurrentRobot] = RobotArray[CurrentRobot].Apex.Y;
                        Current_Angle[CurrentRobot] = RobotArray[CurrentRobot].Angle;
                        velocity[CurrentRobot] = Math.Abs(ControllerArray[CurrentRobot].CalculateVelocity(Current_X[CurrentRobot], Current_Y[CurrentRobot], Current_Angle[CurrentRobot], Destinations[CurrentRobot, 0], Destinations[CurrentRobot, 1], Orientations[CurrentRobot]));
                        velocity2[CurrentRobot] = new double[] { velocity[CurrentRobot], velocity[CurrentRobot] };
                        DirectionControllerOutput[CurrentRobot] = ControllerArray[CurrentRobot].GetDirection();
                        if (!Manual)
                        {
                            switch (DirectionControllerOutput[CurrentRobot])
                            {
                                #region Explanation
                                /*
                                        Forward         = 0,
                                        Backward        = 1,
                                        Clockwise       = 2,
                                        AntiClockwise   = 3
                            */
                                #endregion
                                case 0:
                                    Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[CurrentRobot]).forward(velocity2[CurrentRobot], 0, 0, 0);
                                    break;
                                case 1:
                                    Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[CurrentRobot]).backward(velocity[CurrentRobot], 0, 0, 0);
                                    break;

                                case 2:
                                    velocity2[CurrentRobot][0] = -velocity2[CurrentRobot][0];
                                    Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[CurrentRobot]).forward(velocity2[CurrentRobot], 0, 0, 0);
                                    break;

                                case 3:
                                    velocity2[CurrentRobot][1] = -velocity2[CurrentRobot][1];
                                    Gateway.CTRLModule.getInstance().getRobotRemoteCTRL(RobotAssingment[CurrentRobot]).forward(velocity2[CurrentRobot], 0, 0, 0);
                                    break;

                                default:

                                    break;
                            }
                        }


                        Robot_Phase[CurrentRobot] = ControllerArray[CurrentRobot].GetPhase();


                    }

                   
                }
                #region explanation of numbers to phases
                /*
             * 1 = First_Rotation_Phase
             * 2 = Translation_Phase
             * 3 = Second_Rotation_Phase
             * 4 = Stop_Phase
             * 5 = Finished_Phase
             * */
                #endregion
            }
            if (WatchDogCounter >= 600)
            {
                MessageBox.Show("CONTROLLER QUIT DUE TO RUNNING MORE THAN :" + IterationsLimit + " Iterations.");
            }

            Gateway.CTRLModule.getInstance().Controller_Robot1.SetPhase(1); // set phase to inital phase
            Gateway.CTRLModule.getInstance().Controller_Robot2.SetPhase(1); // set phase to inital phase
            Gateway.CTRLModule.getInstance().Controller_Robot3.SetPhase(1); // set phase to inital phase


        }
    }
}
