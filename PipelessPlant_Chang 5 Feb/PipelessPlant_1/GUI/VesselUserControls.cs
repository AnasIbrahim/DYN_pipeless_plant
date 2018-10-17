using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.GUI
{
    public class VesselUserControls
    {
        public int ID;
        public UserControlsSummary.VesselSummary vesSum;
        public UserControlsView.Vessel vesView;

        public VesselUserControls(int id)
        {
            this.ID = id;
        }
    }
}
