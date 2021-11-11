using Gw2_AddonHelper.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Model.UI
{
    public class AddonListBatchActionEventArgs : EventArgs
    {
        public AddonBatchAction AddonBatchAction { get; set; }
        public InstallState InstallState { get; set; }

        public AddonListBatchActionEventArgs(AddonBatchAction addonBatchAction, InstallState installState)
        {
            AddonBatchAction = addonBatchAction;
            InstallState = installState;
        }
    }
}
