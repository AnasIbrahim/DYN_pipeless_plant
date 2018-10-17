using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.GUI
{
    class AGVUserControls
    {
        public int ID;
        public UserControlsCTRL.AGVCTRL agvCTRL;
        public UserControlsSummary.AGVSummary agvSum;
        public UserControlsView.AGV agvView;
        public UserControlsView.AGV shadowView;

        public AGVUserControls(int id)
        {
            this.ID = id;
        }
    }
}
