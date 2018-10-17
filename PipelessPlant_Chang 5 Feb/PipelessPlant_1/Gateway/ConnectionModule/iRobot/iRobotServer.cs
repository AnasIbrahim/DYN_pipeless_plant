using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;

using System.IO;

namespace MULTIFORM_PCS.Gateway.ConnectionModule.iRobot
{
  // server listening for clients and incoming udp packets
  public class iRobotServer {

    #region quickHack;

    private bool lockStop;

    // whether the server is running
    private bool serverRunning;
    public bool ServerRunning {
      get { return serverRunning; }
      set { serverRunning = value; }
    }

    // the local ip address used for the tcp listening socket
    private string tcpIP;
    public string TcpIP {
      get { return tcpIP; }
      set { tcpIP = value; }
    }
    // the local ip port used for the tcp listening socket
    private int tcpPort;
    public int TcpPort {
      get { return tcpPort; }
      set { tcpPort = value; }
    }
    // the local ip address used for the udp socket
    private string udpIP;
    public string UdpIP {
      get { return udpIP; }
      set { udpIP = value; }
    }
    // the local ip port used for the udp socket
    private int udpPort;
    public int UpdPort {
      get { return udpPort; }
      set { udpPort = value; }
    }

    // singleton pattern
    #region singletonPattern;
    private static iRobotServer server;
    public static iRobotServer getInstance() {
      if (server == null) {
        server = new iRobotServer();
      }
      return server;
    }
    private iRobotServer() {
      this.tcpIP = "192.168.1.200";
      this.tcpPort = 2001;
      this.udpIP = "192.168.1.200";
      this.udpPort = 2000;

      // add console cmds
      MULTIFORM_PCS.GUI.PCSMainWindow.getInstance().consoleCmd += (string[] splits) => {
        if (!serverRunning)
          return;
        if (splits.Length < 1)
          return;
        if (splits[0] == "goal") {
          if (splits.Length != 4)
            return;
          try {
            if (MULTIFORM_PCS.GUI.PCSMainWindow.getInstance().activeAGVCTRL == null)
              return;
            var robot = robotFromId(MULTIFORM_PCS.GUI.PCSMainWindow.getInstance().activeAGVCTRL.agvID);
            if (robot == null)
              return;
            robot.SetGoal(new iRobotRouter.Robot.PosAngle(new iRobotVector(double.Parse(splits[1]), double.Parse(splits[2])), double.Parse(splits[3]) / 180.0 * Math.PI));
          } catch (Exception) {
          }
        }
      };
    }
    #endregion;

    public iRobotRouter router;

    public void startServer() {
      if (!serverRunning) {
        lockStop = true;
        GUI.PCSMainWindow.getInstance().postStatusMessage("Starting iRobot server...");
        serverRunning = true;
        IPEndPoint localTcpEp = new IPEndPoint(System.Net.IPAddress.Parse(tcpIP), tcpPort);
        IPEndPoint localUdpEp = new IPEndPoint(System.Net.IPAddress.Parse(udpIP), udpPort);
        try {
          listener = new TcpListener(localTcpEp);
          client = new UdpClient(localUdpEp);
          router = new iRobotRouter(this, (a) => { GUI.PCSMainWindow.getInstance().Dispatcher.Invoke(a); });
          router.robotAdded += (r) => {
            r.setup();
            var avg = Gateway.ObserverModule.getInstance().getCurrentPlant().getAGV(r.RobotID);
            r.sensorUpdate += sd =>
            {
              avg.theChargingStatus = sd.chargingStateToString();
              avg.theBatteryLoad = sd.batteryCharge;
                //add distance sensor data here
              Gateway.ObserverModule.getInstance().modelChanged();
            };
            // setup robots initial position
            if (avg.Seen)
            {
              r.init(new iRobotRouter.Robot.PosAngle(new iRobotVector(avg.theCurPosition.X * 10, avg.theCurPosition.Y * 10), avg.theRotation * Math.PI / 180.0), new iRobotRouter.Robot.PosAngle(new iRobotVector(avg.theCurPosition.X * 10, avg.theCurPosition.Y * 10), avg.theRotation * Math.PI / 180.0));
            }
            else
            {
              avg.firstSeen += () =>
              {
                r.init(new iRobotRouter.Robot.PosAngle(new iRobotVector(avg.theCurPosition.X * 10, avg.theCurPosition.Y * 10), avg.theRotation * Math.PI / 180.0), new iRobotRouter.Robot.PosAngle(new iRobotVector(avg.theCurPosition.X * 10, avg.theCurPosition.Y * 10), avg.theRotation * Math.PI / 180.0));
              };
            }

            GUI.PCSMainWindow.getInstance().postStatusMessage("Robot added: " + r.connection.remEp + " ID: " + r.RobotID);
            r.updateCurrent += (x, y, a) => {
              var agv = Gateway.ObserverModule.getInstance().getCurrentPlant().getAGV(r.RobotID);
//              agv.theCurPosition.X = x / 10;
//              agv.theCurPosition.Y = y / 10;
//              agv.theRotation = a * 180.0 / Math.PI;
              agv.ShadowX = x / 10;
              agv.ShadowY = y / 10;
              agv.ShadowRot = a * 180.0 / Math.PI;
              Gateway.ObserverModule.getInstance().modelChanged();
            };
          };
          router.robotRemoved += (r) => {
            GUI.PCSMainWindow.getInstance().postStatusMessage("Robot removed: " + r.connection.remEp);
          };
          router.log += (String msg) => { GUI.PCSMainWindow.getInstance().postStatusMessage(msg); };
          start();
          Gateway.ObserverModule.getInstance().theRouter = router;
          GUI.PCSMainWindow.getInstance().postStatusMessage("iRobot server opened!");
        } catch (Exception) {
          GUI.PCSMainWindow.getInstance().postStatusMessage("Start of iRobot server failed. Wrong ip address. Please change the configuration.");
          Gateway.ObserverModule.getInstance().theRouter = null;
          serverRunning = false;
        }
      }
      lockStop = false;
    }
    public void stopServer() {
      if (serverRunning && !lockStop) {
        router.closeConnections();
        router = null;
        stop();
        serverRunning = false;
        Gateway.ObserverModule.getInstance().theRouter = null;
      }
    }
    // id = 1, ..., 5
    // speed from -500 to 500
    public void directDrive(int id, short speedLeft, short speedRight) {
      var robot = robotFromId(id);
      if (robot != null)
        robot.connection.send(robot.protocol.writeDirectDrive(speedLeft, speedRight));
    }
    public iRobotRouter.Robot robotFromId(int id) {
      if (router != null)
      {
        foreach (var r in router.robots)
        {
          if (r.RobotID == id)
          {
            return r;
          }
        }
      }
      return null;
    }
    #endregion

    // possible packet types
    public enum Type
    {
      fromNetToUart = 0,
      fromUartToTcp = 1,
      fromUartToUdp = 2,
    }

    // tcp listener and udp socket
    private TcpListener listener;
    private UdpClient client;

    // connection
    public class Connection
    {
      // server this connection belongs to
      public readonly iRobotServer server;

      // tcp client and remote endpoint
      private TcpClient client;
      public readonly EndPoint remEp;

      // will be called if end of stream is reached
      public event Action eosReached = null;
      // will be called if an async exception occours
      public event Action<Exception> error = null;
      // will be called if a packet is received
      public delegate void PacketRecved(int type, byte[] buffer, int offset, int size);
      public event PacketRecved packetRecved = null;

      // use end point as string
      public override string ToString() {
        return remEp.ToString();
      }

      // close
      public void close()
      {
        toSend.Add(null);
        client.Close();
      }

      // recve
      private byte[] recvBuffer;
      private int recvSize;
      private void asyncRecvCallback(IAsyncResult r)
      {
        try
        {
          // end async read from
          int size = client.GetStream().EndRead(r);
          recvSize += size;
          // if end of stream is reached
          if (size == 0)
          {
            if (eosReached != null)
              eosReached();
            return;
          }
          // handle recved packets
          int recvOffset = 0;
          while (recvSize >= 2)
          {
            int type = recvBuffer[recvOffset + 0];
            int packetSize = recvBuffer[recvOffset + 1];
            if (recvSize < (packetSize + 2))
              break;
            recvOffset += 2;
            if (packetRecved != null)
              packetRecved(type, recvBuffer, recvOffset, packetSize);
            recvOffset += packetSize;
            recvSize -= 2 + packetSize;
          }
          // posiibly copy data to the front of the buffer
          if (recvSize != 0 && recvOffset != 0)
            Array.Copy(recvBuffer, recvOffset, recvBuffer, 0, recvSize);
          recvOffset = 0;
          // start next async read
          client.GetStream().BeginRead(recvBuffer, recvSize, recvBuffer.Length - recvSize, asyncRecvCallback, null);
        }
        catch (Exception e)
        {
          if (error != null)
            error(e);
        }
      }

      // send
      private byte[] sendBuffer;
      private void send(int type, Write write) {
        int size;
        if (write == null)
          size = 0;
        else {
          var s = new MemoryStream(sendBuffer, 2, sendBuffer.Length - 2);
          var bw = new BinaryWriter(s);
          write(bw);
          bw.Flush();
          s.Flush();
          size = (int) s.Position;
        }
        sendBuffer[0] = (byte)type;
        sendBuffer[1] = (byte)size;
        client.GetStream().Write(sendBuffer, 0, size + 2);
        client.GetStream().Flush();
      }
      // delegate: write with binary writer
      public delegate void Write(BinaryWriter bw);
      // the send worker
      private System.ComponentModel.BackgroundWorker sendWroker = new System.ComponentModel.BackgroundWorker();
      // the output queue
      private System.Collections.Concurrent.BlockingCollection<Write> toSend = new System.Collections.Concurrent.BlockingCollection<Write>();
      // the send worker thread
      private void sendWorkerDoWork(object o, System.ComponentModel.DoWorkEventArgs a) {
        try {
          while (true) {
            // take a write delegate from the queue
            var wr = toSend.Take();
            if (wr == null)
              return;
            // send the write delegate
            send((int)iRobotServer.Type.fromNetToUart, wr);
          }
        } catch (Exception e) {
          if (error != null)
            error(e);
        }
      }
      // adds a write delegate to the output queue
      public void send(Write wr) {
        toSend.Add(wr);
      }

      // start the connection
      public void start() {
        client.GetStream().BeginRead(recvBuffer, recvSize, recvBuffer.Length - recvSize, asyncRecvCallback, null);
        sendWroker.DoWork += sendWorkerDoWork;
        sendWroker.RunWorkerAsync();
      }

      // construction
      public Connection(iRobotServer server, TcpClient client, int recvBufferSize, int sendBufferSize)
      {
        this.server = server;
        this.client = client;
        recvBuffer = new byte[recvBufferSize];
        recvSize = 0;
        sendBuffer = new byte[sendBufferSize];
        remEp = client.Client.RemoteEndPoint;
      }
    }

    // is called when the server accepted a connection
    public delegate void ConnectionAccepted(Connection c);
    public ConnectionAccepted connectionAccepted = null;
    // is called when an udp packet was recved
    public delegate void UdpRecved(IPEndPoint remEp, byte[] buf, int offset, int size);
    public UdpRecved udpRecved = null;

    // will be called if an error occours
    public Action<Exception> error = null;

    // handle accept
    private void asyncAcceptCallback(IAsyncResult r)
    {
      try
      {
        var connection = new Connection(this, listener.EndAcceptTcpClient(r), 512, 512);
        if (connectionAccepted != null)
          connectionAccepted(connection);
        connection.start();
        listener.BeginAcceptTcpClient(asyncAcceptCallback, null);
      }
      catch (Exception e)
      {
        if (error != null)
          error(e);
      }
    }
    // handle udp recve
    private void udpRecvedCallback(IAsyncResult r)
    {
      try
      {
        IPEndPoint remEp = null;
        byte[] buf = client.EndReceive(r, ref remEp);
        if (udpRecved != null)
          udpRecved(remEp, buf, 1, buf.Length - 1);
        client.BeginReceive(udpRecvedCallback, null);
      }
      catch (Exception e)
      {
        if (error != null)
          error(e);
      }
    }
    // construction
    public iRobotServer(IPEndPoint localTcpEp, IPEndPoint localUdpEp)
    {
      listener = new TcpListener(localTcpEp);
      client = new UdpClient(localUdpEp);
    }
    // start the server
    internal void start()
    {
      listener.Start();
      listener.BeginAcceptTcpClient(asyncAcceptCallback, null);
      client.BeginReceive(udpRecvedCallback, null);
    }
    // stop the server
    public void stop()
    {
      listener.Stop();
      client.Close();
    }


      /*new maged
      //====================================
      //my new code "MAGED"

    // buffer for recving
    private byte[] recvBuffer = new byte[1024];
    // how much we habe recvived
    private int recved = 0;
    // handler will be called on a debug packet recved
    public event Action<string, Int32> debugRecved = null;
    // handler for begin recved 
    public void recv(IAsyncResult res)
    {

        // ----------
        // recving
        // ----------

        // end the recve process
        // read tells us how many bytes where written to the buffer
        int read = Connection.client.EndRead(res);

        // so the total bytes recved are added
        recved += read;

        // ----------
        // parsing
        // ----------
        // offset of the current parsed packet in the buffer
        int offset = 0;
        while (recved > 3)
        {
            // the length of the packets user data (containing the id)
            int packetLength = recvBuffer[offset + 1];
            // if we hve not recved a whole packet
            if (recved < packetLength)
                return;

            // get the packet id from the buffer
            int id = recvBuffer[offset + 2];
            if (id == ((int)FromUcToPc.debug))
            {
                // read (nulltermiting string) msg
                string msg = "";
                int i;
                for (i = offset + 3; recvBuffer[i] != 0; ++i)
                {
                    msg += (char)recvBuffer[i];
                }
                ++i;
                // read 32 bit value
                Int32 val = 0;
                val |= ((Int32)recvBuffer[i++]) << 24;
                val |= ((Int32)recvBuffer[i++]) << 16;
                val |= ((Int32)recvBuffer[i++]) << 8;
                val |= ((Int32)recvBuffer[i++]) << 0;
                if (debugRecved != null)
                    debugRecved(msg, val);
            }
            // we processed the packet, so "delete" it from the buffer
            offset += packetLength + 2;
            recved -= packetLength + 2;
        }
        // copy the begin of the next packet to the front of the buffer
        Array.Copy(recvBuffer, offset, recvBuffer, 0, recved);

        // start reading again
        client.GetStream().BeginRead(recvBuffer, recved, recvBuffer.Length - recved, recv, null);
    }

      /*end of my new code "MAGED"
      //====================================*/
      

  }
}
