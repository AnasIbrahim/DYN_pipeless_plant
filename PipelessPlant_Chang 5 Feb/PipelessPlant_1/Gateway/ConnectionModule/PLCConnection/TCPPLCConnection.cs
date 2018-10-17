using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using System.Windows.Media;

namespace MULTIFORM_PCS.Gateway.ConnectionModule.PLCConnection
{
    class TCPPLCConnection
    {
        private byte[] plcFeedback;
        public byte[] PlcFeedback
        {
            get { return plcFeedback; }
        }

        private bool connected;
        public bool Connected
        {
            get { return connected; }
            set { connected = value; }
        }

        private Thread connectionThread;

        /// Deafault Port Number
        private int portNum = 8000;

        /// Default remote server ip address
        private string hostName = "192.168.0.130";

        /// TCP-Connection
        private TcpClient client;

        private DispatcherTimer TCPtimer;

        #region singletonPattern;
        private static TCPPLCConnection plcConnection;
        public static TCPPLCConnection getInstance()
        {
            if (plcConnection == null)
            {
                plcConnection = new TCPPLCConnection();
            }
            return plcConnection;
        }
        private TCPPLCConnection()
        {
            TCPtimer = new DispatcherTimer();
            TCPtimer.Tick += new EventHandler(timerTCP_Tick);
            TCPtimer.Interval = TimeSpan.FromMilliseconds(100);
        }
        #endregion;


        public void connectToPLC()
        {
            if (!Connected)
            {
                connectionThread = new Thread(new ThreadStart(startTCPConnectionToPLC));
                GUI.PCSMainWindow.getInstance().postStatusMessage("Starting TCP connection to PLC on " + hostName + ":" + portNum + "...");
                connectionThread.Start();
            }
        }
        private void startTCPConnectionToPLC()
        {
            try
            {
                // create new tcp client object
                client = new TcpClient(hostName, portNum);

                connected = true;

                GUI.PCSMainWindow.getInstance().postStatusMessage("...connection to PLC established!");
                GUI.PCSMainWindow.getInstance().setPLCLights(1);

                // enable the timer for reading tcp data
                TCPtimer.Start();

                plcFeedback = new byte[30];
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message.ToString());
                GUI.PCSMainWindow.getInstance().postStatusMessage("Connection attempt to PLC failed. Please check the network configuration.");
                GUI.PCSMainWindow.getInstance().setPLCLights(0);
            }
        }
        public void disconnectFromPLC()
        {
            if (connected)
            {
                client.Close();
                connectionThread.Abort();
                connected = false;

                TCPtimer.Stop();

                GUI.PCSMainWindow.getInstance().borderPLCTest.Background = Brushes.LightSteelBlue;
                GUI.PCSMainWindow.getInstance().borderPLCTest.BorderBrush = Brushes.LightSteelBlue;

                GUI.PCSMainWindow.getInstance().postStatusMessage("Disconnected from PLC.");
                GUI.PCSMainWindow.getInstance().setPLCLights(0);
            }
        }
        private void timerTCP_Tick(object sender, EventArgs e)
        {
            if (connected)
            {
                try
                {
                    string helpString;

                    NetworkStream ns = client.GetStream();

                    //byte[] bytes = new byte[30];

                    if (ns.DataAvailable)
                    {
                        int readByteCounter = ns.Read(plcFeedback, 0, plcFeedback.Length);

                        helpString = "";
                        for (int i = 0; i < readByteCounter; i++)
                        {
                            helpString = helpString + " " + plcFeedback[i].ToString("X2");
                        }

                        Console.WriteLine(helpString);
                        //GUI.PCSMainWindow.getInstance().postStatusMessage("PLC feedback recieved!");
                        GUI.PCSMainWindow.getInstance().setPLCLights(2);

                        GUI.PCSMainWindow.getInstance().textBlockPLCTime.Text = "PLC time: " + plcFeedback[9] + ":" + plcFeedback[8] + ":" + plcFeedback[7];
                    }
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message.ToString());
                    GUI.PCSMainWindow.getInstance().postStatusMessage("Failed to recieve PLC feedback! Please check network connection.");
                    GUI.PCSMainWindow.getInstance().setPLCLights(1);
                }
            }
        }

        public void sendControlSignalsToPLC(byte[] signals)
        {
            if (connected)
            {
                try
                {
                    if (!GUI.PCSMainWindow.getInstance().emergencyStop)
                    {
                        //egal ich lass das jetzt so
                    }
                    else
                    {
                        //EMERGENCYSTOP Byte 64 setzt alles stop außer Magnet
                        signals = new byte[64];

                      //Magneten sollen an bleiben
                        signals[17] = 1;
                        signals[18] = 1;
                        signals[19] = 1;
                        signals[20] = 1;
                        signals[21] = 1;


                        signals[63] = 1;
                        GUI.PCSMainWindow.getInstance().postStatusMessage("Execution stopped by Operator!");
                    }
                    NetworkStream ns = client.GetStream();
                    ns.Write(signals, 0, signals.Length);
                    GUI.PCSMainWindow.getInstance().setPLCLights(2);
                }
                catch (Exception exc)
                {
                    GUI.PCSMainWindow.getInstance().postStatusMessage("Signals to PLC Error: " + exc.Message.ToString());
                    GUI.PCSMainWindow.getInstance().setPLCLights(1);
                }
            }
        }
    }
}
