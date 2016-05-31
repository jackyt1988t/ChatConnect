using System;
using System.Net.Sockets;

namespace ChatConnect.Tcp.Protocol.HTTP
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
			writer = new HTTPStream(1000 * 32)
			{
				header = Response
			};
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
			if (!reader.frame.GetHead)
			{
				reader.header = Request;
				if (reader.ReadHead() == -1)
					return;
		
				reader.frame.Handl = 0;
				if (Request.ContainsKey("upgrade"))
				{
					string ng = Request["upgrade"];
					if (ng.ToLower() == "websocket")
					{
						string protocol = string.Empty;
						if (Request.ContainsKey("websocket-protocol"))
							protocol = Request["websocket-protocol"];
						else if (Request.ContainsKey("sec-websocket-version"))
							protocol = Request["sec-websocket-version"];
						else if (Request.ContainsKey("sec-websocket-protocol"))
							protocol = Request["sec-websocket-protocol"];
						
						switch (protocol)
						{
							case "sample":
								reader.frame.Handl = 1;
								reader.frame.bleng = 8;
								break;
							case "7":
								TaskResult.Protocol = TaskProtocol.WSRFC76;
								break;
							case "8":
								TaskResult.Protocol = TaskProtocol.WSRFC76;
								break;
							case "13":
								TaskResult.Protocol = TaskProtocol.WSRFC76;
								break;
							default:
								throw new HTTPException("Неверные заголовки");
							}
						}
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
					if (TaskResult.Protocol != TaskProtocol.HTTP)
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
