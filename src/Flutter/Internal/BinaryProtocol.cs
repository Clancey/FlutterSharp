using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Flutter.Logging;

namespace Flutter.Internal
{
    /// <summary>
    /// Binary protocol for high-performance message encoding/decoding between C# and Dart.
    /// Significantly reduces message size and CPU overhead compared to JSON.
    /// </summary>
    public static class BinaryProtocol
    {
        /// <summary>
        /// Protocol version for compatibility checking.
        /// </summary>
        public const byte ProtocolVersion = 1;

        /// <summary>
        /// Gets or sets whether binary protocol is enabled.
        /// When disabled, falls back to JSON serialization.
        /// </summary>
        public static bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Message type identifiers for binary protocol.
        /// </summary>
        public static class MessageTypes
        {
            public const byte UpdateComponent = 0x01;
            public const byte BatchedUpdate = 0x02;
            public const byte Event = 0x03;
            public const byte StateNotify = 0x04;
            public const byte Disposed = 0x05;
            public const byte Error = 0x06;
            public const byte Lifecycle = 0x07;
            public const byte MemoryWarning = 0x08;
            public const byte ScrollUpdate = 0x09;
            public const byte ScrollCommand = 0x0A;
            public const byte HotReload = 0x0B;
            public const byte ContainerSize = 0x0C;
            public const byte DartException = 0x0D;
            public const byte PartialUpdate = 0x0E;
            public const byte AsyncCallbackComplete = 0x0F;
        }

        // Statistics
        private static long _totalBytesSent = 0;
        private static long _totalBytesReceived = 0;
        private static long _messagesSent = 0;
        private static long _messagesReceived = 0;
        private static long _compressionSavings = 0;

        /// <summary>
        /// Gets protocol statistics.
        /// </summary>
        public static BinaryProtocolStats GetStats() => new BinaryProtocolStats
        {
            TotalBytesSent = _totalBytesSent,
            TotalBytesReceived = _totalBytesReceived,
            MessagesSent = _messagesSent,
            MessagesReceived = _messagesReceived,
            CompressionSavings = _compressionSavings,
            IsEnabled = IsEnabled
        };

        /// <summary>
        /// Resets protocol statistics.
        /// </summary>
        public static void ResetStats()
        {
            _totalBytesSent = 0;
            _totalBytesReceived = 0;
            _messagesSent = 0;
            _messagesReceived = 0;
            _compressionSavings = 0;
        }

        /// <summary>
        /// Encodes an UpdateMessage to binary format.
        /// Format: [Version:1][Type:1][ComponentIdLen:2][ComponentId:N][Address:8]
        /// </summary>
        public static byte[] EncodeUpdateMessage(string componentId, long address)
        {
            var componentIdBytes = Encoding.UTF8.GetBytes(componentId ?? string.Empty);
            var buffer = new byte[1 + 1 + 2 + componentIdBytes.Length + 8];
            var offset = 0;

            buffer[offset++] = ProtocolVersion;
            buffer[offset++] = MessageTypes.UpdateComponent;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset), (ushort)componentIdBytes.Length);
            offset += 2;
            componentIdBytes.CopyTo(buffer.AsSpan(offset));
            offset += componentIdBytes.Length;
            BinaryPrimitives.WriteInt64LittleEndian(buffer.AsSpan(offset), address);

            _totalBytesSent += buffer.Length;
            _messagesSent++;

            // Estimate JSON size for compression savings: {"componentId":"X","address":Y,"messageType":"UpdateComponent"}
            var estimatedJsonSize = 60 + componentId.Length + address.ToString().Length;
            _compressionSavings += estimatedJsonSize - buffer.Length;

            return buffer;
        }

        /// <summary>
        /// Encodes a BatchedUpdateMessage to binary format.
        /// Format: [Version:1][Type:1][Count:4][Updates:N*12]
        /// Each Update: [ComponentIdLen:2][ComponentId:N][Address:8]
        /// </summary>
        public static byte[] EncodeBatchedUpdate(List<UpdateEntry> updates)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

            writer.Write(ProtocolVersion);
            writer.Write(MessageTypes.BatchedUpdate);
            writer.Write(updates.Count);

            foreach (var update in updates)
            {
                var componentIdBytes = Encoding.UTF8.GetBytes(update.ComponentId ?? string.Empty);
                writer.Write((ushort)componentIdBytes.Length);
                writer.Write(componentIdBytes);
                writer.Write(update.Address);
            }

            var buffer = ms.ToArray();
            _totalBytesSent += buffer.Length;
            _messagesSent++;

            return buffer;
        }

        /// <summary>
        /// Encodes a DisposedMessage to binary format.
        /// Format: [Version:1][Type:1][WidgetIdLen:2][WidgetId:N]
        /// </summary>
        public static byte[] EncodeDisposedMessage(string widgetId)
        {
            var widgetIdBytes = Encoding.UTF8.GetBytes(widgetId ?? string.Empty);
            var buffer = new byte[1 + 1 + 2 + widgetIdBytes.Length];
            var offset = 0;

            buffer[offset++] = ProtocolVersion;
            buffer[offset++] = MessageTypes.Disposed;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset), (ushort)widgetIdBytes.Length);
            offset += 2;
            widgetIdBytes.CopyTo(buffer.AsSpan(offset));

            _totalBytesSent += buffer.Length;
            _messagesSent++;

            return buffer;
        }

        /// <summary>
        /// Encodes an ErrorMessage to binary format.
        /// Format: [Version:1][Type:1][MessageLen:2][Message:N][StackTraceLen:2][StackTrace:N]
        /// </summary>
        public static byte[] EncodeErrorMessage(string message, string stackTrace = null)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message ?? string.Empty);
            var stackTraceBytes = Encoding.UTF8.GetBytes(stackTrace ?? string.Empty);
            var buffer = new byte[1 + 1 + 2 + messageBytes.Length + 2 + stackTraceBytes.Length];
            var offset = 0;

            buffer[offset++] = ProtocolVersion;
            buffer[offset++] = MessageTypes.Error;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset), (ushort)messageBytes.Length);
            offset += 2;
            messageBytes.CopyTo(buffer.AsSpan(offset));
            offset += messageBytes.Length;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset), (ushort)stackTraceBytes.Length);
            offset += 2;
            stackTraceBytes.CopyTo(buffer.AsSpan(offset));

            _totalBytesSent += buffer.Length;
            _messagesSent++;

            return buffer;
        }

        /// <summary>
        /// Encodes a LifecycleMessage to binary format.
        /// Format: [Version:1][Type:1][State:1]
        /// </summary>
        public static byte[] EncodeLifecycleMessage(FlutterLifecycleState state)
        {
            var buffer = new byte[3];
            buffer[0] = ProtocolVersion;
            buffer[1] = MessageTypes.Lifecycle;
            buffer[2] = (byte)state;

            _totalBytesSent += buffer.Length;
            _messagesSent++;

            return buffer;
        }

        /// <summary>
        /// Encodes a ScrollCommandMessage to binary format.
        /// Format: [Version:1][Type:1][ControllerIdLen:2][ControllerId:N][Command:1][Offset:8][DurationMs:4][CurveLen:2][Curve:N]
        /// </summary>
        public static byte[] EncodeScrollCommand(string controllerId, ScrollCommandType command, double offset, int durationMs = 0, string curve = null)
        {
            var controllerIdBytes = Encoding.UTF8.GetBytes(controllerId ?? string.Empty);
            var curveBytes = Encoding.UTF8.GetBytes(curve ?? string.Empty);
            var buffer = new byte[1 + 1 + 2 + controllerIdBytes.Length + 1 + 8 + 4 + 2 + curveBytes.Length];
            var pos = 0;

            buffer[pos++] = ProtocolVersion;
            buffer[pos++] = MessageTypes.ScrollCommand;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(pos), (ushort)controllerIdBytes.Length);
            pos += 2;
            controllerIdBytes.CopyTo(buffer.AsSpan(pos));
            pos += controllerIdBytes.Length;
            buffer[pos++] = (byte)command;
            BinaryPrimitives.WriteDoubleLittleEndian(buffer.AsSpan(pos), offset);
            pos += 8;
            BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(pos), durationMs);
            pos += 4;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(pos), (ushort)curveBytes.Length);
            pos += 2;
            curveBytes.CopyTo(buffer.AsSpan(pos));

            _totalBytesSent += buffer.Length;
            _messagesSent++;

            return buffer;
        }

        /// <summary>
        /// Encodes a StateNotifyMessage to binary format.
        /// Format: [Version:1][Type:1][NotifierIdLen:2][NotifierId:N][ValueLen:4][Value:N][Timestamp:8]
        /// </summary>
        public static byte[] EncodeStateNotify(string notifierId, string jsonValue, DateTime timestamp)
        {
            var notifierIdBytes = Encoding.UTF8.GetBytes(notifierId ?? string.Empty);
            var valueBytes = Encoding.UTF8.GetBytes(jsonValue ?? string.Empty);
            var buffer = new byte[1 + 1 + 2 + notifierIdBytes.Length + 4 + valueBytes.Length + 8];
            var pos = 0;

            buffer[pos++] = ProtocolVersion;
            buffer[pos++] = MessageTypes.StateNotify;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(pos), (ushort)notifierIdBytes.Length);
            pos += 2;
            notifierIdBytes.CopyTo(buffer.AsSpan(pos));
            pos += notifierIdBytes.Length;
            BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(pos), valueBytes.Length);
            pos += 4;
            valueBytes.CopyTo(buffer.AsSpan(pos));
            pos += valueBytes.Length;
            BinaryPrimitives.WriteInt64LittleEndian(buffer.AsSpan(pos), timestamp.Ticks);

            _totalBytesSent += buffer.Length;
            _messagesSent++;

            return buffer;
        }

        /// <summary>
        /// Encodes a HotReloadNotificationMessage to binary format.
        /// Format: [Version:1][Type:1][Success:1][WidgetTypeLen:2][WidgetType:N][DurationMs:4][ErrorLen:2][Error:N]
        /// </summary>
        public static byte[] EncodeHotReloadNotification(bool success, string widgetType, int durationMs, string error = null)
        {
            var widgetTypeBytes = Encoding.UTF8.GetBytes(widgetType ?? string.Empty);
            var errorBytes = Encoding.UTF8.GetBytes(error ?? string.Empty);
            var buffer = new byte[1 + 1 + 1 + 2 + widgetTypeBytes.Length + 4 + 2 + errorBytes.Length];
            var pos = 0;

            buffer[pos++] = ProtocolVersion;
            buffer[pos++] = MessageTypes.HotReload;
            buffer[pos++] = (byte)(success ? 1 : 0);
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(pos), (ushort)widgetTypeBytes.Length);
            pos += 2;
            widgetTypeBytes.CopyTo(buffer.AsSpan(pos));
            pos += widgetTypeBytes.Length;
            BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(pos), durationMs);
            pos += 4;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(pos), (ushort)errorBytes.Length);
            pos += 2;
            errorBytes.CopyTo(buffer.AsSpan(pos));

            _totalBytesSent += buffer.Length;
            _messagesSent++;

            return buffer;
        }

        /// <summary>
        /// Encodes an AsyncCallbackCompleteMessage to binary format.
        /// Format: [Version:1][Type:1][CallbackIdLen:2][CallbackId:N]
        /// </summary>
        public static byte[] EncodeAsyncCallbackComplete(string callbackId)
        {
            var callbackIdBytes = Encoding.UTF8.GetBytes(callbackId ?? string.Empty);
            var buffer = new byte[1 + 1 + 2 + callbackIdBytes.Length];
            var pos = 0;

            buffer[pos++] = ProtocolVersion;
            buffer[pos++] = MessageTypes.AsyncCallbackComplete;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(pos), (ushort)callbackIdBytes.Length);
            pos += 2;
            callbackIdBytes.CopyTo(buffer.AsSpan(pos));

            _totalBytesSent += buffer.Length;
            _messagesSent++;

            return buffer;
        }

        /// <summary>
        /// Decodes a binary message header.
        /// Returns the message type and advances the offset past the header.
        /// </summary>
        public static (byte Version, byte MessageType) DecodeHeader(ReadOnlySpan<byte> data)
        {
            if (data.Length < 2)
            {
                throw new InvalidOperationException("Binary message too short for header");
            }

            return (data[0], data[1]);
        }

        /// <summary>
        /// Decodes an UpdateMessage from binary.
        /// </summary>
        public static (string ComponentId, long Address) DecodeUpdateMessage(ReadOnlySpan<byte> data)
        {
            var offset = 2; // Skip header
            var componentIdLen = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset));
            offset += 2;
            var componentId = Encoding.UTF8.GetString(data.Slice(offset, componentIdLen));
            offset += componentIdLen;
            var address = BinaryPrimitives.ReadInt64LittleEndian(data.Slice(offset));

            _totalBytesReceived += data.Length;
            _messagesReceived++;

            return (componentId, address);
        }

        /// <summary>
        /// Decodes a BatchedUpdate from binary.
        /// </summary>
        public static List<(string ComponentId, long Address)> DecodeBatchedUpdate(ReadOnlySpan<byte> data)
        {
            var offset = 2; // Skip header
            var count = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset));
            offset += 4;

            var updates = new List<(string ComponentId, long Address)>(count);
            for (var i = 0; i < count; i++)
            {
                var componentIdLen = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset));
                offset += 2;
                var componentId = Encoding.UTF8.GetString(data.Slice(offset, componentIdLen));
                offset += componentIdLen;
                var address = BinaryPrimitives.ReadInt64LittleEndian(data.Slice(offset));
                offset += 8;
                updates.Add((componentId, address));
            }

            _totalBytesReceived += data.Length;
            _messagesReceived++;

            return updates;
        }

        /// <summary>
        /// Decodes a DisposedMessage from binary.
        /// </summary>
        public static string DecodeDisposedMessage(ReadOnlySpan<byte> data)
        {
            var offset = 2; // Skip header
            var widgetIdLen = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset));
            offset += 2;
            var widgetId = Encoding.UTF8.GetString(data.Slice(offset, widgetIdLen));

            _totalBytesReceived += data.Length;
            _messagesReceived++;

            return widgetId;
        }

        /// <summary>
        /// Decodes a ScrollUpdate from binary (received from Dart).
        /// Format: [Version:1][Type:1][ControllerIdLen:2][ControllerId:N][Offset:8][MaxExtent:8][ViewportDim:8][EventType:1]
        /// </summary>
        public static (string ControllerId, double Offset, double MaxExtent, double ViewportDimension, ScrollEventType EventType) DecodeScrollUpdate(ReadOnlySpan<byte> data)
        {
            var offset = 2; // Skip header
            var controllerIdLen = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset));
            offset += 2;
            var controllerId = Encoding.UTF8.GetString(data.Slice(offset, controllerIdLen));
            offset += controllerIdLen;
            var scrollOffset = BinaryPrimitives.ReadDoubleLittleEndian(data.Slice(offset));
            offset += 8;
            var maxExtent = BinaryPrimitives.ReadDoubleLittleEndian(data.Slice(offset));
            offset += 8;
            var viewportDimension = BinaryPrimitives.ReadDoubleLittleEndian(data.Slice(offset));
            offset += 8;
            var eventType = (ScrollEventType)data[offset];

            _totalBytesReceived += data.Length;
            _messagesReceived++;

            return (controllerId, scrollOffset, maxExtent, viewportDimension, eventType);
        }

        /// <summary>
        /// Decodes a GestureEvent from binary (received from Dart).
        /// This handles common gesture event structures.
        /// Format: [Version:1][Type:1][EventTypeLen:2][EventType:N][PositionX:8][PositionY:8][...additional fields]
        /// </summary>
        public static BinaryGestureEventData DecodeGestureEvent(ReadOnlySpan<byte> data)
        {
            var offset = 2; // Skip header
            var eventTypeLen = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset));
            offset += 2;
            var eventType = Encoding.UTF8.GetString(data.Slice(offset, eventTypeLen));
            offset += eventTypeLen;

            // Basic position data present in all gesture events
            var positionX = BinaryPrimitives.ReadDoubleLittleEndian(data.Slice(offset));
            offset += 8;
            var positionY = BinaryPrimitives.ReadDoubleLittleEndian(data.Slice(offset));
            offset += 8;

            _totalBytesReceived += data.Length;
            _messagesReceived++;

            // Create appropriate event type based on eventType string
            return eventType switch
            {
                "TapDownDetails" => new BinaryTapDownDetails { GlobalPosition = (positionX, positionY) },
                "TapUpDetails" => new BinaryTapUpDetails { GlobalPosition = (positionX, positionY) },
                "DragStartDetails" => new BinaryDragStartDetails { GlobalPosition = (positionX, positionY) },
                "DragUpdateDetails" => DecodeDragUpdateDetails(data, offset, positionX, positionY),
                "DragEndDetails" => DecodeDragEndDetails(data, offset),
                _ => new BinaryUnknownGestureEvent { EventType = eventType, GlobalPosition = (positionX, positionY) }
            };
        }

        private static BinaryDragUpdateDetails DecodeDragUpdateDetails(ReadOnlySpan<byte> data, int offset, double posX, double posY)
        {
            var deltaX = BinaryPrimitives.ReadDoubleLittleEndian(data.Slice(offset));
            offset += 8;
            var deltaY = BinaryPrimitives.ReadDoubleLittleEndian(data.Slice(offset));

            return new BinaryDragUpdateDetails
            {
                GlobalPosition = (posX, posY),
                Delta = (deltaX, deltaY)
            };
        }

        private static BinaryDragEndDetails DecodeDragEndDetails(ReadOnlySpan<byte> data, int offset)
        {
            var velocityX = BinaryPrimitives.ReadDoubleLittleEndian(data.Slice(offset));
            offset += 8;
            var velocityY = BinaryPrimitives.ReadDoubleLittleEndian(data.Slice(offset));

            return new BinaryDragEndDetails
            {
                Velocity = new BinaryVelocity { PixelsPerSecond = (velocityX, velocityY) }
            };
        }
    }

    /// <summary>
    /// Types of scroll commands.
    /// </summary>
    public enum ScrollCommandType : byte
    {
        JumpTo = 0,
        AnimateTo = 1
    }

    /// <summary>
    /// Types of scroll events.
    /// </summary>
    public enum ScrollEventType : byte
    {
        Start = 0,
        Update = 1,
        End = 2
    }

    /// <summary>
    /// Statistics about binary protocol usage.
    /// </summary>
    public class BinaryProtocolStats
    {
        public long TotalBytesSent { get; set; }
        public long TotalBytesReceived { get; set; }
        public long MessagesSent { get; set; }
        public long MessagesReceived { get; set; }
        public long CompressionSavings { get; set; }
        public bool IsEnabled { get; set; }

        public double AverageMessageSize => MessagesSent > 0 ? (double)TotalBytesSent / MessagesSent : 0;
        public double CompressionRatio => TotalBytesSent > 0 ? 1.0 - ((double)TotalBytesSent / (TotalBytesSent + CompressionSavings)) : 0;

        public override string ToString()
        {
            return $"Binary Protocol: {(IsEnabled ? "On" : "Off")}, " +
                   $"Sent: {TotalBytesSent:N0} bytes ({MessagesSent:N0} msgs), " +
                   $"Received: {TotalBytesReceived:N0} bytes ({MessagesReceived:N0} msgs), " +
                   $"Avg Size: {AverageMessageSize:F1} bytes, " +
                   $"Compression: {CompressionRatio:P1}";
        }
    }

    // Simple gesture event classes for binary protocol decoding
    // These have "Binary" prefix to avoid conflicts with existing Flutter.Gestures types
    public abstract class BinaryGestureEventData
    {
        public (double X, double Y) GlobalPosition { get; set; }
    }

    public class BinaryUnknownGestureEvent : BinaryGestureEventData
    {
        public string EventType { get; set; }
    }

    public class BinaryTapDownDetails : BinaryGestureEventData { }

    public class BinaryTapUpDetails : BinaryGestureEventData { }

    public class BinaryDragStartDetails : BinaryGestureEventData { }

    public class BinaryDragUpdateDetails : BinaryGestureEventData
    {
        public (double X, double Y) Delta { get; set; }
    }

    public class BinaryDragEndDetails : BinaryGestureEventData
    {
        public BinaryVelocity Velocity { get; set; }
    }

    public class BinaryVelocity
    {
        public (double X, double Y) PixelsPerSecond { get; set; }
    }
}
