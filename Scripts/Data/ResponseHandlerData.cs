using FishNet.Connection;
using FishNet.Serializing;

namespace FishNet.Insthync.ResquestResponse
{
    public struct ResponseHandlerData
    {
        public uint RequestId { get; private set; }
        public ReqResHandler ReqResHandler { get; private set; }
        public NetworkConnection NetworkConnection { get; private set; }
        public Reader Reader { get; private set; }

        public ResponseHandlerData(uint requestId, ReqResHandler reqResHandler, NetworkConnection networkConnection, Reader reader)
        {
            RequestId = requestId;
            ReqResHandler = reqResHandler;
            NetworkConnection = networkConnection;
            Reader = reader;
        }
    }
}
