using System;
using System.Net;
using System.Net.Sockets;
	using System.Threading;

namespace MyWebSocket.Tcp
{
	class WServer
	{
		public static int Pool = 100; 
		public static int SendSize = 16;
		public static int ReceiveSize = 64;

		/// <summary>
		/// Запускает WebSocket сервер на указанном адрессе и порте
		/// </summary>
		/// <param name="adress">Сетевой адресс</param>
		/// <param name="port">Порт прослушивания</param>
		/// <param name="count">Количество рабочих потоков</param>
		public WServer(string adress, int port, int count)
		{
			IPEndPoint point = new IPEndPoint(IPAddress.Parse(adress), port);
			Socket slistener = new Socket(point.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			slistener.Bind(point);
			slistener.Listen(Pool);
			int work = 0;
			while (work++ < count)
			{
				Thread thr = new Thread(Agregator.Loop);
					   thr.IsBackground = true;
					   thr.Start();
				Thread.Sleep(100);
			}
			while (true)
			{
				Socket socket = null;
				try
				{

					socket = slistener.Accept();

					socket.NoDelay = false;
					socket.Blocking = false;
					socket.SendBufferSize = SendSize;
					socket.ReceiveBufferSize = ReceiveSize;
					Agregator ObjectProtocol = new Agregator(socket);

				}
				catch (Exception exc)
				{
					if (socket != null)
						socket.Dispose();

					Console.WriteLine(exc.Message);
				}

			}
		}
	}
}
