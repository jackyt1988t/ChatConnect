using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
	class Program
	{
		static void Main(string[] args)
		{
			IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8081);
			Socket slistener = new Socket(point.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			slistener.Connect(point);

			byte[] header = Encoding.UTF8.GetBytes(
				"\r\n" +
				"Connection: keep\r\n" +
						   " -alive\r\n\r\n"
			);

			slistener.Send(header);
		}
	}
}
