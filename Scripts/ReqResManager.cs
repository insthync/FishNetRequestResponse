using FishNet.Managing;
using UnityEngine;

namespace FishNet.Insthync.ResquestResponse
{
    public class ReqResManager : MonoBehaviour
    {
        [SerializeField]
        private NetworkManager networkManager;
        public NetworkManager NetworkManager => networkManager;

        [SerializeField]
        private ushort requestPacketId = 20;
        public ushort RequestPacketId => requestPacketId;

        [SerializeField]
        private ushort responsePacketId = 21;
        public ushort ResponsePacketId => responsePacketId;

        private ReqResHandler _serverReqResHandler;
        private ReqResHandler _clientReqResHandler;

        private void Awake()
        {
            _serverReqResHandler = new ReqResHandler(this);
            _clientReqResHandler = new ReqResHandler(this);
        }

        public void ServerSendRequest()
        {

        }

        public void ClientSendRequest()
        {

        }

        public void RegisterRequestToServer<TRequest, TResponse>(ushort reqType, RequestDelegate<TRequest, TResponse> requestHandler, ResponseDelegate<TResponse> responseHandler = null)
            where TRequest : new()
            where TResponse : new()
        {
            _serverReqResHandler.RegisterRequestHandler(reqType, requestHandler);
            _clientReqResHandler.RegisterResponseHandler<TRequest, TResponse>(reqType, responseHandler);
        }

        public void UnregisterRequestToServer(ushort reqType)
        {
            _serverReqResHandler.UnregisterRequestHandler(reqType);
            _clientReqResHandler.UnregisterResponseHandler(reqType);
        }

        public void RegisterRequestToClient<TRequest, TResponse>(ushort reqType, RequestDelegate<TRequest, TResponse> requestHandler, ResponseDelegate<TResponse> responseHandler = null)
            where TRequest : new()
            where TResponse : new()
        {
            _clientReqResHandler.RegisterRequestHandler(reqType, requestHandler);
            _serverReqResHandler.RegisterResponseHandler<TRequest, TResponse>(reqType, responseHandler);
        }

        public void UnregisterRequestToClient(ushort reqType)
        {
            _clientReqResHandler.UnregisterRequestHandler(reqType);
            _serverReqResHandler.UnregisterResponseHandler(reqType);
        }
    }
}
