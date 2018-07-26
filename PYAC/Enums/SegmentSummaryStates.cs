using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace PYAC
{
    public enum SegmentSummaryStates
    {
        [Description("En Attente")]
        EnAttente,
        [Description("Cycle Arrêté")]
        CycleArrete,
        [Description("Cycle En Cours")]
        CycleEnCours
    }

    

}
