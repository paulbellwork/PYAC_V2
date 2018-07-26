using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PYAC.Models
{
    public class SegmentSummaryPageModel : BindableBase
    {
        private string _startStopLabel = "Start";
        public string StartStopLabel
        {
            get { return _startStopLabel; }
            set { SetProperty(ref _startStopLabel, value); }
        }
        private bool _startStopEnabled = true;
        public bool StartStopEnabled
        {
            get { return _startStopEnabled; }
            set { SetProperty(ref _startStopEnabled, value); }
        }
    }
}
