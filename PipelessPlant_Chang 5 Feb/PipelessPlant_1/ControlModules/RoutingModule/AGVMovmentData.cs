using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MULTIFORM_PCS.ControlModules.RoutingModule
{
    class AGVMovmentData
    {
        public string path;
        public String pathToScheduleLog = AppDomain.CurrentDomain.BaseDirectory;
        public StreamReader scheduleReader;
        public StreamReader sequenceReader;
        List<string> seguences_before_split = new List<string>();
        List<string> lastSchedule = new List<string>();

        List<AGVData> AGVInfo = new List<AGVData>();
        public List<string[]> sequences = new List<string[]>();
        public List<string[]> bestSchedule = new List<string[]>();
        public List<List<AGVData>> agvGroups = new List<List<AGVData>>();
        public List<AGVData> agvMovementInfo = new List<AGVData>();
        public AGVData lastMovementTask;
        public string[] AvailableAGVs = new string[] { "", "", "" };  // of each recipe
        public string[] AGVusedForRecipeID = new string[] { "", "", "", "", "", "" };
        public List<AGVData> initialAGVmovementInfo = new List<AGVData>();

        /* Timing information */

        public int AGVMovementTime = 15;
        public int dockingTime = 10;
        public int undockingTime = 5;
        public int mixingGrabTime = 36;
        public int mixingReleaseTime = 39;
        public int storageRelease = 74;
        public int storageGrab = 87;

        public AGVMovmentData(string[] AvailableAGVs, List<AGVData> initialAGVmovementInfo, string path)//string path, string[] AGVusedForRecipeID, List<AGVData> initialAGVmovementInfo)
        {
            this.initialAGVmovementInfo = initialAGVmovementInfo;
            this.AvailableAGVs = AvailableAGVs;
            this.path = path;

            //init(path);
        }

        public void init(string path)
        {
            scheduleReader = new StreamReader(path, false);
            sequenceReader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\sequence_mapping.log", false);
            readSchedules(path);
            agvMovementInfo = getAGVMovementData();
            agvGroups = getGroupOfCommandsWithPossiblePathCollisions(AvailableAGVs);//getAGVgroupsOfMovement(agvMovementInfo);
            if (agvMovementInfo.Count != 0)
                this.lastMovementTask = agvMovementInfo[agvMovementInfo.Count - 1];
        }


        public void updateAgvMovementInfo(List<AGVData> newAGVmovementInfo)
        {
            agvMovementInfo = newAGVmovementInfo;
        }



        public void readSchedules(string path)
        {
            /** Read sequence_mapping.log **/
            List<string> lines_for_sequence = new List<string>();
            while (!sequenceReader.EndOfStream)
            {
                string line = sequenceReader.ReadLine();
                if (line != "")
                {
                    lines_for_sequence.Add(line);
                    sequences.Add(line.Split('\t', ' '));
                }
            }
            seguences_before_split = lines_for_sequence.GetRange(0, lines_for_sequence.Count);
            sequenceReader.Close();

            /** Read schedules.log **/
            List<string> lines = new List<string>();
            while (!scheduleReader.EndOfStream)
            {
                string line = scheduleReader.ReadLine();
                if (line != "")
                {
                    lines.Add(line);
                }
            }
            scheduleReader.Close();

            if (lines.Count > 2)
            {
                int end = lines.Count - 1;
                int start = 0;
                for (int i = lines.Count - 2; i >= 0; i--)
                {
                    if (lines[i] == "#" || lines[i].Contains("#"))
                    {
                        start = i;
                        break;
                    }
                }
                lastSchedule = lines.GetRange(start + 1, lines.Count - 1 - (start + 1));
            }
        }

        public string[] getUsedAGVs()
        {
            return AGVusedForRecipeID;
        }

        public List<AGVData> getAGVMovementData()
        {

            StreamWriter agvInfo = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "agv_info.log", false);
            List<string[]> sortedSchedule = getSortedSchedule();
            //Console.WriteLine(sequences[0][5]);

            int[] previousRecipeID = new int[] { -1, -1, -1, -1, -1 };
            string agvUsed = "";

            bool agvAssigned = false;

            if (sortedSchedule != null)
            {
                for (int i = 0; i < sortedSchedule.Count; i++)
                {
                    int taskID = int.Parse(sortedSchedule[i][1]);
                    int recipeID = taskID / 10000;

                    //if (startTime >= AGVMovementTime)
                    //    continue;  // movement task is finished

                    for (int j = 0; j < sequences.Count; j++)
                    {
                        if (int.Parse(sequences[j][3]) == taskID && sequences[j][5] != "DUMMY" && sequences[j][5] != "HARDEN") //&& (sequences[j][5].Contains("MOVE") || sequences[j][5].Contains("OTHER")))
                        {


                            string[] agvMovementTimes = sequences[j][6].Split('+');
                            //AGVMovementTime = int.Parse(agvMovementTimes[0]);

                            if (AGVusedForRecipeID[recipeID - 1] != "")//&& sequences[j][5] != "RELEASE+UNDOCK")
                            {
                                agvUsed = AGVusedForRecipeID[recipeID - 1];
                                agvAssigned = true;
                            }
                            if (sequences[j][5] == "RELEASE+UNDOCK")
                            {
                                agvUsed = getAGVatStation(getPreviousTaskDifferentRecipe(taskID, sortedSchedule), "STORAGE", AGVInfo);
                                AGVusedForRecipeID[recipeID - 1] = agvUsed;
                                agvAssigned = true;
                            }

                            if (!agvAssigned)
                            {
                                if (sequences[j][4] == "3001")
                                {
                                    //Console.WriteLine(sequences[j][3].Substring(4, 1));
                                    if (sequences[j][3].Substring(4, 1) == "1" || sequences[j][3].Substring(4, 1) == "2")
                                    {
                                        AGVusedForRecipeID[recipeID - 1] = AvailableAGVs[0]; //"AGV1";
                                        agvUsed = AGVusedForRecipeID[recipeID - 1];
                                    }
                                    else if (sequences[j][3].Substring(4, 1) == "3" || sequences[j][3].Substring(4, 1) == "4")
                                    {
                                        AGVusedForRecipeID[recipeID - 1] = AvailableAGVs[1]; //"AGV2";
                                        agvUsed = AGVusedForRecipeID[recipeID - 1];
                                    }
                                    else if (sequences[j][3].Substring(4, 1) == "5" || sequences[j][3].Substring(4, 1) == "6")
                                    {
                                        AGVusedForRecipeID[recipeID - 1] = AvailableAGVs[2]; //"AGV3";
                                        agvUsed = AGVusedForRecipeID[recipeID - 1];
                                    }
                                }
                            }

                            if (sequences[j][5].Contains("MOVE"))
                            {
                                string[] agvStartAndEndPlace = sequences[j][5].Split('_', '+');

                                if (!agvStartAndEndPlace.Contains("OTHER") && agvStartAndEndPlace.Contains("MOVE"))
                                {

                                    string agvStartPlace = agvStartAndEndPlace[1];
                                    string agvEndPlace = agvStartAndEndPlace[3];
                                    //Console.WriteLine(agvStartPlace);
                                    //Console.WriteLine(agvEndPlace);
                                    AGVData agvData = new AGVData(taskID, agvUsed, decimal.Parse(sortedSchedule[i][2]), decimal.Parse(sortedSchedule[i][2]) + AGVMovementTime, agvStartPlace, agvEndPlace);//, sequences[j][5]);
                                    AGVInfo.Add(agvData);
                                    agvInfo.WriteLine(taskID + "\t" + agvData.usedAGV + "\t" + agvData.startTime + "\t" + agvData.endTime + "\t" + agvData.startPosition + "\t" + agvData.endPosition);

                                }
                                else if (agvStartAndEndPlace.Contains("OTHER"))
                                {
                                    string agvStartPlace = agvStartAndEndPlace[1];
                                    string agvEndPlace = agvStartAndEndPlace[3];
                                    AGVData agvData = new AGVData(taskID, agvUsed, decimal.Parse(sortedSchedule[i][2]), decimal.Parse(sortedSchedule[i][2]) + AGVMovementTime, agvStartPlace, agvEndPlace);//, sequences[j][5]);
                                    AGVInfo.Add(agvData);
                                    //Console.WriteLine(taskID + "\t" + agvData.usedAGV + "\t" + agvData.startTime + "\t" + agvData.endTime + "\t" + agvData.startPosition + "\t" + agvData.endPosition);

                                    agvInfo.WriteLine(taskID + "\t" + agvData.usedAGV + "\t" + agvData.startTime + "\t" + agvData.endTime + "\t" + agvData.startPosition + "\t" + agvData.endPosition);

                                    agvUsed = getPreviousAGVatStation(taskID, "STORAGE", sortedSchedule, AGVInfo, initialAGVmovementInfo);
                                    string agvStartPlace_nextAGV = agvStartAndEndPlace[3];
                                    string agvEndPlace_nextAGV = agvStartAndEndPlace[8];
                                    if (agvEndPlace_nextAGV == "INIT")
                                    {
                                        if (agvUsed == "AGV1")
                                            agvEndPlace_nextAGV = "INIT1";
                                        else if (agvUsed == "AGV2")
                                            agvEndPlace_nextAGV = "INIT2";
                                        else if (agvUsed == "AGV3")
                                            agvEndPlace_nextAGV = "INIT3";

                                    }
                                    AGVData agvData_nextAGV = new AGVData(int.Parse(taskID.ToString() + "0"), agvUsed, decimal.Parse(sortedSchedule[i][2]), decimal.Parse(sortedSchedule[i][3]), agvStartPlace_nextAGV, agvEndPlace_nextAGV);//, sequences[j][5]);
                                    AGVInfo.Add(agvData_nextAGV);
                                    //Console.WriteLine(taskID + "\t" + agvData_nextAGV.usedAGV + "\t" + agvData_nextAGV.startTime + "\t" + agvData_nextAGV.endTime + "\t" + agvData_nextAGV.startPosition + "\t" + agvData_nextAGV.endPosition);
                                    agvInfo.WriteLine(taskID + "\t" + agvData_nextAGV.usedAGV + "\t" + agvData_nextAGV.startTime + "\t" + agvData_nextAGV.endTime + "\t" + agvData_nextAGV.startPosition + "\t" + agvData_nextAGV.endPosition);
                                }
                            }
                            agvAssigned = false;
                            break;
                        }
                    }
                }
            }

            agvInfo.Flush();
            agvInfo.Close();

            return AGVInfo;
        }

        public string[] getAGVusedForRecipeID()
        {
            return AGVusedForRecipeID;
        }


        public List<List<AGVData>> getAGVgroupsOfMovement(List<AGVData> agvMovementInfo)
        {
            int i = 0;//1;
            int step = 0;
            decimal timeStep = 100;
            int index = 0;

            ///List<List<AGVData>> agvGroups = new List<List<AGVData>>();
            List<AGVData> agvSingleGroup = new List<AGVData>();
            StreamWriter agv_groups = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\agv_movement_groups.log", false);  // includes information of groups of paths which need to be planned

            AGVData firstMovementInfo = agvMovementInfo[0];


            // add first group 
            List<AGVData> firstGroup = new List<AGVData>();
            //firstGroup.Add(firstMovementInfo);

            //agvGroups.Add(firstGroup);
            //agv_groups.WriteLine("Group " + step);
            //agv_groups.WriteLine(firstMovementInfo.taskID + "\t" + firstMovementInfo.usedAGV + "\t" + firstMovementInfo.startTime + "\t" + firstMovementInfo.endTime + "\t" + firstMovementInfo.startPosition + "\t" + firstMovementInfo.endPosition);


            while (i != agvMovementInfo.Count)
            {
                while (i != agvMovementInfo.Count && agvMovementInfo[i].startTime <= (timeStep * (step + 1)) && agvMovementInfo[i].startTime >= (timeStep * step))
                {
                    agvSingleGroup.Add(agvMovementInfo[i]);
                    //Console.WriteLine(agvMovementInfo[i].taskID + "\t" + agvMovementInfo[i].usedAGV + "\t" + agvMovementInfo[i].startTime);
                    i++;
                    if (i == agvMovementInfo.Count)
                        break;
                }

                if (agvSingleGroup.Count != 0)
                {
                    //if (agvSingleGroup.Count == 4)
                    //    Console.WriteLine("4 memebers in one AGVGroup!!!");
                    agv_groups.WriteLine("Group " + (index));
                    agvGroups.Add(agvSingleGroup);
                    for (int j = 0; j < agvSingleGroup.Count; j++)
                    {
                        agv_groups.WriteLine(agvSingleGroup[j].taskID + "\t" + agvSingleGroup[j].usedAGV + "\t" + agvSingleGroup[j].startTime + "\t" + agvSingleGroup[j].endTime + "\t" + agvSingleGroup[j].startPosition + "\t" + agvSingleGroup[j].endPosition);
                    }
                    agvSingleGroup = new List<AGVData>(); // reset agvSingleGroupe
                    index++;
                }
                step++;
            }
            agv_groups.Flush();
            agv_groups.Close();

            return agvGroups;

        }

        public List<List<AGVData>> groupCommandsByAGV(List<AGVData> agvMovementInfo, string[] AvailableAGVs)
        {
            List<AGVData> agv1Commands = new List<AGVData>();
            List<AGVData> agv2Commands = new List<AGVData>();
            List<AGVData> agv3Commands = new List<AGVData>();

            StreamWriter firstAGV_commands = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\1stAGV_commands.log", false);
            StreamWriter secondAGV_commands = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\2ndAGV_commands.log", false);
            StreamWriter thirdAGV_commands = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\3rdAGV_commands.log", false);

            List<List<AGVData>> commandsByAGV = new List<List<AGVData>>();

            for (int i = 0; i < agvMovementInfo.Count; i++)
            {
                if (agvMovementInfo[i].usedAGV == AvailableAGVs[0])
                {
                    agv1Commands.Add(agvMovementInfo[i]);
                    firstAGV_commands.WriteLine(agvMovementInfo[i].taskID + "\t" + agvMovementInfo[i].usedAGV + "\t" + agvMovementInfo[i].startTime + "\t" +
                                            agvMovementInfo[i].endTime + "\t" + agvMovementInfo[i].startPosition + "\t" + agvMovementInfo[i].endPosition);
                }
                else if (agvMovementInfo[i].usedAGV == AvailableAGVs[1])
                {
                    agv2Commands.Add(agvMovementInfo[i]);
                    secondAGV_commands.WriteLine(agvMovementInfo[i].taskID + "\t" + agvMovementInfo[i].usedAGV + "\t" + agvMovementInfo[i].startTime + "\t" +
                                            agvMovementInfo[i].endTime + "\t" + agvMovementInfo[i].startPosition + "\t" + agvMovementInfo[i].endPosition);
                }
                else if (agvMovementInfo[i].usedAGV == AvailableAGVs[2])
                {
                    agv3Commands.Add(agvMovementInfo[i]);
                    thirdAGV_commands.WriteLine(agvMovementInfo[i].taskID + "\t" + agvMovementInfo[i].usedAGV + "\t" + agvMovementInfo[i].startTime + "\t" +
                                            agvMovementInfo[i].endTime + "\t" + agvMovementInfo[i].startPosition + "\t" + agvMovementInfo[i].endPosition);
                }
            }

            commandsByAGV.Add(agv1Commands); commandsByAGV.Add(agv2Commands); commandsByAGV.Add(agv3Commands);

            firstAGV_commands.Flush(); firstAGV_commands.Close();
            secondAGV_commands.Flush(); secondAGV_commands.Close();
            thirdAGV_commands.Flush(); thirdAGV_commands.Close();

            return commandsByAGV;
        }

        public List<List<AGVData>> getGroupOfCommandsWithPossiblePathCollisions(string[] AvailableAGVs)
        {
            /* This function outputs a group of commands which might have a collision in their paths
             the commands from different agvs that might start together or share a time interval during their execution */

            List<List<AGVData>> commandsGroupedByUsedAGV = groupCommandsByAGV(agvMovementInfo, AvailableAGVs);
            List<List<AGVData>> groupsWithPossibleCollisions = new List<List<AGVData>>();
            List<AGVData> alreadyAddedCommands = new List<AGVData>();  /* to check if all the commands in agvMovementInfo are added */

            StreamWriter agv_cmds = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\agv_commands_grouped.log", false);

            for (int i = 0; i < commandsGroupedByUsedAGV[0].Count; i++) /*AGV1 commands*/
            {
                List<AGVData> singleGroup = new List<AGVData>();

                singleGroup.Add(commandsGroupedByUsedAGV[0][i]);
                alreadyAddedCommands.Add(commandsGroupedByUsedAGV[0][i]);

                for (int j = 0; j < commandsGroupedByUsedAGV[1].Count; j++)  /*AGV2 commands*/
                {
                    if ((commandsGroupedByUsedAGV[1][j].startTime >= commandsGroupedByUsedAGV[0][i].startTime &&
                        commandsGroupedByUsedAGV[1][j].startTime <= commandsGroupedByUsedAGV[0][i].endTime) ||

                        commandsGroupedByUsedAGV[1][j].startTime == commandsGroupedByUsedAGV[0][i].endTime)
                    {
                        singleGroup.Add(commandsGroupedByUsedAGV[1][j]);
                        alreadyAddedCommands.Add(commandsGroupedByUsedAGV[1][j]);
                        break;
                    }
                }

                for (int k = 0; k < commandsGroupedByUsedAGV[2].Count; k++) /*AGV3 commands*/
                {
                    if ((commandsGroupedByUsedAGV[2][k].startTime >= commandsGroupedByUsedAGV[0][i].startTime &&
                        commandsGroupedByUsedAGV[2][k].startTime <= commandsGroupedByUsedAGV[0][i].endTime) ||

                        commandsGroupedByUsedAGV[2][k].startTime == commandsGroupedByUsedAGV[0][i].endTime)
                    {
                        singleGroup.Add(commandsGroupedByUsedAGV[2][k]);
                        alreadyAddedCommands.Add(commandsGroupedByUsedAGV[2][k]);
                        break;
                    }

                }
                groupsWithPossibleCollisions.Add(singleGroup);
            }

            /* Check if there is a missing command */
            int number_of_missing_commands = agvMovementInfo.Count - alreadyAddedCommands.Count;
            List<AGVData> missingCommands = new List<AGVData>();

            if (number_of_missing_commands != 0)
            {
                for (int i = 0; i < agvMovementInfo.Count; i++)
                {
                    AGVData result = alreadyAddedCommands.Find(
                        delegate(AGVData command)
                        {
                            return command.taskID == agvMovementInfo[i].taskID;
                        }
                    );

                    if (result == null)
                    {
                        missingCommands.Add(agvMovementInfo[i]);
                        if (missingCommands.Count == number_of_missing_commands)
                            break;
                    }
                }
                /*Check if the missing command belongs to any of the already calculated groups*/
                for (int i = 0; i < groupsWithPossibleCollisions.Count; i++)
                {
                    for (int k = 0; k < groupsWithPossibleCollisions[i].Count; k++)
                    {
                        for (int j = 0; j < missingCommands.Count; j++)
                        {
                            if ((groupsWithPossibleCollisions[i][k].startTime >= missingCommands[j].startTime &&
                                groupsWithPossibleCollisions[i][k].startTime <= missingCommands[j].endTime) ||
                                missingCommands[j].startTime == groupsWithPossibleCollisions[i][k].endTime)
                            {
                                groupsWithPossibleCollisions[i].Add(missingCommands[j]);
                                missingCommands.Remove(missingCommands[j]);
                            }

                        }
                    }
                }

                if (missingCommands.Count != 0)
                {
                    /* Group the remaining commands*/

                    for (int i = 0; i < missingCommands.Count; i++)
                    {
                        List<AGVData> singleGroup = new List<AGVData>();
                        if (!alreadyAddedCommands.Contains(missingCommands[i]))
                        {
                            singleGroup.Add(missingCommands[i]);
                            alreadyAddedCommands.Add(missingCommands[i]);

                            for (int j = i + 1; j < missingCommands.Count; j++)
                                //if (i != j)
                                //{
                                if (missingCommands[i].usedAGV != missingCommands[j].usedAGV && !alreadyAddedCommands.Contains(missingCommands[j]))
                                {
                                    if ((missingCommands[j].startTime >= missingCommands[i].startTime &&
                                    missingCommands[j].startTime <= missingCommands[i].endTime) ||
                                    missingCommands[j].startTime == missingCommands[i].endTime)
                                    {
                                        singleGroup.Add(missingCommands[j]);
                                        alreadyAddedCommands.Add(missingCommands[j]);
                                    }
                                }
                            //}
                            /*Find the correct index to insert the missingCommand*/
                            int index = -1;
                            for (int l = 0; l < groupsWithPossibleCollisions.Count; l++)
                            {
                                //for (int m = 0; m < groupsWithPossibleCollisions[l].Count; m++)
                                //{
                                if (singleGroup[0].startTime > groupsWithPossibleCollisions[l][0].startTime && l != groupsWithPossibleCollisions.Count - 1)
                                {
                                    if (singleGroup[0].startTime < groupsWithPossibleCollisions[l + 1][0].startTime)
                                    {
                                        index = l;
                                        break;
                                    }
                                }
                                else if (l == groupsWithPossibleCollisions.Count - 1 && singleGroup[0].startTime > groupsWithPossibleCollisions[l][0].startTime)
                                {
                                    index = l;
                                    break;
                                }
                                //}
                            }
                            groupsWithPossibleCollisions.Insert(index + 1, singleGroup);
                            if (alreadyAddedCommands.Count == agvMovementInfo.Count)
                            {
                                break;//return groupsWithPossibleCollisions;
                            }
                        }
                    }
                }
                //else
                //{
                //    agv_cmds.Flush(); agv_cmds.Close();
                //    return groupsWithPossibleCollisions;
                //}
            }

            /* Write grouped commands to agv_commands_grouped.log */
            for (int l = 0; l < groupsWithPossibleCollisions.Count; l++)
            {
                agv_cmds.WriteLine("Group " + l + ":");
                for (int m = 0; m < groupsWithPossibleCollisions[l].Count; m++)
                {
                    agv_cmds.WriteLine(groupsWithPossibleCollisions[l][m].taskID + "\t" + groupsWithPossibleCollisions[l][m].usedAGV + "\t" + groupsWithPossibleCollisions[l][m].startTime + "\t" +
                                                   groupsWithPossibleCollisions[l][m].endTime + "\t" + groupsWithPossibleCollisions[l][m].startPosition + "\t" + groupsWithPossibleCollisions[l][m].endPosition);
                }
            }
            agv_cmds.Flush(); agv_cmds.Close();

            return groupsWithPossibleCollisions;

        }





        public List<AGVData> getPreviousAGVgroup(List<AGVData> AGVmovementInfo)
        {
            int index_current_group = agvGroups.IndexOf(AGVmovementInfo);
            return agvGroups[index_current_group - 1];
        }


        public string getAGVatStation(int taskID_before_OTHER, string station, List<AGVData> AGVInfo)
        {
            int j;
            for (int i = AGVInfo.Count - 1; i >= 0; i--)
            {
                if (AGVInfo[i].taskID == taskID_before_OTHER)
                {
                    for (j = i; j >= 0; j--)
                        if (AGVInfo[j].endPosition == station)
                        {
                            //Console.WriteLine(AGVInfo[j].taskID);
                            return AGVInfo[j].usedAGV;
                        }

                }
            }
            return null;
        }




        public List<AGVData> getSortedAGVMovementData(List<AGVData> agvMovementInfo)
        {
            StreamWriter agvInfo_sorted_startTime = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "agv_info_sorted_startTime.log", false);

            decimal[,][] start_timesAndTaskIDs = new decimal[2, agvMovementInfo.Count][];

            // GetLength(0)----> number of rows
            // GetLength(1)----> number of Columns

            for (int i = 0; i < start_timesAndTaskIDs.GetLength(0); i++)
            {
                for (int j = 0; j < start_timesAndTaskIDs.GetLength(1); j++)
                {
                    start_timesAndTaskIDs[i, j] = new decimal[] { agvMovementInfo[j].startTime, agvMovementInfo[j].taskID };
                    //Console.WriteLine(start_timesAndTaskIDs[i, j][0] + "#" + start_timesAndTaskIDs[i, j][1]);
                }

            }
            decimal[] startTimes = new decimal[start_timesAndTaskIDs.GetLength(1)];
            decimal[] taskIDs = new decimal[start_timesAndTaskIDs.GetLength(1)];

            for (int i = 0; i < startTimes.Length; i++)
            {
                startTimes[i] = start_timesAndTaskIDs[0, i][0];
                taskIDs[i] = start_timesAndTaskIDs[0, i][1];
            }

            Array.Sort(startTimes, taskIDs);

            /**Sorted AGV sequence regarding the start times**/

            List<AGVData> agvDataSorted = new List<AGVData>();
            for (int i = 0; i < taskIDs.Length; i++)
            {
                for (int j = 0; j < taskIDs.Length; j++)
                    if (agvMovementInfo[j].taskID == taskIDs[i])
                    {
                        agvDataSorted.Add(agvMovementInfo[j]);
                        agvInfo_sorted_startTime.WriteLine(agvMovementInfo[j].taskID + "\t" + agvMovementInfo[j].usedAGV + "\t" + agvMovementInfo[j].startTime + "\t" + agvMovementInfo[j].endTime + "\t" + agvMovementInfo[j].startPosition + "\t" + agvMovementInfo[j].endPosition);
                        break;
                    }

            }
            agvInfo_sorted_startTime.Flush();
            agvInfo_sorted_startTime.Close();


            return agvDataSorted;
        }




        public int getPreviousTaskSameRecipe(int taskID, List<string[]> sortedSchedule)
        {
            int recipeID = taskID / 10000;
            int j = -1;
            for (int i = 0; i < sortedSchedule.Count; i++)
            {
                if (int.Parse(sortedSchedule[i][1]) == taskID)
                {
                    j = i - 1;
                    for (int counter = j; counter >= 0; counter--)
                    {
                        //Console.WriteLine(sortedSchedule[counter][1]);
                        if ((int.Parse(sortedSchedule[counter][1]) / 10000) == recipeID)
                            return int.Parse(sortedSchedule[counter][1]);
                    }
                }
            }
            return 0;
        }

        public int getPreviousTaskDifferentRecipe(int taskID, List<string[]> sortedSchedule)
        {
            int j = -1;
            decimal taskStartTime = -1;
            for (int i = 0; i < sortedSchedule.Count; i++)
            {
                if (int.Parse(sortedSchedule[i][1]) == taskID)
                {
                    j = i - 1;
                    taskStartTime = decimal.Parse(sortedSchedule[i][2]);
                    break;
                }
            }

            for (int counter = j; counter >= 0; counter--)
            {
                if (decimal.Parse(sortedSchedule[counter][2]) < taskStartTime)
                    return int.Parse(sortedSchedule[counter][1]);
            }
            return 0;
        }


        public decimal getSimultaneousTask(int taskID, List<string[]> sortedSchedule)
        {
            int j = -1;
            decimal taskStartTime = -1;
            for (int i = 0; i < sortedSchedule.Count; i++)
            {
                if (decimal.Parse(sortedSchedule[i][1]) == taskID)
                {
                    j = i - 1;
                    taskStartTime = decimal.Parse(sortedSchedule[i][2]);
                    break;
                }
            }

            for (int counter = j; counter >= 0; counter--)
            {
                if (decimal.Parse(sortedSchedule[counter][2]) == taskStartTime)
                    return decimal.Parse(sortedSchedule[counter][1]);
            }
            return 0;
        }

        public string getPreviousAGVatStation(int taskID, string station, List<string[]> sortedSchedule, List<AGVData> AGVinfo, List<AGVData> initialAGVinfo)
        {
            int j = -1;
            decimal taskStartTime = -1;
            if (initialAGVinfo.Count == 0)
            {
                for (int i = 0; i < sortedSchedule.Count; i++)
                {
                    if (int.Parse(sortedSchedule[i][1]) == taskID)
                    {
                        j = i - 1;
                        taskStartTime = decimal.Parse(sortedSchedule[i][2]);
                        break;
                    }
                }

                for (int i = AGVinfo.Count - 1; i >= 0; i--)
                {
                    //Console.WriteLine("AGVinfo[i].taskID = " + AGVinfo[i].taskID);
                    if (AGVinfo[i].startTime < taskStartTime && AGVinfo[i].endPosition == station)
                        return AGVinfo[i].usedAGV;
                }
            }

            else   /*History of used AGVs are not empty, which means a rescheuling procedure has happened*/
            {
                for (int i = 0; i < initialAGVinfo.Count; i++)
                {
                    if (initialAGVinfo[i].taskID == taskID)
                    {
                        j = i - 1;
                        taskStartTime = initialAGVinfo[i].startTime;
                        break;
                    }
                }

                for (int i = initialAGVinfo.Count - 1; i >= 0; i--)
                {
                    //Console.WriteLine("AGVinfo[i].taskID = " + AGVinfo[i].taskID);
                    if (initialAGVinfo[i].startTime < taskStartTime && initialAGVinfo[i].endPosition == station)
                        return initialAGVinfo[i].usedAGV;
                }

            }
            return null;
        }




        public decimal getResourceNumber(int taskID)
        {
            for (int i = 0; i < sequences.Count; i++)
            {
                if (decimal.Parse(sequences[i][3]) == taskID)
                    return decimal.Parse(sequences[i][4]);
            }
            return 0;
        }

        public List<string[]> getSortedSchedule()
        {
            if (lastSchedule.Count != 0)
            {
                for (int i = 0; i < lastSchedule.Count; i++)
                {
                    bestSchedule.Add(lastSchedule[i].Split('\t', ' '));
                }
            }
            else
                bestSchedule = null;

            if (bestSchedule != null)
            {
                StreamWriter schedule_sorted = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "schedule_sorted.log", false);

                decimal[,][] start_timesAndTaskIDs = new decimal[2, bestSchedule.Count][]; // changed int to decimal 

                // GetLength(0)----> number of rows
                // GetLength(1)----> number of Columns

                //Console.WriteLine(start_timesAndTaskIDs.GetLength(1));
                for (int i = 0; i < start_timesAndTaskIDs.GetLength(0); i++)
                {
                    for (int j = 0; j < start_timesAndTaskIDs.GetLength(1); j++)
                    {
                        start_timesAndTaskIDs[i, j] = new decimal[] { decimal.Parse(bestSchedule[j][2]), decimal.Parse(bestSchedule[j][1]) };
                        //Console.WriteLine(start_timesAndTaskIDs[i, j][0] + "#" + start_timesAndTaskIDs[i, j][1]);
                    }

                }
                decimal[] startTimes = new decimal[start_timesAndTaskIDs.GetLength(1)];
                decimal[] taskIDs = new decimal[start_timesAndTaskIDs.GetLength(1)];

                for (int i = 0; i < startTimes.Length; i++)
                {
                    startTimes[i] = start_timesAndTaskIDs[0, i][0];
                    taskIDs[i] = start_timesAndTaskIDs[0, i][1];
                }

                Array.Sort(startTimes, taskIDs);

                /**Sorted AGV sequence regarding the start times**/

                List<string[]> scheduleSorted = new List<string[]>();
                for (int i = 0; i < taskIDs.Length; i++)
                {
                    for (int j = 0; j < taskIDs.Length; j++)
                        if (decimal.Parse(bestSchedule[j][1]) == taskIDs[i])
                        {
                            for (int k = 0; k < sequences.Count; k++)
                            {
                                if (decimal.Parse(sequences[k][3]) == taskIDs[i])
                                {
                                    scheduleSorted.Add(bestSchedule[j]);
                                    schedule_sorted.WriteLine(bestSchedule[j][0] + "\t" + bestSchedule[j][1] + "\t" + bestSchedule[j][2] + "\t" + bestSchedule[j][3] + "\t" + bestSchedule[j][4] + "\t" + sequences[k][5]);
                                    break;
                                }
                            }
                            break;
                        }

                }
                schedule_sorted.Flush();
                schedule_sorted.Close();

                return scheduleSorted;
            }
            else
            {
                Console.WriteLine("Schedule is null!!!");
                return null;
            }
        }
    }
}
