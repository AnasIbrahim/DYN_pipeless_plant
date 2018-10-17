using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.Gateway.ConnectionModule.iRobot {
  // camera snapshot
  public class iRobotCameraSnapshot {
    public readonly iRobotVector pos;
    public readonly double angle;
    public readonly ulong time;
    public iRobotCameraSnapshot(iRobotVector pos, double angle, ulong time) {
      this.pos = pos;
      this.angle = angle;
      this.time = time;
    }
  }
  // robot snapshot
  public class iRobotRobotSnapshot {
    public readonly uint segNum;
    public readonly short distance;
    public readonly short angle;
    public readonly ulong time;
    public iRobotRobotSnapshot(uint segNum, short distance, short angle, ulong time) {
      this.segNum = segNum;
      this.distance = distance;
      this.angle = angle;
      this.time = time;
    }
  }

  public class iRobotSnapshotManager {
    // segment manager
    public readonly iRobotSegmentManager segmentManager;

    // segment snapshots of the robot
    public readonly List<iRobotRobotSnapshot> robotSnapshots = new List<iRobotRobotSnapshot>();
    // pos angle the camera thinks the robot is at a specific (camera) time
    public readonly List<iRobotCameraSnapshot> cameraSnapshots = new List<iRobotCameraSnapshot>();

    // time offset between robot and camera
    public ulong cameraTimeOffset;

    public iRobotSnapshotManager(iRobotSegmentManager segmentManager) {
      this.segmentManager = segmentManager;
    }

    public void addCameraSnapshot(iRobotCameraSnapshot cs) {
      cameraSnapshots.Add(cs);
    }
    public void addRobotSnapshot(iRobotRobotSnapshot rs) {
      robotSnapshots.Add(rs);
    }

    public void updateSnapshots() {
      if (robotSnapshots.Count == 0 || cameraSnapshots.Count == 0)
        return;
      ulong rtime = robotSnapshots[robotSnapshots.Count - 1].time;
      ulong ctime = cameraSnapshots[cameraSnapshots.Count - 1].time + cameraTimeOffset;
      if (rtime >= ctime) {
      } else {
        int i = cameraSnapshots.Count - 2;
        while (i >= 0) {
          ulong octime = cameraSnapshots[i].time + cameraTimeOffset;
          if (octime <= rtime) {
            
          }
        }
      }
    }
  }
}
