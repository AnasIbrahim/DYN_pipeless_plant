using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.Path
{
    using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.DataTypes;
    class Orientation
    {
        public List<double> getOrientations(List<Position> Path, string FinishPosition)
        {
            List<double> allOrientations = new List<double>();

            for (int i = 0; i < Path.Count-1; i++)
            {
                allOrientations.Add(Math.Atan2(Path[i + 1].Y - Path[i].Y, Path[i + 1].X - Path[i].X)*180/Math.PI);
            }
            //allOrientation[Path.Count-1]
            allOrientations.Add(getFinalOrientation(FinishPosition));

            return allOrientations;
        }

        public double getFinalOrientation(string FinalPosition)
        {
            /*Absolute orientations of the robot when it is located at one of the stations or initial positions*/
            double finalOrientation = 0;  // in degrees
            
            if (FinalPosition == "COL1")
                finalOrientation = 180;
            else if (FinalPosition == "COL2")
                finalOrientation = 0;
            else if (FinalPosition == "MIX")
                finalOrientation = 270;
            else if (FinalPosition == "STORAGE" || FinalPosition == "INIT1" || FinalPosition == "INIT2" || FinalPosition == "INIT3")
                finalOrientation = 90;

            return finalOrientation;
        }
    }
}
