using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.Datastructure.Model.Stations
{
    public class FillingStation : AbstractStation
    {
        private Vessel.Vessel curCon;
        public Vessel.Vessel theCurContainer
        {
            get { return curCon; }
            set { curCon = value; }
        }
        private int[] colorID;
        public int[] theColorID
        {
            get { return colorID; }
            set { colorID = value; }
        }
        private double fillRate_1;
        public double theFillRate_1
        {
            get { return fillRate_1; }
            set { fillRate_1 = value; }
        }
        private double fillRate_2;
        public double theFillRate_2
        {
            get { return fillRate_2; }
            set { fillRate_2 = value; }
        }

        public FillingStation(int id, String name, double x, double y, double length, double width, double rotation, double dockingX, double dockingY, double dockingRot, Vessel.Vessel curCon, int color1_ID, int color2_ID, double fillRate_1, double fillRate_2)
            : base(id, name, x, y, length, width, rotation, dockingX, dockingY, dockingRot)
        {
            this.curCon = curCon;
            this.colorID = new int[] {color1_ID, color2_ID};
            this.fillRate_1 = fillRate_1;
            this.fillRate_2 = fillRate_2;
        }

        public override bool isFillingStation()
        {
            return true;
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
            return false;
        }
    }
}
