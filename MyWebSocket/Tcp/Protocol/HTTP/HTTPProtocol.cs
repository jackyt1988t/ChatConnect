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
		protected override void data()
		{	
			if (!_Reader._Frame.GetBody)
			{
				_Writer.header = Response;
				if (_Reader.ReadBody() == -1)
					return;

				switch (_Reader._Frame.Pcod)
				{
					case HTTPFrame.DATA:
					if (TaskResult.Jump)
						TaskResult.Option = TaskOption.Protocol;
					else
						OnEventData();
					break;
					case HTTPFrame.CHUNK:
					break;
				}
			}
		}
		protected override void Data()
		{
			
			if (_Reader.Empty)
				return;

			if (_Reader._Frame.GetHead && _Reader._Frame.GetBody)
			{
				_Reader._Frame.Clear();
				_Reader.header = Request;
				_Writer.header = Response;
			}
			if (!_Reader._Frame.GetHead)
			{
				_Reader.header = Request;
				if (_Reader.ReadHead() == -1)
					return;
				
				switch (Request.Method)
				{
					case "GET":
						if (_Reader._Frame.bleng > 0)
							throw new HTTPException("Неверная длина запроса");
						break;
					default:
							throw new HTTPException("Метод не поддерживается");
				}
				if (!string.IsNullOrEmpty(Request.Upgrade))
				{
					TaskResult.Jump = true;
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
								_Reader._Frame.Handl = 1;
								_Reader._Frame.bleng = 8;
								TaskResult.Protocol = TaskProtocol.WSAMPLE;
								break;
						}
					}
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
