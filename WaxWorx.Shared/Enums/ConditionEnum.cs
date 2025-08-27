using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaxWorx.Shared.Enums
{
    public enum ConditionEnum
    {
        [Description("Mint")]
        Mint = 0,
        [Description("Near Mint")]
        NearMint = 1,
        [Description("Very Good+")]
        VeryGoodPlus = 2,
        [Description("Very Good")]
        VeryGood = 3,
        [Description("Good")]
        Good = 4,
        [Description("Fair")]
        Fair = 5,
        [Description("Poor")]
        Poor = 6
    }
}
