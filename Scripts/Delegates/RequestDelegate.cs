namespace Insthync.FishNet
{
    public delegate void RequestDelegate<TRequest, TResponse>(RequestHandlerData requestHandler, TRequest request, RequestProceedResultDelegate<TResponse> responseProceedResult);
}
