namespace FishNet.Insthync.ResquestResponse
{
    public struct AsyncResponseData<TResponse>
    {
        public ResponseHandlerData RequestHandler { get; private set; }
        public AckResponseCode ResponseCode { get; private set; }
        public TResponse Response { get; private set; }
        public bool IsSuccess { get { return ResponseCode == AckResponseCode.Success; } }

        public AsyncResponseData(ResponseHandlerData requestHandler, AckResponseCode responseCode, TResponse response)
        {
            RequestHandler = requestHandler;
            ResponseCode = responseCode;
            Response = response;
        }
    }
}
