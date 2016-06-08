using System;
using System.Net.Sockets;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	
	class HTTPProtocol : HTTP
	{
		const long WAIT = 10 * 1000 * 1000 * 20;

		HTTPStream _Reader;
		public override StreamS Reader
		{
			get
			{
				return _Reader;
			}
		}
		HTTPStream _Writer;
		public override StreamS Writer
		{
			get
			{
				return _Writer;
			}
		}
		public HTTPProtocol(Socket tcp) :
			base()
        {
            Tcp = tcp;
			_Reader = new HTTPStream(1000 * 32)
			{
				header = Request
			};
			_Writer = new HTTPStream(1000 * 128)
			{
				header = Response
			};
			TaskResult.Protocol   =   TaskProtocol.HTTP;
		}
		protected override void Work()
		{
			if ((__twaitconn + WAIT) < DateTime.Now.Ticks)
				Close(string.Empty);
			OnEventWork();
		}
		protected override void Data()
		{
			if (_Reader.Empty)
				return;
			
			if (!_Reader.frame.GetHead)
			{
				_Reader.header = Request;
				if (_Reader.ReadHead() == -1)
					return;
				
				if (Request.ContainsKey("upgrade"))
				{
					TaskResult.Jump = true;
					if (Request["upgrade"].ToLower() == "websocket")
					{
						string version = string.Empty;
						string protocol	= string.Empty;
						if (Request.ContainsKey("websocket-protocol"))
						{
							version = Request["websocket-protocol"].ToLower();
							protocol = "websocket-protocol";
						}
						else if (Request.ContainsKey("sec-websocket-version"))
						{
							version = Request["sec-websocket-version"].ToLower();
							protocol = "sec-websocket-version";
						}
						else if (Request.ContainsKey("sec-websocket-protocol"))
						{
							version = Request["sec-websocket-protocol"].ToLower();
							protocol = "sec-websocket-protocol"; 
						}
						switch (version)
						{
							case "7":
								TaskResult.Jump = true;
								TaskResult.Protocol = TaskProtocol.WSN13;
								break;
							case "8":
								TaskResult.Jump = true;
								TaskResult.Protocol = TaskProtocol.WSN13;
								break;
							case "13":
								TaskResult.Jump = true;
								TaskResult.Protocol = TaskProtocol.WSN13;
								break;
							case "sample":
								TaskResult.Jump = true;
								_Reader.frame.Handl = 1;
								_Reader.frame.bleng = 8;
								TaskResult.Protocol = TaskProtocol.WSAMPLE;
								break;
							}
						}
					}
							else if (!__handconn)
								throw new HTTPException("Неверные заголовки");

					if (Request.ContainsKey("connection")
					    && Request["connection"] == "close")
					{
						Request.Close = true;
						Response.Close = true;
						Response.Add("Connection", "close");
					}
					if (Request.ContainsKey("content-length"))
					{
						if (int.TryParse(Request["content-length"], 
													  out _Reader.frame.bleng))
							if (_Reader.frame.bleng > 0)
								_Reader.frame.Handl = 1;
							else
								throw new HTTPException("Неверные заголовки");
					}
					if (Request.ContainsKey("transfer-encoding"))
					{
							if (_Reader.frame.bleng > 0)
								throw new HTTPException("Неверные заголовки");
							else
								_Reader.frame.Handl = 2;
					}	
				}
			if (!_Reader.frame.GetBody)
			{
				_Writer.header = Response;
				if (_Reader.ReadBody() == -1)
					return;

			    if (_Reader.frame.Pcod == HTTPFrame.DATA)
				{
					Request.SetReq();
					if (TaskResult.Jump)
						TaskResult.Option = TaskOption.Protocol;
					_Reader.frame.Clear();
					return;
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

		}
		protected override void Connection(IHeader request, IHeader response)
		{
			OnEventConnect(request, response);
		}
    }
}
