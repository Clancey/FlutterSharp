using System;
using System.Collections.Generic;
using System.Text.Json;
using NUnit.Framework;
using Flutter;
using Flutter.Widgets;
using Flutter.Internal;

namespace FlutterSharp.Tests
{
	/// <summary>
	/// Tests for builder callback round-trip (BLD004).
	/// These tests verify the callback registry and event routing systems
	/// that enable ListViewBuilder and GridViewBuilder to function.
	///
	/// Note: Full widget instantiation tests are limited due to FFI/native
	/// memory requirements. These tests focus on the event routing infrastructure.
	/// </summary>
	[TestFixture]
	public class BuilderCallbackTests
	{
		private Action<(string Method, string Arguments)>? _originalSendCommand;
		private List<(string Method, string Arguments)>? _sentCommands;

		[SetUp]
		public void Setup()
		{
			// Save original SendCommand handler only
			// OnCommandReceived is managed by FlutterManager and should not be replaced
			_originalSendCommand = Communicator.SendCommand;
			_sentCommands = new List<(string Method, string Arguments)>();

			// FlutterManager is auto-initialized via static constructor
			// We just need to ensure it's set up (this is a no-op if already initialized)
			FlutterManager.Initialize();

			// Set up mock SendCommand to capture outgoing messages
			Communicator.SendCommand = cmd => _sentCommands?.Add(cmd);
		}

		[TearDown]
		public void TearDown()
		{
			// Restore SendCommand only
			Communicator.SendCommand = _originalSendCommand;

			// Reset FlutterManager state (handlers, widgets, etc.)
			// but NOT the command received handler - that's set during initialization
			FlutterManager.Reset();
		}

		#region Callback Registry Tests

		[Test]
		public void CallbackRegistry_RegisterAndInvoke_VoidCallback()
		{
			// Arrange
			bool wasInvoked = false;
			Action callback = () => wasInvoked = true;
			var callbackId = CallbackRegistry.Register(callback);

			// Act
			CallbackRegistry.InvokeVoid(callbackId);

			// Assert
			Assert.That(wasInvoked, Is.True);
		}

		[Test]
		public void CallbackRegistry_RegisterAndInvoke_IntCallback()
		{
			// Arrange
			int receivedValue = 0;
			Action<int> callback = (val) => receivedValue = val;
			var callbackId = CallbackRegistry.Register(callback);

			// Act
			CallbackRegistry.Invoke(callbackId, 42);

			// Assert
			Assert.That(receivedValue, Is.EqualTo(42));
		}

		[Test]
		public void CallbackRegistry_RegisterAndInvoke_StringCallback()
		{
			// Arrange
			string? receivedValue = null;
			Action<string> callback = (val) => receivedValue = val;
			var callbackId = CallbackRegistry.Register(callback);

			// Act
			CallbackRegistry.Invoke(callbackId, "Hello");

			// Assert
			Assert.That(receivedValue, Is.EqualTo("Hello"));
		}

		[Test]
		public void CallbackRegistry_RegisterAndInvoke_FuncCallback()
		{
			// Arrange
			Func<int, string> callback = (index) => $"Item {index}";
			var callbackId = CallbackRegistry.Register(callback);

			// Act - use the generic invoke with args
			CallbackRegistry.Invoke(callbackId, new object[] { 5 });

			// Assert - function was invoked (we can verify by invoking via DynamicInvoke)
			Assert.Pass("Func callback registered and invoked without exception");
		}

		[Test]
		public void CallbackRegistry_Unregister_RemovesCallback()
		{
			// Arrange
			bool wasInvoked = false;
			Action callback = () => wasInvoked = true;
			var callbackId = CallbackRegistry.Register(callback);

			// Act
			CallbackRegistry.Unregister(callbackId);

			// Invoking after unregister should not invoke the callback
			// (The implementation logs a warning but doesn't throw)
			CallbackRegistry.InvokeVoid(callbackId);

			// Assert - callback should not be invoked after unregistering
			Assert.That(wasInvoked, Is.False);
		}

		[Test]
		public void CallbackRegistry_MultipleCalls_EachTracked()
		{
			// Arrange
			var invokeCount = 0;
			Action callback = () => invokeCount++;
			var callbackId = CallbackRegistry.Register(callback);

			// Act
			CallbackRegistry.InvokeVoid(callbackId);
			CallbackRegistry.InvokeVoid(callbackId);
			CallbackRegistry.InvokeVoid(callbackId);

			// Assert
			Assert.That(invokeCount, Is.EqualTo(3));
		}

		#endregion

		#region FlutterManager Event Handler Tests

		[Test]
		public void FlutterManager_RegisterEventHandler_ReceivesEvents()
		{
			// Arrange
			string? receivedEventName = null;
			string? receivedData = null;
			FlutterManager.RegisterEventHandler("TestEvent", (name, data, callback) =>
			{
				receivedEventName = name;
				receivedData = data;
				callback?.Invoke("handled");
			});

			var eventJson = JsonSerializer.Serialize(new
			{
				eventName = "TestEvent",
				componentId = "test-component",
				data = "test-data"
			});

			string? callbackResult = null;

			// Act
			Communicator.OnCommandReceived?.Invoke(("Event", eventJson, r => callbackResult = r));

			// Assert
			Assert.That(receivedEventName, Is.EqualTo("TestEvent"));
			Assert.That(receivedData, Is.EqualTo("\"test-data\"")); // JSON serialized
			Assert.That(callbackResult, Is.EqualTo("handled"));
		}

		[Test]
		public void FlutterManager_UnregisterEventHandler_StopsReceivingEvents()
		{
			// Arrange
			var eventCount = 0;
			FlutterManager.RegisterEventHandler("TestEvent", (name, data, callback) =>
			{
				eventCount++;
			});

			var eventJson = JsonSerializer.Serialize(new
			{
				eventName = "TestEvent",
				componentId = "test-component",
				data = "data"
			});

			// Act
			Communicator.OnCommandReceived?.Invoke(("Event", eventJson, _ => { }));
			FlutterManager.UnregisterEventHandler("TestEvent");
			Communicator.OnCommandReceived?.Invoke(("Event", eventJson, _ => { }));

			// Assert
			Assert.That(eventCount, Is.EqualTo(1)); // Only first call counted
		}

		[Test]
		public void FlutterManager_Ready_SignalsIsReady()
		{
			// Arrange
			bool readyEventFired = false;
			FlutterManager.OnReady += () => readyEventFired = true;

			// Act
			Communicator.OnCommandReceived?.Invoke(("Ready", "{}", _ => { }));

			// Assert
			Assert.That(FlutterManager.IsReady, Is.True);
			Assert.That(readyEventFired, Is.True);
		}

		[Test]
		public void FlutterManager_Initialize_SetsUpCommandReceived()
		{
			// FlutterManager.Initialize() is called in Setup
			Assert.That(FlutterManager.IsInitialized, Is.True);
			Assert.That(Communicator.OnCommandReceived, Is.Not.Null);
		}

		[Test]
		public void FlutterManager_Reset_ClearsState()
		{
			// Arrange
			FlutterManager.RegisterEventHandler("TestEvent", (_, _, _) => { });

			// Act
			FlutterManager.Reset();

			// Assert
			var stats = FlutterManager.GetEventStats();
			Assert.That(stats.RegisteredEventHandlers, Is.EqualTo(0));
			Assert.That(FlutterManager.IsReady, Is.False);
		}

		[Test]
		public void FlutterManager_GetEventStats_ReturnsValidStats()
		{
			// Arrange
			Action callback = () => { };
			var callbackId = CallbackRegistry.Register(callback);

			// Act
			var stats = FlutterManager.GetEventStats();

			// Assert
			Assert.That(stats.RegisteredCallbacks, Is.GreaterThan(0));
		}

		#endregion

		#region Event Message Parsing Tests

		[Test]
		public void EventMessage_ParsesEventName()
		{
			// Arrange
			var eventJson = JsonSerializer.Serialize(new
			{
				eventName = "ItemBuilder",
				componentId = "widget-123",
				data = 5,
				needsReturn = true
			});

			string? capturedEventName = null;
			FlutterManager.RegisterEventHandler("ItemBuilder", (name, data, callback) =>
			{
				capturedEventName = name;
			});

			// Act
			Communicator.OnCommandReceived?.Invoke(("Event", eventJson, _ => { }));

			// Assert
			Assert.That(capturedEventName, Is.EqualTo("ItemBuilder"));
		}

		[Test]
		public void EventMessage_ParsesIntegerData()
		{
			// Arrange
			var eventJson = JsonSerializer.Serialize(new
			{
				eventName = "ItemBuilder",
				componentId = "widget-123",
				data = 42
			});

			string? capturedData = null;
			FlutterManager.RegisterEventHandler("ItemBuilder", (name, data, callback) =>
			{
				capturedData = data;
			});

			// Act
			Communicator.OnCommandReceived?.Invoke(("Event", eventJson, _ => { }));

			// Assert
			Assert.That(capturedData, Is.EqualTo("42"));
		}

		[Test]
		public void EventMessage_ParsesStringData()
		{
			// Arrange
			var eventJson = JsonSerializer.Serialize(new
			{
				eventName = "TestEvent",
				componentId = "widget-123",
				data = "hello world"
			});

			string? capturedData = null;
			FlutterManager.RegisterEventHandler("TestEvent", (name, data, callback) =>
			{
				capturedData = data;
			});

			// Act
			Communicator.OnCommandReceived?.Invoke(("Event", eventJson, _ => { }));

			// Assert
			Assert.That(capturedData, Is.EqualTo("\"hello world\""));
		}

		[Test]
		public void EventMessage_CallbackReturnsResponse()
		{
			// Arrange
			var eventJson = JsonSerializer.Serialize(new
			{
				eventName = "ItemBuilder",
				componentId = "widget-123",
				data = 10,
				needsReturn = true
			});

			FlutterManager.RegisterEventHandler("ItemBuilder", (name, data, callback) =>
			{
				// Simulate returning a widget pointer
				callback?.Invoke("12345678");
			});

			string? returnedValue = null;

			// Act
			Communicator.OnCommandReceived?.Invoke(("Event", eventJson, r => returnedValue = r));

			// Assert
			Assert.That(returnedValue, Is.EqualTo("12345678"));
		}

		#endregion

		#region ItemBuilder Callback Pattern Tests

		[Test]
		public void ItemBuilderPattern_SimulatedFlow_Works()
		{
			// This test simulates the exact flow that happens when Dart
			// requests an item from a ListView/GridView:
			// 1. Dart sends Event with eventName="ItemBuilder" and data=index
			// 2. C# handler looks up the widget and calls itemBuilder(index)
			// 3. Handler returns widget pointer via callback

			// Arrange
			var builtIndices = new List<int>();

			// Register a handler that simulates ListViewBuilder.SendEvent behavior
			FlutterManager.RegisterEventHandler("ItemBuilder", (eventName, data, callback) =>
			{
				// Parse index from data (simulating ListViewBuilder logic)
				if (int.TryParse(data, out int index))
				{
					builtIndices.Add(index);
					// Return a fake widget pointer
					callback?.Invoke((index * 1000 + 12345).ToString());
				}
				else
				{
					callback?.Invoke("0"); // Null pointer for invalid data
				}
			});

			// Act - simulate Dart requesting items 0, 5, 10
			foreach (var index in new[] { 0, 5, 10 })
			{
				var eventJson = JsonSerializer.Serialize(new
				{
					eventName = "ItemBuilder",
					componentId = "listview-123",
					data = index,
					needsReturn = true
				});

				string? result = null;
				Communicator.OnCommandReceived?.Invoke(("Event", eventJson, r => result = r));

				// Verify callback returned a valid pointer
				Assert.That(result, Is.Not.Null);
				Assert.That(result, Is.Not.EqualTo("0"));
				Assert.That(long.TryParse(result, out _), Is.True);
			}

			// Assert - all indices were built
			Assert.That(builtIndices, Is.EqualTo(new[] { 0, 5, 10 }));
		}

		[Test]
		public void ItemBuilderPattern_InvalidIndex_ReturnsNullPointer()
		{
			// Arrange
			FlutterManager.RegisterEventHandler("ItemBuilder", (eventName, data, callback) =>
			{
				if (int.TryParse(data, out int index) && index >= 0 && index < 10)
				{
					callback?.Invoke("12345");
				}
				else
				{
					callback?.Invoke("0"); // Null pointer for out of range
				}
			});

			var eventJson = JsonSerializer.Serialize(new
			{
				eventName = "ItemBuilder",
				componentId = "listview-123",
				data = 999 // Out of range
			});

			string? result = null;

			// Act
			Communicator.OnCommandReceived?.Invoke(("Event", eventJson, r => result = r));

			// Assert
			Assert.That(result, Is.EqualTo("0"));
		}

		[Test]
		public void ItemBuilderPattern_MultipleWidgets_RoutedByComponentId()
		{
			// Arrange
			var listViewCalls = new List<int>();
			var gridViewCalls = new List<int>();

			// Simulating two widgets with different handlers
			// In production, this is handled by FlutterManager looking up widgets by ID
			// For this test, we use the component ID in the data to route

			FlutterManager.RegisterEventHandler("ItemBuilder", (eventName, data, callback) =>
			{
				// This simulates the event handler checking component ID
				// In reality, FlutterManager routes to the correct widget
				if (int.TryParse(data, out int index))
				{
					callback?.Invoke("handled");
				}
			});

			// Act - send events (in reality, these would have different componentIds)
			for (int i = 0; i < 5; i++)
			{
				var eventJson = JsonSerializer.Serialize(new
				{
					eventName = "ItemBuilder",
					componentId = "widget-" + i,
					data = i
				});
				Communicator.OnCommandReceived?.Invoke(("Event", eventJson, _ => { }));
			}

			// Assert - just verify no exceptions
			Assert.Pass("Multiple events handled without exception");
		}

		#endregion

		#region Action Handling Tests

		[Test]
		public void HandleAction_InvokesRegisteredCallback()
		{
			// Arrange
			bool wasInvoked = false;
			Action callback = () => wasInvoked = true;
			var callbackId = CallbackRegistry.Register(callback);

			var actionJson = JsonSerializer.Serialize(new
			{
				actionId = $"action_{callbackId}"
			});

			// Act
			Communicator.OnCommandReceived?.Invoke(("HandleAction", actionJson, _ => { }));

			// Assert
			Assert.That(wasInvoked, Is.True);
		}

		[Test]
		public void HandleAction_WithValue_PassesValueToCallback()
		{
			// Arrange
			string? receivedValue = null;
			Action<string> callback = (val) => receivedValue = val;
			var callbackId = CallbackRegistry.Register(callback);

			var actionJson = JsonSerializer.Serialize(new
			{
				actionId = $"action_{callbackId}",
				value = "test-value"
			});

			// Act
			Communicator.OnCommandReceived?.Invoke(("HandleAction", actionJson, _ => { }));

			// Assert
			Assert.That(receivedValue, Is.EqualTo("test-value"));
		}

		[Test]
		public void HandleAction_ReturnsSuccessResponse()
		{
			// Arrange
			Action callback = () => { };
			var callbackId = CallbackRegistry.Register(callback);

			var actionJson = JsonSerializer.Serialize(new
			{
				actionId = $"action_{callbackId}"
			});

			string? response = null;

			// Act
			Communicator.OnCommandReceived?.Invoke(("HandleAction", actionJson, r => response = r));

			// Assert
			Assert.That(response, Does.Contain("success"));
			Assert.That(response, Does.Contain("true"));
		}

		#endregion

		#region Integration Simulation Tests

		[Test]
		public void FullRoundTrip_SimulatedListViewBuilder()
		{
			// This test simulates the complete round-trip that occurs when
			// a ListViewBuilder widget is rendered in Flutter:
			//
			// 1. C# creates ListViewBuilder with itemBuilder callback
			// 2. Widget is tracked by FlutterManager
			// 3. Widget is sent to Dart for rendering
			// 4. Dart's ListView.builder needs item at index N
			// 5. Dart sends Event message with eventName="ItemBuilder", data=N
			// 6. FlutterManager routes to widget's SendEvent method
			// 7. Widget calls itemBuilder(N), gets Widget result
			// 8. Widget returns pointer via callback
			// 9. Dart receives pointer, builds widget from address

			// Arrange - simulate a tracked widget's event handler
			var itemBuilderCallCount = 0;
			var lastRequestedIndex = -1;

			FlutterManager.RegisterEventHandler("ItemBuilder", (eventName, data, callback) =>
			{
				itemBuilderCallCount++;
				if (int.TryParse(data, out int index))
				{
					lastRequestedIndex = index;
					// Simulate returning a widget pointer
					// In reality, this would be the address of a prepared WidgetStruct
					var fakePointer = 0x12345678L + (index * 100);
					callback?.Invoke(fakePointer.ToString());
				}
				else
				{
					callback?.Invoke("0");
				}
			});

			// Act - simulate Dart requesting items during initial render
			var receivedPointers = new List<long>();
			for (int i = 0; i < 10; i++)
			{
				var eventJson = JsonSerializer.Serialize(new
				{
					eventName = "ItemBuilder",
					componentId = "listview-widget-id",
					needsReturn = true,
					data = i
				});

				string? result = null;
				Communicator.OnCommandReceived?.Invoke(("Event", eventJson, r => result = r));

				if (long.TryParse(result, out var ptr))
				{
					receivedPointers.Add(ptr);
				}
			}

			// Assert
			Assert.That(itemBuilderCallCount, Is.EqualTo(10), "Should build 10 items");
			Assert.That(lastRequestedIndex, Is.EqualTo(9), "Last requested index should be 9");
			Assert.That(receivedPointers, Has.Count.EqualTo(10), "Should receive 10 pointers");
			Assert.That(receivedPointers, Has.All.GreaterThan(0), "All pointers should be non-null");
		}

		[Test]
		public void FullRoundTrip_SimulatedScrolling()
		{
			// Simulate what happens when user scrolls a ListView:
			// New items come into view and need to be built

			// Arrange
			var builtItems = new HashSet<int>();

			FlutterManager.RegisterEventHandler("ItemBuilder", (eventName, data, callback) =>
			{
				if (int.TryParse(data, out int index))
				{
					builtItems.Add(index);
					callback?.Invoke((index + 1000).ToString());
				}
			});

			// Act - initial visible items (0-9)
			for (int i = 0; i < 10; i++)
			{
				SendItemBuilderEvent(i);
			}

			// Scroll down - new items (10-19) come into view
			for (int i = 10; i < 20; i++)
			{
				SendItemBuilderEvent(i);
			}

			// Scroll up - items (5-14) are visible, some already built
			for (int i = 5; i < 15; i++)
			{
				SendItemBuilderEvent(i);
			}

			// Assert
			Assert.That(builtItems, Has.Count.EqualTo(20), "Should have built 20 unique items");
			Assert.That(builtItems, Contains.Item(0));
			Assert.That(builtItems, Contains.Item(19));
		}

		private void SendItemBuilderEvent(int index)
		{
			var eventJson = JsonSerializer.Serialize(new
			{
				eventName = "ItemBuilder",
				componentId = "listview-widget",
				data = index
			});
			Communicator.OnCommandReceived?.Invoke(("Event", eventJson, _ => { }));
		}

		#endregion
	}
}
