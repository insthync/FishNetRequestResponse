namespace FishNet.Insthync.ResquestResponse
{
    public delegate void RequestDelegate<TRequest, TResponse>(RequestHandlerData requestHandler, TRequest request, RequestProceedResultDelegate<TResponse> result);
}
