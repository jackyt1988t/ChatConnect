using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace MyWebSocket.Tcp.Protocol
{
	/// <summary>
	/// Базовая реализация протокола
	/// </summary>
	public class BaseProtocol : IProtocol, IDisposable
	{
		/// <summary>
		/// Время закрытия
		/// </summary>
		public static int TIMEWAIT = 2;
		/// <summary>
		/// Длинна чтения сокета
		/// </summary
		public static int LENGTHREAD = 1000 * 8;
		/// <summary>
		/// Длинна записи сокета
		/// </summary>
		public static int LENGTHWRITE = 1000 * 64;
		/// <summary>
		/// минимальный размер потока
		/// </summary>
		public static int MINLENGTHBUFFER = 1000 * 32;
		/// <summary>
		/// максимальный размер потока
		/// </summary>
		public static int MAXLENGTHBUFFER = 1000 * 1024;

		/// <summary>
		/// Длинна данных
		/// </summary>
		public long Len;
		/// <summary>
		/// tcp/ip сокет подключения
		/// </summary>
		public Socket Tcp
		{
			get;
			protected set;
		}
		/// <summary>
		/// Статус текущего протокола.
		/// </summary>
		public States State
		{
			get;
			protected set;
		}
        /// <summary>
        /// Функция обработчик цикла
        /// </summary>
        public Action Worker;
		/// <summary>
		/// Объект для синхронизации
		/// </summary>
		public object __ObSync
		{
			get;
			protected set;
		}
		/// <summary>
		/// Содержит Полученные заголовки
		/// </summary>
		public Header Request
		{
			get;
			protected set;
		}
		/// <summary>
		/// Содержит Отправленные заголовки
		/// </summary>
		public Header Response
		{
			get;
			protected set;
		}
        /// <summary>
        /// Вермя старта открытия соединения
        /// </summary>
        public TimeSpan TimeOpen
        {
            get;
            protected set;
        }
		/// <summary>
		/// Время начала закрытия соединения
		/// </summary>
		public TimeSpan TimeClose
		{
			get;
			protected set;
		}
		/// <summary>
		/// 
		/// </summary>
		public IContext ContextRs;
		/// <summary>
		/// 
		/// </summary>
		public IContext ContextRq;
		/// <summary>
		/// Последняя зафиксировання ошибка
		/// </summary>
		public Exception Exception
		{
			get;
			protected set;
		}
		
		/// <summary>
		/// Кольцевой буффер хранения данных
		/// </summary>
		public TcpStream TcpStream
		{
			get;
			protected set;
		}
		
		/// <summary>
		/// Состояние обработки текущего протоколв
		/// </summary>
		public TaskResult TaskResult
		{
			get;
			protected set;
		}
		
		/// <summary>
		/// 
		/// </summary>
		public Queue<IContext> AllContext;
		

		/// <summary>
		/// 
		/// </summary>
		volatile 
		protected int state = 0;
		/// <summary>
		/// 
		/// </summary>
		protected bool waitclose = false;

        /// <summary>
        /// Создает экземпляр класса Base
        /// </summary>
        void Base()
        {
            TimeOpen = new TimeSpan(DateTime.Now.Ticks);
        }

		/// <summary>
		/// Закрывает HTTP соединение, если оно еще не закрыто
		/// </summary>
		/// <param name="wait">false если закрыть сразу</param>
		public void Close(bool wait = false)
		{
			lock (__ObSync)
			{
				if (state < 5)
					state = 5;
			}
			if (wait)
				waitclose = wait;

			TimeClose = new TimeSpan(DateTime.Now.Ticks + 
									 TimeSpan.TicksPerSecond * TIMEWAIT);
		}
		/// <summary>
		/// Обрабатывает происходящие ошибки и назначает оьраьотчики
		/// </summary>
		/// <param name="error">Ошибка</param>
		public void Error(Exception error)
		{
            
			lock (__ObSync)
			{
                if (state > 4)
                    state = 7;
                else
                {
                    state = 4;
                    Exception = error;
                }
			}
		}
		/// <summary>
		/// Очищает управляемые ресурсы
		/// </summary>
		public virtual void Dispose()
		{
			Dispose(true);
			
			GC.SuppressFinalize(this);
		}
		/// <summary>
		/// Очищает управляемые и неуправляемые ресурсы
		/// </summary>
		/// <param name="disposing">если true очитстить неуправляемые ресурсы</param>
		public virtual void Dispose(bool disposing)
		{
			if (Tcp != null)
				Tcp.Dispose();
			if (TcpStream.Writer != null)
				TcpStream.Writer.Dispose();
			if (TcpStream.Reader != null)
				TcpStream.Reader.Dispose();
		}
		/// <summary>
		/// Не поддерживается текущей реализацией
		/// </summary>
		/// <returns>инфорамация о работе протокола</returns>
		public virtual TaskResult HandlerProtocol()
		{
			throw new NotSupportedException("Не поддерживается");
		}
		/// <summary>
		/// Читает данные из сокета и записывает их в поток чтения
		/// </summary>
		/// <returns>SocketError.Success если выполнено успешно</returns>
		protected SocketError Read()
		{
			SocketError error = SocketError.Success;
				lock (TcpStream.Reader.__Sync)			
				{
					int count = 
					   LENGTHREAD;
					int start =
					   (int)TcpStream.Reader.PointW;
					byte[] buffer =
							TcpStream.Reader.Buffer;
				
					if (TcpStream.Reader.Count - start < count)
						count =
						  (int)(TcpStream.Reader.Count - start);
			
					int length = Tcp.Receive(buffer, start, count, SocketFlags.None, out error);
					if (length > 0)
					{
						Len -= length;
						try
						{
							TcpStream.Reader.SetLength(length);
						}
						catch (IOException)
						{
							error = SocketError.SocketError;
						}
					}
				}
			return error;
		}
		/// <summary>
		/// Читает данные из потока записи и записывает их в сокет
		/// </summary>
		/// <returns>SocketError.Success если выполнено успешно</returns>
		protected SocketError Send()
		{
			SocketError error = SocketError.Success;
			
				lock (TcpStream.Writer.__Sync)
				{
					int start =
						(int)TcpStream.Writer.PointR;
					int write =
						(int)TcpStream.Writer.Length;
					if (write > LENGTHWRITE)
						write = LENGTHWRITE;
					byte[] buffer =
							 TcpStream.Writer.Buffer;

					if (TcpStream.Writer.Count - start < write)
						write =
						  (int)(TcpStream.Writer.Count - start);
					int length = Tcp.Send(buffer, start, write, SocketFlags.None, out error);
					if (length > 0)
					{
						try
						{
						TcpStream.Writer.Position = length;
						}
						catch (IOException)
						{
							error = SocketError.SocketError;
						}
					}
				}
			return error;
		}
		protected SocketError Write(byte[] buffer, int start, int write)
		{
			SocketError error = SocketError.Success;
			
				lock (TcpStream.Writer.__Sync)
				{

					if (TcpStream.Writer.Empty)
						start = Tcp.Send(buffer, start, write, SocketFlags.None, out error);

					int length = write - start;
					if (length > 0)
					{
						try
						{
							TcpStream.Writer.Write(buffer, write - length, length);
						}
						catch (IOException)
						{
							error = SocketError.SocketError;
						}
					}
				}
			return error;
		}
	}
}
