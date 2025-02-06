using FishNet.Connection;
using FishNet.Serializing;

namespace FishNet.Insthync.ResquestResponse
{
    public class ResponseHandlerData
    {
        public uint RequestId { get; private set; }
        public RequestResponseHandler ReqResHandler { get; private set; }
        public NetworkConnection NetworkConnection { get; private set; }
        public Reader Reader { get; private set; }

        public ResponseHandlerData(uint requestId, RequestResponseHandler reqResHandler, NetworkConnection networkConnection, Reader reader)
        {
            RequestId = requestId;
            ReqResHandler = reqResHandler;
            NetworkConnection = networkConnection;
            Reader = reader;
        }
    }
}
