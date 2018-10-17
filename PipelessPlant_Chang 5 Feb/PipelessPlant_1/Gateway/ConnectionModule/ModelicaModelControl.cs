using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace MULTIFORM_PCS.Gateway.ConnectionModule
{
    public class ModelicaModelControl
    {
        private TcpClient client;
        private Stream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private String[] digitalInputsAsString;
        private static double[] digitalInputs;
        private static double[] digitalOutputs;
        private static bool vectorLock;

        public delegate void Data_RecievedEventHandler(object sender, EventArgs e);
        public event Data_RecievedEventHandler Data_Recieved;

        public ModelicaModelControl(TcpClient client)
        {
            this.client = client;
            stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            GUI.PCSMainWindow.getInstance().postStatusMessage("Socket connection with DYMOLA simulation established");
        }

        public void control()
        {
            Gateway.CTRLModule.getInstance().SimulationRunning = true;

            GUI.PCSMainWindow.getInstance().postStatusMessage("Datastructure connected with running DYMOLA simulation!");

            Datastructure.Model.Plant plant = Gateway.ObserverModule.getInstance().getCurrentPlant();
            digitalInputs = new double[plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 3 + plant.getBattStatCount() + plant.AllVessels.Count * 22 + 1];
            digitalOutputs = new double[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 2 + plant.getBattStatCount()];
            for (int i = 0; i < digitalInputs.Length; i++)
            {
                digitalInputs[i] = 0;
            }
            for (int i = 0; i < digitalOutputs.Length; i++)
            {
                digitalOutputs[i] = 0;
            }
            /** CODING OF THE OUTPUT 
             *  1 == true
             *  0 == false
             *  
             *  The first 4 signals are boolean signals which tell the robot if it should walk or rotate.
             *  The second 4 signals refer to the robot speed and robot rotationspeed of the two robots.
             */
            try
            {
                while (true)
                {
                    digitalInputsAsString = reader.ReadLine().Split(',');
                    for (int i = 0; i < digitalInputsAsString.Length; i++)
                    {
                        digitalInputs[i] = double.Parse(digitalInputsAsString[i], System.Globalization.CultureInfo.InvariantCulture);
                        //Console.WriteLine("Msg rcvd : " + digitalInputs[i]);
                    }
                    for (int i = 0; i < plant.AllAGVs.Count; i++)
                    {
                        plant.AllAGVs[i].theCurPosition = new MULTIFORM_PCS.Datastructure.Model.General.Position(digitalInputs[i * 5], digitalInputs[i * 5 + 1]);
                        plant.AllAGVs[i].theRotation = digitalInputs[i * 5 + 2];
                        plant.AllAGVs[i].theVessel = plant.getContainer(Convert.ToInt32(digitalInputs[i * 5 + 3]));
                        plant.AllAGVs[i].theBatteryLoad = digitalInputs[i * 5 + 4];
                    }
                    int two = 0;
                    int mix = 0;
                    int col = 0;
                    int bat = 0;
                    for (int i = 0; i < plant.AllStations.Count; i++)
                    {
                        Datastructure.Model.Stations.AbstractStation stat = plant.AllStations[i];
                        if (stat.isStorageStation())
                        {
                            Datastructure.Model.Stations.StorageStation twoStat = (Datastructure.Model.Stations.StorageStation)Gateway.CTRLModule.getInstance().getStationCTRLAtPos(i).theStation;
                            twoStat.theVessels[0] = plant.getContainer(Convert.ToInt32(digitalInputs[plant.AllAGVs.Count * 5 + two * 9]));
                            twoStat.theVessels[1] = plant.getContainer(Convert.ToInt32(digitalInputs[plant.AllAGVs.Count * 5 + two * 9 + 1]));
                            twoStat.theVessels[2] = plant.getContainer(Convert.ToInt32(digitalInputs[plant.AllAGVs.Count * 5 + two * 9 + 2]));
                            twoStat.theVessels[3] = plant.getContainer(Convert.ToInt32(digitalInputs[plant.AllAGVs.Count * 5 + two * 9 + 3]));
                            twoStat.theVessels[4] = plant.getContainer(Convert.ToInt32(digitalInputs[plant.AllAGVs.Count * 5 + two * 9 + 4]));
                            twoStat.theVessels[5] = plant.getContainer(Convert.ToInt32(digitalInputs[plant.AllAGVs.Count * 5 + two * 9 + 5]));
                            twoStat.theVessels[6] = plant.getContainer(Convert.ToInt32(digitalInputs[plant.AllAGVs.Count * 5 + two * 9 + 6]));
                            twoStat.theAltitude = digitalInputs[plant.AllAGVs.Count * 5 + two * 9 + 7];
                            twoStat.theTraverse = digitalInputs[plant.AllAGVs.Count * 5 + two * 9 + 8];
                            two++;
                        }
                        else if (stat.isMixingStation())
                        {
                            Datastructure.Model.Stations.MixingStation mixStat = (Datastructure.Model.Stations.MixingStation)Gateway.CTRLModule.getInstance().getStationCTRLAtPos(i).theStation;
                            mixStat.theCurrentVessel = plant.getContainer(Convert.ToInt32(digitalInputs[plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + mix * 5]));
                            mixStat.theRotating = (digitalInputs[plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + mix * 5 + 1] > 0.1) ? true : false;
                            mixStat.theSensor = digitalInputs[plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + mix * 5 + 2];
                            mixStat.theAltitude = digitalInputs[plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + mix * 5 + 3];
                            mixStat.theFillRate = digitalInputs[plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + mix * 5 + 4];
                            mix++;
                        }
                        else if (stat.isFillingStation())
                        {
                            Datastructure.Model.Stations.FillingStation colStat = (Datastructure.Model.Stations.FillingStation)Gateway.CTRLModule.getInstance().getStationCTRLAtPos(i).theStation;
                            colStat.theCurContainer = plant.getContainer(Convert.ToInt32(digitalInputs[plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + plant.getMixStatCount() * 5 + col * 3]));
                            colStat.theFillRate_1 = digitalInputs[plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + plant.getMixStatCount() * 5 + col * 3 + 1];
                            colStat.theFillRate_2 = digitalInputs[plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + plant.getMixStatCount() * 5 + col * 3 + 2];
                            col++;
                        }
                        else if (stat.isChargingStation())
                        {
                            Datastructure.Model.Stations.ChargingStation batStat = (Datastructure.Model.Stations.ChargingStation)Gateway.CTRLModule.getInstance().getStationCTRLAtPos(i).theStation;
                            batStat.theLoadRate = digitalInputs[plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 3 + bat];
                            bat++;
                        }
                    }
                    for (int i = 0; i < plant.AllVessels.Count; i++)
                    {
                        plant.AllVessels[i].theCurFillHard = digitalInputs[plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 3 + plant.getBattStatCount() + i * 22];
                        plant.AllVessels[i].theCurFillWater = digitalInputs[plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 3 + plant.getBattStatCount() + i * 22 + 1];
                        int tmp = 2;
                        for (int j = 0; j < 5; j++)
                        {
                            for (int k = 0; k < plant.ChoosenColorIDs.Length; k++)
                            {
                                plant.AllVessels[i].theLayers[j].theIngredients[k].theCurVolume = digitalInputs[plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 3 + plant.getBattStatCount() + i * 22 + tmp];
                                tmp++;
                            }
                        }
                    }
                    plant.theCurSimTime = digitalInputs[digitalInputs.Length - 1];
                    Gateway.ObserverModule.getInstance().modelChanged();

                    Data_Recieved(this, new EventArgs());

                    if (!vectorLock)
                    {
                        //Console.WriteLine("vector length :" + digitalOutputs.Length);
                        String message = "";
                        for (int i = 0; i < digitalOutputs.Length; i++)
                        {
                            //runden auf drei stellen nach dem komma!
                            digitalOutputs[i] = Convert.ToInt32(digitalOutputs[i] * 1000.0d) / 1000.0d;

                            if (i == 0)
                            {
                                message += digitalOutputs[i].ToString().Replace(',', '.');
                            }
                            else
                            {
                                message += "," + digitalOutputs[i].ToString().Replace(',', '.');
                            }
                        }
                        message = message.ToLower();
                        Console.WriteLine(message);
                        writer.Write(message + "\n");
                        writer.Flush();
                        //Console.WriteLine("Message send");
                        #region containerLockReleaseSwitch;
                        for (int i = 0; i < plant.AllStations.Count; i++)
                        {
                            int pos = Gateway.ObserverModule.getInstance().getCurrentPlant().getPosOfStation(plant.AllStations[i].theId);
                            if (plant.AllStations[i].isStorageStation())
                            {
                                if (digitalOutputs[plant.AllAGVs.Count * 2 + pos * 5 + 2] != 0)
                                {
                                    digitalOutputs[plant.AllAGVs.Count * 2 + pos * 5 + 2] = 0;
                                }
                            }
                            else if (plant.AllStations[i].isMixingStation())
                            {
                                if (digitalOutputs[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + pos * 5 + 3] != 0)
                                {
                                    digitalOutputs[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + pos * 5 + 3] = 0;
                                }
                            }
                            else if (plant.AllStations[i].isFillingStation())
                            {
                                /**if (digitalOutputs[arena.AllAGVs.Count * 2 + arena.getTwoArmStatCount() * 5 + arena.getMixStatCount() * 5 + pos * 4 + 2] != 0)
                                {
                                    digitalOutputs[arena.AllAGVs.Count * 2 + arena.getTwoArmStatCount() * 5 + arena.getMixStatCount() * 5 + pos * 4 + 2] = 0;
                                }*/
                            }
                        }
                        #endregion;


                    }
                }
            }
            catch (Exception e)
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("Socket connection terminated by unknown exception!");
                Console.WriteLine(e.Message);
                ConnectionCTRLModule.getInstance().killConnection();
                return;
            }
        }

        public double getCurSimTime()
        {
            return digitalInputs[digitalInputs.Length - 1];
        }
        public void setCTRLVector(double[] vector)
        {
            digitalOutputs = vector;
        }
        public double[] getCTRLVector()
        {
            return digitalOutputs;
        }
        public void changeVectorLock()
        {
            if (vectorLock)
            {
                vectorLock = false;
            }
            else
            {
                vectorLock = true;
            }
        }
    }
}
