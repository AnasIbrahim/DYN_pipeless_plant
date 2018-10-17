using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.Path
{
    using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.DataTypes;
    class Astar
    {
        private int[][] routingGrid;
        private int interpolation = 10;
        StreamWriter grid;
        private int[,] finalRouteIndexes;  /*Route indexes excluding start and end point*/

        public List<Position> calculateRoute(Position posStart, Position posEnd)
        {
            if (routingGrid == null)
            {
                int interpol = 10;
                routingGrid = getRoutingGrid(interpol);
                this.interpolation = interpol;
            }

            Position gridStart = new Position((int)(posStart.X / interpolation), (int)(posStart.Y / interpolation));
            Position gridEnd = new Position((int)(posEnd.X / interpolation), (int)(posEnd.Y / interpolation));

            List<AStarNode> openlist = new List<AStarNode>();
            List<AStarNode> runTimeList = new List<AStarNode>();
            AStarNode startNode = new AStarNode();

            startNode.G = 0;
            startNode.H = heuristic(gridStart, gridEnd);
            startNode.F = startNode.H;

            startNode.position = gridStart;


            List<AStarNode> closedList = new List<AStarNode>();
            openlist.Add(startNode);
            runTimeList.Add(startNode);

            while (openlist.Count > 0)
            {
                openlist.Sort();
                AStarNode curNode = openlist[0];
                openlist.RemoveAt(0);
                if (curNode.position.X == gridEnd.X && curNode.position.Y == gridEnd.Y)
                {
                    return getRoute(curNode, posStart, posEnd);
                }
                expandNode(openlist, closedList, runTimeList, curNode, gridEnd);
                closedList.Add(curNode);
            }
            return null;
        }
        private void expandNode(List<AStarNode> openlist, List<AStarNode> closedList, List<AStarNode> runTimeList, AStarNode curNode, Position gridEnd)
        {
            /**
             *     0
             * 7 X X X 1
             * 6 X   X 2
             * 5 X X X 3
             *     4
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

                bool tentative_is_better;
                bool was_added = false;
                double tentative_score = runTimeList[runTimeList_Contains(curNode, runTimeList)].G + neighborDistance(curNode, node);//+ heuristic(node.st, STendNode);//interpolation + 1000 * routingGrid[(int)(node.st.S)][(int)(node.st.T)];//+ interpolation*neighborDistance(curNode,succ);

                //Console.WriteLine("neighborDistance:" + neighborDistance(curNode, node));

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
                {
                    tentative_is_better = false;
                }

                if (tentative_is_better)
                {
                    node.parent = curNode;

                    if (runTimeList_Contains(node, runTimeList) < 0)
                        runTimeList.Add(node);

                    runTimeList[runTimeList_Contains(node, runTimeList)].G = tentative_score;
                    runTimeList[runTimeList_Contains(node, runTimeList)].H = heuristic(node.position, gridEnd);
                    runTimeList[runTimeList_Contains(node, runTimeList)].F = runTimeList[runTimeList_Contains(node, runTimeList)].G + runTimeList[runTimeList_Contains(node, runTimeList)].H;

                    if (!was_added)//(contains >= 0)
                    {
                        openlist[contains] = node;
                    }
                }

            }
        }

        private AStarNode[] StoreNeighborNodes(AStarNode curNode)
        {
            float x = curNode.position.X;
            float y = curNode.position.Y;
            AStarNode[] neighbors = new AStarNode[8];

            for (int i = 0; i < neighbors.Length; i++)
            {
                neighbors[i] = new AStarNode();
            }


            if ((x > 0) && (y > 0))
                neighbors[0].position = new Position(x - 1, y - 1);
            else
                neighbors[0] = null;

            if (y > 0)
                neighbors[1].position = new Position(x, y - 1);
            else
                neighbors[1] = null;

            if ((x < (routingGrid.Length - 1)) && (y > 0))
                neighbors[2].position = new Position(x + 1, y - 1);
            else
                neighbors[2] = null;

            if (x > 0)
                neighbors[3].position = new Position(x - 1, y);
            else
                neighbors[3] = null;

            if (x < (routingGrid.Length - 1))
                neighbors[4].position = new Position(x + 1, y);
            else
                neighbors[4] = null;

            if ((x > 0) && (y < (routingGrid[0].Length - 1)))
                neighbors[5].position = new Position(x - 1, y + 1);
            else
                neighbors[5] = null;

            if (y < (routingGrid[0].Length - 1))
                neighbors[6].position = new Position(x, y + 1);
            else
                neighbors[6] = null;

            if ((x < (routingGrid.Length - 1)) && (y < (routingGrid[0].Length - 1)))
                neighbors[7].position = new Position(x + 1, y + 1);
            else
                neighbors[7] = null;


            return neighbors;

        }



        private double neighborDistance(AStarNode node1, AStarNode node2)
        {

            double diffX = Math.Abs(node1.position.X - node2.position.X);
            double diffY = Math.Abs(node1.position.Y - node2.position.Y);

            if (diffX + diffY == 2)
                return 14;
            else
                return 10;
            //return Math.Sqrt(Math.Pow(diffX,2) + Math.Pow(diffY,2)); 
        }

        private double heuristic(Position node1, Position node2)
        {
            return Math.Sqrt(Math.Pow((node1.X - node2.X), 2) + Math.Pow((node1.Y - node2.Y), 2));
        }

        private bool closedList_Contains(AStarNode checkNode, List<AStarNode> closedList)
        {
            for (int i = 0; i < closedList.Count; i++)
            {
                if (checkNode.position.X == closedList[i].position.X
                    && checkNode.position.Y == closedList[i].position.Y)
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
                if (checkNode.position.X == openList[i].position.X
                    && checkNode.position.Y == openList[i].position.Y)
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
                if (checkNode.position.X == runTimeList[i].position.X
                    && checkNode.position.Y == runTimeList[i].position.Y)
                {
                    return i;
                }
            }
            return -1;
        }

        private bool isWalkable(AStarNode node)  // when node is already devided by interpolation
        {
            if (routingGrid[(int)(node.position.X)][(int)(node.position.Y)] == 0)
                return true;
            else
                return false;
        }

        private List<Position> getRoute(AStarNode target, Position posStart, Position posEnd)
        {
            List<Position> route = new List<Position>();
            route.Add(posStart);
            List<Position> posList = new List<Position>();
            
            while (target != null)
            {
                target.position.X = target.position.X * interpolation;
                target.position.Y = target.position.Y * interpolation;

                posList.Add(target.position);
                //Console.Write(target.position.X + "#" +target.position.Y + "\t");
                target = target.parent;
            }
            posList.Reverse();
            int[,] finalRouteIndexes = new int [posList.Count-2,2];

            for (int i = 1; i < posList.Count-1; i++)
            {
                finalRouteIndexes[i - 1, 0] = (int)(posList[i].X /interpolation) ;
                finalRouteIndexes[i - 1, 1] = (int)(posList[i].Y /interpolation) ;
                route.Add(posList[i]);
            }
            this.finalRouteIndexes = finalRouteIndexes;
            route.Add(posEnd);

            /* Copy the routing grid to a log file */
            StreamWriter grid_writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "astar_grid.log", false);
            bool flag_already_written = false;
            for (int m = 0; m < routingGrid.Length; m++)
            {
                for (int n = 0; n < routingGrid[m].Length; n++)
                {
                    for (int p = 0; p < finalRouteIndexes.GetLength(0); p++)
                    {
                        if (m == finalRouteIndexes[p, 0] && n == finalRouteIndexes[p, 1])
                        {
                            grid_writer.Write(8);
                            flag_already_written = true;
                        }
                    }
                    if(!flag_already_written)
                        grid_writer.Write(routingGrid[m][n]);
                    flag_already_written = false;
                }
                grid_writer.WriteLine("\n");
            }
            grid_writer.Flush(); grid_writer.Close();
            return route;
        }

        public void updateRoutingGrid()
        {
            float AGVDiameter = 26;
            grid = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\grid.log", false);
            
            for (int i = 0; i < routingGrid.Length; i++)
            {
                for (int j = 0; j < routingGrid[i].Length; j++)
                {
                    for (int k = 0; k < finalRouteIndexes.GetLength(0); k++)
                    {
                        if (Math.Abs(finalRouteIndexes[k, 0] - i) <= AGVDiameter / 2 + 2 &&
                            Math.Abs(finalRouteIndexes[k, 1] - j) <= AGVDiameter / 2 + 2)
                            routingGrid[i][j] = 1;
                    }
                }
            }
                    

            
            for (int i = 0; i < routingGrid.Length; i++)
            {
                for (int j = 0; j < routingGrid[i].Length; j++)
                    grid.Write(routingGrid[i][j]);
                grid.WriteLine("\n");
            }

            grid.Flush();
            grid.Close();
        }

        public int[][] getRoutingGrid(int interpolation)
        {
            StationSize curPlant = new StationSize(400, 300);

            StationSize CharStationSize = new StationSize(20, 20);
            StationSize FillStationSize = new StationSize(40, 40);
            StationSize StrStationSize = new StationSize(40, 40);
            StationSize MixStationSize = new StationSize(40, 40);

            //Position[] positionAllstations;
            Position Charging1StationPose = new Position(72, 230);
            Position Charging2StationPose = new Position(328, 230);
            Position Filling1StationPose = new Position(34, 150);
            Position Filling2StationPose = new Position(366.5f, 150);
            Position StorageStationPose = new Position(200, 279.5f);
            Position MixingStationPose = new Position(200, 20.5f);

            //Position AGV1Pose = new Position(287, 40);
            //Position AGV2Pose = new Position(367, 67);

            float AGVDiameter = 26;

            //int numberOfStations = 4;
            

            int[][] routingGrid = new int[(int)curPlant.Height / interpolation][];
            for (int i = 0; i < routingGrid.Length; i++)
            {
                routingGrid[i] = new int[(int)curPlant.Width / interpolation];
                for (int j = 0; j < routingGrid[i].Length; j++)
                {
                    //Add Stations here!
                    if (Math.Abs(Charging1StationPose.X - i * interpolation) <= CharStationSize.Height / 2 + 2 &&
                        Math.Abs(Charging1StationPose.Y - j * interpolation) <= CharStationSize.Width / 2 + 2)
                    {

                        routingGrid[i][j] = 0;//2;
                    }

                    if (Math.Abs(Charging2StationPose.X - i * interpolation) <= CharStationSize.Height / 2 + 2 &&
                        Math.Abs(Charging2StationPose.Y - j * interpolation) <= CharStationSize.Width / 2 + 2)
                    {

                        routingGrid[i][j] = 0;//2;
                    }

                    if (Math.Abs(Filling1StationPose.X - i * interpolation) <= FillStationSize.Height / 2 + 2 &&
                        Math.Abs(Filling1StationPose.Y - j * interpolation) <= FillStationSize.Width / 2 + 2)
                    {

                        routingGrid[i][j] = 3;
                    }

                    if (Math.Abs(Filling2StationPose.X - i * interpolation) <= FillStationSize.Height / 2 + 2 &&
                        Math.Abs(Filling2StationPose.Y - j * interpolation) <= FillStationSize.Width / 2 + 2)
                    {

                        routingGrid[i][j] = 3;
                    }

                    if (Math.Abs(MixingStationPose.X - i * interpolation) <= MixStationSize.Height / 2 + 2 &&
                        Math.Abs(MixingStationPose.Y - j * interpolation) <= MixStationSize.Width / 2 + 2)
                    {

                        routingGrid[i][j] = 4;
                    }

                    if (Math.Abs(StorageStationPose.X - i * interpolation) <= StrStationSize.Height / 2 + 2 &&
                        Math.Abs(StorageStationPose.Y - j * interpolation) <= StrStationSize.Width / 2 + 2)
                    {

                        routingGrid[i][j] = 5;
                    }

                    //if (Math.Abs(AGV1Pose.X - i * interpolation) <= AGVDiameter/ 2 + 2 &&
                    //    Math.Abs(AGV1Pose.Y - j * interpolation) <= AGVDiameter/ 2 + 2)
                    //{
                    //    routingGrid[i][j] = 1;
                    //}

                    //if (Math.Abs(AGV2Pose.X - i * interpolation) <= AGVDiameter / 2 + 2 &&
                    //        Math.Abs(AGV2Pose.Y - j * interpolation) <= AGVDiameter / 2 + 2)
                    //{
                    //    routingGrid[i][j] = 1;
                    //}
                }
            }

            
            return routingGrid;
        }

        public void getFinalRouteIndexes()
        {
            
        }



        private class AStarNode : IComparable
        {
            public double G;
            public double H;
            public double F;

            public AStarNode parent;
            public Position position;

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
