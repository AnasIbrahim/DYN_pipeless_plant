using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace MULTIFORM_PCS.Datastructure.Model.AGV
{
    public class AGV
    {


        private double diameter;
        public double Diameter
        {
            get { return diameter; }
            set { diameter = value; }
        }
        private Datastructure.Model.General.Position curPosition;
        public Datastructure.Model.General.Position theCurPosition
        {
            get { return curPosition; }
            set { curPosition = value; }
        }
        private double rotation;
        public double theRotation
        {
            get { return rotation; }
            set { rotation = value; }
        }
        private DateTime lastUpdateCam;

        public DateTime LastUpdateCam {
          get { return lastUpdateCam; }
          set { lastUpdateCam = value; }
        }
        private int id;
        public int Id
        {
            get { return id; }
            set { id = value;}
        }
        private Vessel.Vessel vessel;
        public Vessel.Vessel theVessel
        {
            get { return vessel; }
            set { vessel = value; }
        }
        private double batteryLoad;
        public double theBatteryLoad
        {
            get { return batteryLoad; }
            set { batteryLoad = value; }
        }
        private string chargingStatus;
        public string theChargingStatus
        {
          get { return chargingStatus; }
          set { chargingStatus = value; }
        }
        private int goalX;
        public int GoalX
        {
          get { return goalX; }
          set { goalX = value; }
        }
        private int goalY;
        public int GoalY
        {
          get { return goalY; }
          set { goalY = value; }
        }
        private double goalRot;
        public double GoalRot
        {
          get { return goalRot; }
          set { goalRot = value; }
        }
        private bool seen;

        public bool Seen
        {
          get { return seen; }
          set { seen = value; }
        }
        public Action firstSeen;
        private double shadowX;
        public double ShadowX
        {
          get { return shadowX; }
          set { shadowX = value; }
        }
        private double shadowY;
        public double ShadowY
        {
          get { return shadowY; }
          set { shadowY = value; }
        }
        private double shadowRot;
        public double ShadowRot
        {
          get { return shadowRot; }
          set { shadowRot = value; }
        }

        public AGV(int id, Vessel.Vessel initialVessel, double initialPositionX, double initialPositionY, double initialRotation, double diameter, double initialBatteryLoad)
        {
            this.shadowX = -50;
            this.shadowY = -50;
            this.diameter = diameter;
            this.curPosition = new Datastructure.Model.General.Position(initialPositionX, initialPositionY);
            this.id = id;
            this.vessel = initialVessel;
            this.batteryLoad = initialBatteryLoad;
            this.rotation = initialRotation;
            this.goalRot = -1;
            this.goalX = -1;
            this.goalY = -1;
            this.seen = false;
        }
    }
}
