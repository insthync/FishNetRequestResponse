using FishNet.Connection;
using FishNet.Serializing;

namespace FishNet.Insthync.ResquestResponse
{
    public struct RequestHandlerData
    {
        public ushort RequestType { get; private set; }
        public uint RequestId { get; private set; }
        public RequestResponseHandler ReqResHandler { get; private set; }
        public NetworkConnection NetworkConnection { get; private set; }
        public Reader Reader { get; private set; }

        public RequestHandlerData(ushort requestType, uint requestId, RequestResponseHandler reqResHandler, NetworkConnection networkConnection, Reader reader)
        {
            RequestType = requestType;
            RequestId = requestId;
            ReqResHandler = reqResHandler;
            NetworkConnection = networkConnection;
            Reader = reader;
        }
    }
}
