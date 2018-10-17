using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.Datastructure.Model.Stations
{
    public abstract class OneArmStation : AbstractStation
    {
        private double altitude;
        public double theAltitude
        {
            get { return altitude; }
            set { altitude = value; }
        }
        private double armVelocity;
        public double theArmVelocity
        {
            get { return armVelocity; }
            set { armVelocity = value; }
        }
        private double[] armpositions;
        public double[] theArmpositions
        {
            get { return armpositions; }
            set { armpositions = value; }
        }

        public OneArmStation(int id, String name, double x, double y, double length, double width, double rotation, double dockingX, double dockingY, double dockingRot, double altitude, double armVelocity, double[] vArmPositions)
            : base(id, name, x, y, length, width, rotation, dockingX, dockingY, dockingRot)
        {
            this.altitude = altitude;
            this.armVelocity = armVelocity;
            this.theArmpositions = vArmPositions;
        }

        public int getArmPos()
        {
            for (int i = 0; i < armpositions.Length; i++)
            {
                if (altitude > armpositions[i] - 0.5d && altitude < armpositions[i] + 0.5d)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
