using System;
using System.IO;
using System.Text;

	using System.Net;
	using System.Net.Sockets;

	using System.Threading;
	using System.Threading.Tasks;

namespace ChatConnect.Tcp.Protocol.WS
{
    abstract class WS : IProtocol
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

		static public bool Deb;
 		/// <summary>
		/// tcp/ip соединение
		/// </summary>
        public Socket Tcp
        {
            get;
            protected set;
        }
		/// <summary>
		/// Объект синхронизации
		/// </summary>
		public object Sync
		{
			get;
			protected set;
		}
		volatile int state;
		/// <summary>
		/// Текщий статус протокола
		/// </summary>
		public States State
        {
            get
			{
				return (States)state;
			}
            protected set
			{
				state = ( int )value;
			}
        }
		/// <summary>
		/// Информации о закрытии соединения
		/// </summary>
		public WSClose close
		{
			get;
			protected set;
		}
		/// <summary>
		/// 
		/// </summary>
abstract
		public WStream Reader
		{
			get;
		}
		/// <summary>
		/// 
		/// </summary>
abstract
		public WStream Writer
		{
			get;
		}
		/// <summary>
		/// Заголвоки полученные при открытии соединеия
		/// </summary>
		public IHeader Request
		{
			get;
			protected set;
		}
		/// <summary>
		/// Заголвоки которые были отправлены удаленной стороне
		/// </summary>
		public IHeader Response
		{
			get;
			protected set;
		}
        public TaskResult TaskResult
        {
        	get;
            protected set;
		}
		/// <summary>
		/// Событие которое наступает при проходе по циклу
		/// </summary>
		public event PHandlerEvent EventWork
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
		/// <summary>
		/// Событие которое наступает когда приходит фрейм с данными
		/// </summary>
		public event PHandlerEvent EventData
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
		/// Событие которое наступает когда приходит заврешающий фрейм
		/// </summary>
		public event PHandlerEvent EventClose
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
		/// Событие которое наступает когда приходит при ошибке протокола
		/// </summary>
		public event PHandlerEvent EventError
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
		/// Событие которое наступает когда приходит кусок отправленных данных
		/// </summary>
		public event PHandlerEvent EventChunk
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
		/// Событие которое наступает при открвтии соединения когда получены заголвоки
		/// </summary>
		public event PHandlerEvent EventConnect
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

		private object SyncEvent = new object();
		private event PHandlerEvent __EventWork;
		private event PHandlerEvent __EventPing;
		private event PHandlerEvent __EventPong;
		private event PHandlerEvent __EventData;
		private event PHandlerEvent __EventError;
		private event PHandlerEvent __EventClose;
		private event PHandlerEvent __EventChunk;
static	private event PHandlerEvent __EventConnect;
		
		/// <summary>
		/// Отправляет данные текущему подключению
		/// </summary>
		/// <param name="message">массив байт для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Send(byte[] message)
		{
			if (state == 4 || state == 5 || state == 7)
				return false;
			try
			{
				Write(message);
				return true;
			}
			catch (WSException exc)
			{
				state = 4;
				Error(exc);
				state = 5;
				close = new WSClose(    "Server", exc.Closes    );
				return false;
			}
		}
		/// <summary>
		/// Отправляет фрейм пинг текущему подключению
		/// </summary>
		/// <param name="message">строка данных для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Ping(string message)
		{
			return Ping(Encoding.UTF8.GetBytes(message));
		}
		/// <summary>
		/// Отправляет фрейм понг текущему подключению
		/// </summary>
		/// <param name="message">строка данных для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Pong(string message)
		{
			return Pong(Encoding.UTF8.GetBytes(message));
		}
		/// <summary>
		/// Отправляет текстовый фрейм текущему подключению
		/// </summary>
		/// <param name="message">строка данных для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Message(string message)
		{
			byte[] _buffer = Encoding.UTF8.GetBytes(
											    message);
			return Message(_buffer, WSOpcod.Text, WSFin.Last);
		}
		/// <summary>
		/// Отправляет текстовый фрейм текущему подключению
		/// </summary>
		/// <param name="message">строка данных для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Message(byte[] message)
		{
			return Message(message, WSOpcod.Binnary, WSFin.Last);
		}
		/// <summary>
		/// Функция 1 прохода обработки ws протокола соединения
		/// </summary>
		/// <returns>информация о дальнейшей обработки соединения</returns>
		public TaskResult TaskLoopHandlerProtocol()
		{
			try
			{
				/*==================================================================
					Запускает функцию обработки пользоватлеьских данных,в случае
					если статус не был изменен выполняет переход к следующему
					обраотчику, обработчику чтения данных.						   
				===================================================================*/
				if (state == 0)
				{
					Work();
				/*==================================================================
					Проверяет сокет были получены данные или нет если. Если 
					данные были получены Запускает функцию для получения данных.
					В случае если соединеие было закрыто назначается 
					соотвествующий обработчик, если нет утсанавливает обработчик 
					отправки данных.
				==================================================================*/
					if (Interlocked.CompareExchange(ref state, 1, 0) != 0)
						return TaskResult;					
					if (Tcp.Poll(0, SelectMode.SelectRead))
					{
						if (Tcp.Available > 0)
						{
							Read();
						}
						else
						{
							state = 5;
							close = new WSClose(Address(), 
												     WSCloseNum.Abnormal);
							return TaskResult;
						}
					}
					Data();
				/*==================================================================
					Проверяет возможность отправки данных. Если данные можно 
					отправить запускает функцию для отправки данных, в случае 
					если статус не был изменен выполняет переход к следующему 
					обраотчику, обработчику обработки пользовательски данных.
				==================================================================*/
					if (Interlocked.CompareExchange(ref state, 2, 1) != 1)
						return TaskResult;
					if (Tcp.Poll(0, SelectMode.SelectWrite))
					{
						Send();
					}
						else if (!Tcp.Connected)
						{
							state = 5;
							close = new WSClose(Address(),
												     WSCloseNum.Abnormal);
						}
					if (Interlocked.CompareExchange(ref state, 0, 2) == 2)
						return TaskResult;
				}						

						if (state == 5)
						{	
							Close(close);
							state = 7;
						}
						if (state == 6)
						{
							Connection(Request, Response);
							Interlocked.CompareExchange (ref state, 0, 6);
						}
						if (state == 7)
						{
							if (Tcp.Connected)
								Tcp.Close();
						}
            }
            catch (WSException exc)
            {
				/*if (exc.Closes == WSCloseNum.ServerError)
					Response.SegmentsBuffer.Clear();					
				else
					Close( WSCloseMsg.Message(exc.Closes ), (int)exc.Closes);*/

                state = 4;
				Error(  exc  );
				state = 5;
				close = new WSClose("Server", exc.Closes);
			}
			catch (ExecutionEngineException exc)
			{
				Log.Logout.AddMessage(exc.Message, "Log/log.log", Log.Log.Fatail);
			}
			return TaskResult;
        }
		/// <summary>
		/// Возвращает адрес удаленной стороны текущего соденинения
		/// </summary>
		/// <returns>строковое представдение ip адреса</returns>
		public virtual string Address()
		{
			return ((IPEndPoint)Tcp.RemoteEndPoint).Address.ToString();
		}
		/// <summary>
		/// Возвращает протокол установленного соединения поверх tcp/ip
		/// </summary>
		/// <returns>строковое представление текущего протокола</returns>
        public override string ToString()
        {
        	return "WS";
        }


		/// <summary>
		/// Отправляет фрейм пинг текущему подключению
		/// </summary>
		/// <param name="message">массив данных для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public abstract bool Ping(byte[] message);
		/// <summary>
		/// Отправляет фрейм понг текущему подключению
		/// </summary>
		/// <param name="message">массив данных для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public abstract bool Pong(byte[] message);
		/// <summary>
		/// Отправляет закрывающий фрейм с кодом 1000.
		/// </summary>
		/// <param name="message">строка данных для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public abstract bool Close(string message);
		/// <summary>
		/// Отправляет текстовый фрейм текущему подключению
		/// </summary>
		/// <param name="message">массив байт для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public abstract bool Message(byte[] message, WSOpcod opcod, WSFin fin);


		/// <summary>
		/// 
		/// </summary>
		private void Read()
		{
			int recive = 0;
			int length = Tcp.Available;
			if (length > 4000)
				length = 4000;
			byte[] buffer = new byte[length];

			SocketError error;
			recive += Tcp.Receive(buffer, recive, length, SocketFlags.None, out error);
			if ( error != SocketError.Success && error != SocketError.WouldBlock )
			{
				throw new WSException("Ошибка при чтении данных из Socketа...", error,
															   WSCloseNum.ServerError);
			}
			try
			{
				Reader.Write(buffer, 0, recive);
			}
			catch (IOException exc)
			{
				throw new WSException("Ошибка при чтении данных из Socketа...", 
														    WsError.BufferLimitLength,
															   WSCloseNum.ServerError);
			}
		}
		/// <summary>
		/// Отправляет сообщение
		/// </summary>
		/// <param name="data">Данные</param>
		private void Write(byte[] buffer)
		{
			int recive = 0;
			int length = buffer.Length;

			SocketError error;
			recive   =   Tcp.Send(buffer, recive, length, SocketFlags.None, out error);
			if ( error != SocketError.Success )
			{
				if (error != SocketError.WouldBlock
					&& error != SocketError.NoBufferSpaceAvailable)
					{
						throw new WSException("Ошибка записи данных в Socket.", error,
															   WSCloseNum.ServerError);
					}
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
				e(this, new PEventArgs(S_DATA, m, frame));
		}
		protected void OnEventPing(WSBinnary frame)
		{
			string m = "Получен фрейм Ping";
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventPing;
			if (e != null)
				e(this, new PEventArgs(S_PING, m, frame));
		}
		protected void OnEventPong(WSBinnary frame)
		{
			string m = "Получен фрейм Pong";
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventPong;
			if (e != null)
				e(this, new PEventArgs(S_PONG, m, frame));
		}
		protected void OnEventClose(WSClose _close)
		{
			string m = _close.ToString();

			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventClose;
			if (e != null)
				e(this, new PEventArgs(S_CLOSE, m, _close));
		}
		protected void OnEventChunk(WSBinnary frame)
		{
			string m = "Получена часть данных";

			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventChunk;
			if (e != null)
				e(this, new PEventArgs(S_CHUNK, m, frame));
		}
		protected void OnEventError(WSException _error)
		{
			string m = _error.ToString();
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventError;
			if (e != null)
				e(this, new PEventArgs(S_ERROR, m, _error));
		}
		protected void OnEventConnect(IHeader request, IHeader response)
		{
			string m = "Подключение было установлено";
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventConnect;
			if (e != null)
				e(this, new PEventArgs(S_CONNECT, m, null));
		}
		/// <summary>
		/// 
		/// </summary>
		protected abstract void Work();
		/// <summary>
		/// 
		/// </summary>
		protected abstract void Send();
		/// <summary>
		/// 
		/// </summary>
		protected abstract void Data();
		/// <summary>
		/// 
		/// </summary>
		protected abstract void Close(WSClose close);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="error"></param>
		protected abstract void Error(WSException error);
		
		/// <summary>
		/// 
		/// </summary>
		protected abstract void Connection(IHeader reauest, IHeader response);
    }
}