using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.Datastructure.Model.Stations
{
    public class ChargingStation : AbstractStation
    {
        private double loadRate;
        public double theLoadRate
        {
            get { return loadRate; }
            set { loadRate = value; }
        }

        public ChargingStation(double loadRate, int id, String name, double x, double y, double length, double width, double rotation, double dockingX, double dockingY, double dockingRot)
            : base(id, name, x, y, length, width, rotation, dockingX, dockingY,dockingRot)
        {
            this.loadRate = loadRate;
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
            return true;
        }
        public override bool isStorageStation()
        {
            return false;
        }
    }
}
