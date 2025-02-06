using System;

namespace FishNet.Insthync.ResquestResponse
{
    public interface IResponseInvoker
    {
        void InvokeResponse(ResponseHandlerData responseHandlerData, ResponseCode responseCode, ResponseDelegate<object> responseHandler);
        bool IsRequestTypeValid(Type type);
    }

    public class ResponseInvoker<TRequest, TResponse> : IResponseInvoker
        where TRequest : new()
        where TResponse : new()
    {
        private ResponseDelegate<TResponse> _responseDelegate;

        public ResponseInvoker(ResponseDelegate<TResponse> responseDelegate)
        {
            _responseDelegate = responseDelegate;
        }

        public void InvokeResponse(ResponseHandlerData responseHandlerData, ResponseCode responseCode, ResponseDelegate<object> responseHandler)
        {
            TResponse response = new TResponse();
            if (responseCode != ResponseCode.Timeout &&
                responseCode != ResponseCode.Unimplemented)
            {
                if (responseHandlerData.Reader != null)
                    response = responseHandlerData.Reader.Read<TResponse>();
            }
            if (_responseDelegate != null)
                _responseDelegate.Invoke(responseHandlerData, responseCode, response);
            if (responseHandler != null)
                responseHandler.Invoke(responseHandlerData, responseCode, response);
        }

        public bool IsRequestTypeValid(Type type)
        {
            return typeof(TRequest) == type;
        }
    }
}
