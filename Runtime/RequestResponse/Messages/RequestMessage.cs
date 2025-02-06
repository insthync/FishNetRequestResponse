using FishNet.Broadcast;

namespace FishNet.Insthync.ResquestResponse
{
    public struct RequestMessage : IBroadcast
    {
        public ushort requestType;
        public uint requestId;
        public byte[] data;
    }
}