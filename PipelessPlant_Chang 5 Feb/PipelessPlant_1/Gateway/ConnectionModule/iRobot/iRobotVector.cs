using System;

namespace MULTIFORM_PCS.Gateway.ConnectionModule.iRobot {
  public struct iRobotVector {
    public readonly double x;
    public readonly double y;
    public readonly double length;
    public iRobotVector(double x, double y) {
      this.x = x;
      this.y = y;
      this.length = Math.Sqrt(x * x + y * y);
    }
    public iRobotVector sub(iRobotVector o) {
      return new iRobotVector(x - o.x, y - o.y);
    }
    public iRobotVector add(iRobotVector o) {
      return new iRobotVector(x + o.x, y + o.y);
    }
    public iRobotVector mul(double s) {
      return new iRobotVector(x * s, y * s);
    }
    public double mul(iRobotVector o) {
      return x * o.x + y * o.y;
    }
    public iRobotVector div(double d) {
      return new iRobotVector(x / d, y / d);
    }
    public iRobotVector rot {
      get {
        return new iRobotVector(y, -x);
      }
    }
    public iRobotVector norm {
      get {
        return div(length);
      }
    }
  }
}
