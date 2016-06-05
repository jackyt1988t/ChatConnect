using System;
using System.Net.Sockets;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	
	class HTTPProtocol : HTTP
	{
		const long WAIT = 10 * 1000 * 1000 * 20;

		HTTPStream reader;
		public override StreamS Reader
		{
			get
			{
				return reader;
			}
		}
		HTTPStream writer;
		public override StreamS Writer
		{
			get
			{
				return writer;
			}
		}
		public HTTPProtocol(Socket tcp) :
			base()
        {
            Tcp = tcp;
			reader = new HTTPStream(1000 * 32)
			{
				header = Request
			};
			writer = new HTTPStream(1000 * 128)
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
			if (!reader.frame.GetHead)
			{
				reader.header = Request;
				if (reader.ReadHead() == -1)
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
								reader.frame.Handl = 1;
								reader.frame.bleng = 8;
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
													  out reader.frame.bleng))
							if (reader.frame.bleng > 0)
								reader.frame.Handl = 1;
							else
								throw new HTTPException("Неверные заголовки");
					}
					if (Request.ContainsKey("transfer-encoding"))
					{
							if (reader.frame.bleng > 0)
								throw new HTTPException("Неверные заголовки");
							else
								reader.frame.Handl = 2;
					}	
				}
			if (!reader.frame.GetBody)
			{
				writer.header = Response;
				if (reader.ReadBody() == -1)
					return;

			    if (reader.frame.Pcod == HTTPFrame.DATA)
				{
					Request.SetReq();
					if (TaskResult.Jump)
						TaskResult.Option = TaskOption.Protocol;
					reader.frame.Clear();
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
