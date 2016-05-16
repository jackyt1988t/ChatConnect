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
        public event PHandlerEvent EventWork;
		/// <summary>
		/// Событие которое наступает когда приходит фрейм пинг
		/// </summary>
		public event PHandlerEvent EventPing;
		/// <summary>
		/// Событие которое наступает когда приходит фрейм понг
		/// </summary>
		public event PHandlerEvent EventPong;
		/// <summary>
		/// Событие которое наступает когда приходит фрейм с данными
		/// </summary>
		public event PHandlerEvent EventData;
		/// <summary>
		/// Событие которое наступает когда приходит заврешающий фрейм
		/// </summary>
		public event PHandlerEvent EventClose;
		/// <summary>
		/// Событие которое наступает когда приходит при ошибке протокола
		/// </summary>
		public event PHandlerEvent EventError;
		/// <summary>
		/// Событие которое наступает когда приходит кусок отправленных данных
		/// </summary>
		public event PHandlerEvent EventChunk;
		/// <summary>
		/// Событие которое наступает при открвтии соединения когда получены заголвоки
		/// </summary>
		public event PHandlerEvent EventConnect;
		
		private event PHandlerEvent __EventWork;
		private event PHandlerEvent __EventPing;
		private event PHandlerEvent __EventPong;
		private event PHandlerEvent __EventData;
		private event PHandlerEvent __EventError;
		private event PHandlerEvent __EventClose;
		private event PHandlerEvent __EventChunk;
		private event PHandlerEvent __EventConnect;

		public bool Msg(SArray buffers)
		{
			if (state == 4 || state == 5 || state == 7)
				return false;
			try
			{
				MessageSend(buffers);
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
		/// Отправляет данные текущему подключению
		/// </summary>
		/// <param name="message">массив байт для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Msg(byte[] message)
		{
			if (state == 4 || state == 5 || state == 7)
				return false;
			try
			{
				MessageSend(message);
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
  async public void File(string pathlog)
		{
			await Task.Run(() =>
			{
				file(pathlog);
			});
		}
		/// <summary>
		/// Отправка файла поверх протокола ws
		/// </summary>
		/// <param name="pathlog">путь к файлу</param>
		public void file(string pathlog)
		{
			int i = 0;
			int sleep = 30;
			int maxlen = 1000 * 128;
			using (FileStream sr = new FileStream(pathlog, FileMode.Open, FileAccess.Read))
			{
				int count = (int)(sr.Length / maxlen);
				int length = (int)(sr.Length - count * maxlen);
				try
				{
					while (i++ < count)
					{
						int __read = 0;
						int recive = 0;
						byte[ ] header;
						if (  i == 1  )
						{
							recive = 7;
							header = new byte[7];
							header[0] = 0;
							BitConverter.GetBytes(sr.Length).CopyTo(header, 1);
						}
						else
						{
							recive = 1;
							header = new byte[1];
							header[0] = 1;
						}
						byte[] buffer = new byte [recive];
							   header.CopyTo( buffer, 0 );
						while (__read < maxlen)
						{
							__read = sr.Read(buffer, (recive + __read), 
													 (maxlen - __read));
						}
						Frame (buffer, WSFrameRFC76.BINARY, 1);
						if (Response.SegmentsBuffer.Count < 10)
						{
							if (sleep  > 30)
								sleep -= 30;
						}
						else
								sleep += 30;
						Thread.Sleep( sleep );
					}
					if (length > 0)
					{
						int __read = 0;
						int recive = 0;
						byte[ ] header;
						if (  i == 1  )
						{
							recive = 7;
							header = new byte[7];
							header[0] = 0;
							BitConverter.GetBytes(sr.Length).CopyTo(header, 1);
						}
						else
						{
							recive = 1;
							header = new byte[1];
							header[0] = 1;
						}
						byte[] buffer = new byte[recive];
						       header.CopyTo( buffer, 0 );
						while (__read < maxlen)
						{
							__read = sr.Read(buffer, (recive + __read),
													 (maxlen - __read));
						}
						Frame (buffer, WSFrameRFC76.BINARY, 1);
					}
				}
				catch (Exception exc)
				{
					byte[] header = new byte[1];
					header[0] = 2;
					Frame(header, WSFrameRFC76.BINARY, 1);
					Log.Logout.AddMessage(exc.Message, "Log/log.log", Log.Log.Fatail);
				}
			}
		}
		/// <summary>
		/// Отправляет текстовый фрейм текущему подключению
		/// </summary>
		/// <param name="message">массив байт для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Send(   byte[] message   )
		{
			return Frame(   message, WSFrameRFC76.TEXT, 1  );
		}
		/// <summary>
		/// Отправляет текстовый фрейм текущему подключению
		/// </summary>
		/// <param name="message">строка данных для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Send(   string message   )
		{
			return Send(Encoding.UTF8.GetBytes(message));
		}
		/// <summary>
		/// Отправляет фрейм пинг текущему подключению
		/// </summary>
		/// <param name="message">строка данных для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Ping(	string message	 )
		{
			return Ping(Encoding.UTF8.GetBytes(message));
		}
		/// <summary>
		/// Отправляет фрейм пинг текущему подключению
		/// </summary>
		/// <param name="message">массив данных для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Ping(	byte[] message	 )
        {
			return Frame(   message, WSFrameRFC76.PING, 1   );
        }
		/// <summary>
		/// Отправляет фрейм понг текущему подключению
		/// </summary>
		/// <param name="message">строка данных для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Pong(	string message	 )
		{
			return Pong(Encoding.UTF8.GetBytes(message));
		}
		/// <summary>
		/// Отправляет фрейм понг текущему подключению
		/// </summary>
		/// <param name="message">массив данных для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Pong(	byte[] message	 )
        {
			return Frame(   message, WSFrameRFC76.PONG, 1   );
		}
		/// <summary>
		/// Отправляет закрывающий фрейм с кодом 1000.
		/// </summary>
		/// <param name="message">строка данных для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Close(   string message   )
        {
			return Close(         message, 1000         );
        }
		/// <summary>
		/// Отправляет указанный ws фрейм текущему соединению
		/// </summary>
		/// <param name="wsframe">пользовательский ws фрейм</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Frame(  IWSFrame wsframe  )
		{
			return Msg(     wsframe.GetDataFrame()     );
		}
		/// <summary>
		/// Отправляет завршающия фрейм и закрывает соединение
		/// </summary>
		/// <param name="message">строка данных для отправки</param>
		/// <param name="number">код закрытия соденинеия 1000-1012</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Close( string message, int number)
        {
			if (number < 1000 || number > 1012)
				throw new ArgumentNullException( "number" );

			State = States.Close;
			close = new WSClose(Address(), (WSCloseNum)number);

			if (string.IsNullOrEmpty(message))
				message = WSCloseMsg.Message((WSCloseNum)number);
			byte[] _wsbody = Encoding.UTF8.GetBytes(  message  );
            byte[] _wsdata = new byte [  2  +  _wsbody.Length  ];

			_wsdata[0] = (byte)(number  >>  08);
			_wsdata[1] = (byte)(number  <<  24 >> 24);
				         _wsbody.CopyTo( _wsdata, 2 );

			return Frame(  _wsdata, WSFrameRFC76.CLOSE, 1  );
		}
		public void Reset(Socket socket, IHeader requset)
		{
			if (State != States.Disconnect)
				throw new InvalidOperationException("State is not disconnect");
			if (socket == null || !socket.Connected)
				throw new ArgumentNullException("Socket is null or disconnect");
			if (requset == null || string.IsNullOrEmpty( requset.StartString ))
				throw new ArgumentNullException("Headers is null or empty values");

			Tcp      = socket;			
			State    = States.Connection;
			Request  = requset;
			Response = new Header();
			TaskResult.Option = TaskOption.Loop;
		}
		/// <summary>
		/// Отправляет указанный ws фрейм текущему соединению
		/// </summary>
		/// <param name="message">массив данных для отправки</param>
		/// <param name="pcod">опкод</param>
		/// <param name="find">бит заврешения отправки данных 0-1</param>
		/// <returns></returns>
		public bool Frame(byte[] message, int pcod, int find)
		{
				IWSFrame wsframe = new WSFrameRFC76();
						 wsframe.BitFind = find;
						 wsframe.BitPcod = pcod;
						 wsframe.DataBody = message;
						 wsframe.LengBody = message.Length;
						 wsframe.SetHeader();

			return Frame(wsframe);
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
					соотвествующий обработчик, если нет утсанавливает обработчик						отправки данных.
				==================================================================*/
					if (Interlocked.CompareExchange(ref state, 1, 0) != 0)
						return TaskResult;
					if (Tcp.Poll(0, SelectMode.SelectRead))
					{
						if (Tcp.Available > 0)
						{
							Data();
						}
						else
						{
							state = 5;
							close = new WSClose(Address(), 
												     WSCloseNum.Abnormal);
							return TaskResult;
						}
					}
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
						if (close.Host == "Server")
						{
							if (Response.SegmentsBuffer.Count > 0)
							{
								Send();
								return TaskResult;
							}
						}
							TaskResult.Option = TaskOption.Delete;
							if (Tcp.Connected
								&& !Tcp.Poll(0, SelectMode.SelectError))
								Tcp.Close();

						state = 7;
						Close();
					}
						if (state == 6)
						{
							Connection();
							Interlocked.CompareExchange(ref state, 0, 6);
						}
            }
            catch (WSException exc)
            {
				if (exc.Closes == WSCloseNum.ServerError)
					Response.SegmentsBuffer.Clear();					
				else
					Close( WSCloseMsg.Message(exc.Closes ), (int)exc.Closes);

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
		public string Address()
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
		/// <param name="error"></param>
		protected abstract void Error(WSException error);
		/// <summary>
		/// 
		/// </summary>
		protected abstract void Close();
		/// <summary>
		/// 
		/// </summary>
		protected abstract void Connection();
		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		protected abstract void MessageSend(SArray data);
		/// <summary>
		/// Отправляет сообщение
		/// </summary>
		/// <param name="data">Данные</param>
		protected abstract void MessageSend(byte[] data);
    }
}
