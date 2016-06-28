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
		protected Queue<HTTPContext> Contexts;

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

			Contexts = new Queue<HTTPContext>();
			Contexts.Enqueue( (Context = new HTTPContext()
			{
				Request = Request
			}) );
				Response = Context.Response;
				__Writer = Context.__Writer;
				
								
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
		protected override void End()
		{
			if (Contexts.Count > 0)
			{
				HTTPContext context = Contexts.Dequeue();

				__Writer = context.__Writer;
				Response = context.Response;
			}
		}
		protected override void Work()
        {
            OnEventWork();
            // вермя до закрытия(  keep-alive  )
            if (Alive.Ticks < DateTime.Now.Ticks && Writer.Empty)
            {
                close();
                        Log.Loging.AddMessage( "Соединеине Keep-Alive вермя истекло", "log.log", Log.Log.Info );
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

            /*--------------------------------------------------------------------------------------------------------
            
                Обрабатываем заголвоки запроса. Если заголвоки не были получены, читаем данные из кольцевого 
                потока данных. Проверяем доступные методы обработки. При переходе на Websocket, меняем протокол WS 
                Устанавливаем заголвоки:
                Date
                Server
                Connection(если необходимо)
                Cintent-Encoding(если в заголвоке Accept-Encoding указаны gzip или deflate)
                Когда все заголвоки будут получены и пройдут первоначальную обработку произойдет событие EventOpen

            --------------------------------------------------------------------------------------------------------*/
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
					
                }
            }
            /*--------------------------------------------------------------------------------------------------------

                Обрабатываем тело запроса. Если тело не было получено, читаем данные из кольцевого потока данных.
                При заголвоке Transfer-Encoding тело будет приходяить по частям и будет полность получено после 0
                данных и насупить событие EventData, до этого момента будет насупать событиеEventChunk, если был 
                указан заголвок Content-Length событие EventChunk происходить не будет, а событие EventData 
                произойдет только тогда когда все данные будут получены. Максимальный размер блока данных chuncked
                можно указать задав значение HTTPReader.LENCHUNK. В случае с Content-Length можно обработать 
                заголвок в соыбтие EventOpen и при привышении допустимого значения закрыть соединение.
            
            --------------------------------------------------------------------------------------------------------*/
            if (!__Reader._Frame.GetBody)
            {
                if (__Reader.ReadBody() == -1)
                    return;

                switch (__Reader._Frame.Pcod)
                {
                    case HTTPFrame.DATA:
                        if (!Result.Jump)
                        {
                            OnEventData(Context);
							Contexts.Enqueue((Context = new HTTPContext()
							{
								Request = Request = new Header()
							}));

						Log.Loging.AddMessage( "Все данные Http запроса получены", "log.log", Log.Log.Info );
                        }
                        else
                            Result.Option = TaskOption.Protocol;
                        break;
                    case HTTPFrame.CHUNK:
                            OnEventChunk(Context);
                            Log.Loging.AddMessage("Часть данных Http запроса получена", "log.log", Log.Log.Info);
                        break;
                }
            }
        }
        protected override void Close()
        {
			foreach (HTTPContext cntx in Contexts)
			{
				cntx.Response.SetEnd();
			}
            OnEventClose();
            
        }
        protected override void Error(HTTPException error)
        {
            OnEventError(error);

                        Log.Loging.AddMessage("Информация об ошибке:\r\n" + 
                                              "Ошибка протокола Http:"+ error.Message, "log.log", Log.Log.Info);
								
            if (Response.IsRes || error.Status.value == 500)
                close();
            else
            {
                lock (ObSync)
                {
                    __Writer.header = Response = new Header();
              	}
                        Response.StrStr = "HTTP/1.1 " + error.Status.value
                                                                    .ToString()
                                                + " " + error.Status.ToString();
						
                        Log.Loging.AddMessage("Информация об ошибке готова к отправке", "log.log", Log.Log.Info);
            }
        }
        protected override void Connection()
        {
            OnEventConnect();
        }
    }
}
