using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenSSL.SSL;
using OpenSSL.X509;
using OpenSSL.Core;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
    public class HTTProtocol : BaseProtocol
    {
		
		/// <summary>
		/// шифровать данные
		/// </summary>
		public static bool Security;
		/// <summary>
		/// Путь к сертификату
		/// </summary>
		public static string Path_Pem;
		/// <summary>
		/// Возвращает поток данных
		/// </summary>
		public Stream GetStream
		{
			
			get
			{
				if (Security)
					return SslStream;
				else
					return TcpStream;
			}
		}
		/// <summary>
		/// Поток шифрования данных
		/// </summary>
		public SslStreamServer SslStream;
		

		event PHandlerEvent eventWork;
		/// <summary>
		/// Событие которое наступает при проходе по циклу
		/// </summary>
		public event PHandlerEvent EventWork
		{
			add
			{
				lock (SyncEvent)
					eventWork += value;

			}
			remove
			{
				lock (SyncEvent)
					eventWork -= value;
			}
		}
		event PHandlerEvent eventData;
		/// <summary>
		/// Событие которое наступает когда приходит фрейм с данными
		/// </summary>
		public event PHandlerEvent EventData
		{
			add
			{
				lock (SyncEvent)
					eventData += value;

			}
			remove
			{
				lock (SyncEvent)
					eventData -= value;
			}
		}
		event PHandlerEvent eventclose;
		/// <summary>
		/// Событие которое наступает когда приходит заврешающий фрейм
		/// </summary>
		public event PHandlerEvent EventClose
		{
			add
			{
				lock (SyncEvent)
					eventclose += value;

			}
			remove
			{
				lock (SyncEvent)
					eventclose -= value;
			}
		}
		event PHandlerEvent eventerror;
		/// <summary>
		/// Событие которое наступает когда приходит при ошибке протокола
		/// </summary>
		public event PHandlerEvent EventError
		{
			add
			{
				lock (SyncEvent)
					eventerror += value;

			}
			remove
			{
				lock (SyncEvent)
					eventerror -= value;
			}
		}
		event PHandlerEvent eventchunk;
		/// <summary>
		/// Событие которое наступает когда приходит кусок отправленных данных
		/// </summary>
		public event PHandlerEvent EventChunk
		{
			add
			{
				lock (SyncEvent)
					eventchunk += value;

			}
			remove
			{
				lock (SyncEvent)
					eventchunk -= value;
			}
		}
		event PHandlerEvent eventOnopen;
		/// <summary>
		/// Событие которое наступает при открвтии соединения когда получены заголвоки
		/// </summary>
		public event PHandlerEvent EventOnOpen
		{
			add
			{
				eventOnopen += value;
			}
			remove
			{
				eventOnopen -= value;
			}
		}
		static event PHandlerEvent eventconnect;
		/// <summary>
		/// Событие которое наступает при открвтии соединения когда получены заголвоки
		/// </summary>
		static
		public event PHandlerEvent EventConnect
		{
			add
			{
				eventconnect += value;
			}
			remove
			{
				eventconnect -= value;
			}
		}
		protected object SyncEvent = new object();
		protected static X509Certificate sertificate;
		static HTTProtocol()
		{
			try
			{
				sertificate = X509Certificate.FromPKCS12(BIO.File("server.pfx", "r"), "Tv7bU9m");
			}
			catch (Exception error)
			{
				;
			}
		}
		/// <summary>
		/// Создает объект обработки данных
		/// </summary>
		/// <param name="tcp">tcp/ip соединеине</param>
		public HTTProtocol(Socket tcp)
        {
			
			Tcp = tcp;
            State = 
				States.Connection;
					
			ObSync = new object();
            TaskResult   =
					new TaskResult();
				Request  = new Header();
                Response = new Header();
			
			TcpStream = new TcpStream();
			Security = true;
			if (Security)
				SslStream = new SslStreamServer(TcpStream, 
					sertificate, 
							false, 
								null, 
									SslProtocols.Tls, 
										SslStrength.All, 
													true, 
													(sender, cert, chain, depth, result) =>
													{
														return true;
													});			
			ContextRq = ContextRs = new HTTPContext( this );

			AllContext = new Queue<IContext>();

			OnEventConnect();
			
			Interlocked.CompareExchange(  ref state, 0, 3  );

			
		}
		/// <summary>
		/// Освобождаем ресурсы перед очисткой
		/// </summary>
		~HTTProtocol()
		{
			Dispose();
		}
		/// <summary>
		/// Создает новый контекст для обработки
		/// </summary>
		internal void NewContext(IContext cntx)
		{
			AllContext.Enqueue(
				(ContextRq = ContextRq.Context()));
		}
		/// <summary>
		/// Потокобезопасный запуск события Work
		/// желательно запускать в обработчике Work
		/// </summary>
		internal void OnEventWork()
		{
			if (ContextRs.Cancel 
				 && TcpStream.Writer.Empty)
			{
				ContextRs = AllContext.Dequeue();
			}

			string s = "work";
			string m = "Цикл обработки";

			PHandlerEvent e;
			lock (SyncEvent)
				e = eventWork;
			if (e != null)
				e(this, new PEventArgs(m, s));
		}
		/// <summary>
		/// Потокобезопасный запуск события Data
		/// желательно запускать в обработчике Data
		/// </summary>
		internal void OnEventData(IContext cntx)
		{

			string s = "data";
			string m = "Получены все данные";

			PHandlerEvent e;
			lock (SyncEvent)
				e = eventData;
			if (e != null)
				e(this, new PEventArgs(s, m, cntx));
		}
		/// <summary>
		/// Потокобезопасный запуск события Chunk
		/// желательно запускать в обработчике Chunk
		/// </summary>
		internal void OnEventChunk(IContext cntx)
		{
			string s = "сhunk";
			string m = "Получена часть данных";

			PHandlerEvent e;
			lock (SyncEvent)
				e = eventchunk;
			if (e != null)
				e(this, new PEventArgs(s, m, cntx));
		}
		/// <summary>
		/// Потокобезопасный запуск события Close
		/// желательно запускать в обработчике Close
		/// </summary>
		internal void OnEventClose()
		{
			string s = "close";
			string m = "Соединение было закрыто";

			IContext[] cntx = AllContext.ToArray();

			PHandlerEvent e;
			lock (SyncEvent)
				e = eventclose;
			if (e != null)
				e(this, new PEventArgs(s, m, cntx));
		}
		/// <summary>
		/// Потокобезопасный запуск события OnOpen 
		/// </summary>
		internal void OnEventOpen(IContext cntx)
		{

			string s = "connect";
			string m = "Соединение было установлено, протокол ws";

			PHandlerEvent e;
			lock (SyncEvent)
				e = eventOnopen;
			if (e != null)
				e(this, new PEventArgs(s, m, cntx));
		}
		/// <summary>
		/// Потокобезопасный запуск события Error
		/// желательно запускать в обработчике Error
		/// </summary>
		internal void OnEventError(Exception error)
		{

			string s = "error";
			string m = "Произошла ошибка во время исполнения";

			PHandlerEvent e;
			lock (SyncEvent)
				e = eventerror;
			if (e != null)
				e(this, new PEventArgs(s, m, error));
		}
		/// <summary>
		/// Потокобезопасный запуск события Connection
		/// желательно запускать в обработчике Connection
		/// </summary>
		internal void OnEventConnect()
		{

			string s = "connect";
			string m = "Соединение было установлено, протокол ws";

			PHandlerEvent e;
			lock (SyncEvent)
				e = eventconnect;
			if (e != null)
				e(this, new PEventArgs(s, m, null));

		}

		public override TaskResult HandlerProtocol()
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
					OnEventWork();
					/*==================================================================
						Проверяет сокет были получены данные или нет. Читаем данные 
						из сокета, если есть данные обрабатываем  их. Когда данные
						будут получены и обработаны переходим к следующему обработчику,
						обработчику отправки данных.
					==================================================================*/
					if (Interlocked.CompareExchange(ref state, 1, 0) != 0)
						return TaskResult;
						read();
					/*==================================================================
						Проверяет возможность отправки данных. Если данные можно 
						отправить запускает функцию для отправки данных, в случае 
						если статус не был изменен выполняет переход к следующему 
						обраотчику.
					==================================================================*/
					if (Interlocked.CompareExchange(ref state, 2, 1) != 1)
						return TaskResult;
						write();
					if (Interlocked.CompareExchange(ref state, 0, 2) == 2)
						return TaskResult;
				}
				/*============================================================
                                        Обработчик ошибок
                    Запускам функцию обработки ошибок. Если заголвоки были
                    отправлены закрываем соединение, если нет отправляем 
                    информацию о произошедшей ошибки. При ошибке клиента 
                    400 или ошибке сервера 500 указываем серверу после 
                    отправки данных закрыть моединение. 					   
                ==============================================================*/
								if (state == 4)
								{
									/////Ошибка/////
									OnEventError(Exception);
									Interlocked.Exchange(ref state, 0);
								}
                /*============================================================
                                        Закрываем соединеие						   
                ==============================================================*/
								if (state == 5)
								{
									state = 7;
								}
								if (state == 7)
								{
									OnEventClose();
									if (Tcp.Connected)
										Tcp.Close( 0 );
										TaskResult.Option  =  TaskOption.Delete;
								}
            }
            catch (HTTPException err)
            {
                Error(err);
            }
            catch (Exception err)
            {
                Error(new HTTPException("Критическая ошибка. " + err.Message, HTTPCode._500_, err));
                Log.Loging.AddMessage(err.Message + Log.Loging.NewLine + err.StackTrace, "log.log", Log.Log.Debug);
            }
            return TaskResult;
        }

		#region ovverride
		/// <summary>
		/// Очищает данные связанные с объектом
		/// </summary>
		/// <param name="disposing">true чтобы очистить неуправляеме объекты</param>
		public override void Dispose(bool disposing)
		{			
			if (disposing)
			{
				
			}
			if (SslStream != null)
				SslStream.Dispose();
			base.Dispose(disposing);
		}
		/// <summary>
		/// Возврощает информацию о текущем объекте
		/// </summary>
		/// <returns>информацию о текущем объекте</returns>
		public override string ToString()
        {
            return "HTTP";
        }
		
		#endregion

		/// <summary>
		/// получает данные
		/// </summary>
		private void read()
        {
			/*
                Если функция Poll Вернет true проверяем наличие данных, если данных нет значит соединение
                было закрыто. Если есть данные читаем данные из сокета проверяем на наличие ошибок, если
                выполнение произошло с ошибкой, обрабатываем.
            */
			if (Tcp.Poll(0, SelectMode.SelectRead))
			{
				if (Tcp.Available == 0)
				{
					Close();
				}
				else
				{
					SocketError error;
					if ((error = Read()) != SocketError.Success)
					{
						// проверка является данная ошибка критической
						if (error == SocketError.WouldBlock
						 || error == SocketError.NoBufferSpaceAvailable)
						{
							Handler();
						}
						else
						{
							/*         Текущее подключение было закрыто сброшено или разорвано          */
							if (error == SocketError.Disconnecting || error == SocketError.ConnectionReset
																   || error == SocketError.ConnectionAborted)
								Close();
							else
							{
								Error(new HTTPException("Ошибка чтения http данных: " + error.ToString(), HTTPCode._500_));
							}
						}
					}
				}
			}
						else
						{
							Handler();
						}
        }
        /// <summary>
        /// Отправляет сообщение
        /// </summary>
        private void write()
        {
            /*
                Если функция Poll Вернет false или есть наличие данные, считываем данные из сокета, иначе закрываем
                соединение. Если проверка прошла успешно читаем данные из сокета
            */
            if (Tcp.Poll(0, SelectMode.SelectWrite))
            {
                SocketError error;
                if (!TcpStream.Writer.Empty)
                {
                    if ((error = Send()) != SocketError.Success)
                    {
                        // проверка является данная ошибка критической
                        if (error != SocketError.WouldBlock
							  && error != SocketError.NoBufferSpaceAvailable)
                        {
							/*         Текущее подключение было закрыто сброшено или разорвано          */
							if (error == SocketError.Disconnecting || error == SocketError.ConnectionReset
																   || error == SocketError.ConnectionAborted)
								Close();
							else
							{
								Error(new HTTPException("Ошибка чтения http данных: " + error.ToString(),HTTPCode._500_));
							}

						}
                    }
                }
            }
        }
		private void Handler()
		{
			if (!TcpStream.Reader.Empty)
			{
				if (!Security || SslStream.HandshakeComplete)
				{
					ContextRq.Handler();
				}
				else
				{
							try
							{
								SslStream.AuthenticateServer();
							}
							catch (Exception error)
							{
								Error(new HTTPException("Ошибка авторизации https соединения: " + error.ToString(), HTTPCode._500_));
							}
				}
			}
		}
    }
}
