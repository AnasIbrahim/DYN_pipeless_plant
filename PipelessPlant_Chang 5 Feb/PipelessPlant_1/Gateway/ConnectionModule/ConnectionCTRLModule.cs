using System;
using System.Collections.Generic;
using System.Text;
//using System.Runtime.InteropServices;
using NDde;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace MULTIFORM_PCS.Gateway.ConnectionModule
{
    public class ConnectionCTRLModule
    {
        private static double[] controlSignals;

        public delegate void Data_RecievedEventHandler(object sender, EventArgs e);
        public event Data_RecievedEventHandler SimulationDataUpdate_Recieved;

        #region singletonPattern;
        private static ConnectionCTRLModule conModule;
        public static ConnectionCTRLModule getInstance()
        {
            if (conModule == null)
            {
                conModule = new ConnectionCTRLModule();
            }
            return conModule;
        }
        private ConnectionCTRLModule()
        {
            dymolaClient = new NDde.Client.DdeClient("dymola", "dymola");
            simulationSocketStarted = false;

            Datastructure.Model.Plant plant = Gateway.ObserverModule.getInstance().getCurrentPlant();

            controlSignals = new double[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 2 + plant.getBattStatCount()];
        }
        #endregion;

        #region dymolaModelicaConnection;
        private NDde.Client.DdeClient dymolaClient;
        private NDde.Client.DdeClient dymosimClient;
        private bool fileOpen = false;

        public void loadModelicaFileToDymola(String modelicaFile)
        {           
            dymolaClient.TryConnect();
            dymolaClient.TryExecute("open "+ modelicaFile, 5000);
            if (dymolaClient.IsConnected)
            {
                dymolaClient.Disconnect();
                fileOpen = true;
            }
        }

        public void compileInDymola()
        {
            if (fileOpen)
            {
                dymolaClient.TryConnect();
                dymolaClient.TryExecute("compile(\""+Gateway.ObserverModule.getInstance().getCurrentPlant().theName+".Arena\");", 5000);
                dymolaClient.Disconnect();
            }
        }

        public void translateInDymola()
        {
            if (fileOpen)
            {
                dymolaClient.TryConnect();
                dymolaClient.TryExecute("translateModel(\"" + Gateway.ObserverModule.getInstance().getCurrentPlant().theName + ".Arena\");", 5000);
                dymolaClient.Disconnect();
            }
        }

        public void simulateInDymola()
        {
            if (fileOpen)
            {
                dymolaClient.TryConnect();
                dymolaClient.TryExecute("simulateModel(\"" + Gateway.ObserverModule.getInstance().getCurrentPlant().theName + ".Arena\", stopTime = 99999, method=\"dassl\", resultFile=\"Arena\");", 5000);
                dymolaClient.Disconnect();
            }
        }
        public void stopDymolaSimulation()
        {
            if (fileOpen)
            {
                dymolaClient.TryConnect();
                dymolaClient.TryExecute("stop", 5000);
                dymolaClient.Disconnect();
            }
        }

        public void runModel()
        {
            dymosimClient = new NDde.Client.DdeClient("dymosim", "xxx");
            dymosimClient.TryConnect();
            //dymosimClient.Poke("realtime_", "1", 5000);
            dymosimClient.TryExecute("run", 5000);
            //dymosimClient.Poke("PipelessPlant_v2.currentPlant.x", "1", 5000);
        }

        public void stopModel()
        {
            dymosimClient.TryExecute("stop", 5000);
            dymosimClient.Disconnect();
        }
        #endregion;

        #region connectionToSimulationiRobotAndPLC;
        private TcpListener simulationListener;
        private Thread simulationControlThread;
        private bool accept = true;
        private ModelicaModelControl modelicaSimulationControl;
        public bool simulationSocketStarted;

        public void startSocketConnection()
        {
            Thread controlThreadMain;
            controlThreadMain = new Thread(new ThreadStart(startControlThread));
            controlThreadMain.Start();
            simulationSocketStarted = true;
        }
        private void startControlThread()
        {
            if (simulationListener != null)
            {
                simulationListener.Stop();
            }
            accept = true;
            int port = 12345;
            simulationListener = new TcpListener(System.Net.IPAddress.Parse("127.0.0.1") ,port);
            simulationListener.Start();
            TcpClient client;
            simulationControlThread = null;

            while (accept)
            {
                try
                {
                    GUI.PCSMainWindow.getInstance().postStatusMessage("Waiting for socket connection of DYMOLA simulation");
                    client = simulationListener.AcceptTcpClient();
                    if (simulationControlThread != null)
                        if (simulationControlThread.IsAlive)
                            simulationControlThread.Abort(); //<-- funktioniert dies dafür???
                    modelicaSimulationControl = new ModelicaModelControl(client);
                    modelicaSimulationControl.Data_Recieved += new ModelicaModelControl.Data_RecievedEventHandler(simulationDataRecieved);
                    simulationControlThread = new Thread(new ThreadStart(modelicaSimulationControl.control));
                    simulationControlThread.Start();
                }
                catch (SocketException soc)
                {
                    GUI.PCSMainWindow.getInstance().postStatusMessage("Socket connection terminated! " + soc.Message);
                    simulationSocketStarted = false;
                }
            }
        }
        public void pauseSim()
        {
            simulationControlThread.Suspend();
            Gateway.CTRLModule.getInstance().SimulationPaused = true;
        }
        public void resumeSim()
        {
            simulationControlThread.Resume();
            Gateway.CTRLModule.getInstance().SimulationPaused = false;
        }

        public void simulationDataRecieved(object sender, EventArgs e)
        {
            SimulationDataUpdate_Recieved(sender, e);
        }

        public void killConnection()
        {
            accept = false;
            simulationListener.Stop();
            simulationControlThread.Abort();
            simulationSocketStarted = false;
            Gateway.CTRLModule.getInstance().SimulationRunning = false;
        }

        public void setCTRLVector(double[] vector)
        {
            controlSignals = vector;

            //MODELICA SIMULATION
            if (modelicaSimulationControl != null)
            {
                modelicaSimulationControl.changeVectorLock();
                modelicaSimulationControl.setCTRLVector(controlSignals);
                modelicaSimulationControl.changeVectorLock();
            }

            //IROBOT SERVER //CHANGE HERE TODO
            if (iRobot.iRobotServer.getInstance().ServerRunning)
            {
                //if (!Gateway.CTRLModule.getInstance().AutomaticCTRLRunning)
                {
                    for (int i = 0; i < iRobot.iRobotServer.getInstance().router.robots.Count; i++)
                    {
                        double[] agvCTRLSignals = getCTRLVectorForRobot(iRobot.iRobotServer.getInstance().router.robots[i].RobotID);
                        if (agvCTRLSignals[0] != 0 && agvCTRLSignals[1] != 0) //move and turn
                        {
                            if (agvCTRLSignals[1] > 0)
                            {
                                iRobot.iRobotServer.getInstance().router.robots[i].DriveDirect((short)(-agvCTRLSignals[1] + agvCTRLSignals[0]), (short)(agvCTRLSignals[1] + agvCTRLSignals[0]));
                            }
                            else
                            {
                                iRobot.iRobotServer.getInstance().router.robots[i].DriveDirect((short)(-agvCTRLSignals[1] + agvCTRLSignals[0]), (short)(agvCTRLSignals[1] + agvCTRLSignals[0]));
                            }
                        }
                        else if (agvCTRLSignals[0] != 0) // move
                        {
                            iRobot.iRobotServer.getInstance().router.robots[i].DriveDirect((short)agvCTRLSignals[0], (short)agvCTRLSignals[0]);
                        }
                        else if (agvCTRLSignals[1] != 0) // turn
                        {
                            if (agvCTRLSignals[1] > 0)
                            {
                                iRobot.iRobotServer.getInstance().router.robots[i].DriveDirect((short)-agvCTRLSignals[1], (short)agvCTRLSignals[1]);
                            }
                            else
                            {
                                iRobot.iRobotServer.getInstance().router.robots[i].DriveDirect((short)-agvCTRLSignals[1], (short)agvCTRLSignals[1]);
                            }
                        }
                        else
                        {
                            iRobot.iRobotServer.getInstance().router.robots[i].connection.send(iRobot.iRobotServer.getInstance().router.robots[i].protocol.writeDirectDrive(0, 0));
                        }
                    }
                }
                //else
                {
                    //GUI.PCSMainWindow.getInstance().postStatusMessage("MANUAL CMD BLOCKED, AUTOMATIC RUNNING...");
                }
            }

            //PLC
            bool blockPLCSignals = false;
            if (Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().Connected)
            {
                if (!Gateway.CTRLModule.getInstance().AutomaticCTRLRunning)
                {

                  if (!blockPLCSignals)
                  {
                    blockPLCSignals = true;
                    Datastructure.Model.Plant p = Gateway.ObserverModule.getInstance().getCurrentPlant();

                    byte[] signals = new byte[64];//0-255

                    double[] col1Signals = getCTRLVectorForStation(p.getColorStations()[0]);
                    if (col1Signals[0] > 0)
                    {
                      signals[1] = 1;
                      signals[2] = 18;
                    }
                    else
                    {
                      signals[1] = 0;
                      signals[2] = 0;
                    }
                    if (col1Signals[1] > 0)
                    {
                      signals[3] = 1;
                      signals[4] = 18;
                    }
                    else
                    {
                      signals[3] = 0;
                      signals[4] = 0;
                    }

                    double[] col2Signals = getCTRLVectorForStation(p.getColorStations()[1]);
                    if (col2Signals[0] > 0)
                    {
                      signals[5] = 1;
                      signals[6] = 18;
                    }
                    else
                    {
                      signals[5] = 0;
                      signals[6] = 0;
                    }
                    if (col2Signals[1] > 0)
                    {
                      signals[7] = 1;
                      signals[8] = 18;
                    }
                    else
                    {
                      signals[7] = 0;
                      signals[8] = 0;
                    }

                    double[] mixSignals = getCTRLVectorForStation(p.getMixingStations()[0]);
                    if (mixSignals[3] > 0)
                    {
                      signals[18] = 1;
                      signals[19] = 1;
                    }
                    else
                    {
                      signals[18] = 0;
                      signals[19] = 0;
                    }
                    if (mixSignals[2] > 0)
                    {
                      signals[22] = 1;
                      signals[23] = 93;
                    }
                    else
                    {
                      signals[22] = 0;
                      signals[23] = 0;
                    }
                    if (mixSignals[1] > 0)
                    {
                      signals[24] = 0xFF;
                      signals[25] = 0x14;
                      signals[26] = 93;
                      signals[27] = 1;
                    }
                    else
                    {
                      signals[24] = 0;
                      signals[25] = 0;
                      signals[26] = 0;
                      signals[27] = 0;
                    }
                    if (mixSignals[0] == 1)
                    {
                      signals[47] = 1;
                      signals[48] = 0;
                      signals[49] = 0;
                      signals[50] = 0;
                      signals[55] = 1;
                      signals[56] = 0;
                      signals[57] = 0;
                      signals[58] = 0;
                    }
                    else if (mixSignals[0] == 2)
                    {
                      signals[47] = 0;
                      signals[48] = 0;
                      signals[49] = 0;
                      signals[50] = 0;
                      signals[55] = 1;
                      signals[56] = 0;
                      signals[57] = 0;
                      signals[58] = 1;
                    }
                    else if (mixSignals[0] == 3)
                    {
                      signals[47] = 0;
                      signals[48] = 1;
                      signals[49] = 0;
                      signals[50] = 0;
                      signals[55] = 1;
                      signals[56] = 0;
                      signals[57] = 0;
                      signals[58] = 0;
                    }
                    else if (mixSignals[0] == 4)
                    {
                      signals[47] = 0;
                      signals[48] = 0;
                      signals[49] = 1;
                      signals[50] = 0;
                      signals[55] = 1;
                      signals[56] = 0;
                      signals[57] = 0;
                      signals[58] = 0;
                    }
                    else if (mixSignals[0] == 5)
                    {
                      signals[47] = 0;
                      signals[48] = 0;
                      signals[49] = 0;
                      signals[50] = 1;
                      signals[55] = 1;
                      signals[56] = 0;
                      signals[57] = 0;
                      signals[58] = 0;
                    }

                    double[] stoSignals = getCTRLVectorForStation(p.getStorageStations()[0]);
                    if (stoSignals[2] > 0)
                    {
                      signals[20] = 1;
                      signals[21] = 1;
                    }
                    else
                    {
                      signals[20] = 0;
                      signals[21] = 0;
                    }
                    int indexOfStorageStationInCTRLVector = Gateway.ObserverModule.getInstance().getCurrentPlant().getPosOfStation(p.getStorageStations()[0].theId);
                    if (stoSignals[0] != p.getStorageStations()[0].theAltitude && stoSignals[1] != p.getStorageStations()[0].theTraverse)
                    {
                      signals[28] = 0;
                      signals[29] = 0;
                      signals[30] = 0;
                      signals[31] = 0;
                      signals[32] = 1;
                      signals[33] = 0;
                      signals[44] = 0;
                      signals[45] = 0;

                      signals[34] = 0;
                      signals[35] = 0;
                      signals[36] = 0;
                      signals[37] = 0;
                      signals[38] = 0;
                      signals[39] = 0;
                      signals[40] = 0;
                      signals[41] = 0;
                      signals[42] = 1;
                      signals[43] = 0;

                      vector[p.AllAGVs.Count * 2 + indexOfStorageStationInCTRLVector * 5] = p.getStorageStations()[0].theAltitude;
                      vector[p.AllAGVs.Count * 2 + indexOfStorageStationInCTRLVector * 5 + 1] = p.getStorageStations()[0].theTraverse;

                      GUI.PCSMainWindow.getInstance().postStatusMessage("CMD for StorageStation blocked!");
                    }
                    else
                    {
                      if (stoSignals[0] == 1 && p.getStorageStations()[0].theTraverse == 4)
                      {
                        signals[28] = 1;
                        signals[29] = 0;
                        signals[30] = 0;
                        signals[31] = 1;
                        signals[32] = 0;
                        signals[33] = 0;

                        signals[44] = 0;
                        signals[45] = 0;
                        p.getStorageStations()[0].theAltitude = stoSignals[0];
                      }
                      else if (stoSignals[0] == 2 && p.getStorageStations()[0].theTraverse == 4)
                      {
                        signals[28] = 0;
                        signals[29] = 0;
                        signals[30] = 0;
                        signals[31] = 1;
                        signals[32] = 0;
                        signals[33] = 0;

                        signals[44] = 0;
                        signals[45] = 1;
                        p.getStorageStations()[0].theAltitude = stoSignals[0];
                      }
                      else if (stoSignals[0] == 3)
                      {
                        signals[28] = 0;
                        signals[29] = 1;
                        signals[30] = 0;
                        signals[31] = 1;
                        signals[32] = 0;
                        signals[33] = 0;

                        signals[44] = 0;
                        signals[45] = 0;
                        p.getStorageStations()[0].theAltitude = stoSignals[0];
                      }
                      else if (stoSignals[0] == 4)
                      {
                        signals[28] = 0;
                        signals[29] = 0;
                        signals[30] = 0;
                        signals[31] = 1;
                        signals[32] = 0;
                        signals[33] = 0;

                        signals[44] = 1;
                        signals[45] = 0;
                        p.getStorageStations()[0].theAltitude = stoSignals[0];
                      }
                      else if (stoSignals[0] == 5)
                      {
                        signals[28] = 0;
                        signals[29] = 0;
                        signals[30] = 1;
                        signals[31] = 1;
                        signals[32] = 0;
                        signals[33] = 0;

                        signals[44] = 0;
                        signals[45] = 0;
                        p.getStorageStations()[0].theAltitude = stoSignals[0];
                      }
                      if (stoSignals[1] == 1 && p.getStorageStations()[0].theAltitude == 5)
                      {
                        signals[34] = 1;
                        signals[35] = 0;
                        signals[36] = 0;
                        signals[37] = 0;
                        signals[38] = 0;
                        signals[39] = 0;
                        signals[40] = 0;
                        signals[41] = 1;
                        signals[42] = 0;
                        signals[43] = 0;
                        p.getStorageStations()[0].theTraverse = stoSignals[1];
                      }
                      else if (stoSignals[1] == 2 && p.getStorageStations()[0].theAltitude == 5)
                      {
                        signals[34] = 0;
                        signals[35] = 1;
                        signals[36] = 0;
                        signals[37] = 0;
                        signals[38] = 0;
                        signals[39] = 0;
                        signals[40] = 0;
                        signals[41] = 1;
                        signals[42] = 0;
                        signals[43] = 0;
                        p.getStorageStations()[0].theTraverse = stoSignals[1];
                      }
                      else if (stoSignals[1] == 3 && p.getStorageStations()[0].theAltitude == 5)
                      {
                        signals[34] = 0;
                        signals[35] = 0;
                        signals[36] = 1;
                        signals[37] = 0;
                        signals[38] = 0;
                        signals[39] = 0;
                        signals[40] = 0;
                        signals[41] = 1;
                        signals[42] = 0;
                        signals[43] = 0;
                        p.getStorageStations()[0].theTraverse = stoSignals[1];
                      }
                      else if (stoSignals[1] == 4 && p.getStorageStations()[0].theAltitude == 5)
                      {
                        signals[34] = 0;
                        signals[35] = 0;
                        signals[36] = 0;
                        signals[37] = 1;
                        signals[38] = 0;
                        signals[39] = 0;
                        signals[40] = 0;
                        signals[41] = 1;
                        signals[42] = 0;
                        signals[43] = 0;
                        p.getStorageStations()[0].theTraverse = stoSignals[1];
                      }
                      else if (stoSignals[1] == 5 && p.getStorageStations()[0].theAltitude == 5)
                      {
                        signals[34] = 0;
                        signals[35] = 0;
                        signals[36] = 0;
                        signals[37] = 0;
                        signals[38] = 1;
                        signals[39] = 0;
                        signals[40] = 0;
                        signals[41] = 1;
                        signals[42] = 0;
                        signals[43] = 0;
                        p.getStorageStations()[0].theTraverse = stoSignals[1];
                      }
                      else if (stoSignals[1] == 6 && p.getStorageStations()[0].theAltitude == 5)
                      {
                        signals[34] = 0;
                        signals[35] = 0;
                        signals[36] = 0;
                        signals[37] = 0;
                        signals[38] = 0;
                        signals[39] = 1;
                        signals[40] = 0;
                        signals[41] = 1;
                        signals[42] = 0;
                        signals[43] = 0;
                        p.getStorageStations()[0].theTraverse = stoSignals[1];
                      }
                      else if (stoSignals[1] == 7 && p.getStorageStations()[0].theAltitude == 5)
                      {
                        signals[34] = 0;
                        signals[35] = 0;
                        signals[36] = 0;
                        signals[37] = 0;
                        signals[38] = 0;
                        signals[39] = 0;
                        signals[40] = 1;
                        signals[41] = 1;
                        signals[42] = 0;
                        signals[43] = 0;
                        p.getStorageStations()[0].theTraverse = stoSignals[1];
                      }
                    }

                    Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);

                    Thread.Sleep(125);

                    signals[41] = 0;
                    signals[31] = 0;
                    signals[55] = 0;

                    Thread.Sleep(125);

                    Gateway.ConnectionModule.PLCConnection.TCPPLCConnection.getInstance().sendControlSignalsToPLC(signals);

                    vector[p.AllAGVs.Count * 2 + indexOfStorageStationInCTRLVector * 5] = p.getStorageStations()[0].theAltitude;
                    vector[p.AllAGVs.Count * 2 + indexOfStorageStationInCTRLVector * 5 + 1] = p.getStorageStations()[0].theTraverse;

                    Gateway.ObserverModule.getInstance().modelChanged();
                    blockPLCSignals = false;
                  }
                }
                else
                {
                    GUI.PCSMainWindow.getInstance().postStatusMessage("MANUAL CMD BLOCKED, AUTOMATIC RUNNING...");
                }
            }
        }
        public void setCTRLForRobot(int robotID, double walkSpeed, double rotSpeed, double walkEndTime, double rotEndTime, double goalx, double goaly, double goalrot)
        {
            double[] vector = getCTRLVector();
            while (vector == null)
            {
                vector = getCTRLVector();
            }
            int pos = Gateway.ObserverModule.getInstance().getCurrentPlant().getPosOfRobot(robotID);
            vector[pos * 2] = walkSpeed;
            vector[pos * 2 + 1] = rotSpeed;

            setCTRLVector(vector);
        }
    
        public void setCTRLForRobot(int robotID, double walkSpeed, double rotSpeed)
        {
            double[] vector = getCTRLVector();
            while (vector == null)
            {
                vector = getCTRLVector();
            }
            int pos = Gateway.ObserverModule.getInstance().getCurrentPlant().getPosOfRobot(robotID);
            vector[pos * 2] = walkSpeed;
            vector[pos * 2 + 1] = rotSpeed;

            setCTRLVector(vector);
        }
        public void setCTRLForStation(Datastructure.Model.Stations.AbstractStation stat, double[] instructions)
        {
            Datastructure.Model.Plant plant = ObserverModule.getInstance().getCurrentPlant();
            double[] vector = getCTRLVector();
            while (vector == null)
            {
                vector = getCTRLVector();
            }
            int pos = Gateway.ObserverModule.getInstance().getCurrentPlant().getPosOfStation(stat.theId);
            if (stat.isStorageStation())
            {
                vector[plant.AllAGVs.Count * 2 + pos * 5] = instructions[0];
                vector[plant.AllAGVs.Count * 2 + pos * 5 + 1] = instructions[1];
                vector[plant.AllAGVs.Count * 2 + pos * 5 + 2] = instructions[2];
                vector[plant.AllAGVs.Count * 2 + pos * 5 + 3] = instructions[3];
                vector[plant.AllAGVs.Count * 2 + pos * 5 + 4] = instructions[4];
            }
            else if (stat.isMixingStation())
            {
                vector[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + pos * 5] = instructions[0];
                vector[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + pos * 5 + 1] = instructions[1];
                vector[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + pos * 5 + 2] = instructions[2];
                vector[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + pos * 5 + 3] = instructions[3];
                vector[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + pos * 5 + 4] = instructions[4];
            }
            else if (stat.isFillingStation())
            {
                vector[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + plant.getMixStatCount() * 5 + pos * 2] = instructions[0];
                vector[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + plant.getMixStatCount() * 5 + pos * 2 + 1] = instructions[1];
            }
            else if (stat.isChargingStation())
            {
                vector[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 2 + pos] = instructions[0];
            }

            setCTRLVector(vector);
        }
        public double[] getCTRLVectorForStation(Datastructure.Model.Stations.AbstractStation stat)
        {
            Datastructure.Model.Plant plant = ObserverModule.getInstance().getCurrentPlant();
            double[] vector = getCTRLVector();
            while (vector == null)
            {
                vector = getCTRLVector();
            }
            int pos = Gateway.ObserverModule.getInstance().getCurrentPlant().getPosOfStation(stat.theId);
            if (stat.isStorageStation())
            {
                return new double[] { vector[plant.AllAGVs.Count * 2 + pos * 5], vector[plant.AllAGVs.Count * 2 + pos * 5 + 1], vector[plant.AllAGVs.Count * 2 + pos * 5 + 2], vector[plant.AllAGVs.Count * 2 + pos * 5 + 3], vector[plant.AllAGVs.Count * 2 + pos * 5 + 4] };
            }
            else if (stat.isMixingStation())
            {
                return new double[] { vector[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + pos * 5], vector[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + pos * 5 + 1], vector[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + pos * 5 + 2], vector[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + pos * 5 + 3], vector[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + pos * 5 + 4] };
            }
            else if (stat.isFillingStation())
            {
                return new double[] { vector[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + plant.getMixStatCount() * 5 + pos * 2], vector[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + plant.getMixStatCount() * 5 + pos * 2+ 1] };
            }
            else if (stat.isChargingStation())
            {
                return new double[] { vector[plant.AllAGVs.Count * 2 + plant.getStorageStationCount() * 5 + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 2 + pos] };
            }
            return null;
        }
        public double[] getCTRLVectorForRobot(int robotID)
        {
            Datastructure.Model.Plant arena = ObserverModule.getInstance().getCurrentPlant();
            double[] vector = getCTRLVector();
            while (vector == null)
            {
                vector = getCTRLVector();
            }
            int pos = arena.getPosOfRobot(robotID);
            return new double[] { vector[pos * 2], vector[pos * 2 + 1]};
        }
        public double[] getCTRLVector()
        {
            return controlSignals;

            /**if (modelicaSimulationControl != null)
            {
                return modelicaSimulationControl.getCTRLVector();
            }
            return null;*/
        }
        #endregion;

        public void killiRobotSocketConnections()
        {
            iRobot.iRobotServer.getInstance().stopServer();
        }
    }
}
