using FishNet.Connection;
using FishNet.Serializing;
using FishNet.Transporting;
using LiteNetLib;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FishNet.Insthync.ResquestResponse
{
    // TODO: Implement better logger
    public class ReqResHandler
    {
        protected readonly ReqResManager manager;
        protected readonly Writer Writer = new Writer();
        protected readonly Dictionary<ushort, IRequestInvoker> requestInvokers = new Dictionary<ushort, IRequestInvoker>();
        protected readonly Dictionary<ushort, IResponseInvoker> responseInvokers = new Dictionary<ushort, IResponseInvoker>();
        protected readonly ConcurrentDictionary<uint, RequestCallback> requestCallbacks = new ConcurrentDictionary<uint, RequestCallback>();
        protected uint nextRequestId;

        public ReqResHandler(ReqResManager manager)
        {
            this.manager = manager;
        }

        private uint CreateRequest(
            IResponseInvoker responseInvoker,
            ResponseDelegate<object> responseDelegate,
            int millisecondsTimeout)
        {
            uint requestId = nextRequestId++;
            // Get response callback by request type
            requestCallbacks.TryAdd(requestId, new RequestCallback(requestId, this, responseInvoker, responseDelegate));
            RequestTimeout(requestId, millisecondsTimeout);
            return requestId;
        }

        private async void RequestTimeout(uint requestId, int millisecondsTimeout)
        {
            if (millisecondsTimeout > 0)
            {
                await Task.Delay(millisecondsTimeout);
                if (requestCallbacks.TryRemove(requestId, out RequestCallback callback))
                    callback.ResponseTimeout();
            }
        }

        protected bool CreateAndWriteRequest<TRequest>(
            Writer writer,
            ushort requestType,
            TRequest request,
            ResponseDelegate<object> responseDelegate,
            int millisecondsTimeout,
            SerializerDelegate extraRequestSerializer)
            where TRequest : new()
        {
            if (!responseInvokers.ContainsKey(requestType))
            {
                responseDelegate.Invoke(new ResponseHandlerData(nextRequestId++, this, -1, null), AckResponseCode.Unimplemented, EmptyMessage.Value);
                Debug.LogError($"Cannot create request. Request type: {requestType} not registered.");
                return false;
            }
            if (!responseInvokers[requestType].IsRequestTypeValid(typeof(TRequest)))
            {
                responseDelegate.Invoke(new ResponseHandlerData(nextRequestId++, this, -1, null), AckResponseCode.Unimplemented, EmptyMessage.Value);
                Debug.LogError($"Cannot create request. Request type: {requestType}, {typeof(TRequest)} is not valid message type.");
                return false;
            }
            // Create request
            uint requestId = CreateRequest(responseInvokers[requestType], responseDelegate, millisecondsTimeout);
            // Write request
            writer.Reset();
            writer.Write(manager.RequestPacketId);
            writer.Write(requestType);
            writer.Write(requestId);
            writer.Write(request);
            if (extraRequestSerializer != null)
                extraRequestSerializer.Invoke(writer);
            return true;
        }

        private void ProceedRequest(NetworkConnection networkConnection, Reader reader)
        {
            ushort requestType = reader.ReadUInt16();
            uint requestId = reader.ReadUInt32();
            if (!requestInvokers.ContainsKey(requestType))
            {
                // No request-response handler
                RequestProceeded(networkConnection, requestId, AckResponseCode.Unimplemented, EmptyMessage.Value, null);
                Debug.LogError($"Cannot proceed request {requestType} not registered.");
                return;
            }
            // Invoke request and create response
            requestInvokers[requestType].InvokeRequest(new RequestHandlerData(requestType, requestId, this, networkConnection, reader), RequestProceeded);
        }

        private void RequestProceeded(NetworkConnection networkConnection, uint requestId, AckResponseCode responseCode, object response, SerializerDelegate extraResponseSerializer)
        {
            // Write response
            Writer.Reset();
            Writer.Write(manager.ResponsePacketId);
            Writer.Write(requestId);
            Writer.Write(responseCode);
            Writer.Write(response);
            if (extraResponseSerializer != null)
                extraResponseSerializer.Invoke(Writer);
            // Send response
            if (networkConnection == null)
                manager.NetworkManager.TransportManager.SendToServer((byte)Channel.Reliable, Writer.GetArraySegment());
            else
                manager.NetworkManager.TransportManager.SendToClient((byte)Channel.Reliable, Writer.GetArraySegment(), networkConnection);
        }

        private void ProceedResponse(long connectionId, Reader reader)
        {
            uint requestId = reader.ReadUInt32();
            AckResponseCode responseCode = (AckResponseCode)reader.ReadByte();
            if (requestCallbacks.ContainsKey(requestId))
            {
                requestCallbacks[requestId].Response(connectionId, reader, responseCode);
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
            requestInvokers[requestType] = new RequestInvoker<TRequest, TResponse>(handlerDelegate);
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
