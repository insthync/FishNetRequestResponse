using Insthync.FishNet;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Insthync.FishNet
{
    public class ReqResHandler : MonoBehaviour
    {
        protected readonly Dictionary<ushort, IRequestHandlerInvoker> requestHandlerInvokers = new Dictionary<ushort, IRequestHandlerInvoker>();
        protected readonly Dictionary<ushort, IResponseHandlerInvoker> responseHandlerInvokers = new Dictionary<ushort, IResponseHandlerInvoker>();

        private uint CreateRequest(
            LiteNetLibResponseHandler responseHandler,
            ResponseDelegate<INetSerializable> responseDelegate,
            int millisecondsTimeout)
        {
            uint requestId = nextRequestId++;
            // Get response callback by request type
            requestCallbacks.TryAdd(requestId, new LiteNetLibRequestCallback(requestId, this, responseHandler, responseDelegate));
            RequestTimeout(requestId, millisecondsTimeout).Forget();
            return requestId;
        }

        private async UniTaskVoid RequestTimeout(uint requestId, int millisecondsTimeout)
        {
            if (millisecondsTimeout > 0)
            {
                await UniTask.Delay(millisecondsTimeout);
                LiteNetLibRequestCallback callback;
                if (requestCallbacks.TryRemove(requestId, out callback))
                    callback.ResponseTimeout();
            }
        }

        protected bool CreateAndWriteRequest<TRequest>(
            NetDataWriter writer,
            ushort requestType,
            TRequest request,
            ResponseDelegate<INetSerializable> responseDelegate,
            int millisecondsTimeout,
            SerializerDelegate extraRequestSerializer)
            where TRequest : INetSerializable, new()
        {
            if (!responseHandlers.ContainsKey(requestType))
            {
                responseDelegate.Invoke(new ResponseHandlerData(nextRequestId++, this, -1, null), AckResponseCode.Unimplemented, EmptyMessage.Value);
                Logging.LogError($"Cannot create request. Request type: {requestType} not registered.");
                return false;
            }
            if (!responseHandlers[requestType].IsRequestTypeValid(typeof(TRequest)))
            {
                responseDelegate.Invoke(new ResponseHandlerData(nextRequestId++, this, -1, null), AckResponseCode.Unimplemented, EmptyMessage.Value);
                Logging.LogError($"Cannot create request. Request type: {requestType}, {typeof(TRequest)} is not valid message type.");
                return false;
            }
            // Create request
            uint requestId = CreateRequest(responseHandlers[requestType], responseDelegate, millisecondsTimeout);
            // Write request
            writer.Reset();
            writer.PutPackedUShort(RequestMessageType);
            writer.PutPackedUShort(requestType);
            writer.PutPackedUInt(requestId);
            writer.Put(request);
            if (extraRequestSerializer != null)
                extraRequestSerializer.Invoke(writer);
            return true;
        }

        private void ProceedRequest(
            long connectionId,
            NetDataReader reader)
        {
            ushort requestType = reader.GetPackedUShort();
            uint requestId = reader.GetPackedUInt();
            if (!requestHandlers.ContainsKey(requestType))
            {
                // No request-response handler
                RequestProceeded(connectionId, requestId, AckResponseCode.Unimplemented, EmptyMessage.Value, null);
                Logging.LogError($"Cannot proceed request {requestType} not registered.");
                return;
            }
            // Invoke request and create response
            requestHandlers[requestType].InvokeRequest(new RequestHandlerData(requestType, requestId, this, connectionId, reader), RequestProceeded);
        }

        private void RequestProceeded(long connectionId, uint requestId, AckResponseCode responseCode, INetSerializable response, SerializerDelegate responseSerializer)
        {
            // Write response
            Writer.Reset();
            Writer.PutPackedUShort(ResponseMessageType);
            Writer.PutPackedUInt(requestId);
            Writer.PutValue(responseCode);
            Writer.Put(response);
            if (responseSerializer != null)
                responseSerializer.Invoke(Writer);
            // Send response
            SendMessage(connectionId, 0, DeliveryMethod.ReliableUnordered, Writer);
        }

        private void ProceedResponse(long connectionId, NetDataReader reader)
        {
            uint requestId = reader.GetPackedUInt();
            AckResponseCode responseCode = reader.GetValue<AckResponseCode>();
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
            where TRequest : INetSerializable, new()
            where TResponse : INetSerializable, new()
        {
            requestHandlerInvokers[requestType] = new RequestHandlerInvoker<TRequest, TResponse>(handlerDelegate);
        }

        public void UnregisterRequestHandler(ushort requestType)
        {
            requestHandlerInvokers.Remove(requestType);
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
            where TRequest : INetSerializable, new()
            where TResponse : INetSerializable, new()
        {
            responseHandlerInvokers[requestType] = new ResponseHandlerInvoker<TRequest, TResponse>(handlerDelegate);
        }

        public void UnregisterResponseHandler(ushort requestType)
        {
            responseHandlerInvokers.Remove(requestType);
        }
    }
}
