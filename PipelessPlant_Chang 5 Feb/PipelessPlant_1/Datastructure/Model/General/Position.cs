using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.Datastructure.Model.General
{
    public class Position
    {
        private double x;
        public double X
        {
            get { return x; }
            set { x = value; }
        }
        private double y;
        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        public Position(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
