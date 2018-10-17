using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using MULTIFORM_PCS.Datastructure.Schedule;
using System.Threading;
using System.Windows.Media;
using System.Diagnostics;

namespace MULTIFORM_PCS.Gateway
{
    class CTRLModule
    {
        #region automaticExecutionOfAProductionPlan;
        //DONT CHANGE!
        private bool paused = false;
        public bool Paused
        {
            get { return paused; }
            set { paused = value; }
        }
        private int cylceTimeInMilliseconds = 250;
        private bool debug = true;
        public bool Debug
        {
            get { return debug; }
            set { debug = value; }
        }
        private bool ack = false;
        public bool Ack
        {
            get { return ack; }
            set { ack = value; }
        }
        //DONT CHANGE!

        private bool automaticCTRLRunning;
        public bool AutomaticCTRLRunning
        {
            get { return automaticCTRLRunning; }
        }
        private DispatcherTimer executionEngine;
        private Datastructure.Schedule.Schedule currentSchedule;
        public Datastructure.Schedule.Schedule CurrentSchedule
        {
            get { return currentSchedule; }
            set { currentSchedule = value; }
        }
        private Datastructure.Schedule.DetailedProductionPlan detailedPlan;
        public Datastructure.Schedule.DetailedProductionPlan DetailedPlan
        {
            get { return detailedPlan; }
            set { detailedPlan = value; }
        }
        private UInt64 executionTime = 0; //MILISECONDS!
        private DetailedProductionPlan.ProductionPlanEntry currentCol1CMD;
        private int col1Idx = -1;
        private DetailedProductionPlan.ProductionPlanEntry currentCol2CMD;
        private int col2Idx = -1;
        private DetailedProductionPlan.ProductionPlanEntry currentMixCMD;
        private int mixIdx = -1;
        private DetailedProductionPlan.ProductionPlanEntry currentStoCMD;
        private int stoIdx = -1;
        private DetailedProductionPlan.ProductionPlanEntry[] curAGVCMDs;
        private int[] agvIdcs = { -1, -1, -1, -1, -1 };
        private DetailedProductionPlan.ProductionPlanEntry[] curVesselCMDs;
        private int[] vIdcs = { -1, -1, -1, -1, -1, -1 };
        private byte[] plcSignals;
        private int agv1;
        private int agv2;
        private DetailedProductionPlan.ProductionPlanEntry[] causingDelay;
        private Stopwatch dog = new Stopwatch();
        private double totalSecondsMixStart;

        /**
         * THIS IS THE MAIN CTRL LOOP FOR THE SCHEDULE EXECUTION
         */
        private void CTRL_PLANT(object sender, EventArgs e)
        {
            if (automaticCTRLRunning)
            {
                dog.Start();

                Datastructure.Model.Plant p = ObserverModule.getInstance().getCurrentPlant();

                /**
                 * SEND CTRL SIGNALS TO PLC!
                 */
                bool delayed = false;

                if (Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().Connected || debug)//COMMENT FOR DEBUG
                {
                    byte[] curPLCFeedback = Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().PlcFeedback;
                    if (debug && !Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().Connected)
                    {
                        curPLCFeedback = new byte[30];
                    }

                    plcSignals[28] = 0;
                    plcSignals[29] = 0;
                    plcSignals[30] = 0;
                    plcSignals[31] = 0;
                    plcSignals[32] = 0;
                    plcSignals[33] = 0;
                    plcSignals[44] = 0;//Deactivate motor start bits for rising edge
                    plcSignals[45] = 0;

                    plcSignals[34] = 0;
                    plcSignals[35] = 0;
                    plcSignals[36] = 0;
                    plcSignals[37] = 0;
                    plcSignals[38] = 0;
                    plcSignals[39] = 0;
                    plcSignals[40] = 0;
                    plcSignals[41] = 0;
                    plcSignals[42] = 0;//Deactivate motor start bits for rising edge
                    plcSignals[43] = 0;

                    plcSignals[47] = 0;
                    plcSignals[48] = 0;
                    plcSignals[49] = 0;
                    plcSignals[50] = 0;
                    plcSignals[55] = 0;//Deactivate motor start bits for rising edge
                    plcSignals[56] = 0;
                    plcSignals[57] = 0;
                    plcSignals[58] = 0;

                    //Deactivate valve signals for rising edge
                    plcSignals[1] = 0;
                    plcSignals[2] = 0;
                    plcSignals[3] = 0;
                    plcSignals[4] = 0;
                    plcSignals[5] = 0;
                    plcSignals[6] = 0;
                    plcSignals[7] = 0;
                    plcSignals[8] = 0;

                    //Deactivate stirrer signals for rising edge
                    plcSignals[22] = 0;
                    plcSignals[23] = 0;

                    //Deactivate doser signals for rising edge
                    plcSignals[24] = 0;
                    plcSignals[25] = 0;
                    plcSignals[26] = 0;
                    plcSignals[27] = 0;

                    //CHECK IF ANY EXECUTION FINISHED FIRST! //MAYBE RUN ANOTHER CYCLE WHEN DELAYED!
                    if (currentCol1CMD != null)
                    {
                        if (currentCol1CMD.state == DetailedProductionPlan.STATUS.EXECUTING)//Check if task can be set to finished and get next task. The next task could directly be started (maybe!)
                        {
                            if (currentCol1CMD.operation == DetailedProductionPlan.ELEMENTS.FILL)//DURATION EXPIRE ONLY VALID BY FILLING
                            {
                                if (currentCol1CMD.target == 0 || currentCol1CMD.target == 5 || currentCol1CMD.target == 6)
                                {
                                    if (currentCol1CMD.endTime * 1000.0f <= executionTime || curPLCFeedback[15] == 1)//NOT SURE IF THE NECCESSARY CONDITION HERE SHOULD BE && INSTEAD OF ||
                                    {
                                        currentCol1CMD.state = DetailedProductionPlan.STATUS.FINISHED;
                                        currentCol1CMD.execEndedAt = executionTime;
                                        //currentCol1CMD.visual.Opacity = 0.5d;
                                        currentCol1CMD.visual.BorderBrush = Brushes.Green;
                                        col1Idx++;
                                        if (causingDelay[0] == currentCol1CMD)
                                        {
                                            //delayed = false;
                                            causingDelay[0] = null;
                                        }
                                    }
                                }
                                else if (currentCol1CMD.target == 1)
                                {
                                    if (currentCol1CMD.endTime * 1000.0f <= executionTime || curPLCFeedback[16] == 1)//NOT SURE IF THE NECCESSARY CONDITION HERE SHOULD BE && INSTEAD OF ||
                                    {
                                        currentCol1CMD.state = DetailedProductionPlan.STATUS.FINISHED;
                                        currentCol1CMD.execEndedAt = executionTime;
                                        //currentCol1CMD.visual.Opacity = 0.5d;
                                        currentCol1CMD.visual.BorderBrush = Brushes.Green;
                                        col1Idx++;
                                        if (causingDelay[0] == currentCol1CMD)
                                        {
                                            //delayed = false;
                                            causingDelay[0] = null;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (currentCol1CMD.endTime * 1000.0f <= executionTime)
                                {
                                    currentCol1CMD.state = DetailedProductionPlan.STATUS.FINISHED;
                                    currentCol1CMD.execEndedAt = executionTime;
                                    //currentCol1CMD.visual.Opacity = 0.5d;
                                    currentCol1CMD.visual.BorderBrush = Brushes.Green;
                                    col1Idx++;
                                    if (causingDelay[0] == currentCol1CMD)
                                    {
                                        //delayed = false;
                                        causingDelay[0] = null;
                                    }
                                }
                            }
                        }
                    }
                    if (currentCol2CMD != null)
                    {
                        if (currentCol2CMD.state == DetailedProductionPlan.STATUS.EXECUTING)//Check if task can be set to finished and get next task. The next task could directly be started (maybe!)
                        {
                            if (currentCol2CMD.operation == DetailedProductionPlan.ELEMENTS.FILL)//DURATION EXPIRE ONLY VALID BY FILLING
                            {
                                if (currentCol2CMD.target == 2 || currentCol2CMD.target == 4 || currentCol2CMD.target == 6)
                                {
                                    if (currentCol2CMD.endTime * 1000.0f <= executionTime || curPLCFeedback[17] == 1)//NOT SURE IF THE NECCESSARY CONDITION HERE SHOULD BE && INSTEAD OF ||
                                    {
                                        currentCol2CMD.state = DetailedProductionPlan.STATUS.FINISHED;
                                        currentCol2CMD.execEndedAt = executionTime;
                                        //currentCol2CMD.visual.Opacity = 0.5d;
                                        currentCol2CMD.visual.BorderBrush = Brushes.Green;
                                        col2Idx++;
                                        if (causingDelay[1] == currentCol2CMD)
                                        {
                                            //delayed = false;
                                            causingDelay[1] = null;
                                        }
                                    }
                                }
                                else if (currentCol2CMD.target == 3 || currentCol2CMD.target == 5 || currentCol2CMD.target == 7)
                                {
                                    if (currentCol2CMD.endTime * 1000.0f <= executionTime || curPLCFeedback[18] == 1)//NOT SURE IF THE NECCESSARY CONDITION HERE SHOULD BE && INSTEAD OF ||
                                    {
                                        currentCol2CMD.state = DetailedProductionPlan.STATUS.FINISHED;
                                        currentCol2CMD.execEndedAt = executionTime;
                                        //currentCol2CMD.visual.Opacity = 0.5d;
                                        currentCol2CMD.visual.BorderBrush = Brushes.Green;
                                        col2Idx++;
                                        if (causingDelay[1] == currentCol2CMD)
                                        {
                                            //delayed = false;
                                            causingDelay[1] = null;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (currentCol2CMD.endTime * 1000.0f <= executionTime)
                                {
                                    currentCol2CMD.state = DetailedProductionPlan.STATUS.FINISHED;
                                    currentCol2CMD.execEndedAt = executionTime;
                                    //currentCol2CMD.visual.Opacity = 0.5d;
                                    currentCol2CMD.visual.BorderBrush = Brushes.Green;
                                    col2Idx++;
                                    if (causingDelay[1] == currentCol2CMD)
                                    {
                                        //delayed = false;
                                        causingDelay[1] = null;
                                    }
                                }
                            }
                        }
                    }
                    if (currentMixCMD != null)
                    {
                        if (currentMixCMD.state == DetailedProductionPlan.STATUS.EXECUTING)//Check if task can be set to finished and get next task. The next task could directly be started (maybe!)
                        {
                            if (currentMixCMD.operation == DetailedProductionPlan.ELEMENTS.MIX)
                            {
                                if (currentMixCMD.endTime * 1000.0f <= executionTime 
                                    || (curPLCFeedback[23] == 1 && curPLCFeedback[24] == 1)
                                    || (curPLCFeedback[10] * 24.0d * 3600.0d + curPLCFeedback[9] * 3600.0d + curPLCFeedback[8] * 60.0d + curPLCFeedback[7]) - totalSecondsMixStart >= 85)//NOT SURE IF THE NECCESSARY CONDITION HERE SHOULD BE && INSTEAD OF ||
                                {
                                    currentMixCMD.state = DetailedProductionPlan.STATUS.FINISHED;
                                    currentMixCMD.execEndedAt = executionTime;
                                    //currentMixCMD.visual.Opacity = 0.5d;
                                    currentMixCMD.visual.BorderBrush = Brushes.Green;
                                    mixIdx++;
                                    if (causingDelay[2] == currentMixCMD)
                                    {
                                        //delayed = false;
                                        causingDelay[2] = null;
                                    }
                                }
                            }
                            else if (currentMixCMD.operation == DetailedProductionPlan.ELEMENTS.GRAB)
                            {
                                if (currentMixCMD.endTime * 1000.0f <= executionTime || (curPLCFeedback[7] == 1))//NOT SURE IF THE NECCESSARY CONDITION HERE SHOULD BE && INSTEAD OF ||
                                {
                                    currentMixCMD.state = DetailedProductionPlan.STATUS.FINISHED;
                                    currentMixCMD.execEndedAt = executionTime;
                                    // currentMixCMD.visual.Opacity = 0.5d;
                                    currentMixCMD.visual.BorderBrush = Brushes.Green;
                                    mixIdx++;
                                    if (causingDelay[2] == currentMixCMD)
                                    {
                                        //delayed = false;
                                        causingDelay[2] = null;
                                    }
                                }
                            }
                            else
                            {
                                if (currentMixCMD.endTime * 1000.0f <= executionTime)
                                {
                                    currentMixCMD.state = DetailedProductionPlan.STATUS.FINISHED;
                                    currentMixCMD.execEndedAt = executionTime;
                                    //  currentMixCMD.visual.Opacity = 0.5d;
                                    currentMixCMD.visual.BorderBrush = Brushes.Green;
                                    mixIdx++;
                                    if (causingDelay[2] == currentMixCMD)
                                    {
                                        //delayed = false;
                                        causingDelay[2] = null;
                                    }
                                }
                            }
                        }
                    }
                    if (currentStoCMD != null)
                    {
                        if (currentStoCMD.state == DetailedProductionPlan.STATUS.EXECUTING)//Check if task can be set to finished and get next task. The next task could directly be started (maybe!)
                        {
                            if (currentStoCMD.operation == DetailedProductionPlan.ELEMENTS.GRAB || (curPLCFeedback[6] == 1))//NOT SURE IF THE NECCESSARY CONDITION HERE SHOULD BE && INSTEAD OF ||
                            {
                                if (currentStoCMD.endTime * 1000.0f <= executionTime)
                                {
                                    currentStoCMD.state = DetailedProductionPlan.STATUS.FINISHED;
                                    currentStoCMD.execEndedAt = executionTime;
                                    // currentStoCMD.visual.Opacity = 0.5d;
                                    currentStoCMD.visual.BorderBrush = Brushes.Green;
                                    stoIdx++;
                                    if (causingDelay[3] == currentStoCMD)
                                    {
                                        //delayed = false;
                                        causingDelay[3] = null;
                                    }
                                }
                            }
                            else if (currentStoCMD.operation == DetailedProductionPlan.ELEMENTS.RELEASE || (curPLCFeedback[6] == 1))//NOT SURE IF THE NECCESSARY CONDITION HERE SHOULD BE && INSTEAD OF ||
                            {
                                if (currentStoCMD.endTime * 1000.0f <= executionTime)
                                {
                                    currentStoCMD.state = DetailedProductionPlan.STATUS.FINISHED;
                                    currentStoCMD.execEndedAt = executionTime;
                                    // currentStoCMD.visual.Opacity = 0.5d;
                                    currentStoCMD.visual.BorderBrush = Brushes.Green;
                                    stoIdx++;
                                    if (causingDelay[3] == currentStoCMD)
                                    {
                                        //delayed = false;
                                        causingDelay[3] = null;
                                    }
                                }
                            }
                            else
                            {
                                if (currentStoCMD.endTime * 1000.0f <= executionTime)
                                {
                                    currentStoCMD.state = DetailedProductionPlan.STATUS.FINISHED;
                                    currentStoCMD.execEndedAt = executionTime;
                                    //currentStoCMD.visual.Opacity = 0.5d;
                                    currentStoCMD.visual.BorderBrush = Brushes.Green;
                                    stoIdx++;
                                    if (causingDelay[3] == currentStoCMD)
                                    {
                                        //delayed = false;
                                        causingDelay[3] = null;
                                    }
                                }
                            }
                        }
                    }
                    for (int i = 0; i < curVesselCMDs.Length; i++)
                    {
                        if (curVesselCMDs[i] != null)
                        {
                            if (curVesselCMDs[i].state == DetailedProductionPlan.STATUS.EXECUTING)//Check if task can be set to finished and get next task. The next task could directly be started (maybe!)
                            {
                                if (curVesselCMDs[i].endTime * 1000.0f <= executionTime)
                                {
                                    curVesselCMDs[i].state = DetailedProductionPlan.STATUS.FINISHED;
                                    curVesselCMDs[i].execEndedAt = executionTime;
                                    //curVesselCMDs[i].visual.Opacity = 0.5d;
                                    curVesselCMDs[i].visual.BorderBrush = Brushes.Green;
                                    p.AllVessels[i].Finished[curVesselCMDs[i].target] = true;
                                    vIdcs[i]++;
                                    if (causingDelay[4 + i] == curVesselCMDs[i])
                                    {
                                        //delayed = false;
                                        causingDelay[4 + i] = null;
                                    }
                                }
                            }
                        }
                    }
                    /**
                     * CHECK IF AGVs FINISHED TASKS!
                     * 
                     */
                    if (debug)
                    {
                        for (int i = 0; i < curAGVCMDs.Length; i++)
                        {
                            if (curAGVCMDs[i] != null)
                            {
                                if (curAGVCMDs[i].state == DetailedProductionPlan.STATUS.EXECUTING)//Check if task can be set to finished and get next task. The next task could directly be started (maybe!)
                                {
                                    if (curAGVCMDs[i].endTime * 1000.0f <= executionTime && ack)
                                    {
                                        ack = false;
                                        /**==> CHECK IF AGV HAS DONE THE COMMAND! */
                                        curAGVCMDs[i].state = DetailedProductionPlan.STATUS.FINISHED;
                                        curAGVCMDs[i].execEndedAt = executionTime;
                                        //curAGVCMDs[i].visual.Opacity = 0.5d;
                                        curAGVCMDs[i].visual.BorderBrush = Brushes.Green;
                                        agvIdcs[i]++;
                                        if (causingDelay[4 + curVesselCMDs.Length + i] == curAGVCMDs[i])
                                        {
                                            //delayed = false;
                                            causingDelay[4 + curVesselCMDs.Length + i] = null;
                                        }
                                    }
                                    else if (curAGVCMDs[i].endTime * 1000.0f <= executionTime)
                                    {
                                        //delayed = true;
                                        causingDelay[4 + curVesselCMDs.Length + i] = curAGVCMDs[i];
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < curAGVCMDs.Length; i++)
                        {
                            if (curAGVCMDs[i] != null)
                            {
                                if (curAGVCMDs[i].state == DetailedProductionPlan.STATUS.EXECUTING)//Check if task can be set to finished and get next task. The next task could directly be started (maybe!)
                                {
                                    if (curAGVCMDs[i].operation == DetailedProductionPlan.ELEMENTS.WAIT)
                                    {
                                        if (curAGVCMDs[i].endTime * 1000.0f <= executionTime)
                                        {
                                            /**==> CHECK IF AGV HAS DONE THE COMMAND! */
                                            curAGVCMDs[i].state = DetailedProductionPlan.STATUS.FINISHED;
                                            curAGVCMDs[i].execEndedAt = executionTime;
                                            //curAGVCMDs[i].visual.Opacity = 0.5d;
                                            curAGVCMDs[i].visual.BorderBrush = Brushes.Green;
                                            agvIdcs[i]++;
                                            if (causingDelay[4 + curVesselCMDs.Length + i] == curAGVCMDs[i])
                                            {
                                                //delayed = false;
                                                causingDelay[4 + curVesselCMDs.Length + i] = null;
                                            }
                                        }
                                    }
                                    else if (curAGVCMDs[i].operation == DetailedProductionPlan.ELEMENTS.MOVE_TO)
                                    {
                                        if (i == 0)//IT IS SUPPOSED, THAT ONLY TWO AGVS ARE USED!!!!!!!!!
                                        {
                                            if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                                            {
                                                for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                {
                                                    if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv1)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].State == ConnectionModule.iRobot.iRobotRouter.Robot.RobotState.Idle)
                                                        {
                                                            curAGVCMDs[i].state = DetailedProductionPlan.STATUS.FINISHED;
                                                            curAGVCMDs[i].execEndedAt = executionTime;
                                                            //curAGVCMDs[i].visual.Opacity = 0.5d;
                                                            curAGVCMDs[i].visual.BorderBrush = Brushes.Green;
                                                            agvIdcs[i]++;
                                                            if (causingDelay[4 + curVesselCMDs.Length + i] == curAGVCMDs[i])
                                                            {
                                                                //delayed = false;
                                                                causingDelay[4 + curVesselCMDs.Length + i] = null;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                                            {
                                                for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                {
                                                    if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv2)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].State == ConnectionModule.iRobot.iRobotRouter.Robot.RobotState.Idle)
                                                        {
                                                            curAGVCMDs[i].state = DetailedProductionPlan.STATUS.FINISHED;
                                                            curAGVCMDs[i].execEndedAt = executionTime;
                                                            //curAGVCMDs[i].visual.Opacity = 0.5d;
                                                            curAGVCMDs[i].visual.BorderBrush = Brushes.Green;
                                                            agvIdcs[i]++;
                                                            if (causingDelay[4 + curVesselCMDs.Length + i] == curAGVCMDs[i])
                                                            {
                                                                //delayed = false;
                                                                causingDelay[4 + curVesselCMDs.Length + i] = null;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (curAGVCMDs[i].operation == DetailedProductionPlan.ELEMENTS.SPECIAL_MOVE)
                                    {
                                        if (i == 0)//IT IS SUPPOSED, THAT ONLY TWO AGVS ARE USED!!!!!!!!!
                                        {
                                            if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                                            {
                                                for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                {
                                                    if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv1)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].State == ConnectionModule.iRobot.iRobotRouter.Robot.RobotState.Idle)
                                                        {
                                                            curAGVCMDs[i].state = DetailedProductionPlan.STATUS.FINISHED;
                                                            curAGVCMDs[i].execEndedAt = executionTime;
                                                            //curAGVCMDs[i].visual.Opacity = 0.5d;
                                                            curAGVCMDs[i].visual.BorderBrush = Brushes.Green;
                                                            agvIdcs[i]++;
                                                            if (causingDelay[4 + curVesselCMDs.Length + i] == curAGVCMDs[i])
                                                            {
                                                                //delayed = false;
                                                                causingDelay[4 + curVesselCMDs.Length + i] = null;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                                            {
                                                for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                {
                                                    if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv2)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].State == ConnectionModule.iRobot.iRobotRouter.Robot.RobotState.Idle)
                                                        {
                                                            curAGVCMDs[i].state = DetailedProductionPlan.STATUS.FINISHED;
                                                            curAGVCMDs[i].execEndedAt = executionTime;
                                                            //curAGVCMDs[i].visual.Opacity = 0.5d;
                                                            curAGVCMDs[i].visual.BorderBrush = Brushes.Green;
                                                            agvIdcs[i]++;
                                                            if (causingDelay[4 + curVesselCMDs.Length + i] == curAGVCMDs[i])
                                                            {
                                                                //delayed = false;
                                                                causingDelay[4 + curVesselCMDs.Length + i] = null;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (curAGVCMDs[i].operation == DetailedProductionPlan.ELEMENTS.DOCK)
                                    {
                                        if (i == 0)//IT IS SUPPOSED, THAT ONLY TWO AGVS ARE USED!!!!!!!!!
                                        {
                                            if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                                            {
                                                for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                {
                                                    if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv1)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].State == ConnectionModule.iRobot.iRobotRouter.Robot.RobotState.StationDocked)
                                                        {
                                                            curAGVCMDs[i].state = DetailedProductionPlan.STATUS.FINISHED;
                                                            curAGVCMDs[i].execEndedAt = executionTime;
                                                            // curAGVCMDs[i].visual.Opacity = 0.5d;
                                                            curAGVCMDs[i].visual.BorderBrush = Brushes.Green;
                                                            agvIdcs[i]++;
                                                            if (causingDelay[4 + curVesselCMDs.Length + i] == curAGVCMDs[i])
                                                            {
                                                                //delayed = false;
                                                                causingDelay[4 + curVesselCMDs.Length + i] = null;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                                            {
                                                for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                {
                                                    if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv2)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].State == ConnectionModule.iRobot.iRobotRouter.Robot.RobotState.StationDocked)
                                                        {
                                                            curAGVCMDs[i].state = DetailedProductionPlan.STATUS.FINISHED;
                                                            curAGVCMDs[i].execEndedAt = executionTime;
                                                            //  curAGVCMDs[i].visual.Opacity = 0.5d;
                                                            curAGVCMDs[i].visual.BorderBrush = Brushes.Green;
                                                            agvIdcs[i]++;
                                                            if (causingDelay[4 + curVesselCMDs.Length + i] == curAGVCMDs[i])
                                                            {
                                                                //delayed = false;
                                                                causingDelay[4 + curVesselCMDs.Length + i] = null;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (curAGVCMDs[i].operation == DetailedProductionPlan.ELEMENTS.UNDOCK)
                                    {
                                        if (i == 0)//IT IS SUPPOSED, THAT ONLY TWO AGVS ARE USED!!!!!!!!!
                                        {
                                            if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                                            {
                                                for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                {
                                                    if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv1)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].State == ConnectionModule.iRobot.iRobotRouter.Robot.RobotState.Idle)
                                                        {
                                                            curAGVCMDs[i].state = DetailedProductionPlan.STATUS.FINISHED;
                                                            curAGVCMDs[i].execEndedAt = executionTime;
                                                            // curAGVCMDs[i].visual.Opacity = 0.5d;
                                                            curAGVCMDs[i].visual.BorderBrush = Brushes.Green;
                                                            agvIdcs[i]++;
                                                            if (causingDelay[4 + curVesselCMDs.Length + i] == curAGVCMDs[i])
                                                            {
                                                                //delayed = false;
                                                                causingDelay[4 + curVesselCMDs.Length + i] = null;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                                            {
                                                for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                {
                                                    if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv2)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].State == ConnectionModule.iRobot.iRobotRouter.Robot.RobotState.Idle)
                                                        {
                                                            curAGVCMDs[i].state = DetailedProductionPlan.STATUS.FINISHED;
                                                            curAGVCMDs[i].execEndedAt = executionTime;
                                                            // curAGVCMDs[i].visual.Opacity = 0.5d;
                                                            curAGVCMDs[i].visual.BorderBrush = Brushes.Green;
                                                            agvIdcs[i]++;
                                                            if (causingDelay[4 + curVesselCMDs.Length + i] == curAGVCMDs[i])
                                                            {
                                                                //delayed = false;
                                                                causingDelay[4 + curVesselCMDs.Length + i] = null;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    bool found = false;
                    bool[] alarmLED = new bool[4 + curVesselCMDs.Length + curAGVCMDs.Length];
                    for (int i = 0; i < causingDelay.Length; i++)
                    {
                        if (causingDelay[i] != null)
                        {
                            delayed = true;
                            found = true;
                            alarmLED[i] = true;
                            //break;
                        }
                    }
                    if (!found)
                    {
                        delayed = false;
                    }
                    GUI.PCSMainWindow.getInstance().setLEDAlarm(alarmLED);

                    //SET NEXT CMD
                    if (col1Idx < detailedPlan.Col1Plan.Count)
                    {
                        if (detailedPlan.Col1Plan[col1Idx].state == DetailedProductionPlan.STATUS.WAITING)
                        {
                            currentCol1CMD = detailedPlan.Col1Plan[col1Idx];
                        }
                    }
                    if (col2Idx < detailedPlan.Col2Plan.Count)
                    {
                        if (detailedPlan.Col2Plan[col2Idx].state == DetailedProductionPlan.STATUS.WAITING)
                        {
                            currentCol2CMD = detailedPlan.Col2Plan[col2Idx];
                        }
                    }
                    if (mixIdx < detailedPlan.MixPlan.Count)
                    {
                        if (detailedPlan.MixPlan[mixIdx].state == DetailedProductionPlan.STATUS.WAITING)
                        {
                            currentMixCMD = detailedPlan.MixPlan[mixIdx];
                        }
                    }
                    if (stoIdx < detailedPlan.StoPlan.Count)
                    {
                        if (detailedPlan.StoPlan[stoIdx].state == DetailedProductionPlan.STATUS.WAITING)
                        {
                            currentStoCMD = detailedPlan.StoPlan[stoIdx];
                        }
                    }
                    for (int i = 0; i < detailedPlan.AgvPlans.Length; i++)
                    {
                        if (agvIdcs[i] < detailedPlan.AgvPlans[i].Count && agvIdcs[i] != -1)
                        {
                            if (detailedPlan.AgvPlans[i][agvIdcs[i]].state == DetailedProductionPlan.STATUS.WAITING)
                            {
                                curAGVCMDs[i] = detailedPlan.AgvPlans[i][agvIdcs[i]];
                            }
                        }
                    }
                    for (int i = 0; i < detailedPlan.VPlans.Length; i++)
                    {
                        if (vIdcs[i] < detailedPlan.VPlans[i].Count)
                        {
                            if (detailedPlan.VPlans[i][vIdcs[i]].state == DetailedProductionPlan.STATUS.WAITING)
                            {
                                curVesselCMDs[i] = detailedPlan.VPlans[i][vIdcs[i]];
                            }
                        }
                    }

                    if (!delayed)//Do not start other tasks before schedule synchronized?
                    {
                        //CHECK IF NEXT EXECUTION CAN BE STARTED!
                        /**
                         * TARGET TABELLE:
                         * COLORSTATIONS:
                         * 0 - YELLOW
                         * 1 - BLACK
                         * 2 - RED
                         * 3 - BLUE
                         * 4 - PURPLE (RED)
                         * 5 - GREEN
                         * 6 - ORANGE
                         * 7 - PURPLE (BLUE)
                         * ROBOT MOVEMENT:
                         * 0 - MIX RIGHT
                         * 1 - MIX LEFt
                         * 2 - COL1
                         * 3 - COL2
                         * 4 - CHA1
                         * 5 - CHA2
                         * 6 - STO RIGHT
                         * 7 - STO LEFT
                         * 8 - INI1
                         * 9 - INI2
                         * MIXING LAYER:
                         * 0 - L1 (BOTTOM)
                         * 1 - L2 (MIDDLE)
                         * 2 - L3 (TOP)
                         * STORAGE POSITION:
                         * 0 - P1
                         * 1 - P2
                         * 2 - P3
                         * 3 - P4
                         * 4 - P5
                         * 5 - P6
                         */
                        if (currentCol1CMD != null)
                        {
                            if (currentCol1CMD.state == DetailedProductionPlan.STATUS.WAITING)//Check if task is waiting to be executed
                            {
                                if (currentCol1CMD.startTime * 1000.0f <= executionTime)//Should be equal, otherwise task was delayed due to not met preconditions
                                {
                                    if (currentCol1CMD.preConditionsAndSynchMet())//START EXECUTION //MIGHT BE A PROBLEME HERE WITH SYNCH CASE!
                                    {
                                        currentCol1CMD.state = DetailedProductionPlan.STATUS.EXECUTING;
                                        currentCol1CMD.visual.BorderBrush = Brushes.Yellow;
                                        currentCol1CMD.execStartedAt = executionTime;
                                        if (currentCol1CMD.operation == DetailedProductionPlan.ELEMENTS.FILL)
                                        {
                                            if (currentCol1CMD.target == 0 || currentCol1CMD.target == 5 || currentCol1CMD.target == 6)//YELLOW
                                            {
                                                plcSignals[1] = 1;
                                                plcSignals[2] = 30;
                                            }
                                            else if (currentCol1CMD.target == 1)//BLACK
                                            {
                                                plcSignals[3] = 1;
                                                plcSignals[4] = 30;
                                            }
                                        }
                                        else if (currentCol1CMD.operation == DetailedProductionPlan.ELEMENTS.MOVE_TO)
                                        {
                                            //NOOP
                                        }
                                        else if (currentCol1CMD.operation == DetailedProductionPlan.ELEMENTS.DOCK)
                                        {
                                            //NOOP
                                        }
                                        else if (currentCol1CMD.operation == DetailedProductionPlan.ELEMENTS.UNDOCK)
                                        {
                                            //NOOP
                                        }
                                    }
                                }
                            }
                        }
                        if (currentCol2CMD != null)
                        {
                            if (currentCol2CMD.state == DetailedProductionPlan.STATUS.WAITING)//Check if task is waiting to be executed
                            {
                                if (currentCol2CMD.startTime * 1000.0f <= executionTime)//Should be equal, otherwise task was delayed due to not met preconditions
                                {
                                    if (currentCol2CMD.preConditionsAndSynchMet())//START EXECUTION //MIGHT BE A PROBLEME HERE WITH SYNCH CASE!
                                    {
                                        currentCol2CMD.state = DetailedProductionPlan.STATUS.EXECUTING;
                                        currentCol2CMD.visual.BorderBrush = Brushes.Yellow;
                                        currentCol2CMD.execStartedAt = executionTime;
                                        if (currentCol2CMD.operation == DetailedProductionPlan.ELEMENTS.FILL)
                                        {
                                            if (currentCol2CMD.target == 2 || currentCol2CMD.target == 4 || currentCol2CMD.target == 6)//RED
                                            {
                                                plcSignals[5] = 1;
                                                plcSignals[6] = 30;
                                            }
                                            else if (currentCol2CMD.target == 3 || currentCol2CMD.target == 5 || currentCol2CMD.target == 7)//BLUE
                                            {
                                                plcSignals[7] = 1;
                                                plcSignals[8] = 30;
                                            }
                                        }
                                        else if (currentCol2CMD.operation == DetailedProductionPlan.ELEMENTS.MOVE_TO)
                                        {
                                            //NOOP
                                        }
                                        else if (currentCol2CMD.operation == DetailedProductionPlan.ELEMENTS.DOCK)
                                        {
                                            //NOOP
                                        }
                                        else if (currentCol2CMD.operation == DetailedProductionPlan.ELEMENTS.UNDOCK)
                                        {
                                            //NOOP
                                        }
                                    }
                                }
                            }
                        } 
                        if (currentMixCMD != null)
                        {
                            if (currentMixCMD.state == DetailedProductionPlan.STATUS.WAITING)//Check if task is waiting to be executed
                            {
                                if (currentMixCMD.startTime * 1000.0f <= executionTime || currentMixCMD.operation == DetailedProductionPlan.ELEMENTS.RELEASE)//Should be equal, otherwise task was delayed due to not met preconditions
                                {
                                    if (currentMixCMD.preConditionsAndSynchMet() || (currentMixCMD.operation == DetailedProductionPlan.ELEMENTS.RELEASE && currentMixCMD.preConditionsMet()))//START EXECUTION
                                    {
                                        currentMixCMD.state = DetailedProductionPlan.STATUS.EXECUTING;
                                        currentMixCMD.visual.BorderBrush = Brushes.Yellow;
                                        currentMixCMD.execStartedAt = executionTime;
                                        if (currentMixCMD.operation == DetailedProductionPlan.ELEMENTS.MOVE_TO)
                                        {
                                            //NOOP
                                        }
                                        else if (currentMixCMD.operation == DetailedProductionPlan.ELEMENTS.DOCK)
                                        {
                                            //NOOP
                                        }
                                        else if (currentMixCMD.operation == DetailedProductionPlan.ELEMENTS.GRAB)
                                        {
                                            //plcSignals[18] = 1; //MAGNETS ON
                                            //plcSignals[19] = 1; //MAGNETS ON

                                            if (currentMixCMD.target == 0)
                                            {
                                                plcSignals[47] = 0;
                                                plcSignals[48] = 0;
                                                plcSignals[49] = 0;
                                                plcSignals[50] = 1;
                                                plcSignals[55] = 1;
                                                plcSignals[56] = 0;
                                                plcSignals[57] = 0;
                                                plcSignals[58] = 0;
                                            }
                                            else if (currentMixCMD.target == 1)
                                            {
                                                plcSignals[47] = 0;
                                                plcSignals[48] = 0;
                                                plcSignals[49] = 1;
                                                plcSignals[50] = 0;
                                                plcSignals[55] = 1;
                                                plcSignals[56] = 0;
                                                plcSignals[57] = 0;
                                                plcSignals[58] = 0;
                                            }
                                            else if (currentMixCMD.target == 2)
                                            {
                                                plcSignals[47] = 0;
                                                plcSignals[48] = 1;
                                                plcSignals[49] = 0;
                                                plcSignals[50] = 0;
                                                plcSignals[55] = 1;
                                                plcSignals[56] = 0;
                                                plcSignals[57] = 0;
                                                plcSignals[58] = 0;
                                            }
                                        }
                                        else if (currentMixCMD.operation == DetailedProductionPlan.ELEMENTS.MIX)
                                        {
                                            plcSignals[18] = 1; //MAGNETS ON
                                            plcSignals[19] = 1; //MAGNETS ON

                                            plcSignals[22] = 1;
                                            plcSignals[23] = 85;
                                            plcSignals[24] = 0xFF;
                                            plcSignals[25] = 0x14;
                                            plcSignals[26] = 85;
                                            plcSignals[27] = 1;

                                            totalSecondsMixStart = curPLCFeedback[10] * 24.0d * 3600.0d + curPLCFeedback[9] * 3600.0d + curPLCFeedback[8] * 60.0d + curPLCFeedback[7];
                                        }
                                        else if (currentMixCMD.operation == DetailedProductionPlan.ELEMENTS.RELEASE)
                                        {
                                            plcSignals[18] = 0; //MAGNETS OFF
                                            plcSignals[19] = 0; //MAGNETS OFF

                                            plcSignals[47] = 0;
                                            plcSignals[48] = 0;
                                            plcSignals[49] = 0;
                                            plcSignals[50] = 0;
                                            plcSignals[55] = 1;
                                            plcSignals[56] = 0;
                                            plcSignals[57] = 0;
                                            plcSignals[58] = 1;
                                        }
                                        else if (currentMixCMD.operation == DetailedProductionPlan.ELEMENTS.UNDOCK)
                                        {
                                            // plcSignals[18] = 0; //MAGNETS OFF
                                            // plcSignals[19] = 0; //MAGNETS OFF
                                        }
                                    }
                                }
                            }
                            if (currentStoCMD != null)
                            {
                                if (currentStoCMD.state == DetailedProductionPlan.STATUS.WAITING)//Check if task is waiting to be executed
                                {
                                    if (currentStoCMD.startTime * 1000.0f <= executionTime)//Should be equal, otherwise task was delayed due to not met preconditions
                                    {
                                        if (currentStoCMD.preConditionsAndSynchMet())//START EXECUTION 
                                        {
                                            currentStoCMD.state = DetailedProductionPlan.STATUS.EXECUTING;
                                            currentStoCMD.visual.BorderBrush = Brushes.Yellow;
                                            currentStoCMD.execStartedAt = executionTime;
                                            if (currentStoCMD.operation == DetailedProductionPlan.ELEMENTS.MOVE_TO)
                                            {
                                                //NOOP
                                            }
                                            else if (currentStoCMD.operation == DetailedProductionPlan.ELEMENTS.DOCK)
                                            {
                                                //NOOP
                                            }
                                            else if (currentStoCMD.operation == DetailedProductionPlan.ELEMENTS.RELEASE)
                                            {
                                                if (currentStoCMD.target == 1)
                                                {
                                                    plcSignals[34] = 1;
                                                    plcSignals[35] = 0;
                                                    plcSignals[36] = 0;
                                                    plcSignals[38] = 0;
                                                    plcSignals[39] = 0;
                                                    plcSignals[40] = 0;
                                                    plcSignals[41] = 0;
                                                    plcSignals[42] = 0;
                                                    plcSignals[43] = 0;

                                                    plcSignals[44] = 0;
                                                    plcSignals[45] = 1;
                                                }
                                                else if (currentStoCMD.target == 2)
                                                {
                                                    plcSignals[34] = 0;
                                                    plcSignals[35] = 1;
                                                    plcSignals[36] = 0;
                                                    plcSignals[38] = 0;
                                                    plcSignals[39] = 0;
                                                    plcSignals[40] = 0;
                                                    plcSignals[41] = 0;
                                                    plcSignals[42] = 0;
                                                    plcSignals[43] = 0;

                                                    plcSignals[44] = 0;
                                                    plcSignals[45] = 1;
                                                }
                                                else if (currentStoCMD.target == 3)
                                                {
                                                    plcSignals[34] = 0;
                                                    plcSignals[35] = 0;
                                                    plcSignals[36] = 1;
                                                    plcSignals[38] = 0;
                                                    plcSignals[39] = 0;
                                                    plcSignals[40] = 0;
                                                    plcSignals[41] = 0;
                                                    plcSignals[42] = 0;
                                                    plcSignals[43] = 0;

                                                    plcSignals[44] = 0;
                                                    plcSignals[45] = 1;
                                                }
                                                else if (currentStoCMD.target == 4)
                                                {
                                                    plcSignals[34] = 0;
                                                    plcSignals[35] = 0;
                                                    plcSignals[36] = 0;
                                                    plcSignals[38] = 1;
                                                    plcSignals[39] = 0;
                                                    plcSignals[40] = 0;
                                                    plcSignals[41] = 0;
                                                    plcSignals[42] = 0;
                                                    plcSignals[43] = 0;

                                                    plcSignals[44] = 0;
                                                    plcSignals[45] = 1;
                                                }
                                                else if (currentStoCMD.target == 5)
                                                {
                                                    plcSignals[34] = 0;
                                                    plcSignals[35] = 0;
                                                    plcSignals[36] = 0;
                                                    plcSignals[38] = 0;
                                                    plcSignals[39] = 1;
                                                    plcSignals[40] = 0;
                                                    plcSignals[41] = 0;
                                                    plcSignals[42] = 0;
                                                    plcSignals[43] = 0;

                                                    plcSignals[44] = 0;
                                                    plcSignals[45] = 1;
                                                }
                                                else if (currentStoCMD.target == 6)
                                                {
                                                    plcSignals[34] = 0;
                                                    plcSignals[35] = 0;
                                                    plcSignals[36] = 0;
                                                    plcSignals[38] = 0;
                                                    plcSignals[39] = 0;
                                                    plcSignals[40] = 1;
                                                    plcSignals[41] = 0;
                                                    plcSignals[42] = 0;
                                                    plcSignals[43] = 0;

                                                    plcSignals[44] = 0;
                                                    plcSignals[45] = 1;
                                                }
                                            }
                                            else if (currentStoCMD.operation == DetailedProductionPlan.ELEMENTS.GRAB)
                                            {
                                                //plcSignals[21] = 1; //MAGNETS ON
                                                //plcSignals[22] = 1; //MAGNETS ON

                                                if (currentStoCMD.target == 1)
                                                {
                                                    plcSignals[34] = 1;
                                                    plcSignals[35] = 0;
                                                    plcSignals[36] = 0;
                                                    plcSignals[38] = 0;
                                                    plcSignals[39] = 0;
                                                    plcSignals[40] = 0;
                                                    plcSignals[41] = 0;
                                                    plcSignals[42] = 0;
                                                    plcSignals[43] = 0;

                                                    plcSignals[44] = 1;
                                                    plcSignals[45] = 0;
                                                }
                                                else if (currentStoCMD.target ==2)
                                                {
                                                    plcSignals[34] = 0;
                                                    plcSignals[35] = 1;
                                                    plcSignals[36] = 0;
                                                    plcSignals[38] = 0;
                                                    plcSignals[39] = 0;
                                                    plcSignals[40] = 0;
                                                    plcSignals[41] =0;
                                                    plcSignals[42] = 0;
                                                    plcSignals[43] = 0;

                                                    plcSignals[44] = 1;
                                                    plcSignals[45] = 0;
                                                }
                                                else if (currentStoCMD.target == 3)
                                                {
                                                    plcSignals[34] = 0;
                                                    plcSignals[35] = 0;
                                                    plcSignals[36] = 1;
                                                    plcSignals[38] = 0;
                                                    plcSignals[39] = 0;
                                                    plcSignals[40] = 0;
                                                    plcSignals[41] = 0;
                                                    plcSignals[42] = 0;
                                                    plcSignals[43] = 0;

                                                    plcSignals[44] = 1;
                                                    plcSignals[45] = 0;
                                                }
                                                else if (currentStoCMD.target == 4)
                                                {
                                                    plcSignals[34] = 0;
                                                    plcSignals[35] = 0;
                                                    plcSignals[36] = 0;
                                                    plcSignals[38] = 1;
                                                    plcSignals[39] = 0;
                                                    plcSignals[40] = 0;
                                                    plcSignals[41] = 0;
                                                    plcSignals[42] = 0;
                                                    plcSignals[43] = 0;

                                                    plcSignals[44] = 1;
                                                    plcSignals[45] = 0;
                                                }
                                                else if (currentStoCMD.target == 5)
                                                {
                                                    plcSignals[34] = 0;
                                                    plcSignals[35] = 0;
                                                    plcSignals[36] = 0;
                                                    plcSignals[38] = 0;
                                                    plcSignals[39] = 1;
                                                    plcSignals[40] = 0;
                                                    plcSignals[41] = 0;
                                                    plcSignals[42] = 0;
                                                    plcSignals[43] = 0;

                                                    plcSignals[44] = 1;
                                                    plcSignals[45] = 0;
                                                }
                                                else if (currentStoCMD.target == 6)
                                                {
                                                    plcSignals[34] = 0;
                                                    plcSignals[35] = 0;
                                                    plcSignals[36] = 0;
                                                    plcSignals[38] = 0;
                                                    plcSignals[39] = 0;
                                                    plcSignals[40] = 1;
                                                    plcSignals[41] = 0;
                                                    plcSignals[42] = 0;
                                                    plcSignals[43] = 0;

                                                    plcSignals[44] = 1;
                                                    plcSignals[45] = 0;
                                                }
                                            }
                                            else if (currentMixCMD.operation == DetailedProductionPlan.ELEMENTS.UNDOCK)
                                            {
                                                //plcSignals[21] = 0; //MAGNETS OFF
                                                //plcSignals[22] = 0; //MAGNETS OFF
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    for (int i = 0; i < curVesselCMDs.Length; i++)
                    {
                        if (curVesselCMDs[i] != null)
                        {
                            if (curVesselCMDs[i].state == DetailedProductionPlan.STATUS.WAITING)//Check if task is waiting to be executed
                            {
                                if (curVesselCMDs[i].startTime * 1000.0f <= executionTime)//Should be equal, otherwise task was delayed due to not met preconditions
                                {
                                    if (curVesselCMDs[i].preConditionsAndSynchMet())//START EXECUTION
                                    {
                                        curVesselCMDs[i].state = DetailedProductionPlan.STATUS.EXECUTING;
                                        curVesselCMDs[i].visual.BorderBrush = Brushes.Yellow;
                                        curVesselCMDs[i].execStartedAt = executionTime;
                                    }
                                }
                            }
                        }
                    }

                    Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(plcSignals);

                    /**
                     * SEND CTRL TO ROBOTS! IT IS SUPPOSED, THAT ONLY TWO AGVS ARE USED!
                     */
                    for (int i = 0; i < curAGVCMDs.Length; i++)
                    {
                        if (curAGVCMDs[i] != null)
                        {
                            if (curAGVCMDs[i].state == DetailedProductionPlan.STATUS.WAITING)//Check if task is waiting to be executedD:\schoppmeyerc\MULTIFORM-PP-PCS-Framework\PipelessPlant_1\PipelessPlant_1\GUI\MenuIcons\
                            {
                                if (curAGVCMDs[i].startTime * 1000.0f <= executionTime)//Should be equal, otherwise task was delayed due to not met preconditions
                                {
                                    if (curAGVCMDs[i].preConditionsAndSynchMet())//START EXECUTION
                                    {
                                        curAGVCMDs[i].state = DetailedProductionPlan.STATUS.EXECUTING;
                                        curAGVCMDs[i].visual.BorderBrush = Brushes.Yellow;
                                        curAGVCMDs[i].execStartedAt = executionTime;

                                        if (curAGVCMDs[i].operation == DetailedProductionPlan.ELEMENTS.WAIT)
                                        {
                                            //NOOP
                                        }
                                        else if (curAGVCMDs[i].operation == DetailedProductionPlan.ELEMENTS.DOCK)
                                        {
                                            if (i == 0)//IT IS SUPPOSED, THAT ONLY TWO AGVS ARE USED!!!!!!!!!
                                            {
                                                if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                                                {
                                                    for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv1)
                                                        {
                                                            Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].Dock();
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            else//IT IS SUPPOSED, THAT ONLY TWO AGVS ARE USED!!!!!!!!!
                                            {
                                                if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                                                {
                                                    for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv2)
                                                        {
                                                            Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].Dock();
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (curAGVCMDs[i].operation == DetailedProductionPlan.ELEMENTS.UNDOCK)
                                        {
                                            if (i == 0)//IT IS SUPPOSED, THAT ONLY TWO AGVS ARE USED!!!!!!!!!
                                            {
                                                if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                                                {
                                                    for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv1)
                                                        {
                                                            Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].Undock();
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            else//IT IS SUPPOSED, THAT ONLY TWO AGVS ARE USED!!!!!!!!!
                                            {
                                                if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                                                {
                                                    for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv2)
                                                        {
                                                            Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].Undock();
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (curAGVCMDs[i].operation == DetailedProductionPlan.ELEMENTS.MOVE_TO)
                                        {
                                            /**
                                             * ROBOT MOVEMENT:
                                             * 0 - MIX RIGHT
                                             * 1 - MIX LEFt
                                             * 2 - COL1
                                             * 3 - COL2
                                             * 4 - CHA1
                                             * 5 - CHA2
                                             * 6 - STO RIGHT
                                             * 7 - STO LEFT
                                             * 8 - INI1
                                             * 9 - INI2
                                             */
                                            Gateway.ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle goal = null;
                                            if (curAGVCMDs[i].target == 0)//MIX RIGHT
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(193, 108.8), 320);
                                            }
                                            else if (curAGVCMDs[i].target == 1)//MIX LEFT
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(217, 108.8), 240);
                                            }
                                            else if (curAGVCMDs[i].target == 2)//COL1
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(127.8, 168), 200);
                                            }
                                            else if (curAGVCMDs[i].target == 3)//COL2
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(281.6, 168), 325);
                                            }
                                            else if (curAGVCMDs[i].target == 4)//CHA1
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(90.5, 210), 135);
                                            }
                                            else if (curAGVCMDs[i].target == 5)//CHA2
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(310, 210), 45);
                                            }
                                            else if (curAGVCMDs[i].target == 6)//STO RIGHT
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(217, 189.5), 115);
                                            }
                                            else if (curAGVCMDs[i].target == 7)//STO LEFT
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(193, 189.5), 65);
                                            }
                                            else if (curAGVCMDs[i].target == 8)//INI1
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(287, 40), 110);
                                            }
                                            else if (curAGVCMDs[i].target == 9)//INI2
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(367, 67), 135);
                                            }

                                            if (i == 0)//IT IS SUPPOSED, THAT ONLY TWO AGVS ARE USED!!!!!!!!!
                                            {
                                                if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                                                {
                                                    for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv1)
                                                        {
                                                            //Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].SetGoal(goal);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            else//IT IS SUPPOSED, THAT ONLY TWO AGVS ARE USED!!!!!!!!!
                                            {
                                                if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                                                {
                                                    for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv2)
                                                        {
                                                            //Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].SetGoal(goal);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (curAGVCMDs[i].operation == DetailedProductionPlan.ELEMENTS.SPECIAL_MOVE)
                                        {
                                            Gateway.ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle goal = null;
                                            if (curAGVCMDs[i].target == 0)//MIX RIGHT
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(193, 108.8), 320);
                                            }
                                            else if (curAGVCMDs[i].target == 1)//MIX LEFT
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(217, 108.8), 240);
                                            }
                                            else if (curAGVCMDs[i].target == 2)//COL1
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(127.8, 168), 200);
                                            }
                                            else if (curAGVCMDs[i].target == 3)//COL2
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(281.6, 168), 325);
                                            }
                                            else if (curAGVCMDs[i].target == 4)//CHA1
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(90.5, 210), 135);
                                            }
                                            else if (curAGVCMDs[i].target == 5)//CHA2
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(310, 210), 45);
                                            }
                                            else if (curAGVCMDs[i].target == 6)//STO RIGHT
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(217, 189.5), 115);
                                            }
                                            else if (curAGVCMDs[i].target == 7)//STO LEFT
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(193, 189.5), 65);
                                            }
                                            else if (curAGVCMDs[i].target == 8)//INI1
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(287, 40), 110);
                                            }
                                            else if (curAGVCMDs[i].target == 9)//INI2
                                            {
                                                goal = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(367, 67), 135);
                                            }

                                            Gateway.ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle goal2 = new ConnectionModule.iRobot.iRobotRouter.Robot.PosAngle(new ConnectionModule.iRobot.iRobotVector(367, 67), 135);

                                            if (i == 0)//IT IS SUPPOSED, THAT ONLY TWO AGVS ARE USED!!!!!!!!!
                                            {
                                                if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                                                {
                                                    for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv1)
                                                        {
                                                            //Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].SetGoal(goal);
                                                            break;
                                                        }
                                                    }
                                                    for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv2)
                                                        {
                                                            //Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].SetGoal(goal2);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            else//IT IS SUPPOSED, THAT ONLY TWO AGVS ARE USED!!!!!!!!!
                                            {
                                                if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
                                                {
                                                    for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv2)
                                                        {
                                                            //Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].SetGoal(goal);
                                                            break;
                                                        }
                                                    }
                                                    for (int j = 0; j < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; j++)
                                                    {
                                                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].RobotID == agv1)
                                                        {
                                                            //Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[j].SetGoal(goal2);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    /**
                     * LET TIME PASS
                     */
                    if (!delayed && !paused)
                    {
                        if (currentCol1CMD != null)
                        {
                            if (currentCol1CMD.state == DetailedProductionPlan.STATUS.EXECUTING)
                            {
                                currentCol1CMD.clock += cylceTimeInMilliseconds;
                            }
                        }
                        if (currentCol2CMD != null)
                        {
                            if (currentCol2CMD.state == DetailedProductionPlan.STATUS.EXECUTING)
                            {
                                currentCol2CMD.clock += cylceTimeInMilliseconds;
                            }
                        }
                        if (currentMixCMD != null)
                        {
                            if (currentMixCMD.state == DetailedProductionPlan.STATUS.EXECUTING)
                            {
                                currentMixCMD.clock += cylceTimeInMilliseconds;
                            }
                        }
                        if (currentStoCMD != null)
                        {
                            if (currentStoCMD.state == DetailedProductionPlan.STATUS.EXECUTING)
                            {
                                currentStoCMD.clock += cylceTimeInMilliseconds;
                            }
                        }
                        for (int i = 0; i < curAGVCMDs.Length; i++)
                        {
                            if (curAGVCMDs[i] != null)
                            {
                                if (curAGVCMDs[i].state == DetailedProductionPlan.STATUS.EXECUTING)
                                {
                                    curAGVCMDs[i].clock += cylceTimeInMilliseconds;
                                }
                            }
                        }
                        for (int i = 0; i < curVesselCMDs.Length; i++)
                        {
                            if (curVesselCMDs[i] != null)
                            {
                                if (curVesselCMDs[i].state == DetailedProductionPlan.STATUS.EXECUTING)
                                {
                                    curVesselCMDs[i].clock += cylceTimeInMilliseconds;
                                }
                            }
                        }
                        executionTime += (ulong)cylceTimeInMilliseconds; //MILISECONDS!
                    }
                }
                else
                {
                    GUI.PCSMainWindow.getInstance().postStatusMessage("Automatic execution not possible. No connection to PLC!");
                }

                TimeSpan t = TimeSpan.FromMilliseconds(executionTime);
                GUI.PCSMainWindow.getInstance().updateGanttChartUI(t);
                Gateway.ObserverModule.getInstance().modelChanged();

                if (paused)
                {
                    if (GUI.PCSMainWindow.getInstance().automatic.Fill == (Brush)GUI.PCSMainWindow.getInstance().FindResource("LGBYellow")
                        || GUI.PCSMainWindow.getInstance().automatic.Fill == (Brush)GUI.PCSMainWindow.getInstance().FindResource("LGBGreen"))
                    {
                        GUI.PCSMainWindow.getInstance().automatic.Fill = (Brush)GUI.PCSMainWindow.getInstance().FindResource("LGBRed");
                    }
                    else
                    {
                        if (debug)
                        {
                            GUI.PCSMainWindow.getInstance().automatic.Fill = (Brush)GUI.PCSMainWindow.getInstance().FindResource("LGBYellow");
                        }
                        else
                        {
                            GUI.PCSMainWindow.getInstance().automatic.Fill = (Brush)GUI.PCSMainWindow.getInstance().FindResource("LGBGreen");
                        }
                    }
                }

                //CHECK IF PRODUCTION FINISHED:
                if (currentCol1CMD != null && currentCol1CMD.state == DetailedProductionPlan.STATUS.FINISHED)
                {
                    if (col1Idx == detailedPlan.Col1Plan.Count - 1)
                    {
                        if (currentCol2CMD != null && currentCol2CMD.state == DetailedProductionPlan.STATUS.FINISHED)
                        {
                            if (col2Idx == detailedPlan.Col2Plan.Count - 1)
                            {
                                if (currentMixCMD != null && currentMixCMD.state == DetailedProductionPlan.STATUS.FINISHED)
                                {
                                    if (mixIdx == detailedPlan.MixPlan.Count - 1)
                                    {
                                        if (currentStoCMD != null && currentStoCMD.state == DetailedProductionPlan.STATUS.FINISHED)
                                        {
                                            if (stoIdx == detailedPlan.StoPlan.Count - 1)
                                            {
                                                bool found = false;
                                                for (int i = 0; i < curVesselCMDs.Length; i++)
                                                {
                                                    if (curVesselCMDs[i] != null && curVesselCMDs[i].state == DetailedProductionPlan.STATUS.FINISHED)
                                                    {
                                                        if (vIdcs[i] != detailedPlan.VPlans[i].Count - 1)
                                                        {
                                                            found = true; //one layer missing
                                                        }
                                                    }
                                                }
                                                if (!found)//FINISHED PRODUCTION
                                                {
                                                    executionEngine.Stop();
                                                    automaticCTRLRunning = false;
                                                    GUI.PCSMainWindow.getInstance().postStatusMessage("PRODUCTION FINISHED!");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                dog.Stop();
                Console.WriteLine(dog.ElapsedMilliseconds);
                dog.Reset();
            }
        }
        /**
         * START AUTOMATIC MODE
         */
        public void startExecution(int agv1, int agv2)
        {
            if (detailedPlan != null)
            {
                //TWO AVAILABLE AND SELECTED AGVS
                this.agv1 = agv1;
                this.agv2 = agv2;

                //FIRST STATION CMDS
                if (detailedPlan.Col1Plan.Count > 0)
                {
                    currentCol1CMD = detailedPlan.Col1Plan[0];
                    col1Idx = 0;
                }
                if (detailedPlan.Col2Plan.Count > 0)
                {
                    currentCol2CMD = detailedPlan.Col2Plan[0];
                    col2Idx = 0;
                }
                if (detailedPlan.MixPlan.Count > 0)
                {
                    currentMixCMD = detailedPlan.MixPlan[0];
                    mixIdx = 0;
                }
                if (detailedPlan.Col1Plan.Count > 0)
                {
                    currentStoCMD = detailedPlan.StoPlan[0];
                    stoIdx = 0;
                }
                //FIRST AGV CMDS
                curAGVCMDs = new DetailedProductionPlan.ProductionPlanEntry[detailedPlan.AgvPlans.Length];
                for (int i = 0; i < detailedPlan.AgvPlans.Length; i++)
                {
                    if (detailedPlan.AgvPlans[i].Count > 0)
                    {
                        curAGVCMDs[i] = detailedPlan.AgvPlans[i][0];
                        agvIdcs[i] = 0;
                    }
                }
                //FIRST V COMMANDS (HARDENING)
                curVesselCMDs = new DetailedProductionPlan.ProductionPlanEntry[detailedPlan.VPlans.Length];
                for (int i = 0; i < detailedPlan.VPlans.Length; i++)
                {
                    if (detailedPlan.VPlans[i].Count > 0)
                    {
                        curVesselCMDs[i] = detailedPlan.VPlans[i][0];
                        vIdcs[i] = 0;
                    }
                }

                causingDelay = new DetailedProductionPlan.ProductionPlanEntry[4 + curVesselCMDs.Length + curAGVCMDs.Length];
                GUI.PCSMainWindow.getInstance().addDelayAlarmLED("Delay Color1");
                GUI.PCSMainWindow.getInstance().addDelayAlarmLED("Delay Color2");
                GUI.PCSMainWindow.getInstance().addDelayAlarmLED("Delay Mixing");
                GUI.PCSMainWindow.getInstance().addDelayAlarmLED("Delay Storage");
                for(int i = 0; i  <curVesselCMDs.Length; i++)
                {
                    GUI.PCSMainWindow.getInstance().addDelayAlarmLED("Delay Vessel"+(i+1));
                }
                for (int i = 0; i < curAGVCMDs.Length; i++)
                {
                    GUI.PCSMainWindow.getInstance().addDelayAlarmLED("Delay AGV" + (i + 1));
                }

                plcSignals = new byte[64];

                GUI.PCSMainWindow.getInstance().postStatusMessage("Starting automatic execution of current production plan...");
                startExecutionEngine();
            }
            else
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("Automatic execution without detailed production plan is not possible. Please use TAOpt 4.3 to create a new schedule and detailed production plan!");
            }
        }
        /**
         * START THE EXECUTION ENGINE
         */
        private void startExecutionEngine()
        {
            if (!automaticCTRLRunning)
            {
                if (executionEngine != null)
                {
                    executionEngine.Stop();
                }
                else
                {
                    executionEngine = new DispatcherTimer();
                }
                executionEngine.Dispatcher.Thread.Priority = ThreadPriority.Highest;
                executionEngine.Interval = TimeSpan.FromMilliseconds(cylceTimeInMilliseconds);
                executionEngine.Tick += new EventHandler(CTRL_PLANT);
                executionEngine.Start();

                automaticCTRLRunning = true;

                if (debug)
                {
                    GUI.PCSMainWindow.getInstance().automatic.Fill = (Brush)GUI.PCSMainWindow.getInstance().FindResource("LGBYellow");
                }
                else
                {
                    GUI.PCSMainWindow.getInstance().automatic.Fill = (Brush)GUI.PCSMainWindow.getInstance().FindResource("LGBGreen");
                }
            }
        }
        /**
         * ABORT AUTOMATIC EXECUTION
         */
        public void ABORT_AUTOMATIC()//!!!!!!!!MAGNETS WILL BE TURNED OFF!!!
        {
            if (automaticCTRLRunning)
            {
                executionEngine.Stop();
                automaticCTRLRunning = false;
            }
        }
        #endregion;


        public ControlModules.CollisionModule.StandardModule collisionModule;
        public ControlModules.RobotModule.RobotModule[] robotModules;
        public ControlModules.RobotModule.RemoteModule[] robotRemotes;
        public ControlModules.StationModule.StationModule[] stationModules;
        public ControlModules.RoutingModule.RoutingModule routingModule;
        public ControlModules.CameraModule.CameraControl camCtrl = ControlModules.CameraModule.CameraControl.getInstance();

        #region simulationStuff;
        public string pathToDymola;
        private bool simulationRunning;
        public bool SimulationRunning
        {
            get { return simulationRunning; }
            set { simulationRunning = value; }
        }
        private bool simulationPaused;
        public bool SimulationPaused
        {
            get { return simulationPaused; }
            set { simulationPaused = value; }
        }
        /**
         * DYMOLA SIMULATION
         */
        public void startDymolaSimulation(bool dymola7)
        {
            if (Gateway.CTRLModule.getInstance().SimulationRunning)
            {
                return;
            }
            if (pathToDymola == "NaN")
            {
                Microsoft.Win32.OpenFileDialog dymolaPathDialog = new Microsoft.Win32.OpenFileDialog();
                dymolaPathDialog.InitialDirectory = Directory.GetCurrentDirectory();
                dymolaPathDialog.Filter = "Dymola Executeable | *.exe";
                dymolaPathDialog.Title = "Select Dymola.exe...";
                dymolaPathDialog.ShowDialog();
                FileInfo f = new FileInfo(dymolaPathDialog.FileName);
                pathToDymola = f.Directory.FullName;
            }

            System.Diagnostics.Process[] dymosimProcesses = System.Diagnostics.Process.GetProcessesByName("dymosim");
            for (int i = 0; i < dymosimProcesses.Length; i++)
            {
                dymosimProcesses[i].Kill();
            }
            System.Diagnostics.Process[] dymolaProcesses = System.Diagnostics.Process.GetProcessesByName("dymola");
            for (int i = 0; i < dymolaProcesses.Length; i++)
            {
                dymolaProcesses[i].Kill();
            }
            String programmname = pathToDymola + "\\Dymola.exe";
            System.Diagnostics.Process.Start(programmname);
            DateTime start = DateTime.Now;
            while (DateTime.Now < start.AddSeconds(3))
            {
                //Waiting for Dymola Start-Up of DDE Server.
            }
            String currentModelName = "";
            if (dymola7)
            {
                currentModelName = Path.GetFullPath(Gateway.ObserverModule.getInstance().getCurrentPlant().theName + ".mo");
            }
            else
            {
                currentModelName = Path.GetFullPath(Gateway.ObserverModule.getInstance().getCurrentPlant().theName + "_modelica2.mo");
            }
            Console.WriteLine("Open Model : " + currentModelName);
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().loadModelicaFileToDymola(currentModelName);
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().startSocketConnection();
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().SimulationDataUpdate_Recieved += new ConnectionModule.ConnectionCTRLModule.Data_RecievedEventHandler(CTRL_PLANT);
            start = DateTime.Now;
            while (DateTime.Now < start.AddSeconds(3))
            {
                //Waiting for Dymola Load of Model.
            }
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().simulateInDymola();
        }
        public void killDymolaSimulation()
        {
            System.Diagnostics.Process[] dymosimProcesses = System.Diagnostics.Process.GetProcessesByName("dymosim");
            for (int i = 0; i < dymosimProcesses.Length; i++)
            {
                dymosimProcesses[i].Kill();
            }
            System.Diagnostics.Process[] dymolaProcesses = System.Diagnostics.Process.GetProcessesByName("dymola");
            for (int i = 0; i < dymolaProcesses.Length; i++)
            {
                dymolaProcesses[i].Kill();
            }

            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().killiRobotSocketConnections();
        }
        #endregion;

        #region singleton pattern
        private static CTRLModule theCTRLModule;
        private CTRLModule()
        {
            simulationRunning = false;
            simulationPaused = false;
            pathToDymola = "NaN";
            automaticCTRLRunning = false;
        }
        public static CTRLModule getInstance()
        {
            if (theCTRLModule == null)
            {
                theCTRLModule = new CTRLModule();
            }
            return theCTRLModule;
        }
        #endregion

        #region ModuleAccess;
        public ControlModules.RobotModule.RemoteModule getRobotRemoteCTRL(Datastructure.Model.AGV.AGV robot)
        {
            for (int i = 0; i < robotRemotes.Length; i++)
            {
                if (robotRemotes[i].agvID == robot.Id)
                {
                    return robotRemotes[i];
                }
            }
            return null;
        }
        public ControlModules.RobotModule.RemoteModule getRobotRemoteCTRL(int id)
        {
            for (int i = 0; i < robotRemotes.Length; i++)
            {
                if (robotRemotes[i].agvID == id)
                {
                    return robotRemotes[i];
                }
            }
            return null;
        }
        public ControlModules.StationModule.StationModule getStationCTRL(Datastructure.Model.Stations.AbstractStation station)
        {
            for (int i = 0; i < stationModules.Length; i++)
            {
                if (stationModules[i].theStation == station)
                {
                    return stationModules[i];
                }
            }
            return null;
        }
        public ControlModules.StationModule.StationModule getStationCTRL(int id)
        {
            for (int i = 0; i < stationModules.Length; i++)
            {
                if (stationModules[i].theStation.theId == id)
                {
                    return stationModules[i];
                }
            }
            return null;
        }
        public ControlModules.StationModule.StationModule getStationCTRLAtPos(int pos)
        {
            return stationModules[pos];
        }
        #endregion;

        public void constructModules(Datastructure.Model.Plant cur_arena, int robotCTRLFlag, int stationCTRLFlag)
        {
            collisionModule = new ControlModules.CollisionModule.StandardModule();
            robotModules = new MULTIFORM_PCS.ControlModules.RobotModule.RobotModule[cur_arena.AllAGVs.Count];
            robotRemotes = new MULTIFORM_PCS.ControlModules.RobotModule.RemoteModule[cur_arena.AllAGVs.Count];
            routingModule = new ControlModules.RoutingModule.AStarModule();
  
            for (int i = 0; i < cur_arena.AllAGVs.Count; i++)
            {
                robotRemotes[i] = new ControlModules.RobotModule.RemoteModule(cur_arena.AllAGVs[i].Id);
                robotModules[i] = new ControlModules.RobotModule.SimpleFeedbackControl(cur_arena.AllAGVs[i].Id);

            }
            stationModules = new MULTIFORM_PCS.ControlModules.StationModule.StationModule[cur_arena.AllStations.Count];
            for (int i = 0; i < cur_arena.AllStations.Count; i++)
            {
                stationModules[i] = new ControlModules.StationModule.RemoteModule(cur_arena.AllStations[i]);
            }
        }
    }
}
