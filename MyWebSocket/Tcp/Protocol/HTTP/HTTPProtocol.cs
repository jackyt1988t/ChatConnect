using System;
using System.Net.Sockets;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	
	class HTTPProtocol : HTTP
	{
		const long WAIT = (long)10 * 1000 * 1000 * 15;

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
			Result.Protocol = TaskProtocol.HTTP;
			_Reader = new HTTPStream(MINLENGTHBUFFER)
			{
				header = Request
			};
			_Writer = new HTTPStream(MINLENGTHBUFFER)
			{
				header = Response
			};
		}
		protected override void Work()
		{
			OnEventWork();
			if ((__twaitconn + WAIT) < DateTime.Now.Ticks)
				close();
			
		}
		protected override void Data()
		{
			
			if (_Reader.Empty)
				return;

			if ( _Reader._Frame.GetHead && _Reader._Frame.GetBody )
			{
				_Reader._Frame.Clear();
				_Reader.header = Request;
				_Writer.header = Response;
			}
			if (!_Reader._Frame.GetHead)
			{
				if (_Reader.ReadHead() == -1)
					return;
				
				switch (Request.Method)
				{
					case "GET":
						if (_Reader._Frame.bleng > 0)
							throw new HTTPException("Неверная длина запроса", HTTPCode._400_);
						break;
					default:
							throw new HTTPException("Метод не поддерживается", HTTPCode._501_);
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
								_Reader._Frame.Handl = 1;
								_Reader._Frame.bleng = 8;
								Result.Protocol = TaskProtocol.WSAMPLE;
								break;
						}
					}
				}
				if (!Result.Jump)
					OnEventOpen(Request, Response);
			}
			if (!_Reader._Frame.GetBody)
			{
				if (_Reader.ReadBody() == -1)
					return;

				switch (_Reader._Frame.Pcod)
				{
					case HTTPFrame.DATA:
						if (Result.Jump)
							Result.Option = TaskOption.Protocol;
						else
							OnEventData();
						break;
					case HTTPFrame.CHUNK:
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

		}
		protected override void Connection()
		{
			OnEventConnect();
		}
    }
}
