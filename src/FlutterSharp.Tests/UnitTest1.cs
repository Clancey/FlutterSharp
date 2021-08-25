using NUnit.Framework;
using Flutter.Structs;
namespace FlutterSharp.Tests
{
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void Test1()
		{
			new FlutterObjectStruct();
			new SingleChildRenderObjectWidgetStruct();
			new MultiChildRenderObjectWidgetStruct();
			Assert.Pass();
		}
	}
}