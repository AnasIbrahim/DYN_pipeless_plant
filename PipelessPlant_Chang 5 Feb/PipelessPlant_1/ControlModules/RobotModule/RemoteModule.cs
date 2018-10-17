using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;

namespace MULTIFORM_PCS.ControlModules.RobotModule
{
    public class RemoteModule : RobotModule
    {
        public override void forward(double velocity, double duration, double goalX, double goalY)
        {
            double[] vector = Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().getCTRLVectorForRobot(agvID);
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForRobot(agvID, velocity, vector[1], 0, 0, 0, 0, 0);
        }
        public override void backward(double velocity, double duration, double goalX, double goalY)
        {
            double[] vector = Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().getCTRLVectorForRobot(agvID);
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForRobot(agvID, -velocity, vector[1], 0, 0, 0, 0, 0);
        }
        public override void turnLeft(double velocity, double duration, double goalRot)
        {
            double[] vector = Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().getCTRLVectorForRobot(agvID);
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForRobot(agvID, vector[0], velocity, 0, 0, 0, 0, 0);
        }
        public override void turnRight(double velocity, double duration, double goalRot)
        {
            double[] vector = Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().getCTRLVectorForRobot(agvID);
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForRobot(agvID, vector[0], -velocity, 0, 0, 0, 0, 0);
        }
        public override void stop()
        {
            double[] vector = Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().getCTRLVectorForRobot(agvID);
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForRobot(agvID, 0, 0, 0, 0, 0, 0, 0);
        }
        public override void goToPoint(double max_velocity, double goalX, double goalY, double goalRot)
        {
            throw new NotImplementedException();
        }
        public override void dock()
        {
          //IROBOT SERVER
          if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
          {
              if (!Gateway.CTRLModule.getInstance().AutomaticCTRLRunning)
              {
                  for (int i = 0; i < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; i++)
                  {
                      if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[i].RobotID == agvID)
                      {
                          Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[i].Dock();
                          return;
                      }
                  }
              }
              else
              {
                  GUI.PCSMainWindow.getInstance().postStatusMessage("MANUAL CMD BLOCKED, AUTOMATIC RUNNING...");
              }
          }
        }
        public override void unDock()
        {
            //IROBOT SERVER
            if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().ServerRunning)
            {
                if (!Gateway.CTRLModule.getInstance().AutomaticCTRLRunning)
                {
                    for (int i = 0; i < Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots.Count; i++)
                    {
                        if (Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[i].RobotID == agvID)
                        {
                            Gateway.ConnectionModule.iRobot.iRobotServer.getInstance().router.robots[i].Undock();
                            return;
                        }
                    }
                }
                else
                {
                    GUI.PCSMainWindow.getInstance().postStatusMessage("MANUAL CMD BLOCKED, AUTOMATIC RUNNING...");
                }
            }
        }
        



        public RemoteModule(int id)
            : base(id)
        {
        }
    }
}
