namespace Insthync.FishNet
{
    public interface IRequestHandlerInvoker
    {
        void InvokeRequest(RequestHandlerData requestHandler, RequestProceededDelegate responseProceedResult);
    }

    public struct RequestHandlerInvoker<TRequest, TResponse> : IRequestHandlerInvoker
        where TRequest : new()
        where TResponse : new()
    {
        private RequestDelegate<TRequest, TResponse> requestHandler;

        public RequestHandlerInvoker(RequestDelegate<TRequest, TResponse> requestHandler)
        {
            this.requestHandler = requestHandler;
        }

        public void InvokeRequest(RequestHandlerData requestHandlerData, RequestProceededDelegate responseProceedResult)
        {
            TRequest request = new TRequest();
            // TODO: Read the request
            if (requestHandler != null)
            {
                requestHandler.Invoke(requestHandlerData, request, (responseCode, response) =>
                {
                    responseProceedResult.Invoke(requestHandlerData.ConnectionId, requestHandlerData.RequestId, responseCode, response);
                });
            }
        }
    }
}