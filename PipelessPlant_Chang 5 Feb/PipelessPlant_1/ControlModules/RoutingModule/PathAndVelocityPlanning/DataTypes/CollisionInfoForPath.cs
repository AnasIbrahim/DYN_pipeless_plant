using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.DataTypes
{
    using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.Velocity;

    class CollisionInfoForPath
    {
        public string usedAGV;
        public int pathIndex;
        public int taskID;
        public int numberOfCollisions;
        //public List<int> indexOfCollisions;
        public decimal priority;
        public List<STspace[][]> forbiddenAreas;
        public int numberOfForbiddenRegions;
        public Position[] correspondingForbiddenPoints;
        //public bool flagNoCollision;
        public decimal totalAdditionalTime;   /* in case of a collision the robot is not going to finish its task as planned. 
                                             this variable is used to save the additional time the robot needs to finish its task */

        public int additionalTimeSteps;       /*additional time steps needs to be considered in the ST grid wrt interpolation value of T*/
        public decimal pathFinishingTime;
        public decimal[,] velocityProfile;
        public STspace[] updatedPathST;
        public Position[] updatedPosition;
        public List<double> allOrientations;
        public List<Position> path;

        
    }
}
