using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.DataTypes
{
    class Position
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

        public Position(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class StationSize
    {
        private double height;
        public double Height
        {
            get { return height; }
            set { height = value; }
        }
        private double width;
        public double Width
        {
            get { return width; }
            set { width = value; }
        }

        public StationSize(double height, double width)
        {
            this.height = height;
            this.width = width;
        }
    }

    class Size
    {
        private decimal height;
        public decimal Height
        {
            get { return height; }
            set { height = value; }
        }
        private decimal width;
        public decimal Width
        {
            get { return width; }
            set { width = value; }
        }

        public Size(decimal height, decimal width)
        {
            this.height = height;
            this.width = width;
        }
    }

}
