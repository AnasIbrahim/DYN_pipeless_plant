using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.Velocity
{
    using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.DataTypes;
    
    class STAstar
    {
        public int[][] routingGrid;
        public decimal interpolation; //= 0.5M;
        public decimal interpolationS; //= 1M;
        public decimal interpolationT; //= (0.5M / 13M);//0.1M;//0.04f;

        private decimal velocityMax = 20; //   cm/sec

        public List<STspace> calculateRoute(STspace STStart, STspace STEnd, List<STspace[][]> STForbidden, int additionalTimeSteps , decimal totalAdditionalTime)//, decimal forbiddenTime)
        {
            if (routingGrid == null)
            {
                decimal interpolS = 2M;
                decimal interpolT = interpolS * (STEnd.T - STStart.T) / (STEnd.S - STStart.S);//(0.5M / 13M);//(0.3M / 7M);//0.04f;
                this.interpolationS = interpolS;
                this.interpolationT = interpolT;

                routingGrid = getRoutingGrid(STStart, STEnd, STForbidden,additionalTimeSteps);
                
            }

            STspace actualStart = new STspace(STStart.S, STStart.T);
            STspace actualEnd = new STspace(STEnd.S, STEnd.T + totalAdditionalTime);

            STspace gridStart = new STspace(0, 0);//new STspace(STStart.S / interpolationS, STStart.T / interpolationT);
            //STspace gridEnd = new STspace(Math.Round((STEnd.S - STStart.S) / interpolationS, MidpointRounding.AwayFromZero),Math.Round((STEnd.T - STStart.T) / interpolationT, MidpointRounding.AwayFromZero));//((float)((((decimal)STEnd.S - (decimal)STStart.S) / (decimal)interpolationS)), (float)((((decimal)STEnd.T - (decimal)STStart.T) / (decimal)interpolationT)));//new STspace(STEnd.S / interpolationS, STEnd.T / interpolationT);
            STspace gridEnd = new STspace(routingGrid.Length - 1, routingGrid[0].Length - 1);

            Console.WriteLine(gridStart.S + "#" + gridStart.T);
            Console.WriteLine(gridEnd.S + "#" + gridEnd.T);
            List<AStarNode> openlist = new List<AStarNode>();
            List<AStarNode> runTimeList = new List<AStarNode>();

            AStarNode startNode = new AStarNode();

            startNode.G = 0;
            startNode.H = heuristic(gridStart, gridEnd);
            startNode.F = startNode.H;

            startNode.st = gridStart;
            List<AStarNode> closedList = new List<AStarNode>();

            openlist.Add(startNode);
            runTimeList.Add(startNode);

            while (openlist.Count > 0)
            {
                openlist.Sort();
                AStarNode curNode = openlist[0];
                Console.WriteLine(curNode.st.S + "#" + curNode.st.T + "\t");
                openlist.RemoveAt(0);
                if (curNode.st.S == gridEnd.S )//&& curNode.st.T == gridEnd.T)
                {
                    Console.WriteLine(curNode.st.T);
                    List<STspace> finalRoute = getRoute(curNode, STStart, STEnd);
                    //for (int i = 0; i < finalRoute.Count; i++)
                    //{
                    //    Console.WriteLine(finalRoute[i].S + "#" +finalRoute[i].T) ;
                    //    routingGrid[(int)Math.Round(((finalRoute[i].S -STStart.S)/interpolationS))][(int)Math.Round(((finalRoute[i].T - STStart.T)/interpolationT))] = 8;
                    //}
                    //for (int l = 0; l < routingGrid.Length; l++)
                    //{
                    //    for (int k = 0; k < routingGrid[l].Length; k++)
                    //    {
                    //        grid.Write(routingGrid[l][k]);
                    //    }
                    //    grid.WriteLine("\n");

                    //    //grid.Write(routingGrid[(int)finalRoute[i].S][(int)finalRoute[i].T]);
                    //}
                    //grid.Flush();
                    //grid.Close();

                    return finalRoute;//getRoute(curNode, STStart, STEnd);
                }

                expandNode(openlist, closedList, runTimeList, curNode, gridEnd);
                closedList.Add(curNode);

            }
            return null;
        }
        private AStarNode[] StoreNeighborNodes(AStarNode curNode)
        {
            decimal s = curNode.st.S;
            decimal t = curNode.st.T;
            AStarNode[] neighbors = new AStarNode[8];

            for (int i = 0; i < neighbors.Length; i++)
            {
                neighbors[i] = new AStarNode();
            }


            if ((s > 0) && (t > 0))
                neighbors[0] = null; //.st = new STspace(s - 1, t - 1);
            else
                neighbors[0] = null;

            if (t > 0)
                neighbors[1] = null; //.st = new STspace(s, t - 1);
            else
                neighbors[1] = null;

            if ((s < (routingGrid.Length - 1)) && (t > 0))
                neighbors[2]= null;//.st = new STspace(s + 1, t - 1);
            else
                neighbors[2] = null;

            if (s > 0)
                neighbors[3] = null;//= new STspace(s - 1, t);
            else
                neighbors[3] = null;

            if (s < (routingGrid.Length - 1))
                neighbors[4] = null;//.st = new STspace(s + 1, t);
            else
                neighbors[4] = null;

            if ((s > 0) && (t < (routingGrid[0].Length - 1)))
                neighbors[5] = null;//new STspace(s - 1, t + 1);
            else
                neighbors[5] = null;

            if (t < (routingGrid[0].Length - 1))
                neighbors[6].st = new STspace(s, t + 1);
            else
                neighbors[6] = null;

            if ((s < (routingGrid.Length - 1)) && (t < (routingGrid[0].Length - 1)))
                neighbors[7].st = new STspace(s + 1, t + 1);
            else
                neighbors[7] = null;


            //// delete null elements
            //for (int i = 0; i < neighbors.Length; i++)
            //{
            //    if (neighbors[i] == null)
            //        nullElementCounter++;
            //    else   
            //        nonNullIndexes.Add(i);                
            //}
            //AStarNode[] finalNeighbors = new AStarNode[nonNullIndexes.Count];
            //for (int i = 0; i < nonNullIndexes.Count; i++)
            //{
            //    finalNeighbors[i] = neighbors[nonNullIndexes[i]];
            //    //Console.WriteLine(nonNullIndexes[i]);
            //}


            //return finalNeighbors;
            return neighbors;

        }
        private void expandNode(List<AStarNode> openlist, List<AStarNode> closedList, List<AStarNode> runTimeList, AStarNode curNode, STspace STendNode)
        {
            /**
             *     6
             * 5 X X X 7
             * 3 X   X 4
             *   X X X 
             *   0 1 2
             */
            
            AStarNode[] neighborNodes = StoreNeighborNodes(curNode);

            for (int i = 0; i < neighborNodes.Length; i++)
            {
                AStarNode node = neighborNodes[i];
                if (node == null)
                    continue;

                if (!isWalkable(node))
                    continue;
                
                if ((closedList_Contains(node, closedList)))
                    continue;
                //if (node.st.T <= curNode.st.T)
                //continue;
                //if (((node.st.S - curNode.st.S) / (node.st.T - curNode.st.T))*interpolation > velocityMax)
                //    continue;
                Console.WriteLine("(" + node.st.S + " , " + node.st.T + ")");
                bool tentative_is_better;
                bool was_added = false;
                double tentative_score = runTimeList[runTimeList_Contains(curNode, runTimeList)].G + neighborDistance(curNode, node);//+ heuristic(node.st, STendNode);//interpolation + 1000 * routingGrid[(int)(node.st.S)][(int)(node.st.T)];//+ interpolation*neighborDistance(curNode,succ);
                int contains = openList_Contains(node, openlist);
                if (contains < 0) //&& cost >= openlist[contains].costToReach)
                {
                    openlist.Add(node);
                    tentative_is_better = true;
                    was_added = true;

                    //continue;
                }
                else if (tentative_score < runTimeList[runTimeList_Contains(node, runTimeList)].G)
                    tentative_is_better = true;
                else
                    tentative_is_better = false;

                if (tentative_is_better)
                {
                    node.parent = curNode;

                    if (runTimeList_Contains(node, runTimeList) < 0)
                        runTimeList.Add(node);

                    runTimeList[runTimeList_Contains(node, runTimeList)].G = tentative_score;
                    runTimeList[runTimeList_Contains(node, runTimeList)].H = heuristic(node.st, STendNode);
                    runTimeList[runTimeList_Contains(node, runTimeList)].F = runTimeList[runTimeList_Contains(node, runTimeList)].G + runTimeList[runTimeList_Contains(node, runTimeList)].H;

                    if (!was_added)//(contains >= 0)
                        openlist[contains] = node;
                }

            }
        }

        private double neighborDistance(AStarNode node1, AStarNode node2)
        {

            decimal diffS = node2.st.S - node1.st.S;
            decimal diffT = node2.st.T - node1.st.T;//Math.Abs(node1.st.T - node2.st.T);
            
            if (diffS + diffT == 1)
            {
                if (diffT == 0)
                    return 200;
                else
                    return 40;
            }
            else 
                return 14;//Math.Sqrt(2);
            

        }

        private double heuristic(STspace STnode1, STspace endNode)
        {
            //float v = (endNode.S - STnode1.S) / (endNode.T - STnode1.T);
            //if (STnode2.T <= STnode1.T) //|| v > velocityMax)
            //    return infinity;
            //else
            return Math.Sqrt(Math.Pow((double)(STnode1.S - endNode.S), 2) + Math.Pow((double)(STnode1.T - endNode.T), 2));
        }


        private bool closedList_Contains(AStarNode checkNode, List<AStarNode> closedList)
        {
            for (int i = 0; i < closedList.Count; i++)
            {
                if (checkNode.st.S == closedList[i].st.S
                    && checkNode.st.T == closedList[i].st.T)
                {
                    return true;
                }
            }
            return false;
        }
        private int openList_Contains(AStarNode checkNode, List<AStarNode> openList)
        {
            for (int i = 0; i < openList.Count; i++)
            {
                if (checkNode.st.S == openList[i].st.S
                    && checkNode.st.T == openList[i].st.T)
                {
                    return i;
                }
            }
            return -1;
        }
        private int runTimeList_Contains(AStarNode checkNode, List<AStarNode> runTimeList)
        {
            for (int i = 0; i < runTimeList.Count; i++)
            {
                if (checkNode.st.S == runTimeList[i].st.S
                    && checkNode.st.T == runTimeList[i].st.T)
                {
                    return i;
                }
            }
            return -1;
        }



        private List<STspace> getRoute(AStarNode target, STspace STStart, STspace STEnd)
        {
            List<STspace> route = new List<STspace>();
            StreamWriter grid     = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\grid_st.log", false);
            //route.Add(STStart);
            List<STspace> posList = new List<STspace>();
            while (target != null)
            {
                routingGrid[(int)target.st.S][(int)target.st.T] = 8;

                target.st.S = target.st.S * interpolationS + STStart.S;
                target.st.T = target.st.T * interpolationT + STStart.T;

                posList.Add(target.st);
                target = target.parent;
            }
 
            for (int l = 0; l < routingGrid.Length; l++)
            {
                for (int k = 0; k < routingGrid[l].Length; k++)
                {
                    grid.Write(routingGrid[l][k]);
                }
                grid.WriteLine("\n");

                //grid.Write(routingGrid[(int)finalRoute[i].S][(int)finalRoute[i].T]);
            }
            grid.Flush();
            grid.Close();

            posList.Reverse();
            for (int i = 0; i < posList.Count; i++)
            {
                route.Add(posList[i]);
                Console.WriteLine(posList[i].S + "#" + posList[i].T);
            }
            return route;
        }

        public int[][] getRoutingGrid(STspace STinit, STspace STfinal, List<STspace[][]> STforbidden, int additionalTimeSteps)
        {
            Size curSTSpace = new Size((STfinal.S-STinit.S),(STfinal.T-STinit.T));//((double)((Decimal)STfinal.S - (Decimal)STinit.S), (double)((Decimal)STfinal.T - (Decimal)STinit.T));
            //Console.WriteLine(curSTSpace.Height + "# " + curSTSpace.Width);

            int[][] routingGrid = new int[(int)((curSTSpace.Height/interpolationS) + 1)][];
            for (int i = 0; i < routingGrid.Length; i++)
            {
                routingGrid[i] = new int[routingGrid.Length + additionalTimeSteps];//new int[(int)((curSTSpace.Width /interpolationT) + 1)];
                for (int j = 0; j < routingGrid[i].Length; j++)
                {
                    routingGrid[i][j] = new int();
                }

            }
            for (int k = 0; k < STforbidden.Count; k++)
                for (int l = 0; l < STforbidden[k].Length; l++)
                    for (int p = 0; p < STforbidden[k][l].Length;p++)
                    {
                        {
                            int m = (int)(STforbidden[k][l][p].S / interpolationS) - (int)(STinit.S/interpolationS);    //Be careful!!! if you don't cast Decimal it will convert ex. 1.3 to 1.299999. it represents fixed point data type.
                            int n = (int)(STforbidden[k][l][p].T / interpolationT) - (int)(STinit.T / interpolationT);

                            if (m < routingGrid.Length && n < routingGrid[0].Length)
                                routingGrid[m][n] = 5;
                        }
                    }
                    return routingGrid;
        
        }

        public bool isWalkable(AStarNode node)  // when node is already devided by interpolation
        {
            if (routingGrid[(int)(node.st.S)][(int)(node.st.T)] == 0)
                return true;
            else
                return false;
        }

        public decimal[,]getVelocityProfiles(List<STspace> STroute)
        {
            // this function interprets the route in ST space and outputs the velocity profile for each robot
            int startIndex = 0;
            decimal startTime = 0M;
        
            for (int i = 0; i < STroute.Count - 1; i++)
            {
                if (STroute[i + 1].S != 0)
                {
                    startTime = STroute[i].T;
                    //time[0] = startTime;
                    startIndex = i;
                    break;
                }
            }

            /*
            decimal[] velocity = new decimal[STroute.Count];
            decimal[] time = new decimal[STroute.Count];
            //time[0] = startTime;

            for (int i = startIndex+1; i < STroute.Count; i++)
            {

                velocity[i] = (STroute[i].S - STroute[i-1].S) / (STroute[i].T - STroute[i-1].T);
                time[i] = STroute[i].T;
                Console.WriteLine("Velocity[ " +i+ "] :"+   velocity[i] + ", time[" + i + "] :"   + time[i]);

            }
            */

            decimal[,]VelocityAndTime = new decimal[STroute.Count,2];
            for (int i = 1; i < VelocityAndTime.GetLength(0); i++)
            {
                VelocityAndTime[i, 0] = (STroute[i].S - STroute[i - 1].S) / (STroute[i].T - STroute[i - 1].T);
                Console.WriteLine("Velocity : " + VelocityAndTime[i,0]);
                VelocityAndTime[i, 1] = STroute[i].T;
            }

            //VelocityAndTime[0, 0] = velocity;
            //VelocityAndTime[0, 1] = time;


            return VelocityAndTime;
        }

        public decimal[,][] getVelocityProfiles_withoutAstar(STspace[] STforbidden, STspace[] STRobotWithLowerPriority)
        {
            decimal interpolationS = 2M;
            decimal interpolationT = interpolationS * (STRobotWithLowerPriority[STRobotWithLowerPriority.Length - 1].T - STRobotWithLowerPriority[0].T) / (STRobotWithLowerPriority[STRobotWithLowerPriority.Length - 1].S - STRobotWithLowerPriority[0].S);
            decimal timeStartZeroVelocity = new decimal();
            decimal timeEndZeroVelocity   = new decimal();
            bool intersectionInTime = false;

            for (int i = 0; i < STRobotWithLowerPriority.Length; i++)
            {
                if (STRobotWithLowerPriority[i].S >= STforbidden[0].S && STRobotWithLowerPriority[i].S <= STforbidden[STforbidden.Length - 1].S)
                    if (STRobotWithLowerPriority[i].T >= STforbidden[0].T && STRobotWithLowerPriority[i].T <= STforbidden[STforbidden.Length - 1].T)
                    {
                        intersectionInTime = true;
                        break;
                    }
            }

            int index = new int();
            if (intersectionInTime)
            {
                for (int i = 0; i < STRobotWithLowerPriority.Length;i++ )
                    if (STRobotWithLowerPriority[i].S == STforbidden[0].S)
                    {
                        index = i;
                        break;
                    }
                    
                timeStartZeroVelocity = STRobotWithLowerPriority[index].T;
                timeEndZeroVelocity   = STforbidden[STforbidden.Length - 1].T;
            }

            decimal[] times = new decimal[2];
            times[0] = timeStartZeroVelocity;
            times[1] = timeEndZeroVelocity;
            
            int additionalTimeSteps = (int)((timeEndZeroVelocity-timeStartZeroVelocity) / interpolationT) + 1;
            decimal[] velocity = new decimal[STRobotWithLowerPriority.Length + additionalTimeSteps];
            decimal[] time = new decimal[STRobotWithLowerPriority.Length + additionalTimeSteps];

            for (int i = 0; i < STRobotWithLowerPriority.Length;i++ )
            {
                time[i] = STRobotWithLowerPriority[i].T;
                if (STRobotWithLowerPriority[i].T < times[0])
                    velocity[i] = velocityMax;
                else if (STRobotWithLowerPriority[i].T >= times[0] && STRobotWithLowerPriority[i].T <= times[1])
                    velocity[i] = 0;
                else if (STRobotWithLowerPriority[i].T > times[1])
                    velocity[i] = velocityMax;
            }

            for (int j = 0; j < additionalTimeSteps; j++)
            {
                velocity[STRobotWithLowerPriority.Length + j] = velocityMax;
                time[STRobotWithLowerPriority.Length + j] = STRobotWithLowerPriority[STRobotWithLowerPriority.Length - 1].T + (decimal)(j * interpolationT);
            }

            decimal[,][] VelocityAndTime = new decimal[2, velocity.Length][];
            VelocityAndTime[0, 0] = velocity;
            VelocityAndTime[0, 1] = time;


            return VelocityAndTime;
        }

        public class AStarNode : IComparable
        {
            public double G;
            public double H;
            public double F;    // F = G+H
            public AStarNode parent;
            public STspace st;

            public int CompareTo(object o)
            {
                if (F < ((AStarNode)o).F)
                {
                    return -1;
                }
                else if (F == ((AStarNode)o).F)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }


        }
    }
}

