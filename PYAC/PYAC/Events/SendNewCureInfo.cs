using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PYAC.Events
{
    public class SendNewCureInfoEvent : PubSubEvent<InfoToSendCure>
    {
    }
    public class InfoToSendCure
    {
        public bool isNewCure { get; set; }
        public string batch_Number { get; set; }

        public InfoToSendCure(bool isNewCure, string batch_Number)
        {
            this.isNewCure = isNewCure;
            this.batch_Number = batch_Number;
        }
    }
}
