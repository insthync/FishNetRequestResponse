using FishNet.Serializing;

namespace Insthync.FishNet
{
    public struct RequestHandlerData
    {
        public ushort RequestType { get; private set; }
        public uint RequestId { get; private set; }
        public long ConnectionId { get; private set; }
        public Reader Reader { get; private set; }

        public RequestHandlerData(ushort requestType, uint requestId, long connectionId, Reader reader)
        {
            RequestType = requestType;
            RequestId = requestId;
            ConnectionId = connectionId;
            Reader = reader;
        }
    }
}
