using System;
using MyWebSocket.Tcp.Protocol;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestMyStream
{
	[TestClass]
	public class UnitTest
	{
		[TestMethod]
		public void TestMyStream()
		{
			byte[] buffer = new byte[1024];
			MyStream stream = new MyStream(1024);

			stream.Write(buffer, 0, 1024);
			stream.Read(buffer, 0, 512);
			stream.Write(buffer, 0, 512);

			stream.Position = 1024;
			stream.SetLength( 1024 );

			stream.Position = 1024;

			Assert.AreEqual(stream.Length, 0, "Успех");
		}
	}
}
