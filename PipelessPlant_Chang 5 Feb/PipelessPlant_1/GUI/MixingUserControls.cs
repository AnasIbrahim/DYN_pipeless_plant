using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.GUI
{
    class MixingUserControls
    {
        public int ID;
        public UserControlsCTRL.MixingCTRL mixCTRL;
        public UserControlsSummary.MixingStationSummary mixSum;
        public UserControlsView.MixingStation mixView;

        public MixingUserControls(int id)
        {
            this.ID = id;
        }
    }
}
