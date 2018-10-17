using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.Gateway
{
    public class ObserverModule
    {
        #region singleton pattern
        private static ObserverModule theObserverModul;
        private ObserverModule() { }
        public static ObserverModule getInstance()
        {
            if (theObserverModul == null)
            {
                theObserverModul = new ObserverModule();
            }
            return theObserverModul;
        }
        #endregion

        private Datastructure.Model.Plant currentPlant;
        private Datastructure.Model.Plant predictionArena;
        private int observerID;

        public void regVisual(GUI.PCSMainWindow mainW)
        {
            theVisual = mainW;
        }
        private GUI.PCSMainWindow visual;
        public GUI.PCSMainWindow theVisual
        {
            get { return visual; }
            set { visual = value; }
        }

        private Gateway.ConnectionModule.iRobot.iRobotRouter router;
        public Gateway.ConnectionModule.iRobot.iRobotRouter theRouter {
          get { return router; }
          set { router = value; }
        }

        public Datastructure.Model.Plant getCurrentPlant()
        {
            return currentPlant;
        }
        public int getObserverID()
        {
            return observerID;
        }
        public void setCurrentPlant(Datastructure.Model.Plant arena)
        {
            this.currentPlant = arena;
        }
        public void setPredictionArena(Datastructure.Model.Plant pred)
        {
            this.predictionArena = pred;
        }
        public Datastructure.Model.Plant getPredictionArena()
        {
            return predictionArena;
        }

        public ObserverModule(Datastructure.Model.Plant eineArena, int id)
        {
            this.currentPlant = eineArena;
            this.observerID = id;
        }
        public ObserverModule(int id)
        {
            this.observerID = id;
        }

        public void modelChanged()
        {
            if (theVisual != null)
            {
                theVisual.Dispatcher.Invoke(new Action(
                    delegate()
                    {
                        theVisual.updatePlantView(getCurrentPlant());
                    }
                ));
            }
            if (theRouter != null) {
              theVisual.Dispatcher.Invoke(new Action(() => { theRouter.update(getCurrentPlant()); }));
            }
        }
    }
}
