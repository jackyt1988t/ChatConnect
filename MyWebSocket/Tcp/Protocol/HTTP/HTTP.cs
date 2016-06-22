using System;
using System.Net.Sockets;
        using System.Text;
        using System.Threading;
        using System.Threading.Tasks;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
    public abstract class HTTP : BaseProtocol
    {
		volatile int state;
		/// <summary>
		/// Информация о текщем сотстоянии
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
		/// Объект синхронизации данных
		/// </summary>
		public object Sync
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
		/// Последняя зафиксировання ошибка
		/// </summary>
        public HTTPException Exception
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

        private bool _filewrite;
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
            Sync = new object();
            State
                = States.Connection;
            Result = new TaskResult();
                Request = new Header();
                Response = new Header();

        }
        async public void File(string path, int chunk = 1000 * 64)
        {
            lock (Sync)
            {
                if (_filewrite)
                    throw new HTTPException("Дождитесь окончания записи файла");
                _filewrite = true;
            }
            await Task.Run(() =>
            {
                try
                {
                    file(path, chunk);
                }
                catch (HTTPException err)
                {
                    exc(err);
                }
                catch (Exception err)
                {
                    exc(new HTTPException("Ошибка при чтении файла " + path, HTTPCode._500_, err));
                }
                finally
                {
                    Flush();
                    lock (Sync)
                        _filewrite = false;
                }
            });
        }

        public bool close()
        {
            lock (Sync)
            {
                if (state > 4)
                    return true;
                state = 5;
                return false;
            }
        }
        /// <summary>
        /// Очищает записывающий буффер данных
        /// </summary>
        public void Flush()
        {
            Response.SetEnd();
        }
        /// <summary>
        /// Очищает записывающий буффер данных
        /// Отправляет указанную строку уд. стороне
        /// </summary>
        public void Flush(string message)
        {
            lock (Sync)
            {
                Message(message);
                Flush();
            }
        }
        /// <summary>
        /// Очищает записывающий буффер данных
        /// Отправляет указанный массив данных уд. стороне
        /// </summary>
        public void Flush(byte[] message)
        {
            lock (Sync)
            {
                Message(message);
                Flush();
            }
        }
        /// <summary>
        /// Отправляет указанную строку уд. стороне
        /// </summary>
        /// <returns>true в случае успеха</returns>
        public bool Message(string message)
        {
            return Message(Encoding.UTF8.GetBytes(message));
        }
        /// <summary>
        /// Отправляет указанный массив данных уд. стороне
        /// </summary>
        /// <returns>true в случае успеха</returns>
        public bool Message(byte[] message)
        {
            if (message == null)
                message = new byte[0];
            return Message(message, 0, message.Length);
        }
        public abstract bool Message(byte[] message, int start, int write);

        public override TaskResult TaskLoopHandlerProtocol()
        {
            try
            {
                /*============================================================
                                    Обработчик отправки данных
                    Запускает функцию обработки пользоватлеьских данных, в
                    случае если статус не был изменен переходим к следующему
                    обработчику, обработчику отправки пользовательских 
                    данных.						   
                ==============================================================*/
                if (state == -1)
                {
                    Work();
                /*============================================================
                    Пытаемся отправить данные, если отправка данных 
                    была закончена и все данные были отправлены проверяем 
                    необходимость закрытия текушего соединения если это так,
                    закрываем соединения, если нет обновляем заголвоки и 
                    продолжаем обрабатывать входящие запросы.						   
                ==============================================================*/
                    if (Interlocked.CompareExchange(ref state, 2, -1) != -1)
                        return Result;

                    write();
                    if (Response.IsEnd && Writer.Empty)
                    {
                        if (!Response.IsReq)
                        {
                            End();
                            Response.SetReq();
                        }
                        else
                        {
                            if (Response.Close)
                                close();
                            else
                            {
                                Request = new Header();
                                Response = new Header();
                                Interlocked.CompareExchange(ref state, 0, 2);
                            }
                        }
                    }
                /*============================================================
                    Если во время отправки соединение не было закрыто и не 
                    произошло никаких ошибок возвращаемся к предыдущему
                    обработчику.						   
                ==============================================================*/
                    if (Interlocked.CompareExchange(ref state, -1, 2) == 2)
                        return Result;
                }
                /*============================================================
                    Запускает функцию обработки пользоватлеьских данных, в
                    случае если статус не был изменен переходим к 
                    следующему обработчику, обработчику чтения полученных
                    и обраблтки пользовательских данных.						   
                ==============================================================*/
                if (state == 0)
                {
                    Work();
                /*============================================================
                                    Обработчик получения данных
                    Читаем данные из сокета, если есть данные обрабатываем 
                    их. Когда данные будут получены и обработаны переходим
                    к следующему обработчику, обработчику отправки данных.					   
                ==============================================================*/
                    if (Interlocked.CompareExchange(ref state, 1, 0) != 0)
                        return Result;
                    if (!Request.IsEnd)
                    {
                        read();
                        if (!Reader.Empty)
                            Data();
                    }
                    else
                        Interlocked.CompareExchange(ref state,-1, 1);
                    if (Interlocked.CompareExchange(ref state, 0, 1) == 1)
                        return Result;
                }
                /*============================================================
                                        Заголвоки получены
                    Запускаем обработчик полученных заголвоков, если статус
                    не будет изменен в обработчике и не произойдет никаких
                    ошибок, продолжаем получать пользовательские данные.						   
                ==============================================================*/
                if (state == 3)
                {
                    Connection();
                    if (Interlocked.CompareExchange(ref state, 0, 3) == 3)
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
                            if (Response.IsRes 
								  || Exception.Status.value == 500)
                                close();
                            else
                            	Interlocked.Exchange(ref state, -1);
                                Error(Exception);
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
                            Result.Option = TaskOption.Delete;
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
            lock (Sync)
            {
					
				if (Exception != null)
					state = 7;
                else if (state < 4)
                    state = 4;
					Exception = err;
					
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
                    Response.SetClose();
                    exc(new HTTPException("Ошибка чтения http данных. Соединение закрыто.", HTTPCode._500_));
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
                            Response.SetClose();
                            exc(new HTTPException("Ошибка чтения http данных: " + error.ToString(), HTTPCode._500_));

                        }
                    }
                }
            }
        }
        /// <summary>
        /// Отправляет сообщение
        /// </summary>
        /// <param name="data">Данные</param>
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
                            Response.SetClose();
                            exc(new HTTPException("Ошибка записи http данных: " + error.ToString(), HTTPCode._500_));

                        }
                    }
                }
            }
                        else
                        {
                            Response.SetClose();
                            exc(new HTTPException("Ошибка записи http данных. Соединение закрыто.", HTTPCode._500_));
                        }
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
        protected void OnEventData()
        {
            string s = "data";
            string m = "Получены все данные";

            PHandlerEvent e;
            lock (SyncEvent)
                e = __EventData;
            if (e != null)
                e(this, new PEventArgs(s, m, null));
        }
        /// <summary>
        /// Потокобезопасный запуск события Chunk
        /// желательно запускать в обработчике Chunk
        /// </summary>
        protected void OnEventChunk()
        {
            string s = "сhunk";
            string m = "Получена часть данных";

            PHandlerEvent e;
            lock (SyncEvent)
                e = __EventChunk;
            if (e != null)
                e(this, new PEventArgs(s, m, null));
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
        /// отправляет файл пользователю
        /// </summary>
        /// <param name="path"></param>
        /// <param name="chunk"></param>
        protected abstract void file(string path, int chunk);
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
