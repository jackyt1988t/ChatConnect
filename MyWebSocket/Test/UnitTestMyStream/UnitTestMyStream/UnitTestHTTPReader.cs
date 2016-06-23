using System;
using System.Text;
using System.Collections.Generic;

using MyWebSocket.Tcp.Protocol;
using MyWebSocket.Tcp.Protocol.HTTP;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestMyStream
{
	/// <summary>
	/// Сводное описание для UnitTestHTTPReader
	/// </summary>
	[TestClass]
	public class UnitTestHTTPReader
	{
		public UnitTestHTTPReader()
		{
			//
			// TODO: добавьте здесь логику конструктора
			//
		}

		#region Дополнительные атрибуты тестирования
		//
		// При написании тестов можно использовать следующие дополнительные атрибуты:
		//
		// ClassInitialize используется для выполнения кода до запуска первого теста в классе
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// TestInitialize используется для выполнения кода перед запуском каждого теста 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// TestCleanup используется для выполнения кода после завершения каждого теста
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		[ExpectedException(typeof(HTTPException))]
		public void TestHTTPReader()
		{
			//
			// TODO: добавьте здесь логику теста
			//
			HTTPReader reader = new HTTTPReader(2400)
			{
				header = new Header();
			}
			byte[] header = Encoding.UTF8.GetBytes("
				 GET / HTTP1.1\r			
			");
			reader.Write( header, 0, header.Length );
			reader.ReadHead();
		}
		[TestMethod]
		[ExpectedException(typeof(HTTPException))]
		public void TestHTTPReader1()
		{
			//
			// TODO: добавьте здесь логику теста
			//
			HTTPReader reader = new HTTPReader(2400)
			{
				header = new Header();
			}
			byte[] header = Encoding.UTF8.GetBytes("
				 "GET / HTTP1.1\r\n\r\n"			
			");
			reader.Write( header, 0, header.Length );
			if (reader.ReadHead() == -1)
			{
				Assert.Fail("Переданы верные заголовки");
				return;
			}

			HTTPREADER.STSTR = 1;

			reader.Write( header, 0, header.Length );			
			    reader.ReadHead();

			HTTPReader.STSTR = 1024;
		}
		[TestMethod]
		[ExpectedException(typeof(HTTPException))]
		public void TestHTTPReader2()
		{
			//
			// TODO: добавьте здесь логику теста
			//
			HTTPReader reader = new HTTPReader(2400)
			{
				header = new Header();
			}
			byte[] header = Encoding.UTF8.GetBytes("
				"\r\n" +
				"Test param: test\r\n\r\n"			
			");
			reader.Write( header, 0, header.Length );
			    reader.ReadHead();
		}
		[TestMethod]
		[ExpectedException(typeof(HTTPException))]
		public void TestHTTPReader3()
		{
			//
			// TODO: добавьте здесь логику теста
			//
			HTTPReader reader = new HTTPReader(2400)
			{
				header = new Header();
			}
			byte[] header = Encoding.UTF8.GetBytes(
				"\r\n" +
				"Test: test header\r\n\r\n"			
			);
			reader.Write( header, 0, header.Length );
			if (reader.ReadHead() == -1)
			{
				Assert.Fail("Переданы верные заголовки");
				return;
			}

			HTTPReader.PARAM = 1;

			reader.Write( header, 0, header.Length );			
			    reader.ReadHead();

			HTTPReader.PARAM = 1024;
		}
		[TestMethod]
		[ExpectedException(typeof(HTTPException))]
		public void TestHTTPReader4()
		{
			//
			// TODO: добавьте здесь логику теста
			//
			HTTPReader reader = new HTTPReader(2400)
			{
				header = new Header();
			}
			byte[] header = Encoding.UTF8.GetBytes(
				"\r\n" +
				"Connection: keep\r\n" +
				           " -alive\r\n\r\n";		
			);
			reader.Write( header, 0, header.Length );
			if (reader.ReadHead() == -1)
			{
				Assert.Fail("Переданы верные заголовки");
				return;
			}
			if (reader.header.Connection != "keep-alive")
			{
				Assert.Fail("Переданы верные заголовки");
				return;
			}
			HTTPReader.VALUE = 1;

			reader.Write( header, 0, header.Length );			
			    reader.ReadHead();

			HTTPReader.VALUE = 1024;
		}
	}
}
