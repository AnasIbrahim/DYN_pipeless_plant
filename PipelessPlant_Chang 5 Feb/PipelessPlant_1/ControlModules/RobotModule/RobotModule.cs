using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.RobotModule
{
    public abstract class RobotModule
    {
        public int agvID;
        public abstract void forward(double velocity, double duration, double goalX, double goalY);
        public abstract void backward(double velocity, double duration, double goalX, double goalY);
        public abstract void turnLeft(double velocity, double duration, double goalRot);
        public abstract void turnRight(double velocity, double duration, double goalRot);
        public abstract void stop();
        public abstract void goToPoint(double max_velocity, double goalX, double goalY, double goalRot);
        public abstract void dock();
        public abstract void unDock();


        public RobotModule(int agvID)
        {
            this.agvID = agvID;
        }
    }
}
