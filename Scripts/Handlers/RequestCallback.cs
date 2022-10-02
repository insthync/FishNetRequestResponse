using FishNet.Serializing;

namespace FishNet.Insthync.ResquestResponse
{
    public struct RequestCallback
    {
        public uint RequestId { get; private set; }
        public ReqResHandler ReqResHandler { get; private set; }
        public IResponseInvoker ResponseInvoker { get; private set; }
        public ResponseDelegate<object> ResponseHandler { get; private set; }

        public RequestCallback(
            uint requestId,
            ReqResHandler reqResHandler,
            IResponseInvoker responseInvoker,
            ResponseDelegate<object> responseHandler)
        {
            RequestId = requestId;
            ReqResHandler = reqResHandler;
            ResponseInvoker = responseInvoker;
            ResponseHandler = responseHandler;
        }

        public void ResponseTimeout()
        {
            ResponseInvoker.InvokeResponse(new ResponseHandlerData(RequestId, ReqResHandler, -1, null), AckResponseCode.Timeout, ResponseHandler);
        }

        public void Response(long connectionId, Reader reader, AckResponseCode responseCode)
        {
            ResponseInvoker.InvokeResponse(new ResponseHandlerData(RequestId, ReqResHandler, connectionId, reader), responseCode, ResponseHandler);
        }
    }
}
