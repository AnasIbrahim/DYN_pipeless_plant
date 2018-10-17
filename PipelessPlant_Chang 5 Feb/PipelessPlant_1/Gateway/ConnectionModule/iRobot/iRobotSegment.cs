using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MULTIFORM_PCS.Gateway.ConnectionModule.iRobot {

  // extended segment
  public class iRobotSegment {
    // (half) wheel distance
    public const double wheelDist = 200;
    public const double wheelDistHalf = 0.5 * wheelDist;

    // times are in milliseconds
    // distances are in millimeters
    // angles are in radian 
    // angleRadian: angle in radian
    // radiusLeft: radius at the left wheel of the robot
    // radiusMid: radius at the middle of the robot
    // radiusRight: radius at the right wheel of the robot
    // distance: distance the robot drives
    // speedLeft: translation speed at the left wheel of the robot
    // speedMid: translation speed at the middle of the robot
    // speedRight: translation speed at the right wheel of the robot
    // distance = radiusMid * angleRadian
    // robot should turn forward to the left:
    //   distance > 0
    //   angleRadian > 0
    //   radiusMid > 0
    // robot should turn backwards to the left:
    //   distance < 0
    //   angleRadian < 0
    //   radiusMid > 0
    // robot should turn forward to the right:
    //   distance > 0
    //   angleRadian < 0
    //   radiusMid < 0
    // robot should turn backwards to the right:
    //   distance < 0
    //   angleRadian > 0
    //   radiusMid < 0
    public readonly double distance;
    public readonly double angle;
    public readonly double radiusLeft;
    public readonly double radiusMid;
    public readonly double radiusRight;
    public readonly double speedLeft;
    public readonly double speedMid;
    public readonly double speedRight;
    public readonly double time;

    // relative movement of the robot in this segment
    public readonly Matrix transform;
    public iRobotVector getEndPos(iRobotVector startPos, double angle, double factor = 1.0) {
      if (this.distance == 0) {
        return startPos;
      } else if (this.angle == 0) {
        return startPos.add(new iRobotVector(Math.Cos(angle) * (factor * distance), Math.Sin(angle) * (factor * distance)));
      } else {
        throw new Exception("todo: handle this case");
      }
    }
    public double getEndAngle(double angle, double factor = 1.0) {
      double clampedAngle = angle + (factor * this.angle);
      double zwoPi = 2.0 * Math.PI;
      while (clampedAngle > Math.PI)
        clampedAngle -= zwoPi;
      while (clampedAngle <= -Math.PI)
        clampedAngle += zwoPi;
      return clampedAngle;
    }

    // the basic segment information (robot protocol)
    public readonly iRobotProtocol.Segment segment;

    // multiply movement with factor
    public iRobotSegment mul(double factor) {
      if (this.distance == 0) {
        return new iRobotSegment(distance, factor * angle, radiusLeft, radiusMid, radiusRight, speedLeft, speedMid, speedRight, factor * time, null, new iRobotProtocol.Segment(segment.speedLeft, segment.speedRight, (short) (segment.endDist * factor), (short) (segment.endAngle * factor)));
      } else if (this.angle == 0) {
        return new iRobotSegment(factor * distance, angle, radiusLeft, radiusMid, radiusRight, speedLeft, speedMid, speedRight, factor * time, null, new iRobotProtocol.Segment(segment.speedLeft, segment.speedRight, (short)(segment.endDist * factor), (short)(segment.endAngle * factor)));
      } else {
        throw new Exception("todo: handle this case");
      }

      float ffactor = (float) factor;
      var dm = new Matrix(transform.Elements[0] * ffactor, transform.Elements[1] * ffactor, transform.Elements[3] * ffactor, transform.Elements[4] * ffactor, transform.Elements[2] * ffactor, transform.Elements[5] * ffactor);
      var s = new iRobotProtocol.Segment(segment.speedLeft, segment.speedRight, (short)(((double)segment.endDist) * factor), (short)(((double)segment.endAngle) * factor));
      return new iRobotSegment(distance * factor, angle * factor, radiusLeft * factor, radiusMid * factor, radiusRight * factor, speedLeft, speedMid, speedRight, time * factor, dm, s);
    }

    // split segment
    public List<iRobotSegment> split(double deltaTime) {
      var res = new List<iRobotSegment>();
      double t = 0;
      iRobotSegment fs = mul(deltaTime / time);
      while ((time - t) >= deltaTime) {
        res.Add(fs);
        t += deltaTime;
      }
      if (time != t) {
        res.Add(mul((time - t) / time));
      }
      return res;
    }
    // split segments
    public static List<iRobotSegment> splitSegments(List<iRobotSegment> src, double deltaTime) {
      var res = new List<iRobotSegment>();
      foreach (var s in src)
        res.AddRange(s.split(deltaTime));
      return res;
    }

    public iRobotSegment(double distance, double angleRadian, double radiusLeft, double radiusMid, double radiusRight, double speedLeft, double speedMid, double speedRight, double time, Matrix deltaMovement, iRobotProtocol.Segment segment) {
      this.distance = distance;
      this.angle = angleRadian;
      this.radiusLeft = radiusLeft;
      this.radiusMid = radiusMid;
      this.radiusRight = radiusRight;
      this.speedLeft = speedLeft;
      this.speedMid = speedMid;
      this.speedRight = speedRight;
      this.time = time;
      this.transform = deltaMovement;
      this.segment = segment;
    }

    // create segment from curve
    public static iRobotSegment fromCurve(double angleRadian, double radiusMid, double speedMid) {
      var deltaMovement = new Matrix();
      deltaMovement.RotateAt((float)((angleRadian / (2 * Math.PI)) * 360.0), new System.Drawing.PointF(0, (float)-radiusMid));

      double distance = radiusMid * angleRadian;
      double radiusLeft = radiusMid - wheelDistHalf;
      double radiusRight = radiusMid + wheelDistHalf;
      // speedLeft / speedMid = radiusLeft / radiusMid
      // wheelLeft = (radiusLeft / radiusMid) * speedMid
      double speedLeft = (radiusLeft / radiusMid) * speedMid;
      // wheelRight / wheelMid = radiusRight / radiusMid
      // wheelRight = (radiusRight / radiusMid) * wheelMid
      double speedRight = (radiusRight / radiusMid) * speedMid;

      var segment = new iRobotProtocol.Segment((short)speedLeft, (short)speedRight, (short)distance, 0);

      double time = Math.Abs(distance / speedMid) * 1000.0;

      return new iRobotSegment(distance, angleRadian, radiusLeft, radiusMid, radiusRight, speedLeft, speedMid, speedRight, time, deltaMovement, segment);
    }

    // create segment from straight
    public static iRobotSegment fromStraigt(double distance, double speed) {
      var deltaMovement = new Matrix();
      deltaMovement.Translate((float)distance, 0);

      double speedMid = ((distance >= 0) ? speed : -speed);
      var segment = new iRobotProtocol.Segment((short)speedMid, (short)speedMid, (short)distance, 0);
      if (segment.speedLeft == 0 || segment.endDist == 0)
        return fromZero();

      double time = Math.Abs(distance / speed) * 1000.0;

      return new iRobotSegment(distance, 0, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, speedMid, speedMid, speedMid, time, deltaMovement, segment);
    }
    public static iRobotSegment fromTurn(double angleRadian, double speed) {

      double clampedAngle = angleRadian;
      double zwoPi = 2.0 * Math.PI;
      while (clampedAngle > Math.PI)
        clampedAngle -= zwoPi;
      while (clampedAngle <= -Math.PI)
        clampedAngle += zwoPi;
      short clampedAngleDeg = (short)((clampedAngle / (2.0 * Math.PI)) * 360.0);
      if (clampedAngleDeg == 0)
        return fromZero();

      var deltaMovement = new Matrix();
      deltaMovement.Rotate((float)(clampedAngle / (2 * Math.PI) * 360.0));
      
      double speedLeft = (angleRadian >= 0) ? -speed : speed;
      double speedRight = (angleRadian >= 0) ? speed : -speed;
      var segment = new iRobotProtocol.Segment((short)speedLeft, (short)speedRight, 0, clampedAngleDeg);

      double time = Math.Abs(wheelDistHalf * clampedAngle / speed) * 1000.0;

      return new iRobotSegment(0, clampedAngle, 0, 0, 0, speedLeft, 0, speedRight, time, deltaMovement, segment);
    }
    public static iRobotSegment fromZero() {
      var segment = new iRobotProtocol.Segment(0, 0, 0, 0);
      return new iRobotSegment(0, 0, 0, 0, 0, 0, 0, 0, 1, new Matrix(), segment);
    }
  }

  public class iRobotSegmentManager {
    // ordered list of all the segments we know about
    public List<iRobotSegment> segments = new List<iRobotSegment>();
    // number of the first segment in list
    public int listBeginSegmentNum = 1;
    // number of the last segment in list
    public int listEndSegmentNum { get { return listBeginSegmentNum + segments.Count; } }
    // what segment shoud be send next
    public int nextSendSegmentNum = 1;
    // last segment that is done
    public int lastDoneSegmentNum = 0;
    // get segment by number
    public iRobotSegment get(int num) {
      return segments[num - listBeginSegmentNum];
    }
    // remove segments from the front of the list (new first segment has number segNum)
    public void removeSegmentsAtBegin(int segNum) {
      int numSegmentsToRemove = segNum - listBeginSegmentNum;
      if (numSegmentsToRemove > 0) {
        segments.RemoveRange(0, numSegmentsToRemove);
        listBeginSegmentNum += numSegmentsToRemove;
      }
    }
    // remove segments at the end of the list (new last segment has number (segNum - 1))
    public void removeSegmentsAtEnd(int segNum) {
      int numSegmentsToRemove = listEndSegmentNum - segNum;
      if (numSegmentsToRemove > 0)
        segments.RemoveRange(segments.Count - numSegmentsToRemove, numSegmentsToRemove);
    }
    // change and / or add segments of / to the list
    public void updateList(int firstSegmentNum, List<iRobotSegment> nextSegments) {
      removeSegmentsAtEnd(firstSegmentNum);
      if (firstSegmentNum < nextSendSegmentNum) {
        nextSendSegmentNum = firstSegmentNum;
      }
      segments.AddRange(nextSegments);
    }
    // return first possible seq num to update
    public int getFirstPossibleUpdateSegNum() {
      return Math.Min(nextSendSegmentNum, lastDoneSegmentNum + 10);
    }
    // how much segments the uc can store
    public const int ucSegmentsBufferSize = 10;
    // send segments which have to be send
    public void updateSending(iRobotServer.Connection connection, iRobotProtocol protocol) {
      int numSent = nextSendSegmentNum - lastDoneSegmentNum - 1;
      int numToSend = Math.Min(ucSegmentsBufferSize - numSent, listEndSegmentNum - nextSendSegmentNum);
      if (numToSend > 0) {
        var toSendSegments = new List<iRobotProtocol.Segment>();
        for (int i = 0; i < numToSend; ++i)
          toSendSegments.Add(segments[nextSendSegmentNum + i - listBeginSegmentNum].segment);
        connection.send(protocol.writeAddSegments((uint)nextSendSegmentNum, toSendSegments));
        nextSendSegmentNum += numToSend;
      }
    }
    public int lastUpdateSegmentNum = 0;
    public double lastUpdateSegmentDelta = 0;
  }
}
