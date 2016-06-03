using System.Text;

using System.Net;
using System.Net.Sockets;

using System.Threading;
using System;

namespace MyWebSocket.Tcp.Protocol.WS
{
	abstract class WS : BaseProtocol
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
		public static int SizeRead = 1000 * 128;
		/// <summary>
		/// Размер отсылочного буффера
		/// </summary>
		public static int SizeWrite = 1000 * 32;

		public static bool Debug;
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
		/// Информации о закрытии соединения
		/// </summary>
		public CloseWS close
		{
			get;
			protected set;
		}
		/// <summary>
		/// Текщий статус протокола
		/// </summary>
override
		public  States State
		{
			protected set
			{
				state = (int)value;
			}
			get
			{
				return (States)state;
			}
		}
		public WSEssion Session
		{
			get;
			protected set;
		}
		public TaskResult TaskResult
		{
			get;
			protected set;
		}
		public WSPingControl PingControl;
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
		static
			  public event PHandlerEvent EventConnect
		{
			add
			{
				__EventConnect += value;

			}
			remove
			{
				__EventConnect -= value;
			}
		}
		private object SyncEvent = new object();
		private  event PHandlerEvent __EventWork;
		private  event PHandlerEvent __EventPing;
		private  event PHandlerEvent __EventPong;
		private  event PHandlerEvent __EventData;
		private  event PHandlerEvent __EventError;
		private  event PHandlerEvent __EventClose;
		private  event PHandlerEvent __EventChunk;
		private static event PHandlerEvent __EventConnect;

		public WS()
		{
			Sync = new object();
			State =
				States.Connection;
			Response = new Header();
			TaskResult = new TaskResult();
			PingControl = new WSPingControl();
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
		/// Отправляет фрейм пинг текущему подключению
		/// </summary>
		/// <param name="message">массив данных для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Ping(byte[] message)
		{
			return Message(message, WSOpcod.Ping, WSFin.Last);
		}
		/// <summary>
		/// Отправляет фрейм понг текущему подключению
		/// </summary>
		/// <param name="message">массив данных для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Pong(byte[] message)
		{
			return Message(message, WSOpcod.Pong, WSFin.Last);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="numcode"></param>
		/// <returns></returns>
		public bool Close(WSClose numcode)
		{
			string buffer = CloseWS.Message[numcode];
			byte[] wsbody = Encoding.UTF8.GetBytes(buffer);
			byte[] wsdata = new byte [2  +  wsbody.Length];
		  	       wsdata[0] = (byte) ((int)numcode >> 08);
		   	       wsdata[1] = (byte) ((int)numcode >> 00);
		   	       wsbody.CopyTo(wsdata, 2);
			
			bool rtrn = false;
			lock(Sync)
			{
				if (state > 4)
				{
					if (state == 5
					    && !close.Res)
					        close.Res = true;
					rtrn = false;
				}
				else if (close == null)
				{

				rtrn = Message(wsdata, WSOpcod.Close, WSFin.Last);

					close = new CloseWS(    "Server", numcode    )
					{
						Res = true
					};
					state = 5;
				}
			}
			return rtrn;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="numcode"></param>
		/// <returns></returns>
		public bool сlose(WSClose numcode)
		{
			string buffer = CloseWS.Message[numcode];
			byte[] wsbody = Encoding.UTF8.GetBytes(buffer);
			byte[] wsdata = new byte [2  +  wsbody.Length];
		  	       wsdata[0] = (byte) ((int)numcode >> 00);
		   	       wsdata[1] = (byte) ((int)numcode >> 08);
		   	       wsbody.CopyTo(wsdata, 2);
			bool rtrn = false;
			lock(Sync)
			{
				string _point = 
				    Session.Address.ToString();
				if (state > 4)
				{
					if (state == 5
					    && !close.Req)
					        close.Req = true;
					rtrn = false;
				}
				else if (close == null)
				{

				rtrn = Message(wsdata, WSOpcod.Close, WSFin.Last);
					
					close = new CloseWS(     _point, numcode     )
					{
						Req = true
					};
					state = 5;
				}
			}
			return rtrn;
		}
		/// <summary>
		/// Отправляет текстовый фрейм текущему подключению
		/// </summary>
		/// <param name="message">строка данных для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Message(string message)
		{
			byte[] _buffer = Encoding.UTF8.GetBytes(message);
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
		/// Отправляет данные текущему подключению
		/// </summary>
		/// <param name="message">массив байт для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Message(byte[] message, int start, int write)
		{
			lock (Sync)
			{
				if (state > 4)
					return false;
				SocketError error;
				if ((error = Write(message, start, write)) != SocketError.Success)
				{
					if (error != SocketError.WouldBlock
						&& error != SocketError.NoBufferSpaceAvailable)
					{
						/*        Текущее подключение было отключено сброшено или разорвано         */
						if (error == SocketError.Disconnecting || error == SocketError.ConnectionReset
															   || error == SocketError.ConnectionAborted)
							Close(WSClose.Abnormal);
						else
							exc(new WSException("Ошибка записи данных.", error, WSClose.ServerError));
						return false;
					}
				}
			}
			return true;
		}
		/// <summary>
		/// Отправляет текстовый фрейм текущему подключению
		/// </summary>
		/// <param name="message">массив байт для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Message(byte[] message, WSOpcod opcod, WSFin fin)
		{
			return Message(message, 0, message.Length, opcod, fin);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message">массив данных для отправки</param>
		/// <param name="recive">начальная позиция в массиве данных</param>
		/// <param name="length">количество которое необходимо отправить</param>
		/// <param name="opcod">опкод который необходимо отправить</param>
		/// <param name="fin">указывает окончательный фрагмент сообщения</param>
		/// <returns></returns>
		public abstract bool Message(byte[] message, int recive, int length, WSOpcod opcod, WSFin fin);
		/// <summary>
		/// Функция 1 прохода обработки ws протокола соединения
		/// </summary>
		/// <returns>информация о дальнейшей обработки соединения</returns>
override
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
					Проверяет сокет были получены данные или нет. Если 
					данные были получены Запускает функцию для получения данных.
					В случае если соединеие было закрыто назначается 
					соотвествующий обработчик, если нет утсанавливает обработчик 
					отправки данных.
				==================================================================*/
					if (Interlocked.CompareExchange(ref state, 1, 0) != 0)
						return TaskResult;
					read();
					Data();
				/*==================================================================
					Проверяет возможность отправки данных. Если данные можно 
					отправить запускает функцию для отправки данных, в случае 
					если статус не был изменен выполняет переход к следующему 
					обраотчику, обработчику обработки пользовательски данных.
				==================================================================*/
					if (Interlocked.CompareExchange(ref state, 2, 1) != 1)
						return TaskResult;
					write();
					if (Interlocked.CompareExchange(ref state, 0, 2) == 2)
						return TaskResult;
				}
					if (state == 3)
					{
						Connection(Request, Response);
						Interlocked.CompareExchange(ref state, 0, 3);
					}
				/*==================================================================
					Если соединение было закрыто правильно пытается отправить
					оставшиеся данные в течении одной секунды после чего 
					закрывает соединение.
				==================================================================*/
					if (state == 5)										 
					{
						if ((!close.Req || !Writer.Empty) 
								     && close.AwaitTime.Seconds < 2)
						{
							if (!close.Req)
							{
								Read();
								Data();
							}
							if (!Writer.Empty)
								write();
							return TaskResult;
						}
						state = 7;
					}
						if (state == 7)
						{
							Close(close);
							TaskResult.Option  =  TaskOption.Delete;
							if (Tcp != null)
								Tcp.Close();
						}
			}
			catch (WSException err)
			{
				exc(err);
			}
			return TaskResult;
		}
		/// <summary>
		/// Возвращает протокол установленного соединения поверх tcp/ip
		/// </summary>
		/// <returns>строковое представление текущего протокола</returns>
		public override string ToString()
		{
			return "WS";
		}
		
		private void exc(WSException err)
		{
			lock(Sync)
			{

				if (state < 5 
						&& err.Close != WSClose.ServerError)
				{
					state = 4;
					Error(err);
					Close(err.Close);
				}
				else if (state < 7)
				{
					state = 4;
					Error(err);
					close = new CloseWS("Server", err.Close);
				}
				Interlocked.CompareExchange(ref state, 7, 4);
			}
		}
		/// <summary>
		/// 
		/// </summary>
		private void read()
		{
			SocketError error;
			if ((error = Read()) != SocketError.Success)
			{
				if (error != SocketError.WouldBlock
					&& error != SocketError.NoBufferSpaceAvailable)
				{
					/*        Текущее подключение было отключено сброшено или разорвано         */
					if (error == SocketError.Disconnecting || error == SocketError.ConnectionReset
									       || error == SocketError.ConnectionAborted)
						Close(WSClose.Abnormal);
					else
						exc(new WSException("Ошибка записи данных.", error, WSClose.ServerError));
				}
			}
		}
		/// <summary>
		/// Отправляет сообщение
		/// </summary>
		/// <param name="data">Данные</param>
		private void write()
		{
			if (Writer.Empty)
				return;
			SocketError error;
			if ((error = Send()) !=  SocketError.Success)
			{
				if (error != SocketError.WouldBlock
					&& error != SocketError.NoBufferSpaceAvailable)
				{
					Writer.Position = Writer.Length;
					/*        Текущее подключение было отключено сброшено или разорвано         */
					if (error == SocketError.Disconnecting || error == SocketError.ConnectionReset
									       || error == SocketError.ConnectionAborted)
						Close(WSClose.Abnormal);
					else
						exc(new WSException("Ошибка записи данных.", error, WSClose.ServerError));
				}
			}
		}		
		protected void OnEventWork()
		{
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventWork;
			if (e != null)
				e(null, PEventArgs.EmptyArgs);
		}
		protected void OnEventData(WSData frame)
		{
			//string m = "Получен фрейм Data";
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventData;
			if (e != null)
				e(this, new PEventArgs(S_DATA, string.Empty, frame));
		}
		protected void OnEventPing(WSData frame)
		{
			//string m = "Получен фрейм Ping";
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventPing;
			if (e != null)
				e(this, new PEventArgs(S_PING, string.Empty, frame));
		}
		protected void OnEventPong(WSData frame)
		{
			//string m = "Получен фрейм Pong";
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventPong;
			if (e != null)
				e(this, new PEventArgs(S_PONG, string.Empty, frame));
		}
		protected void OnEventClose(CloseWS _close)
		{
			//string m = _close.ToString();
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventClose;
			if (e != null)
				e(this, new PEventArgs(S_CLOSE, string.Empty, _close));
		}
		protected void OnEventChunk(WSData frame)
		{
			//string m = "Получена часть данных";
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventChunk;
			if (e != null)
				e(this, new PEventArgs(S_CHUNK, string.Empty, frame));
		}
		protected void OnEventError(WSException _error)
		{
			string m = _error.ToString();
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventError;
			if (e != null)
				e(this, new PEventArgs(S_ERROR, string.Empty, _error));
		}
		protected void OnEventConnect(IHeader request, IHeader response)
		{
			//string m = "Подключение было установлено";
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventConnect;
			if (e != null)
				e(this, new PEventArgs(S_CONNECT, string.Empty, null));
		}
		/// <summary>
		/// 
		/// </summary>
		protected abstract void Work();
		/// <summary>
		/// 
		/// </summary>
		protected abstract void Data();
		/// <summary>
		/// 
		/// </summary>
		protected abstract void Close(CloseWS close);
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
