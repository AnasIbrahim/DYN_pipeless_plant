using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace MULTIFORM_PCS.ControlModules.StationModule
{
    class RemoteModule : StationModule
    {
        public override void lockContainer()
        {
            double[] vector;
            vector = Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().getCTRLVectorForStation(station);
            if (station.isMixingStation())
            {
                vector[3] = 1;
            }
            else
            {
                vector[2] = 1;
            }
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForStation(station, vector);
        }
        public override void releaseContainer()
        {
            double[] vector;
            vector = Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().getCTRLVectorForStation(station);
            if (station.isMixingStation())
            {
                vector[3] = -1;
            }
            else
            {
                vector[2] = -1;
            }
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForStation(station, vector);
        }
        public override void startLoading()
        {
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForStation(station, new double[] { 1 });
        }
        public override void stopLoading()
        {
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForStation(station, new double[] { 0 });
        }
        public override void startMixing()
        {
            double[] vector = Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().getCTRLVectorForStation(station);
            vector[2] = 1;
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForStation(station, vector);
        }
        public override void stopMixing()
        {
            double[] vector = Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().getCTRLVectorForStation(station);
            vector[2] = 0;
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForStation(station, vector);
        }
        public override void startFilling(int color)
        {
            double[] vector = Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().getCTRLVectorForStation(station);
            if (station.isMixingStation() || color == 1)
            {
                vector[1] = 1;
            }
            else
            {
                vector[0] = 1;
            }
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForStation(station, vector);
        }
        public override void stopFilling(int color)
        {
            double[] vector = Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().getCTRLVectorForStation(station);
            if (station.isMixingStation() || color == 1)
            {
                vector[1] = 0;
            }
            else
            {
                vector[0] = 0;
            }
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForStation(station, vector);
        }
        public override void moveVArm(int position, double speed)
        {
            double[] vector = Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().getCTRLVectorForStation(station);
            vector[0] = position;
            if (station.isMixingStation())
            {
                Datastructure.Model.Stations.MixingStation mix = (Datastructure.Model.Stations.MixingStation)station;
                if (mix.theAltitude >= mix.theArmpositions[position - 1])
                {
                    vector[4] = -speed;
                }
                else
                {
                    vector[4] = speed;
                }
            }
            else
            {
                if (station.isFillingStation())
                {
                    /** Model.ColorStation col = (Model.ColorStation)station;
                     if (col.theAltitude >= col.theArmpositions[position-1])
                     {
                         vector[3] = -speed;
                     }
                     else
                     {
                         vector[3] = speed;
                     }*/
                }
                else
                {
                    Datastructure.Model.Stations.StorageStation two = (Datastructure.Model.Stations.StorageStation)station;
                    if (two.theAltitude >= two.theVArmPositions[position - 1])
                    {
                        vector[3] = -speed;
                    }
                    else
                    {
                        vector[3] = speed;
                    }
                }
            }
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForStation(station, vector);
        }
        public override void moveHArm(int position, double speed)
        {
            double[] vector = Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().getCTRLVectorForStation(station);
            vector[1] = position;
            Datastructure.Model.Stations.StorageStation two = (Datastructure.Model.Stations.StorageStation)station;
            if (two.theTraverse >= two.theHArmPositions[position - 1])
            {
                vector[4] = -speed;
            }
            else
            {
                vector[4] = speed;
            }
            Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForStation(station, vector);
        }
        public override void stop()
        {
            if (station.isChargingStation())
            {
                Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForStation(station, new double[] { 0 });
            }
            else if (station.isFillingStation())
            {
                Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForStation(station, new double[] { 0/*,0,0,0*/ });
            }
            else if (station.isMixingStation())
            {
                Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForStation(station, new double[] { 0, 0, 0, 0, 0 });
            }
            else if (station.isStorageStation())
            {
                Gateway.ConnectionModule.ConnectionCTRLModule.getInstance().setCTRLForStation(station, new double[] { 0, 0, 0, 0, 0 });
            }
        }

        public RemoteModule() { }
        public RemoteModule(Datastructure.Model.Stations.AbstractStation station)
        {
            theStation = station;
        }
    }
}
