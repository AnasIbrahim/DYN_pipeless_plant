using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.ConnectionModule
{
    class RobotBatteryStatusSubscriber
    {
        public static RoutingModule.AGVMovmentData agvMovementData;
        public static decimal[] startTimes = new decimal[] { };
        public static List<RoutingModule.AGVData> initialAGVinfo = new List<RoutingModule.AGVData>();

        public class SubscriberBatteryAGV1{

            public string path;
            
            //public Routing.PathAndVelocityPlanning.PAndVPlanning pAndv = new Routing.PathAndVelocityPlanning.PAndVPlanning(agvMovementData, startTimes);
            //private string id;
            public SubscriberBatteryAGV1(RobotBatteryStatusPublishers.PublisherBatteryAGV1 pub, RoutingModule.PathAndVelocityPlanning.PAndVPlanning pAndv)//, string path,string[] AGVusedForRecipeID)
            {
                //id = ID;
                pub.lowBatteryEvent += pAndv.HandleAGVLowBatt;
                //this.path = path;
                //agvMovementData = new Routing.AGVMovmentData(path, AGVusedForRecipeID, initialAGVinfo);
            }
            SubscriberBatteryAGV1(string path, string[] AGVusedForRecipeID)
            {
                
            }
        }

        public class SubscriberBatteryAGV2
        {

            //private string id;
            public string path;
            public static RoutingModule.AGVMovmentData agvMovementData;
            //public Routing.PathAndVelocityPlanning.PAndVPlanning pAndv = new Routing.PathAndVelocityPlanning.PAndVPlanning(agvMovementData,startTimes);
            public SubscriberBatteryAGV2(RobotBatteryStatusPublishers.PublisherBatteryAGV2 pub, RoutingModule.PathAndVelocityPlanning.PAndVPlanning pAndv)//, string path, string[] AGVusedForRecipeID)
            {
                //id = ID;
                pub.lowBatteryEvent += pAndv.HandleAGVLowBatt;
                //this.path = path;
                //agvMovementData = new Routing.AGVMovmentData(path, AGVusedForRecipeID, initialAGVinfo);
            }
            
        }

        public class SubscriberBatteryAGV3
        {
            public string path;
            //private string id;
            public static RoutingModule.AGVMovmentData agvMovementData;
            //public Routing.PathAndVelocityPlanning.PAndVPlanning pAndv = new Routing.PathAndVelocityPlanning.PAndVPlanning(agvMovementData,startTimes);
            public SubscriberBatteryAGV3(RobotBatteryStatusPublishers.PublisherBatteryAGV3 pub, RoutingModule.PathAndVelocityPlanning.PAndVPlanning pAndv)//, string path,string[] AGVusedForRecipeID)
            {
                //id = ID;
                //this.path = path;
                pub.lowBatteryEvent += pAndv.HandleAGVLowBatt;
                //agvMovementData = new Routing.AGVMovmentData(path, AGVusedForRecipeID, initialAGVinfo);
            }
        }
    }
}

