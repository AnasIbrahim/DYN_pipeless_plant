using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MULTIFORM_PCS.ControlModules.RoutingModule
{
    public class AGVData
    {
        public decimal startTime;
        public decimal endTime;
        public string startPosition;
        public string endPosition;
        public string agvSequence;
        public string usedAGV;
        public int taskID;
        //public bool flag_taskContainsOTHER;   // the task contains a sequence with "OTHER"
        /*public enum AGV
        {
            AGV1,
            AGV2,
            AGV3
        }*/

        public AGVData(int ID, string usedAGV, decimal startT, decimal endT, string start, string target )//, bool flag_other) // string sequence)
        {
            //this.flag_taskContainsOTHER = flag_other;
            this.taskID = ID;
            this.usedAGV = usedAGV;
            this.startTime = startT;
            this.endTime = endT;
            //this.agvSequence = sequence;
            this.startPosition = start;
            this.endPosition = target;
        }
    }
}
