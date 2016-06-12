using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
    abstract class HTTP : BaseProtocol
    {

		public static readonly byte[] ENDCHUNCK = { 0x0D, 0x0A };
		public static readonly byte[] EOFCHUNCK = { 0x30, 0x0D, 0x0A, 0x0D, 0x0A };
		/// <summary>
		/// Объект синхронизации
		/// </summary>
		public object Sync
        {
            get;
            protected set;
        }
        volatile 
		int state;
        override
        public States State
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
        public TaskResult Result
        {
            get;
            protected set;
        }
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
                __handconn = true;
                __EventOnOpen += value;
            }
            remove
            {
                __handconn = false;
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
                __handconn = true;
                __EventConnect += value;
            }
            remove
            {
                __handconn = false;
                __EventConnect -= value;
            }
        }

        private object SyncEvent = new object();
        private  event PHandlerEvent __EventWork;
        private  event PHandlerEvent __EventData;
        private  event PHandlerEvent __EventError;
        private  event PHandlerEvent __EventClose;
        private  event PHandlerEvent __EventChunk;
        private  event PHandlerEvent __EventOnOpen;
        static 
        private  event PHandlerEvent __EventConnect;
        static
        protected bool __handconn = false;
        protected long __twaitconn = DateTime.Now.Ticks;

        public HTTP()
        {
            Sync     = new object();
            State    
                = States.Connection;
            Result   = new TaskResult();
            Request  = new Header();
            Response = new Header();
            
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
        public void Error(string message, string stack, codexxx status)
        {
            if (string.IsNullOrEmpty(stack))
                stack = string.Empty;
            if (string.IsNullOrEmpty(message))
                message = string.Empty;
            Response.StartString = "HTTP/1.1 " + status.value + " " + status.ToString();
            Response.AddHeader("Content-Type", "text/html; charset=utf-8");
            byte[] __body = Encoding.UTF8.GetBytes(
            "<html>" +
            "<head>" +
                "<style>" +
                    ".head {margin: auto; color: red; text-align: center; width: 300px}" +
                    ".body {display: block; color: blue; text-align: center; width: 600px}" +
                "</style>" +
                "</head>" +
                "<body>" +
                    "<div class=\"head\">" +
                        message +
                    "</div>" +
                    "<div class=\"body\">" +
                        "<pre>" +
                            stack +
                        "</pre>" +
                    "</div>" +
                "</body>" +
            "</html>");

            Response.AddHeader("Content-Length", __body.Length.ToString());
            Message( __body );
            Response.SetEnd();
        }
		public bool Message(string message)
        {
            return Message(Encoding.UTF8.GetBytes(message));
        }
		public bool Message(byte[] message)
        {
            return Message(   message, 0, message.Length  );
        }
		public bool MESSAGE(byte[] message, int start, int write)
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
						Response.Close = true;
						exc(new HTTPException("Ошибка записи http данных: " + error.ToString(), HTTPCode._500_));
						return false;
					}
				}
			}
			return true;
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
				if (!Response.IsRes)
				{
					// Заголвоки будут перезаписаны
					Response.AddHeader("Date", DateTimeOffset.Now.ToString());
					Response.AddHeader("Server", "MyWebSocket Server Alpha/1.0");

					byte[] respone = Response.ToByte();
					if (!MESSAGE(respone, 0, respone.Length))
						return false;
					Response.SetRes();
				}
				// данные надо отправлять по частям
				if (Response.TransferEncoding == "chunked")
				{

					// hex длинна
					byte[] respone = Encoding.UTF8.GetBytes(
									message.Length.ToString("X"));
					// длинна данных
					if (!MESSAGE(respone, 0, respone.Length))
						return false;
					// блок данный CRLF
					if (!MESSAGE(ENDCHUNCK, 0, ENDCHUNCK.Length))
						return false;

				}
				// отправка данных
				if (!MESSAGE(message, 0, message.Length))
					return false;
				// данные надо отправлять по частям
				if (Response.TransferEncoding == "chunked")
				{

					// блок данный CRLF
					if (!MESSAGE(ENDCHUNCK, 0, ENDCHUNCK.Length))
						return false;

				}
			}
            return true;
        }
        async
        public void MessageFile(string pathfile, string type, int maxlen = 1000 * 16)
        {
            await Task.Run(() =>
            {
                int i = 0;
                try
                {
                    using (FileStream sr = new FileStream(pathfile, FileMode.Open, FileAccess.Read))
                    {
                        Response.StartString = "HTTP/1.1 200 OK";
                        Response.AddHeader("Content-Type", "text/" + type);
						Response.AddHeader("Transfer-Encoding", "chunked");
						//Response.AddHeader("Content-Length", sr.Length.ToString());

						int _count = (int)(sr.Length / maxlen);
                        int length = (int)(sr.Length - _count * maxlen);
                        while (i++ < _count)
                        {
                            int recive = 0;
                            byte[] buffer = new byte[maxlen];
                            while ((maxlen - recive) > 0)
                            {
                                recive = sr.Read(buffer, recive, maxlen - recive);
                            }
                            if (!Message(buffer, 0, maxlen))
                                return;
                            Thread.Sleep(10);
                        }
                        if (length > 0)
                        {
                            int recive = 0;
                            byte[] buffer = new byte[length];
                            while ((length - recive) > 0)
                            {
                                recive = sr.Read(buffer, recive, length - recive);
                            }
                            if (!Message(buffer, 0, length))
                                return;
                            Thread.Sleep(10);
                        }
                    }
                }
                catch (Exception err)
                {
                        exc(new HTTPException( "Ошибка при четнии файла. " + err.Message, HTTPCode._503_, err ));
                }
                finally
                {
                    Response.SetEnd();
                }
            });
        }
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
                if (state ==-1)
                {
                    Work();
                /*============================================================
                    Происходит отправка данных из буффера и проверка 
                    окончания отправки всех данных, если отправка данных 
                    была закончена и все данные были отправлены проверяем 
                    необходимость закрытия текушего соединения если это так,
                    закрываем соединения, если нет обновляем заголвоки и 
                    продолжаем обрабатывать входящие запросы.						   
                ==============================================================*/
                    if (Interlocked.CompareExchange(ref state, 2,-1) !=-1)
                        return Result;
                    if (!Response.IsEnd || !Writer.Empty)
                        write();
                    else
                    {
						if (Response.TransferEncoding == "chunked")
						{
							if (!MESSAGE(EOFCHUNCK, 0, EOFCHUNCK.Length))
								;
						}
						if (Response.Close)
                            close();
                        else
                        {
                            Request = new Header();
                            Response = new Header();
                            Interlocked.CompareExchange (ref state, 0, 2);
                            
                        }
                    }
                /*============================================================
                    Если во время отправки соединение не было закрыто и не 
                    произошло никаких ошибок возвращаемся к предыдущему
                    обработчику.						   
                ==============================================================*/
                    if (Interlocked.CompareExchange(ref state,-1, 2) == 2)
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
                    Пока данные не были получены и не произошло никаких
                    продолжаем читать данные и обрабатывать их. Когда все
                    данные будут получены переходим к следующему 
                    обработчику, обработчику отправки данных.					   
                ==============================================================*/
                    if (Interlocked.CompareExchange(ref state, 1, 0) != 0)
                        return Result;
                        if (!Request.IsEnd)
                        {
                            read();
                            Data();
                        }
                        else
                            Interlocked.CompareExchange (ref state,-1, 1);
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
                            Error(Exception);
							close();
                            /*if (Response.IsRes
								  || Exception.Status == HTTPCode._400_)
                                close();
							else if (Exception.Status == HTTPCode._500_)
						        state = 7;
							else
                            {
                                state =-1;
                                Error(Exception.Message, 
								                Exception.StackTrace, 
														Exception.Status);
							}*/
                            
						}
                /*============================================================
                                        Закрываем соединеие						   
                ==============================================================*/
                                    if (state == 5)
                                    {
										Tcp.Close();
                                        Close();
                                        state = 7;
                                    }
                                    if (state == 7)
                                    {
                                        Dispose();
                                        Result.Option = TaskOption.Delete;
                                    }
            }
            catch (HTTPException err)
            {
                exc(err);
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
                if (state < 4)
                    state = 4;
                Exception = err;
            }
        }
        /// <summary>
        /// получает данные
        /// </summary>
        private void read()
        {
            SocketError error;
            if ((error = Read()) != SocketError.Success)
            {
                if (error != SocketError.WouldBlock
                    && error != SocketError.NoBufferSpaceAvailable)
                {
                    Response.Close = true;
                    exc( new HTTPException("Ошибка чтения http данных: " + error.ToString(), HTTPCode._500_));
                    
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
            if ((error = Send()) != SocketError.Success)
            {
                if (error != SocketError.WouldBlock
                    && error != SocketError.NoBufferSpaceAvailable)
                {
                    Response.Close = true;
                    exc( new HTTPException("Ошибка записи http данных: " + error.ToString(), HTTPCode._500_));
                        
                }
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
            string m = "Получен фрейм с данными";

            PHandlerEvent e;
            lock (SyncEvent)
                e = __EventData;
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
