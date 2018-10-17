using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.DockingModule
{
    abstract class DockingModule
    {
        public abstract void dock(int robotID);
        public abstract void undock(int robotID);
    }
}
