using System;
using System.Net.Sockets;

		using System.Text;

namespace ChatConnect.Tcp.Protocol.HTTP
{
	
	class HTTPProtocol : HTTP
	{
		const long WAIT = 10 * 1000 * 1000 * 20;

		HTTPFrame __HTTPFrame;

        public object SyncEvent
        {
            get;
            private set;
        }
        public override event PHandlerEvent EventWork
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
        public override event PHandlerEvent EventData
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
        public override event PHandlerEvent EventError
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
        public override event PHandlerEvent EventClose
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
        public override event PHandlerEvent EventConnect
        {
            add
            {
                lock (SyncEvent)
                    __EventConnect += value;

            }
            remove
            {
                lock (SyncEvent)
                    __EventConnect -= value;
            }
        }
        private event PHandlerEvent __EventWork;
		private event PHandlerEvent __EventData;
		private event PHandlerEvent __EventError;
		private event PHandlerEvent __EventClose;
		private event PHandlerEvent __EventConnect;

        public HTTPProtocol(Socket tcp, PHandlerEvent connect)
        {
            Tcp         = tcp;
			State       = States.Work;
			Request     = new Header();
			Response    = new Header();
			SyncEvent   = new object();
			TaskResult  = new TaskResult();
			__HTTPFrame = new HTTPFrame ();
			__twaitconn = DateTime.Now.Ticks;
			__EventConnect       +=   connect;
			TaskResult.Protocol   =   TaskProtocol.HTTP;
		}
		protected override void Work()
		{
			if ((__twaitconn + WAIT) < DateTime.Now.Ticks)
				State = States.Close;
			OnEventWork();
		}
		protected override void Data()
		{
			byte[] buffer;
			if (Request.SegmentsBuffer.Count > 0)
			{				
				lock (Request.SegmentsBuffer)
					  buffer = 
					Request.SegmentsBuffer.Dequeue();

				HandlerFrame(new HTTPStream(buffer));
			}
			else
			{
				int recive = 0;
				int length = Tcp.Available;
				if (length > 4000)
					length = 4000;
				if (Request.IsReq)
					Request = new Header();

				buffer = new byte[ length ];

				SocketError error;
				recive += Tcp.Receive(buffer, recive, length, SocketFlags.None, out error);
				if (error != SocketError.Success && error != SocketError.WouldBlock)
				{
					throw new HTTPException ( "Ошибка при чтении данных из Socket" );
				}
				HandlerFrame(new HTTPStream(buffer, 0, recive));
			}
		}
		protected override void Send()
		{
			if (Response.SegmentsBuffer.Count == 0)
			{
				if (Response.IsEnd)
				{
					Response = new Header();
					if (!Request.ContainsKey("connection")
					  || Request["connection"] != "close")
						State = States.Work;
					else
						State = States.Close;
				}
			}
			else
			{
				while (Response.SegmentsBuffer.Count > 0)
				{
					byte[] data;
					int recive = 0;
					int length = 0;
					int counts = 0;
					if (!Response.IsRes)
					{
						Response.Res();
						data = Response.ToByte();
					}
					else
					{
						lock (Response.SegmentsBuffer)
						{
							data = Response.
										SegmentsBuffer.Dequeue();
						}
					}

					SocketError error;
					while (recive < data.Length)
					{
						length = data.Length - recive;
						recive += Tcp.Send(data, recive, length, SocketFlags.None, out error);
						if (error != SocketError.Success)
						{
							if (error != SocketError.WouldBlock
									&& error != SocketError.NoBufferSpaceAvailable)
							{
								throw new HTTPException("Ошибка при отправке данных в Socket");
							}
						}
					}
					if ((counts += data.Length) > 1000 * 32)
						break;
				}
			}
		}
		protected override void Close()
		{
			OnEventClose();

			Request = null;
			Response = null;
			__HTTPFrame.Clear();
			
		}
		protected override void Connection()
		{
			OnEventConnect();
		}
		protected override void OnEventWork()
		{
			string s = "work";
			string m = "Цикл обработки";
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventWork;
			if (e != null)
				e(this, new PEventArgs(m, s));
		}
		protected override void HandlerFrame( HTTPStream stream )
		{
			while ((stream.Length - stream.Position)  >  0)
			{
				if (!__HTTPFrame.GetHead)
				{
					if (stream.ReadHead(ref __HTTPFrame, Request) == -1)
						break;
					
					__HTTPFrame.Handl = 0;
					if (Request.ContainsKey( "upgrade" ))
					{
						string ng = Request[ "upgrade" ];
						if (ng.ToLower() == "websocket")
						{
							if (Request.ContainsKey("sec-websocket-key"))
							{
								TaskResult.Protocol = TaskProtocol.WSRFC76;
							}
							else if(Request.ContainsKey("sec-websocket-key1"))
							{
								TaskResult.Protocol = TaskProtocol.WSRFC75;
								__HTTPFrame.Handl = 1;
								__HTTPFrame.bleng = 8;
							}
						}
					}

						if (Request.ContainsKey("content-length"))
						{
							if (int.TryParse(Request["content-length"],
													  out __HTTPFrame.bleng))
								if (__HTTPFrame.bleng > 0)
									__HTTPFrame.Handl = 1;
							else
								throw new HTTPException("Неверные заголовки");
						}
						if (Request.ContainsKey("transfer-encoding"))
							__HTTPFrame.Handl = 2;
				}
				if (!__HTTPFrame.GetBody)
				{
					if (stream.ReadBody(ref __HTTPFrame, Request) == -1)
						break;
					if (__HTTPFrame.Pcod != HTTPFrame.CHUNK)
					{
						if ((stream.Length - stream.Position) > 0)
							Request.SegmentsBuffer.Enqueue(stream
														.ToArray());

						Request.Req();
						__HTTPFrame.Clear();
						switch (TaskResult.Protocol)
						{	
							case TaskProtocol.WSRFC76:								
								TaskResult.Option = TaskOption.Protocol;
								break;
							case TaskProtocol.HTTP:
								State = States.Connection;
								File("Html" + Request.Path );
								break;
						}						
						return;
					}
				}
			}
		}
		protected override void OnEventData()
		{
			string s = "data";
			string m = "Получен фрейм с данными";

			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventData;
			if (e != null)
				e(this, new PEventArgs(s, m, null));
		}
        protected override void OnEventClose()
        {
			string s = "close";
			string m = "Соединение было закрыто";

			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventClose;
			if (e != null)
				e(this, new PEventArgs(s, m, null));
		}
        protected override void OnEventError(HTTPException _error)
        {
			string s = "error";
			string m = "Произошла ошибка во время исполнения";

			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventError;
			if (e != null)
				e(this, new PEventArgs(s, m, _error));
		}
        protected override void OnEventConnect()
        {
			string s = "connect";
			string m = "Соединение было установлено, протокол ws";

			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventConnect;
			if (e != null)
				e(this, new PEventArgs(s, m, null));
		}
    }
}