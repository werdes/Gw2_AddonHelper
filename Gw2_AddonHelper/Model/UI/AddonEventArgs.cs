using Gw2_AddonHelper.AddonLib.Model.GameState;
using System;

namespace Gw2_AddonHelper.Model.UI
{
    public class AddonEventArgs : EventArgs
    {
        public AddonContainer AddonContainer { get; set; }

        public AddonEventArgs(AddonContainer addonContainer)
        {
            AddonContainer = addonContainer;
        }
    }
}
