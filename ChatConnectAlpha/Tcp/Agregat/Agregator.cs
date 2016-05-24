using System;
		using System.Net.Sockets;
			using System.Threading;
	using System.Runtime.InteropServices;
		using System.Collections.Concurrent;

using ChatConnect.Tcp.Protocol;
using ChatConnect.Tcp.Protocol.WS;
using ChatConnect.Tcp.Protocol.HTTP;


namespace ChatConnect.Tcp
{
	/*[System.Security.SuppressUnmanagedCodeSecurityAttribute()]
	class Import
	{
		const short WSPOLLPRI = 0x0400;

		const short WSPOLLERR = 0x0001;
		const short WSPOLLHUP = 0x0002;		
		const short WSPOLLNVAL = 0x0004;
		const short WSPOLLWRNORM = 0x0010;
		const short WSPOLLWRBAND = 0x0020;
		const short WSPOLLRDNORM = 0x0100;
		const short WSPOLLRDBAND = 0x0200;
		
		[StructLayout(LayoutKind.Sequential)]
		public struct WSAPOLLFD
		{
			public IntPtr fd;
				public short events;
				public short revents;
		}
		#if PLATFORM_UNIX

		#else
		[DllImport("Ws2_32.dll", SetLastError = true)]
		public static extern int WSAPoll(
			[In, Out] WSAPOLLFD[] fdarray,
			[In] ulong nfds,
			[In] int wait);
		[DllImport("Ws2_32.dll", SetLastError = true)]
		public static extern int WSAGetLastError();
		#endif
	}*/
	class Agregator
	{
		public IProtocol Protocol;
		public static ConcurrentQueue<WS> Read;

		private static PHandlerEvent Connect;		
		private static ConcurrentQueue<Agregator> Container;

		static Agregator()
		{
			Read = new ConcurrentQueue<WS>();
			Container = new ConcurrentQueue<Agregator>();
		}
		public Agregator(Socket tcp)
		{
			Protocol = new HTTPProtocol( tcp, Connect );
			Container.Enqueue(this);
		}
		static public void loop()
		{
			short loop = 0;
			while (true)
			{
				try
				{
					WS ws = null;
					if (!Read.TryDequeue(out ws))
						Thread.Sleep(1);
					else
					{
						ws.Read();
						if (loop++ > 1000)
						{
							loop = 0;
							Thread.Sleep(1);
						}
						if ((int)ws.State < 4)
							Read.Enqueue(ws);
	}
				}
				catch (FieldAccessException exc)
				{
					Console.WriteLine("Обработчик: " + exc.Message);
				}
			}
		}
		static public void Loop()
		{
			short loop = 0;
			while ( true )
			{
				Agregator agregator = null;
				try
				{
					
					if (!Container.TryDequeue(out agregator))
						Thread.Sleep(1);
					else
					{
						agregator.TaskLoopHandler();
						if (loop++ > 500)
						{
							loop = 0;
							Thread.Sleep(1);
						}
					}
				}
				catch ( FieldAccessException exc )
				{
					Console.WriteLine("Обработчик: " + exc.Message);
				}
			}
		}
 static public void Connection(PHandlerEvent connect)
		{
			Connect = connect;
		}
		private void TaskLoopHandler()
		{
			TaskResult TaskResult = Protocol.TaskLoopHandlerProtocol();
			switch (TaskResult.Option)
			{
				case TaskOption.Loop:
					Container.Enqueue(this);
					break;
				case TaskOption.Protocol:
					if (TaskResult.Protocol == TaskProtocol.WSRFC76)
						Protocol = new WSProtocol7(Protocol);
					Read.Enqueue((WS)Protocol);
					Container.Enqueue(this);
					break;
				case TaskOption.Threading:
						Thread thr = new Thread( TaskLoopThreading );
							   thr.IsBackground = true;
							   thr.Start();
						break;
			}
		}
		private void TaskLoopThreading()
		{
					while (true)
					{
						TaskResult TaskResult = Protocol.TaskLoopHandlerProtocol();
						switch (TaskResult.Option)
						{
							case TaskOption.Loop:
								break;
							case TaskOption.Delete:
								return;
							default:
								throw new ArgumentException("TaskResult");
						}
						Thread.Sleep(1);
					}
		} 
	}
}