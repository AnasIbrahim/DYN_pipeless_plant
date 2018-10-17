using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace MULTIFORM_PCS.Datastructure.Schedule
{
    public class DetailedProductionPlan
    {
        public class ProductionPlanEntry
        {
            public float startTime; //SECONDS!
            public float endTime;//SECONDS!
            public RESOURCE res;
            public STATUS state;
            public ELEMENTS operation;
            public ProductionPlanEntry preconditionSameRecipe;
            public ProductionPlanEntry preconditionSameResource;
            public ProductionPlanEntry synchCondition;
            public string taskID;
            public int recipeID;
            public int target;

            public bool preConditionsMet()
            {
                if ((preconditionSameRecipe == null || preconditionSameRecipe.state == STATUS.FINISHED)
                    && (preconditionSameResource == null || preconditionSameResource.state == STATUS.FINISHED))
                {
                    return true;
                }
                return false;
            }
            public bool preConditionsAndSynchMet()
            {
                if ((preconditionSameRecipe == null || preconditionSameRecipe.state == STATUS.FINISHED)
                    && (preconditionSameResource == null || preconditionSameResource.state == STATUS.FINISHED)
                    && (synchCondition == null || synchCondition.preConditionsMet()))
                {
                    return true;
                }
                return false;
            }

            /**
             * DATA FOR VISUAL
             */
            public Border visual;
            //public Label indicator;

            /**
             * DATA FOR EXECUTION
             */
            public float execStartedAt;//MILLISECONDS!
            public float clock;//MILLISECONDS!
            public float execEndedAt;//MILLISECONDS!

            public ProductionPlanEntry(float start, float end, RESOURCE res, ELEMENTS op, ProductionPlanEntry preCondSameRecipe, ProductionPlanEntry preCondSameResource, string taskID, int recipeID, int tar)
            {
                this.state = STATUS.WAITING;
                this.startTime = start;
                this.endTime = end;
                this.res = res;
                this.operation = op;
                this.preconditionSameRecipe = preCondSameRecipe;
                this.preconditionSameResource = preCondSameResource;
                this.synchCondition = null;
                this.taskID = taskID;
                this.recipeID = recipeID;
                this.target = tar;
            }
        }

        private List<ProductionPlanEntry> overallStationProductionPlan;
        public List<ProductionPlanEntry> OverallStationProductionPlan
        {
            get { return overallStationProductionPlan; }
            set { overallStationProductionPlan = value; }
        }
        private List<ProductionPlanEntry> overallAGVProductionPlan;
        public List<ProductionPlanEntry> OverallAGVProductionPlan
        {
            get { return overallAGVProductionPlan; }
            set { overallAGVProductionPlan = value; }
        }

        private List<ProductionPlanEntry> col1Plan;
        public List<ProductionPlanEntry> Col1Plan
        {
            get { return col1Plan; }
            set { col1Plan = value; }
        }
        private List<ProductionPlanEntry> col2Plan;
        public List<ProductionPlanEntry> Col2Plan
        {
            get { return col2Plan; }
            set { col2Plan = value; }
        }
        private List<ProductionPlanEntry> mixPlan;
        public List<ProductionPlanEntry> MixPlan
        {
            get { return mixPlan; }
            set { mixPlan = value; }
        }
        private List<ProductionPlanEntry> stoPlan;
        public List<ProductionPlanEntry> StoPlan
        {
            get { return stoPlan; }
            set { stoPlan = value; }
        }
        private List<ProductionPlanEntry>[] agvPlans;
        public List<ProductionPlanEntry>[] AgvPlans
        {
            get { return agvPlans; }
            set { agvPlans = value; }
        }
        private List<ProductionPlanEntry>[] vPlans;
        public List<ProductionPlanEntry>[] VPlans
        {
            get { return vPlans; }
            set { vPlans = value; }
        }

        public DetailedProductionPlan(Schedule s)
        {
            List<int> recipeIDs = new List<int>();
            List<List<string[]>> bestScheduleByRecipes = new List<List<string[]>>();
            for (int i = 0; i < s.BestSchedule.Count; i++)
            {
                int taskID = int.Parse(s.BestSchedule[i][1]);
                int recipeID = taskID / 10000;

                bool added = false;
                for (int j = 0; j < recipeIDs.Count; j++)
                {
                    if (recipeIDs[j] == recipeID)
                    {
                        bestScheduleByRecipes[j].Add(s.BestSchedule[i]);
                        added = true;
                        break;
                    }
                }
                if (!added)
                {
                    bestScheduleByRecipes.Add(new List<string[]>());
                    bestScheduleByRecipes[bestScheduleByRecipes.Count - 1].Add(s.BestSchedule[i]);
                    recipeIDs.Add(recipeID);
                }
            }

            overallStationProductionPlan = new List<ProductionPlanEntry>();
            overallAGVProductionPlan = new List<ProductionPlanEntry>();

            for (int i = 0; i < bestScheduleByRecipes.Count; i++)
            {
                ProductionPlanEntry lastInserted = null;
                int layer = 0;
                for (int j = 0; j < bestScheduleByRecipes[i].Count; j++)
                {
                    int unit = int.Parse(bestScheduleByRecipes[i][j][0]);
                    int taskID = int.Parse(bestScheduleByRecipes[i][j][1]);
                    float start = float.Parse(bestScheduleByRecipes[i][j][2]);
                    float end = float.Parse(bestScheduleByRecipes[i][j][3]);
                    int recipeID = taskID / 10000;

                    for (int k = 0; k < s.Sequence_mapping.Count; k++)
                    {
                        if (int.Parse(s.Sequence_mapping[k][3]) == taskID)
                        {
                            if (s.Sequence_mapping[k][5] == "HARDEN")
                            {
                                overallStationProductionPlan.Add(new ProductionPlanEntry(start, end, getVesselForRecipeID(recipeID), ELEMENTS.HARDEN, lastInserted, null, taskID + "-1", recipeID,layer));
                                lastInserted = overallStationProductionPlan[overallStationProductionPlan.Count - 1];
                                layer++;
                                continue;
                            }
                            else if (s.Sequence_mapping[k][5] == "DUMMY")
                            {
                                //NOTHING TO DO HERE SO FAR!
                                continue;
                            }
                            else
                            {
                                int usedAGV = -1;
                                for (int l = 0; l < s.AgvData.Count; l++) //FIND THE AGV USED FOR THIS TASK!
                                {
                                    for (int m = 1; m < s.AgvData[l].Count - 1; m++)
                                    {
                                        if (s.AgvData[l][m][1] < 10)
                                        {
                                            continue; //TODO: ADD MOVEMENT FROM INIT/BACK TO INIT
                                        }
                                        float startAGVMovement = s.AgvData[l][m - 1][0];
                                        float endAGVMovement = s.AgvData[l][m + 1][0];

                                        int movementForRecipeID = (int)s.AgvData[l][m][1] - 10;

                                        if (recipeID == movementForRecipeID)
                                        {
                                            if (start >= startAGVMovement && end <= endAGVMovement)
                                            {
                                                usedAGV = l;
                                                break;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                }

                                List<ProductionPlanEntry> newSeqAGV = SequenceBuilder.getInstance().getAGVSequence(s.Sequence_mapping[k][5], s.Sequence_mapping[k][6], start, end, taskID, recipeID, usedAGV, layer);
                                overallAGVProductionPlan.AddRange(newSeqAGV);

                                if (unit == 0) //Storage
                                {
                                    List<ProductionPlanEntry> newSeqSto = SequenceBuilder.getInstance().getStorageSequence(s.Sequence_mapping[k][5], s.Sequence_mapping[k][6], start, end, taskID, recipeID);
                                    newSeqSto[0].preconditionSameRecipe = lastInserted;
                                    overallStationProductionPlan.AddRange(newSeqSto);
                                    lastInserted = overallStationProductionPlan[overallStationProductionPlan.Count - 1];

                                    for (int l = 0; l < newSeqSto.Count; l++)
                                    {
                                        newSeqSto[l].synchCondition = newSeqAGV[l];
                                        newSeqAGV[l].synchCondition = newSeqSto[l];
                                    }
                                }
                                else if (unit == 1) //Col1
                                {
                                    List<ProductionPlanEntry> newSeqCol1 = SequenceBuilder.getInstance().getFillingSequence(true, s.Sequence_mapping[k][5], s.Sequence_mapping[k][6], start, end, taskID, recipeID);
                                    newSeqCol1[0].preconditionSameRecipe = lastInserted;
                                    overallStationProductionPlan.AddRange(newSeqCol1);
                                    lastInserted = overallStationProductionPlan[overallStationProductionPlan.Count - 1];

                                    if (newSeqCol1.Count > newSeqAGV.Count)
                                    {
                                        newSeqCol1[0].synchCondition = newSeqAGV[0];
                                        newSeqAGV[0].synchCondition = newSeqCol1[0];
                                        newSeqCol1[1].synchCondition = newSeqAGV[1];
                                        newSeqAGV[1].synchCondition = newSeqCol1[1];
                                        newSeqCol1[2].synchCondition = newSeqAGV[2];
                                        newSeqAGV[2].synchCondition = newSeqCol1[3];
                                        newSeqCol1[3].synchCondition = newSeqAGV[2];
                                        //newSeqAGV[2].synchCondition = newSeqCol1[3];
                                        newSeqCol1[4].synchCondition = newSeqAGV[3];
                                        newSeqAGV[3].synchCondition = newSeqCol1[4];
                                    }
                                    else
                                    {
                                        for (int l = 0; l < newSeqCol1.Count; l++)
                                        {
                                            newSeqCol1[l].synchCondition = newSeqAGV[l];
                                            newSeqAGV[l].synchCondition = newSeqCol1[l];
                                        }
                                    }
                                }
                                else if (unit == 2) //Col2
                                {
                                    List<ProductionPlanEntry> newSeqCol2 = SequenceBuilder.getInstance().getFillingSequence(false, s.Sequence_mapping[k][5], s.Sequence_mapping[k][6], start, end, taskID, recipeID);
                                    newSeqCol2[0].preconditionSameRecipe = lastInserted;
                                    overallStationProductionPlan.AddRange(newSeqCol2);
                                    lastInserted = overallStationProductionPlan[overallStationProductionPlan.Count - 1];

                                    if (newSeqCol2.Count > newSeqAGV.Count)
                                    {
                                        newSeqCol2[0].synchCondition = newSeqAGV[0];
                                        newSeqAGV[0].synchCondition = newSeqCol2[0];
                                        newSeqCol2[1].synchCondition = newSeqAGV[1];
                                        newSeqAGV[1].synchCondition = newSeqCol2[1];
                                        newSeqCol2[2].synchCondition = newSeqAGV[2];
                                        newSeqAGV[2].synchCondition = newSeqCol2[3];
                                        newSeqCol2[3].synchCondition = newSeqAGV[2];
                                        //newSeqAGV[2].synchCondition = newSeqCol2[3];
                                        newSeqCol2[4].synchCondition = newSeqAGV[3];
                                        newSeqAGV[3].synchCondition = newSeqCol2[4];
                                    }
                                    else
                                    {
                                        for (int l = 0; l < newSeqCol2.Count; l++)
                                        {
                                            newSeqCol2[l].synchCondition = newSeqAGV[l];
                                            newSeqAGV[l].synchCondition = newSeqCol2[l];
                                        }
                                    }
                                }
                                else if (unit == 3) //Mix
                                {
                                    List<ProductionPlanEntry> newSeqMix = SequenceBuilder.getInstance().getMixingSequence(s.Sequence_mapping[k][5], s.Sequence_mapping[k][6], start, end, taskID, recipeID, layer);
                                    newSeqMix[0].preconditionSameRecipe = lastInserted;
                                    overallStationProductionPlan.AddRange(newSeqMix);
                                    lastInserted = overallStationProductionPlan[overallStationProductionPlan.Count - 1];

                                    for (int l = 0; l < newSeqMix.Count; l++)
                                    {
                                        newSeqMix[l].synchCondition = newSeqAGV[l];
                                        newSeqAGV[l].synchCondition = newSeqMix[l];
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }

            /**
             * ADD RESOURCE BASED DEPENDENCIES!
             */
            List<ProductionPlanEntry> allStorageTasks = overallStationProductionPlan.FindAll(delegate(ProductionPlanEntry p) { return p.res == RESOURCE.STORAGESTATION; });
            allStorageTasks.Sort((x, y) => x.startTime.CompareTo(y.startTime));
            for (int i = 1; i < allStorageTasks.Count; i++)
            {
                if (allStorageTasks[i].preconditionSameResource == null)
                {
                    allStorageTasks[i].preconditionSameResource = allStorageTasks[i - 1];
                }
            }
            stoPlan = allStorageTasks;
            List<ProductionPlanEntry> allMixingTasks = overallStationProductionPlan.FindAll(delegate(ProductionPlanEntry p) { return p.res == RESOURCE.MIXINGSTATION; });
            allMixingTasks.Sort((x, y) => x.startTime.CompareTo(y.startTime));
            for (int i = 1; i < allMixingTasks.Count; i++)
            {
                if (allMixingTasks[i].preconditionSameResource == null)
                {
                    allMixingTasks[i].preconditionSameResource = allMixingTasks[i - 1];
                }
            }
            mixPlan = allMixingTasks;
            List<ProductionPlanEntry> allCol1Tasks = overallStationProductionPlan.FindAll(delegate(ProductionPlanEntry p) { return p.res == RESOURCE.COLORSTATION1; });
            allCol1Tasks.Sort((x, y) => x.startTime.CompareTo(y.startTime));
            for (int i = 1; i < allCol1Tasks.Count; i++)
            {
                if (allCol1Tasks[i].preconditionSameResource == null)
                {
                    allCol1Tasks[i].preconditionSameResource = allCol1Tasks[i - 1];
                }
            }
            col1Plan = allCol1Tasks;
            List<ProductionPlanEntry> allCol2Tasks = overallStationProductionPlan.FindAll(delegate(ProductionPlanEntry p) { return p.res == RESOURCE.COLORSTATION2; });
            allCol2Tasks.Sort((x, y) => x.startTime.CompareTo(y.startTime));
            for (int i = 1; i < allCol2Tasks.Count; i++)
            {
                if (allCol2Tasks[i].preconditionSameResource == null)
                {
                    allCol2Tasks[i].preconditionSameResource = allCol2Tasks[i - 1];
                }
            }
            col2Plan = allCol2Tasks;
            List<ProductionPlanEntry> allCha1Tasks = overallStationProductionPlan.FindAll(delegate(ProductionPlanEntry p) { return p.res == RESOURCE.CHARGINGSTATION1; });
            allCha1Tasks.Sort((x, y) => x.startTime.CompareTo(y.startTime));
            for (int i = 1; i < allCha1Tasks.Count; i++)
            {
                if (allCha1Tasks[i].preconditionSameResource == null)
                {
                    allCha1Tasks[i].preconditionSameResource = allCha1Tasks[i - 1];
                }
            }
            List<ProductionPlanEntry> allCha2Tasks = overallStationProductionPlan.FindAll(delegate(ProductionPlanEntry p) { return p.res == RESOURCE.CHARGINGSTATION2; });
            allCha2Tasks.Sort((x, y) => x.startTime.CompareTo(y.startTime));
            for (int i = 1; i < allCha2Tasks.Count; i++)
            {
                if (allCha2Tasks[i].preconditionSameResource == null)
                {
                    allCha2Tasks[i].preconditionSameResource = allCha2Tasks[i - 1];
                }
            }
            vPlans = new List<ProductionPlanEntry>[6];
            for (int j = 0; j < 6; j++)
            {
                DetailedProductionPlan.RESOURCE v = DetailedProductionPlan.RESOURCE.NOTFOUND;
                if (j == 0)
                {
                    v = DetailedProductionPlan.RESOURCE.VESSEL1;
                }
                else if (j == 1)
                {
                    v = DetailedProductionPlan.RESOURCE.VESSEL2;
                }
                else if (j == 2)
                {
                    v = DetailedProductionPlan.RESOURCE.VESSEL3;
                }
                else if (j == 3)
                {
                    v = DetailedProductionPlan.RESOURCE.VESSEL4;
                }
                else if (j == 4)
                {
                    v = DetailedProductionPlan.RESOURCE.VESSEL5;
                }
                else if (j == 5)
                {
                    v = DetailedProductionPlan.RESOURCE.VESSEL6;
                }
                List<ProductionPlanEntry> allVTasks = overallStationProductionPlan.FindAll(delegate(ProductionPlanEntry p) { return p.res == v; });
                allVTasks.Sort((x, y) => x.startTime.CompareTo(y.startTime));
                for (int i = 1; i < allVTasks.Count; i++)
                {
                    if (allVTasks[i].preconditionSameResource == null)
                    {
                        allVTasks[i].preconditionSameResource = allVTasks[i - 1];
                    }
                }
                vPlans[j] = allVTasks;
            }

            agvPlans = new List<ProductionPlanEntry>[5];
            for (int j = 0; j < 5; j++)
            {
                DetailedProductionPlan.RESOURCE agv = DetailedProductionPlan.RESOURCE.NOTFOUND;
                if (j == 0)
                {
                    agv = DetailedProductionPlan.RESOURCE.AGV1;
                }
                else if (j == 1)
                {
                    agv = DetailedProductionPlan.RESOURCE.AGV2;
                }
                else if (j == 2)
                {
                    agv = DetailedProductionPlan.RESOURCE.AGV3;
                }
                else if (j == 3)
                {
                    agv = DetailedProductionPlan.RESOURCE.AGV4;
                }
                else if (j == 4)
                {
                    agv = DetailedProductionPlan.RESOURCE.AGV5;
                }
                List<ProductionPlanEntry> allAGVTasks = overallAGVProductionPlan.FindAll(delegate(ProductionPlanEntry p) { return p.res == agv; });
                allAGVTasks.Sort((x, y) => x.startTime.CompareTo(y.startTime));
                for (int i = 1; i < allAGVTasks.Count; i++)
                {
                    if (allAGVTasks[i].preconditionSameResource == null)
                    {
                        allAGVTasks[i].preconditionSameResource = allAGVTasks[i - 1];
                    }
                }
                agvPlans[j] = allAGVTasks;
            }
        }


        public enum RESOURCE
        {
            AGV1,
            AGV2,
            AGV3,
            AGV4,
            AGV5,
            AGV6,

            VESSEL1,
            VESSEL2,
            VESSEL3,
            VESSEL4,
            VESSEL5,
            VESSEL6,

            COLORSTATION1,
            COLORSTATION2,

            MIXINGSTATION,

            STORAGESTATION,

            CHARGINGSTATION1,
            CHARGINGSTATION2,

            NOTFOUND
        }

        public enum STATUS
        {
            WAITING,
            EXECUTING,
            FINISHED
        }

        public enum ELEMENTS
        {
            MOVE_TO,
            SPECIAL_MOVE,
            ROTATE_TO,
            DOCK,
            UNDOCK,
            WAIT,

            GRAB,
            RELEASE,
            //V_MOVE_TO_POS,
            //H_MOVE_TO_POS,
            FILL,
            MIX,

            HARDEN
        }

        public enum TARGET
        {
            YELLOW,
            RED,
            BLACK,
            GREEN,
            PURPLE,
            BLUE,
            ORANGE,

            MIX,
            COL1,
            COL2,
            CHA1,
            CHA2,
            STO,
            INI1,
            INI2,

            L1,
            L2,
            L3,

            P1,
            P2,
            P3,
            P4,
            P5,
            P6
        }

        private RESOURCE getVesselForRecipeID(int resID) //can only be 1-6
        {
            if (resID == 1)
            {
                return RESOURCE.VESSEL1;
            }
            else if (resID == 2)
            {
                return RESOURCE.VESSEL2;
            }
            else if (resID == 3)
            {
                return RESOURCE.VESSEL3;
            }
            else if (resID == 4)
            {
                return RESOURCE.VESSEL4;
            }
            else if (resID == 5)
            {
                return RESOURCE.VESSEL5;
            }
            else if (resID == 6)
            {
                return RESOURCE.VESSEL6;
            }
            return RESOURCE.NOTFOUND;
        }
    }
}
