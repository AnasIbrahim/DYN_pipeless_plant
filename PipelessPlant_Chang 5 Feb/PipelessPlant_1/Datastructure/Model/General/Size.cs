using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.Datastructure.Model.General
{
    public class Size
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

        public Size(double height, double width)
        {
            this.height = height;
            this.width = width;
        }
    }
}
