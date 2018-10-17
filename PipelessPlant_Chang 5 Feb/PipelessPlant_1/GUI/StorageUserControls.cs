using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.GUI
{
    class StorageUserControls
    {
        public int ID;
        public UserControlsCTRL.StorageCTRL stoCTRL;
        public UserControlsSummary.StorageStationSummary stoSum;
        public UserControlsView.StorageStation stoView;

        public StorageUserControls(int id)
        {
            this.ID = id;
        }
    }
}
