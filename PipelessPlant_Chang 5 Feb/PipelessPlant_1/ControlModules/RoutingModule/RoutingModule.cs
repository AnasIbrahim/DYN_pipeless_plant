using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.RoutingModule
{
    public abstract class RoutingModule
    {
        public abstract List<Datastructure.Model.General.Position> calculateRoute(Datastructure.Model.General.Position posStart, Datastructure.Model.General.Position posEnd);
        public abstract int[][] getRoutingGrid(int interpolation);
    }
}
