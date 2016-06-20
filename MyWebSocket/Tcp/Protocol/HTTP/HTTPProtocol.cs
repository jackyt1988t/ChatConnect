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
            ALIVE = 40;
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
        /// <param name="chunk"></param>
        protected override void file(string path, int chunk)
        {
            FileInfo fileinfo = new FileInfo(path);
            
            if (!fileinfo.Exists)
            {
                throw new HTTPException("Указанный файл не найден " + path, HTTPCode._404_);
            }
            else
            {
                if (Response.ContentType == null
                    || Response.ContentType.Count == 0)
                {
                    Response.ContentType = new List<string>()
                                           {
                                               "text/" + fileinfo.Extension.Substring( 01 ),
                                               "charset=utf-8"
                                           };
                }
                using (FileStream sr = fileinfo.OpenRead())
                {
                    int i = 0;
                    int _count = (int)(sr.Length / chunk);
                    int length = (int)(sr.Length - _count * chunk);

                    if (string.IsNullOrEmpty(Response.TransferEncoding))
                        Response.ContentLength  =  (   int   )sr.Length;
                    
                    while (i++ < _count)
                    {
                        int recive = 0;
                        byte[] buffer = new byte[chunk];
                        while ((chunk - recive) > 0)
                        {
                            recive = sr.Read(buffer, recive, chunk - recive);
                        }
                        while ( Writer.Length > 128000 )
                        {
                            if (State == States.work || State == States.Send)
                                Thread.Sleep(20);
                            else
                                return;
                        }
                        if (!Message(buffer, 0, chunk))
                            return;
                    }
                    if (length > 0)
                    {
                        int recive = 0;
                        byte[] buffer = new byte[length];
                        while ((length - recive) > 0)
                        {
                            recive = sr.Read(buffer, recive, length - recive);
                        }
                        while ( Writer.Length > 128000 )		
                        {
                            if (State == States.work || State == States.Send)
                                Thread.Sleep(20);
                            else
                                return;
                        }
                        if (!Message(buffer, 0, length))
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
            if ( buffer == null)
            {
                 start = 0;
                 write = 0;
                 buffer = new byte[0];
            }
            if (string.IsNullOrEmpty(Response.StartString))
                Response.StartString  =  "HTTP/1.1 200 OK";
            
                if (Response.CashControl != null
                        && Response.CashControl.Count == 0)
                    Response.CashControl = new List<string> 
                                               { 
                                                   "no-case", 
                                                   "no-store" 
                                               };
                if (Response.ContentType != null
                        && Response.ContentType.Count == 0)
                    Response.ContentType = new List<string>
                                               {
                                                   "text/plain",
                                                   "charset=utf-8"
                                               };

            lock (Sync)
            {
                if (Response.IsEnd 
                    && (State == States.Close 
                         || State == States.Disconnect))
                    result = false;
                else
                {
                    try
                    {
                        switch (Response.ContentEncoding)
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
                        Log.Loging.AddMessage(exc.Message + Log.Loging.NewLine + exc.StackTrace, "log.log", Log.Log.Debug);
                    }
                }
            }
            return result;
        }

        protected override void End()
        {
            lock (Sync)
            {
                if (__Arhiv != null)
                    __Arhiv.Dispose();
                // Отправить блок данных chunked 0CRLFCRLF
                if (Response.TransferEncoding == "chunked")
                {
                    try
                    {
                        if (State != States.Close
                             || State != States.Disconnect)
                            __Writer.Eof();
                    }
                    catch (IOException exc)
                    {
                        close();
                        Log.Loging.AddMessage(exc.Message + Log.Loging.NewLine + exc.StackTrace, "log.log", Log.Log.Debug);
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
                __Reader.header = Request;
                __Writer.header = Response;
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
                        if (Result.Jump)
                            Result.Option = TaskOption.Protocol;
                        else
                            OnEventData();
                        break;
                    case HTTPFrame.CHUNK:
                            OnEventChunk();
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
            if (Response.IsRes)
                close();
            else
            {
                if (Exception.Status.value >= 500)
                    close();
                else if (Exception.Status.value >= 400)
                {
                    Response.ClearHeaders();
                    Response.StartString = "HTTP/1.1 " + 
                                     Exception.Status.value.ToString() +  
                                     " " + Exception.Status.ToString();
                    
                    file(  "Html/" + Exception.Status.value.ToString() + 
                                                        ".html", 6000  );
                }
                else
                    close();
            }

        }
        protected override void Connection()
        {
            OnEventConnect();
        }
    }
}
