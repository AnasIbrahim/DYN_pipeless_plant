using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.Datastructure.Schedule
{
    public class SequenceBuilder
    {
        #region singletonPattern;
        private static SequenceBuilder builder;
        public static SequenceBuilder getInstance()
        {
            if (builder == null)
            {
                builder = new SequenceBuilder();
            }
            return builder;
        }
        private SequenceBuilder()
        {
        }
        #endregion;

        public List<DetailedProductionPlan.ProductionPlanEntry> getStorageSequence(string seq, string timing, float seqStart, float seqEnd, int taskID, int recipeID)
        {
            /**
             * KNOWN SEQUENCES FOR STORAGE STATION:
             * -RELEASE+UNDOCK
             * -MOVE_INIT1_TO_STORAGE+DOCK+RELEASE+UNDOCK
             * -MOVE_INIT2_TO_STORAGE+DOCK+RELEASE+UNDOCK
             * -MOVE_MIX_TO_STORAGE+DOCK+GRAB
             * -MOVE_MIX_TO_STORAGE+MOVE_OTHER_AGV_TO_INIT+DOCK+GRAB
             */
            if (seq == "RELEASE+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> stoSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                stoSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.STORAGESTATION, DetailedProductionPlan.ELEMENTS.RELEASE, null, null, taskID + "-1", recipeID, recipeID));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                stoSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.STORAGESTATION, DetailedProductionPlan.ELEMENTS.UNDOCK, stoSeq[stoSeq.Count - 1], stoSeq[stoSeq.Count - 1], taskID + "-2", recipeID, -1));
                return stoSeq;
            }
            else if (seq == "MOVE_INIT1_TO_STORAGE+DOCK+RELEASE+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> stoSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                stoSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.STORAGESTATION, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 5));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                stoSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.STORAGESTATION, DetailedProductionPlan.ELEMENTS.DOCK, stoSeq[stoSeq.Count - 1], stoSeq[stoSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                stoSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.STORAGESTATION, DetailedProductionPlan.ELEMENTS.RELEASE, stoSeq[stoSeq.Count - 1], stoSeq[stoSeq.Count - 1], taskID + "-3", recipeID, recipeID));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                stoSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.STORAGESTATION, DetailedProductionPlan.ELEMENTS.UNDOCK, stoSeq[stoSeq.Count - 1], stoSeq[stoSeq.Count - 1], taskID + "-4", recipeID, -1));
                return stoSeq;
            }
            else if (seq == "MOVE_INIT2_TO_STORAGE+DOCK+RELEASE+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> stoSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                stoSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.STORAGESTATION, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 5));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                stoSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.STORAGESTATION, DetailedProductionPlan.ELEMENTS.DOCK, stoSeq[stoSeq.Count - 1], stoSeq[stoSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                stoSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.STORAGESTATION, DetailedProductionPlan.ELEMENTS.RELEASE, stoSeq[stoSeq.Count - 1], stoSeq[stoSeq.Count - 1], taskID + "-3", recipeID, recipeID));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                stoSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.STORAGESTATION, DetailedProductionPlan.ELEMENTS.UNDOCK, stoSeq[stoSeq.Count - 1], stoSeq[stoSeq.Count - 1], taskID + "-4", recipeID, -1));
                return stoSeq;
            }
            else if (seq == "MOVE_MIX_TO_STORAGE+DOCK+GRAB")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> stoSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                stoSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.STORAGESTATION, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 5));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                stoSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.STORAGESTATION, DetailedProductionPlan.ELEMENTS.DOCK, stoSeq[stoSeq.Count - 1], stoSeq[stoSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                stoSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.STORAGESTATION, DetailedProductionPlan.ELEMENTS.GRAB, stoSeq[stoSeq.Count - 1], stoSeq[stoSeq.Count - 1], taskID + "-3", recipeID, recipeID));
                return stoSeq;
            }
            else if (seq == "MOVE_MIX_TO_STORAGE+MOVE_OTHER_AGV_TO_INIT+DOCK+GRAB")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> stoSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                stoSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.STORAGESTATION, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 5));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);//SPECIAL CASE!
                stoSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.STORAGESTATION, DetailedProductionPlan.ELEMENTS.DOCK, stoSeq[stoSeq.Count - 1], stoSeq[stoSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);//SPECIAL CASE!
                stoSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.STORAGESTATION, DetailedProductionPlan.ELEMENTS.GRAB, stoSeq[stoSeq.Count - 1], stoSeq[stoSeq.Count - 1], taskID + "-3", recipeID, recipeID));
                return stoSeq;
            }
            else
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("Unknown sequence for StorageStation! Automatic control aborted!");
                throw new Exception();
            }
        }
        public List<DetailedProductionPlan.ProductionPlanEntry> getFillingSequence(bool col1, string seq, string timing, float seqStart, float seqEnd, int taskID, int recipeID)
        {
            if (col1)
            {
                /**
                 * KNOWN SEQUENCES FOR FILLING STATION 1:
                 * -MOVE_STORAGE_TO_COL1+DOCK+FILL_BLACK+UNDOCK
                 * -MOVE_STORAGE_TO_COL1+DOCK+FILL_YELLOW+UNDOCK
                 * -MOVE_STORAGE_TO_COL1+DOCK+FILL_GREEN+UNDOCK
                 * -MOVE_COL2_TO_COL1+DOCK+FILL_GREEN+UNDOCK
                 * -MOVE_STORAGE_TO_COL1+DOCK+FILL_ORANGE+UNDOCK
                 * -MOVE_COL2_TO_COL1+DOCK+FILL_ORANGE+UNDOCK
                 */
                if (seq == "MOVE_STORAGE_TO_COL1+DOCK+FILL_BLACK+UNDOCK")
                {
                    List<DetailedProductionPlan.ProductionPlanEntry> col1Seq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                    string[] timingSplit = timing.Split('+');
                    float curStart = seqStart;
                    float curEnd = seqStart + float.Parse(timingSplit[0]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 1));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[1]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.DOCK, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-2", recipeID, -1));
                    curStart = curEnd;
                    float half = float.Parse(timingSplit[2]) / 2.0f;
                    curEnd += half;
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.FILL, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-3", recipeID, 1));
                    curStart = curEnd;
                    curEnd += half;
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.FILL, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-3", recipeID, 1));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[3]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.UNDOCK, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-4", recipeID, -1));
                    return col1Seq;
                }
                else if (seq == "MOVE_STORAGE_TO_COL1+DOCK+FILL_YELLOW+UNDOCK")
                {
                    List<DetailedProductionPlan.ProductionPlanEntry> col1Seq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                    string[] timingSplit = timing.Split('+');
                    float curStart = seqStart;
                    float curEnd = seqStart + float.Parse(timingSplit[0]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 1));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[1]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.DOCK, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-2", recipeID, -1));
                    curStart = curEnd;
                    float half = float.Parse(timingSplit[2]) / 2.0f;
                    curEnd += half;
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.FILL, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-3", recipeID, 0));
                    curStart = curEnd;
                    curEnd += half;
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.FILL, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-3", recipeID, 0));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[3]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.UNDOCK, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-4", recipeID, -1));
                    return col1Seq;
                }
                else if (seq == "MOVE_STORAGE_TO_COL1+DOCK+FILL_GREEN+UNDOCK")
                {
                    List<DetailedProductionPlan.ProductionPlanEntry> col1Seq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                    string[] timingSplit = timing.Split('+');
                    float curStart = seqStart;
                    float curEnd = seqStart + float.Parse(timingSplit[0]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 1));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[1]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.DOCK, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-2", recipeID, -1));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[2]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.FILL, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-3", recipeID, 5));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[3]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.UNDOCK, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-4", recipeID, -1));
                    return col1Seq;
                }
                else if (seq == "MOVE_COL2_TO_COL1+DOCK+FILL_GREEN+UNDOCK")
                {
                    List<DetailedProductionPlan.ProductionPlanEntry> col1Seq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                    string[] timingSplit = timing.Split('+');
                    float curStart = seqStart;
                    float curEnd = seqStart + float.Parse(timingSplit[0]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 1));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[1]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.DOCK, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-2", recipeID, -1));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[2]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.FILL, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-3", recipeID, 5));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[3]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.UNDOCK, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-4", recipeID, -1));
                    return col1Seq;
                }
                else if (seq == "MOVE_STORAGE_TO_COL1+DOCK+FILL_ORANGE+UNDOCK")
                {
                    List<DetailedProductionPlan.ProductionPlanEntry> col1Seq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                    string[] timingSplit = timing.Split('+');
                    float curStart = seqStart;
                    float curEnd = seqStart + float.Parse(timingSplit[0]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 1));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[1]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.DOCK, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-2", recipeID, -1));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[2]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.FILL, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-3", recipeID, 6));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[3]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.UNDOCK, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-4", recipeID, -1));
                    return col1Seq;
                }
                else if (seq == "MOVE_COL2_TO_COL1+DOCK+FILL_ORANGE+UNDOCK")
                {
                    List<DetailedProductionPlan.ProductionPlanEntry> col1Seq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                    string[] timingSplit = timing.Split('+');
                    float curStart = seqStart;
                    float curEnd = seqStart + float.Parse(timingSplit[0]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 1));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[1]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.DOCK, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-2", recipeID, -1));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[2]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.FILL, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-3", recipeID, 6));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[3]);
                    col1Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION1, DetailedProductionPlan.ELEMENTS.UNDOCK, col1Seq[col1Seq.Count - 1], col1Seq[col1Seq.Count - 1], taskID + "-4", recipeID, -1));
                    return col1Seq;
                }
                else
                {
                    GUI.PCSMainWindow.getInstance().postStatusMessage("Unknown sequence for ColorStation1! Automatic control aborted!");
                    throw new Exception();
                }
            }
            else
            {
                /**
                 * KNOWN SEQUENCES FOR FILLING STATION 2:
                 * -MOVE_STORAGE_TO_COL2+DOCK+FILL_BLUE+UNDOCK
                 * -MOVE_STORAGE_TO_COL2+DOCK+FILL_RED+UNDOCK
                 * -MOVE_STORAGE_TO_COL2+DOCK+FILL_PURPLE+UNDOCK
                 * -MOVE_STORAGE_TO_COL2+DOCK+FILL_GREEN+UNDOCK
                 * -MOVE_COL1_TO_COL2+DOCK+FILL_GREEN+UNDOCK
                 * -MOVE_STORAGE_TO_COL2+DOCK+FILL_ORANGE+UNDOCK
                 * -MOVE_COL1_TO_COL2+DOCK+FILL_ORANGE+UNDOCK
                 */
                if (seq == "MOVE_STORAGE_TO_COL2+DOCK+FILL_BLUE+UNDOCK")
                {
                    List<DetailedProductionPlan.ProductionPlanEntry> col2Seq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                    string[] timingSplit = timing.Split('+');
                    float curStart = seqStart;
                    float curEnd = seqStart + float.Parse(timingSplit[0]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 2));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[1]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.DOCK, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-2", recipeID, -1));
                    curStart = curEnd;
                    float half = float.Parse(timingSplit[2]) / 2.0f;
                    curEnd += half;
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.FILL, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-3", recipeID, 3));
                    curStart = curEnd;
                    curEnd += half;
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.FILL, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-3", recipeID, 3));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[3]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.UNDOCK, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-4", recipeID, -1));
                    return col2Seq;
                }
                else if (seq == "MOVE_STORAGE_TO_COL2+DOCK+FILL_RED+UNDOCK")
                {
                    List<DetailedProductionPlan.ProductionPlanEntry> col2Seq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                    string[] timingSplit = timing.Split('+');
                    float curStart = seqStart;
                    float curEnd = seqStart + float.Parse(timingSplit[0]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 2));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[1]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.DOCK, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-2", recipeID, -1));
                    curStart = curEnd;
                    float half = float.Parse(timingSplit[2]) / 2.0f;
                    curEnd += half;
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.FILL, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-3", recipeID, 2));
                    curStart = curEnd;
                    curEnd += half;
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.FILL, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-3", recipeID, 2));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[3]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.UNDOCK, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-4", recipeID, -1));
                    return col2Seq;
                }
                else if (seq == "MOVE_STORAGE_TO_COL2+DOCK+FILL_PURPLE+UNDOCK")
                {
                    List<DetailedProductionPlan.ProductionPlanEntry> col2Seq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                    string[] timingSplit = timing.Split('+');
                    float curStart = seqStart;
                    float curEnd = seqStart + float.Parse(timingSplit[0]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 2));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[1]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.DOCK, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-2", recipeID, -1));
                    curStart = curEnd;
                    float half = float.Parse(timingSplit[2]) / 2.0f;
                    curEnd += half;
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.FILL, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-3", recipeID, 4));
                    curStart = curEnd;
                    curEnd += half;
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.FILL, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-3", recipeID, 7));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[3]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.UNDOCK, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-4", recipeID, -1));
                    return col2Seq;
                }
                else if (seq == "MOVE_STORAGE_TO_COL2+DOCK+FILL_GREEN+UNDOCK")
                {
                    List<DetailedProductionPlan.ProductionPlanEntry> col2Seq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                    string[] timingSplit = timing.Split('+');
                    float curStart = seqStart;
                    float curEnd = seqStart + float.Parse(timingSplit[0]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 2));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[1]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.DOCK, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-2", recipeID, -1));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[2]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.FILL, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-3", recipeID, 5));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[3]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.UNDOCK, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-4", recipeID, -1));
                    return col2Seq;
                }
                else if (seq == "MOVE_COL1_TO_COL2+DOCK+FILL_GREEN+UNDOCK")
                {
                    List<DetailedProductionPlan.ProductionPlanEntry> col2Seq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                    string[] timingSplit = timing.Split('+');
                    float curStart = seqStart;
                    float curEnd = seqStart + float.Parse(timingSplit[0]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 2));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[1]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.DOCK, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-2", recipeID, -1));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[2]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.FILL, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-3", recipeID, 5));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[3]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.UNDOCK, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-4", recipeID, -1));
                    return col2Seq;
                }
                else if (seq == "MOVE_STORAGE_TO_COL2+DOCK+FILL_ORANGE+UNDOCK")
                {
                    List<DetailedProductionPlan.ProductionPlanEntry> col2Seq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                    string[] timingSplit = timing.Split('+');
                    float curStart = seqStart;
                    float curEnd = seqStart + float.Parse(timingSplit[0]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 2));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[1]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.DOCK, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-2", recipeID, -1));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[2]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.FILL, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-3", recipeID, 6));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[3]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.UNDOCK, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-4", recipeID, -1));
                    return col2Seq;
                }
                else if (seq == "MOVE_COL1_TO_COL2+DOCK+FILL_ORANGE+UNDOCK")
                {
                    List<DetailedProductionPlan.ProductionPlanEntry> col2Seq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                    string[] timingSplit = timing.Split('+');
                    float curStart = seqStart;
                    float curEnd = seqStart + float.Parse(timingSplit[0]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 2));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[1]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.DOCK, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-2", recipeID, -1));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[2]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.FILL, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-3", recipeID, 6));
                    curStart = curEnd;
                    curEnd += float.Parse(timingSplit[3]);
                    col2Seq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.COLORSTATION2, DetailedProductionPlan.ELEMENTS.UNDOCK, col2Seq[col2Seq.Count - 1], col2Seq[col2Seq.Count - 1], taskID + "-4", recipeID, -1));
                    return col2Seq;
                }
                else
                {
                    GUI.PCSMainWindow.getInstance().postStatusMessage("Unknown sequence for ColorStation2! Automatic control aborted!");
                    throw new Exception();
                }
            }
        }
        public List<DetailedProductionPlan.ProductionPlanEntry> getMixingSequence(string seq, string timing, float seqStart, float seqEnd, int taskID, int recipeID, int layer)
        {
            /**
             * KNOWN SEQUENCES FOR MIXING STATION:
             * -MOVE_COL1_TO_MIX+DOCK+GRAB+MIX+RELEASE+UNDOCK
             * -MOVE_COL2_TO_MIX+DOCK+GRAB+MIX+RELEASE+UNDOCK
             */
            if (seq == "MOVE_COL1_TO_MIX+DOCK+GRAB+MIX+RELEASE+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> mixSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                mixSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.MIXINGSTATION, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 0));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                mixSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.MIXINGSTATION, DetailedProductionPlan.ELEMENTS.DOCK, mixSeq[mixSeq.Count - 1], mixSeq[mixSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                mixSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.MIXINGSTATION, DetailedProductionPlan.ELEMENTS.GRAB, mixSeq[mixSeq.Count - 1], mixSeq[mixSeq.Count - 1], taskID + "-3", recipeID, layer));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                mixSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.MIXINGSTATION, DetailedProductionPlan.ELEMENTS.MIX, mixSeq[mixSeq.Count - 1], mixSeq[mixSeq.Count - 1], taskID + "-4", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[4]);
                mixSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.MIXINGSTATION, DetailedProductionPlan.ELEMENTS.RELEASE, mixSeq[mixSeq.Count - 1], mixSeq[mixSeq.Count - 1], taskID + "-5", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[5]);
                mixSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.MIXINGSTATION, DetailedProductionPlan.ELEMENTS.UNDOCK, mixSeq[mixSeq.Count - 1], mixSeq[mixSeq.Count - 1], taskID + "-6", recipeID, -1));
                return mixSeq;
            }
            else if (seq == "MOVE_COL2_TO_MIX+DOCK+GRAB+MIX+RELEASE+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> mixSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                mixSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.MIXINGSTATION, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 0));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                mixSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.MIXINGSTATION, DetailedProductionPlan.ELEMENTS.DOCK, mixSeq[mixSeq.Count - 1], mixSeq[mixSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                mixSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.MIXINGSTATION, DetailedProductionPlan.ELEMENTS.GRAB, mixSeq[mixSeq.Count - 1], mixSeq[mixSeq.Count - 1], taskID + "-3", recipeID, layer));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                mixSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.MIXINGSTATION, DetailedProductionPlan.ELEMENTS.MIX, mixSeq[mixSeq.Count - 1], mixSeq[mixSeq.Count - 1], taskID + "-4", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[4]);
                mixSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.MIXINGSTATION, DetailedProductionPlan.ELEMENTS.RELEASE, mixSeq[mixSeq.Count - 1], mixSeq[mixSeq.Count - 1], taskID + "-5", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[5]);
                mixSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, DetailedProductionPlan.RESOURCE.MIXINGSTATION, DetailedProductionPlan.ELEMENTS.UNDOCK, mixSeq[mixSeq.Count - 1], mixSeq[mixSeq.Count - 1], taskID + "-6", recipeID, -1));
                return mixSeq;
            }
            else
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("Unknown sequence for MixingStation! Automatic control aborted!");
                throw new Exception();
            }
        }
        public List<DetailedProductionPlan.ProductionPlanEntry> getAGVSequence(string seq, string timing, float seqStart, float seqEnd, int taskID, int recipeID, int agvNumber, int layer)
        {
            DetailedProductionPlan.RESOURCE agv = DetailedProductionPlan.RESOURCE.NOTFOUND;
            if (agvNumber == 0)
            {
                agv = DetailedProductionPlan.RESOURCE.AGV1;
            }
            else if (agvNumber == 1)
            {
                agv = DetailedProductionPlan.RESOURCE.AGV2;
            }
            else if (agvNumber == 2)
            {
                agv = DetailedProductionPlan.RESOURCE.AGV3;
            }
            else if (agvNumber == 3)
            {
                agv = DetailedProductionPlan.RESOURCE.AGV4;
            }
            else if (agvNumber == 4)
            {
                agv = DetailedProductionPlan.RESOURCE.AGV5;
            }
            /**
             * KNOWN SEQUENCES FOR THE AGVS:
             * -RELEASE+UNDOCK
             * -MOVE_INIT1_TO_STORAGE+DOCK+RELEASE+UNDOCK
             * -MOVE_INIT2_TO_STORAGE+DOCK+RELEASE+UNDOCK
             * -MOVE_MIX_TO_STORAGE+DOCK+GRAB
             * -MOVE_MIX_TO_STORAGE+MOVE_OTHER_AGV_TO_INIT+DOCK+GRAB
             * -MOVE_STORAGE_TO_COL1+DOCK+FILL_BLACK+UNDOCK
             * -MOVE_STORAGE_TO_COL1+DOCK+FILL_YELLOW+UNDOCK
             * -MOVE_STORAGE_TO_COL1+DOCK+FILL_GREEN+UNDOCK
             * -MOVE_COL2_TO_COL1+DOCK+FILL_GREEN+UNDOCK
             * -MOVE_STORAGE_TO_COL1+DOCK+FILL_ORANGE+UNDOCK
             * -MOVE_COL2_TO_COL1+DOCK+FILL_ORANGE+UNDOCK
             * -MOVE_STORAGE_TO_COL2+DOCK+FILL_BLUE+UNDOCK
             * -MOVE_STORAGE_TO_COL2+DOCK+FILL_RED+UNDOCK
             * -MOVE_STORAGE_TO_COL2+DOCK+FILL_PURPLE+UNDOCK
             * -MOVE_STORAGE_TO_COL2+DOCK+FILL_GREEN+UNDOCK
             * -MOVE_COL1_TO_COL2+DOCK+FILL_GREEN+UNDOCK
             * -MOVE_STORAGE_TO_COL2+DOCK+FILL_ORANGE+UNDOCK
             * -MOVE_COL1_TO_COL2+DOCK+FILL_ORANGE+UNDOCK
             * -MOVE_COL1_TO_MIX+DOCK+GRAB+MIX+RELEASE+UNDOCK
             * -MOVE_COL2_TO_MIX+DOCK+GRAB+MIX+RELEASE+UNDOCK
             */
            if (seq == "RELEASE+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.RELEASE, null, null, taskID + "-1", recipeID, recipeID));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_INIT1_TO_STORAGE+DOCK+RELEASE+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 6));//STO RIGHT
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.RELEASE, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, recipeID));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_INIT2_TO_STORAGE+DOCK+RELEASE+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 6));//STO RIGHT
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.RELEASE, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, recipeID));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_MIX_TO_STORAGE+DOCK+GRAB")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 7));//STO LEFT
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.GRAB, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, recipeID));
                return agvSeq;
            }
            else if (seq == "MOVE_MIX_TO_STORAGE+MOVE_OTHER_AGV_TO_INIT+DOCK+GRAB")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.SPECIAL_MOVE, null, null, taskID + "-1", recipeID, 7));//STO LEFT
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);//SPECIAL CASE!
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);//SPECIAL CASE!
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.WAIT, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, recipeID));
                return agvSeq;
            }
            else if (seq == "MOVE_STORAGE_TO_COL1+DOCK+FILL_BLACK+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 2));//COL1
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.WAIT, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, 1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_STORAGE_TO_COL1+DOCK+FILL_YELLOW+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 2));//COL1
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.WAIT, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, 0));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_STORAGE_TO_COL1+DOCK+FILL_GREEN+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 2));//COL1
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.WAIT, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, 5));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_COL2_TO_COL1+DOCK+FILL_GREEN+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 2));//COL1
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.WAIT, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, 5));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_STORAGE_TO_COL1+DOCK+FILL_ORANGE+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 2));//COL1
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.WAIT, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, 6));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_COL2_TO_COL1+DOCK+FILL_ORANGE+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 2));//COL1
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.WAIT, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, 6));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_STORAGE_TO_COL2+DOCK+FILL_BLUE+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 3));//COL2
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.WAIT, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, 3));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_STORAGE_TO_COL2+DOCK+FILL_RED+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 3));//COL2
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.WAIT, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, 2));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_STORAGE_TO_COL2+DOCK+FILL_PURPLE+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 3));//COL2
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.WAIT, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, 4));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_STORAGE_TO_COL2+DOCK+FILL_GREEN+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 3));//COL2
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.WAIT, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, 5));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_COL1_TO_COL2+DOCK+FILL_GREEN+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 3));//COL2
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.WAIT, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, 5));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_STORAGE_TO_COL2+DOCK+FILL_ORANGE+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 3));//COL2
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.WAIT, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, 6));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_COL1_TO_COL2+DOCK+FILL_ORANGE+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 3));//COL2
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.WAIT, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, 6));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_COL1_TO_MIX+DOCK+GRAB+MIX+RELEASE+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 0));//MIX RIGHT
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.GRAB, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, layer));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.WAIT, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[4]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.RELEASE, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-5", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[5]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-6", recipeID, -1));
                return agvSeq;
            }
            else if (seq == "MOVE_COL2_TO_MIX+DOCK+GRAB+MIX+RELEASE+UNDOCK")
            {
                List<DetailedProductionPlan.ProductionPlanEntry> agvSeq = new List<DetailedProductionPlan.ProductionPlanEntry>();
                string[] timingSplit = timing.Split('+');
                float curStart = seqStart;
                float curEnd = seqStart + float.Parse(timingSplit[0]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.MOVE_TO, null, null, taskID + "-1", recipeID, 1));//MIX LEFT
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[1]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.DOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-2", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[2]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.GRAB, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-3", recipeID, layer));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[3]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.WAIT, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-4", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[4]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.RELEASE, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-5", recipeID, -1));
                curStart = curEnd;
                curEnd += float.Parse(timingSplit[5]);
                agvSeq.Add(new DetailedProductionPlan.ProductionPlanEntry(curStart, curEnd, agv, DetailedProductionPlan.ELEMENTS.UNDOCK, agvSeq[agvSeq.Count - 1], agvSeq[agvSeq.Count - 1], taskID + "-6", recipeID, -1));
                return agvSeq;
            }
            else
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("Unknown sequence for AGV! Automatic control aborted!");
                throw new Exception();
            }
        }
    }
}
