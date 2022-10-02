namespace Insthync.FishNet
{
    public delegate void ResponseDelegate<TResponse>(ResponseHandlerData requestHandler, AckResponseCode responseCode, TResponse response);
}
