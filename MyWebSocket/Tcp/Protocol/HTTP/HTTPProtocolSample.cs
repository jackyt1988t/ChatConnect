using System;

using System.IO;
using System.IO.Compression;
        using System.Net.Sockets;
            using System.Threading;
            using System.Collections.Generic;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
    
    class HTTPProtocol : HTTP
    {
        public static readonly long ALIVE;
        
        public Stream __Arhiv
        {
            get;
            set;
        }
        /// <summary>
        /// Время ожидания запросов
        /// </summary>
        public TimeSpan Alive
        {
            get;
        }
        
        HTTPReader __Reader;
        public override MyStream Reader
        {
            get
            {
                return __Reader;
            }
        }
        HTTPWriter __Writer;
        public override MyStream Writer
        {
            get
            {
                return __Writer;
            }
        }
        
        static HTTPProtocol()
        {
            ALIVE = 25;
            HTTPWriter.MINRESIZE = MINLENGTHBUFFER;
            HTTPWriter.MAXRESIZE = MAXLENGTHBUFFER * 64;
        }
        public HTTPProtocol(Socket tcp) :
            base()
        {
            Tcp = tcp;
            Alive = new TimeSpan(DateTime.Now.Ticks + 
                        TimeSpan.TicksPerSecond * ALIVE);
            
                Result.Protocol    =    TaskProtocol.HTTP;
                __Reader = new HTTPReader(MINLENGTHBUFFER)
                {
                    header = Request
                };
                __Writer = new HTTPWriter(MINLENGTHBUFFER)
                {
                    header = Response
                };
        }
        public override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (__Arhiv != null)
                    __Arhiv.Dispose();
            }
                base.Dispose(disposing);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="_chunk"></param>
        protected override void file( string path, int _chunk )
        {
			Header response = null;
			lock (ObSync)
				response = Response;
			
			FileInfo fileinfo = new FileInfo(path);
            
            if (!fileinfo.Exists)
            {
                throw new HTTPException("Файл не найден " + path, HTTPCode._404_);
            }
            else
            {
                if (response.ContentType == null
                    || response.ContentType.Count == 0)
                {
                    string extension = 
                            fileinfo.Extension.Substring(1);
                    response.ContentType = new List<string>()
                                           {
                                               "text/" + extension,
                                               "charset=utf-8"
                                           };
                }
                using (FileStream stream = fileinfo.OpenRead())
                {
                    int i = 0;
                    int _count = (int)(stream.Length / _chunk);
                    int length = (int)(stream.Length - _count * _chunk);

                    if (string.IsNullOrEmpty(response.TransferEncoding))
                        response.ContentLength  =  ( int )stream.Length;
                    
                    while (i++ < _count)
                    {
                        int recive = 0;
                        byte[] buffer = new byte[_chunk];
                        while ((_chunk - recive) > 0)
                        {
                            recive = stream.Read(buffer, recive, _chunk - recive);
                        }
                        if (response.IsEnd    ||    !Message(  buffer, 0, _chunk  ))
                            return;
                    }
                    if (length > 0)
                    {
                        int recive = 0;
                        byte[] buffer = new byte[length];
                        while ((length - recive) > 0)
                        {
                            recive = stream.Read(buffer, recive, length - recive);
                        }
                        if (response.IsEnd    ||    !Message(  buffer, 0, length  ))
                            return;
                    }
                }
            }
        }
        /// <summary>
        /// Записываем данные в стандартный поток, если заголвок Content-Encoding
        /// установлен в gzip декодируем данные в формате gzip(быстрое сжатие)
        /// </summary>
        /// <param name="buffer">массив данных</param>
        /// <param name="start">стартовая позиция</param>
        /// <param name="write">количество которое необходимо записать</param>
        /// <returns></returns>
        public override bool Message(byte[] buffer, int start, int write)
        {
            bool result = true;
			Header response = null;

			lock (ObSync)
				response = Response;

			if (!response.IsRes)
            {
                if (string.IsNullOrEmpty(response.StrStr))
                    response.StrStr  =  "HTTP/1.1 200 OK";
                
                if (response.CashControl == null
                     || response.CashControl.Count == 0)
                    response.CashControl = new List<string>
                                               {
                                                   "no-store",
                                                   "no-cache",


                                               };
                if (response.ContentType == null
                     || response.ContentType.Count == 0)
                    response.ContentType = new List<string>
                                               {
                                                   "text/plain",
                                                   "charset=utf-8"
                                               };
            }
            lock (ObSync)
            {
                if (response.IsEnd 
                     || (State == States.Close 
                          || State == States.Disconnect))
                    result = false;
                else
                {
                    try
                    {
                            switch (response.ContentEncoding )
                            {
                                case "gzip":
                                    __Arhiv.Write(buffer, start, write);
                                    break;
                                case "deflate":
                                    __Arhiv.Write(buffer, start, write);
                                    break;
                                default:
                                    __Writer.Write(buffer, start, write);
                                    break;
                            }
                    }
                    catch (IOException exc)
                    {
                        close();
                        result = false;
                        Log.Loging.AddMessage(exc.Message + "/r/n" + exc.StackTrace, "log.log", Log.Log.Debug);
                    }
                }
            }
            return result;
        }

        protected override void End()
        {
            lock (ObSync)
            {
				Header response = null;
				lock (ObSync)
					response = Response;

                if (__Arhiv != null)
                    __Arhiv.Dispose();
                // Отправить блок данных chunked 0CRLFCRLF
                if (response.TransferEncoding == "chunked")
                {
                    try
                    {
                        if (State != States.Close
                                && State != States.Disconnect)
                            __Writer.Eof();
                    }
                    catch (IOException exc)
                    {
                        close();
                        Log.Loging.AddMessage(exc.Message + "/r/n" + exc.StackTrace, "log.log", Log.Log.Debug);
                    }
                }
            }
        }
        protected override void Work()
        {
            OnEventWork();
            // вермя до закрытия(  keep-alive  )
            if (Alive.Ticks < DateTime.Now.Ticks && Writer.Empty)
            {
                close();
            }
        }
        protected override void Data()
        {
            if (__Reader._Frame.GetHead && __Reader._Frame.GetBody)
            {
                __Reader._Frame.Clear();
                __Writer._Frame.Clear();
				lock (ObSync)
				{
					__Reader.header = Request;
					__Writer.header = Response;
				}
			}

            /*
                ----------------------------------------------------------------------------------
                Обрабатываем заголвоки запроса. Если заголвоки не были получены, читаем данные из
                кольцевого потока данных. Проверяем доступные методы обработки. При переходе на
                Websocket меняем протокол. 
                Устанавливаем заголвоки:
                Date
                Server
                Connection(если необходимо)
                Cintent-Encoding(если в заголвоке Accept-Encoding указаны gzip или deflate)
                Запускаем событие EventOpen
                ----------------------------------------------------------------------------------  
            */
            if (!__Reader._Frame.GetHead)
            {
                if (__Reader.ReadHead() == -1)
                    return;

                switch (Request.Method)
                {
                    case "GET":
                        if (__Reader._Frame.bleng > 0)
                            throw new HTTPException("Неверная длина запроса GET", HTTPCode._400_);
                        break;
                    case "POST":
                        if (__Reader._Frame.Handl == 0)
                            throw new HTTPException("Неверная длина запроса POST", HTTPCode._400_);
                        break;
                    default:
                            throw new HTTPException("Не поддерживается текущей реализацией", HTTPCode._501_);
                }
                if (!string.IsNullOrEmpty(Request.Upgrade))
                {
                    Result.Jump = true;
                    if (Request.Upgrade.ToLower() == "websocket")
                    {
                        string version;
                        string protocol = string.Empty;
                        if (Request.ContainsKeys("websocket-protocol", out version, true))
                            protocol = "websocket-protocol";
                        else if (Request.ContainsKeys("sec-websocket-version", out version, true))
                            protocol = "sec-websocket-version";
                        else if (Request.ContainsKeys("sec-websocket-protocol", out version, true))
                            protocol = "sec-websocket-protocol"; 
                        switch (version.ToLower())
                        {
                            case "7":
                                Result.Jump = true;
                                Result.Protocol = TaskProtocol.WSN13;
                                break;
                            case "8":
                                Result.Jump = true;
                                Result.Protocol = TaskProtocol.WSN13;
                                break;
                            case "13":
                                Result.Jump = true;
                                Result.Protocol = TaskProtocol.WSN13;
                                break;
                            case "sample":
                                Result.Jump = true;
                                __Reader._Frame.Handl = 1;
                                __Reader._Frame.bleng = 8;
                                Result.Protocol = TaskProtocol.WSAMPLE;
                                break;
                        }
                    }
                }

                if (!Result.Jump)
                {
                    if (Request.Connection == "close")
                    {
                        Response.SetClose();
                        Response.Connection = "close";
                    }
                    else
                        Response.Connection = "keep-alive";

                    if (Request.AcceptEncoding != null)
                    {
                        if (Request.AcceptEncoding.Contains("gzip"))
                            Response.ContentEncoding = "gzip";
                        else if (Request.AcceptEncoding.Contains("deflate"))
                            Response.ContentEncoding = "deflate";
                    }
                    Response.TransferEncoding = "chunked";

                    Response.AddHeader("Date", 
                                        DateTimeOffset.Now.ToString() + " UTC");
                    Response.AddHeader("Server", "MyWebSocket Vers. Alpha 1.0");

                    OnEventOpen(Request, Response);

                    if (Response.ContentEncoding == "gzip")
                        __Arhiv = new GZipStream(__Writer, CompressionLevel.Fastest, true);
                    else if (Response.ContentEncoding == "deflate")
                        __Arhiv = new DeflateStream(__Writer, CompressionLevel.Fastest, true);
                    Log.Loging.AddMessage("Http заголовки успешно обработаны", "log.log", Log.Log.Info);
                }
            }
            /*
                ----------------------------------------------------------------------------------
                Обрабатываем тело запроса. Если тело не было получено, читаем данные из кольцевого 
                потока данных. При заголвоке Transfer-Encoding тело будет приходяить по частям и
                будет полность получено после 0 данных и насупить событие EventData, до этого 
                момента будет насупать событие EventChunk, ечли был указан заголвок Content-Length
                событие EventChunk происходить не будет, а событие EventData произойдет только 
                тогда когда все данные будут получены. Максимальный размер блока данных chuncked
                можно указать задав значение HTTPReader.LENCHUNK. В случае с Content-Length можно
                обработать заголвок в соыбтие EventOpen и при привышении допустимого значения
                закрыть соединение.
                ----------------------------------------------------------------------------------  
            */
            if (!__Reader._Frame.GetBody)
            {
                if (__Reader.ReadBody() == -1)
                    return;

                switch (__Reader._Frame.Pcod)
                {
                    case HTTPFrame.DATA:
                        if (!Result.Jump)
                        {
                            OnEventData();
                            Log.Loging.AddMessage("Все данные Http запроса получены", "log.log", Log.Log.Info);
                        }
                        else
                            Result.Option = TaskOption.Protocol;
                        break;
                    case HTTPFrame.CHUNK:
                            OnEventChunk();
                            Log.Loging.AddMessage("Часть данных Http запроса получена", "log.log", Log.Log.Info);
                        break;
                }
            }
        }
        protected override void Close()
        {
            OnEventClose();
            
        }
        protected override void Error(HTTPException error)
        {
            OnEventError(error);

			Header response = null;
			lock (ObSync)
				response = Response;
			
			if (response.IsRes || response.TransferEncoding != "chunked")
				close();
			else
			{
				__Reader._Frame.Clear();
				__Writer._Frame.Clear();
				lock (ObSync)
				{
					__Reader.header = Request = new Header();
					__Writer.header = Response = new Header();
				}
				if (response.IsRes && response.TransferEncoding == "chunked")
				{
					try
					{
						if (State != States.Close
							 && State != States.Disconnect)
							__Writer.Eof();
					}
					catch (IOException exc)
					{
						Log.Loging.AddMessage(exc.Message + "/r/n" + exc.StackTrace, "log.log", Log.Log.Info);
					}
				}
						Response.StrStr = "HTTP/1.1 " + error.Status.value
																	.ToString()
												+ " " + error.Status.ToString();
				
						File("Html/" + error.Status.value.ToString() + ".html");
						Log.Loging.AddMessage("Информация об ошибке готова к отправке", "log.log", Log.Log.Info);
			}
		}
        protected override void Connection()
        {
            OnEventConnect();
        }
    }
}
