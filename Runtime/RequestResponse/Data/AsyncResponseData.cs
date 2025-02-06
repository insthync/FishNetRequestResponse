namespace FishNet.Insthync.ResquestResponse
{
    public struct AsyncResponseData<TResponse>
    {
        public ResponseHandlerData RequestHandler { get; private set; }
        public ResponseCode ResponseCode { get; private set; }
        public TResponse Response { get; private set; }
        public bool IsSuccess { get { return ResponseCode == ResponseCode.Success; } }

        public AsyncResponseData(ResponseHandlerData requestHandler, ResponseCode responseCode, TResponse response)
        {
            RequestHandler = requestHandler;
            ResponseCode = responseCode;
            Response = response;
        }
    }
}
