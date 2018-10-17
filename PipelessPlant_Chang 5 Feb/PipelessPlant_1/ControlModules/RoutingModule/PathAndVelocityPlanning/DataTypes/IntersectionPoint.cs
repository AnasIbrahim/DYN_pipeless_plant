using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.DataTypes
{
    using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.Velocity;
    class IntersectionPoint
    {
        
        public Position intersectPoint;
        public List<Position> additionalPoints;   // two additional points(for each intersection Point) for identifying on which side of the collision point the forbidden points are located
        public int indexPath1;   //first Path involved in intersection
        public int indexPath2;   //second Path involved in intersection 

        public decimal[] priority = new decimal[2];
        //public int X;
        //public int Y;
  
        public IntersectionPoint(Position intersetion, int indexPath1, int indexPath2, decimal[] priority, List<Position> additionalpoints)
        {
            this.additionalPoints = additionalpoints;
            this.priority = priority;
            this.intersectPoint = intersetion;
            this.indexPath1 = indexPath1;
            this.indexPath2 = indexPath2;
        }

    }
}
