namespace Insthync.FishNet
{
    public delegate void RequestProceededDelegate(long connectionId, uint requestId, AckResponseCode responseCode, object response);
}
