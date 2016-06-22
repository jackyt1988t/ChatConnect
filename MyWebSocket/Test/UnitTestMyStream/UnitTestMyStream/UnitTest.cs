using System;
using MyWebSocket.Tcp.Protocol;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestMyStream
{
	[TestClass]
	public class UnitTest
	{
		[TestMethod]
		public void TestWrite()
		{
			byte[] buffer = new byte[1024];
			MyStream stream = new MyStream(1024);

			stream.Write(buffer, 0, 1024);
			stream.Read(buffer, 0, 512);
			stream.Write(buffer, 0, 512);
			stream.Read(buffer, 0, 512);
			stream.Read(buffer, 0, 512);
			stream.Write(buffer, 0, 1024);

			Assert.AreEqual(stream.Clear, 0, "Clear");
			Assert.AreEqual(stream.Length, 1024, "Length");
		}
		[TestMethod]
		public void TestLength()
		{
			byte[] buffer = new byte[1024];
			MyStream stream = new MyStream(1024);

			stream.SetLength(1024);
			stream.Position = 512;
			stream.SetLength(512);
			stream.Position = 512;
			stream.Position = 512;
			stream.SetLength(1024);

			Assert.AreEqual(stream.Clear, 0, "Clear");
			Assert.AreEqual(stream.Length, 1024, "Length");
		}
		[TestMethod]
		public void TestResize()
		{
			byte[] buffer = new byte[1024];
			for (int i = 0; i < 1024; i++)
			{
				buffer[i] = i;
			}
			MyStream stream = new MyStream(1024);

			stream.SetLength(1024);
			stream.Position = 512;
			stream.SetLength(512);
			stream.Position = 512;
			stream.Position = 512;
			stream.Write(buffer, 0, 1024);
			
			stream.Resize(2048);
			Assert.AreEqual(stream.Count, 2048, "Count");
			Assert.AreEqual(stream.Clear, 1024, "Clear");
			Assert.AreEqual(stream.Length, 1024, "Length");

			stream.Read(buffer, 0, 1024);
			for (int i = 0; i < 1024; i++)
			{
				if (buffer[i] != i)
				{
					Assert.Fail("неверное значение в массиве");
					return;
				}
			}

			stream.Write(buffer, 0, 1024);
			
			stream.Resize(1024);
			Assert.AreEqual(stream.Count, 1024, "Count");
			Assert.AreEqual(stream.Clear, 0, "Clear");
			Assert.AreEqual(stream.Length, 1024, "Length");
		}
	}
}
