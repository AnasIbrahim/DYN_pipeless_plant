using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MULTIFORM_PCS.ControlModules.CameraModule.Algorithm
{
    class BufferFilter
    {
        private int robID;
        private int size;
        private PointF[] coords;
        private double[] rot;
        private DateTime[] timestamps;
        private int pointer;

        public BufferFilter(int robID, int size)
        {
            if (size < 2)
            {
                size = 2;
            }
            this.robID = robID;
            this.coords = new PointF[size];
            this.rot = new double[size];
            this.timestamps = new DateTime[size];
            this.pointer = 0;
            this.size = size;
        }

        public void insert(float x, float y, double rot, DateTime now)
        {
            pointer = (pointer + 1) % size;
            this.coords[pointer] = new PointF(x,y);
            this.rot[pointer] = rot;
            this.timestamps[pointer] = now;
        }

        public PointF getFilteredPosition()
        {
          int last = (pointer - 1);
          if (last == -1)
          {
            last = size - 1;
          }
            if (coords[last] == null)
            {
                return coords[pointer];
            }
            else
            {
                if (euclidianDistance(coords[last], coords[pointer]) > 60)
                {
                    return coords[last];
                }
                else
                {
                    return coords[pointer];
                }
            }
        }
        public PointF getLastPosition()
        {
                return coords[pointer];
        }
        public double getFilteredRotation()
        {
          int last = (pointer - 1);
          if (last == -1)
          {
            last = size - 1;
          }
            if (Math.Abs(rot[last] - rot[pointer]) > 90 && Math.Abs(rot[last] - rot[pointer]) < 270)
            {
                return rot[last];
            }
            else
            {
                return rot[pointer];
            }
        }
        public double getLastRotation()
        {
            return rot[pointer];
        }
        public DateTime getFilteredTimeStamp()
        {
            //throw new NotImplementedException();
          return getLastTimeStamp();
        }
        public DateTime getLastTimeStamp()
        {
            return timestamps[pointer];
        }
        public int getAGVID()
        {
            return robID;
        }

        private double euclidianDistance(PointF p1, PointF p2)
        {
            double dist = Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            dist = Math.Round(dist, 2);
            return dist;
        }
    }
}
