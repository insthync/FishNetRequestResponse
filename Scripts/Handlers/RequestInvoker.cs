using FishNet.Connection;

namespace FishNet.Insthync.ResquestResponse
{
    public interface IRequestInvoker
    {
        void InvokeRequest(RequestHandlerData requestHandler);
    }

    public class RequestInvoker<TRequest, TResponse> : IRequestInvoker
        where TRequest : new()
        where TResponse : new()
    {
        private RequestResponseHandler handler;
        private RequestDelegate<TRequest, TResponse> requestHandler;

        public RequestInvoker(RequestResponseHandler handler, RequestDelegate<TRequest, TResponse> requestHandler)
        {
            this.handler = handler;
            this.requestHandler = requestHandler;
        }

        public void InvokeRequest(RequestHandlerData requestHandlerData)
        {
            TRequest request = new TRequest();
            if (requestHandlerData.Reader != null)
                request = requestHandlerData.Reader.Read<TRequest>();
            if (requestHandler != null)
                requestHandler.Invoke(requestHandlerData, request, (responseCode, response, extraResponseSerializer) => RequestProceeded(requestHandlerData.NetworkConnection, requestHandlerData.RequestId, responseCode, response, extraResponseSerializer));
        }

        /// <summary>
        /// Send response to the requester
        /// </summary>
        /// <param name="networkConnection"></param>
        /// <param name="requestId"></param>
        /// <param name="responseCode"></param>
        /// <param name="response"></param>
        /// <param name="extraResponseSerializer"></param>
        private void RequestProceeded(NetworkConnection networkConnection, uint requestId, AckResponseCode responseCode, TResponse response, SerializerDelegate extraResponseSerializer)
        {
            // Write response
            handler.Writer.Reset();
            handler.Writer.Write(response);
            if (extraResponseSerializer != null)
                extraResponseSerializer.Invoke(handler.Writer);
            ResponseMessage responseMessage = new ResponseMessage()
            {
                requestId = requestId,
                responseCode = responseCode,
                data = handler.Writer.GetArraySegment().ToArray(),
            };
            // Send response
            if (networkConnection == null)
                handler.Manager.NetworkManager.ClientManager.Broadcast(responseMessage);
            else
                handler.Manager.NetworkManager.ServerManager.Broadcast(networkConnection, responseMessage);
        }
    }
}