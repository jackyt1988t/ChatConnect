using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using MyWebSocket.Tcp.Protocol.HTTP;

namespace MyWebSocket.Tcp.Protocol.WS
{
	public abstract class WSProtocol : HTTProtocol
	{
		private static readonly string S_WORK = "work";
		private static readonly string S_SEND = "send";
		private static readonly string S_DATA = "data";
		private static readonly string S_PING = "ping";
		private static readonly string S_PONG = "pong";
		private static readonly string S_CHUNK = "chunk";
		private static readonly string S_ERROR = "error";
		private static readonly string S_CLOSE = "close";
		private static readonly string S_CONNECT = "connect";
		/// <summary>
		/// Размер приемного буффера
		/// </summary>
		public static int SizeRead = 1000 * 32;
		/// <summary>
		/// Размер отсылочного буффера
		/// </summary>
		public static int SizeWrite = 1000 * 32;
		/// <summary>
		/// 
		/// </summary>
		public static bool DebugPrint;

		/// <summary>
		/// Событие которое наступает когда приходит фрейм пинг
		/// </summary>
		public event PHandlerEvent EventPing
		{
			add
			{
				lock (SyncEvent)
					__EventPing += value;

			}
			remove
			{
				lock (SyncEvent)
					__EventPing -= value;
			}
		}
		/// <summary>
		/// Событие которое наступает когда приходит фрейм понг
		/// </summary>
		public event PHandlerEvent EventPong
		{
			add
			{
				lock (SyncEvent)
					__EventPong += value;

			}
			remove
			{
				lock (SyncEvent)
					__EventPong -= value;
			}
		}

		private  event PHandlerEvent __EventPing;
		private  event PHandlerEvent __EventPong;

		public WSProtocol(Socket tcp) : 
			base(tcp)
			{
			
			}

		protected internal void OnEventPing(IContext cntx)
		{
			//string m = "Получен фрейм Ping";
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventPing;
			if (e != null)
				e(this, new PEventArgs(S_PING, string.Empty, cntx));
		}
		protected internal void OnEventPong(IContext cntx)
		{
			//string m = "Получен фрейм Pong";
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventPong;
			if (e != null)
				e(this, new PEventArgs(S_PONG, string.Empty, cntx));
		}
	}
}
