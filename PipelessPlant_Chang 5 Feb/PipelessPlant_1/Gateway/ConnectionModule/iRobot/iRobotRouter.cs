using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Net;
using System.Net.Sockets;

namespace MULTIFORM_PCS.Gateway.ConnectionModule.iRobot {

  // router
  public class iRobotRouter {
    // robot that is routed
    public class Robot {

      #region quickHack
      private bool isConnected;
      public bool IsConnected {
        get { return isConnected; }
        set { isConnected = value; }
      }
      private string ip;
      public string Ip {
        get { return ip; }
      }
      private string port;
      public string Port {
        get { return port; }
        set { port = value; }
      }

      private int robotID;
      public int RobotID {
        get { return robotID; }
      }

      public short left = 0;
      public short right = 0;

      public void setup() 
      {
        string[] splitLine = connection.ToString().Split(':');
        this.ip = splitLine[0];
        this.port = splitLine[1];
        string last = ip.Substring(ip.Length - 1);
        this.robotID = int.Parse(last) - 1;
        this.isConnected = true;
        Gateway.ObserverModule.getInstance().modelChanged();
      }
      #endregion

      // log for debug information
      private void logMsg(String msg) {
        router.logMsg(connection.remEp.ToString() + ": " + msg);
      }


      // pos and angle
      public class PosAngle {
        public readonly iRobotVector pos;
        public readonly double angle;
        public PosAngle(iRobotVector pos, double angle) {
          this.pos = pos;
          this.angle = angle;
        }
      }

      // write the ip address
      public override string ToString() {
        return connection.ToString();
      }

      // the state
      public enum RobotState {
        Idle,
        SegmentDriving,
        SegmentWaiting,
        DirectDriving,
        StationDocking,
        StationDocked,
        StationUndocking,
        DemoDocking,
        Error,
      }
      private RobotState state = RobotState.Idle;
      public RobotState State {
        get { return state; }
      }
      public event Action StateChanged = null;

      // the router this robot belongs to
      public readonly iRobotRouter router;
      // the connection this robot belongs to
      public readonly iRobotServer.Connection connection;
      // the protocol
      public readonly iRobotProtocol protocol = new iRobotProtocol();
      // the segemnt manager
      private iRobotSegmentManager segmentManager = new iRobotSegmentManager();
      // the snapshot manager
      private iRobotSnapshotManager snapshotManager;

      // the current position-angle based on the last segment done
      private PosAngle lastDoneRobotCurrent = new PosAngle(new iRobotVector(0, 0), 0);
      public PosAngle LastDoneRobotCurrent {
        get { return lastDoneRobotCurrent; }
      }
      // the current position-angle based on the llast segment update
      private PosAngle lastUpdatedRobotCurrent = new PosAngle(new iRobotVector(0, 0), 0);
      public PosAngle LastUpdatedRobotCurrent {
        get { return lastUpdatedRobotCurrent; }
      }

      // the goal position-angle
      private PosAngle goal = new PosAngle(new iRobotVector(0, 0), (0));
      public PosAngle Goal {
        get { return goal; }
      }
      
      public void init(PosAngle current, PosAngle goal) {
        this.lastDoneRobotCurrent = current;
        this.lastUpdatedRobotCurrent = current;
        this.goal = goal;

        if (updateCurrent != null)
          updateCurrent(lastUpdatedRobotCurrent.pos.x, lastUpdatedRobotCurrent.pos.y, lastUpdatedRobotCurrent.angle);
      }

      // is called if new sensor data is recved
      public event Action<iRobotSensorData> sensorUpdate = null;
      // is called if the the 
      public event Action<double, double, double> updateCurrent = null;

      public MULTIFORM_PCS.MotionControl motionControl;
      // set next goal for driving
      public void SetGoal(PosAngle g) {
          motionControl.updateGoal(new double[] {g.pos.x, g.pos.y, g.angle});

        /*
        if (state != RobotState.SegmentDriving) {
          segmentManager = new iRobotSegmentManager();
          snapshotManager = new iRobotSnapshotManager(segmentManager);
          state = RobotState.SegmentDriving;
          if (StateChanged != null)
            StateChanged();
        }

        if (segmentManager.lastDoneSegmentNum == (segmentManager.nextSendSegmentNum - 1)) {
          var a = Gateway.ObserverModule.getInstance().getCurrentPlant().getAGV(robotID);
          double ang = a.theRotation * Math.PI / 180;
          while (ang <= -Math.PI)
            ang += 2.0 * Math.PI;
          while (ang > Math.PI)
            ang -= 2.0 * Math.PI;
          lastDoneRobotCurrent = new PosAngle(new iRobotVector(a.theCurPosition.X * 10, a.theCurPosition.Y * 10), ang);
          lastUpdatedRobotCurrent = new PosAngle(lastDoneRobotCurrent.pos, lastDoneRobotCurrent.angle);
          updateCurrent(lastUpdatedRobotCurrent.pos.x, lastUpdatedRobotCurrent.pos.y, lastUpdatedRobotCurrent.angle);
        }

        goal = g;

        int firstSegNum = segmentManager.getFirstPossibleUpdateSegNum();
        // update current position angle
        var current = new PosAngle(this.lastDoneRobotCurrent.pos, this.lastDoneRobotCurrent.angle);
        for (int sn = segmentManager.lastDoneSegmentNum + 1; sn < firstSegNum; ++sn) {
          current = new PosAngle(segmentManager.get(sn).getEndPos(current.pos, current.angle), segmentManager.get(sn).getEndAngle(current.angle));
        }

        // current direction vector
        var cdir = new iRobotVector(Math.Cos(current.angle), Math.Sin(current.angle)).norm;
        // dest direction vector
        var ddir = goal.pos.sub(current.pos).norm;
        double angle = Math.Acos(cdir.mul(ddir));
        if (cdir.rot.mul(ddir) > 0)
          angle = -angle;
        logMsg("delta angle: " + angle);
        var l = new List<iRobotSegment>();
        l.Add(iRobotSegment.fromTurn(angle, 100));
        l.Add(iRobotSegment.fromStraigt(goal.pos.sub(current.pos).length, 100));
        l.Add(iRobotSegment.fromTurn(goal.angle - (current.angle + angle), 100));
        l.Add(iRobotSegment.fromZero());
        segmentManager.updateList(firstSegNum, l);
        segmentManager.updateSending(connection, protocol);
        */
      }
      // dock with demo
      public void DockDemo() {
        state = RobotState.DemoDocking;
        connection.send(protocol.writeDockDemo());
        if (StateChanged != null)
          StateChanged();
      }
      // drive direct
      public void DriveDirect(short left, short right) {
        state = RobotState.DirectDriving;
        connection.send(protocol.writeDirectDrive(left, right));
        if (StateChanged != null)
          StateChanged();
      }
      // dock to station
      public void Dock() {
        state = RobotState.StationDocking;
        connection.send(protocol.writeDock());
        if (StateChanged != null)
          StateChanged();
      }
      public void Undock() {
        state = RobotState.StationUndocking;
        connection.send(protocol.writeUndock());
        if (StateChanged != null)
          StateChanged();
      }

      public void addCameraSnapshot(iRobotCameraSnapshot cs) {
        snapshotManager.cameraSnapshots.Add(cs);
      }

      public Robot(iRobotRouter router, iRobotServer.Connection connection) {
        this.router = router;
        this.connection = connection;
        this.snapshotManager = new iRobotSnapshotManager(segmentManager);
        // remove robot from list if connection is closed
        connection.eosReached += () => router.invoke(() => {
          router.robots.Remove(this);
          router.robotMap.Remove(connection.remEp);
          if (router.robotRemoved != null)
            router.robotRemoved(this);
        });
        connection.error += (desc) => router.invoke(() => {
          router.robots.Remove(this);
          router.robotMap.Remove(connection.remEp);
          if (router.robotRemoved != null)
            router.robotRemoved(this);
        });
        protocol.stateUpdate += s => router.invoke(() => {
          state = (RobotState) s;
          if (StateChanged != null)
            StateChanged();
        });
        // update done segments
        protocol.updateDoneSegments += (uint segNum, ulong timestamp) => router.invoke(() => {
          logMsg("seg done: " + segNum);

          // if we are not driving segments
          if (state != RobotState.SegmentDriving)
            return;

          // ignore if its old information
          if (segNum <= segmentManager.lastUpdateSegmentNum)
            return;

          var s = segmentManager.get((int) segNum);
          snapshotManager.addRobotSnapshot(new iRobotRobotSnapshot(segNum, s.segment.endDist, s.segment.endAngle, timestamp));

          // update current position angle
          for (int sn = segmentManager.lastDoneSegmentNum + 1; sn <= segNum; ++sn) {
            lastDoneRobotCurrent = new PosAngle(segmentManager.get(sn).getEndPos(lastDoneRobotCurrent.pos, lastDoneRobotCurrent.angle), segmentManager.get(sn).getEndAngle(lastDoneRobotCurrent.angle));
          }
          lastUpdatedRobotCurrent = new PosAngle(lastDoneRobotCurrent.pos, lastDoneRobotCurrent.angle);

          // update last updated segment
          segmentManager.lastUpdateSegmentNum = (int)segNum;
          segmentManager.lastUpdateSegmentDelta = 1.0;
          // update last done segment
          segmentManager.lastDoneSegmentNum = (int)segNum;
          // remove unused segments
          segmentManager.removeSegmentsAtBegin((int) segNum);
          // update sending
          segmentManager.updateSending(connection, protocol);

          // tell the word that our position was updated
          if (updateCurrent != null)
            updateCurrent(lastDoneRobotCurrent.pos.x, lastDoneRobotCurrent.pos.y, lastDoneRobotCurrent.angle);

          if (segmentManager.lastDoneSegmentNum == (segmentManager.nextSendSegmentNum - 1)) {
            state = RobotState.Idle;
            if (StateChanged != null)
              StateChanged();
          }
        });
        protocol.segmentUpdate += (uint segNum, ulong timestamp, short distance, short angle) => router.invoke(() => {
          logMsg("seg update: " + segNum);

          // if we are not driving segments
          if (state != RobotState.SegmentDriving)
            return;

          // ignore if its old information
          if (segNum < segmentManager.lastUpdateSegmentNum)
            return;
          // calculate delta
          var segment = segmentManager.get((int) segNum);
          double delta = (segment.angle == 0) ? (((double)distance) / ((double)segment.segment.endDist)) : (((double)angle) / ((double)segment.segment.endAngle));
//          GUI.PCSMainWindow.getInstance().postStatusMessage("Angle: " + angle + ";" + segment.angle + ";" + delta);
          // ignore if its old information
          if (segNum == segmentManager.lastUpdateSegmentNum && delta < segmentManager.lastUpdateSegmentDelta)
            return;

          snapshotManager.addRobotSnapshot(new iRobotRobotSnapshot(segNum, distance, angle, timestamp));

          // update current position angle
          for (int sn = segmentManager.lastDoneSegmentNum + 1; sn < segNum; ++sn) {
            lastDoneRobotCurrent = new PosAngle(segmentManager.get(sn).getEndPos(lastDoneRobotCurrent.pos, lastDoneRobotCurrent.angle), segmentManager.get(sn).getEndAngle(lastDoneRobotCurrent.angle));
          }
          lastUpdatedRobotCurrent = new PosAngle(segment.getEndPos(lastDoneRobotCurrent.pos, lastDoneRobotCurrent.angle, delta), segment.getEndAngle(lastDoneRobotCurrent.angle, delta));
          if (updateCurrent != null)
            updateCurrent(lastUpdatedRobotCurrent.pos.x, lastUpdatedRobotCurrent.pos.y, lastUpdatedRobotCurrent.angle);
        });
        protocol.debug += (String msg, int val) => router.invoke(() => { logMsg("debug " + msg + " " + val.ToString()); });
        protocol.assert += (String file, int line) => router.invoke(() => logMsg("assert " + file + " " + line.ToString()));
        protocol.sensorUpdate += (iRobotSensorData sensorData) => router.invoke(() => { if (sensorUpdate != null) sensorUpdate(sensorData); });
        connection.packetRecved += protocol.parsePacket;
      }
    }

    private Action<Action> invoke;
    public readonly iRobotServer server;
    public readonly List<Robot> robots = new List<Robot>();
    public readonly Dictionary<EndPoint, Robot> robotMap = new Dictionary<EndPoint, Robot>();

    public event Action<Robot> robotAdded = null;
    public event Action<Robot> robotRemoved = null;
    public event Action<String> log = null;

    private void logMsg(String msg) {
      if (log != null)
        log(msg);
    }

    public void update(Datastructure.Model.Plant plant) {
      if (!server.ServerRunning)
        return;
      foreach (var p in plant.AllAGVs) {
        var r = server.robotFromId(p.Id);
        if (r != null)
          r.addCameraSnapshot(new iRobotCameraSnapshot(new iRobotVector(p.theCurPosition.X * 10, p.theCurPosition.Y * 10), p.theRotation, (ulong)(p.LastUpdateCam.Ticks / 10000)));
      }
    }

    public void closeConnections() {
      foreach (var r in robots) {
        r.connection.close();
      }
    }

    public iRobotRouter(iRobotServer server, Action<Action> invoke) {
      this.server = server;
      this.invoke = invoke;
      server.connectionAccepted = (iRobotServer.Connection c) => invoke(() => {
        var robot = new Robot(this, c);
        robots.Add(robot);
        robotMap.Add(c.remEp, robot);
        if (robotAdded != null)
          robotAdded(robot);
        robot.motionControl = new MotionControl(robot);
        robot.motionControl.updateGoal(new double[] { 2000, 1500, (Math.PI /180) *270 });//goal point is in mm,mm,degrees
       
      });
    }
  }
}
