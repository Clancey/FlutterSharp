using System;
using System.Collections.Generic;
using System.Text.Json;
using Flutter.Logging;
using Flutter.Messages;

namespace Flutter.Internal
{
    /// <summary>
    /// Binary-protocol aware communicator that automatically selects encoding based on settings.
    /// Provides drop-in replacement for message sending with optional binary optimization.
    /// </summary>
    public static class BinaryCommunicator
    {
        /// <summary>
        /// Delegate to send binary data to Flutter. Set by platform implementation.
        /// </summary>
        public static Action<byte[]> SendBinaryData { get; set; }

        /// <summary>
        /// Callback when binary data is received from Flutter.
        /// </summary>
        public static Action<byte[]> OnBinaryDataReceived { get; set; }

        /// <summary>
        /// Sends an update message using the optimal encoding.
        /// </summary>
        public static void SendUpdate(string componentId, long address)
        {
            if (BinaryProtocol.IsEnabled && SendBinaryData != null)
            {
                var data = BinaryProtocol.EncodeUpdateMessage(componentId, address);
                SendBinaryData(data);
                FlutterSharpLogger.LogDebug("Sent binary update: {ComponentId}, {ByteCount} bytes", componentId, data.Length);
            }
            else
            {
                SendJsonUpdate(componentId, address);
            }
        }

        /// <summary>
        /// Sends a batched update message using the optimal encoding.
        /// </summary>
        public static void SendBatchedUpdate(List<UpdateEntry> updates)
        {
            if (BinaryProtocol.IsEnabled && SendBinaryData != null)
            {
                var data = BinaryProtocol.EncodeBatchedUpdate(updates);
                SendBinaryData(data);
                FlutterSharpLogger.LogDebug("Sent binary batch: {Count} updates, {ByteCount} bytes", updates.Count, data.Length);
            }
            else
            {
                SendJsonBatch(updates);
            }
        }

        /// <summary>
        /// Sends a disposal notification using the optimal encoding.
        /// </summary>
        public static void SendDisposed(string widgetId)
        {
            if (BinaryProtocol.IsEnabled && SendBinaryData != null)
            {
                var data = BinaryProtocol.EncodeDisposedMessage(widgetId);
                SendBinaryData(data);
                FlutterSharpLogger.LogDebug("Sent binary disposed: {WidgetId}, {ByteCount} bytes", widgetId, data.Length);
            }
            else
            {
                Communicator.SendDisposed(widgetId);
            }
        }

        /// <summary>
        /// Sends an error message using the optimal encoding.
        /// </summary>
        public static void SendError(string message, string stackTrace = null)
        {
            if (BinaryProtocol.IsEnabled && SendBinaryData != null)
            {
                var data = BinaryProtocol.EncodeErrorMessage(message, stackTrace);
                SendBinaryData(data);
            }
            else
            {
                if (Communicator.SendCommand != null)
                {
                    var errorMsg = new ErrorMessage
                    {
                        ErrorText = message,
                        StackTrace = stackTrace
                    };
                    var json = JsonSerializer.Serialize(errorMsg);
                    Communicator.SendCommand.Invoke((errorMsg.MessageType, json));
                }
            }
        }

        /// <summary>
        /// Sends a lifecycle message using the optimal encoding.
        /// </summary>
        public static void SendLifecycle(FlutterLifecycleState state)
        {
            if (BinaryProtocol.IsEnabled && SendBinaryData != null)
            {
                var data = BinaryProtocol.EncodeLifecycleMessage(state);
                SendBinaryData(data);
            }
            else
            {
                if (Communicator.SendCommand != null)
                {
                    var stateString = state switch
                    {
                        FlutterLifecycleState.Resumed => "resumed",
                        FlutterLifecycleState.Inactive => "inactive",
                        FlutterLifecycleState.Paused => "paused",
                        FlutterLifecycleState.Detached => "detached",
                        _ => "resumed"
                    };
                    var message = new LifecycleMessage
                    {
                        State = stateString
                    };
                    var json = JsonSerializer.Serialize(message);
                    Communicator.SendCommand.Invoke((message.MessageType, json));
                }
            }
        }

        /// <summary>
        /// Sends a scroll command using the optimal encoding.
        /// </summary>
        public static void SendScrollCommand(string controllerId, ScrollCommandType command, double offset, int durationMs = 0, string curve = null)
        {
            if (BinaryProtocol.IsEnabled && SendBinaryData != null)
            {
                var data = BinaryProtocol.EncodeScrollCommand(controllerId, command, offset, durationMs, curve);
                SendBinaryData(data);
            }
            else
            {
                if (Communicator.SendCommand != null)
                {
                    var message = new ScrollCommandMessage
                    {
                        ControllerId = controllerId,
                        Command = command == ScrollCommandType.JumpTo ? "jumpTo" : "animateTo",
                        Offset = offset,
                        DurationMs = durationMs,
                        Curve = curve ?? "easeInOut"
                    };
                    var json = JsonSerializer.Serialize(message);
                    Communicator.SendCommand.Invoke(("ScrollCommand", json));
                }
            }
        }

        /// <summary>
        /// Sends a state notify message using the optimal encoding.
        /// </summary>
        public static void SendStateNotify(string notifierId, object value, DateTime timestamp)
        {
            if (BinaryProtocol.IsEnabled && SendBinaryData != null)
            {
                var jsonValue = value is string s ? s : JsonSerializer.Serialize(value);
                var data = BinaryProtocol.EncodeStateNotify(notifierId, jsonValue, timestamp);
                SendBinaryData(data);
            }
            else
            {
                if (Communicator.SendCommand != null)
                {
                    var message = new StateNotifyMessage
                    {
                        NotifierId = notifierId,
                        Value = value,
                        Timestamp = timestamp.Ticks
                    };
                    var json = JsonSerializer.Serialize(message);
                    Communicator.SendCommand.Invoke((message.MessageType, json));
                }
            }
        }

        /// <summary>
        /// Sends a hot reload notification using the optimal encoding.
        /// </summary>
        public static void SendHotReloadNotification(bool success, string widgetType, int durationMs, string error = null)
        {
            if (BinaryProtocol.IsEnabled && SendBinaryData != null)
            {
                var data = BinaryProtocol.EncodeHotReloadNotification(success, widgetType, durationMs, error);
                SendBinaryData(data);
            }
            else
            {
                if (Communicator.SendCommand != null)
                {
                    var message = new HotReloadNotificationMessage
                    {
                        Success = success,
                        WidgetType = widgetType,
                        DurationMs = durationMs,
                        ErrorMessage = error
                    };
                    var json = JsonSerializer.Serialize(message);
                    Communicator.SendCommand.Invoke((message.MessageType, json));
                }
            }
        }

        /// <summary>
        /// Sends an async callback complete message using the optimal encoding.
        /// </summary>
        public static void SendAsyncCallbackComplete(string callbackId)
        {
            if (BinaryProtocol.IsEnabled && SendBinaryData != null)
            {
                var data = BinaryProtocol.EncodeAsyncCallbackComplete(callbackId);
                SendBinaryData(data);
            }
            else
            {
                if (Communicator.SendCommand != null)
                {
                    var payload = new { messageType = "AsyncCallbackComplete", callbackId };
                    var json = JsonSerializer.Serialize(payload);
                    Communicator.SendCommand.Invoke(("AsyncCallbackComplete", json));
                }
            }
        }

        /// <summary>
        /// Processes received binary data from Flutter.
        /// </summary>
        public static void ProcessReceivedBinaryData(byte[] data)
        {
            if (data == null || data.Length < 2) return;

            try
            {
                var (version, messageType) = BinaryProtocol.DecodeHeader(data);

                if (version != BinaryProtocol.ProtocolVersion)
                {
                    FlutterSharpLogger.LogWarning("Binary protocol version mismatch: expected {Expected}, got {Actual}",
                        BinaryProtocol.ProtocolVersion, version);
                    return;
                }

                switch (messageType)
                {
                    case BinaryProtocol.MessageTypes.ScrollUpdate:
                        HandleScrollUpdate(data);
                        break;
                    case BinaryProtocol.MessageTypes.Event:
                        HandleGestureEvent(data);
                        break;
                    default:
                        FlutterSharpLogger.LogDebug("Unknown binary message type: {MessageType}", messageType);
                        break;
                }
            }
            catch (Exception ex)
            {
                FlutterSharpLogger.LogError(ex, "Error processing binary data");
            }
        }

        private static void HandleScrollUpdate(byte[] data)
        {
            var (controllerId, offset, maxExtent, viewportDimension, eventType) =
                BinaryProtocol.DecodeScrollUpdate(data);

            // Delegate to FlutterManager's scroll handling
            FlutterManager.HandleBinaryScrollUpdate(controllerId, offset, maxExtent, viewportDimension, eventType);
        }

        private static void HandleGestureEvent(byte[] data)
        {
            var gestureEvent = BinaryProtocol.DecodeGestureEvent(data);
            // Gesture events are typically associated with specific widgets
            // This needs to be routed through the callback system
            FlutterSharpLogger.LogDebug("Received binary gesture event: {EventType}", gestureEvent.GetType().Name);
        }

        private static void SendJsonUpdate(string componentId, long address)
        {
            if (Communicator.SendCommand == null) return;

            var message = new UpdateMessage
            {
                ComponentId = componentId,
                Address = address
            };
            var json = JsonSerializer.Serialize(message);
            Communicator.SendCommand.Invoke((message.MessageType, json));
        }

        private static void SendJsonBatch(List<UpdateEntry> updates)
        {
            if (Communicator.SendCommand == null) return;

            var message = new BatchedUpdateMessage
            {
                Updates = updates
            };
            var json = JsonSerializer.Serialize(message);
            Communicator.SendCommand.Invoke((message.MessageType, json));
        }
    }
}
