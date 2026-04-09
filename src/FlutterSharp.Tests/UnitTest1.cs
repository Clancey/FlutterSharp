using NUnit.Framework;

namespace FlutterSharp.Tests
{
	[TestFixture]
	public class BasicTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void TypesExist_SmokeTest()
		{
			// Basic smoke test - verify core types exist and are accessible
			// Note: We can't instantiate structs directly due to FFI pinning requirements
			Assert.That(typeof(Flutter.Structs.FlutterObjectStruct), Is.Not.Null);
			Assert.That(typeof(Flutter.Widgets.ListViewBuilder), Is.Not.Null);
			Assert.That(typeof(Flutter.Widgets.GridViewBuilder), Is.Not.Null);
			Assert.That(typeof(Flutter.Internal.FlutterManager), Is.Not.Null);
			Assert.That(typeof(Flutter.CallbackRegistry), Is.Not.Null);
			Assert.Pass();
		}
	}
}
