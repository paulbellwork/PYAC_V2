using System;
using System.Collections.Generic;
using System.Text;
using Kepware.ClientAce.OpcDaClient;

namespace Utl
{
    public interface IOPCInterface
    {
        void OPC_ReadCompleted(int transactionHandle, bool allQualitiesGood, bool noErrors, ItemValueCallback[] itemValues);
        void OPC_DataChanged(int clientSubscription, bool allQualitiesGood, bool noErrors, ItemValueCallback[] itemValues);
        void OPC_ServerStateChanged(int clientHandle, ServerState state);
    }
}
