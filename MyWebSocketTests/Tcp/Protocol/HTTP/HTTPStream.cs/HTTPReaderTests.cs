using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyWebSocket.Tcp.Protocol.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
/*using System.Text;
using System.Threading.Tasks;

namespace MyWebSocket.Tcp.Protocol.HTTP.Tests
{
	[TestClass()]
	public class HTTPReaderTests
	{
		[TestMethod]
		[ExpectedException(typeof(HTTPException))]
		public void TestHTTPReader()
		{
			//
			// TODO: добавьте здесь логику теста
			//
			HTTPReader reader = new HTTPReader(2400)
			{
				Header = new Header()
			};
			byte[] header = Encoding.UTF8.GetBytes(
				 "GET / HTTP1.1\ra"
			);
			reader.Write(header, 0, header.Length);
			reader.ReadHead();
		}
		[TestMethod]
		public void TestHTTPReader1()
		{
			//
			// TODO: добавьте здесь логику теста
			//
			HTTPReader reader = new HTTPReader(2400)
			{
				Header = new Header()
			};
			byte[] header = Encoding.UTF8.GetBytes(
				 "GET / HTTP1.1\r\n\r\n"
			);
			reader.Write(header, 0, header.Length);
			if (reader.ReadHead() == -1)
			{
				Assert.Fail("заголвоки верные");
				return;
			}

			HTTPReader.STSTR = 1;

			reader._Frame.Clear();
			reader.Header = new Header();
			reader.Write(header, 0, header.Length);
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
				Header = new Header()
			};
			byte[] header = Encoding.UTF8.GetBytes(
				"\r\n" +
				"Test param: test\r\n\r\n"
			);
			reader.Write(header, 0, header.Length);
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
				Header = new Header()
			};
			byte[] header = Encoding.UTF8.GetBytes(
				"\r\n" +
				"Test: test header\r\n\r\n"
			);
			reader.Write(header, 0, header.Length);
			if (reader.ReadHead() == -1)
			{
				Assert.Fail("заголвоки верные");
				return;
			}

			HTTPReader.PARAM = 1;

			reader._Frame.Clear();
			reader.Header = new Header();
			reader.Write(header, 0, header.Length);
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
				Header = new Header()
			};
			byte[] header = Encoding.UTF8.GetBytes(
				"\r\n" +
				"Connection: keep\r\n" +
						   " -alive\r\n\r\n"
			);
			reader.Write(header, 0, header.Length);
			if (reader.ReadHead() == -1)
			{
				Assert.Fail("заголвоки верные");
				return;
			}
			if (reader.Header.Connection != "keep-alive")
			{
				Assert.Fail("заголвоки верные");
				return;
			}

			HTTPReader.VALUE = 1;

			reader._Frame.Clear();
			reader.Header = new Header();
			reader.Write(header, 0, header.Length);
			reader.ReadHead();

			HTTPReader.VALUE = 1024;
		}
	}
}*/