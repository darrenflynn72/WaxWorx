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
        Mint = 1,
        [Description("Near Mint")]
        NearMint = 2,
        [Description("Very Good+")]
        VeryGoodPlus = 3,
        [Description("Very Good")]
        VeryGood = 4,
        [Description("Good")]
        Good = 5,
        [Description("Fair")]
        Fair = 6,
        [Description("Poor")]
        Poor = 7
    }
}
