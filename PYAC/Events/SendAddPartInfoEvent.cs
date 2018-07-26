using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PYAC.Events
{
    public class SendAddPartInfoEvent : PubSubEvent<InfoToSendParts>
    {
    }

    public class InfoToSendParts
    {
        public string partNumber { get; set; }
        public string travellerNumber { get; set; }
        public string operation { get; set; }

        public InfoToSendParts(string param1, string param2, string param3)
        {
            this.partNumber = param1;
            this.travellerNumber = param2;
            this.operation = param3;
        }
    }
}
