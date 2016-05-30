using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

using ChatConnect.Tcp.Protocol;
using System.Collections;

namespace ChatConnect.Tcp
{
	class WServer
	{
		public static int Pool = 100; 
		public static int SendSize = 16;
		public static int ReceiveSize = 64;

		public static ArrayList ArrSocket = new ArrayList();
		public static Dictionary<int, Agregator> ArrProtocol = new Dictionary<int, Agregator>();

		public int Iprotocol
		{
			get;
			private set;
		}

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
			Thread Thr = new Thread(ss);
			Thr.IsBackground = true;
			Thr.Start();
			Thread.Sleep(100);
			while (true)
			{
				Socket socket = null;
				try
				{

					socket = slistener.Accept();
					if (socket != null)
					{
						socket.NoDelay = false;
						socket.Blocking = false;
						socket.SendBufferSize = SendSize;
						socket.ReceiveBufferSize = ReceiveSize;
						Agregator ObjectProtocol = new Agregator(socket);
						lock (ArrSocket)
							ArrSocket.Add(socket);
						lock (ArrProtocol)
							ArrProtocol.Add((int)socket.Handle, ObjectProtocol);
					}

				}
				catch (Exception exc)
				{
					if (socket != null)
						socket.Dispose();

					Console.WriteLine(exc.Message);
				}

			}
		}
		public void ss()
		{
			while (true)
			{
				ArrayList write;
				if (ArrSocket.Count == 0)
				{
					Thread.Sleep(1);
					continue;
				}
				lock (ArrSocket)
					write = new ArrayList(ArrSocket);
				try
				{
				Socket.Select(write, null, null, 0);
				
					for (int i = 0; i < write.Count; i++)
					{
						Socket _socket = write[i] as Socket;
						if (_socket != null)
						{
							lock (ArrProtocol)
							{
								if (ArrProtocol.ContainsKey((int)_socket.Handle))
									ArrProtocol[(int)_socket.Handle].Protocol.ssdwrite = true;
							}
						}
					}
					Thread.Sleep(1);
				}
				catch (SocketException e)
				{
					;
				}
			}
		}
	}
}
