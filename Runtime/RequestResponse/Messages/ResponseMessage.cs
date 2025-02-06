using FishNet.Broadcast;

namespace FishNet.Insthync.ResquestResponse
{
    public struct ResponseMessage : IBroadcast
    {
        public uint requestId;
        public ResponseCode responseCode;
        public byte[] data;
    }
}