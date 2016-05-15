using System;
using System.Net;
using System.Net.Sockets;

	using System.Threading;

using ChatConnect.Tcp;
using ChatConnect.Tcp.Protocol;
using ChatConnect.Tcp.Protocol.WS;
using ChatConnect.Tcp.Protocol.HTTP;

using ChatConnect.WebModul;

namespace ChatConnect
{
    class Program
    {
        static void Main(string[] args)
        {
			int work = 0;
			int count = 1;
			while ( work++ < count )
			{
				Thread thr = new Thread(Agregator.Loop);
					   thr.IsBackground = true;
					   thr.Start();
				Thread.Sleep (100);
			}
			IPEndPoint point = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8081);
			Socket listener = new Socket(point.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			listener.Bind(point);
			listener.Listen (80);

			LingerOption LOption = new LingerOption(true, 0);
			
			Agregator.Connection(
				new PHandlerEvent((object sender, PEventArgs e) =>
			{
				switch (sender.ToString())
				{
					case "WS":
						WS ws = 
							(WS)sender;
						WebModule wm = 
							new WebModule(ws); 
						break;
					case "HTTP":
						HTTP http = (HTTP)sender;
						break;
				}
			}));
			while ( true )
			{
				Socket socket = null;
				try
				{

					socket = listener.Accept();
					socket.Blocking = false;
					socket.LingerState = LOption;
					socket.SendBufferSize = 1000 * 1024;
					socket.ReceiveBufferSize = 1000 * 1024;
					Agregator ObjectProtocol = new Agregator(socket);

				}
				catch ( Exception exc )
				{
					if ( socket != null )
						socket.Dispose();

					Console.WriteLine ( exc.Message );
				}
			}
		}
    }
}