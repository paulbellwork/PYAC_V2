using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PYAC.Enums
{
    public enum RunningSegmentStatus
    {
        [Description("Ramp Complete")]
        RampComplete,
        [Description("Ramp Down")]
        RampDown,
        [Description("Soak")]
        Soak,
        [Description("Purge")]
        Purge,
        [Description("Ramp Up")]
        RampUp
    }
}
