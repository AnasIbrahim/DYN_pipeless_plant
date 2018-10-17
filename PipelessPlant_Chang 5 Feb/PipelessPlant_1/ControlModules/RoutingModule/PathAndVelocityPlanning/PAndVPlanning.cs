using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning
{
    using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.DataTypes;
    using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.Path;
    using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.Velocity;
    using MULTIFORM_PCS.ControlModules.SchedulingModule;

    class PAndVPlanning
    {
        /* Constructor */
        public PAndVPlanning(AGVMovmentData agvMovementData,decimal[] startTimes)
        {
            this.agvMovementData = agvMovementData;
            Array.Resize(ref this.startTimes, startTimes.Length);
            this.startTimes = startTimes;
            //Init();
        }
        
        public Astar astar = new Astar();
        public Bspline bspline = new Bspline();
        public Orientation orientation = new Orientation();
        public STspace stSpace = new STspace(0, 0);
        public STAstar stAstar = new STAstar();
        public Plot plot = new Plot();
        public VelocityPlanning vp = new VelocityPlanning();

        public decimal time_where_higher_priority_robot_leaves_Mutual_Pathsection = -1;

        public AGVMovmentData agvMovementData;// = new AGVMovmentData();
        //public StreamWriter Astar_routes = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\astar_routes.log", false);
        
        public int bsplineDegree = 3;
        //public List<List<Position>> allPaths = new List<List<Position>>();
        public List<List<Position>> allPaths = new List<List<Position>>();

        public decimal[] startTimes = new decimal[]{};


        public decimal[] priority = new decimal[3];

        public List<STspace[]> allSTspaces  = new List<STspace[]>();
        public List<Position[]> allBsplines = new List<Position[]>();
        public List<decimal[,][]> allVelocityProfiles = new List<decimal[,][]>();
        public CollisionInfoForPath[] collisionInfoForPath = new CollisionInfoForPath[] { };
        public List<STspace[][]> allForbiddenAreas = new List<STspace[][]>();
        public List<decimal> allPathDurations = new List<decimal>();

        public float AGVDiameter = 26.5f;//26
        public float safetyDistance = 13.0f;
        //public int velocityMax = 25;

        public Decimal interpolationS;
        public Decimal interpolationT;

        public bool flag_rescheduling_needed = false;
        public bool flag_agv_low_batt = false;
        public CollisionInfoForPath[] agvLowBattCollisionInfo = new CollisionInfoForPath[] { };
        public List<int> taskIDsWithNewDuration = new List<int>();
        public List<double> Orientations = new List<double>();
        public List<List<double>> allOrientations = new List<List<double>>();

        public void Init()
        {
            calculateAllPaths();
        }
        
        public void calculateAllPaths()
        {
            for (int i = 0; i < agvMovementData.agvMovementInfo.Count; i++)
            {
                List<Position> path = astar.calculateRoute(getStationPosition(agvMovementData.agvMovementInfo[i].startPosition), getStationPosition(agvMovementData.agvMovementInfo[i].endPosition));
                STspace[] st_path = stSpace.generateSTspace(path.ToArray(), startTimes[i]);  //edited for group project -- 30.01.2015
                Orientations = orientation.getOrientations(path, agvMovementData.agvMovementInfo[i].endPosition);
                //STspace[] st_path = stSpace.generateSTspace(bspline.getBSpline(bsplineDegree, path, bspline.generateKnotVector(path.Count - 1, bsplineDegree)), startTimes[i]);
                Console.WriteLine("Calculating path " + i + " ...");
                allPathDurations.Add(st_path[st_path.Length - 1].T - st_path[0].T);
                allPaths.Add(path);
                allOrientations.Add(Orientations);
            }
        }
        
        public void calculateOnePath(int group_index,int OneAGVsequence_index)
        {
            int degree = 3;
            AGVData OneAGVsequence = agvMovementData.agvGroups[group_index][OneAGVsequence_index];

            List<Position> firstRoute = astar.calculateRoute(getStationPosition(OneAGVsequence.startPosition), getStationPosition(OneAGVsequence.endPosition));
            for (int i = 0; i < firstRoute.Count; i++)
                Console.WriteLine("Path[" + i + "] = (" + firstRoute[i].X + " , " + firstRoute[i].Y + ")");
            
            
            Position[] smoothedPath = bspline.getBSpline(degree, firstRoute, bspline.generateKnotVector(firstRoute.Count - 1, degree));
            STspace[] stFirstPath = stSpace.generateSTspace(smoothedPath,0);

            List<STspace[][]> stForbidden_FirstPath = new List<STspace[][]>();
            List<STspace> STroute = stAstar.calculateRoute(stFirstPath[0], stFirstPath[stFirstPath.Length - 1], stForbidden_FirstPath, 0, 0);
            Decimal[,]velocity_first_path = stAstar.getVelocityProfiles(STroute);

            Decimal[] velocity = new decimal[velocity_first_path.GetLength(0)];
            Decimal[] time = new decimal[velocity_first_path.GetLength(0)];

            for (int i = 0; i < velocity_first_path.GetLength(0); i++)
            {
                velocity[i] = velocity_first_path[i, 0];
                time[i] = velocity_first_path[i, 1];
            }
            

            string path = AppDomain.CurrentDomain.BaseDirectory + "plot.png";
            //plot.PlotSplineSolution("", time, velocity, path); 
            

            Decimal endTimeFirstPath = stFirstPath[stFirstPath.Length - 1].T;
            Console.WriteLine("First movement end time is : " + endTimeFirstPath);
        }

        public CollisionInfoForPath[] calculateOneGroupOfPaths(List<AGVData> AGVmovementInfo, List<List<Position>> allAstarPaths, decimal[] startTimes, List<decimal> pathDuration)
        {
            /* Reset public variables*/
            //allPaths.Clear();
            allBsplines.Clear();
            allSTspaces.Clear();
            allVelocityProfiles.Clear();

            
            int bsplineDegree = 3;
            int numberOfCommands = AGVmovementInfo.Count;


            /** Resize collisionInfoForPath to the numberOfCommands **/
            Array.Resize(ref collisionInfoForPath, numberOfCommands);            
      
            //decimal[] pathFinishingTimes = new decimal[numberOfCommands];  // to be reported to the scheduling module  
            List<IntersectionPoint> intersections = new List<IntersectionPoint>(); // assuming two paths can meet at more than one point 

            for (int i = 0; i < AGVmovementInfo.Count; i++)
            {
                //List<Position> path = astar.calculateRoute(getStationPosition(AGVmovementInfo[i].startPosition), getStationPosition(AGVmovementInfo[i].endPosition));
                //allPaths.Add(path);
                //startTimes[i] = AGVmovementInfo[i].startTime;
                allBsplines.Add(bspline.getBSpline(bsplineDegree, allAstarPaths[i], bspline.generateKnotVector(allAstarPaths[i].Count - 1, bsplineDegree)));
                allSTspaces.Add(stSpace.generateSTspace(bspline.getBSpline(bsplineDegree, allAstarPaths[i], bspline.generateKnotVector(allAstarPaths[i].Count - 1, bsplineDegree)), startTimes[i]));
                if (AGVmovementInfo[i].taskID == 61965)
                {
                    for (int k = 0; k < allSTspaces[i].Length; k++)
                        Console.WriteLine(allSTspaces[i][k].S + "\t" + allSTspaces[i][k].T);

                }
                collisionInfoForPath[i] = new CollisionInfoForPath();
                collisionInfoForPath[i].taskID  = AGVmovementInfo[i].taskID;
                collisionInfoForPath[i].usedAGV = AGVmovementInfo[i].usedAGV;
            }

            /**Check whether there exist collisions in space**/
            List<STspace[][]> allForbiddenAreasForPath = new List<STspace[][]>();
            if (numberOfCommands == 1)
            {
                //collisionInfoForPath[0] = new CollisionInfoForPath();
                collisionInfoForPath[0].pathIndex = 0;
                collisionInfoForPath[0].numberOfCollisions = 0;
                collisionInfoForPath[0].totalAdditionalTime = 0;
                collisionInfoForPath[0].additionalTimeSteps = 0;
                //collisionInfoForPath[0].flagNoCollision = true;
                collisionInfoForPath[0].updatedPathST = allSTspaces[0];
                collisionInfoForPath[0].updatedPosition = allBsplines[0];
                collisionInfoForPath[0].forbiddenAreas = null;
                collisionInfoForPath[0].numberOfForbiddenRegions = 0;
                collisionInfoForPath[0].pathFinishingTime = startTimes[0]+ pathDuration[0];
                collisionInfoForPath[0].path = allAstarPaths[0];
                collisionInfoForPath[0].allOrientations = orientation.getOrientations(allAstarPaths[0], AGVmovementInfo[0].endPosition);
            }

            else
            {
                for (int n = 0; n < numberOfCommands; n++)
                {
                    //collisionInfoForPath[n] = new CollisionInfoForPath();
                    //collisionInfoForPath[n].forbiddenAreas = new List<STspace[][]>();
                    collisionInfoForPath[n].pathIndex = n;
                    collisionInfoForPath[n].numberOfCollisions = 0;
                    collisionInfoForPath[n].totalAdditionalTime = 0;
                    collisionInfoForPath[n].additionalTimeSteps = 0;
                    collisionInfoForPath[n].allOrientations = orientation.getOrientations(allAstarPaths[n], AGVmovementInfo[n].endPosition);
                    collisionInfoForPath[n].path = allAstarPaths[n];

                    for (int i = 0; i < allAstarPaths.Count - 1; i++)
                    {
                        for (int j = 1; j < allAstarPaths.Count; j++)
                        {
                            if (i != j && (i == n || j == n) && (AGVmovementInfo[i].usedAGV != AGVmovementInfo[j].usedAGV))
                            {
                                if(i==n)
                                    collisionInfoForPath[n].usedAGV = AGVmovementInfo[i].usedAGV;
                                else if (j==n)
                                    collisionInfoForPath[n].usedAGV = AGVmovementInfo[j].usedAGV;
                                
                                if (allSTspaces[i][allSTspaces[i].Length - 1].T <= allSTspaces[j][0].T ||
                                    allSTspaces[i][0].T >= allSTspaces[j][allSTspaces[j].Length - 1].T)    // is a task finishes before the other ones start, there will be no collision 
                                {
                                    //collisionInfoForPath[n].flagNoCollision = true;
                                    continue;
                                }

                                /* Priority Assignment */
                                int[] priorities = new int[numberOfCommands];
                                int higher_priority = -1;
                                int lower_priority = -1;

                                int index_path_higher_priority = -1;
                                int index_path_lower_priority = -1;

                                //if (AGVmovementInfo[i].startPosition == AGVmovementInfo[j].endPosition ||
                                //    AGVmovementInfo[j].startPosition == AGVmovementInfo[i].endPosition)
                                ////{
                                    if (AGVmovementInfo[i].startPosition == AGVmovementInfo[j].endPosition)
                                    {
                                        // path i --> higher priority
                                        index_path_higher_priority = i; // priorities[i] = i
                                        index_path_lower_priority = j; //priorities[j] = j;
                                    }
                                    else if (AGVmovementInfo[j].startPosition == AGVmovementInfo[i].endPosition)
                                    {
                                        // path j --> higher priority
                                        index_path_higher_priority = j;
                                        index_path_lower_priority = i;


                                    }
                                    //priorities[i] = Math.Min(j,i);  //  indexPath1 = path[priorities[i]] --> path with lower priority
                                    //priorities[j] = Math.Max(i, j);//i;  //  indexPath2 = path[priorities[j]] --> path with higher priority
                                    //higher_priority = Array.IndexOf(priorities, Math.Max(priorities[i], priorities[j]));
                                    //lower_priority = Array.IndexOf(priorities, Math.Min(priorities[i], priorities[j]));

                                //}
                                else  /*Arbitrary Assignmnet*/
                                {
                                    index_path_lower_priority = i;
                                    index_path_higher_priority = j;

                                    //priorities[i] = i;
                                    //priorities[j] = j;
                                    //higher_priority = Array.IndexOf(priorities, Math.Max(priorities[i], priorities[j]));
                                    //lower_priority = Array.IndexOf(priorities, Math.Min(priorities[i], priorities[j]));

                                    //Console.WriteLine("higher_priority = " + higher_priority);
                                    //Console.WriteLine("lower_priority = " + lower_priority);
                                }
                                /*End*/
                                    List<IntersectionPoint> intrsct = checkCollisionInSpace(allAstarPaths[index_path_lower_priority], allAstarPaths[index_path_higher_priority], new int[] {index_path_lower_priority,index_path_higher_priority });//Math.Min(i,j), Math.Max(i,j) });  /*{Path with lower priority :PathIndex1, Path with higher priority : Pathindex2}*/

                                /**Check whether there exist collisions in time**/
                                //if (intrsct.Count == 0)
                                //    bool x = 1; // test //collisionInfoForPath[n].flagNoCollision = true;
                                if (intrsct.Count!= 0) //else
                                {
                                    //collisionInfoForPath[n].flagNoCollision = false;
                                    STspace[][] forbidden = new STspace[][] { };
                                    bool flag_one_segment_of_intersection = false;
                                    bool flag_more_than_one_segment_of_intersection = false;
                                    bool flag_group_of_intersection_points = false;


                                    if (intrsct.Count >= 1)
                                    {
                                        List<List<IntersectionPoint>> listOfIntersections = IsIntersectionASegmentOrAPoint(intrsct, allAstarPaths[index_path_lower_priority]);
                                        bool flag_intersection_segment = false;
                                        List<int> groupsWithIntersectionSegments = new List<int>();

                                        if (listOfIntersections.Count == 1)
                                        { /*One segment of intersection*/

                                            flag_one_segment_of_intersection = true;

                                            /* Find start and End of the forbidden area */
                                            List<STspace[][]> forbiddenForListOfIntersection = new List<STspace[][]>();
                                            for (int k = 0; k < listOfIntersections[0].Count; k++)
                                            {
                                                STspace[][] frbdn = IsThereAforbiddenPathSegment(listOfIntersections[0][k], allAstarPaths);
                                                if (frbdn != null)
                                                {
                                                    forbiddenForListOfIntersection.Add(frbdn);
                                                    Console.WriteLine("starting s&t: " + frbdn[0][0].S + " , " + frbdn[0][0].T);
                                                    Console.WriteLine("Ending s&t:" + frbdn[frbdn.Length - 1][frbdn[frbdn.Length - 1].Length - 1].S + " , " + frbdn[frbdn.Length - 1][frbdn[frbdn.Length - 1].Length - 1].T);
                                                }
                                            }

                                            

                                            int index_smallest_S = -1; int index_biggest_S = -1; int smallest_T = -1; int biggest_T = -1;
                                            if (forbiddenForListOfIntersection.Count > 1)
                                            {
                                                /* Forbidden area with the smallest starting S is forbiddenStart*/
                                                index_smallest_S = forbiddenForListOfIntersection.IndexOf(forbiddenForListOfIntersection.First(b => b[0][0].S == forbiddenForListOfIntersection.Min(e => e[0][0].S)));

                                                /* Forbidden area with the biggest finishing S is forbiddenStart*/
                                                index_biggest_S = forbiddenForListOfIntersection.IndexOf(forbiddenForListOfIntersection.First(b => b[b.Length - 1][b[b.Length - 1].Length - 1].S == forbiddenForListOfIntersection.Max(e => e[e.Length - 1][e[e.Length - 1].Length - 1].S)));

                                                /*Min forbidden T index*/
                                                smallest_T = forbiddenForListOfIntersection.IndexOf(forbiddenForListOfIntersection.First(b => b[0][0].T == forbiddenForListOfIntersection.Min(e => e[0][0].T)));
                                                /* Max forbidden T index*/
                                                biggest_T = forbiddenForListOfIntersection.IndexOf(forbiddenForListOfIntersection.First(b => b[b.Length - 1][b[b.Length - 1].Length - 1].T == forbiddenForListOfIntersection.Max(e => e[e.Length - 1][e[e.Length - 1].Length - 1].T)));

                                                /*Get T forbidden */
                                                // for a section intersction just for making sure that no collision will happen, the whole interval will be considered as forbidden

                                                decimal[] dist = new decimal[allSTspaces[index_path_higher_priority].Length];
                                                for (int l = 0; l < allBsplines[index_path_higher_priority].Length; l++)
                                                {
                                                    dist[l] = stSpace.dist(listOfIntersections[0][0].intersectPoint, allBsplines[index_path_higher_priority][l]);
                                                }
                                                decimal forbiddenT_start = allSTspaces[index_path_higher_priority][Array.IndexOf(dist, dist.Min())].T;


                                                dist = new decimal[allSTspaces[index_path_higher_priority].Length];
                                                for (int l = 0; l < allBsplines[index_path_higher_priority].Length; l++)
                                                {
                                                    dist[l] = stSpace.dist(listOfIntersections[0][listOfIntersections[0].Count - 1].intersectPoint, allBsplines[index_path_higher_priority][l]);
                                                }
                                                decimal forbiddenT_End = allSTspaces[index_path_higher_priority][Array.IndexOf(dist, dist.Min())].T;




                                                STspace[][] forbiddenStart = forbiddenForListOfIntersection[index_smallest_S];//IsThereAforbiddenPathSegment(listOfIntersections[0][0]);
                                                STspace[][] forbiddenEnd = forbiddenForListOfIntersection[index_biggest_S];//IsThereAforbiddenPathSegment(listOfIntersections[0][listOfIntersections[0].Count - 1]);

                                                //Console.WriteLine("Corresponding position index start : " + allSTspaces[index_path_lower_priority].ToList().FindIndex(item => item.S == forbiddenStart[0][0].S));
                                                //Console.WriteLine("Corresponding position index start : " + allSTspaces[index_path_lower_priority].ToList().FindIndex(item => item.S ==
                                                //                   forbiddenEnd[forbiddenEnd.Length - 1][forbiddenEnd[forbiddenEnd.Length - 1].Length - 1].S));


                                                if (n == index_path_lower_priority)
                                                {
                                                    collisionInfoForPath[n].correspondingForbiddenPoints =
                                                                       new Position[] {allBsplines[index_path_lower_priority][allSTspaces[index_path_lower_priority].ToList().FindIndex(item => item.S == forbiddenStart[0][0].S)], 
                                                                   allBsplines[index_path_lower_priority][allSTspaces[index_path_lower_priority].ToList().FindIndex(item => item.S ==
                                                                   forbiddenEnd[forbiddenEnd.Length - 1][forbiddenEnd[forbiddenEnd.Length - 1].Length - 1].S)]};
                                                }
                                                //Console.WriteLine("(" + collisionInfoForPath[n].correspondingForbiddenPoints[0].X + "," + collisionInfoForPath[n].correspondingForbiddenPoints[0].Y + ")");
                                                //Console.WriteLine("(" + collisionInfoForPath[n].correspondingForbiddenPoints[1].X + "," + collisionInfoForPath[n].correspondingForbiddenPoints[1].Y + ")");
                                                
                                                /*get forbidden grid*/
                                                Array.Resize(ref forbidden, (int)((forbiddenEnd[forbiddenEnd.Length - 1][forbiddenEnd[forbiddenEnd.Length - 1].Length - 1].S - forbiddenStart[0][0].S) / interpolationS) + 1);
                                                //STspace[][] forbidden = new STspace[(int)((forbiddenEnd[forbiddenEnd.Length - 1][forbiddenEnd.Length - 1].S - forbiddenStart[0][0].S) / interpolationS) + 1][];
                                                for (int mm = 0; mm < forbidden.Length; mm++)
                                                {
                                                    forbidden[mm] = new STspace[(int)((forbiddenForListOfIntersection[biggest_T][forbiddenForListOfIntersection[biggest_T].Length - 1][forbiddenForListOfIntersection[biggest_T][forbiddenForListOfIntersection[biggest_T].Length - 1].Length - 1].T - forbiddenForListOfIntersection[smallest_T][0][0].T) / interpolationT) + 1];
                                                }


                                                for (int mm = 0; mm < forbidden.Length; mm++)
                                                {
                                                    for (int nn = 0; nn < forbidden[mm].Length; nn++)
                                                    {
                                                        forbidden[mm][nn] = new STspace(0, 0);
                                                    }
                                                }
                                                for (int mm = 0; mm < forbidden.Length; mm++)
                                                {
                                                    for (int nn = 0; nn < forbidden[mm].Length; nn++)
                                                    {
                                                        forbidden[mm][nn].S = forbiddenStart[0][0].S + (Decimal)(mm * interpolationS);
                                                        forbidden[mm][nn].T = forbiddenStart[0][0].T + (Decimal)(nn * interpolationT);  //Math.Min(forbiddenT_start,forbiddenT_End)
                                                        //Console.WriteLine(forbidden[m][n].S + "#" + forbidden[m][n].T);
                                                    }
                                                }
                                                //// Copy the last element exactly from STspace 
                                                //forbidden[forbidden.Length - 1][forbidden[forbidden.Length - 1].Length - 1] = new STspace(forbiddenEnd[forbiddenEnd.Length - 1][forbiddenEnd[forbiddenEnd.Length - 1].Length - 1].S,
                                                //    forbiddenEnd[forbiddenEnd.Length - 1][forbiddenEnd[forbiddenEnd.Length - 1].Length - 1].T);
                                            }
                                            else if (forbiddenForListOfIntersection.Count == 1)
                                            {
                                                for (int c = 0; c < forbiddenForListOfIntersection[0].Length; c++)
                                                {
                                                    Console.WriteLine("C:" + c);
                                                    for (int d = 0; d < forbiddenForListOfIntersection[0][c].Length; d++)
                                                    {
                                                        Console.WriteLine(forbiddenForListOfIntersection[0][c][d].S + " , " + forbiddenForListOfIntersection[0][c][d].T);
                                                    }
                                                }
                                                Array.Resize(ref forbidden, (int)((forbiddenForListOfIntersection[0][forbiddenForListOfIntersection[0].Length - 1][forbiddenForListOfIntersection[0][forbiddenForListOfIntersection[0].Length - 1].Length - 1].S - forbiddenForListOfIntersection[0][0][0].S) / interpolationS) + 1);
                                                for (int mm = 0; mm < forbidden.Length; mm++)
                                                {
                                                    forbidden[mm] = new STspace[(int)((forbiddenForListOfIntersection[0][forbiddenForListOfIntersection[0].Length - 1][forbiddenForListOfIntersection[0][forbiddenForListOfIntersection[0].Length - 1].Length - 1].T - forbiddenForListOfIntersection[0][0][0].T) / interpolationT) + 1];
                                                }
                                                for (int mm = 0; mm < forbidden.Length; mm++)
                                                {
                                                    for (int nn = 0; nn < forbidden[0].Length; nn++)
                                                    {
                                                        forbidden[mm][nn] = new STspace(0, 0);
                                                    }
                                                }
                                                for (int mm = 0; mm < forbidden.Length; mm++)
                                                {
                                                    for (int nn = 0; nn < forbidden[mm].Length; nn++)
                                                    {
                                                        forbidden[mm][nn].S = forbiddenForListOfIntersection[0][0][0].S + (Decimal)(mm * interpolationS);
                                                        forbidden[mm][nn].T = forbiddenForListOfIntersection[0][0][0].T + (Decimal)(nn * interpolationT);
                                                        //Console.WriteLine(forbidden[m][n].S + "#" + forbidden[m][n].T);
                                                    }
                                                }
                                            }

                                            else if (forbiddenForListOfIntersection.Count == 0)
                                                forbidden = null;

                                        }
                                        else
                                        { /*More that one group of intersection*/

                                            flag_more_than_one_segment_of_intersection = true;

                                            for (int t = 0; t < listOfIntersections.Count; t++)
                                            {
                                                if (listOfIntersections[t].Count > 1)
                                                { /*Intersection segment*/
                                                    flag_intersection_segment = true;
                                                    groupsWithIntersectionSegments.Add(t);
                                                }
                                                else
                                                {/*intersection point*/
                                                    if (IsThereAforbiddenPathSegment(listOfIntersections[t][0],allAstarPaths) != null)
                                                        allForbiddenAreasForPath.Add(IsThereAforbiddenPathSegment(listOfIntersections[t][0],allAstarPaths));
                                                }
                                            }
                                            if (groupsWithIntersectionSegments.Count != 0)
                                            {
                                                int index_smallest_S = -1; int index_biggest_S = -1;
                                                for (int tt = 0; tt < groupsWithIntersectionSegments.Count; tt++)
                                                {
                                                    List<STspace[][]> forbiddenForListOfIntersection = new List<STspace[][]>();

                                                    for (int c = 0; c < listOfIntersections[groupsWithIntersectionSegments[tt]].Count; c++)
                                                    {
                                                        if (IsThereAforbiddenPathSegment(listOfIntersections[groupsWithIntersectionSegments[tt]][c],allAstarPaths) != null)
                                                            forbiddenForListOfIntersection.Add(IsThereAforbiddenPathSegment(listOfIntersections[groupsWithIntersectionSegments[tt]][c],allAstarPaths));
                                                    }

                                                    if (forbiddenForListOfIntersection.Count > 1)
                                                    {
                                                        /* Forbidden area with the smallest starting S is forbiddenStart*/
                                                        index_smallest_S = forbiddenForListOfIntersection.IndexOf(forbiddenForListOfIntersection.First(b => b[0][0].S == forbiddenForListOfIntersection.Min(e => e[0][0].S)));

                                                        /* Forbidden area with the biggest finishing S is forbiddenStart*/
                                                        index_biggest_S = forbiddenForListOfIntersection.IndexOf(forbiddenForListOfIntersection.First(b => b[b.Length - 1][b[b.Length - 1].Length - 1].S == forbiddenForListOfIntersection.Max(e => e[e.Length - 1][e[e.Length - 1].Length - 1].S)));


                                                        STspace[][] forbiddenStart = IsThereAforbiddenPathSegment(listOfIntersections[groupsWithIntersectionSegments[tt]][index_smallest_S],allAstarPaths);
                                                        STspace[][] forbiddenEnd = IsThereAforbiddenPathSegment(listOfIntersections[groupsWithIntersectionSegments[tt]][index_biggest_S],allAstarPaths);

                                                        Array.Resize(ref forbidden, (int)((forbiddenEnd[forbiddenEnd.Length - 1][forbiddenEnd[forbiddenEnd.Length - 1].Length - 1].S - forbiddenStart[0][0].S) / interpolationS) + 1);
                                                        for (int mm = 0; mm < forbidden.Length; mm++)
                                                        {
                                                            forbidden[mm] = new STspace[(int)((forbiddenEnd[forbiddenEnd.Length - 1][forbiddenEnd[forbiddenEnd.Length - 1].Length - 1].T - forbiddenStart[0][0].T) / interpolationT) + 1];
                                                        }


                                                        for (int mm = 0; mm < forbidden.Length; mm++)
                                                        {
                                                            for (int nn = 0; nn < forbidden[0].Length; nn++)
                                                            {
                                                                forbidden[mm][nn] = new STspace(0, 0);
                                                            }
                                                        }
                                                        for (int mm = 0; mm < forbidden.Length; mm++)
                                                        {
                                                            for (int nn = 0; nn < forbidden[mm].Length; nn++)
                                                            {
                                                                forbidden[mm][nn].S = forbiddenStart[0][0].S + (Decimal)(mm * interpolationS);
                                                                forbidden[mm][nn].T = forbiddenStart[0][0].T + (Decimal)(nn * interpolationT);
                                                                //Console.WriteLine(forbidden[m][n].S + "#" + forbidden[m][n].T);
                                                            }
                                                        }
                                                        //// Copy the last element exactly from STspace 
                                                        //forbidden[forbidden.Length - 1][forbidden[forbidden.Length - 1].Length - 1] = new STspace(forbiddenEnd[forbiddenStart.Length - 1][forbiddenStart[forbiddenStart.Length - 1].Length - 1].S,
                                                        //    forbiddenEnd[forbiddenStart.Length - 1][forbiddenStart.Length - 1].T);
                                                    }
                                                    else if (forbiddenForListOfIntersection.Count == 0)
                                                    {
                                                        forbidden = null;
                                                    }
                                                    else if (forbiddenForListOfIntersection.Count == 1)
                                                    {

                                                        /* ForbiddenEnd is not null */
                                                        Array.Resize(ref forbidden, (int)((forbiddenForListOfIntersection[0][forbiddenForListOfIntersection[0].Length - 1][forbiddenForListOfIntersection[0][forbiddenForListOfIntersection[0].Length - 1].Length - 1].S - forbiddenForListOfIntersection[0][0][0].S) / interpolationS) + 1);
                                                        for (int mm = 0; mm < forbidden.Length; mm++)
                                                        {
                                                            forbidden[mm] = new STspace[(int)((forbiddenForListOfIntersection[0][forbiddenForListOfIntersection[0].Length - 1][forbiddenForListOfIntersection[0][forbiddenForListOfIntersection[0].Length - 1].Length - 1].T - forbiddenForListOfIntersection[0][0][0].T) / interpolationT) + 1];
                                                        }
                                                        for (int mm = 0; mm < forbidden.Length; mm++)
                                                        {
                                                            for (int nn = 0; nn < forbidden[0].Length; nn++)
                                                            {
                                                                forbidden[mm][nn] = new STspace(0, 0);
                                                            }
                                                        }
                                                        for (int mm = 0; mm < forbidden.Length; mm++)
                                                        {
                                                            for (int nn = 0; nn < forbidden[mm].Length; nn++)
                                                            {
                                                                forbidden[mm][nn].S = forbiddenForListOfIntersection[0][0][0].S + (Decimal)(mm * interpolationS);
                                                                forbidden[mm][nn].T = forbiddenForListOfIntersection[0][0][0].T + (Decimal)(nn * interpolationT);
                                                                //Console.WriteLine(forbidden[m][n].S + "#" + forbidden[m][n].T);
                                                            }
                                                        }
                                                    }

                                                    if (forbidden != null)
                                                        allForbiddenAreasForPath.Add(forbidden);
                                                }
                                            }
                                            else
                                            {
                                                /*No intersetion segment Only a group of point intersections*/
                                            }

                                        }
                                    }
                                    else if (intrsct.Count == 1)
                                    {
                                        forbidden = IsThereAforbiddenPathSegment(intrsct[0],allAstarPaths);
                                    }

                                    if (n == index_path_lower_priority)//(n == 0)
                                    {
                                        if (flag_more_than_one_segment_of_intersection)
                                        {
                                            if (allForbiddenAreasForPath.Count != 0)//forbidden != null)
                                            {
                                                collisionInfoForPath[n].forbiddenAreas = new List<STspace[][]>();
                                                for (int p = 0; p < allForbiddenAreas.Count; p++)
                                                    allForbiddenAreas.Add(allForbiddenAreas[p]);

                                                collisionInfoForPath[n].forbiddenAreas = allForbiddenAreasForPath;
                                                STspace lastElement = allForbiddenAreasForPath[allForbiddenAreasForPath.Count - 1][allForbiddenAreasForPath[allForbiddenAreasForPath.Count - 1].Length - 1][allForbiddenAreasForPath[allForbiddenAreasForPath[allForbiddenAreasForPath.Count - 1].Length - 1].Length - 1];

                                                //collisionInfoForPath[n].additionalTimeSteps += (int)((lastElement.T - allForbiddenAreasForPath[0][0][0].T) / interpolationT);
                                                //collisionInfoForPath[n].totalAdditionalTime += lastElement.T - allForbiddenAreasForPath[0][0][0].T;

                                                collisionInfoForPath[n].numberOfCollisions++;
                                                //collisionInfoForPath[n].flagNoCollision = false;
                                            }

                                            else
                                            {
                                                //collisionInfoForPath[n].flagNoCollision = true;
                                                //collisionInfoForPath[n].updatedPathST = allSTspaces[n];
                                                //collisionInfoForPath[n].updatedPosition = allBsplines[n];
                                            }
                                        }
                                        else if (flag_one_segment_of_intersection)
                                        {
                                            if (forbidden != null)
                                            {
                                                
                                                allForbiddenAreas.Add(forbidden);

                                                collisionInfoForPath[n].forbiddenAreas = new List<STspace[][]>();
                                                
                                                collisionInfoForPath[n].forbiddenAreas.Add(forbidden);

                                                //int length_1 = collisionInfoForPath[n].forbiddenAreas.Count;
                                                //int length_2 = collisionInfoForPath[n].forbiddenAreas[length_1 - 1].Length;
                                                //int length_3 = collisionInfoForPath[n].forbiddenAreas[length_1 - 1][length_2 - 1].Length;
                                                //decimal[] times = new decimal[length_3];

                                                //Console.WriteLine("ForbiddenAreas :");
                                                //for (int k = 0; k < collisionInfoForPath[n].forbiddenAreas.Count; k++)
                                                //    for (int l = 0; l < collisionInfoForPath[n].forbiddenAreas[k].Length; l++)
                                                //        for (int p = 0; p < collisionInfoForPath[n].forbiddenAreas[k][l].Length; p++)
                                                //        {
                                                //            {
                                                //                {
                                                //                    times[p] = collisionInfoForPath[n].forbiddenAreas[k][l][p].T;
                                                //                    Console.WriteLine(collisionInfoForPath[n].forbiddenAreas[k][l][p].S + "\t" + collisionInfoForPath[n].forbiddenAreas[k][l][p].T);
                                                //                }
                                                //            }
                                                //        }
                                               
                                                decimal timeEndForbidden = getLeavingTime();


                                                collisionInfoForPath[n].additionalTimeSteps += (int)((timeEndForbidden - forbidden[0][0].T) / interpolationT) + 1;//(int)((forbidden[forbidden.Length - 1][forbidden[forbidden.Length - 1].Length - 1].T - forbidden[0][0].T) / interpolationT);
                                                collisionInfoForPath[n].totalAdditionalTime += timeEndForbidden - forbidden[0][0].T;//times.Min();//forbidden[forbidden.Length - 1][forbidden[forbidden.Length - 1].Length - 1].T - forbidden[0][0].T;

                                                collisionInfoForPath[n].numberOfCollisions++;
                                                //collisionInfoForPath[n].flagNoCollision = false;
                                            }

                                            else
                                            {
                                                //collisionInfoForPath[n].flagNoCollision = true;
                                                //collisionInfoForPath[n].updatedPathST = allSTspaces[n];
                                                //collisionInfoForPath[n].updatedPosition = allBsplines[n];
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //collisionInfoForPath[n].flagNoCollision = true;
                                    }
                                }
                            }
                        }
                    }
                    
                        /* Update ST and Positions*/

                        if (collisionInfoForPath[n].forbiddenAreas == null)
                        {
                            collisionInfoForPath[n].updatedPathST = allSTspaces[n];
                            collisionInfoForPath[n].updatedPosition = allBsplines[n];
                        }
                        //else if (collisionInfoForPath[n].forbiddenAreas.Count == 0)
                        //{
                        //    collisionInfoForPath[n].updatedPathST = allSTspaces[n];
                        //    collisionInfoForPath[n].updatedPosition = allBsplines[n];
                        //}
                        else
                        {
                            int[] index_start_forbidden = new int[collisionInfoForPath[n].forbiddenAreas.Count];
                            int[] index_end_forbidden = new int[collisionInfoForPath[n].forbiddenAreas.Count];
                            List<STspace[][]> forbidden = collisionInfoForPath[n].forbiddenAreas;
                            int additionalTimeSteps = 0; // index_end_forbidden - index_start_forbidden;

                            for (int a = 0; a < forbidden.Count; a++)
                            {
                                STspace[][] forbiddenElement = forbidden[a];
                      
                                for (int b = 0; b < allSTspaces[n].Length; b++)
                                {
                                    if ((allSTspaces[n][b].S == forbiddenElement[0][0].S)) //&& (allSTspaces[n][b].T == forbiddenElement[0][0].T))
                                    {
                                        Console.WriteLine("forbiddenT start : " + forbiddenElement[0][0].T);
                                        index_start_forbidden[a] = b;
                                        break;
                                    }
                                }

                                for (int b = index_start_forbidden[a]; b < allSTspaces[n].Length; b++)
                                {
                                    if (allSTspaces[n][b].S == forbiddenElement[forbiddenElement.Length - 1][forbiddenElement[forbiddenElement.Length - 1].Length - 1].S) //&&
                                    //(allSTspaces[n][b].S == forbiddenElement[forbiddenElement.Length - 1][forbiddenElement[forbiddenElement.Length - 1].Length - 1].S))
                                    {
                                        Console.WriteLine("forbiddenT end : " + forbiddenElement[forbiddenElement.Length - 1][forbiddenElement[forbiddenElement.Length - 1].Length - 1].T);
                                        index_end_forbidden[a] = b;
                                        break;
                                    }
                                }
                                additionalTimeSteps += index_end_forbidden[a] - index_start_forbidden[a];
                            }

                            Array.Sort(index_start_forbidden);
                            Array.Sort(index_end_forbidden);

                            STspace[] stSpaceForPath = new STspace[allSTspaces[n].Length + additionalTimeSteps];
                            Position[] updatedPosition = new Position[allSTspaces[n].Length + additionalTimeSteps];

                            for (int k = 0; k < forbidden.Count; k++)
                            {
                                for (int a = 0; a <= index_end_forbidden[k]; a++)
                                {
                                    //for (int k = 0; k < forbidden.Count; k++)
                                    //{
                                    int index = index_start_forbidden[k];
                                    if (k == 0)
                                    {
                                        if (a < index_start_forbidden[k])
                                        {
                                            stSpaceForPath[a] = new STspace(allSTspaces[n][a].S, allSTspaces[n][a].T);
                                            updatedPosition[a] = new Position(allBsplines[n][a].X, allBsplines[n][a].Y);
                                        }

                                        else if (a == index_start_forbidden[k])
                                        {
                                            stSpaceForPath[a] = new STspace(allSTspaces[n][index_start_forbidden[k]].S, allSTspaces[n][index_start_forbidden[k]].T);
                                            updatedPosition[a] = new Position(allBsplines[n][index_start_forbidden[k]].X, allBsplines[n][index_start_forbidden[k]].Y);
                                        }

                                        else if (a > index_start_forbidden[k] && a <= index_end_forbidden[k])
                                        {
                                            Console.WriteLine("index_end_forbidden[" + k + "] = " + index_end_forbidden[k]);
                                            stSpaceForPath[a] = new STspace(allSTspaces[n][index_start_forbidden[k]].S, allSTspaces[n][a].T);
                                            updatedPosition[a] = new Position(allBsplines[n][index_start_forbidden[k]].X, allBsplines[n][index_start_forbidden[k]].Y);
                                        }
                                    }
                                    if (k != 0)
                                    {
                                        index = index_start_forbidden[k - 1];
                                        index++;
                                        Console.WriteLine("index_end_forbidden[" + (k - 1) + "] = " + index_end_forbidden[k - 1]);
                                        if (a < index_start_forbidden[k])
                                        {
                                            stSpaceForPath[a + index_end_forbidden[k - 1] + 1] = new STspace(allSTspaces[n][index].S, allSTspaces[n][a + index].T);
                                            updatedPosition[a + index_end_forbidden[k - 1] + 1] = new Position(allBsplines[n][index].X, allBsplines[n][index].Y);
                                        }
                                        else if (a == index_start_forbidden[k])
                                        {
                                            stSpaceForPath[a + index_end_forbidden[k - 1] + 1] = new STspace(allSTspaces[n][index_start_forbidden[k]].S, allSTspaces[n][index_start_forbidden[k]].T);
                                            updatedPosition[a + index_end_forbidden[k - 1] + 1] = new Position(allBsplines[n][index_start_forbidden[k]].X, allBsplines[n][index_start_forbidden[k]].Y);
                                        }

                                        else if (a > index_start_forbidden[k] && a <= index_end_forbidden[k])
                                        {
                                            stSpaceForPath[a + index_end_forbidden[k - 1] + 1] = new STspace(allSTspaces[n][index_start_forbidden[k]].S, allSTspaces[n][a + index].T);
                                            updatedPosition[a + index_end_forbidden[k - 1] + 1] = new Position(allBsplines[n][index_start_forbidden[k]].X, allBsplines[n][index_start_forbidden[k]].Y);
                                        }

                                        if (k == forbidden.Count - 1 && a + index_end_forbidden[k - 1] == index_end_forbidden[k])
                                        {
                                            for (int b = index_end_forbidden[k]; b < stSpaceForPath.Length; b++)
                                            {
                                                index++;
                                                stSpaceForPath[b] = new STspace(allSTspaces[n][b].S, (allSTspaces[n][allSTspaces[n].Length - 1].T + b * interpolationT));
                                                updatedPosition[b] = new Position(allBsplines[n][b].X, allBsplines[n][b].Y);
                                                //a++;
                                            }
                                        }
                                    }
                                }
                            }
                            //}
                            collisionInfoForPath[n].updatedPathST = stSpaceForPath;
                            collisionInfoForPath[n].updatedPosition = updatedPosition;
                        }
                    }
                }
            //}
            
            /* Get Velocity Profiles */
            for (int i = 0; i < collisionInfoForPath.Length; i++)
            {
                if (collisionInfoForPath[i].forbiddenAreas != null)
                {
                    
                    //int length_1 = collisionInfoForPath[i].forbiddenAreas.Count;
                    //int length_2 = collisionInfoForPath[i].forbiddenAreas[length_1-1].Length;
                    //int length_3 = collisionInfoForPath[i].forbiddenAreas[length_1-1][length_2-1].Length;
                    //decimal[] times = new decimal[length_3];

                    //Console.WriteLine("ForbiddenAreas :");
                    //for (int k = 0; k < collisionInfoForPath[i].forbiddenAreas.Count; k++)
                    //    for (int l = 0; l < collisionInfoForPath[i].forbiddenAreas[k].Length; l++)
                    //        for (int p = 0; p < collisionInfoForPath[i].forbiddenAreas[k][l].Length; p++)
                    //    {
                    //        {
                    //            {
                    //                times[p] = collisionInfoForPath[i].forbiddenAreas[k][l][p].T;
                    //                Console.WriteLine(collisionInfoForPath[i].forbiddenAreas[k][l][p].S + "\t" + collisionInfoForPath[i].forbiddenAreas[k][l][p].T);
                    //            }
                    //        }
                    //    }
                    //Console.WriteLine("Maximum time : " + times.Max());
                    //Console.WriteLine("Minimum time : " + times.Min());
                    // interpolationS = 2M;
                    //interpolationT = interpolationS * (allSTspaces[collisionInfoForPath[i].pathIndex][allSTspaces[collisionInfoForPath[i].pathIndex].Length - 1].T - allSTspaces[collisionInfoForPath[i].pathIndex][0].T)
                    //                                / (allSTspaces[collisionInfoForPath[i].pathIndex][allSTspaces[collisionInfoForPath[i].pathIndex].Length - 1].S - allSTspaces[collisionInfoForPath[i].pathIndex][0].S);

                    //collisionInfoForPath[i].additionalTimeSteps += (int)(((times.Max() - times.Min()) / interpolationT)+1);
                    //collisionInfoForPath[i].totalAdditionalTime += times.Max() - times.Min();
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    StreamWriter timeAndVel = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "time.log", false);    
                    List<STspace> route = stAstar.calculateRoute(allSTspaces[collisionInfoForPath[i].pathIndex][0], allSTspaces[collisionInfoForPath[i].pathIndex][allSTspaces[collisionInfoForPath[i].pathIndex].Length - 1],
                                                                     collisionInfoForPath[i].forbiddenAreas, 50, collisionInfoForPath[i].totalAdditionalTime);//, totalForbiddenDuration);
                    sw.Stop();
                    Console.WriteLine(sw.ElapsedMilliseconds);
                        decimal[,] velocityAndTime = stAstar.getVelocityProfiles(route);
                        for (int p = 0; p < velocityAndTime.GetLength(0); p++)
                        {
                            timeAndVel.WriteLine(velocityAndTime[p, 0] + "\t" + velocityAndTime[p, 1]);
                        }
                        timeAndVel.Flush(); timeAndVel.Close();

                        //allVelocityProfiles.Add(velocityAndTime);    
                        //allVelocityProfiles[i, 0] = velocityAndTime[0, 0];
                        //allVelocityProfiles[i, 1] = velocityAndTime[0, 1];
                        Console.WriteLine("velocityAndTime length :" + velocityAndTime.GetLength(0));
                        Console.WriteLine("Path finishing time: " + velocityAndTime[velocityAndTime.GetLength(0)-1, 1]);
                        //pathFinishingTimes[i] = velocityAndTime[velocityAndTime.GetLength(0) - 1, 1];
                        collisionInfoForPath[i].pathFinishingTime = velocityAndTime[velocityAndTime.GetLength(0) - 1, 1];
                        collisionInfoForPath[i].velocityProfile = velocityAndTime;
                                     
                }
                else
                {
                    interpolationS = 2M;
                    interpolationT = interpolationS * (allSTspaces[collisionInfoForPath[i].pathIndex][allSTspaces[collisionInfoForPath[i].pathIndex].Length - 1].T - allSTspaces[collisionInfoForPath[i].pathIndex][0].T)
                                                    / (allSTspaces[collisionInfoForPath[i].pathIndex][allSTspaces[collisionInfoForPath[i].pathIndex].Length - 1].S - allSTspaces[collisionInfoForPath[i].pathIndex][0].S);

                    int dim = (int)((allSTspaces[collisionInfoForPath[i].pathIndex][allSTspaces[collisionInfoForPath[i].pathIndex].Length - 1].T - allSTspaces[collisionInfoForPath[i].pathIndex][0].T) / interpolationT);

                    decimal[,] timeAndVelocity = new decimal[dim, 2];

                    for (int p = 0; p < timeAndVelocity.GetLength(0); p++)
                    {
                        timeAndVelocity[p, 0] = allSTspaces[collisionInfoForPath[i].pathIndex][0].T + (p * interpolationT);
                        timeAndVelocity[p, 1] = vp.getMaxVelocity(); //velocityMax;
                    }

                    //velocityAndTime[0, 0] = velocity;
                    //velocityAndTime[0, 1] = time;

                    //allVelocityProfiles.Add(velocityAndTime);
                    //pathFinishingTimes[i] = timeAndVelocity[timeAndVelocity.GetLength(0) - 1, 0];
                    collisionInfoForPath[i].pathFinishingTime = timeAndVelocity[timeAndVelocity.GetLength(0) - 1, 0];
                    collisionInfoForPath[i].velocityProfile = timeAndVelocity;

                }
               
            }
           
            

            /* Check if finishing times differ from the ones in the Schedule */

            for (int i = 0; i < collisionInfoForPath.Length; i++)
            {
                Console.WriteLine("original finishing time for taskID: " + collisionInfoForPath[i].taskID + "\t" + AGVmovementInfo[i].endTime);//(startTimes[i] + pathDuration[i]));
                if (collisionInfoForPath[i].pathFinishingTime > AGVmovementInfo[i].endTime) //(startTimes[i] + pathDuration[i])
                {
                    Console.WriteLine("End Time: " + AGVmovementInfo[i].endTime);
                    Console.WriteLine("Path Duration: " + pathDuration[i]);
                    Console.WriteLine("start Time: " + startTimes[i]);
                    flag_rescheduling_needed = true;
                    taskIDsWithNewDuration.Add(collisionInfoForPath[i].taskID);
                    
                    ///*Plot Velocity Profile*/
                    //decimal[] time = new decimal[collisionInfoForPath[i].velocityProfile.GetLength(0)];
                    //decimal[] velocity = new decimal[collisionInfoForPath[i].velocityProfile.GetLength(0)];
                    //StreamWriter time_str_wrt = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\time.log", false);
                    //StreamWriter position_wrt = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\position.log", false);
                    
                    //for (int k = 0; k < collisionInfoForPath[i].velocityProfile.GetLength(0); k++)
                    //{
                    //    time[k]     = collisionInfoForPath[i].velocityProfile[k,1];
                    //    velocity[k] = collisionInfoForPath[i].velocityProfile[k,0];
                    //    time_str_wrt.WriteLine(time[k] + "\t" + velocity[k]);
                    //}
                    //time_str_wrt.Flush(); time_str_wrt.Close();
                    //for (int j = 0; j < collisionInfoForPath[i].updatedPosition.Length; j++)
                    //{
                    //    position_wrt.WriteLine(collisionInfoForPath[i].updatedPosition[j].X + "\t" + collisionInfoForPath[i].updatedPosition[j].Y);
                    //}

                    //position_wrt.Flush(); position_wrt.Close();
                    //plot.PlotSplineSolution("", time, velocity, AppDomain.CurrentDomain.BaseDirectory + "plot.png");
                }
            }
            return collisionInfoForPath;

        }

        public bool isReschedulingNeeded()
        {
            return flag_rescheduling_needed;
        }

        public List<int> tasksWithNewDuration()
        {
            return taskIDsWithNewDuration;
        }

        public List<IntersectionPoint> checkCollisionInSpace(List<Position> Path1, List<Position> Path2, int[] pathIndexes)
        {
            List<IntersectionPoint> intersections = new List<IntersectionPoint>();
            //Console.WriteLine("Path1 start: (" + Path1[0].X + "," + Path1[0].Y + ")");
            //Console.WriteLine("Path1 end: (" + Path1[Path1.Count - 1].X + "," + Path1[Path1.Count - 1].Y + ")");

            //Console.WriteLine("\n");
            //Console.WriteLine("Path2 start: (" + Path2[0].X + "," + Path2[0].Y + ")");
            //Console.WriteLine("Path2 end: (" + Path2[Path2.Count - 1].X + "," + Path2[Path2.Count - 1].Y + ")");



            for (int j = 0; j < Path1.Count; j++)   // the intersection between start of one path and end of the other path is not considered as an intersection.
                                                      // ex. for INIT2-STO and STO-COL2  , STO is not considered as a collision point. 
            {
                //Console.WriteLine(Path1[j].X + "\t" + Path1[j].Y);
                for (int k = 0; k < Path2.Count; k++)
                {
                    //Console.WriteLine(Path2[k].X + "\t" + Path2[k].Y);

                    if (Path1[j].X == Path2[k].X && Path1[j].Y == Path2[k].Y)
                    {
                        if ((j == 0 && k == Path2.Count - 1) || (k == 0 && j == Path1.Count - 1))
                            continue;
                        Position intrsct = new Position(Path1[j].X, Path1[j].Y);
                        List<Position> additionalPoints = new List<Position>();

                        //**Choose Additional Points**//
                        if (j + 1 != Path1.Count && k + 1 != Path2.Count)
                        {
                            additionalPoints.Add(new Position(Path2[k + 1].X, Path2[k + 1].Y));
                            additionalPoints.Add(new Position(Path1[j + 1].X, Path1[j + 1].Y));
                        }
                        else if (j == 0 && k + 1 == Path2.Count)
                        {
                            additionalPoints.Add(new Position(Path2[k - 1].X, Path2[k - 1].Y));
                            additionalPoints.Add(new Position(Path1[j + 1].X, Path1[j + 1].Y));
                        }
                        else if (k == 0 && j + 1 == Path1.Count)
                        {
                            additionalPoints.Add(new Position(Path2[k + 1].X, Path2[k + 1].Y));
                            additionalPoints.Add(new Position(Path1[j - 1].X, Path1[j - 1].Y));
                        }
                        //**End**//
                        intersections.Add(new IntersectionPoint(intrsct, pathIndexes[0], pathIndexes[1], priority, additionalPoints));
                    }
                }
            }
            return intersections;
        }

        public STspace[][] IsThereAforbiddenPathSegment(IntersectionPoint intersection, List<List<Position>> allPaths) //STspace[][] return type
        {
            
            Console.WriteLine("intersection point = ( " + intersection.intersectPoint.X + " , " + intersection.intersectPoint.Y + ")");
            // get the duration at which the robot with the higher priority touches the path with Lower priority and leaves it 
            Point forbidden1; Point forbidden2;
            Point forbidden1_forT = new Point(0,0); Point forbidden2_forT = new Point(0,0);
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


            List<Position> pathLowerPriority = new List<Position>();
            List<Position> pathHigherPriority = new List<Position>();

            List<float> distances = new List<float>();
            List<Point> PointsOutsideCollisionRadius = new List<Point>();

            //for (int i = 0; i < intersections.Count; i++)
            //{
            Console.WriteLine("Path with Lower Priority Starting point = (" + allPaths[intersection.indexPath1][0].X + " , " +allPaths[intersection.indexPath1][0].Y +  ")");
            Console.WriteLine("Path with Higher Priority Starting point = (" + allPaths[intersection.indexPath2][0].X + " , " + allPaths[intersection.indexPath2][0].Y + ")");
            pathLowerPriority = allPaths[intersection.indexPath1];
                pathHigherPriority = allPaths[intersection.indexPath2];

                interpolationS = 2M;
                interpolationT = interpolationS * (allSTspaces[intersection.indexPath1][allSTspaces[intersection.indexPath1].Length - 1].T - allSTspaces[intersection.indexPath1][0].T)
                                                / (allSTspaces[intersection.indexPath1][allSTspaces[intersection.indexPath1].Length - 1].S - allSTspaces[intersection.indexPath1][0].S);

                /**Calculating S Forbidden**/

                for (int j = 0; j < pathLowerPriority.Count; j++)
                {
                    if (((float)stSpace.dist(pathLowerPriority[j], intersection.intersectPoint) > (float)(AGVDiameter + safetyDistance))) // / 2)))
                    {// point on the path is outside the circle 

                        possibleForbiddenPointIndexes.Add(j);

                        //Console.WriteLine(Math.Sign((intersection.additionalPoints[0].X - intersection.intersectPoint.X) * (pathLowerPriority[j].Y - intersection.intersectPoint.Y) - (intersection.additionalPoints[0].Y - intersection.intersectPoint.X) * (pathLowerPriority[j].X - intersection.intersectPoint.X)));

                        //if (Math.Sign((intersection.additionalPoints[0].X - intersection.intersectPoint.X) * (pathLowerPriority[j].Y - intersection.intersectPoint.Y) - (intersection.additionalPoints[0].Y - intersection.intersectPoint.X) * (pathLowerPriority[j].X - intersection.intersectPoint.X)) != 0)
                        //{
                        //    if (Math.Sign((intersection.additionalPoints[0].X - intersection.intersectPoint.X) * (pathLowerPriority[j].Y - intersection.intersectPoint.Y) - (intersection.additionalPoints[0].Y - intersection.intersectPoint.X) * (pathLowerPriority[j].X - intersection.intersectPoint.X)) > 0 )//(path[i].X > intersectionPoint.X)
                        //    {
                        //        RHSPoints_forS.Add((float)stSpace.dist(pathLowerPriority[j], intersection.intersectPoint));
                        //        RHSIndexes_forS.Add(j);
                        //    }
                        //    else if (Math.Sign((intersection.additionalPoints[0].X - intersection.intersectPoint.X) * (pathLowerPriority[j].Y - intersection.intersectPoint.Y) - (intersection.additionalPoints[0].Y - intersection.intersectPoint.X) * (pathLowerPriority[j].X - intersection.intersectPoint.X)) < 0)
                        //    {
                        //        LHSPoints_forS.Add((float)stSpace.dist(pathLowerPriority[j], intersection.intersectPoint));
                        //        LHSIndexes_forS.Add(j);
                        //    }
                        //}

                        //else if (Math.Sign((intersection.additionalPoints[0].X - intersection.intersectPoint.X) * (pathLowerPriority[j].Y - intersection.intersectPoint.Y) - (intersection.additionalPoints[0].Y - intersection.intersectPoint.X) * (pathLowerPriority[j].X - intersection.intersectPoint.X)) == 0)
                        //{
                            if (Math.Atan2(pathLowerPriority[j].Y - intersection.intersectPoint.Y, pathLowerPriority[j].X - intersection.intersectPoint.X) > 0)
                            {
                                RHSPoints_forS.Add((float)stSpace.dist(pathLowerPriority[j], intersection.intersectPoint));
                                RHSIndexes_forS.Add(j);
                                //LHSPoints_forS.Add((float)stSpace.dist(pathLowerPriority[j], intersection.intersectPoint));
                                //LHSIndexes_forS.Add(j);
                            }
                            else if (Math.Atan2(pathLowerPriority[j].Y - intersection.intersectPoint.Y, pathLowerPriority[j].X - intersection.intersectPoint.X) <= 0)
                            {
                                LHSPoints_forS.Add((float)stSpace.dist(pathLowerPriority[j], intersection.intersectPoint));
                                LHSIndexes_forS.Add(j);
                                //RHSPoints_forS.Add((float)stSpace.dist(pathLowerPriority[j], intersection.intersectPoint));
                                //RHSIndexes_forS.Add(j);
                            }
                        //}
                        distances.Add((float)stSpace.dist(pathLowerPriority[j], intersection.intersectPoint));
                    }
                }

                //if (RHSPoints_forS.Count != 0 && LHSPoints_forS.Count!=0)
                //{
                int forbiddenindex1 = -1, forbiddenindex2 = -1, indexStart = -1, indexEnd = -1;
                bool flag_RHSpoints_null = false, flag_LHSpoints_null = false;
                if (RHSPoints_forS.Count != 0)
                {
                    forbiddenindex1 = Array.IndexOf(RHSPoints_forS.ToArray(), RHSPoints_forS.Min(element => Math.Abs(element)));
                    forbidden1 = new Point(pathLowerPriority[RHSIndexes_forS[forbiddenindex1]].X, pathLowerPriority[RHSIndexes_forS[forbiddenindex1]].Y);
                }
                else
                {
                    flag_RHSpoints_null = true;
                    Console.WriteLine((float)stSpace.dist(pathLowerPriority[pathLowerPriority.Count - 1], intersection.intersectPoint));
                    Console.WriteLine((float)stSpace.dist(pathLowerPriority[0], intersection.intersectPoint));

                    if ((float)stSpace.dist(pathLowerPriority[pathLowerPriority.Count - 1], intersection.intersectPoint) <= (float)stSpace.dist(pathLowerPriority[0], intersection.intersectPoint))
                    {
                        forbiddenindex1 = pathLowerPriority.Count - 1;
                        forbidden1 = new Point(pathLowerPriority[pathLowerPriority.Count - 1].X, pathLowerPriority[pathLowerPriority.Count - 1].Y);
                    }

                    else if ((float)stSpace.dist(pathLowerPriority[0], intersection.intersectPoint) <= (float)stSpace.dist(pathLowerPriority[pathLowerPriority.Count - 1], intersection.intersectPoint))
                    {
                        forbiddenindex1 = 0;
                        forbidden1 = new Point(pathLowerPriority[0].X, pathLowerPriority[0].Y);
                    }
                }

                if (LHSPoints_forS.Count != 0)
                {
                    forbiddenindex2 = Array.IndexOf(LHSPoints_forS.ToArray(), LHSPoints_forS.Min(element => Math.Abs(element)));
                    forbidden2 = new Point(pathLowerPriority[LHSIndexes_forS[forbiddenindex2]].X, pathLowerPriority[LHSIndexes_forS[forbiddenindex2]].Y);
                }
                else
                {
                    flag_LHSpoints_null = true;
                    if ((float)stSpace.dist(pathLowerPriority[pathLowerPriority.Count - 1], intersection.intersectPoint) <= (float)stSpace.dist(pathLowerPriority[0], intersection.intersectPoint))
                    {
                        forbiddenindex2 = pathLowerPriority.Count - 1;
                        forbidden2 = new Point(pathLowerPriority[pathLowerPriority.Count - 1].X, pathLowerPriority[pathLowerPriority.Count - 1].Y);
                    }

                    else if ((float)stSpace.dist(pathLowerPriority[0], intersection.intersectPoint) <= (float)stSpace.dist(pathLowerPriority[pathLowerPriority.Count - 1], intersection.intersectPoint))
                    {
                        forbiddenindex2 = 0;
                        forbidden2 = new Point(pathLowerPriority[0].X, pathLowerPriority[0].Y);
                    }
                }

                if (!flag_LHSpoints_null && !flag_RHSpoints_null)
                {
                    indexStart = Math.Min(RHSIndexes_forS[forbiddenindex1], LHSIndexes_forS[forbiddenindex2]);
                    indexEnd = Math.Max(RHSIndexes_forS[forbiddenindex1], LHSIndexes_forS[forbiddenindex2]);
                }
                else if (flag_RHSpoints_null)
                {
                    indexStart = Math.Min(forbiddenindex1, LHSIndexes_forS[forbiddenindex2]);
                    indexEnd = Math.Max(forbiddenindex1, LHSIndexes_forS[forbiddenindex2]);
                }
                else if (flag_LHSpoints_null)
                {
                    indexStart = Math.Min(RHSIndexes_forS[forbiddenindex1], forbiddenindex2);
                    indexEnd = Math.Max(RHSIndexes_forS[forbiddenindex1], forbiddenindex2);
                }

                Decimal[] forbiddenS = new Decimal[] { allSTspaces[intersection.indexPath1][indexStart].S, allSTspaces[intersection.indexPath1][indexEnd].S };
                Decimal[] correspondingT = new Decimal[] { allSTspaces[intersection.indexPath1][indexStart].T, allSTspaces[intersection.indexPath1][indexEnd].T };

                /**Calculating T forbidden**/

                for (int k = 0; k < pathHigherPriority.Count; k++)
                {
                    if (((float)stSpace.dist(pathHigherPriority[k], intersection.intersectPoint) >= (float)(AGVDiameter + safetyDistance))) // / 2)))
                    {// point on the path is outside the circle 

                        //Console.WriteLine(Math.Sign((intersection.additionalPoints[1].X - intersection.intersectPoint.X) * (pathHigherPriority[k].Y - intersection.intersectPoint.Y) - (intersection.additionalPoints[1].Y - intersection.intersectPoint.X) * (pathHigherPriority[k].X - intersection.intersectPoint.X)));
                        //if (Math.Sign((intersection.additionalPoints[0].X - intersection.intersectPoint.X) * (pathHigherPriority[k].Y - intersection.intersectPoint.Y) - (intersection.additionalPoints[0].Y - intersection.intersectPoint.X) * (pathHigherPriority[k].X - intersection.intersectPoint.X)) != 0)
                        //{
                        //    if (Math.Sign((intersection.additionalPoints[1].X - intersection.intersectPoint.X) * (pathHigherPriority[k].Y - intersection.intersectPoint.Y) - (intersection.additionalPoints[1].Y - intersection.intersectPoint.X) * (pathHigherPriority[k].X - intersection.intersectPoint.X)) > 0)//(path[i].X > intersectionPoint.X)
                        //    {
                        //        RHSPoints_forT.Add((float)stSpace.dist(pathHigherPriority[k], intersection.intersectPoint));
                        //        RHSIndexes_forT.Add(k);
                        //    }
                        //    else if (Math.Sign((intersection.additionalPoints[1].X - intersection.intersectPoint.X) * (pathHigherPriority[k].Y - intersection.intersectPoint.Y) - (intersection.additionalPoints[1].Y - intersection.intersectPoint.X) * (pathHigherPriority[k].X - intersection.intersectPoint.X)) <= 0)
                        //    {
                        //        LHSPoints_forT.Add((float)stSpace.dist(pathHigherPriority[k], intersection.intersectPoint));
                        //        LHSIndexes_forT.Add(k);
                        //    }
                        //}
                        //else if (Math.Sign((intersection.additionalPoints[0].X - intersection.intersectPoint.X) * (pathHigherPriority[k].Y - intersection.intersectPoint.Y) - (intersection.additionalPoints[0].Y - intersection.intersectPoint.X) * (pathHigherPriority[k].X - intersection.intersectPoint.X)) == 0)
                        //{
                            if (Math.Atan2(pathHigherPriority[k].Y - intersection.intersectPoint.Y, pathHigherPriority[k].X - intersection.intersectPoint.X) <= 0)
                            {
                                //RHSPoints_forT.Add((float)stSpace.dist(pathHigherPriority[k], intersection.intersectPoint));
                                //RHSIndexes_forT.Add(k);
                                LHSPoints_forT.Add((float)stSpace.dist(pathHigherPriority[k], intersection.intersectPoint));
                                LHSIndexes_forT.Add(k);
                            }
                            else if (Math.Atan2(pathHigherPriority[k].Y - intersection.intersectPoint.Y, pathHigherPriority[k].X - intersection.intersectPoint.X) > 0)
                            {
                                RHSPoints_forT.Add((float)stSpace.dist(pathHigherPriority[k], intersection.intersectPoint));
                                RHSIndexes_forT.Add(k);
                                //LHSPoints_forT.Add((float)stSpace.dist(pathHigherPriority[k], intersection.intersectPoint));
                                //LHSIndexes_forT.Add(k);
                            }
                        //}
                    }
                }



                int forbiddenindex1_forT = -1; int forbiddenindex2_forT = -1; int indexStart_forT = -1; int indexEnd_forT = -1;
                bool flag_RHSpoints_forT_null = false, flag_LHSpoints_forT_null = false;
                if (RHSPoints_forT.Count != 0)
                {
                    forbiddenindex1_forT = Array.IndexOf(RHSPoints_forT.ToArray(), RHSPoints_forT.Min(element => Math.Abs(element)));
                    forbidden1_forT = new Point(pathHigherPriority[RHSIndexes_forT[forbiddenindex1_forT]].X, pathHigherPriority[RHSIndexes_forT[forbiddenindex1_forT]].Y);
                }
                else
                {
                    flag_RHSpoints_forT_null = true;
                    if ((float)stSpace.dist(pathHigherPriority[pathHigherPriority.Count - 1], intersection.intersectPoint) <= (float)stSpace.dist(pathHigherPriority[0], intersection.intersectPoint))
                    {
                        forbiddenindex1_forT = pathHigherPriority.Count - 1;
                        forbidden1_forT = new Point(pathHigherPriority[pathHigherPriority.Count - 1].X, pathHigherPriority[pathHigherPriority.Count - 1].Y);
                    }

                    else if ((float)stSpace.dist(pathHigherPriority[0], intersection.intersectPoint) <= (float)stSpace.dist(pathHigherPriority[pathHigherPriority.Count - 1], intersection.intersectPoint))
                    {
                        forbiddenindex1_forT = 0;
                        forbidden1_forT = new Point(pathHigherPriority[0].X, pathHigherPriority[0].Y);
                    }
                }
                if (LHSPoints_forT.Count != 0)
                {
                    forbiddenindex2_forT = Array.IndexOf(LHSPoints_forT.ToArray(), LHSPoints_forT.Min(element => Math.Abs(element)));
                    forbidden2_forT = new Point(pathHigherPriority[LHSIndexes_forT[forbiddenindex2_forT]].X, pathHigherPriority[LHSIndexes_forT[forbiddenindex2_forT]].Y);
                }

                else
                {
                    flag_LHSpoints_forT_null = true;
                    if ((float)stSpace.dist(pathHigherPriority[pathHigherPriority.Count - 1], intersection.intersectPoint) <= (float)stSpace.dist(pathHigherPriority[0], intersection.intersectPoint))
                    {
                        forbiddenindex2_forT = pathHigherPriority.Count - 1;
                        forbidden2_forT = new Point(pathHigherPriority[pathHigherPriority.Count - 1].X, pathHigherPriority[pathHigherPriority.Count - 1].Y);
                    }

                    else if ((float)stSpace.dist(pathHigherPriority[0], intersection.intersectPoint) <= (float)stSpace.dist(pathHigherPriority[pathHigherPriority.Count - 1], intersection.intersectPoint))
                    {
                        forbiddenindex2_forT = 0;
                        forbidden2_forT = new Point(pathHigherPriority[0].X, pathHigherPriority[0].Y);
                    }
                }

                if (!flag_LHSpoints_forT_null && !flag_RHSpoints_forT_null)
                {
                    indexStart_forT = Math.Min(RHSIndexes_forT[forbiddenindex1_forT], LHSIndexes_forT[forbiddenindex2_forT]);
                    indexEnd_forT = Math.Max(RHSIndexes_forT[forbiddenindex1_forT], LHSIndexes_forT[forbiddenindex2_forT]);
                }
                else if (flag_RHSpoints_forT_null)
                {
                    indexStart_forT = Math.Min(forbiddenindex1_forT, LHSIndexes_forT[forbiddenindex2_forT]);
                    indexEnd_forT = Math.Max(forbiddenindex1_forT, LHSIndexes_forT[forbiddenindex2_forT]);
                }
                else if (flag_LHSpoints_forT_null)
                {
                    indexStart_forT = Math.Min(RHSIndexes_forT[forbiddenindex1_forT], forbiddenindex2_forT);
                    indexEnd_forT = Math.Max(RHSIndexes_forT[forbiddenindex1_forT], forbiddenindex2_forT);
                }

                
            /*In case of a segment intersection, get the time when the other robot leaves the mutual section (considering the safety distance)*/
            bool flag_leaving_point_frbdn1 = false;
            bool flag_leavind_point_frbdn2 = false;
                
            int index_frbdn1 = pathLowerPriority.FindIndex(x=> x.X ==forbidden1_forT.X && x.Y == forbidden1_forT.Y);
            int index_frbdn2 = pathLowerPriority.FindIndex(x => x.X == forbidden2_forT.X && x.Y == forbidden2_forT.Y);

            if (index_frbdn1 == -1)
                time_where_higher_priority_robot_leaves_Mutual_Pathsection = allSTspaces[intersection.indexPath2][RHSIndexes_forT[forbiddenindex1_forT]].T;
            else
                if (index_frbdn2 == -1)
                    time_where_higher_priority_robot_leaves_Mutual_Pathsection = allSTspaces[intersection.indexPath2][LHSIndexes_forT[forbiddenindex2_forT]].T;
     
            /*End*/




                Decimal[] forbiddenT = new Decimal[] { allSTspaces[intersection.indexPath2][indexStart_forT].T, allSTspaces[intersection.indexPath2][indexEnd_forT].T };

                Decimal[] intervalOverlap = checkIntervalOverLap(correspondingT, forbiddenT);


                if (intervalOverlap != null)
                {
                    /**Calculating grid forbidden**/

                    STspace[][] forbidden = new STspace[(int)((forbiddenS[1] - forbiddenS[0]) / interpolationS) + 1][];
                    for (int m = 0; m < forbidden.Length; m++)
                    {
                        forbidden[m] = new STspace[(int)((intervalOverlap[1] - intervalOverlap[0]) / interpolationT) + 1];
                    }


                    for (int m = 0; m < forbidden.Length; m++)
                    {
                        for (int n = 0; n < forbidden[0].Length; n++)
                        {
                            forbidden[m][n] = new STspace(0, 0);
                        }
                    }
                    for (int m = 0; m < forbidden.Length; m++)
                    {
                        for (int n = 0; n < forbidden[m].Length; n++)
                        {
                            forbidden[m][n].S = forbiddenS[0] + (Decimal)(m * interpolationS);
                            forbidden[m][n].T = intervalOverlap[0] + (Decimal)(n * interpolationT);
                            //Console.WriteLine(forbidden[m][n].S + "#" + forbidden[m][n].T);
                        }
                    }
                    // Copy the last element exactly from STspace 
                    forbidden[forbidden.Length - 1][forbidden[forbidden.Length - 1].Length - 1] = new STspace(forbiddenS[1], intervalOverlap[1]);


                    return forbidden;
                
            }
            
                //}  
                
            return null;
        }

        public decimal getLeavingTime()
        {
            /* If there is a segment intersection between the path, this function returns the time when the robot with higher 
             priority leaves the mutual segment and the path is free for the robot with lower priority to move */

            return time_where_higher_priority_robot_leaves_Mutual_Pathsection;
        }

        public Decimal[] checkIntervalOverLap(Decimal[] Timeinterval1, Decimal[] TimeintervalForbidden)
        {
            /*returns  the overlap interval [start_time_overlap end_time_overlap] */
            decimal[] startAndEnd_Overlap = new decimal[2];

            if (((Timeinterval1[0] >= TimeintervalForbidden[0] && Timeinterval1[0] <= TimeintervalForbidden[1]) ||
                (Timeinterval1[1] >= TimeintervalForbidden[0] && Timeinterval1[1] <= TimeintervalForbidden[1])) 
                ||
               ((TimeintervalForbidden[0] >= Timeinterval1[0] && TimeintervalForbidden[0] <= Timeinterval1[1]) ||
                (TimeintervalForbidden[1] >= Timeinterval1[0] && TimeintervalForbidden[1] <= Timeinterval1[1])))
            {
                return new decimal[] { Math.Min(Timeinterval1[0], TimeintervalForbidden[0]), TimeintervalForbidden[1] }; //Math.Min  //Math.Min(Timeinterval1[1], Timeinterval2[1]) };
            }
            return null;
        }


        public STspace getIntersectionST(IntersectionPoint intersection)
        {
            //STspace intersectionST = new STspace(0,0);
            //for (int i = 0; i < intersections.Count; i++)
            //{
                Position[] smoothenedPath = bspline.getBSpline(bsplineDegree, allPaths[intersection.indexPath1], bspline.generateKnotVector(allPaths[intersection.indexPath1].Count - 1, bsplineDegree));
                List<Decimal> distances = new List<Decimal>(); List<int> indexes = new List<int>();
                for (int j = 0; j < smoothenedPath.Length; j++)
                {
                    distances.Add(stSpace.dist(smoothenedPath[j], intersection.intersectPoint));
                    //Console.WriteLine(stSpace.dist(smoothenedPath[j], intersections[i].intersectPoint));
                    //Console.WriteLine(smoothenedPath[j].X + "\t" + smoothenedPath[j].Y);
                }
                int index_min = Array.IndexOf(distances.ToArray(), distances.Min(element => Math.Abs(element)));
                return new STspace(allSTspaces[intersection.indexPath1][index_min].S, allSTspaces[intersection.indexPath1][index_min].T);
            //}
            //return intersectionST;
        }
        
        public void checkCollisionInTime(List<IntersectionPoint> intersections, List<List<Position>> allPaths, List<Position> addtionalPoints)
        {
            var lowestPriority =
                from intersect in intersections
                group intersect by intersect.priority into priorities
                let minPriority = priorities.Min( intersect => intersect.priority)
                select new {priority = priorities.Key , lowPriority = priorities.Where(intersect => intersect.priority == minPriority)};

            int index_minPriority = Array.IndexOf(intersections.ToArray(), lowestPriority);
                
        }

        public void sendVelociyProfilesToAGVs(string agv,decimal[,] timeAndVelocityProfile , decimal startSendingTime , decimal endSendingTime)
        {
            decimal[] time = new decimal[timeAndVelocityProfile.GetLength(0)];
            decimal[] diff = new decimal[time.Length];
            for (int i = 0; i < time.Length; i++)
                diff[i] = Math.Abs(time[i] - startSendingTime);
          
            int index_start_sending = Array.IndexOf(diff, diff.Min());
            diff = new decimal[time.Length];

            for (int i = 0; i < time.Length; i++)
                diff[i] = Math.Abs(time[i] - endSendingTime);
            int index_end_sending = Array.IndexOf(diff, diff.Min());

            /* To be done by the AGV Controller */

            /*Send velocity Commands to AGVs from time timeAndVelocityProfile[index_start_sending,0] to time timeAndVelocityProfile[index_end_sending,0] */
        }






        public void HandleAGVLowBatt(object sender, MULTIFORM_PCS.ControlModules.ConnectionModule.RobotBatteryStatusPublishers.CustomEventArgs e)
        {
            /*AGV Low battery Event handler*/
            flag_agv_low_batt = true;
            Console.WriteLine(" received this message: {0}", e.Message);

            string[] message_splitted = e.Message.Split(' ', '\t');


            string lowBattAGV = message_splitted[0];//"AGV1";
            //DateTime subscriptionTime = DateTime.Now;
            //DateTime processStartTime = new DateTime();

            ///*Process starting time*/
            //if (agvMovementData.agvGroups[0][0].startTime< 60 )
            //    processStartTime = new DateTime(subscriptionTime.Year, subscriptionTime.Month, subscriptionTime.Day, 0, 0, (int)agvMovementData.agvGroups[0][0].startTime);
            //else if (agvMovementData.agvGroups[0][0].startTime> 60 && agvMovementData.agvGroups[0][0].startTime<3600)
            //    processStartTime = new DateTime(subscriptionTime.Year, subscriptionTime.Month, subscriptionTime.Day, 0, (int)agvMovementData.agvGroups[0][0].startTime/60 , (int)agvMovementData.agvGroups[0][0].startTime%60);
            //else if (agvMovementData.agvGroups[0][0].startTime>3600)
            //    processStartTime = new DateTime(subscriptionTime.Year, subscriptionTime.Month, subscriptionTime.Day, (int)agvMovementData.agvGroups[0][0].startTime/3600, (int)(agvMovementData.agvGroups[0][0].startTime%3600)/60 , (int)(agvMovementData.agvGroups[0][0].startTime%3600)%60);

            //TimeSpan timeOfLowBatt = subscriptionTime.Subtract(processStartTime);
            Console.WriteLine(decimal.Parse(message_splitted[4]));
            TimeSpan timeOfLowBatt = TimeSpan.FromSeconds(double.Parse(message_splitted[4]));
            /* Getting the commands that need to be considered in the event of a low batt ... */
            List<AGVData> agvGroupForLowBatt = new List<AGVData>();
            List<List<Position>> Paths = new List<List<Position>>();
            AGVMovmentData agvInitialInfo = new AGVMovmentData(new string[] { "", "", "", "", "", "" }, new List<AGVData>(), AppDomain.CurrentDomain.BaseDirectory + "schedules.log");
            agvInitialInfo.init(AppDomain.CurrentDomain.BaseDirectory + "\\working_dir\\initial_run\\schedules.log");

            for (int i = 0; i < agvInitialInfo.agvMovementInfo.Count; i++)
            {
                if ((timeOfLowBatt.TotalSeconds >= (double)agvInitialInfo.agvMovementInfo[i].startTime &&
                    timeOfLowBatt.TotalSeconds <= (double)agvInitialInfo.agvMovementInfo[i].endTime) ||
                    timeOfLowBatt.TotalSeconds == (double)agvInitialInfo.agvMovementInfo[i].endTime)
                {
                    agvGroupForLowBatt.Add(agvInitialInfo.agvMovementInfo[i]);
                }
            }

            decimal[] startTimes = new decimal[agvGroupForLowBatt.Count];
            List<decimal> pathDurations = new List<decimal>();
            for (int i=0;i<startTimes.Length;i++)
            {
                startTimes[i] = agvGroupForLowBatt[i].startTime;
            }

            for (int j = 0; j < agvGroupForLowBatt.Count; j++)
                {
                    for (int k = 0; k < agvInitialInfo.agvMovementInfo.Count; k++)
                    {
                        if (agvGroupForLowBatt[j].taskID == agvInitialInfo.agvMovementInfo[k].taskID)
                        {
                            Paths.Add(astar.calculateRoute(getStationPosition(agvInitialInfo.agvMovementInfo[k].startPosition),getStationPosition(agvInitialInfo.agvMovementInfo[k].endPosition)));
                            STspace[] st_space = stSpace.generateSTspace(bspline.getBSpline(bsplineDegree, Paths[j], bspline.generateKnotVector(Paths[j].Count - 1, bsplineDegree)), agvInitialInfo.agvMovementInfo[k].startTime);
                            pathDurations.Add(st_space[st_space.Length-1].T - st_space[0].T);
                            break;
                        }
                    }
                if (Paths.Count == agvGroupForLowBatt.Count)
                    break;
                }

            CollisionInfoForPath[] collisionInfoForLowBattAGV = calculateOneGroupOfPaths(agvGroupForLowBatt, Paths, startTimes, pathDurations);
            int index_sequence_with_lowBattAGV = -1;// = new int[1];
            //for (int k = 0; k < index_sequence_with_lowBattAGV.Length; k++)
            //{
                for (int i = 0; i < collisionInfoForLowBattAGV.Length; i++)
                {
                    if (collisionInfoForLowBattAGV[i].usedAGV == lowBattAGV)
                    {
                        index_sequence_with_lowBattAGV = i;
                        break;
                    }
                }
            //}

            /* Calculating new Paths considering the low batt event */
            // full batt agv actually needs to be choosed wrt to the AGV sitting at the charging station. but this is not possible without having the feedback position. 
            // instead the agvmovementInfo is being checked to figure out which AGVs are not existing in there and choose one of them.
            
            // Choosing full batt agv
            string[] availableAGVs = new string[] {"AGV1" , "AGV2" , "AGV3","AGV4" , "AGV5"}; 
            bool[] flag_available_agvs = new bool[] {false,false,false,false,false};
            for (int k = 0; k < availableAGVs.Length; k++)
            {
                for (int i = 0; i < agvInitialInfo.agvMovementInfo.Count; i++)
                    if (agvInitialInfo.agvMovementInfo[i].usedAGV == availableAGVs[k])
                    {
                        flag_available_agvs[k] = true;
                        break;
                    }
            }
            
            List<int> indexes_full_batt_agvs = new List<int>();
            string[] fullBattAGVs = new string[]{};
            for (int i = 0; i < flag_available_agvs.Length; i++)
                if (flag_available_agvs[i] == false)
                    indexes_full_batt_agvs.Add(i);

            if (indexes_full_batt_agvs.Count != 0)
            {
                Array.Resize(ref fullBattAGVs,indexes_full_batt_agvs.Count);
                for (int i = 0; i < fullBattAGVs.Length; i++)
                    fullBattAGVs[i] = availableAGVs[indexes_full_batt_agvs[i]];
            }
            else
            {
                Console.WriteLine("No free full Batt AGV is available... ");
            }
            
            /* Plan new paths*/
            agvLowBattCollisionInfo = PlanPathInCaseOfLowBattEvent(Paths,agvInitialInfo.agvMovementInfo,pathDurations,collisionInfoForLowBattAGV, index_sequence_with_lowBattAGV, timeOfLowBatt, lowBattAGV , new string[] { fullBattAGVs[0] });
        }

        public CollisionInfoForPath[] getCollisionInfoAfterLowBatt()
        {
            return agvLowBattCollisionInfo;
        }

        public CollisionInfoForPath[] PlanPathInCaseOfLowBattEvent(List<List<Position>> relativePaths,List<AGVData> agvInfo,List<decimal> pathDurations, CollisionInfoForPath[] collisionInfoForGroupWithLowBattAGV, int index_sequence_with_lowBattAGV, TimeSpan timeOfLowBatt, string lowBattAGV, string[] fullBattAGV)
        {
             decimal responseTime = 5; // A response time of 5 Secs is considered for calculating new paths and checking the possible collisions. 

            /*Get Groups of AGVs which are moving throughout the plant at the time of Low battery*/
            List<int> present_sequences = new List<int>();
            //List<List<Position>> relativePaths = new List<List<Position>>();
            List<AGVData> agvMovementInfoWhenLowBatt = new List<AGVData>();

            //for (int k = 0; k < lowBattAGV.Length; k++)
            //{
                //for (int i = 0; i < agvInfo.Count; i++)
                //{
                //    if ((decimal)timeOfLowBatt.TotalSeconds + responseTime >= agvInfo[i].startTime && (decimal)timeOfLowBatt.TotalSeconds + responseTime <= agvInfo[i].endTime)
                //    {
                //        if (agvInfo[i].usedAGV != lowBattAGV)
                //        {
                //            agvMovementInfoWhenLowBatt.Add(agvInfo[i]);
                //            present_sequences.Add(i);
                //            relativePaths.Add(allPaths[i]);
                //        }
                //    }
                //}
            //}
            
            /* Get the position of all AGVs at the event of low batt */
            int counter = 0;
            Position lowBattAGVpositionAtTimeOfLowBatt = new Position(0,0);
            Position[] agvPositionsAtTimeOfLowBatt = new Position[collisionInfoForGroupWithLowBattAGV.Length - 1];
            bool[] flag_agv_at_destination_AtTimeOfLowBatt = new bool[collisionInfoForGroupWithLowBattAGV.Length];
            //for (int k = 0; k < index_sequence_with_lowBattAGV.Length; k++)
            //{
                for (int i = 0; i < collisionInfoForGroupWithLowBattAGV.Length; i++)
                {
                    decimal[] timeDiff = new decimal[collisionInfoForGroupWithLowBattAGV[i].updatedPathST.Length];
                    for (int j = 0; j < collisionInfoForGroupWithLowBattAGV[i].updatedPathST.Length; j++)
                        timeDiff[j] = Math.Abs(collisionInfoForGroupWithLowBattAGV[i].updatedPathST[j].T - ((decimal)timeOfLowBatt.TotalSeconds + responseTime));

                    if (i == index_sequence_with_lowBattAGV)
                    {
                        lowBattAGVpositionAtTimeOfLowBatt = collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition[Array.IndexOf(timeDiff, timeDiff.Min())];
                        if (lowBattAGVpositionAtTimeOfLowBatt.X == collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition[collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition.Length - 1].X &&
                            lowBattAGVpositionAtTimeOfLowBatt.Y == collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition[collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition.Length - 1].Y)
                            flag_agv_at_destination_AtTimeOfLowBatt[i] = true;
                        //Console.WriteLine("AGV position at the event of low batt: (" + lowBattAGVpositionAtTimeOfLowBatt.X + "," + lowBattAGVpositionAtTimeOfLowBatt.Y + ")");
                    }
                    else
                    {
                       agvPositionsAtTimeOfLowBatt[counter] = collisionInfoForGroupWithLowBattAGV[i].updatedPosition[Array.IndexOf(timeDiff, timeDiff.Min())];
                       if (agvPositionsAtTimeOfLowBatt[counter].X == collisionInfoForGroupWithLowBattAGV[i].updatedPosition[collisionInfoForGroupWithLowBattAGV[i].updatedPosition.Length - 1].X &&
                            agvPositionsAtTimeOfLowBatt[counter].Y == collisionInfoForGroupWithLowBattAGV[i].updatedPosition[collisionInfoForGroupWithLowBattAGV[i].updatedPosition.Length - 1].Y)
                           flag_agv_at_destination_AtTimeOfLowBatt[i] = true;
                        
                        Console.WriteLine("AGV position at the event of low batt: (" + agvPositionsAtTimeOfLowBatt[counter].X + "," + agvPositionsAtTimeOfLowBatt[counter].Y + ")");
                       counter++;
                    }

                }
            //}

            /* Get section of relative paths after timeOfLowBatt*/
            //if (relativePaths.Count != 0)  // if number of relative paths equals to zero, it means the LowBatt agvand the other AGVs reahced their destination duering response time
            //{
                Position[] relativePathStarts = new Position[agvPositionsAtTimeOfLowBatt.Length];
                int[] index_starts = new int[agvPositionsAtTimeOfLowBatt.Length];

                for (int i = 0; i < agvPositionsAtTimeOfLowBatt.Length; i++)
                {
                    if (flag_agv_at_destination_AtTimeOfLowBatt[i] == true)
                        continue;
                    else
                    {
                        decimal[] diff = new decimal[relativePaths[i].Count];
                        for (int j = 0; j < relativePaths[i].Count; j++)
                        {
                            if (j != index_sequence_with_lowBattAGV)
                            {
                                diff[j] = stSpace.dist(relativePaths[i][j], agvPositionsAtTimeOfLowBatt[i]);
                                Console.WriteLine(relativePaths[i][j].X + "\t" + relativePaths[i][j].Y);
                                Console.WriteLine("(" + agvPositionsAtTimeOfLowBatt[i].X + " , " + agvPositionsAtTimeOfLowBatt[i].Y + ")");
                                Console.WriteLine("diff[" + j + "] = " + diff[j]);
                            }
                        }
                        index_starts[i] = Array.IndexOf(diff, diff.Min());
                        relativePathStarts[i].X = relativePaths[i][Array.IndexOf(diff, diff.Min())].X;
                        relativePathStarts[i].Y = relativePaths[i][Array.IndexOf(diff, diff.Min())].Y;
                    }
                }
            
                for (int i = 0; i < relativePaths.Count; i++)
                {
                    if (i == index_sequence_with_lowBattAGV)
                        continue;

                    if (flag_agv_at_destination_AtTimeOfLowBatt[i] == true)
                        continue;
                    else                
                        relativePaths[i].RemoveRange(0, index_starts[i]);
                }
            //}
            /* Choose the station at which the LowBattAGV will release its vessel*/
            bool flag_noChange = false;
            decimal releaseEndTime = 0M;
            string lowBattAGVreleaseDestination = "";// = new string[lowBattAGV.Length];
            // If the destination is Mix let the low batt robot release the vessel at Mix
            //for (int k = 0; k < lowBattAGV.Length; k++)
            //{   
                if (collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition[collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition.Length - 1].X == getStationPosition("MIX").X &&
                    collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition[collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition.Length - 1].Y == getStationPosition("MIX").Y)
                {
                    lowBattAGVreleaseDestination = "MIX"; // No Change!!
                    flag_noChange = true;
                    releaseEndTime = collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPathST[collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPathST.Length - 1].T + responseTime + agvMovementData.mixingGrabTime + agvMovementData.dockingTime + agvMovementData.undockingTime;
                }
                else 
                    if (collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition[collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition.Length - 1].X == getStationPosition("STORAGE").X &&
                collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition[collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition.Length - 1].Y == getStationPosition("STORAGE").Y)
                {
                    lowBattAGVreleaseDestination = "STORAGE"; // No Change!!
                    flag_noChange = true;
                    releaseEndTime = collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPathST[collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPathST.Length - 1].T + responseTime +agvMovementData.mixingGrabTime + agvMovementData.dockingTime + agvMovementData.undockingTime;

                }
                else
                {
                    decimal dist_to_mix = stSpace.dist(lowBattAGVpositionAtTimeOfLowBatt, getStationPosition("MIX"));
                    decimal dist_to_storage = stSpace.dist(lowBattAGVpositionAtTimeOfLowBatt, getStationPosition("STORAGE"));
                    releaseEndTime = (decimal)timeOfLowBatt.TotalSeconds + agvMovementData.AGVMovementTime + agvMovementData.mixingGrabTime + agvMovementData.dockingTime + agvMovementData.undockingTime;
                    if (dist_to_mix < dist_to_storage)
                        lowBattAGVreleaseDestination= "MIX";
                    else if (dist_to_mix > dist_to_storage)
                        lowBattAGVreleaseDestination = "STORAGE";
                }


                /* Add new AGV's info to the current list of AGVData*/
                if (!flag_noChange)
                {
                    AGVData lowBattAGVToRelease = new AGVData(0, "", 0, 0, "", "");
                    if (lowBattAGVreleaseDestination == "MIX")
                        lowBattAGVToRelease = new AGVData(0, lowBattAGV, (decimal)timeOfLowBatt.TotalSeconds + responseTime, releaseEndTime, "AGVLowBattPosition", lowBattAGVreleaseDestination);
                    else if (lowBattAGVreleaseDestination == "STORAGE")
                        lowBattAGVToRelease = new AGVData(0, lowBattAGV, (decimal)timeOfLowBatt.TotalSeconds + responseTime, releaseEndTime, "AGVLowBattPosition", lowBattAGVreleaseDestination);


                    agvMovementInfoWhenLowBatt.Add(lowBattAGVToRelease);
                    // Plan new paths 
                    List<Position> pathReleaseVessel = astar.calculateRoute(lowBattAGVpositionAtTimeOfLowBatt, getStationPosition(lowBattAGVreleaseDestination));
                    relativePaths.Add(pathReleaseVessel);

                }
                //if (k == 0)
                //{
                    AGVData AGVafterReleaseToCharging = new AGVData(1, lowBattAGV, releaseEndTime, releaseEndTime + agvMovementData.AGVMovementTime, lowBattAGVreleaseDestination, "CHARGING1");
                    AGVData fullBattAGVtoVesselLocation = new AGVData(2, fullBattAGV[0], AGVafterReleaseToCharging.startTime, AGVafterReleaseToCharging.startTime + agvMovementData.AGVMovementTime, "CHARGING1", lowBattAGVreleaseDestination);
                    agvMovementInfoWhenLowBatt.Add(AGVafterReleaseToCharging);
                    agvMovementInfoWhenLowBatt.Add(fullBattAGVtoVesselLocation);
                    
                    if(!flag_noChange){

                        Position lowBattAGVdestination = new Position(collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition[collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition.Length - 1].X,
                                                               collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition[collisionInfoForGroupWithLowBattAGV[index_sequence_with_lowBattAGV].updatedPosition.Length - 1].Y);
                        List<Position> pathToFinalDestination = astar.calculateRoute(getStationPosition(lowBattAGVreleaseDestination), lowBattAGVdestination);
                        //agvMovementInfoWhenLowBatt.Add(pathToFinalDestination);
                    }

                //}
                //else if (k == 1)
                //{
                //    AGVData AGVafterReleaseToCharging = new AGVData(1, lowBattAGV, releaseEndTime, releaseEndTime + agvMovementData.AGVMovementTime, lowBattAGVreleaseDestination[k], "CHARGING2");
                //    AGVData fullBattAGVtoVesselLocation = new AGVData(2, fullBattAGV[k], AGVafterReleaseToCharging.startTime, AGVafterReleaseToCharging.startTime + agvMovementData.AGVMovementTime, "CHARGING2", lowBattAGVreleaseDestination[k]);
                //    agvMovementInfoWhenLowBatt.Add(AGVafterReleaseToCharging);
                //    agvMovementInfoWhenLowBatt.Add(fullBattAGVtoVesselLocation);
                //}
               

            /*Path to charging station*/    
            //if (k == 0)
                //{
                    List<Position> pathToChargingStation = astar.calculateRoute(getStationPosition(lowBattAGVreleaseDestination), getStationPosition("CHARGING1"));

                    for (int i = 0; i < pathToChargingStation.Count; i++)
                        Console.WriteLine(pathToChargingStation[i].X + "\t" + pathToChargingStation[i].Y);
                    

                    List<Position> pathFullBattToVesselLocation = astar.calculateRoute(getStationPosition("CHARGING1"), getStationPosition(lowBattAGVreleaseDestination));
                    for (int i = 0; i < pathFullBattToVesselLocation.Count; i++)
                        Console.WriteLine(pathFullBattToVesselLocation[i].X + "\t" + pathFullBattToVesselLocation[i].Y);
                    
                    relativePaths.Add(pathToChargingStation);
                    relativePaths.Add(pathFullBattToVesselLocation);
                //}
                //else if (k == 1)
                //{
                //    List<Position> pathToChargingStation = astar.calculateRoute(getStationPosition(lowBattAGVreleaseDestination[k]), getStationPosition("CHARGING2"));
                //    List<Position> pathFullBattToVesselLocation = astar.calculateRoute(getStationPosition("CHARGING2"), getStationPosition(lowBattAGVreleaseDestination[k]));
                //    relativePaths.Add(pathFullBattToVesselLocation);
                //    relativePaths.Add(pathToChargingStation);
                //} 
            //}
                
            /* Check collisions between new paths (for moving the robot to charging station) and already existing paths */
            decimal[] startTimes = new decimal[relativePaths.Count];
            List<decimal> pathDuration = new List<decimal>();

            for (int i = 0; i < startTimes.Length; i++)
            {
                startTimes[i] = (decimal)timeOfLowBatt.TotalSeconds + responseTime;
                STspace[] st_space = stSpace.generateSTspace(bspline.getBSpline(bsplineDegree, relativePaths[i], bspline.generateKnotVector(relativePaths[i].Count - 1, bsplineDegree)), startTimes[i]);
                pathDuration[i] = st_space[st_space.Length - 1].T - st_space[0].T;
            }
            
            CollisionInfoForPath[] collisionInfoAfterLowBatt = calculateOneGroupOfPaths(agvMovementInfoWhenLowBatt, relativePaths,startTimes,pathDuration);
            
            /* Sending new velocity commands to robots */
            for (int i = 0; i < collisionInfoAfterLowBatt.Length; i++)
                sendVelociyProfilesToAGVs(collisionInfoAfterLowBatt[i].usedAGV, collisionInfoAfterLowBatt[i].velocityProfile, (decimal)timeOfLowBatt.TotalSeconds + responseTime, collisionInfoAfterLowBatt[i].updatedPathST[collisionInfoAfterLowBatt[i].updatedPathST.Length - 1].T);            
            /* Update agvMovementInfo . replace all seuences of lowBatt agv with the new full batt agv */
            
            List<AGVData> newAGVmovementInfo = new List<AGVData>();

            for (int k = 0; k < lowBattAGV.Length; k++)
            {
                for (int i = 0; i < agvInfo.Count; i++)
                {
                    if (agvInfo[i].usedAGV == lowBattAGV)
                    {
                        newAGVmovementInfo[i] = agvInfo[i];
                        newAGVmovementInfo[i].usedAGV = fullBattAGV[k];
                    }
                }
            }
            agvMovementData.updateAgvMovementInfo(newAGVmovementInfo);
            return collisionInfoAfterLowBatt;
        }

        public int[] getAGVgroupAndSequenceWithLowBattery(TimeSpan timeOfLowBatt)
        {
            int index_group_with_lowBatteryAGV = -1;
            int index_sequence_with_lowBatteryAGV = -1;
 
            for (int i = 0; i < agvMovementData.agvGroups.Count; i++)
            {
                for (int j = 0; j < agvMovementData.agvGroups[i].Count; j++)
                    if (timeOfLowBatt.Seconds >= agvMovementData.agvGroups[i][j].startTime && timeOfLowBatt.Seconds <= agvMovementData.agvGroups[i][j].endTime)
                    {
                        index_group_with_lowBatteryAGV = i;
                        index_sequence_with_lowBatteryAGV = j;
                        return new int[]{index_group_with_lowBatteryAGV,index_sequence_with_lowBatteryAGV};
                    }
            }
            return null;
        }

        
        public List<List<IntersectionPoint>> IsIntersectionASegmentOrAPoint(List<IntersectionPoint> intersections, List<Position> Path1)  //Position[][]
        {
            int index_first_intersection_in_path = -1;
            int index_last_intersection_in_path = -1;
            List<int> index_break = new List<int>();
            int index_intersection_counter = 1;
            int numberOfSegments = 0;
            bool flag_possible_segment_same_direction = false;
            bool flag_possible_segment_opp_direction = false;
     

            for (int j = 0; j < Path1.Count; j++)
            {
                if (Path1[j].X == intersections[0].intersectPoint.X &&
                    Path1[j].Y == intersections[0].intersectPoint.Y)
                {
                    index_first_intersection_in_path = j;
                    break;
                }
            }
            for (int j = 0; j < Path1.Count; j++)
            {
                if (Path1[j].X == intersections[intersections.Count - 1].intersectPoint.X &&
                    Path1[j].Y == intersections[intersections.Count - 1].intersectPoint.Y)
                {
                    index_last_intersection_in_path = j;
                    break;
                }

            }

            
            while (index_intersection_counter!=intersections.Count)
            {
                if (index_first_intersection_in_path != 0)
                {
                    if (intersections[index_intersection_counter].intersectPoint.X == Path1[index_first_intersection_in_path - 1].X &&
                        intersections[index_intersection_counter].intersectPoint.Y == Path1[index_first_intersection_in_path - 1].Y)
                    { 
                        flag_possible_segment_opp_direction = true;
                        //index_intersection_counter++;
                        index_first_intersection_in_path--;
                    }
                    else if (intersections[index_intersection_counter].intersectPoint.Y == Path1[index_first_intersection_in_path + 1].Y &&
                             intersections[index_intersection_counter].intersectPoint.X == Path1[index_first_intersection_in_path + 1].X)
                    {
                        flag_possible_segment_same_direction = true;
                        //index_intersection_counter++;
                        index_first_intersection_in_path++;
                    }
                    else
                    {
                        if (flag_possible_segment_same_direction)
                        {
                            index_break.Add(index_intersection_counter - 1);

                            for (int j = 0; j < Path1.Count; j++)
                            {
                                if (Path1[j].X == intersections[index_intersection_counter].intersectPoint.X &&
                                   Path1[j].Y == intersections[index_intersection_counter].intersectPoint.Y)
                                {
                                    index_first_intersection_in_path = j;
                                    break;
                                }
                            }

                            //index_intersection_counter++;
                            //index_first_intersection_in_path++;
                            //flag_possible_segment_same_direction = false;
                        }

                        else if (flag_possible_segment_opp_direction)
                        {
                            index_break.Add(index_first_intersection_in_path + 1);

                            for (int j = 0; j < Path1.Count; j++)
                            {
                                if (Path1[j].X == intersections[index_intersection_counter].intersectPoint.X &&
                                    Path1[j].Y == intersections[index_intersection_counter].intersectPoint.Y)
                                {
                                    index_first_intersection_in_path = j;
                                    break;
                                }
                            }
                            //index_intersection_counter++;
                            //index_first_intersection_in_path--;
                            //flag_possible_segment_opp_direction = false;
                        }
                    }

                    index_intersection_counter++;
                    //if (flag_possible_segment_same_direction)
                    //    index_first_intersection_in_path++;
                    //if (flag_possible_segment_opp_direction)
                    //    index_first_intersection_in_path--;
                }
                else
                    if (intersections[index_intersection_counter].intersectPoint == Path1[index_first_intersection_in_path + 1])
                    {
                        flag_possible_segment_same_direction = true;
                        index_intersection_counter++;
                        index_first_intersection_in_path--;
                    }
                    else
                    {
                        if (flag_possible_segment_same_direction)
                        {
                            index_break.Add(index_first_intersection_in_path - 1);
                        }

                        else if (flag_possible_segment_opp_direction)
                        {
                            index_break.Add(index_first_intersection_in_path + 1);
                        }
                    }
            }

            int NumberOfGroups = index_break.Count +1;
            List<List<IntersectionPoint>> intersectionGroups = new List<List<IntersectionPoint>>();

            if (NumberOfGroups > 1)
            {
                for (int i = 0; i < NumberOfGroups; i++)
                {
                    List<IntersectionPoint> OneIntersectionGroup = new List<IntersectionPoint>();

                    if (i == 0)
                    {
                        for (int j = 0; j <= index_break[0]; j++)
                        {
                            OneIntersectionGroup.Add(intersections[j]);
                        }
                    }
                    if (i == NumberOfGroups - 1)
                    {
                        for (int j = index_break[index_break.Count - 1] + 1; j < intersections.Count; j++)
                            OneIntersectionGroup.Add(intersections[j]);
                    }
                    else if (i > 0)
                    {
                        for (int j = index_break[i - 1] + 1; j <= index_break[i]; j++)
                        {
                            OneIntersectionGroup.Add(intersections[j]);
                        }
                    }
                    intersectionGroups.Add(OneIntersectionGroup);

                }
            }
            else
                intersectionGroups.Add(intersections);

            return intersectionGroups;
        }



        public int[][] union(int[] set1, int[] set2)
        {
            /* set1 & set2 : consist of two intergers representing the start and end point of each set : */
            /* union : output set*/


            /*union function is going to be used for taking union of forbidden areas if there exist one. 
             set of indexes of start and end forbidden areas are sorted in decsending order. (starting point of set 2 is always greater or equal to starting point of set1)
             * So for 2 sets the following scenarios are only probable: 
             *   
             *               set1               set2
             * case 1:   '-----------'   '-------------------'     
             *          
             *                set1
             * case 2:   '------------'   set2
             *                  '--------------------'
             * 
             *                   set1
             * case 3:   '------------------'
             *                      set2
             *              '---------------'
             * 
             *                  set1
             * case 4:    '----------------' 
             *                  set2
             *            '---------------------------'  
             */

            int[][] union = new int[][] { };

            /*Case 1*/
            if (set1[1] < set2[0])
            {
                /* No intersection*/
                Array.Resize(ref union, 2);
                union[0] = set1;
                union[1] = set2;
                
                return union;
            }

            /*Case 2 & 3 & 4*/
            else if ((set1[1] > set2[0] && set1[1] <= set2[1]) || (set1[1] < set2[1]))
            {
                Array.Resize(ref union, 1);
                union[0] = new int[] { Math.Min(set1[0],set2[0]), Math.Max(set1[1], set2[1]) };
                
                return union;
            }

            return null;
        }

        public Position getStationPosition(string station)
        {
            Position stationPosition = new Position(0, 0);
            switch (station)
            {
                case ("STORAGE"):
                    {
                        stationPosition.X = 200;
                        stationPosition.Y = 250-50; // offset for proper docking
                    }
                    break;
                case ("INIT1"):
                    {
                        stationPosition.X = 287;//300;//287;
                        stationPosition.Y = 40;//50;//40;
                    }
                    break;
                case ("INIT2"):
                    {
                        stationPosition.X = 72 + 20;//300;//367;
                        stationPosition.Y = 230 - 20;//100;//67;
                    }
                    break;
                case ("INIT3"):
                    {
                        stationPosition.X = 328 - 20; //300;//327;
                        stationPosition.Y = 230 - 20;//150;//40;
                    }
                    break;
                case ("COL1"):
                    {
                        stationPosition.X = 60+50; // offset for proper docking
                        stationPosition.Y = 150;
                    }
                    break;
                case ("COL2"):
                    {
                        stationPosition.X = 340 - 50; // offset for proper docking
                        stationPosition.Y = 150;
                    }
                    break;
                case ("MIX"):
                    {
                        stationPosition.X = 200;
                        stationPosition.Y = 50 + 50; // offset for proper docking
                    }
                    break;
                case ("CHARGING1"):
                    {
                        stationPosition.X = 72;
                        stationPosition.Y = 230;
                    }
                    break;
                case ("CHARGING2"):
                    {
                        stationPosition.X = 328;
                        stationPosition.Y = 230;
                    }
                    break;
                default:
                    {
                        Console.WriteLine("No such Place found!!!");
                    }
                    break;
            }
            return stationPosition;
        }

        public string getStation(Position pose)
        {
            if (pose.X == 200 && pose.Y == 250)
                return "STORAGE";
            else if (pose.X == 287 && pose.Y == 40)
                return "INIT1";
            else if (pose.X == 367 && pose.Y == 67)
                return "INIT2";
            else if (pose.X == 327 && pose.Y == 40)
                return "INIT3";
            else if (pose.X == 60 && pose.Y == 150)
                return "COL1";
            else if (pose.X == 340 && pose.Y == 150)
                return "COL2";
            else if (pose.X == 200 && pose.Y == 50)
                return "MIX";
            else if (pose.X == 72 && pose.Y == 230)
                return "CHARGING1";
            else if (pose.X == 328 && pose.Y == 230)
                return "CHARGING2";
            else
                MessageBox.Show("Edit missed");
                return null;
        }



    }
}
