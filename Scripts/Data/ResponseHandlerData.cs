using FishNet.Serializing;

namespace FishNet.Insthync.ResquestResponse
{
    public struct ResponseHandlerData
    {
        public uint RequestId { get; private set; }
        public ReqResHandler ReqResHandler { get; private set; }
        public long ConnectionId { get; private set; }
        public Reader Reader { get; private set; }

        public ResponseHandlerData(uint requestId, ReqResHandler reqResHandler, long connectionId, Reader reader)
        {
            RequestId = requestId;
            ReqResHandler = reqResHandler;
            ConnectionId = connectionId;
            Reader = reader;
        }
    }
}
