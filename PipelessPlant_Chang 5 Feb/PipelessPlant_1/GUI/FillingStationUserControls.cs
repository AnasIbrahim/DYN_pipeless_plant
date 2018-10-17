using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.GUI
{
    class FillingStationUserControls
    {
        public int ID;
        public UserControlsCTRL.FillingCTRL fillCTRL;
        public UserControlsSummary.FillingStationSummary fillSum;
        public UserControlsView.FillingStation fillView;

        public FillingStationUserControls(int id)
        {
            this.ID = id;
        }
    }
}
