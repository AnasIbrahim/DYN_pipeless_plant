using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.Datastructure.Model.Stations
{
    public class StorageStation : AbstractStation
    {
        private double altitude;
        public double theAltitude
        {
            get { return altitude; }
            set { altitude = value; }
        }
        private double traverse;
        public double theTraverse
        {
            get { return traverse; }
            set { traverse = value; }
        }
        private Vessel.Vessel[] container;
        public Vessel.Vessel[] theVessels
        {
            get { return container; }
            set { container = value; }
        }
        private double[] harmpositions;
        public double[] theHArmPositions
        {
            get { return harmpositions; }
            set { harmpositions = value; }
        }
        private double[] armVelocity;
        public double[] theArmVelocity
        {
            get { return armVelocity; }
            set { armVelocity = value; }
        }
        private double[] varmpositions;
        public double[] theVArmPositions
        {
            get { return varmpositions; }
            set { varmpositions = value; }
        }

        public StorageStation(int id, String name, double x, double y, double length, double width, double rotation, double dockingX, double dockingY, double dockingRot, double altitude, double traverse, Vessel.Vessel[] curCons, double[] hArmPositions, double[] armVelocity, double[] vArmPositions)
            : base(id, name, x, y, length, width, rotation, dockingX, dockingY, dockingRot)
        {
            this.altitude = altitude;
            this.traverse = traverse;
            this.container = curCons;
            this.harmpositions = hArmPositions;
            this.armVelocity = armVelocity;
            this.varmpositions = vArmPositions;
        }

        public override bool isFillingStation()
        {
            return false;
        }
        public override bool isMixingStation()
        {
            return false;
        }
        public override bool isChargingStation()
        {
            return false;
        }
        public override bool isStorageStation()
        {
            return true;
        }

        public int getHArmPos()
        {
            for (int i = 0; i < harmpositions.Length; i++)
            {
                if (traverse > harmpositions[i] - 0.5d && traverse < harmpositions[i] + 0.5d)
                {
                    return i;
                }
            }
            return 0;
        }
        public int getVArmPos()
        {
            for (int i = 0; i < varmpositions.Length; i++)
            {
                if (altitude > varmpositions[i] - 0.5d && altitude < varmpositions[i] + 0.5d)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
