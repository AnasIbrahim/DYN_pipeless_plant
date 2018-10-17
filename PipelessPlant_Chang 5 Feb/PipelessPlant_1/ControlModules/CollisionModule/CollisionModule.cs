using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.CollisionModule
{
    abstract class CollisionModule
    {
        public abstract bool checkCollision(Datastructure.Model.AGV.AGV robot);
    }
}
