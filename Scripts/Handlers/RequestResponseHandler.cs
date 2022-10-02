using FishNet.Connection;
using FishNet.Serializing;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FishNet.Insthync.ResquestResponse
{
    // TODO: Implement better logger
    public class RequestResponseHandler
    {
        public readonly RequestResponseManager Manager;
        public readonly Writer Writer = new Writer();
        protected readonly Dictionary<ushort, IRequestInvoker> requestInvokers = new Dictionary<ushort, IRequestInvoker>();
        protected readonly Dictionary<ushort, IResponseInvoker> responseInvokers = new Dictionary<ushort, IResponseInvoker>();
        protected readonly ConcurrentDictionary<uint, RequestCallback> requestCallbacks = new ConcurrentDictionary<uint, RequestCallback>();
        protected uint nextRequestId;

        public RequestResponseHandler(RequestResponseManager manager)
        {
            this.Manager = manager;
        }

        /// <summary>
        /// Create new request callback with a new request ID
        /// </summary>
        /// <param name="responseInvoker"></param>
        /// <param name="responseHandler"></param>
        /// <returns></returns>
        private uint CreateRequest(IResponseInvoker responseInvoker, ResponseDelegate<object> responseHandler)
        {
            uint requestId = nextRequestId++;
            // Get response callback by request type
            requestCallbacks.TryAdd(requestId, new RequestCallback(requestId, this, responseInvoker, responseHandler));
            return requestId;
        }

        /// <summary>
        /// Delay and do something when request timeout
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="millisecondsTimeout"></param>
        private async void HandleRequestTimeout(uint requestId, int millisecondsTimeout)
        {
            if (millisecondsTimeout > 0)
            {
                await Task.Delay(millisecondsTimeout);
                if (requestCallbacks.TryRemove(requestId, out RequestCallback callback))
                    callback.ResponseTimeout();
            }
        }

        /// <summary>
        /// Create a new request and send to target
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="networkConnection"></param>
        /// <param name="requestType"></param>
        /// <param name="request"></param>
        /// <param name="extraRequestSerializer"></param>
        /// <param name="responseHandler"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public bool CreateAndSendRequest<TRequest>(
            NetworkConnection networkConnection,
            ushort requestType,
            TRequest request,
            SerializerDelegate extraRequestSerializer,
            ResponseDelegate<object> responseHandler,
            int millisecondsTimeout)
            where TRequest : new()
        {
            if (!responseInvokers.ContainsKey(requestType))
            {
                responseHandler.Invoke(new ResponseHandlerData(nextRequestId++, this, null, null), AckResponseCode.Unimplemented, EmptyMessage.Value);
                Debug.LogError($"Cannot create request. Request type: {requestType} not registered.");
                return false;
            }
            if (!responseInvokers[requestType].IsRequestTypeValid(typeof(TRequest)))
            {
                responseHandler.Invoke(new ResponseHandlerData(nextRequestId++, this, null, null), AckResponseCode.Unimplemented, EmptyMessage.Value);
                Debug.LogError($"Cannot create request. Request type: {requestType}, {typeof(TRequest)} is not valid message type.");
                return false;
            }
            // Create request
            uint requestId = CreateRequest(responseInvokers[requestType], responseHandler);
            HandleRequestTimeout(requestId, millisecondsTimeout);
            // Write request
            Writer.Reset();
            Writer.Write(request);
            if (extraRequestSerializer != null)
                extraRequestSerializer.Invoke(Writer);
            RequestMessage requestMessage = new RequestMessage()
            {
                requestType = requestType,
                requestId = requestId,
                data = Writer.GetArraySegment().ToArray(),
            };
            // Send request
            if (networkConnection == null)
                Manager.NetworkManager.ClientManager.Broadcast(requestMessage);
            else
                Manager.NetworkManager.ServerManager.Broadcast(networkConnection, requestMessage);
            return true;
        }

        /// <summary>
        /// Proceed request which reveived from server or client
        /// </summary>
        /// <param name="networkConnection"></param>
        /// <param name="requestMessage"></param>
        public void ProceedRequest(NetworkConnection networkConnection, RequestMessage requestMessage)
        {
            ushort requestType = requestMessage.requestType;
            uint requestId = requestMessage.requestId;
            if (!requestInvokers.ContainsKey(requestType))
            {
                // No request-response handler
                ResponseMessage responseMessage = new ResponseMessage()
                {
                    requestId = requestId,
                    responseCode = AckResponseCode.Unimplemented,
                };
                if (networkConnection == null)
                    Manager.NetworkManager.ClientManager.Broadcast(responseMessage);
                else
                    Manager.NetworkManager.ServerManager.Broadcast(networkConnection, responseMessage);
                Debug.LogError($"Cannot proceed request {requestType} not registered.");
                return;
            }
            // Invoke request and create response
            requestInvokers[requestType].InvokeRequest(new RequestHandlerData(requestType, requestId, this, networkConnection, new Reader(requestMessage.data, Manager.NetworkManager)));
        }

        /// <summary>
        /// Proceed response which reveived from server or client
        /// </summary>
        /// <param name="networkConnection"></param>
        /// <param name="responseMessage"></param>
        public void ProceedResponse(NetworkConnection networkConnection, ResponseMessage responseMessage)
        {
            uint requestId = responseMessage.requestId;
            AckResponseCode responseCode = responseMessage.responseCode;
            if (requestCallbacks.ContainsKey(requestId))
            {
                requestCallbacks[requestId].Response(networkConnection, new Reader(responseMessage.data, Manager.NetworkManager), responseCode);
                requestCallbacks.TryRemove(requestId, out _);
            }
        }

        /// <summary>
        /// Register request handler which will read request message and response to requester peer
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="requestType"></param>
        /// <param name="handlerDelegate"></param>
        public void RegisterRequestHandler<TRequest, TResponse>(
            ushort requestType,
            RequestDelegate<TRequest, TResponse> handlerDelegate)
            where TRequest : new()
            where TResponse : new()
        {
            requestInvokers[requestType] = new RequestInvoker<TRequest, TResponse>(this, handlerDelegate);
        }

        public void UnregisterRequestHandler(ushort requestType)
        {
            requestInvokers.Remove(requestType);
        }

        /// <summary>
        /// Register response handler which will read response message and do something by requester
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="requestType"></param>
        /// <param name="handlerDelegate"></param>
        public void RegisterResponseHandler<TRequest, TResponse>(
            ushort requestType,
            ResponseDelegate<TResponse> handlerDelegate = null)
            where TRequest : new()
            where TResponse : new()
        {
            responseInvokers[requestType] = new ResponseInvoker<TRequest, TResponse>(handlerDelegate);
        }

        public void UnregisterResponseHandler(ushort requestType)
        {
            responseInvokers.Remove(requestType);
        }
    }
}
