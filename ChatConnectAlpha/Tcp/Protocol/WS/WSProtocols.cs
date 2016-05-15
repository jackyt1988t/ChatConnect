using System;
using System.Net;
using System.Net.Sockets;

namespace ChatConnect.Tcp.Protocol.WS
{
    class WSProtocol : WS
    {
		private const long PING = 10 * 1000 * 1000 * 20;
		private const long WAIT = 10 * 1000 * 1000 * 20;
		private const long MAXCOUNTS = 1000 * 256;

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
		/// Объект для проверки данных заголвока
		/// </summary>
		public WSChecks WSChecking
		{
			get;
			set;
		}
		/// <summary>
		/// Потококбезопасное событие которое наступает при проходе по циклу
		/// </summary>
		public override event PHandlerEvent EventWork
        {
            add
            {
                lock (SyncEvent)
                    __EventWork += value;

            }
            remove
            {
                lock (SyncEvent)
                    __EventWork -= value;
            }
        }
		/// <summary>
		/// Потококбезопасное событие которое наступает когда приходит фрейм пинг
		/// </summary>
		public override event PHandlerEvent EventPing
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
		/// Потококбезопасное событие которое наступает когда приходит фрейм понг
		/// </summary>
		public override event PHandlerEvent EventPong
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
		/// <summary>
		/// Потококбезопасное событие которое наступает когда приходит фрейм с данными
		/// </summary>
		public override event PHandlerEvent EventData
        {
            add
            {
                lock (SyncEvent)
                    __EventData += value;

            }
            remove
            {
                lock (SyncEvent)
                    __EventData -= value;
            }
        }
		/// <summary>
		/// Потококбезопасное событие которое наступает когда приходит заврешающий фрейм
		/// </summary>
		public override event PHandlerEvent EventError
        {
            add
            {
                lock (SyncEvent)
                    __EventError += value;

            }
            remove
            {
                lock (SyncEvent)
                    __EventError -= value;
            }
        }
		/// <summary>
		/// Потококбезопасное событие которое наступает когда приходит при ошибке протокола
		/// </summary>
		public override event PHandlerEvent EventClose
        {
            add
            {
                lock (SyncEvent)
                    __EventClose += value;

            }
            remove
            {
                lock (SyncEvent)
                    __EventClose -= value;
            }
        }
		/// <summary>
		/// Потококбезопасное событие которое наступает когда приходит часть отправленных данных
		/// </summary>
		public override event PHandlerEvent EventChunk
		{
			add
			{
				lock (SyncEvent)
					__EventChunk += value;

			}
			remove
			{
				lock (SyncEvent)
					__EventChunk -= value;
			}
		}
		/// <summary>
		/// Потококбезопасное событие которое наступает при открвтии соединения когда получены заголвоки
		/// </summary>
		public override event PHandlerEvent EventConnect
        {
            add
            {
                lock (SyncEvent)
                    __EventConnect += value;

            }
            remove
            {
                lock (SyncEvent)
                    __EventConnect -= value;
            }
        }
		private bool _init;
		private long _tnextsend;
		private long _tlastdata;
		private object SyncEvent;
		protected WSBinnary __ReqPing;
		protected WSBinnary __Binnary;

		private event PHandlerEvent __EventWork;
		private event PHandlerEvent __EventPing;
		private event PHandlerEvent __EventPong;
		private event PHandlerEvent __EventData;
		private event PHandlerEvent __EventError;
		private event PHandlerEvent __EventClose;
		private event PHandlerEvent __EventChunk;
		private event PHandlerEvent __EventConnect;
		
		/// <summary>
		/// Ининцилазириует класс протокола WS без подключения
		/// </summary>
		public WSProtocol()
		{
			Sync       = new object();
			State      = States.Connection;
			Response   = new Header();
			SyncEvent  = new object();
			__ReqPing  = new WSBinnary(WSFrameRFC76.PING);
			WSChecking = new WSChecks();
			TaskResult = new TaskResult();
			TaskResult.Protocol   =   TaskProtocol.WSRFC76;

			if (Deb)
			{
				EventData += WSDebug.DebugText;
				EventClose += WSDebug.DebugClose;
				EventConnect += WSDebug.DebugConnect;
			}
		}
		/// <summary>
		/// Инициализрует класс протокола WS с указанным обработчиком
		/// </summary>
		/// <param name="http">протокол  http</param>
		/// <param name="connect">обрабтчик собятия подключения</param>
		public WSProtocol(IProtocol http, PHandlerEvent connect)
        {
			Tcp        = http.Tcp;
			Sync       = new object();
			State      = 
					States.Connection;
			Request    = http.Request;
			Response   = new Header();
			SyncEvent  = new object();
			__ReqPing  = new WSBinnary(WSFrameRFC76.PING);
			WSChecking = new WSChecks();
			TaskResult = new TaskResult();
			__EventConnect		 +=   connect;
			TaskResult.Protocol   =   TaskProtocol.WSRFC76;

			if (Deb)
			{
				EventData += WSDebug.DebugText;
				EventClose += WSDebug.DebugClose;
				EventConnect += WSDebug.DebugConnect;
			}
		}

		protected void OnEventWork()
        {
            PHandlerEvent e;
            lock (SyncEvent)
                e = __EventWork;
            if (e != null)
                e(this, PEventArgs.EmptyArgs);
        }
		protected void OnEventData(WSBinnary frame)
        {	
        	string m = "Получен фрейм Data";
            PHandlerEvent e;
            lock (SyncEvent)
                e = __EventData;
            if (e != null)
                e(this, new PEventArgs(  S_DATA, m, frame  ));
                
            __Binnary = null;
        }
		protected void OnEventPing(WSBinnary frame)
        {	
        	string m = "Получен фрейм Ping";
        	Pong(    __Binnary.Buffer     );
            PHandlerEvent e;
            lock (SyncEvent)
                e = __EventPing;
            if (e != null)
                e(this, new PEventArgs(  S_PING, m, frame  ));

			__Binnary = null;
		}
		protected void OnEventPong(WSBinnary frame)
        {
        	string m = "Получен фрейм Pong";

			if (__Binnary.Buffer.Length != __ReqPing.Buffer.Length)
				throw new WSException("Неврное тело ответа Pong", WsError.PongBodyIncorrect,
																    WSCloseNum.ProtocolError);
			for (   int i = 0; i < __Binnary.Buffer.Length; i++   )
			{
				if (  __Binnary.Buffer[i] != __ReqPing.Buffer[i]  )
					throw new WSException("Неврное тело ответа Pong", WsError.PongBodyIncorrect, 
																	    WSCloseNum.ProtocolError);	
			}
        	
            PHandlerEvent e;
            lock (SyncEvent)
                e = __EventPong;
            if (e != null)
                e(this, new PEventArgs(  S_PONG, m, frame  ));

			
			__Binnary = null;
			__ReqPing.Buffer = null;
		}
		protected void OnEventClose(WSClose _close)
        {        		
        	string m = _close.ToString();
            
            PHandlerEvent e;
            lock (SyncEvent)
                e = __EventClose;
            if (e != null)
                e(this, new PEventArgs(  S_CLOSE, m, _close  ));
        }
		protected void OnEventChunk(WSBinnary frame)
		{
			string m = "Получена часть данных";

			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventChunk;
			if (e != null)	
				e(this, new PEventArgs(  S_CHUNK, m, frame  ));
		}
		protected void OnEventError( WSException _error )
        {
			string m = _error.ToString();
			PHandlerEvent e;
            lock (SyncEvent)
                e = __EventError;
            if (e != null)
                e(this, new PEventArgs( S_ERROR, m, _error  ));
        }
		protected void OnEventConnect()
        {
			string m = "Подключение было установлено";
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventConnect;
			if (e != null)
				e( this, new PEventArgs( S_CONNECT, m, null));
		}
		protected override void Work()
		{
			/*if (__Binnary != null)
			{
				if ((__Binnary.Mofieid.Ticks + WAIT) < DateTime.Now.Ticks)
					throw new WSException("Не получен фрейм Find", WsError.BodyWaitLimit,
																	 WSCloseNum.PolicyViolation);
			}
			if (__ReqPing.Buffer != null)
			{
				if ((__ReqPing.Mofieid.Ticks + PING) < DateTime.Now.Ticks)
					throw new WSException("Не получен фрейм Pong", WsError.PingNotResponse,
																     WSCloseNum.PolicyViolation);
			}
			else if ((__ReqPing.Mofieid.Ticks + PING) < DateTime.Now.Ticks)
			{
				__ReqPing = new WSBinnary(  WSFrame.PING  );
				__ReqPing.AddBinary( Encoding.UTF8.GetBytes(  DateTimeOffset.Now.ToString()  ) );
					 Ping(  __ReqPing.Buffer  );
			}*/
			/*   WORK   */
			OnEventWork();
		}
		protected override void Send()
		{			
			if (Response.SegmentsBuffer.Count == 0)
				return;
			if (Response.SegmentsBuffer.Count > 300)
				throw new WSException("Ошибка при отправки в Socket", WsError.BufferLimitLength,
																	     WSCloseNum.ServerError);
			if (_tnextsend   >   DateTime.Now.Ticks)
				return;

			while (Response.SegmentsBuffer.Count > 0)
			{
				int recive = 0;
				int length = 0;
				int counts = 0;
				byte[ ] buffer;
				lock (Response.SegmentsBuffer)
				{
					buffer = Response.
							   SegmentsBuffer.Dequeue();
				}
				
				SocketError error;
				lock (Sync)
				{
			while (recive < buffer.Length)
			{
				length = buffer.Length - recive;
				recive += Tcp.Send(buffer, recive, length, SocketFlags.None, out error);
				if (error != SocketError.Success)
				{
					if (error != SocketError.WouldBlock
							&& error != SocketError.NoBufferSpaceAvailable)
					{
						throw new WSException("Ошибка при отправке данных в Socket", error,
																	WSCloseNum.ServerError);
					}
					
					lock (Response.SegmentsBuffer)
					{
						int count = Response.SegmentsBuffer.Count;
					
						Response.SegmentsBuffer.Enqueue(  buffer  );
						for (int i = 0; i < count; i++)
						{
							Response.SegmentsBuffer.Enqueue(Response.SegmentsBuffer.Dequeue());
						}
					}
					_tnextsend = DateTime.Now.Ticks + 10 * 1000 * 5;
					return;
				}
			}
			_tnextsend = DateTime.Now.Ticks;
			if ((counts += buffer.Length) > MAXCOUNTS)
				break;
				}
			}
		}
		protected override void Data()
		{
			int recive = 0;
			int length = Tcp.Available;
			if (length > 4000)
				length = 4000;
			byte[] buffer = new byte[length];

			SocketError error;
			recive += Tcp.Receive(buffer, recive, length, SocketFlags.None, out error);
			if (   error != SocketError.Success && error != SocketError.WouldBlock   )
			{
				throw new WSException(  "Ошибка при чтении данных из Socket", error,
															 WSCloseNum.ServerError  );
			}
			_tlastdata = DateTime.Now.Ticks;
			// Обрабатываем по прротоколу RFC76
			HandlerFrame(  buffer, 0, recive  );
		}
		protected override void Error(WSException error)
		{
			OnEventError(error);
		}
		protected override void Close()
		{
            OnEventClose(close);
		}
		protected override void Connection()
		{
			OnEventConnect();

			MessageSend(Response.ToByte());
			if (Request.SegmentsBuffer.Count > 0)
			{
				byte[] buffer =
						 Request.SegmentsBuffer.Dequeue();
				// Обрабатываем данные по прротоколу RFC76
				HandlerFrame(  buffer, 0, buffer.Length  );
			}
		}
		protected override void MessageSend(SArray data)
		{
			throw new NotSupportedException();
		}
		protected override void MessageSend(byte[] buffer)
		{
			int recive = 0;
			int length = 0;
			SocketError error;
			lock (Response.SegmentsBuffer)
			{
				if (Response.SegmentsBuffer.Count > 0)
				{
					Response.SegmentsBuffer.Enqueue(buffer);
					return;
				}
			}
			lock (Sync)
			{
				while (   recive < buffer.Length   )
				{
					length = buffer.Length  -  recive;
					recive += Tcp.Send(buffer, recive, length, SocketFlags.None, out error);
					if (error != SocketError.Success)
					{

						if (error != SocketError.WouldBlock
								&& error != SocketError.NoBufferSpaceAvailable)
						{
							throw new WSException ("Ошибка при записи данных в Socket", error, 
																	   WSCloseNum.ServerError);
						}
						lock (Response.SegmentsBuffer)
							Response.SegmentsBuffer.Enqueue(buffer);
						return;
					}
				}
				
			}
			_tnextsend = DateTime.Now.Ticks;
		}
		protected virtual void HandlerFrame(byte[] buffer, int recive, int length)
		{
		
		}		
    }
	
}