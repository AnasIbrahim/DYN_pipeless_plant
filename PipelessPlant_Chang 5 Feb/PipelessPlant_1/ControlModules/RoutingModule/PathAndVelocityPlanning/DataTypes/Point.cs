using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.DataTypes
{
    class Point
    {
        private float x;
        public float X
        {
            get { return x; }
            set { x = value; }
        }
        private float y;
        public float Y
        {
            get { return y; }
            set { y = value; }
        }
        public static Point operator +(Point a, Point b)
        {
            return new Point(a.x + b.x, a.y + b.y);
        }


        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
