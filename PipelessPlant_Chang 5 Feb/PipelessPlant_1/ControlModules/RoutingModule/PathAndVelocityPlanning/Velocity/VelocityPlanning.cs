using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.Velocity
{
    using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.DataTypes;

    class VelocityPlanning
    {
        public Decimal velocityMax = 20.0M; //cm/s
        
        public float AGVDiameter = 26.0f;
        public float safetyDistance = 13.0f; 

        public STspace[] generateSTspace(Point[] path)
        {
            Decimal[] pathLength = new Decimal[path.Length];
            Decimal[] time = new Decimal[path.Length];
            STspace[] st = new STspace[path.Length];
            StreamWriter grid_st = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\st_values.log", false);

            pathLength[0] = 0;
            time[0] = 0;

            st[0] = new STspace(pathLength[0], time[0]);

            for (int i = 1; i < path.Length; i++)
            {
                pathLength[i] = pathLength[i - 1] + dist(path[i], path[i - 1]);
                time[i] = time[i - 1] + (dist(path[i], path[i - 1])) / velocityMax;

                st[i] = new STspace(Math.Round(1 * pathLength[i], 0, MidpointRounding.AwayFromZero) / 1,
                                    Math.Round(10 * time[i], 0, MidpointRounding.AwayFromZero) / 10);
                grid_st.WriteLine(st[i].S + "\t" + st[i].T);
            }
            grid_st.Flush();
            grid_st.Close();
            return st;
        }
        public STspace[][] getForbiddenPathSegment(Point intersectionPoint, Point[] pathLowerPriority,Point[] pathHigherPriority, Point[] additionalPoint)//, float epsilon) // STSpace[][]  (output type)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //Decimal interpolation = 0.5M;//0.25f;
            //Decimal interpolationS = 1M;
            //Decimal interpolationT = 0.1f;//0.04f;

            Point forbidden1; Point forbidden2;
            Point forbidden1_forT; Point forbidden2_forT;
            List<int> possibleForbiddenPointIndexes = new List<int>();


            // get S initial and S final of the forbidden section
            List<float> RHSPoints_forS = new List<float>();
            List<float> LHSPoints_forS = new List<float>();
            List<int> RHSIndexes_forS = new List<int>();
            List<int> LHSIndexes_forS = new List<int>();

            // get T initial and T final of the forbidden section
            List<float> RHSPoints_forT = new List<float>();
            List<float> LHSPoints_forT = new List<float>();
            List<int> RHSIndexes_forT = new List<int>();
            List<int> LHSIndexes_forT = new List<int>();



            List<float> distances = new List<float>();
            List<Point> PointsOutsideCollisionRadius = new List<Point>();
            Console.WriteLine("intersectionPoint = (" + intersectionPoint.X + ", " + intersectionPoint.Y + ")");

            /**Calculating S forbidden **/
            for (int i = 0; i < pathLowerPriority.Length; i++)
            {
                Console.WriteLine("path[" + i + "] = (" + pathLowerPriority[i].X + ", " + pathLowerPriority[i].Y + ")");

                if (((float)dist(pathLowerPriority[i], intersectionPoint) > (float)(AGVDiameter + safetyDistance))) // / 2)))
                {// point on the path is outside the circle 

                    possibleForbiddenPointIndexes.Add(i);
                    if (Math.Sign((additionalPoint[0].X - intersectionPoint.X) * (pathLowerPriority[i].Y - intersectionPoint.Y) - (additionalPoint[0].Y - intersectionPoint.X) * (pathLowerPriority[i].X - intersectionPoint.X)) == 1)//(path[i].X > intersectionPoint.X)
                    {
                        RHSPoints_forS.Add((float)dist(pathLowerPriority[i], intersectionPoint));
                        RHSIndexes_forS.Add(i);
                    }
                    else if (Math.Sign((additionalPoint[0].X - intersectionPoint.X) * (pathLowerPriority[i].Y - intersectionPoint.Y) - (additionalPoint[0].Y - intersectionPoint.X) * (pathLowerPriority[i].X - intersectionPoint.X)) == -1)
                    {
                        LHSPoints_forS.Add((float)dist(pathLowerPriority[i], intersectionPoint));
                        LHSIndexes_forS.Add(i);
                    }
                    distances.Add((float)dist(pathLowerPriority[i], intersectionPoint));
                }
            }

            int forbiddenindex1 = Array.IndexOf(RHSPoints_forS.ToArray(), RHSPoints_forS.Min(element => Math.Abs(element)));
            forbidden1 = new Point(pathLowerPriority[RHSIndexes_forS[forbiddenindex1]].X, pathLowerPriority[RHSIndexes_forS[forbiddenindex1]].Y);

            Console.WriteLine(RHSIndexes_forS[forbiddenindex1]);

            int forbiddenindex2 = Array.IndexOf(LHSPoints_forS.ToArray(), LHSPoints_forS.Min(element => Math.Abs(element)));
            forbidden2 = new Point(pathLowerPriority[LHSIndexes_forS[forbiddenindex2]].X, pathLowerPriority[LHSIndexes_forS[forbiddenindex2]].Y);

            Console.WriteLine(LHSIndexes_forS[forbiddenindex2]);


            int indexStart = Math.Min(RHSIndexes_forS[forbiddenindex1], LHSIndexes_forS[forbiddenindex2]);
            int indexEnd = Math.Max(RHSIndexes_forS[forbiddenindex1], LHSIndexes_forS[forbiddenindex2]);


            STspace[] ST = generateSTspace(pathLowerPriority);

            Decimal[] forbiddenS = new Decimal[] { ST[indexStart].S, ST[indexEnd].S };//, indexStart, indexEnd };//STSpace[] forbiddenST = new STspace[indexEnd - indexStart + 1];


            /**Calculating T forbidden**/
            for (int i = 0; i < pathHigherPriority.Length; i++)
            {
                Console.WriteLine("path[" + i + "] = (" + pathHigherPriority[i].X + ", " + pathHigherPriority[i].Y + ")");

                if (((float)dist(pathHigherPriority[i], intersectionPoint) >= (float)(AGVDiameter + safetyDistance))) // / 2)))
                {// point on the path is outside the circle 

                    if (Math.Sign((additionalPoint[1].X - intersectionPoint.X) * (pathHigherPriority[i].Y - intersectionPoint.Y) - (additionalPoint[1].Y - intersectionPoint.X) * (pathHigherPriority[i].X - intersectionPoint.X)) == 1)//(path[i].X > intersectionPoint.X)
                    {
                        RHSPoints_forT.Add((float)dist(pathHigherPriority[i], intersectionPoint));
                        RHSIndexes_forT.Add(i);
                    }
                    else if (Math.Sign((additionalPoint[1].X - intersectionPoint.X) * (pathHigherPriority[i].Y - intersectionPoint.Y) - (additionalPoint[1].Y - intersectionPoint.X) * (pathHigherPriority[i].X - intersectionPoint.X)) == -1)
                    {
                        LHSPoints_forT.Add((float)dist(pathHigherPriority[i], intersectionPoint));
                        LHSIndexes_forT.Add(i);
                    }
                }
            }

            int forbiddenindex1_forT = Array.IndexOf(RHSPoints_forT.ToArray(), RHSPoints_forT.Min(element => Math.Abs(element)));
            forbidden1_forT = new Point(pathHigherPriority[RHSIndexes_forT[forbiddenindex1_forT]].X, pathHigherPriority[RHSIndexes_forT[forbiddenindex1_forT]].Y);

            Console.WriteLine(RHSIndexes_forT[forbiddenindex1_forT]);

            int forbiddenindex2_forT = Array.IndexOf(LHSPoints_forT.ToArray(), LHSPoints_forT.Min(element => Math.Abs(element)));
            forbidden2_forT = new Point(pathHigherPriority[LHSIndexes_forT[forbiddenindex2_forT]].X, pathHigherPriority[LHSIndexes_forT[forbiddenindex2_forT]].Y);

            Console.WriteLine(LHSIndexes_forT[forbiddenindex2_forT]);


            int indexStart_forT = Math.Min(RHSIndexes_forT[forbiddenindex1_forT], LHSIndexes_forT[forbiddenindex2_forT]);
            int indexEnd_forT = Math.Max(RHSIndexes_forT[forbiddenindex1_forT], LHSIndexes_forT[forbiddenindex2_forT]);

            STspace[] ST_forT = generateSTspace(pathHigherPriority);

            Decimal[] forbiddenT = new Decimal[] { ST_forT[indexStart_forT].T, ST_forT[indexEnd_forT].T };//, indexStart_forT, indexEnd_forT};

            Decimal interpolationS = 2M;
            Decimal interpolationT = interpolationS * (ST[ST.Length - 1].T - ST[0].T) / (ST[ST.Length - 1].S - ST[0].S);

            /**Calculating grid forbidden**/

            STspace[][] forbidden = new STspace[(int)((ST[indexEnd].S - ST[indexStart].S) / interpolationS) + 1][];
            for (int i = 0; i < forbidden.Length; i++)
            {
                forbidden[i] = new STspace[(int)((ST_forT[indexEnd_forT].T - ST_forT[indexStart_forT].T) / interpolationT) + 1];
            }


            for (int i = 0; i < forbidden.Length; i++)
            {
                for (int j = 0; j < forbidden[0].Length; j++)
                {
                    forbidden[i][j] = new STspace(0, 0);

                }
            }
            for (int i = 0; i < forbidden.Length; i++)
            {
                for (int j = 0; j < forbidden[0].Length; j++)
                {
                    forbidden[i][j].S = ST[indexStart].S + (Decimal)(j * interpolationS);
                    forbidden[i][j].T = ST_forT[indexStart_forT].T + (Decimal)(i * interpolationT);
                    Console.WriteLine(forbidden[i][j].S + "#" + forbidden[i][j].T);
                }
            }



            //for (int i = 0; i < forbiddenST.Length; i++)
            //{
            //    float roundedS = ST[i + indexStart].S;//(float) Math.Round(4*ST[i + indexStart].S, 0, MidpointRounding.AwayFromZero)/4;
            //    float roundedT = ST[i + indexStart].T;//(float) Math.Round(4*ST[i + indexStart].T, 0, MidpointRounding.AwayFromZero)/4;
            //    forbiddenST[i] = new STspace(roundedS,roundedT);
            //    //forbiddenST[i] = ST[i + indexStart];
            //}


            //STspace[][] forbidden = new STspace[((int)((forbiddenST[forbiddenST.Length - 1].T - forbiddenST[0].T) / interpolationT) + 1)][];
            //for (int i = 0; i < forbidden.Length; i++)
            //{
            //    forbidden[i] = new STspace[((int)((forbiddenST[forbiddenST.Length - 1].S - forbiddenST[0].S) / interpolationS) + 1)];
            //}


            //for (int i = 0; i < (int)((forbiddenST[forbiddenST.Length - 1].T - forbiddenST[0].T) / interpolationT) + 1; i++)
            //{
            //    for (int j = 0; j < (int)((forbiddenST[forbiddenST.Length - 1].S - forbiddenST[0].S) / interpolationS) + 1; j++)
            //    {
            //        forbidden[i][j] = new STspace(0,0); 

            //    }
            //}
            //for (int i = 0;i<(int)((forbiddenST[forbiddenST.Length - 1].T - forbiddenST[0].T) / interpolationT)+1;i++) 
            //{
            //    for (int j = 0; j < (int)((forbiddenST[forbiddenST.Length - 1].S - forbiddenST[0].S) / interpolationS) + 1; j++)
            //    {
            //        forbidden[i][j].S = forbiddenST[0].S + (j * interpolationS);
            //        forbidden[i][j].T = forbiddenST[0].T + (i * interpolationT);
            //        Console.WriteLine(forbidden[i][j].S + "#" + forbidden[i][j].T);
            //    }
            //}

            STspace[] forbiddenStartAndEnd = new STspace[2];
            forbiddenStartAndEnd[0] = new STspace(forbiddenS[0], forbiddenT[0]);
            forbiddenStartAndEnd[1] = new STspace(forbiddenS[1], forbiddenT[1]);

            //return forbiddenStartAndEnd;
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds / 1000);
            return forbidden;
            //return forbiddenS;
        }

        public Decimal dist(Point p1, Point p2)
        {
            return (Decimal)(Math.Sqrt((double)(Math.Pow((double)(p1.X - p2.X), 2) + Math.Pow((double)(p1.Y - p2.Y), 2))));
        }

        public Decimal getMaxVelocity()
        {
            return velocityMax;
        }

        public float getPathLength(Point pathPoint, Point[] path)
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
