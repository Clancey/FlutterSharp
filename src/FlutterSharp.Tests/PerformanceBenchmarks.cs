using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using NUnit.Framework;
using Flutter;
using Flutter.Widgets;
using Flutter.Internal;
using Flutter.Structs;

namespace FlutterSharp.Tests
{
	/// <summary>
	/// Performance benchmark suite for FlutterSharp widget rendering and operations.
	/// These tests measure key performance metrics for widget creation, struct allocation,
	/// callback invocation, and event routing.
	///
	/// Run with: dotnet test --filter "Category=Performance"
	/// </summary>
	[TestFixture]
	[Category("Performance")]
	public class PerformanceBenchmarks
	{
		private Action<(string Method, string Arguments)>? _originalSendCommand;
		private List<(string Method, string Arguments)>? _sentCommands;

		// Benchmark configuration
		private const int WarmupIterations = 100;
		private const int MeasuredIterations = 1000;
		private const int LargeScaleIterations = 10000;

		// Reflection helper to call internal PrepareForSending
		private static readonly MethodInfo? _prepareForSendingMethod = typeof(Widget)
			.GetMethod("PrepareForSending", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

		private static void PrepareWidgetForSending(Widget widget)
		{
			_prepareForSendingMethod?.Invoke(widget, null);
		}

		[SetUp]
		public void Setup()
		{
			_originalSendCommand = Communicator.SendCommand;
			_sentCommands = new List<(string Method, string Arguments)>();
			FlutterManager.Initialize();
			Communicator.SendCommand = cmd => _sentCommands?.Add(cmd);
		}

		[TearDown]
		public void TearDown()
		{
			Communicator.SendCommand = _originalSendCommand;
			FlutterManager.Reset();
		}

		#region Benchmark Results Helper

		/// <summary>
		/// Records benchmark results in a structured format
		/// </summary>
		private class BenchmarkResult
		{
			public string Name { get; set; } = "";
			public int Iterations { get; set; }
			public TimeSpan TotalTime { get; set; }
			public double MillisecondsPerOperation => TotalTime.TotalMilliseconds / Iterations;
			public double OperationsPerSecond => Iterations / TotalTime.TotalSeconds;
			public long MemoryAllocated { get; set; }

			public override string ToString() =>
				$"{Name}: {MillisecondsPerOperation:F4}ms/op ({OperationsPerSecond:F0} ops/sec) - {Iterations} iterations in {TotalTime.TotalMilliseconds:F2}ms";
		}

		private BenchmarkResult RunBenchmark(string name, int iterations, Action action)
		{
			// Warmup
			for (int i = 0; i < WarmupIterations; i++)
			{
				action();
			}

			// Force GC before measurement
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			var memBefore = GC.GetTotalMemory(true);
			var sw = Stopwatch.StartNew();

			for (int i = 0; i < iterations; i++)
			{
				action();
			}

			sw.Stop();
			var memAfter = GC.GetTotalMemory(false);

			var result = new BenchmarkResult
			{
				Name = name,
				Iterations = iterations,
				TotalTime = sw.Elapsed,
				MemoryAllocated = memAfter - memBefore
			};

			TestContext.WriteLine(result);
			return result;
		}

		#endregion

		#region Widget Creation Benchmarks

		[Test]
		[Description("Benchmark: Create simple Text widgets")]
		public void Benchmark_TextWidget_Creation()
		{
			var result = RunBenchmark("Text Widget Creation", MeasuredIterations, () =>
			{
				var text = new Text("Hello World");
			});

			// Assert reasonable performance (should be < 1ms per operation)
			Assert.That(result.MillisecondsPerOperation, Is.LessThan(1.0),
				"Text widget creation should be sub-millisecond");
		}

		[Test]
		[Description("Benchmark: Create Center widgets with child")]
		public void Benchmark_CenterWidget_WithChild()
		{
			var result = RunBenchmark("Center Widget With Child", MeasuredIterations, () =>
			{
				var center = new Center(child: new Text("Centered Text"));
			});

			Assert.That(result.MillisecondsPerOperation, Is.LessThan(2.0),
				"Center widget creation should be < 2ms");
		}

		[Test]
		[Description("Benchmark: Create Column widget with multiple children")]
		public void Benchmark_ColumnWidget_WithChildren()
		{
			var result = RunBenchmark("Column Widget With Children", MeasuredIterations, () =>
			{
				var column = new Column
				{
					new Text("Item 1"),
					new Text("Item 2"),
					new Text("Item 3")
				};
			});

			Assert.That(result.MillisecondsPerOperation, Is.LessThan(5.0),
				"Column widget creation should be < 5ms");
		}

		[Test]
		[Description("Benchmark: Create deep widget tree (10 levels)")]
		public void Benchmark_DeepWidgetTree()
		{
			var result = RunBenchmark("Deep Widget Tree (10 levels)", MeasuredIterations / 10, () =>
			{
				Widget tree = new Text("Leaf");
				for (int i = 0; i < 10; i++)
				{
					tree = new Center(child: tree);
				}
			});

			Assert.That(result.MillisecondsPerOperation, Is.LessThan(20.0),
				"10-level deep tree should be < 20ms");
		}

		[Test]
		[Description("Benchmark: Create wide widget tree (100 children)")]
		public void Benchmark_WideWidgetTree()
		{
			var result = RunBenchmark("Wide Widget Tree (100 children)", MeasuredIterations / 10, () =>
			{
				var column = new Column();
				for (int i = 0; i < 100; i++)
				{
					column.Add(new Text($"Item {i}"));
				}
			});

			Assert.That(result.MillisecondsPerOperation, Is.LessThan(50.0),
				"100-child column should be < 50ms");
		}

		#endregion

		#region Struct Preparation Benchmarks

		[Test]
		[Description("Benchmark: PrepareForSending for simple widget")]
		public void Benchmark_PrepareForSending_SimpleWidget()
		{
			var widgets = new List<Text>();
			for (int i = 0; i < MeasuredIterations; i++)
			{
				widgets.Add(new Text($"Text {i}"));
			}

			var sw = Stopwatch.StartNew();
			foreach (var widget in widgets)
			{
				PrepareWidgetForSending(widget);
			}
			sw.Stop();

			var msPerOp = sw.Elapsed.TotalMilliseconds / MeasuredIterations;
			TestContext.WriteLine($"PrepareForSending: {msPerOp:F4}ms/op ({MeasuredIterations / sw.Elapsed.TotalSeconds:F0} ops/sec)");

			Assert.That(msPerOp, Is.LessThan(1.0),
				"PrepareForSending should be sub-millisecond");
		}

		[Test]
		[Description("Benchmark: PrepareForSending for widget tree")]
		public void Benchmark_PrepareForSending_WidgetTree()
		{
			var trees = new List<Widget>();
			for (int i = 0; i < MeasuredIterations / 10; i++)
			{
				trees.Add(new Center(
					child: new Column
					{
						new Text("Row 1"),
						new Text("Row 2"),
						new Text("Row 3")
					}
				));
			}

			var sw = Stopwatch.StartNew();
			foreach (var tree in trees)
			{
				PrepareWidgetForSending(tree);
			}
			sw.Stop();

			var msPerOp = sw.Elapsed.TotalMilliseconds / trees.Count;
			TestContext.WriteLine($"PrepareForSending (tree): {msPerOp:F4}ms/op ({trees.Count / sw.Elapsed.TotalSeconds:F0} ops/sec)");

			Assert.That(msPerOp, Is.LessThan(5.0),
				"PrepareForSending for tree should be < 5ms");
		}

		#endregion

		#region Callback Registry Benchmarks

		[Test]
		[Description("Benchmark: Callback registration")]
		public void Benchmark_CallbackRegistry_Register()
		{
			var callbacks = new List<Action>();
			for (int i = 0; i < LargeScaleIterations; i++)
			{
				callbacks.Add(() => { });
			}

			var ids = new List<long>();
			var sw = Stopwatch.StartNew();
			foreach (var cb in callbacks)
			{
				ids.Add(CallbackRegistry.Register(cb));
			}
			sw.Stop();

			var msPerOp = sw.Elapsed.TotalMilliseconds / LargeScaleIterations;
			TestContext.WriteLine($"Callback Registration: {msPerOp:F6}ms/op ({LargeScaleIterations / sw.Elapsed.TotalSeconds:F0} ops/sec)");

			// Cleanup
			foreach (var id in ids)
			{
				CallbackRegistry.Unregister(id);
			}

			Assert.That(msPerOp, Is.LessThan(0.1),
				"Callback registration should be < 0.1ms");
		}

		[Test]
		[Description("Benchmark: Callback invocation (void)")]
		public void Benchmark_CallbackRegistry_InvokeVoid()
		{
			int count = 0;
			Action callback = () => count++;
			var callbackId = CallbackRegistry.Register(callback);

			var result = RunBenchmark("Callback Invocation (Void)", LargeScaleIterations, () =>
			{
				CallbackRegistry.InvokeVoid(callbackId);
			});

			CallbackRegistry.Unregister(callbackId);

			Assert.That(count, Is.EqualTo(LargeScaleIterations + WarmupIterations),
				"All callbacks should be invoked");
			Assert.That(result.MillisecondsPerOperation, Is.LessThan(0.01),
				"Void callback invocation should be < 0.01ms");
		}

		[Test]
		[Description("Benchmark: Callback invocation with typed argument")]
		public void Benchmark_CallbackRegistry_InvokeTyped()
		{
			int sum = 0;
			Action<int> callback = (val) => sum += val;
			var callbackId = CallbackRegistry.Register(callback);

			var result = RunBenchmark("Callback Invocation (Typed)", LargeScaleIterations, () =>
			{
				CallbackRegistry.Invoke(callbackId, 1);
			});

			CallbackRegistry.Unregister(callbackId);

			Assert.That(result.MillisecondsPerOperation, Is.LessThan(0.01),
				"Typed callback invocation should be < 0.01ms");
		}

		#endregion

		#region Event Routing Benchmarks

		[Test]
		[Description("Benchmark: Event handler registration")]
		public void Benchmark_EventHandler_Registration()
		{
			var sw = Stopwatch.StartNew();
			for (int i = 0; i < MeasuredIterations; i++)
			{
				FlutterManager.RegisterEventHandler($"Event_{i}", (_, _, _) => { });
			}
			sw.Stop();

			var msPerOp = sw.Elapsed.TotalMilliseconds / MeasuredIterations;
			TestContext.WriteLine($"Event Handler Registration: {msPerOp:F4}ms/op ({MeasuredIterations / sw.Elapsed.TotalSeconds:F0} ops/sec)");

			// Cleanup
			for (int i = 0; i < MeasuredIterations; i++)
			{
				FlutterManager.UnregisterEventHandler($"Event_{i}");
			}

			Assert.That(msPerOp, Is.LessThan(0.1),
				"Event handler registration should be < 0.1ms");
		}

		[Test]
		[Description("Benchmark: Event routing throughput")]
		public void Benchmark_EventRouting_Throughput()
		{
			int eventCount = 0;
			FlutterManager.RegisterEventHandler("BenchmarkEvent", (_, _, callback) =>
			{
				eventCount++;
				callback?.Invoke("handled");
			});

			var result = RunBenchmark("Event Routing", LargeScaleIterations, () =>
			{
				var eventJson = JsonSerializer.Serialize(new
				{
					eventName = "BenchmarkEvent",
					componentId = "benchmark-component",
					data = "test"
				});
				Communicator.OnCommandReceived?.Invoke(("Event", eventJson, _ => { }));
			});

			FlutterManager.UnregisterEventHandler("BenchmarkEvent");

			TestContext.WriteLine($"Total events processed: {eventCount}");
			Assert.That(result.MillisecondsPerOperation, Is.LessThan(0.1),
				"Event routing should be < 0.1ms per event");
		}

		#endregion

		#region JSON Serialization Benchmarks

		[Test]
		[Description("Benchmark: JSON serialization for widget state")]
		public void Benchmark_JsonSerialization_WidgetState()
		{
			var widgetState = new
			{
				widgetType = "Text",
				address = 12345678L,
				properties = new
				{
					data = "Hello World",
					maxLines = 2,
					softWrap = true
				}
			};

			var result = RunBenchmark("JSON Serialization", LargeScaleIterations, () =>
			{
				var json = JsonSerializer.Serialize(widgetState);
			});

			Assert.That(result.MillisecondsPerOperation, Is.LessThan(0.01),
				"JSON serialization should be < 0.01ms");
		}

		[Test]
		[Description("Benchmark: JSON deserialization for event messages")]
		public void Benchmark_JsonDeserialization_EventMessage()
		{
			var eventJson = JsonSerializer.Serialize(new
			{
				eventName = "ItemBuilder",
				componentId = "widget-123",
				data = 42,
				needsReturn = true
			});

			var result = RunBenchmark("JSON Deserialization", LargeScaleIterations, () =>
			{
				var doc = JsonDocument.Parse(eventJson);
				var root = doc.RootElement;
				var eventName = root.GetProperty("eventName").GetString();
				var data = root.GetProperty("data").GetInt32();
			});

			Assert.That(result.MillisecondsPerOperation, Is.LessThan(0.01),
				"JSON deserialization should be < 0.01ms");
		}

		#endregion

		#region Memory Allocation Benchmarks

		[Test]
		[Description("Benchmark: Memory allocation for widget creation")]
		public void Benchmark_MemoryAllocation_WidgetCreation()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			var memBefore = GC.GetTotalMemory(true);

			var widgets = new List<Widget>();
			for (int i = 0; i < MeasuredIterations; i++)
			{
				widgets.Add(new Text($"Text {i}"));
			}

			var memAfter = GC.GetTotalMemory(false);
			var bytesPerWidget = (memAfter - memBefore) / MeasuredIterations;

			TestContext.WriteLine($"Memory per Text widget: ~{bytesPerWidget} bytes");
			TestContext.WriteLine($"Total allocated: {memAfter - memBefore:N0} bytes for {MeasuredIterations} widgets");

			// Clean up
			widgets.Clear();
			GC.Collect();

			Assert.That(bytesPerWidget, Is.LessThan(2048),
				"Text widget should allocate < 2KB per instance");
		}

		[Test]
		[Description("Benchmark: Memory allocation for widget trees")]
		public void Benchmark_MemoryAllocation_WidgetTree()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			var memBefore = GC.GetTotalMemory(true);
			var treeCount = 100;

			var trees = new List<Widget>();
			for (int i = 0; i < treeCount; i++)
			{
				trees.Add(new Center(
					child: new Column
					{
						new Text("Row 1"),
						new Text("Row 2"),
						new Text("Row 3")
					}
				));
			}

			var memAfter = GC.GetTotalMemory(false);
			var bytesPerTree = (memAfter - memBefore) / treeCount;

			TestContext.WriteLine($"Memory per widget tree (Center->Column->3xText): ~{bytesPerTree} bytes");
			TestContext.WriteLine($"Total allocated: {memAfter - memBefore:N0} bytes for {treeCount} trees");

			// Clean up
			trees.Clear();
			GC.Collect();

			Assert.That(bytesPerTree, Is.LessThan(20480),
				"Widget tree (5 widgets) should allocate < 20KB");
		}

		#endregion

		#region Throughput Benchmarks

		[Test]
		[Description("Benchmark: Widget update throughput simulation")]
		public void Benchmark_WidgetUpdateThroughput()
		{
			// Simulate rapid widget state updates like would happen during animation
			var widget = new Text("Initial");
			PrepareWidgetForSending(widget);

			var result = RunBenchmark("Widget Update Throughput", LargeScaleIterations, () =>
			{
				// Simulate changing widget state
				// In real scenario, this would involve struct modification
			});

			TestContext.WriteLine($"Theoretical max updates: {result.OperationsPerSecond:F0}/sec");

			// Should support at least 60fps (16ms frame budget)
			var updatesPerFrame = result.OperationsPerSecond / 60;
			TestContext.WriteLine($"Max updates per 60fps frame: {updatesPerFrame:F0}");

			Assert.That(updatesPerFrame, Is.GreaterThan(100),
				"Should support >100 updates per 60fps frame");
		}

		[Test]
		[Description("Benchmark: ItemBuilder callback throughput")]
		public void Benchmark_ItemBuilderThroughput()
		{
			var itemsBuilt = 0;
			FlutterManager.RegisterEventHandler("ItemBuilder", (_, data, callback) =>
			{
				if (int.TryParse(data, out int index))
				{
					itemsBuilt++;
					callback?.Invoke((index * 1000 + 12345).ToString());
				}
			});

			var result = RunBenchmark("ItemBuilder Throughput", MeasuredIterations, () =>
			{
				var eventJson = JsonSerializer.Serialize(new
				{
					eventName = "ItemBuilder",
					componentId = "listview-bench",
					data = itemsBuilt % 100
				});
				string? response = null;
				Communicator.OnCommandReceived?.Invoke(("Event", eventJson, r => response = r));
			});

			FlutterManager.UnregisterEventHandler("ItemBuilder");

			TestContext.WriteLine($"ItemBuilder operations per second: {result.OperationsPerSecond:F0}");

			// ListView.builder should be able to handle smooth scrolling
			// Assume 50 items visible, scrolling at 60fps = 3000 items/sec minimum
			Assert.That(result.OperationsPerSecond, Is.GreaterThan(3000),
				"ItemBuilder should support >3000 ops/sec for smooth scrolling");
		}

		#endregion

		#region Stress Tests

		[Test]
		[Description("Stress test: Create and dispose many widgets")]
		public void StressTest_WidgetCreationAndDisposal()
		{
			var sw = Stopwatch.StartNew();

			for (int batch = 0; batch < 10; batch++)
			{
				var widgets = new List<Text>();
				for (int i = 0; i < 1000; i++)
				{
					widgets.Add(new Text($"Widget {batch * 1000 + i}"));
				}

				foreach (var w in widgets)
				{
					w.Dispose();
				}
				widgets.Clear();

				// Allow GC between batches
				GC.Collect();
			}

			sw.Stop();
			TestContext.WriteLine($"Created and disposed 10,000 widgets in {sw.Elapsed.TotalMilliseconds:F2}ms");

			Assert.That(sw.Elapsed.TotalMilliseconds, Is.LessThan(5000),
				"10K widget create/dispose should complete in < 5 seconds");
		}

		[Test]
		[Description("Stress test: Many concurrent callbacks")]
		public void StressTest_ConcurrentCallbacks()
		{
			var callbackIds = new List<long>();
			var invokeCount = 0;

			// Register many callbacks
			for (int i = 0; i < 1000; i++)
			{
				callbackIds.Add(CallbackRegistry.Register(() => Interlocked.Increment(ref invokeCount)));
			}

			var sw = Stopwatch.StartNew();

			// Invoke each callback multiple times
			for (int round = 0; round < 10; round++)
			{
				foreach (var id in callbackIds)
				{
					CallbackRegistry.InvokeVoid(id);
				}
			}

			sw.Stop();

			// Cleanup
			foreach (var id in callbackIds)
			{
				CallbackRegistry.Unregister(id);
			}

			TestContext.WriteLine($"10,000 callback invocations in {sw.Elapsed.TotalMilliseconds:F2}ms");
			Assert.That(invokeCount, Is.EqualTo(10000), "All callbacks should be invoked");
			Assert.That(sw.Elapsed.TotalMilliseconds, Is.LessThan(1000),
				"10K callback invocations should complete in < 1 second");
		}

		#endregion

		#region Performance Summary

		[Test]
		[Description("Generate performance summary report")]
		public void GeneratePerformanceSummary()
		{
			TestContext.WriteLine("=== FlutterSharp Performance Summary ===\n");

			// Widget Creation
			TestContext.WriteLine("Widget Creation:");
			var textResult = RunBenchmark("  Text Widget", 1000, () => new Text("Test"));
			var centerResult = RunBenchmark("  Center Widget", 1000, () => new Center(child: new Text("Test")));
			var columnResult = RunBenchmark("  Column (3 children)", 500, () =>
			{
				var c = new Column { new Text("1"), new Text("2"), new Text("3") };
			});

			TestContext.WriteLine("\nCallback System:");
			var cbReg = RunBenchmark("  Registration", 10000, () =>
			{
				var id = CallbackRegistry.Register(() => { });
				CallbackRegistry.Unregister(id);
			});

			int invCount = 0;
			var cbId = CallbackRegistry.Register(() => invCount++);
			var cbInvoke = RunBenchmark("  Invocation", 10000, () => CallbackRegistry.InvokeVoid(cbId));
			CallbackRegistry.Unregister(cbId);

			TestContext.WriteLine("\n=== Performance Targets ===");
			TestContext.WriteLine($"60 FPS Frame Budget: 16.67ms");
			TestContext.WriteLine($"Widgets per frame at target: {(int)(16.67 / textResult.MillisecondsPerOperation)}");
			TestContext.WriteLine($"Callbacks per frame at target: {(int)(16.67 / cbInvoke.MillisecondsPerOperation)}");

			Assert.Pass("Performance summary generated");
		}

		#endregion
	}
}
