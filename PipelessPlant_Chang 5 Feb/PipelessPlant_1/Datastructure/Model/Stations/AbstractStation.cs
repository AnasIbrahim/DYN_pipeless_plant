using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.Datastructure.Model.Stations
{
    public abstract class AbstractStation
    {
        private int id;
        public int theId
        {
            get { return id; }
            set { id = value; }
        }
        private String name;
        public String theName
        {
            get { return name; }
            set { name = value; }
        }
        private Datastructure.Model.General.Position position;
        public Datastructure.Model.General.Position thePosition
        {
            get { return position; }
            set { position = value; }
        }
        private Datastructure.Model.General.Size size;
        public Datastructure.Model.General.Size theSize
        {
            get { return size; }
            set { size = value; }
        }
        private double rotation;
        public double theRotation
        {
            get { return rotation; }
            set { rotation = value; }
        }
        private Datastructure.Model.General.Position dockingPos;
        public Datastructure.Model.General.Position theDockingPos
        {
            get { return dockingPos; }
            set { dockingPos = value; }
        }
        private double dockingRot;
        public double theDockingRot
        {
            get { return dockingRot; }
            set { dockingRot = value; }
        }

        public AbstractStation(int id, String name, double x, double y, double length, double width, double rotation, double dockingX, double dockingY, double dockingRot)
        {
            this.id = id;
            this.name = name;
            this.position = new Datastructure.Model.General.Position(x, y);
            this.size = new Datastructure.Model.General.Size(length, width);
            this.rotation = rotation;
            this.dockingPos = new Datastructure.Model.General.Position(dockingX, dockingY);
            this.dockingRot = dockingRot;
        }

        public abstract bool isMixingStation();
        public abstract bool isFillingStation();
        public abstract bool isChargingStation();
        public abstract bool isStorageStation();
        public string getLength()
        {
            string rtn = "" + size.Height;
            rtn = rtn.Replace(',', '.');
            return rtn;
        }
        public string getWidth()
        {
            string rtn = "" + size.Width;
            rtn = rtn.Replace(',', '.');
            return rtn;
        }
    }
}
