using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.GUI
{
    class ChargingStationUserControls
    {
        public int ID;
        public UserControlsCTRL.ChargingCTRL chargeCTRL;
        public UserControlsSummary.ChargingStationSummary chargeSum;
        public UserControlsView.ChargingStation chargeView;

        public ChargingStationUserControls(int id)
        {
            this.ID = id;
        }
    }
}
