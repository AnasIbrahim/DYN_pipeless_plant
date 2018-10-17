using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MULTIFORM_PCS.ControlModules.RoutingModule
{
    class AStarModule : RoutingModule
    {
        private int[][] routingGrid;
        private int interpolation;

        public override List<Datastructure.Model.General.Position> calculateRoute(Datastructure.Model.General.Position posStart, Datastructure.Model.General.Position posEnd)
        {
            if (routingGrid == null)
            {
                int interpol = 10;
                routingGrid = getRoutingGrid(interpol);
                this.interpolation = interpol;
            }
            Datastructure.Model.General.Position gridStart = new Datastructure.Model.General.Position((int)(posStart.X / interpolation), (int)(posStart.Y / interpolation));
            Datastructure.Model.General.Position gridEnd = new Datastructure.Model.General.Position((int)(posEnd.X / interpolation), (int)(posEnd.Y / interpolation));
            List<AStarNode> openlist = new List<AStarNode>();
            AStarNode startNode = new AStarNode();
            startNode.costToReach = 0;
            startNode.position = gridStart;
            List<AStarNode> closedList = new List<AStarNode>();
            openlist.Add(startNode);
            while(openlist.Count > 0)
            {
                openlist.Sort();
                AStarNode curNode = openlist[0];
                openlist.RemoveAt(0);
                if (curNode.position.X == gridEnd.X && curNode.position.Y == gridEnd.Y)
                {
                    return getRoute(curNode, posStart, posEnd);
                }
                expandNode(openlist, closedList, curNode);
                closedList.Add(curNode);
            }
            return null;
        }
        private void expandNode(List<AStarNode> openlist, List<AStarNode> closedList, AStarNode curNode)
        {
            /**
             *     0
             * 7 X X X 1
             * 6 X   X 2
             * 5 X X X 3
             *     4
             */
            for (int i = 0; i < 8; i++)
            {
                AStarNode succ = new AStarNode();
                succ.position = new Datastructure.Model.General.Position(curNode.position.X, curNode.position.Y);
                if (i == 0)
                {
                    succ.position.Y = succ.position.Y + 1;
                }
                else if (i == 1)
                {
                    succ.position.X = succ.position.X + 1;
                    succ.position.Y = succ.position.Y + 1;
                }
                else if (i == 2)
                {
                    succ.position.X = succ.position.X + 1;
                }
                else if (i == 3)
                {
                    succ.position.X = succ.position.X + 1;
                    succ.position.Y = succ.position.Y - 1;
                }
                else if (i == 4)
                {
                    succ.position.Y = succ.position.Y - 1;
                }
                else if (i == 5)
                {
                    succ.position.X = succ.position.X - 1;
                    succ.position.Y = succ.position.Y - 1;
                }
                else if (i == 6)
                {
                    succ.position.X = succ.position.X - 1;
                }
                else if (i == 7)
                {
                    succ.position.X = succ.position.X - 1;
                    succ.position.Y = succ.position.Y + 1;
                }
                if (closedList_Contains(succ, closedList))
                {
                    continue;
                }
                if (succ.position.X < 0
                    || succ.position.X >= routingGrid.Length
                    || succ.position.Y < 0
                    || succ.position.Y >= routingGrid[0].Length)
                {
                    continue;
                }
                double cost = curNode.costToReach + interpolation + 1000 * routingGrid[(int)succ.position.X][(int)succ.position.Y];
                int contains = openList_Contains(succ, openlist);
                if (contains >= 0 && cost >= openlist[i].costToReach)
                {
                    continue;
                }
                succ.parent = curNode;
                succ.costToReach = cost;
                if (contains >= 0)
                {
                    openlist[i] = succ;
                }
                else
                {
                    openlist.Add(succ);
                }
            }
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
        private List<Datastructure.Model.General.Position> getRoute(AStarNode target, Datastructure.Model.General.Position posStart, Datastructure.Model.General.Position posEnd)
        {
            List<Datastructure.Model.General.Position> route = new List<Datastructure.Model.General.Position>();
            route.Add(posStart);
            List<Datastructure.Model.General.Position> posList = new List<Datastructure.Model.General.Position>();
            while (target != null)
            {
                posList.Add(target.position);
                target = target.parent;
            }
            posList.Reverse();
            for (int i = 0; i < posList.Count; i++)
            {
                route.Add(posList[i]);
            }
            return route;
        }

        public override int[][] getRoutingGrid(int interpolation)
        {
            Datastructure.Model.Plant curPlant = Gateway.ObserverModule.getInstance().getCurrentPlant();
            int[][] routingGrid = new int[(int)curPlant.theSize.Height / interpolation][];
            for (int i = 0; i < routingGrid.Length; i++)
            {
                routingGrid[i] = new int[(int)curPlant.theSize.Width / interpolation];
                for (int j = 0; j < routingGrid[i].Length; j++)
                {
                    //Add Stations here!
                    for (int k = 0; k < curPlant.AllStations.Count; k++)
                    {
                        if (Math.Abs(curPlant.AllStations[k].thePosition.X - i * interpolation) <= curPlant.AllStations[k].theSize.Height / 2 + 2 &&
                            Math.Abs(curPlant.AllStations[k].thePosition.Y - j * interpolation) <= curPlant.AllStations[k].theSize.Width / 2 + 2)
                        {
                            if (curPlant.AllStations[k].isChargingStation())
                            {
                                routingGrid[i][j] = 2;
                            }
                            else if (curPlant.AllStations[k].isFillingStation())
                            {
                                routingGrid[i][j] = 3;
                            }
                            else if (curPlant.AllStations[k].isMixingStation())
                            {
                                routingGrid[i][j] = 4;
                            }
                            else if (curPlant.AllStations[k].isStorageStation())
                            {
                                routingGrid[i][j] = 5;
                            }
                        }
                    }

                    for (int k = 0; k < curPlant.AllAGVs.Count; k++)
                    {
                        if (Math.Abs(curPlant.AllAGVs[k].theCurPosition.X - i * interpolation) <= curPlant.AllAGVs[k].Diameter / 2 + 2 &&
                            Math.Abs(curPlant.AllAGVs[k].theCurPosition.Y - j * interpolation) <= curPlant.AllAGVs[k].Diameter / 2 + 2)
                        {
                            routingGrid[i][j] = 1;
                        }
                    }
                }
            }
            return routingGrid;
        }

        private class AStarNode : IComparable
        {
            public double costToReach;
            public AStarNode parent;
            public Datastructure.Model.General.Position position;

            public int CompareTo(object o)
            {
                if (costToReach < ((AStarNode)o).costToReach)
                {
                    return -1;
                }
                else if (costToReach == ((AStarNode)o).costToReach)
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
