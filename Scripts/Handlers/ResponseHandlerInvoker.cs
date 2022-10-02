using LiteNetLib.Utils;
using System;

namespace Insthync.FishNet
{
    public interface IResponseHandlerInvoker
    {
        void InvokeResponse(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseDelegate<object> anotherResponseHandler);
        bool IsRequestTypeValid(Type type);
    }

    public struct ResponseHandlerInvoker<TRequest, TResponse> : IResponseHandlerInvoker
        where TRequest : new()
        where TResponse : new()
    {
        private ResponseDelegate<TResponse> responseDelegate;

        public ResponseHandlerInvoker(ResponseDelegate<TResponse> responseDelegate)
        {
            this.responseDelegate = responseDelegate;
        }

        public void InvokeResponse(ResponseHandlerData responseHandlerData, AckResponseCode responseCode, ResponseDelegate<object> anotherResponseHandler)
        {
            TResponse response = new TResponse();
            if (responseCode != AckResponseCode.Timeout &&
                responseCode != AckResponseCode.Unimplemented)
            {
                // TODO: Read the response
            }
            if (responseDelegate != null)
                responseDelegate.Invoke(responseHandlerData, responseCode, response);
            if (anotherResponseHandler != null)
                anotherResponseHandler.Invoke(responseHandlerData, responseCode, response);
        }

        public bool IsRequestTypeValid(Type type)
        {
            return typeof(TRequest) == type;
        }
    }
}
