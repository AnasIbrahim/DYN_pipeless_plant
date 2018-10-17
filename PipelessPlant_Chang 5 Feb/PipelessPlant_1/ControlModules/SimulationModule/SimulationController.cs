using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MULTIFORM_PCS.ControlModules.SimulationModule
{
    public class SimulationController
    {
        #region datastructure;
        private Datastructure.Schedule.Schedule currentSchedule;
        public Datastructure.Schedule.Schedule CurrentSchedule
        {
            get { return currentSchedule; }
            set { currentSchedule = value; }
        }
        private bool running;
        //private Thread simCTRLThread;
        #endregion;

        #region singletonPattern;
        private static SimulationController ctrl;
        public static SimulationController getInstance()
        {
            if (ctrl == null)
            {
                ctrl = new SimulationController();
            }
            return ctrl;
        }
        private SimulationController()
        {
            running = false;
        }
        #endregion;

        #region members;
        public void startSimulation()
        {
            if (currentSchedule != null)
            {
                if (!running)
                {

                }
            }
        }
        #endregion;
    }
}
