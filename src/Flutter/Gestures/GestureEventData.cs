using System;

namespace Flutter.Gestures
{
    // Note: Uses Flutter.Offset from the main namespace
    // This file only defines gesture event detail classes

    /// <summary>
    /// Represents velocity with pixels per second.
    /// </summary>
    public struct Velocity
    {
        public Offset PixelsPerSecond { get; set; }

        public Velocity(Offset pixelsPerSecond)
        {
            PixelsPerSecond = pixelsPerSecond;
        }

        public static readonly Velocity Zero = new Velocity(Offset.Zero);

        public override string ToString() => $"Velocity({PixelsPerSecond.Dx}, {PixelsPerSecond.Dy})";
    }

    // ===== TAP EVENTS =====

    /// <summary>
    /// Details for a pointer tap down event.
    /// </summary>
    public class TapDownDetails
    {
        public Offset GlobalPosition { get; set; }
        public Offset LocalPosition { get; set; }
        public int Kind { get; set; } // PointerDeviceKind

        public TapDownDetails() { }

        public TapDownDetails(Offset globalPosition, Offset localPosition = default, int kind = 0)
        {
            GlobalPosition = globalPosition;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? globalPosition : localPosition;
            Kind = kind;
        }
    }

    /// <summary>
    /// Details for a pointer tap up event.
    /// </summary>
    public class TapUpDetails
    {
        public Offset GlobalPosition { get; set; }
        public Offset LocalPosition { get; set; }
        public int Kind { get; set; }

        public TapUpDetails() { }

        public TapUpDetails(Offset globalPosition, Offset localPosition = default, int kind = 0)
        {
            GlobalPosition = globalPosition;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? globalPosition : localPosition;
            Kind = kind;
        }
    }

    /// <summary>
    /// Details for a pointer tap move event (when a tap gesture is still being processed).
    /// </summary>
    public class TapMoveDetails
    {
        public Offset GlobalPosition { get; set; }
        public Offset LocalPosition { get; set; }

        public TapMoveDetails() { }

        public TapMoveDetails(Offset globalPosition, Offset localPosition = default)
        {
            GlobalPosition = globalPosition;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? globalPosition : localPosition;
        }
    }

    // ===== DRAG EVENTS =====

    /// <summary>
    /// Details for a pointer drag down event.
    /// </summary>
    public class DragDownDetails
    {
        public Offset GlobalPosition { get; set; }
        public Offset LocalPosition { get; set; }

        public DragDownDetails() { }

        public DragDownDetails(Offset globalPosition, Offset localPosition = default)
        {
            GlobalPosition = globalPosition;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? globalPosition : localPosition;
        }
    }

    /// <summary>
    /// Details for a pointer drag start event.
    /// </summary>
    public class DragStartDetails
    {
        public Offset GlobalPosition { get; set; }
        public Offset LocalPosition { get; set; }
        public DateTime? SourceTimeStamp { get; set; }

        public DragStartDetails() { }

        public DragStartDetails(Offset globalPosition, Offset localPosition = default, DateTime? sourceTimeStamp = null)
        {
            GlobalPosition = globalPosition;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? globalPosition : localPosition;
            SourceTimeStamp = sourceTimeStamp;
        }
    }

    /// <summary>
    /// Details for a pointer drag update event.
    /// </summary>
    public class DragUpdateDetails
    {
        public Offset GlobalPosition { get; set; }
        public Offset LocalPosition { get; set; }
        public Offset Delta { get; set; }
        public Offset PrimaryDelta { get; set; }
        public DateTime? SourceTimeStamp { get; set; }

        public DragUpdateDetails() { }

        public DragUpdateDetails(Offset globalPosition, Offset localPosition = default, Offset delta = default, DateTime? sourceTimeStamp = null)
        {
            GlobalPosition = globalPosition;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? globalPosition : localPosition;
            Delta = delta;
            SourceTimeStamp = sourceTimeStamp;
        }
    }

    /// <summary>
    /// Details for a pointer drag end event.
    /// </summary>
    public class DragEndDetails
    {
        public Velocity Velocity { get; set; }
        public double PrimaryVelocity { get; set; }

        public DragEndDetails() { }

        public DragEndDetails(Velocity velocity, double primaryVelocity = 0)
        {
            Velocity = velocity;
            PrimaryVelocity = primaryVelocity;
        }
    }

    // ===== LONG PRESS EVENTS =====

    /// <summary>
    /// Details for a pointer long press down event.
    /// </summary>
    public class LongPressDownDetails
    {
        public Offset GlobalPosition { get; set; }
        public Offset LocalPosition { get; set; }
        public int Kind { get; set; }

        public LongPressDownDetails() { }

        public LongPressDownDetails(Offset globalPosition, Offset localPosition = default, int kind = 0)
        {
            GlobalPosition = globalPosition;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? globalPosition : localPosition;
            Kind = kind;
        }
    }

    /// <summary>
    /// Details for a pointer long press start event.
    /// </summary>
    public class LongPressStartDetails
    {
        public Offset GlobalPosition { get; set; }
        public Offset LocalPosition { get; set; }

        public LongPressStartDetails() { }

        public LongPressStartDetails(Offset globalPosition, Offset localPosition = default)
        {
            GlobalPosition = globalPosition;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? globalPosition : localPosition;
        }
    }

    /// <summary>
    /// Details for a pointer long press move update event.
    /// </summary>
    public class LongPressMoveUpdateDetails
    {
        public Offset GlobalPosition { get; set; }
        public Offset LocalPosition { get; set; }
        public Offset OffsetFromOrigin { get; set; }
        public Offset LocalOffsetFromOrigin { get; set; }

        public LongPressMoveUpdateDetails() { }

        public LongPressMoveUpdateDetails(Offset globalPosition, Offset localPosition = default, Offset offsetFromOrigin = default, Offset localOffsetFromOrigin = default)
        {
            GlobalPosition = globalPosition;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? globalPosition : localPosition;
            OffsetFromOrigin = offsetFromOrigin;
            LocalOffsetFromOrigin = localOffsetFromOrigin;
        }
    }

    /// <summary>
    /// Details for a pointer long press end event.
    /// </summary>
    public class LongPressEndDetails
    {
        public Offset GlobalPosition { get; set; }
        public Offset LocalPosition { get; set; }
        public Velocity Velocity { get; set; }

        public LongPressEndDetails() { }

        public LongPressEndDetails(Offset globalPosition, Offset localPosition = default, Velocity velocity = default)
        {
            GlobalPosition = globalPosition;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? globalPosition : localPosition;
            Velocity = velocity;
        }
    }

    // ===== SCALE/ZOOM EVENTS =====

    /// <summary>
    /// Details for a scale start event.
    /// </summary>
    public class ScaleStartDetails
    {
        public Offset FocalPoint { get; set; }
        public Offset LocalFocalPoint { get; set; }
        public int PointerCount { get; set; }
        public DateTime? SourceTimeStamp { get; set; }

        public ScaleStartDetails() { }

        public ScaleStartDetails(Offset focalPoint, Offset localFocalPoint = default, int pointerCount = 1, DateTime? sourceTimeStamp = null)
        {
            FocalPoint = focalPoint;
            LocalFocalPoint = localFocalPoint.Dx == 0 && localFocalPoint.Dy == 0 ? focalPoint : localFocalPoint;
            PointerCount = pointerCount;
            SourceTimeStamp = sourceTimeStamp;
        }
    }

    /// <summary>
    /// Details for a scale update event.
    /// </summary>
    public class ScaleUpdateDetails
    {
        public Offset FocalPoint { get; set; }
        public Offset LocalFocalPoint { get; set; }
        public Offset FocalPointDelta { get; set; }
        public double Scale { get; set; }
        public double HorizontalScale { get; set; }
        public double VerticalScale { get; set; }
        public double Rotation { get; set; }
        public int PointerCount { get; set; }
        public DateTime? SourceTimeStamp { get; set; }

        public ScaleUpdateDetails() { }

        public ScaleUpdateDetails(Offset focalPoint, double scale = 1.0, double rotation = 0.0, int pointerCount = 1)
        {
            FocalPoint = focalPoint;
            Scale = scale;
            HorizontalScale = scale;
            VerticalScale = scale;
            Rotation = rotation;
            PointerCount = pointerCount;
        }
    }

    /// <summary>
    /// Details for a scale end event.
    /// </summary>
    public class ScaleEndDetails
    {
        public Velocity Velocity { get; set; }
        public int PointerCount { get; set; }

        public ScaleEndDetails() { }

        public ScaleEndDetails(Velocity velocity, int pointerCount = 0)
        {
            Velocity = velocity;
            PointerCount = pointerCount;
        }
    }

    // ===== FORCE PRESS EVENTS =====

    /// <summary>
    /// Details for force press events (3D Touch / Force Touch).
    /// </summary>
    public class ForcePressDetails
    {
        public Offset GlobalPosition { get; set; }
        public Offset LocalPosition { get; set; }
        public double Pressure { get; set; }

        public ForcePressDetails() { }

        public ForcePressDetails(Offset globalPosition, Offset localPosition = default, double pressure = 0.0)
        {
            GlobalPosition = globalPosition;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? globalPosition : localPosition;
            Pressure = pressure;
        }
    }

    // ===== POINTER EVENTS =====

    /// <summary>
    /// Base class for pointer events.
    /// </summary>
    public abstract class PointerEvent
    {
        public Offset Position { get; set; }
        public Offset LocalPosition { get; set; }
        public int Pointer { get; set; }
        public int Kind { get; set; } // PointerDeviceKind
        public int Buttons { get; set; }
        public double Pressure { get; set; }
        public double PressureMin { get; set; }
        public double PressureMax { get; set; }
        public DateTime? TimeStamp { get; set; }
    }

    /// <summary>
    /// Event when a pointer comes into contact with the screen.
    /// </summary>
    public class PointerDownEvent : PointerEvent
    {
        public PointerDownEvent() { }

        public PointerDownEvent(Offset position, Offset localPosition = default, int pointer = 0, int kind = 0)
        {
            Position = position;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? position : localPosition;
            Pointer = pointer;
            Kind = kind;
        }
    }

    /// <summary>
    /// Event when a pointer moves while in contact with the screen.
    /// </summary>
    public class PointerMoveEvent : PointerEvent
    {
        public Offset Delta { get; set; }
        public Offset LocalDelta { get; set; }

        public PointerMoveEvent() { }

        public PointerMoveEvent(Offset position, Offset localPosition = default, Offset delta = default, int pointer = 0)
        {
            Position = position;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? position : localPosition;
            Delta = delta;
            Pointer = pointer;
        }
    }

    /// <summary>
    /// Event when a pointer stops being in contact with the screen.
    /// </summary>
    public class PointerUpEvent : PointerEvent
    {
        public PointerUpEvent() { }

        public PointerUpEvent(Offset position, Offset localPosition = default, int pointer = 0, int kind = 0)
        {
            Position = position;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? position : localPosition;
            Pointer = pointer;
            Kind = kind;
        }
    }

    /// <summary>
    /// Event when input from a pointer is no longer directed towards this receiver.
    /// </summary>
    public class PointerCancelEvent : PointerEvent
    {
        public PointerCancelEvent() { }

        public PointerCancelEvent(Offset position, Offset localPosition = default, int pointer = 0, int kind = 0)
        {
            Position = position;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? position : localPosition;
            Pointer = pointer;
            Kind = kind;
        }
    }

    /// <summary>
    /// Event when a pointer hovers over the receiver (no contact).
    /// </summary>
    public class PointerHoverEvent : PointerEvent
    {
        public Offset Delta { get; set; }
        public Offset LocalDelta { get; set; }

        public PointerHoverEvent() { }

        public PointerHoverEvent(Offset position, Offset localPosition = default, Offset delta = default, int pointer = 0)
        {
            Position = position;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? position : localPosition;
            Delta = delta;
            Pointer = pointer;
        }
    }

    /// <summary>
    /// Event when a pointer enters the receiver's hit test area.
    /// </summary>
    public class PointerEnterEvent : PointerEvent
    {
        public Offset Delta { get; set; }
        public Offset LocalDelta { get; set; }

        public PointerEnterEvent() { }
    }

    /// <summary>
    /// Event when a pointer leaves the receiver's hit test area.
    /// </summary>
    public class PointerExitEvent : PointerEvent
    {
        public Offset Delta { get; set; }
        public Offset LocalDelta { get; set; }

        public PointerExitEvent() { }
    }

    /// <summary>
    /// Event when a scroll action occurs (e.g., mouse scroll wheel).
    /// </summary>
    public class PointerScrollEvent : PointerEvent
    {
        public Offset ScrollDelta { get; set; }

        public PointerScrollEvent() { }

        public PointerScrollEvent(Offset position, Offset localPosition = default, Offset scrollDelta = default, int pointer = 0)
        {
            Position = position;
            LocalPosition = localPosition.Dx == 0 && localPosition.Dy == 0 ? position : localPosition;
            ScrollDelta = scrollDelta;
            Pointer = pointer;
        }
    }
}
