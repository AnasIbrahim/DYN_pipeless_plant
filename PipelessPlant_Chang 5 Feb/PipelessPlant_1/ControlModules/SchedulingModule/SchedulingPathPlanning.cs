using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;



namespace MULTIFORM_PCS.ControlModules.SchedulingModule
{
    using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.Path;   //test---> delete later 
    using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.DataTypes;  //test---> delete later
    using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.Velocity;
    using MULTIFORM_PCS.ControlModules.SchedulingModule;
    using MULTIFORM_PCS.ControlModules.RoutingModule;
    using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning;
    using MULTIFORM_PCS.ControlModules.ConnectionModule;
    using MULTIFORM_PCS.Datastructure.Schedule;

    class SchedulingPathPlanning
    {
        CollisionInfoForPath[][] FinalVelocityProfileStructures;
        //public static bool flag_all_agv_commands_done = false;
        //public static bool flag_rescheduling_needed = false;


        /*public static Schedules.SCHEDULEENTRY getLastMovementTask(Schedule schedule, List<AGVData> agvMovementInfo)
        {
            Schedules.SCHEDULEENTRY lastMovementTask = new Schedules.SCHEDULEENTRY();
            for (int i = schedule.Count - 1; i >= 0; i--)
            {
                if (schedule[i].resourceIndex <= 3)
                {
                    int taskID = schedule[i].orderTaskIndex;
                    for (int j = agvMovementInfo.Count - 1; j >= 0; j--)
                    {
                        if (taskID == agvMovementInfo[j].taskID)
                        {
                            return schedule[i];
                        }
                    }
                }
            }
        
            return null;
        }*/
        public void SchedulerPathPlanningMain(string[] AvailableAGVs)
            {
            
            List<CollisionInfoForPath[]> MyList = new List<CollisionInfoForPath[]>();
            /**First step : Get AGV movement data**/

            /* Class instances */
            StopWatchWithOffset stopWatch = new StopWatchWithOffset(TimeSpan.FromSeconds(530));
            string movementStartTime = DateTime.Now.ToString();
            //AGVMovmentData agvData = new AGVMovmentData();
            //PAndVPlanning pAndv = new PAndVPlanning(agvData);
            Bspline bspline = new Bspline();
            STspace stSpace = new STspace(0, 0);
            Plot plot = new Plot();
            /**/
            /*Get the initial schedule*/
            string path = AppDomain.CurrentDomain.BaseDirectory + "schedules.log";
            //TAOptModule.getInstance().scheduleRecipes();
            CollisionInfoForPath[][] allCollisionInfos = new CollisionInfoForPath[][] { };

            string[] AGVusedForRecipeID = new string[] { "", "", "", "", "", "" };  // of each recipe
            List<AGVData> previousAGVmovementInfo = new List<AGVData>();
            /**/

            /*Full AGV information for implementation, including tasks with extended durations, etc */
            List<AGVData> AGVfullInfo = new List<AGVData>();
            /**/

            AGVMovmentData agvData = new AGVMovmentData(AvailableAGVs, previousAGVmovementInfo, path);
         
            decimal[] allStartTimes = new decimal[] { };
            PAndVPlanning pAndv = new PAndVPlanning(agvData, allStartTimes);
            List<List<AGVData>> agvCalculatedGroups = new List<List<AGVData>>();
       
            //int index_rescheduling = -1;
            //int counter_finished_tasks = 0;

            /* Main Loop */

                    agvData.path = path; agvData.initialAGVmovementInfo = previousAGVmovementInfo;
                    //agvData = new AGVMovmentData(AGVusedForRecipeID, previousAGVmovementInfo, path);
                    agvData.init(path);

                    previousAGVmovementInfo = agvData.agvMovementInfo;

                    /*Get the last movement task*/
                    //Schedules.SCHEDULEENTRY lastMovementTask = getLastMovementTask(executedSchedule, agvData.agvMovementInfo);
                    /**/

                    allStartTimes = new decimal[agvData.agvMovementInfo.Count];
                    for (int i = 0; i < allStartTimes.Length; i++)
                        allStartTimes[i] = agvData.agvMovementInfo[i].startTime;

                    pAndv = new PAndVPlanning(agvData, allStartTimes);
                    pAndv.Init();

                    agvCalculatedGroups = agvData.getGroupOfCommandsWithPossiblePathCollisions(AvailableAGVs);
                    allCollisionInfos = new CollisionInfoForPath[agvCalculatedGroups.Count][];
                   

                        for (int i = 0; i < agvCalculatedGroups.Count; i++)
                        {
                            /**/
                            Console.WriteLine("Group" + i + ":");
                            List<List<Position>> oneGroupOfPaths = new List<List<Position>>();
                            List<decimal> durations = new List<decimal>();

                            /* Get respective A* Paths */
                            for (int j = 0; j < agvCalculatedGroups[i].Count; j++)
                            {
                                for (int k = 0; k < agvData.agvMovementInfo.Count; k++)
                                {
                                    if (agvCalculatedGroups[i][j].taskID == agvData.agvMovementInfo[k].taskID && agvCalculatedGroups[i][j].startPosition == agvData.agvMovementInfo[k].startPosition
                                        && agvCalculatedGroups[i][j].endPosition == agvData.agvMovementInfo[k].endPosition)
                                    {
                                        oneGroupOfPaths.Add(pAndv.allPaths[k]);
                                        durations.Add(pAndv.allPathDurations[k]);
                                        Console.WriteLine("duration: " + durations[j]);
                                        break;
                                    }
                                }
                            }

                            /**/
                            decimal[] startTimes = new decimal[agvCalculatedGroups[i].Count];
                            for (int k = 0; k < agvCalculatedGroups[i].Count; k++)
                            {
                                startTimes[k] = agvCalculatedGroups[i][k].startTime;
                                //agvCalculatedGroups[i][k].endTime = durations[k] + startTimes[k]; // update the end times
                                Console.WriteLine("endTimes[" + k + "] = " + agvCalculatedGroups[i][k].endTime);
                            }

                            CollisionInfoForPath[] collisionInfo = pAndv.calculateOneGroupOfPaths(agvCalculatedGroups[i], oneGroupOfPaths, startTimes, durations);
                            MyList.Add(collisionInfo.ToArray());
                            allCollisionInfos[i] = collisionInfo; //ToArray();
                            int xxxx = collisionInfo.Length;
                            /*AGV info for implementation*/
                            for (int p = 0; p < collisionInfo.Length; p++)
                            {
                                AGVData agv_data = new AGVData(collisionInfo[p].taskID, collisionInfo[p].usedAGV, agvCalculatedGroups[i][p].startTime, collisionInfo[p].pathFinishingTime, agvCalculatedGroups[i][p].startPosition,
                                    agvCalculatedGroups[i][p].endPosition);
                                AGVfullInfo.Add(agv_data);
                            }

                            /**/
                        }

            CollisionInfoForPath[][] Myarray = MyList.ToArray();
            CollisionInfoForPath[][] MyarrayCopy = MyList.ToArray();
            FinalVelocityProfileStructures = MakeAllVelocityProfilesEqual(Myarray,AvailableAGVs);
            //CollisionInfoForPath[] xxy = FinalVelocityProfileStructures[41];
            Console.WriteLine("\nPlanning done...");
            Console.WriteLine("\nPlanning done...");

                # region OldCode
                //int currentAGVForCurrentGroup = 0;
                //string TempNumber = "1";
                //int longestVP = 0;
                //for (int i = 0; i < (Myarray.Length); i++)
                //{
                //    CollisionInfoForPath[] currentGroup = Myarray[i];
                //    for (int k = 0; k < currentGroup.Length; k++)
                //    {
                //        if (longestVP < currentGroup[k].velocityProfile.GetLength(0))
                //            longestVP = currentGroup[k].velocityProfile.GetLength(0);
                //    }
                //    for (int j = 0; j < currentGroup.Length; j++)
                //    {
                //        if (currentGroup[j].velocityProfile.GetLength(0) < longestVP) //needs to be extended
                //        {
                //            decimal[,] NewVelocityProfile = new decimal[longestVP,2];
                //            //NewVelocityProfile[0:currentGroup[j].velocityProfile.GetLength(0)-1][:] = currentGroup[j].velocityProfile[0:currentGroup[j].velocityProfile.GetLength(0)-1,:]; 
                //            for (int p1 = 0; p1 < currentGroup[j].velocityProfile.GetLength(0); p1++)
                //            {
                //                for (int p2 = 0; p2 < currentGroup[j].velocityProfile.GetLength(1); p2++)
                //            {


                //            }

                //            }
                //        }

                //        if(currentGroup[j].usedAGV.EndsWith("1"))
                //        {
                //            ArrayCollisonInfo[i][0] = currentGroup[j];// 1st AGV used
                //        }
                //        else if (currentGroup[j].usedAGV.EndsWith("2"))
                //        {
                //            ArrayCollisonInfo[i][1] = currentGroup[j];// 2nd AGV used
                //        }
                //        else
                //        {
                //            ArrayCollisonInfo[i][2] = currentGroup[j]; // 3rd AGV used
                //        }
                //    }

                //    longestVP = 0;
                //}
                #endregion
                #region CommentedCode

                /* Start Implementation */
                // The AGV Controller has to be applied here. 
                //Console.WriteLine("\n" + "Implementation started...");
                //double epsilon = 1;
                //stopWatch.Start();
                //float timeOfLowBatt = new float();
                //float response_time_for_lowBatt = 15;
                //bool flag_implementation_done = false;

                //while (!flag_implementation_done)
                //{
                //while (!pAndv.flag_agv_low_batt)
                //{
                //    /*Send velocity commands to AGVs (velocity profiles of all movements are stored in allCollisionInfo.velocityProfile) */

                //    /*Event of Low Batt*/
                //    if (Math.Abs(stopWatch.ElapsedTimeSpan.TotalSeconds - 530) <= epsilon)
                //    {
                //        timeOfLowBatt = (float)stopWatch.ElapsedTimeSpan.TotalSeconds;
                //        pubAGVlowBatt.LowBatt(stopWatch.ElapsedTimeSpan.TotalSeconds);
                //        Console.WriteLine("Batt low event!!");
                //        flag_rescheduling_needed = pAndv.isReschedulingNeeded();
                //    }
                //}


                //if (flag_rescheduling_needed)
                //{
                //    /*Online planning*/

                //    flag_all_agv_commands_done = false;
                //    while (!flag_all_agv_commands_done)
                //    {
                //        CollisionInfoForPath[] collisionInfoLowBatt = pAndv.getCollisionInfoAfterLowBatt();
                //        List<int> IDsNewDurations = pAndv.tasksWithNewDuration();
                //        decimal[] startTimes = new decimal[collisionInfoLowBatt.Length];
                //        decimal[] finishingTimes = new decimal[collisionInfoLowBatt.Length];
                //        for (int i = 0; i < collisionInfoLowBatt.Length; i++)
                //        {
                //            startTimes[i] = (decimal)initialSchedule[initialSchedule.FindIndex(t => t.orderTaskIndex == collisionInfoLowBatt[i].taskID)].scheduledStartTime;
                //        }

                //        float executedUntil = timeOfLowBatt + response_time_for_lowBatt;

                //        for (int i = 0; i < initialSchedule.Count; i++)
                //        {
                //            if (initialSchedule[i].scheduledStartTime <= executedUntil)
                //            {
                //                initialSchedule[i].started = true;
                //                initialSchedule[i].finishedExecution = false;
                //                initialSchedule[i].remainingDuration = initialSchedule[i].scheduledEndTime - executedUntil;
                //            }

                //            if (initialSchedule[i].scheduledEndTime <= executedUntil)
                //            {
                //                initialSchedule[i].started = true;
                //                initialSchedule[i].finishedExecution = true;
                //                initialSchedule[i].remainingDuration = 0;
                //            }
                //        }


                //        decimal[] taskOriginalFinishingTimes = new decimal[IDsNewDurations.Count];
                //        for (int i = 0; i < IDsNewDurations.Count; i++)
                //        {
                //            taskOriginalFinishingTimes[i] = (decimal)initialSchedule[initialSchedule.FindIndex(t => t.orderTaskIndex == taskOriginalFinishingTimes[i])].scheduledEndTime;
                //            optimizer.reportNewRemainingTaskDuration(IDsNewDurations[i] / 10000, IDsNewDurations[i] % 10000, (float)collisionInfoLowBatt[Array.FindIndex(collisionInfoLowBatt, t => t.taskID == IDsNewDurations[i])].pathFinishingTime -
                //                initialSchedule[initialSchedule.FindIndex(t => t.orderTaskIndex == IDsNewDurations[i])].scheduledStartTime, initialSchedule, timeOfLowBatt + response_time_for_lowBatt);

                //        }
                //        float responseTime = 15; //for rescheduling 
                //        executedSchedule = optimizer.addResponseTimeExecuteRunningNoStarts(executedSchedule, executedUntil, responseTime);
                //        optimizer.setCurrentStateOfExecution(executedSchedule, executedUntil, responseTime);  // updating RTN model

                //        List<Schedules.SCHEDULEENTRY> newSchedule = optimizer.reSchedule();
                //        StreamWriter new_schedule = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\online_schedule.log", false);

                //        new_schedule.WriteLine("# TAOpt4.3.0 " + DateTime.Now + " by S. Panek <sebastian.panek@udo.edu> and C. Schoppmeyer <christian.schoppmeyer@bci.tu-dortmund.de> , BCI, TU Dortmund University,");
                //        new_schedule.WriteLine("# Starting optimization of model Test at " + DateTime.Now);
                //        new_schedule.WriteLine("\n");

                //        List<Schedules.SCHEDULEENTRY> scheduleForGrouping = new List<Schedules.SCHEDULEENTRY>();
                //        for (int i = 0; i < newSchedule.Count; i++)
                //        {
                //            newSchedule[i].scheduledStartTime += executedUntil + responseTime;
                //            newSchedule[i].scheduledEndTime += executedUntil + responseTime;
                //            bool movement_task_done = false;

                //            /* If the task is in taskIDsNewDuration it means the movement task took longer than planned, the movement task is considered as done*/
                //            for (int j = 0; j < IDsNewDurations.Count; j++)
                //                if (newSchedule[i].orderTaskIndex == IDsNewDurations[j])
                //                {
                //                    newSchedule[i].finishedExecution = true;
                //                    movement_task_done = true;
                //                }
                //            /*Consider the part of the schedule after duration Changes*/
                //            if (!movement_task_done)
                //            {
                //                new_schedule.WriteLine(newSchedule[i].resourceIndex + "\t" + newSchedule[i].orderTaskIndex + "\t" + newSchedule[i].scheduledStartTime + "\t" + newSchedule[i].scheduledEndTime + "\t" + 1);
                //                scheduleForGrouping.Add(newSchedule[i]);
                //            }
                //        }

                //        path = AppDomain.CurrentDomain.BaseDirectory + "\\online_schedule.log";
                //        AGVusedForRecipeID = agvData.getUsedAGVs();
                //        executedSchedule = newSchedule;
                //        flag_rescheduling_needed = false;




                //        agvData.initialAGVmovementInfo = previousAGVmovementInfo;
                //        agvData = new AGVMovmentData(AGVusedForRecipeID,previousAGVmovementInfo,path);
                //        previousAGVmovementInfo = agvData.agvMovementInfo;

                //        /*Get the last movement task*/
                //        Schedules.SCHEDULEENTRY lastMovementTask = getLastMovementTask(executedSchedule, agvData.agvMovementInfo);
                //        /**/

                //        allStartTimes = new decimal[agvData.agvMovementInfo.Count];
                //        for (int i = 0; i < allStartTimes.Length; i++)
                //            allStartTimes[i] = agvData.agvMovementInfo[i].startTime;

                //        pAndv = new PAndVPlanning(agvData, allStartTimes);
                //        pAndv.Init();

                //        agvCalculatedGroups = agvData.getGroupOfCommandsWithPossiblePathCollisions();

                //        while (!flag_rescheduling_needed)
                //        {

                //            for (int i = 0; i < agvCalculatedGroups.Count; i++)
                //            {

                //                /*Updating executed schedule*/
                //                for (int j = 0; j < executedSchedule.Count; j++)
                //                {
                //                    for (int l = 0; l < agvCalculatedGroups[i].Count; l++)
                //                    {
                //                        if (agvCalculatedGroups[i][l].taskID > 100000)
                //                        {
                //                            if (executedSchedule[j].orderTaskIndex == (agvCalculatedGroups[i][l].taskID) / 10)
                //                            {
                //                                executedSchedule[j].started = true;
                //                                executedSchedule[j].finishedExecution = false;
                //                                break;
                //                            }
                //                        }
                //                        else
                //                        {
                //                            if (executedSchedule[j].orderTaskIndex == agvCalculatedGroups[i][l].taskID)
                //                            {
                //                                executedSchedule[j].started = true;
                //                                executedSchedule[j].finishedExecution = false;
                //                                break;
                //                            }
                //                        }
                //                        if (executedSchedule[j].scheduledStartTime < (float)agvCalculatedGroups[i][l].startTime)
                //                        {
                //                            executedSchedule[j].started = true;
                //                            executedSchedule[j].finishedExecution = true;
                //                            executedSchedule[j].remainingDuration = 0;
                //                            break;

                //                        }
                //                    }


                //                }
                //                /**/
                //                List<List<Position>> oneGroupOfPaths = new List<List<Position>>();
                //                List<decimal> durations = new List<decimal>();

                //                /* Get respective A* Paths */
                //                for (int j = 0; j < agvCalculatedGroups[i].Count; j++)
                //                {
                //                    for (int k = 0; k < agvData.agvMovementInfo.Count; k++)
                //                    {
                //                        if (agvCalculatedGroups[i][j].taskID == agvData.agvMovementInfo[k].taskID && agvCalculatedGroups[i][j].startPosition == agvData.agvMovementInfo[k].startPosition
                //                            && agvCalculatedGroups[i][j].endPosition == agvData.agvMovementInfo[k].endPosition)
                //                        {
                //                            oneGroupOfPaths.Add(pAndv.allPaths[k]);
                //                            durations.Add(pAndv.allPathDurations[k]);
                //                            Console.WriteLine("duration: " + durations[j]);
                //                            break;
                //                        }
                //                    }
                //                }

                //                /**/
                //                decimal[] startingTimes = new decimal[agvCalculatedGroups[i].Count];
                //                for (int k = 0; k < agvCalculatedGroups[i].Count; k++)
                //                {
                //                    startingTimes[k] = agvCalculatedGroups[i][k].startTime;
                //                    agvCalculatedGroups[i][k].endTime = durations[k] + startingTimes[k]; // update the end times
                //                    Console.WriteLine("endTimes[" + k + "] = " + agvCalculatedGroups[i][k].endTime);
                //                }

                //                CollisionInfoForPath[] collisionInfo = pAndv.calculateOneGroupOfPaths(agvCalculatedGroups[i], oneGroupOfPaths, startTimes, durations);
                //                allCollisionInfos.Add(collisionInfo);

                //                /*AGV info for implementation*/
                //                for (int p = 0; p < collisionInfo.Length; p++)
                //                {
                //                    AGVData agv_data = new AGVData(collisionInfo[p].taskID, collisionInfo[p].usedAGV, agvCalculatedGroups[i][p].startTime, collisionInfo[p].pathFinishingTime, agvCalculatedGroups[i][p].startPosition,
                //                        agvCalculatedGroups[i][p].endPosition);
                //                    AGVfullInfo.Add(agv_data);
                //                }
                //                /**/

                //                /* Update flag rescheduling */
                //                flag_rescheduling_needed = pAndv.isReschedulingNeeded();  /* isReschedulingNeeded returns the value of flag rescheduling in PAndVPlanning class */
                //                if (flag_rescheduling_needed)
                //                {
                //                    index_rescheduling = allCollisionInfos.Count - 1; //test
                //                    break;
                //                }

                //            }

                //            /* Check if the last movement task has started. This means the motion planning is done*/
                //            if (lastMovementTask.started == true)
                //            {
                //                Console.WriteLine("Last Movement task ID : " + lastMovementTask.orderTaskIndex);
                //                /* Motion planning finished!! */
                //                flag_rescheduling_needed = false;
                //                flag_all_agv_commands_done = true;
                //                break;
                //            }
                //        }
                //    }

                //    /*Implement the calculated paths and Velocity profiles on AGVs*/

                //}  

                //}
                #endregion
            }


        public static CollisionInfoForPath[][] MakeAllVelocityProfilesEqual(CollisionInfoForPath[][] InitialCollisionInfoArray,string[] AvailableAGVs) // why must it be static
        {
            CollisionInfoForPath[][] ArrayCollisonInfo = new CollisionInfoForPath[InitialCollisionInfoArray.GetLength(0)][]; // construct a Strong array with the number of groups as the row and the AGV number as the coloumn
            int LongestVelocityProfile = 0;
            int indexOfLongestVp = -1;
            int NumberOfAGVsInCurrentGroup = 0;
            int AVGnumber = 0;
            int numberOfAGVs = AvailableAGVs.Length;
            int[] IndexOfSelectedAgvs = new int[AvailableAGVs.Length];
            // Loop for extracting the Selected AGV numbers From the input strings
            for (int SelectedAgvMaping = 0; SelectedAgvMaping < IndexOfSelectedAgvs.Length; SelectedAgvMaping++)
            {
                IndexOfSelectedAgvs[SelectedAgvMaping] = Convert.ToInt32(AvailableAGVs[SelectedAgvMaping][AvailableAGVs[SelectedAgvMaping].Length - 1].ToString());
            }
            decimal TimeInterval = InitialCollisionInfoArray[0][0].velocityProfile[1, 0] - InitialCollisionInfoArray[0][0].velocityProfile[0, 0];
            decimal TimeIncrement = 0;
            for (int GroupNumber = 0; GroupNumber < InitialCollisionInfoArray.Length; GroupNumber++) // loop through Each group
            {
                NumberOfAGVsInCurrentGroup = InitialCollisionInfoArray[GroupNumber].Length;
                ArrayCollisonInfo[GroupNumber] = new CollisionInfoForPath[numberOfAGVs];
                for (int CurretnAGVinGroup = 0; CurretnAGVinGroup < NumberOfAGVsInCurrentGroup; CurretnAGVinGroup++) // loop through members of the group
                {
                    if (LongestVelocityProfile < InitialCollisionInfoArray[GroupNumber][CurretnAGVinGroup].velocityProfile.GetLength(0))
                    {
                        LongestVelocityProfile = InitialCollisionInfoArray[GroupNumber][CurretnAGVinGroup].velocityProfile.GetLength(0); // find longest velocity profile in group
                        indexOfLongestVp = CurretnAGVinGroup;
                    }
                    AVGnumber = Convert.ToInt32(InitialCollisionInfoArray[GroupNumber][CurretnAGVinGroup].usedAGV[InitialCollisionInfoArray[GroupNumber][CurretnAGVinGroup].usedAGV.Length - 1].ToString()); // find AGV number
                    bool Assigned = false;
                    for (int AssingmentOfAgv = 0; AssingmentOfAgv < numberOfAGVs; AssingmentOfAgv++)
                    {
                        if (AVGnumber == IndexOfSelectedAgvs[AssingmentOfAgv])
                        {
                            ArrayCollisonInfo[GroupNumber][AssingmentOfAgv] = InitialCollisionInfoArray[GroupNumber][CurretnAGVinGroup];
                            Assigned = true;
                        }
                    }
                    if (!Assigned)
                    {
                        MessageBox.Show("Found an AGV which is not From the selected AGVs in the GUI :" + " ProblemInRouterSection");
                    }
                    AVGnumber = 0; // reset the AGV number
                }
                for (int i = 0; i < numberOfAGVs; i++) // Check The number of AGvs and if not definde Define them
                {
                    if (ArrayCollisonInfo[GroupNumber][i] == null)
                    {
                        ArrayCollisonInfo[GroupNumber][i] = new CollisionInfoForPath();
                        ArrayCollisonInfo[GroupNumber][i].usedAGV = "AGV " + IndexOfSelectedAgvs[i].ToString(); // staically programed part again Due to the i+1
                        ArrayCollisonInfo[GroupNumber][i].velocityProfile = new decimal[LongestVelocityProfile, 2]; // full of zeros TimeInterval
                        TimeIncrement = InitialCollisionInfoArray[GroupNumber][indexOfLongestVp].velocityProfile[0, 0]; // start time of the AGV with the longest velocity profile
                        for (int j = 0; j < LongestVelocityProfile; j++) // Time Update
                        {
                            ArrayCollisonInfo[GroupNumber][i].velocityProfile[j, 0] = TimeIncrement;
                            TimeIncrement = TimeIncrement + TimeInterval;
                        }
                        TimeIncrement = 0;
                    }
                    
                    else // here we update the orientations and locations to correspond to the velocities (this else corresponds to the defined groups)
                    {
                        int OriginalVelProfLength = ArrayCollisonInfo[GroupNumber][i].velocityProfile.GetLength(0);
                        
                        int OrigPointListLength = ArrayCollisonInfo[GroupNumber][i].path.Count;
                        int OrigOrientationListLength = ArrayCollisonInfo[GroupNumber][i].allOrientations.Count;
                        List<Position> ExtendedPosition = new List<Position>();
                        List<double> ExtendedOrientations = new List<double>();
                        if(OrigPointListLength > 2 && OrigOrientationListLength>2) 
                        {
                            int FloorOfDivisionPosition = Convert.ToInt32(Math.Floor(Convert.ToDouble(OriginalVelProfLength / (OrigPointListLength-1)))); // the -1 is the removal of the 1st and last point will be fixed manually and the rest will be extended
                            int FloorOfDivisionOrientaion = Convert.ToInt32(Math.Floor(Convert.ToDouble(OriginalVelProfLength / (OrigOrientationListLength-1))));
                            // Add the first Elemetns to the Lists Manually To fix them
                            
                            // Orientation Extend 
                            for (int OrientationExtend = 0; OrientationExtend < OriginalVelProfLength-1; OrientationExtend++)// -1 leave space for the last
                            {
                                int IndexOfOriginalOrientationList = Convert.ToInt32(Math.Floor(Convert.ToDouble(OrientationExtend/FloorOfDivisionOrientaion)));
                                if (IndexOfOriginalOrientationList < OrigOrientationListLength - 1)
                                {
                                    ExtendedOrientations.Add(ArrayCollisonInfo[GroupNumber][i].allOrientations[IndexOfOriginalOrientationList]);
                                }
                                else
                                {
                                    ExtendedOrientations.Add(ArrayCollisonInfo[GroupNumber][i].allOrientations[OrigOrientationListLength-2]);// Add the Element Before Last(-2) since we finished early due to floor
                                }
                            }
                            ExtendedOrientations.Add(ArrayCollisonInfo[GroupNumber][i].allOrientations[OrigOrientationListLength-1]);// last element Final Orientation
                            ArrayCollisonInfo[GroupNumber][i].allOrientations = ExtendedOrientations;
                            // Position Extend
                            for (int PointExtend = 0; PointExtend < OriginalVelProfLength-1; PointExtend++)
                            {
                                int IndexOfOriginalPositionList = Convert.ToInt32(Math.Floor(Convert.ToDouble(PointExtend/FloorOfDivisionPosition)));
                                if (IndexOfOriginalPositionList < OrigPointListLength - 1)
                                {
                                    ExtendedPosition.Add(ArrayCollisonInfo[GroupNumber][i].path[IndexOfOriginalPositionList]);
                                }
                                else
                                {
                                    ExtendedPosition.Add(ArrayCollisonInfo[GroupNumber][i].path[OrigPointListLength-2]);// Add the Element Before Last(-2) since we finished early due to floor
                                }
                            }
                            ExtendedPosition.Add(ArrayCollisonInfo[GroupNumber][i].path[OrigPointListLength-1]);// last element Final Position
                            ArrayCollisonInfo[GroupNumber][i].path = ExtendedPosition;

                        }
                        else
                        {
                            MessageBox.Show("The Extending of the Position and Orienttion FAILS Check 'SchedulingPathPlanning.cs' ");
                        }
                        

                    }
                    
                }
                ArrayCollisonInfo[GroupNumber] = EqualizeVelocityProfilesforAgroup(ArrayCollisonInfo[GroupNumber], LongestVelocityProfile, numberOfAGVs);
                LongestVelocityProfile = 0;
                NumberOfAGVsInCurrentGroup = 0;
            }
            return ArrayCollisonInfo;
        }
        public static CollisionInfoForPath[] EqualizeVelocityProfilesforAgroup(CollisionInfoForPath[] InputGroup, int VprofileLength, int numberOfAGVs)
        {
            decimal timeIncrement = 0;
            decimal LastTime = 0;
            int DiffrenceOfLength = 0;
            for (int CurrentAGV = 0; CurrentAGV < numberOfAGVs; CurrentAGV++)
            {
                if (InputGroup[CurrentAGV].velocityProfile.GetLength(0) < VprofileLength) // if true the AGV's velocity profile must be padded with zeros
                {
                    timeIncrement = InputGroup[CurrentAGV].velocityProfile[1, 0] - InputGroup[CurrentAGV].velocityProfile[0, 0];
                    LastTime = InputGroup[CurrentAGV].velocityProfile[InputGroup[CurrentAGV].velocityProfile.GetLength(0) - 1, 0];
                    decimal[,] NewVelocityProfile = new decimal[VprofileLength, 2];// new velocity profile
                    Array.Copy(InputGroup[CurrentAGV].velocityProfile, NewVelocityProfile, InputGroup[CurrentAGV].velocityProfile.Length); // copy old velocity profile
                    DiffrenceOfLength = VprofileLength - InputGroup[CurrentAGV].velocityProfile.GetLength(0);
                    for (int i = 0; i < DiffrenceOfLength; i++)
                    {
                        LastTime = LastTime + timeIncrement;
                        NewVelocityProfile[i + InputGroup[CurrentAGV].velocityProfile.GetLength(0), 0] = LastTime;
                        InputGroup[CurrentAGV].allOrientations.Add(InputGroup[CurrentAGV].allOrientations.Last());
                        InputGroup[CurrentAGV].path.Add(InputGroup[CurrentAGV].path.Last());
                    }
                    InputGroup[CurrentAGV].velocityProfile = NewVelocityProfile; // uodate the velocity profile
                }
                LastTime = 0;
                DiffrenceOfLength = 0;
            }
            return InputGroup;
        }
        public CollisionInfoForPath[][] GetFinalVelocityProfileStructures()
        {
            return FinalVelocityProfileStructures;
        }
    }
}
