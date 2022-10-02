using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Client;
using FishNet.Serializing;
using UnityEngine;

namespace FishNet.Insthync.ResquestResponse
{
    public class RequestResponseManager : MonoBehaviour
    {
        [SerializeField]
        private NetworkManager networkManager;
        public NetworkManager NetworkManager => networkManager;

        public int clientRequestTimeoutInMilliseconds = 30000;
        public int serverRequestTimeoutInMilliseconds = 30000;

        private RequestResponseHandler _serverReqResHandler;
        private RequestResponseHandler _clientReqResHandler;

        private void Awake()
        {
            _serverReqResHandler = new RequestResponseHandler(this);
            _clientReqResHandler = new RequestResponseHandler(this);
        }

        private void Start()
        {
            networkManager.ServerManager.RegisterBroadcast<RequestMessage>(ServerRequestHandler);
            networkManager.ServerManager.RegisterBroadcast<ResponseMessage>(ServerResponseHandler);
            networkManager.ClientManager.RegisterBroadcast<RequestMessage>(ClientRequestHandler);
            networkManager.ClientManager.RegisterBroadcast<ResponseMessage>(ClientResponseHandler);
        }

        private void ServerRequestHandler(NetworkConnection networkConnection, RequestMessage msg)
        {
            _serverReqResHandler.ProceedRequest(networkConnection, msg);
        }

        private void ServerResponseHandler(NetworkConnection networkConnection, ResponseMessage msg)
        {
            _serverReqResHandler.ProceedResponse(networkConnection, msg);
        }

        private void ClientRequestHandler(RequestMessage msg)
        {
            _clientReqResHandler.ProceedRequest(null, msg);
        }

        private void ClientResponseHandler(ResponseMessage msg)
        {
            _clientReqResHandler.ProceedResponse(null, msg);
        }

        public bool ServerSendRequest<TRequest>(
            NetworkConnection networkConnection,
            ushort requestType,
            TRequest request,
            SerializerDelegate extraRequestSerializer = null,
            ResponseDelegate<object> responseHandler = null)
            where TRequest : new()
        {
            return _serverReqResHandler.CreateAndSendRequest(networkConnection, requestType, request, extraRequestSerializer, responseHandler, serverRequestTimeoutInMilliseconds);
        }

        public bool ClientSendRequest<TRequest>(
            ushort requestType,
            TRequest request,
            SerializerDelegate extraRequestSerializer = null,
            ResponseDelegate<object> responseHandler = null)
            where TRequest : new()
        {
            return _clientReqResHandler.CreateAndSendRequest(null, requestType, request, extraRequestSerializer, responseHandler, clientRequestTimeoutInMilliseconds);
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
