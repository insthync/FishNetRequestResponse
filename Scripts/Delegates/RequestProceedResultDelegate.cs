namespace Insthync.FishNet
{
    public delegate void RequestProceedResultDelegate<TResponse>(AckResponseCode responseCode, TResponse response);
}
