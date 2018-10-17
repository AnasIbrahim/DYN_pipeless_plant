using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.RobotModule
{
    class SimpleFeedbackControl : RobotModule
    {
        private bool reached;
        public bool Reached
        {
            get { return reached; }
            set { reached = value; }
        }
        private double x;
        public double X
        {
            get { return x; }
            set { x = value; }
        }
        private double y;
        public double Y
        {
            get { return y; }
            set { y = value; }
        }
        private double rot;
        public double Rot
        {
            get { return rot; }
            set { rot = value; }
        }
        public void setGoal(double x, double y, double rot)
        {
            this.x = x;
            this.y = y;
            this.rot = rot;
        }

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
        public override void dock()
        {
          throw new NotImplementedException();
        }
        public override void unDock()
        {
          throw new NotImplementedException();
        }
        public override void goToPoint(double max_velocity, double goalX, double goalY, double goalRot)
        {
          Datastructure.Model.Plant p = Gateway.ObserverModule.getInstance().getCurrentPlant();
          Datastructure.Model.General.Position curP = p.getAGV(agvID).theCurPosition;
          double curR = p.getAGV(agvID).theRotation;

          if (Math.Abs(curP.X - goalX) <= 20.0d && Math.Abs(curP.Y - goalY) <= 20.0d)
          {
            rotateToGoalRotation(max_velocity, goalRot);
          }
          else
          {
            stop();
            double neededRotation = curR;
            double distance = 0;
            if (curP.X > goalX && curP.Y > goalY)
            {
              double b = curP.X - goalX;
              double a = curP.Y - goalY;
              double c = Math.Sqrt(a * a + b * b);
              distance = c;
              neededRotation = 180 + (Math.Acos(b / c) * 180 / Math.PI);
            }
            else if (curP.X > goalX && curP.Y < goalY)
            {
              double b = curP.X - goalX;
              double a = goalY - curP.Y;
              double c = Math.Sqrt(a * a + b * b);
              distance = c;
              neededRotation = 180 - (Math.Acos(b / c) * 180 / Math.PI);
            }
            else if (curP.X < goalX && curP.Y < goalY)
            {
              double b = goalX - curP.X;
              double a = goalY - curP.Y;
              double c = Math.Sqrt(a * a + b * b);
              distance = c;
              neededRotation = (Math.Acos(b / c) * 180 / Math.PI);
            }
            else if (curP.X < goalX && curP.Y > goalY)
            {
              double b = goalX - curP.X;
              double a = curP.Y - goalY;
              double c = Math.Sqrt(a * a + b * b);
              distance = c;
              neededRotation = 360 - (Math.Acos(b / c) * 180 / Math.PI);
            }
            else if (curP.X == goalX && curP.Y > goalY)
            {
              double a = curP.Y - goalY;
              double c = Math.Sqrt(a * a);
              distance = c;
              neededRotation = 270;
            }
            else if (curP.X == goalX && curP.Y < goalY)
            {
              double a = curP.Y - goalY;
              double c = Math.Sqrt(a * a);
              distance = c;
              neededRotation = 90;
            }
            else if (curP.X > goalX && curP.Y == goalY)
            {
              double b = goalX - curP.X;
              double c = Math.Sqrt(b * b);
              distance = c;
              neededRotation = 180;
            }
            else if (curP.X < goalX && curP.Y == goalY)
            {
              double b = goalX - curP.X;
              double c = Math.Sqrt(b * b);
              distance = c;
              neededRotation = 0;
            }
            if (Math.Abs(neededRotation - curR) > 2.0d)
            {
              if (Math.Abs(neededRotation - curR) > 180)
              {
                if (neededRotation < curR)
                {
                  if (Math.Abs(neededRotation - curR) > 355)
                  {
                    turnLeft(25, 0, 0);
                    //return;
                  }
                  else
                  {
                    turnLeft(max_velocity / 2, 0, 0);
                    //return;
                  }
                }
                else
                {
                  if (Math.Abs(neededRotation - curR) > 355)
                  {
                    turnRight(25, 0, 0);
                    //return;
                  }
                  else
                  {
                    turnRight(max_velocity / 2, 0, 0);
                    //return;
                  }
                }
              }
              else
              {
                if (neededRotation < curR)
                {
                  if (Math.Abs(neededRotation - curR) < 5)
                  {
                    turnRight(25, 0, 0);
                    //return;
                  }
                  else
                  {
                    turnRight(max_velocity / 3, 0, 0);
                    //return;
                  }
                }
                else
                {
                  if (Math.Abs(neededRotation - curR) < 5)
                  {
                    turnLeft(25, 0, 0);
                    //return;
                  }
                  else
                  {
                    turnLeft(max_velocity / 3, 0, 0);
                    //return;
                  }
                }
              }
            }
            //else
            {
              if (distance > 50)
              {
                forward(max_velocity, 0, 0, 0);
                return;
              }
              else if (distance > 10)
              {
                forward(75, 0, 0, 0);
                return;
              }
              else
              {
                forward(30, 0, 0, 0);
                return;
              }
            }
          }
        }
        private void rotateToGoalRotation(double max_velocity, double goalRot)
        {
          Datastructure.Model.Plant p = Gateway.ObserverModule.getInstance().getCurrentPlant();
          double curR = p.getAGV(agvID).theRotation;

          if (Math.Abs(goalRot - curR) > 2.0d)
          {
            if (Math.Abs(goalRot - curR) > 180)
            {
              if (goalRot < curR)
              {
                if (Math.Abs(goalRot - curR) > 355)
                {
                  turnLeft(25, 0, 0);
                  //return;
                }
                else
                {
                  turnLeft(max_velocity/2, 0, 0);
                  //return;
                }
              }
              else
              {
                if (Math.Abs(goalRot - curR) > 355)
                {
                  turnRight(25, 0, 0);
                  //return;
                }
                else
                {
                  turnRight(max_velocity/2, 0, 0);
                  //return;
                }
              }
            }
            else
            {
              if (goalRot < curR)
              {
                if (Math.Abs(goalRot - curR) < 10)
                {
                  turnRight(25, 0, 0);
                  //return;
                }
                else
                {
                  turnRight(max_velocity / 3, 0, 0);
                  //return;
                }
              }
              else
              {
                if (Math.Abs(goalRot - curR) < 10)
                {
                  turnLeft(25, 0, 0);
                  //return;
                }
                else
                {
                  turnLeft(max_velocity / 3, 0, 0);
                  //return;
                }
              }
            }
          }
          else
          {
            stop();
            reached = true;
            GUI.PCSMainWindow.getInstance().postStatusMessage("Robot (" + agvID + "): Position reached!");
            //Gateway.CTRLModule.getInstance().robotHasPosition[agvID] = false;
            return;
          }
        }
        

        
        public SimpleFeedbackControl(int id)
            : base(id)
        {
       

        }
    }
}
