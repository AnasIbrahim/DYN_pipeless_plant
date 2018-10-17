using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.Velocity
{
    using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.DataTypes;

    class STspace
    {
        VelocityPlanning vp = new VelocityPlanning();  //just for getting velocityMax
        private decimal s;
        public decimal S
        {
            get { return s; }
            set { s = value; }
        }
        private decimal t;
        public decimal T
        {
            get { return t; }
            set { t = value; }
        }
        public static STspace operator +(STspace a, STspace b)
        {
            return new STspace(a.s + b.s, a.t + b.t);
        }


        public STspace(decimal s, decimal t)
        {
            this.s = s;
            this.t = t;
        }

        public STspace[] generateSTspace(Position[] path,decimal startingTime)
        {
            decimal[] pathLength = new decimal[path.Length];
            decimal[] time = new decimal[path.Length];
            STspace[] st = new STspace[path.Length];
            //StreamWriter grid_st = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\st_values.log", false);

            //decimal velocityMax = 25.0M;

            pathLength[0] = 0;
            time[0] = startingTime;

            st[0] = new STspace(pathLength[0], time[0]);

            for (int i = 1; i < path.Length; i++)
            {
                pathLength[i] = pathLength[i - 1] + Math.Round(dist(path[i], path[i - 1]),0,MidpointRounding.AwayFromZero);
                time[i] = time[i - 1] + Math.Round(dist(path[i], path[i - 1]), 0, MidpointRounding.AwayFromZero) / vp.getMaxVelocity();//velocityMax;
                st[i] = new STspace(pathLength[i] ,time[i]);
                /*new STspace(Math.Round(1 * pathLength[i], 0, MidpointRounding.AwayFromZero) / 1,
                                   Math.Round(10 * time[i], 0, MidpointRounding.AwayFromZero) / 10); */
                //grid_st.WriteLine(st[i].S + "\t" + st[i].T);
            }
            //grid_st.Flush();
            //grid_st.Close();
            return st;
        }

        public decimal dist(Position p1, Position p2)
        {
            return (decimal)(Math.Sqrt((double)(Math.Pow((double)(p1.X - p2.X), 2) + Math.Pow((double)(p1.Y - p2.Y), 2))));
        }

        public float getPathLength(Position pathPoint, Position[] path)
        {
            int index = -1;
            for (int i = 0; i < path.Length; i++)
            {
                if (path[i].X == pathPoint.X && path[i].Y == pathPoint.Y)
                    index = i;
            }
            //int index = Array.IndexOf(path, pathPoint);
            //Console.WriteLine(pathPoint.X + "#" + pathPoint.Y);

            float pathLength = 0.0f;
            for (int i = 1; i <= index; i++)
            {
                pathLength = pathLength + (float)dist(path[i - 1], path[i]);
            }
            return pathLength;
        }



    }
}
