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
        private RequestResponseHandler _handler;
        private RequestDelegate<TRequest, TResponse> _requestHandler;

        public RequestInvoker(RequestResponseHandler handler, RequestDelegate<TRequest, TResponse> requestHandler)
        {
            _handler = handler;
            _requestHandler = requestHandler;
        }

        public void InvokeRequest(RequestHandlerData requestHandlerData)
        {
            TRequest request = new TRequest();
            if (requestHandlerData.Reader != null)
                request = requestHandlerData.Reader.Read<TRequest>();
            if (_requestHandler != null)
                _requestHandler.Invoke(requestHandlerData, request, (responseCode, response, extraResponseSerializer) => RequestProceeded(requestHandlerData.NetworkConnection, requestHandlerData.RequestId, responseCode, response, extraResponseSerializer));
        }

        /// <summary>
        /// Send response to the requester
        /// </summary>
        /// <param name="networkConnection"></param>
        /// <param name="requestId"></param>
        /// <param name="responseCode"></param>
        /// <param name="response"></param>
        /// <param name="extraResponseSerializer"></param>
        private void RequestProceeded(NetworkConnection networkConnection, uint requestId, ResponseCode responseCode, TResponse response, SerializerDelegate extraResponseSerializer)
        {
            // Write response
            _handler.Writer.Clear();
            _handler.Writer.Write(response);
            if (extraResponseSerializer != null)
                extraResponseSerializer.Invoke(_handler.Writer);
            ResponseMessage responseMessage = new ResponseMessage()
            {
                requestId = requestId,
                responseCode = responseCode,
                data = _handler.Writer.GetArraySegment().ToArray(),
            };
            // Send response
            if (networkConnection == null)
                _handler.Manager.NetworkManager.ClientManager.Broadcast(responseMessage);
            else
                _handler.Manager.NetworkManager.ServerManager.Broadcast(networkConnection, responseMessage);
        }
    }
}