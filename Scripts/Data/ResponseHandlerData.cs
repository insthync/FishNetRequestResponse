using FishNet.Serializing;

namespace Insthync.FishNet
{
    public struct ResponseHandlerData
    {
        public uint RequestId { get; private set; }
        public long ConnectionId { get; private set; }
        public Reader Reader { get; private set; }

        public ResponseHandlerData(uint requestId, long connectionId, Reader reader)
        {
            RequestId = requestId;
            ConnectionId = connectionId;
            Reader = reader;
        }
    }
}
