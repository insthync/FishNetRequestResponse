using FishNet.Connection;

namespace FishNet.Insthync.ResquestResponse
{
    public delegate void RequestProceededDelegate(NetworkConnection networkConnection, uint requestId, AckResponseCode responseCode, object response, SerializerDelegate extraResponseSerializer);
}
