using FishNet.Connection;

namespace FishNet.Insthync.ResquestResponse
{
    public delegate void RequestProceededDelegate<TResponse>(NetworkConnection networkConnection, uint requestId, AckResponseCode responseCode, TResponse response, SerializerDelegate extraResponseSerializer);
}
