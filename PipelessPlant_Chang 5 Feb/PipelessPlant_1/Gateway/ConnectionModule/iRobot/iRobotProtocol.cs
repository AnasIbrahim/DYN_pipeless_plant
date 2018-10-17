using System;
using System.IO;
using System.Collections.Generic;

namespace MULTIFORM_PCS.Gateway.ConnectionModule.iRobot
{
  // the protocol of the robot
  public class iRobotProtocol
  {
    // cmds from pc to uc
    public enum FromPcToUc : int
    {
      addSegments = 0,
      dock,
      undock,
      interrupt,
      directDrive,
      dockDemo,
    }
    // returns a delegate to write the specified cmd
    public iRobotServer.Connection.Write writeCmd(FromPcToUc cmd, iRobotServer.Connection.Write write = null)
    {
      return (BinaryWriter bw) =>
      {
        bw.Write((byte)cmd);
        if (write != null)
          write(bw);
      };
    }
    // returns a delegate to write the dock cmd
    public iRobotServer.Connection.Write writeDock()
    {
      return writeCmd(FromPcToUc.dock);
    }
    // returns a delegate to write the undock cmd
    public iRobotServer.Connection.Write writeUndock()
    {
      return writeCmd(FromPcToUc.undock);
    }
    // a segment to drive
    public struct Segment
    {
      public readonly short speedLeft;    // speed of the left wheel in mm / sec
      public readonly short speedRight;   // speed of the right wheel in mm / sec
      public readonly short endDist;      // destination distance in mm (0 if not used)
      public readonly short endAngle;     // destination angle in degree (0 if not used)
      public String toString()
      {
        return "{sl=" + speedLeft + ";sr=" + speedRight + ";endDist=" + endDist + ";endAngle=" + endAngle + "}";
      }
      public Segment(short speedLeft, short speedRight, short endDist, short endAngle) {
        this.speedLeft = speedLeft;
        this.speedRight = speedRight;
        this.endDist = endDist;
        this.endAngle = endAngle;
        /*
        if (endAngle < 0) {
          if (endDist != 0)
            throw new Exception("fuck");
          if ((speedLeft + speedRight) >= 0) {
            if (speedLeft <= speedRight)
              throw new Exception("fuck");
          } else {
            if (speedLeft >= speedRight)
              throw new Exception("fuck");
          }
        } else if (endAngle > 0) {
          if (endDist != 0)
            throw new Exception("fuck");
          if ((speedLeft + speedRight) > 0) {
            if (speedLeft >= speedRight)
              throw new Exception("fuck");
          } else if ((speedLeft + speedRight) < 0) {
            if (speedLeft <= speedRight)
              throw new Exception("fuck");
          } else {
            if (speedLeft == 0)
              throw new Exception("fuck");
          }
        } else {
          if ((speedLeft - speedRight) != 0)
            throw new Exception("fuck");
          if (speedLeft > 0) {
            if (endDist <= 0)
              throw new Exception("fuck");
          } else if (speedLeft < 0) {
            if (endDist >= 0)
              throw new Exception("fuck");
          } else {
            if (endDist != 0)
              throw new Exception("fuck");
          }
        }
        */
      }
    }
    // returns a delegate to write the add segments cmd.
    // the robot will change its state to idle if the last segment it has to process has zero speed,
    // everything else is fatal error.
    // segments may be changed by using segment numbers lower than the last one sent.
    // each segment equal or greater than the first one specified will be discarded and the new ones will be used.
    // if the robot has to change a segment already done or started it will result in an fatal error.
    public iRobotServer.Connection.Write writeAddSegments(uint firstSegNum, List<Segment> segments)
    {
      return writeCmd(FromPcToUc.addSegments, (BinaryWriter bw) =>
      {
        bw.Write(System.Net.IPAddress.HostToNetworkOrder((int)firstSegNum));
        bw.Write((byte)segments.Count);

        foreach (Segment dc in segments)
        {
          bw.Write(System.Net.IPAddress.HostToNetworkOrder((short) dc.speedLeft));
          bw.Write(System.Net.IPAddress.HostToNetworkOrder((short) dc.speedRight));
          bw.Write(System.Net.IPAddress.HostToNetworkOrder((short) dc.endDist));
          bw.Write(System.Net.IPAddress.HostToNetworkOrder((short) dc.endAngle));
        }
      });
    }

    public iRobotServer.Connection.Write writeDirectDrive(short speedLeft, short speedRight) {
      return writeCmd(FromPcToUc.directDrive, (BinaryWriter bw) => {
        bw.Write(System.Net.IPAddress.HostToNetworkOrder((short)speedLeft));
        bw.Write(System.Net.IPAddress.HostToNetworkOrder((short)speedRight));
      });
    }
    public iRobotServer.Connection.Write writeDockDemo() {
      return writeCmd(FromPcToUc.dockDemo);
    }

    // returns a delegate to write the interrupt cmd
    public iRobotServer.Connection.Write writeInterrupt()
    {
      return writeCmd(FromPcToUc.interrupt);
    }


    // packet from uc to pc
    enum FromUcToPc : int
    {
      assert = 0,
      debug,
      segmentDone,
      sensorUpdate,
      segmentUpdate,
      stateUpdate,
    }
    // is called when a debug packet is recved
    public event Action<String, int> debug = null;
    // is called when a assert packet is recved
    public event Action<String, int> assert = null;
    // is called when a update dóne segments packet is recved
    public delegate void UpdateDoneSegments(uint num, ulong timestamp);
    public event UpdateDoneSegments updateDoneSegments = null;
    public delegate void SegmentUpdate(uint num, ulong timestamp, short distance, short angle);
    public event SegmentUpdate segmentUpdate = null;
    // is called when a sensor update packet is recved
    public event Action<iRobotSensorData> sensorUpdate = null;
    // is called when a stat update is recved
    public event Action<byte> stateUpdate = null;
    // handles recved packets
    public void parsePacket(int type, byte[] buffer, int index, int count)
    {
      var s = new MemoryStream(buffer, index, count);
      var br = new BinaryReader(s);
      int id = br.ReadByte();
      switch (id)
      {
        // assert
        case (int) FromUcToPc.assert:
        {
          int strLen = count - 4;
          System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
          var file = enc.GetString(buffer, index + 1, strLen);
          for (int i = 0; i < strLen; ++i)
            br.ReadByte();
          br.ReadByte();
          int line = (ushort) System.Net.IPAddress.NetworkToHostOrder((short)br.ReadUInt16());
          if (assert != null)
            assert(file, line);
          break;
        }
        // segment done
        case (int)FromUcToPc.segmentDone:
        {
          uint segNum = (uint)System.Net.IPAddress.NetworkToHostOrder((int)br.ReadUInt32());
          ulong timestamp = (ulong)System.Net.IPAddress.NetworkToHostOrder((long)br.ReadUInt64());
          if (updateDoneSegments != null)
            updateDoneSegments(segNum, timestamp);
          break;
        }
        // segment update
        case (int)FromUcToPc.segmentUpdate:
        {
          uint segNum = (uint)System.Net.IPAddress.NetworkToHostOrder((int)br.ReadUInt32());
          ulong timestamp = (ulong)System.Net.IPAddress.NetworkToHostOrder((long)br.ReadUInt64());
          short distance = (short)System.Net.IPAddress.NetworkToHostOrder((short)br.ReadInt16());
          short angle = (short)System.Net.IPAddress.NetworkToHostOrder((short)br.ReadInt16());
          if (segmentUpdate != null)
            segmentUpdate(segNum, timestamp, distance, angle);
          break;
        }
        // state update
        case (int)FromUcToPc.stateUpdate: {
          byte state = br.ReadByte();
          if (stateUpdate != null)
            stateUpdate(state);
          break;
        }
        // debug
        case (int) FromUcToPc.debug:
        {
          int strLen = count - 6;
          System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
          var str = enc.GetString(buffer, index + 1, strLen);
          for (int i = 0; i < strLen; ++i)
            br.ReadByte();
          br.ReadByte();
          int v = System.Net.IPAddress.NetworkToHostOrder(br.ReadInt32());
          if (debug != null)
            debug(str, v);
          break;
        }
        // sensor data update
        case (int)FromUcToPc.sensorUpdate:
        {
          var sd = new iRobotSensorData(br);
          if (sensorUpdate != null)
            sensorUpdate(sd);
          break;
        }
        default:
          break;
      }
    }
  }
}
