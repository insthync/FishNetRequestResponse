using FishNet.Broadcast;

namespace FishNet.Insthync.ResquestResponse
{
    public struct ResponseMessage : IBroadcast
    {
        public uint requestId;
        public AckResponseCode responseCode;
        public byte[] data;
    }
}