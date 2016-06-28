using System;
using System.IO;
using System.Net.Sockets;
		using System.Text;
		using System.Threading;
		using System.Threading.Tasks;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
    public abstract class HTTP : BaseProtocol
    {
		/// <summary>
		/// Объект синхронизации данных
		/// </summary>
		public object ObSync
        {
            get;
            protected set;
        }
		/// <summary>
		/// Слстояние обработки потоком
		/// </summary>
        public TaskResult Result
        {
            get;
            protected set;
        }
		/// <summary>
		/// 
		/// </summary>
		public HTTPContext Context;
		/// <summary>
		/// Последняя зафиксировання ошибка
		/// </summary>
		public HTTPException Exception
        {
            get;
            protected set;
        }

		volatile int state;
		/// <summary>
		/// Информация о текщем сотстоянии объекта
		/// </summary>
		override public States State
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
        public event PHandlerEvent EventOnOpen
        {
            add
            {
                __EventOnOpen += value;
            }
            remove
            {
                __EventOnOpen -= value;
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
        private event PHandlerEvent __EventWork;
        private event PHandlerEvent __EventData;
        private event PHandlerEvent __EventError;
        private event PHandlerEvent __EventClose;
        private event PHandlerEvent __EventChunk;
        private event PHandlerEvent __EventOnOpen;
        static
        private event PHandlerEvent __EventConnect;

        public HTTP()
        {
            ObSync = new object();
            State
                = States.Connection;
            Result = new TaskResult();
                Request = new Header();
                Response = new Header();

        }

        public bool close()
        {
            lock (ObSync)
            {
                if (state > 4)
                    return true;
                state = 5;
                return false;
            }
        }
        public override TaskResult TaskLoopHandlerProtocol()
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
						Проверяет сокет были получены данные или нет. Читаем данные 
						из сокета, если есть данные обрабатываем  их. Когда данные
						будут получены и обработаны переходим к следующему обработчику,
						обработчику отправки данных.
					==================================================================*/
					if (Interlocked.CompareExchange(ref state, 1, 0) != 0)
						return Result;
						read();
					if (!Reader.Empty)
						Data();
					/*==================================================================
						Проверяет возможность отправки данных. Если данные можно 
						отправить запускает функцию для отправки данных, в случае 
						если статус не был изменен выполняет переход к следующему 
						обраотчику.
					==================================================================*/
					if (Interlocked.CompareExchange(ref state, 2, 1) != 1)
						return Result;

					if (Response.Close)
						close();
					else if (Response.IsEnd && Writer.Empty)
						End();
					else
						write();
					if (Interlocked.CompareExchange(ref state, 0, 2) == 2)
						return Result;
				}
					if (state == 3)
					{
						Connection();

						if (Interlocked.CompareExchange(ref state, 0, 3) == 5)
							return Result;

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
									Error(Exception);
										Interlocked.Exchange(ref state, -1);
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
									Close();
									if (Tcp.Connected)
										Tcp.Close( 0 );
										Result.Option  =  TaskOption.Delete;
								}
            }
            catch (HTTPException err)
            {
                exc(err);
            }
            catch (Exception err)
            {
                exc(new HTTPException("Критическая ошибка. " + err.Message, HTTPCode._500_, err));
                Log.Loging.AddMessage(err.Message + Log.Loging.NewLine + err.StackTrace, "log.log", Log.Log.Debug);
            }
            return Result;
        }
        public override string ToString()
        {
            return "HTTP";
        }
        /// <summary>
        /// Обрабатывает происходящие ошибки и назначает оьраьотчики
        /// </summary>
        /// <param name="err">Ошибка</param>
        private void exc(HTTPException err)
        {
            lock (ObSync)
            {
				if (state > 3 
				    || Exception != null)
					state = 7;
				else
				{
					state = 4;
				}
				       Response.SetEnd();
				       Exception  =  err;
            }
        }
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
					close();
                }
                else
                {
                    SocketError error;
                    if ((error = Read()) != SocketError.Success)
                    {
                        // проверка является данная ошибка критической
                        if (error != SocketError.WouldBlock
                         && error != SocketError.NoBufferSpaceAvailable)
                        {
							/*         Текущее подключение было закрыто сброшено или разорвано          */
							if (error == SocketError.Disconnecting || error == SocketError.ConnectionReset
																   || error == SocketError.ConnectionAborted)
								close();
							else
							{
								exc(new HTTPException("Ошибка чтения http данных: " + error.ToString(), HTTPCode._500_));
							}
                        }
                    }
                }
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
            if (!Tcp.Poll(0, SelectMode.SelectRead) || Tcp.Available > 0)
            {
                SocketError error;
                if (!Writer.Empty)
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
								close();
							else
							{
								exc(new HTTPException("Ошибка чтения http данных: " + error.ToString(), HTTPCode._500_));
							}

						}
                    }
                }
            }
							else
								close();
        }
        /// <summary>
        /// Потокобезопасный запуск события Work
        /// желательно запускать в обработчике Work
        /// </summary>
        protected void OnEventWork()
        {
            string s = "work";
            string m = "Цикл обработки";
            PHandlerEvent e;
            lock (SyncEvent)
                e = __EventWork;
            if (e != null)
                e(this, new PEventArgs(m, s));
        }
        /// <summary>
        /// Потокобезопасный запуск события Data
        /// желательно запускать в обработчике Data
        /// </summary>
        protected void OnEventData(HTTPContext cntx)
        {
            string s = "data";
            string m = "Получены все данные";

            PHandlerEvent e;
            lock (SyncEvent)
                e = __EventData;
            if (e != null)
                e(this, new PEventArgs(s, m, cntx));
        }
        /// <summary>
        /// Потокобезопасный запуск события Chunk
        /// желательно запускать в обработчике Chunk
        /// </summary>
        protected void OnEventChunk(HTTPContext cntx)
        {
            string s = "сhunk";
            string m = "Получена часть данных";

            PHandlerEvent e;
            lock (SyncEvent)
                e = __EventChunk;
            if (e != null)
                e(this, new PEventArgs(s, m, cntx));
        }
        /// <summary>
        /// Потокобезопасный запуск события Close
        /// желательно запускать в обработчике Close
        /// </summary>
        protected void OnEventClose()
        {
            string s = "close";
            string m = "Соединение было закрыто";

            PHandlerEvent e;
            lock (SyncEvent)
                e = __EventClose;
            if (e != null)
                e(this, new PEventArgs(s, m, null));
        }
        /// <summary>
        /// Потокобезопасный запуск события Error
        /// желательно запускать в обработчике Error
        /// </summary>
        protected void OnEventError(HTTPException error)
        {
            string s = "error";
            string m = "Произошла ошибка во время исполнения";

            PHandlerEvent e;
            lock (SyncEvent)
                e = __EventError;
            if (e != null)
                e(this, new PEventArgs(s, m, error));
        }
        /// <summary>
        /// Потокобезопасный запуск события OnOpen 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        protected void OnEventOpen(IHeader request, IHeader response)
        {
            string s = "connect";
            string m = "Соединение было установлено, протокол ws";
            PHandlerEvent e;
            lock (SyncEvent)
                e = __EventOnOpen;
            if (e != null)
                e(this, new PEventArgs(s, m, null));
        }
        /// <summary>
        /// Потокобезопасный запуск события Connection
        /// желательно запускать в обработчике Connection
        /// </summary>
        protected void OnEventConnect()
        {
            string s = "connect";
            string m = "Соединение было установлено, протокол ws";

            PHandlerEvent e;
            lock (SyncEvent)
                e = __EventConnect;
            if (e != null)
                e(this, new PEventArgs(s, m, null));

        }
        /// <summary>
        /// закончена передача данных чтобы закрыть соединеие не обходимо установить
        /// значение response.Close = true;
        /// в случае ошибок необходимо бросать HTTPException с указанным статусом http
        /// </summary>
        protected abstract void End();
        /// <summary>
        /// 
        /// </summary>
        protected abstract void Work();
        /// <summary>
        /// работаем
        /// в случае ошибок необходимо бросать HTTPException с указанным статусом http
        /// </summary>
        protected abstract void Data();
        /// <summary>
        /// получить заголовки
        /// в случае ошибок необходимо бросать HTTPException с указанным статусом http
        /// </summary>
        protected abstract void Close();
        /// <summary>
        /// произошло закрытие
        /// в случае ошибок необходимо бросать HTTPException с указанным статусом http 
        /// </summary>
        protected abstract void Error(HTTPException error);
        /// <summary>
        /// обработать ошибку
        /// в случае ошибок необходимо бросать HTTPException с указанным статусом http
        /// </summary>
        protected abstract void Connection();
    }
}
