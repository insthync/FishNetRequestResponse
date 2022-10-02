using FishNet.Managing;
using LiteNetLib.Utils;
using UnityEngine;

namespace Insthync.FishNet
{
    public class ReqResManager : MonoBehaviour
    {
        public NetworkManager networkManager;
        private readonly ReqResHandler _serverReqResHandler = new ReqResHandler();
        private readonly ReqResHandler _clientReqResHandler = new ReqResHandler();

        public void ServerSendRequest()
        {

        }

        public void ClientSendRequest()
        {
        }

        public void RegisterRequestToServer<TRequest, TResponse>(ushort reqType, RequestDelegate<TRequest, TResponse> requestHandler, ResponseDelegate<TResponse> responseHandler = null)
            where TRequest : INetSerializable, new()
            where TResponse : INetSerializable, new()
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
            where TRequest : INetSerializable, new()
            where TResponse : INetSerializable, new()
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
