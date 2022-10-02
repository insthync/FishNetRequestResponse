using System;

namespace FishNet.Insthync.ResquestResponse
{
    public interface IResponseInvoker
    {
        void InvokeResponse(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseDelegate<object> anotherResponseHandler);
        bool IsRequestTypeValid(Type type);
    }

    public struct ResponseInvoker<TRequest, TResponse> : IResponseInvoker
        where TRequest : new()
        where TResponse : new()
    {
        private ResponseDelegate<TResponse> responseDelegate;

        public ResponseInvoker(ResponseDelegate<TResponse> responseDelegate)
        {
            this.responseDelegate = responseDelegate;
        }

        public void InvokeResponse(ResponseHandlerData responseHandlerData, AckResponseCode responseCode, ResponseDelegate<object> anotherResponseHandler)
        {
            TResponse response = new TResponse();
            if (responseCode != AckResponseCode.Timeout &&
                responseCode != AckResponseCode.Unimplemented)
            {
                if (responseHandlerData.Reader != null)
                    response = responseHandlerData.Reader.Read<TResponse>();
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
