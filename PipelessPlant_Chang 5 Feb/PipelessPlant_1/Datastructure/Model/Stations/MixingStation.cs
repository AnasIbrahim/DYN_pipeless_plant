using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.Datastructure.Model.Stations
{
    public class MixingStation : OneArmStation
    {
        private Vessel.Vessel curVessel;
        public Vessel.Vessel theCurrentVessel
        {
            get { return curVessel; }
            set { curVessel = value; }
        }
        private bool rotating;
        public bool theRotating
        {
            get { return rotating; }
            set { rotating = value; }
        }
        private double fillRate;
        public double theFillRate
        {
            get { return fillRate; }
            set { fillRate = value; }
        }
        private double sensor;
        public double theSensor
        {
            get { return sensor; }
            set { sensor = value; }
        }

        public MixingStation(int id, String name, double x, double y, double length, double width, double rotation, double dockingX, double dockingY, double dockingRot, double altitude, double armVelocity, double[] vArmPositions, Vessel.Vessel curCon, bool rotating, double fillRate, double sensor)
            : base(id, name, x, y, length, width, rotation, dockingX, dockingY, dockingRot, altitude, armVelocity, vArmPositions)
        {
            this.curVessel = curCon;
            this.rotating = rotating;
            this.fillRate = fillRate;
            this.sensor = sensor;
        }

        public override bool isFillingStation()
        {
            return false;
        }
        public override bool isMixingStation()
        {
            return true;
        }
        public override bool isChargingStation()
        {
            return false;
        }
        public override bool isStorageStation()
        {
            return false;
        }
    }
}
