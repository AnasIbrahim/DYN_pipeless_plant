using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.StationModule
{
    abstract class StationModule
    {
        protected Datastructure.Model.Stations.AbstractStation station;
        public Datastructure.Model.Stations.AbstractStation theStation
        {
            get { return station; }
            set { station = value; }
        }
        public abstract void lockContainer();
        public abstract void releaseContainer();
        public abstract void startLoading();
        public abstract void stopLoading();
        public abstract void startMixing();
        public abstract void stopMixing();
        public abstract void startFilling(int color);
        public abstract void stopFilling(int color);
        public abstract void moveVArm(int position, double speed);
        public abstract void moveHArm(int position, double speed);
        public abstract void stop();
    }
}
