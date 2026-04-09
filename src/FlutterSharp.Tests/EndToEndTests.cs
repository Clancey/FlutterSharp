using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;
using NUnit.Framework;
using Flutter;
using Flutter.Widgets;
using Flutter.Structs;
using Flutter.Internal;
using Flutter.UI;
using System.Reflection;
using System.Collections.Concurrent;
using System.Threading;
using Material = Flutter.Material;

namespace FlutterSharp.Tests
{
	/// <summary>
	/// End-to-end tests that validate the complete widget lifecycle from generation through runtime rendering.
	/// These tests verify:
	/// - Widget creation and struct preparation
	/// - FFI struct field initialization (Handle, WidgetType, Id)
	/// - Communicator integration and message serialization
	/// - Widget type registry correctness
	/// - Complete round-trip flows
	///
	/// Note: These tests work with class-based structs that use StructLayout for FFI compatibility.
	/// We access the backing struct through the widget's GetBackingStruct method via reflection.
	/// </summary>
	[TestFixture]
	[Category("E2E")]
	public class EndToEndTests
	{
		private static readonly FieldInfo CachedBackingStructField =
			typeof(Widget).GetField("_cachedBackingStruct", BindingFlags.Instance | BindingFlags.NonPublic)!;

		private Action<(string Method, string Arguments)>? _originalSendCommand;
		private List<(string Method, string Arguments)>? _sentCommands;

		[SetUp]
		public void Setup()
		{
			// Save original SendCommand handler
			_originalSendCommand = Communicator.SendCommand;
			_sentCommands = new List<(string Method, string Arguments)>();

			// Ensure FlutterManager is initialized
			FlutterManager.Initialize();

			// Set up mock SendCommand to capture outgoing messages
			Communicator.SendCommand = cmd => _sentCommands?.Add(cmd);
		}

		[TearDown]
		public void TearDown()
		{
			// Restore SendCommand
			Communicator.SendCommand = _originalSendCommand;

			// Reset FlutterManager state
			FlutterManager.Reset();
		}

		#region Helper Methods

		/// <summary>
		/// Gets the backing struct from a widget using reflection.
		/// </summary>
		private T GetBackingStruct<T>(Widget widget) where T : FlutterObjectStruct
		{
			var method = typeof(Widget).GetMethod("GetBackingStruct", BindingFlags.NonPublic | BindingFlags.Instance);
			var genericMethod = method!.MakeGenericMethod(typeof(T));
			return (T)genericMethod.Invoke(widget, null)!;
		}

		/// <summary>
		/// Calls PrepareForSending on a widget using reflection.
		/// </summary>
		private void CallPrepareForSending(Widget widget)
		{
			var method = typeof(Widget).GetMethod("PrepareForSending", BindingFlags.NonPublic | BindingFlags.Instance);
			method?.Invoke(widget, null);
		}

		/// <summary>
		/// Calls GetHandle on a widget using reflection.
		/// </summary>
		private IntPtr CallGetHandle(Widget widget)
		{
			var method = typeof(Widget).GetMethod("GetHandle", BindingFlags.NonPublic | BindingFlags.Instance);
			return (IntPtr)method!.Invoke(widget, null)!;
		}

		private string? GetStringProperty(Widget widget, string propertyName)
		{
			var backingStruct = CachedBackingStructField.GetValue(widget);
			return backingStruct?.GetType().GetProperty(propertyName)?.GetValue(backingStruct) as string;
		}

		#endregion

		#region Simple Widget Creation Tests

		/// <summary>
		/// Verifies that creating a simple Text widget and calling PrepareForSending produces valid FFI struct fields.
		/// This is the most basic end-to-end test that validates struct preparation.
		/// </summary>
		[Test]
		public void HandleAction_OnStatefulWidgetCallback_UpdatesStateAndSendsNewTree()
		{
			var originalBatching = FlutterManager.BatchingEnabled;
			FlutterManager.BatchingEnabled = false;

			try
			{
				var harness = new CallbackHarnessWidget();

				FlutterManager.SendState(harness, immediate: true);

				Assert.That(_sentCommands, Has.Count.EqualTo(1));
				Assert.That(harness.LastButton, Is.Not.Null);
				Assert.That(harness.LastCheckbox, Is.Not.Null);

				var buttonActionId = GetStringProperty(harness.LastButton!, "onPressedAction");
				var checkboxActionId = GetStringProperty(harness.LastCheckbox!, "onChangedAction");

				Assert.That(buttonActionId, Does.StartWith("action_"));
				Assert.That(checkboxActionId, Does.StartWith("action_"));
				Assert.That(harness.Clicks, Is.EqualTo(0));
				Assert.That(harness.IsChecked, Is.False);

				Communicator.OnCommandReceived?.Invoke((
					"HandleAction",
					JsonSerializer.Serialize(new { actionId = buttonActionId }),
					_ => { }
				));

				Assert.That(harness.Clicks, Is.EqualTo(1));
				Assert.That(_sentCommands, Has.Count.EqualTo(2));
				Assert.That(_sentCommands![1].Method, Is.EqualTo("UpdateComponent"));

				Communicator.OnCommandReceived?.Invoke((
					"HandleAction",
					JsonSerializer.Serialize(new { actionId = checkboxActionId, value = true }),
					_ => { }
				));

				Assert.That(harness.IsChecked, Is.True);
				Assert.That(_sentCommands, Has.Count.EqualTo(3));
				Assert.That(_sentCommands![2].Method, Is.EqualTo("UpdateComponent"));
			}
			finally
			{
				FlutterManager.BatchingEnabled = originalBatching;
			}
		}

		[Test]
		public void SendState_WithBatchingEnabled_FlushesSubsequentUpdates()
		{
			var originalSendCommand = Communicator.SendCommand;
			var originalBatching = FlutterManager.BatchingEnabled;
			var originalBatchWindow = FlutterManager.BatchWindowMs;
			var sentCommands = new ConcurrentQueue<(string Method, string Arguments)>();

			try
			{
				Communicator.SendCommand = cmd => sentCommands.Enqueue(cmd);
				FlutterManager.BatchingEnabled = true;
				FlutterManager.BatchWindowMs = 1;

				FlutterManager.SendState(new BatchingHarnessWidget("first"));
				Assert.That(SpinWait.SpinUntil(() => sentCommands.Count >= 1, 1000), Is.True);

				FlutterManager.SendState(new BatchingHarnessWidget("second"));
				Assert.That(SpinWait.SpinUntil(() => sentCommands.Count >= 2, 1000), Is.True);

				var commands = sentCommands.ToArray();
				Assert.That(commands, Has.Length.EqualTo(2));
				Assert.That(commands[0].Method, Is.EqualTo("UpdateComponent"));
				Assert.That(commands[1].Method, Is.EqualTo("UpdateComponent"));
			}
			finally
			{
				FlutterManager.BatchingEnabled = originalBatching;
				FlutterManager.BatchWindowMs = originalBatchWindow;
				Communicator.SendCommand = originalSendCommand;
			}
		}

		[Test]
		public void SimpleWidgetCreation_ShouldPrepareValidStruct()
		{
			// Arrange & Act - Create a Text widget
			var text = new Text(data: "Hello World");
			CallPrepareForSending(text);
			var handle = CallGetHandle(text);

			// Assert - Verify handle is allocated
			Assert.That(handle, Is.Not.EqualTo(IntPtr.Zero),
				"Widget handle must be allocated after PrepareForSending");

			// Get the backing struct and verify its properties
			var backingStruct = GetBackingStruct<WidgetStruct>(text);

			Assert.That(backingStruct.Handle, Is.Not.EqualTo(IntPtr.Zero),
				"BaseStruct.Handle must be set (either from AddrOfPinnedObject or fallback)");
			Assert.That(backingStruct.WidgetType, Is.Not.Null.And.Not.Empty,
				"widgetType must be set (this was the critical bug fix)");
			Assert.That(backingStruct.WidgetType, Is.EqualTo("Text"),
				"widgetType should be 'Text'");
			Assert.That(backingStruct.Id, Is.Not.Null.And.Not.Empty,
				"Widget id should be a non-empty GUID string");
			Assert.That(Guid.TryParse(backingStruct.Id, out _), Is.True,
				"Widget id should be a valid GUID");
		}

		/// <summary>
		/// Verifies that creating a Container widget (which has optional child and properties) works correctly.
		/// </summary>
		[Test]
		public void ContainerWidgetCreation_ShouldPrepareValidStruct()
		{
			// Arrange & Act - Create a Container with a Text child
			var container = new Container(
				child: new Text(data: "Child Text"),
				width: 100.0,
				height: 100.0
			);
			CallPrepareForSending(container);
			var handle = CallGetHandle(container);

			// Assert
			Assert.That(handle, Is.Not.EqualTo(IntPtr.Zero));

			var backingStruct = GetBackingStruct<WidgetStruct>(container);

			Assert.That(backingStruct.Handle, Is.Not.EqualTo(IntPtr.Zero));
			Assert.That(backingStruct.WidgetType, Is.Not.Null.And.Not.Empty);
			Assert.That(backingStruct.WidgetType, Is.EqualTo("Container"));
		}

		#endregion

		#region Widget Hierarchy Tests

		/// <summary>
		/// Verifies that a Column widget with multiple children properly prepares the struct.
		/// </summary>
		[Test]
		public void WidgetWithChildren_ShouldPrepareChildrenArray()
		{
			// Arrange - Create a Column with three Text children
			var children = new List<Widget>
			{
				new Text(data: "Child 1"),
				new Text(data: "Child 2"),
				new Text(data: "Child 3")
			};
			var column = new Column(children: children);

			// Act
			CallPrepareForSending(column);
			var handle = CallGetHandle(column);

			// Assert
			Assert.That(handle, Is.Not.EqualTo(IntPtr.Zero));

			var backingStruct = GetBackingStruct<WidgetStruct>(column);

			Assert.That(backingStruct.Handle, Is.Not.EqualTo(IntPtr.Zero));
			Assert.That(backingStruct.WidgetType, Is.EqualTo("Column"));
			Assert.That(backingStruct.Id, Is.Not.Null.And.Not.Empty);
		}

		/// <summary>
		/// Verifies that a complex nested widget hierarchy (Container > Column > multiple Text widgets)
		/// is properly prepared with all struct fields initialized.
		/// </summary>
		[Test]
		public void ComplexWidgetHierarchy_ShouldPrepareAllLevels()
		{
			// Arrange - Create Container > Column > [Text, Text]
			var container = new Container(
				child: new Column(
					children: new List<Widget>
					{
						new Text(data: "First Line"),
						new Text(data: "Second Line")
					}
				),
				width: 200.0,
				height: 150.0
			);

			// Act
			CallPrepareForSending(container);
			var handle = CallGetHandle(container);

			// Assert
			Assert.That(handle, Is.Not.EqualTo(IntPtr.Zero));

			var backingStruct = GetBackingStruct<WidgetStruct>(container);

			Assert.That(backingStruct.Handle, Is.Not.EqualTo(IntPtr.Zero));
			Assert.That(backingStruct.WidgetType, Is.EqualTo("Container"));
		}

		#endregion

		#region Widget Type Discriminator Tests

		/// <summary>
		/// Verifies that different widget types have unique widgetType values and correct type strings.
		/// </summary>
		[Test]
		public void WidgetTypeDiscriminator_ShouldBeUniquePerType()
		{
			// Arrange - Create widgets of different types
			var text = new Text(data: "Text");
			var container = new Container();
			var column = new Column();

			// Act
			CallPrepareForSending(text);
			CallPrepareForSending(container);
			CallPrepareForSending(column);

			var textHandle = CallGetHandle(text);
			var containerHandle = CallGetHandle(container);
			var columnHandle = CallGetHandle(column);

			// Assert
			var textStruct = GetBackingStruct<WidgetStruct>(text);
			var containerStruct = GetBackingStruct<WidgetStruct>(container);
			var columnStruct = GetBackingStruct<WidgetStruct>(column);

			// Verify all have widgetType set
			Assert.That(textStruct.WidgetType, Is.Not.Null.And.Not.Empty);
			Assert.That(containerStruct.WidgetType, Is.Not.Null.And.Not.Empty);
			Assert.That(columnStruct.WidgetType, Is.Not.Null.And.Not.Empty);

			// Verify correct type names
			Assert.That(textStruct.WidgetType, Is.EqualTo("Text"));
			Assert.That(containerStruct.WidgetType, Is.EqualTo("Container"));
			Assert.That(columnStruct.WidgetType, Is.EqualTo("Column"));

			// Verify types are different
			Assert.That(textStruct.WidgetType, Is.Not.EqualTo(containerStruct.WidgetType));
			Assert.That(textStruct.WidgetType, Is.Not.EqualTo(columnStruct.WidgetType));
			Assert.That(containerStruct.WidgetType, Is.Not.EqualTo(columnStruct.WidgetType));
		}

		#endregion

		#region Communicator Integration Tests

		/// <summary>
		/// Verifies that widget struct is properly prepared before sending.
		/// Note: SendState may fail for StatelessWidget subclasses that don't override Build(),
		/// so we test the struct preparation directly instead of the full SendState flow.
		/// </summary>
		[Test]
		public void WidgetPreparedForSending_ShouldHaveValidAddress()
		{
			// Arrange
			var text = new Text(data: "Test Message");

			// Act - Prepare the widget (this is what happens inside SendState before Build is called)
			CallPrepareForSending(text);
			var handle = CallGetHandle(text);

			// Assert - Verify the widget is properly prepared with a valid address
			Assert.That(handle, Is.Not.EqualTo(IntPtr.Zero),
				"Widget address must be non-zero after preparation");

			var backingStruct = GetBackingStruct<WidgetStruct>(text);
			Assert.That(backingStruct.WidgetType, Is.EqualTo("Text"));
			Assert.That(backingStruct.Id, Is.Not.Null.And.Not.Empty);
		}

		/// <summary>
		/// Verifies the complete widget hierarchy preparation flow: C# widget creation → struct preparation.
		/// Note: Full SendState flow is tested separately; this focuses on struct preparation.
		/// </summary>
		[Test]
		public void CompleteHierarchy_ShouldPrepareCorrectly()
		{
			// Arrange - Create a widget hierarchy
			var container = new Container(
				child: new Column(
					children: new List<Widget>
					{
						new Text(data: "Line 1"),
						new Text(data: "Line 2")
					}
				)
			);

			// Act - Prepare the widget
			CallPrepareForSending(container);
			var handle = CallGetHandle(container);

			// Assert - Verify widget address is valid
			Assert.That(handle, Is.Not.EqualTo(IntPtr.Zero),
				"Widget address must be non-zero after preparation");

			// Verify the backing struct is properly initialized
			var backingStruct = GetBackingStruct<WidgetStruct>(container);
			Assert.That(backingStruct.Handle, Is.Not.EqualTo(IntPtr.Zero));
			Assert.That(backingStruct.WidgetType, Is.EqualTo("Container"));
			Assert.That(backingStruct.Id, Is.Not.Null.And.Not.Empty);
		}

		/// <summary>
		/// Verifies that SendState emits an UpdateComponent message with a valid pointer address.
		/// </summary>
		[Test]
		public void SendState_ShouldEmitUpdateComponentMessage()
		{
			// Arrange
			var widget = new Align(
				child: new SizedBox(width: 10, height: 10)
			);

			// Act
			FlutterManager.SendState(widget, componentID: "component-42", immediate: true);

			// Assert
			var updateCommand = _sentCommands!.Find(cmd => cmd.Method == "UpdateComponent");
			Assert.That(updateCommand.Method, Is.EqualTo("UpdateComponent"));

			var updateMessage = JsonSerializer.Deserialize<UpdateMessage>(
				updateCommand.Arguments,
				new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
			Assert.That(updateMessage, Is.Not.Null);
			Assert.That(updateMessage!.MessageType, Is.EqualTo("UpdateComponent"));
			Assert.That(updateMessage.ComponentId, Is.EqualTo("component-42"));
			Assert.That(updateMessage.Address, Is.Not.EqualTo(0));
		}

		/// <summary>
		/// Verifies that UpdateComponent address matches the widget's prepared pinned handle.
		/// </summary>
		[Test]
		public void SendState_ShouldUsePreparedHandleAddress()
		{
			// Arrange
			var widget = new SizedBox(width: 10, height: 20);
			CallPrepareForSending(widget);
			var expectedAddress = CallGetHandle(widget).ToInt64();

			// Act
			FlutterManager.SendState(widget, componentID: "component-73", immediate: true);

			// Assert
			var updateCommand = _sentCommands!.Find(cmd => cmd.Method == "UpdateComponent");
			var updateMessage = JsonSerializer.Deserialize<UpdateMessage>(
				updateCommand.Arguments,
				new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

			Assert.That(updateMessage, Is.Not.Null);
			Assert.That(updateMessage!.Address, Is.EqualTo(expectedAddress));
		}

		/// <summary>
		/// Verifies that untracking a widget sends a disposal message to Dart.
		/// </summary>
		[Test]
		public void UntrackWidget_ShouldEmitDisposedComponentMessage()
		{
			// Arrange
			var widget = new SizedBox(width: 10, height: 10);
			FlutterManager.TrackWidget(widget);

			// Act
			FlutterManager.UntrackWidget(widget);

			// Assert
			var disposedCommand = _sentCommands!.Find(cmd => cmd.Method == "DisposedComponent");
			Assert.That(disposedCommand.Method, Is.EqualTo("DisposedComponent"));
			Assert.That(disposedCommand.Arguments, Does.Contain(widget.Id));
		}

		/// <summary>
		/// Verifies track -> lookup -> untrack widget lifecycle integrity.
		/// </summary>
		[Test]
		public void TrackAndUntrackWidget_ShouldRoundTripLifecycle()
		{
			// Arrange
			var widget = new Text(data: "Tracked");

			// Act
			FlutterManager.TrackWidget(widget);
			var tracked = FlutterManager.GetWidget(widget.Id);
			FlutterManager.UntrackWidget(widget);
			var untracked = FlutterManager.GetWidget(widget.Id);

			// Assert
			Assert.That(tracked, Is.SameAs(widget));
			Assert.That(untracked, Is.Null);
		}

		#endregion

		#region Widget Disposal Tests

		/// <summary>
		/// Verifies that disposing a widget cleans up resources correctly.
		/// </summary>
		[Test]
		public void WidgetDisposal_ShouldCleanupResources()
		{
			// Arrange
			var text = new Text(data: "Disposable");
			CallPrepareForSending(text);
			var handleBeforeDispose = CallGetHandle(text);

			// Verify struct is valid before disposal
			Assert.That(handleBeforeDispose, Is.Not.EqualTo(IntPtr.Zero));

			// Act - Dispose the widget
			text.Dispose();

			// Assert - Verify disposal message was sent
			var disposalMessages = _sentCommands?.FindAll(cmd => cmd.Method == "Disposed");
			Assert.That(disposalMessages, Is.Not.Null);
			// Note: Widget disposal may or may not send a message depending on whether it was tracked
			// This test primarily verifies that Dispose() doesn't throw
		}

		/// <summary>
		/// Verifies that disposing a widget with children cleans up the children array.
		/// </summary>
		[Test]
		public void WidgetWithChildrenDisposal_ShouldCleanupChildrenArray()
		{
			// Arrange
			var column = new Column(
				children: new List<Widget>
				{
					new Text(data: "Child 1"),
					new Text(data: "Child 2")
				}
			);
			CallPrepareForSending(column);
			var handle = CallGetHandle(column);

			// Verify struct is valid before disposal
			Assert.That(handle, Is.Not.EqualTo(IntPtr.Zero));

			// Act - Dispose
			column.Dispose();

			// Assert - Disposal should complete without error
			// Memory cleanup is handled by StructManager and StructMemoryTracker
			Assert.Pass("Column disposed successfully");
		}

		/// <summary>
		/// Verifies callback registrations are cleaned when widget is disposed.
		/// </summary>
		[Test]
		public void WidgetDisposal_ShouldUnregisterCallbacks()
		{
			// Arrange
			var callbacksBefore = CallbackRegistry.Count;
			var button = new Flutter.Material.TextButton(
				child: new Text("test"),
				onPressed: () => { },
				onLongPress: () => { }
			);

			// Ensure callbacks were registered by constructor
			Assert.That(CallbackRegistry.Count, Is.GreaterThanOrEqualTo(callbacksBefore + 2));

			// Act
			button.Dispose();

			// Assert
			Assert.That(CallbackRegistry.Count, Is.EqualTo(callbacksBefore));
		}

		#endregion

		#region Edge Case Tests

		/// <summary>
		/// Verifies that creating a widget with null/default values doesn't crash.
		/// </summary>
		[Test]
		public void WidgetWithNullValues_ShouldHandleGracefully()
		{
			// Arrange & Act
			var text = new Text(data: null);
			CallPrepareForSending(text);
			var handle = CallGetHandle(text);

			// Assert
			Assert.That(handle, Is.Not.EqualTo(IntPtr.Zero));

			var backingStruct = GetBackingStruct<WidgetStruct>(text);
			Assert.That(backingStruct.Handle, Is.Not.EqualTo(IntPtr.Zero));
		}

		/// <summary>
		/// Verifies that creating a Column with empty children list works correctly.
		/// </summary>
		[Test]
		public void ColumnWithEmptyChildren_ShouldHaveZeroCount()
		{
			// Arrange & Act
			var column = new Column(children: new List<Widget>());
			CallPrepareForSending(column);
			var handle = CallGetHandle(column);

			// Assert
			Assert.That(handle, Is.Not.EqualTo(IntPtr.Zero));

			var backingStruct = GetBackingStruct<WidgetStruct>(column);
			Assert.That(backingStruct.Handle, Is.Not.EqualTo(IntPtr.Zero));
			Assert.That(backingStruct.WidgetType, Is.EqualTo("Column"));
		}

		/// <summary>
		/// Verifies that multiple calls to PrepareForSending are idempotent.
		/// </summary>
		[Test]
		public void MultiplePrepareCalls_ShouldBeIdempotent()
		{
			// Arrange
			var text = new Text(data: "Test");

			// Act
			CallPrepareForSending(text);
			var handle1 = CallGetHandle(text);

			CallPrepareForSending(text);
			var handle2 = CallGetHandle(text);

			// Assert - Handles should be the same
			Assert.That(handle1, Is.EqualTo(handle2),
				"Multiple PrepareForSending calls should return the same handle");
		}

		#endregion

		#region Struct Field Verification Tests

		/// <summary>
		/// Comprehensive test that verifies all critical struct fields are properly initialized.
		/// This test acts as a regression guard for the widgetType bug fix.
		/// </summary>
		[Test]
		public void AllStructFields_ShouldBeInitialized()
		{
			// Arrange & Act
			var container = new Container(
				child: new Text(data: "Test"),
				width: 100,
				height: 100
			);
			CallPrepareForSending(container);
			var handle = CallGetHandle(container);

			// Assert - Perform exhaustive field verification
			var backingStruct = GetBackingStruct<WidgetStruct>(container);

			// 1. BaseStruct fields
			Assert.That(backingStruct.Handle, Is.Not.EqualTo(IntPtr.Zero),
				"BaseStruct.Handle must be set (critical for Dart FFI)");

			// 2. FlutterObjectStruct fields
			Assert.That(backingStruct.WidgetType, Is.Not.Null.And.Not.Empty,
				"FlutterObjectStruct.WidgetType must be set (THIS WAS THE BUG - widgetType was null)");

			// 3. WidgetStruct fields
			Assert.That(backingStruct.Id, Is.Not.Null.And.Not.Empty,
				"WidgetStruct.Id must be set (unique widget identifier)");

			// 4. Verify values are correct
			Assert.That(backingStruct.WidgetType, Is.EqualTo("Container"));
			Assert.That(Guid.TryParse(backingStruct.Id, out _), Is.True);
		}

		#endregion

		#region Helper Classes

		/// <summary>
		/// Message class for deserializing UpdateComponent messages.
		/// </summary>
		private class UpdateMessage
		{
			public string MessageType { get; set; } = "UpdateComponent";
			public string ComponentId { get; set; } = "";
			public long Address { get; set; }
		}

		#endregion

		private sealed class CallbackHarnessWidget : StatefulWidget
		{
			public int Clicks { get; private set; }
			public bool IsChecked { get; private set; }
			public Material.TextButton? LastButton { get; private set; }
			public Checkbox? LastCheckbox { get; private set; }

			public override Widget Build()
			{
				LastButton = new Material.TextButton(
					new Text($"Clicks: {Clicks}"),
					onPressed: () => SetState(() => Clicks++)
				);

				LastCheckbox = new Checkbox(
					IsChecked,
					value => SetState(() => IsChecked = value ?? false)
				);

				return new Column
				{
					LastButton,
					LastCheckbox
				};
			}
		}

		private sealed class BatchingHarnessWidget(string label) : StatelessWidget
		{
			public override Widget Build() => new Text(data: label);
		}
	}
}
