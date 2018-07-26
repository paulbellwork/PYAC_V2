using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kepware.ClientAce.OpcDaClient;


namespace PYAC.Events
{
    public class UpdatedSourceEvent : PubSubEvent<OPCObject>
    {
    }
    
    public class OPCObject
    {
        public string itemIdentifier { get; set; }
        public string itemValue { get; set; }
        public OPCObject(string itemIdentifier, string itemValue)
        {
            this.itemIdentifier = itemIdentifier;
            this.itemValue = itemValue;
        }
    }
}
