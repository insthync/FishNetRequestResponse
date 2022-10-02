namespace FishNet.Insthync.ResquestResponse
{
    public interface IRequestInvoker
    {
        void InvokeRequest(RequestHandlerData requestHandler, RequestProceededDelegate responseProceedResult);
    }

    public struct RequestInvoker<TRequest, TResponse> : IRequestInvoker
        where TRequest : new()
        where TResponse : new()
    {
        private RequestDelegate<TRequest, TResponse> requestHandler;

        public RequestInvoker(RequestDelegate<TRequest, TResponse> requestHandler)
        {
            this.requestHandler = requestHandler;
        }

        public void InvokeRequest(RequestHandlerData requestHandlerData, RequestProceededDelegate responseProceedResult)
        {
            TRequest request = new TRequest();
            if (requestHandlerData.Reader != null)
                request = requestHandlerData.Reader.Read<TRequest>();
            if (requestHandler != null)
            {
                requestHandler.Invoke(requestHandlerData, request, (responseCode, response, extraResponseSerializer) => responseProceedResult.Invoke(requestHandlerData.NetworkConnection, requestHandlerData.RequestId, responseCode, response, extraResponseSerializer));
            }
        }
    }
}