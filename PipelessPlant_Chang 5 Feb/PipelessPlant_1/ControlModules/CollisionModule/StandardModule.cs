using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.CollisionModule
{
    class StandardModule : CollisionModule
    {
        public override bool checkCollision(Datastructure.Model.AGV.AGV robot)
        {
            Datastructure.Model.Plant p = Gateway.ObserverModule.getInstance().getCurrentPlant();
            //Check robot leaving plant
            if (robot.theCurPosition.X - robot.Diameter / 2 <= 0
                || robot.theCurPosition.X + robot.Diameter / 2 >= p.theSize.Width
                || robot.theCurPosition.Y - robot.Diameter / 2 <= 0
                || robot.theCurPosition.Y + robot.Diameter / 2 >= p.theSize.Height)
            {
                return true;
            }
            //Check collision with other robots
            for (int i = 0; i < p.AllAGVs.Count; i++)
            {
                if (p.AllAGVs[i].Id == robot.Id)
                {
                    continue;
                }
                if (Math.Sqrt(
                    Math.Pow(robot.theCurPosition.X - p.AllAGVs[i].theCurPosition.X, 2)  
                    + Math.Pow(robot.theCurPosition.Y - p.AllAGVs[i].theCurPosition.Y,2))
                    <= robot.Diameter)
                {
                    return true;
                }
            }
            //Check collision with stations
            for (int i = 0; i < p.AllStations.Count; i++)
            {
                if (robot.Diameter > p.AllStations[i].theSize.Width)
                {
                    if (Math.Sqrt(
                        Math.Pow(robot.theCurPosition.X - p.AllStations[i].thePosition.X, 2)
                        + Math.Pow(robot.theCurPosition.Y - p.AllStations[i].thePosition.Y, 2))
                        <= robot.Diameter)
                    {
                        return true;
                    }
                }
                else
                {
                    if (Math.Sqrt(
                        Math.Pow(robot.theCurPosition.X - p.AllStations[i].thePosition.X, 2)
                        + Math.Pow(robot.theCurPosition.Y - p.AllStations[i].thePosition.Y, 2))
                        <= p.AllStations[i].theSize.Width)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
