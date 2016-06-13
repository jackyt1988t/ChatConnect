using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	
	class HTTPProtocol : HTTP
	{
		public static readonly int CHUNK;
		public static readonly long ALIVE;
		public static readonly byte[] ENDCHUNCK;
		public static readonly byte[] EOFCHUNCK;

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
		static HTTPProtocol()
		{
			ALIVE = TimeSpan.TicksPerSecond * 15;
			ENDCHUNCK =
				new byte[] { 0x0D, 0x0A };
			EOFCHUNCK =
				new byte[] { 0x30, 0x0D, 0x0A, 0x0D, 0x0A };
		}
		public HTTPProtocol(Socket tcp) :
			base()
        {
            Tcp = tcp;
			Result.Protocol = TaskProtocol.HTTP;
			__twaitconn = DateTime.Now.Ticks;
			_Reader = new HTTPStream(MINLENGTHBUFFER)
			{
				header = Request
			};
			_Writer = new HTTPStream(MINLENGTHBUFFER)
			{
				header = Response
			};
		}
		public override void file(string path, int chunk)
		{
			int i = 0;
			Response.StartString = "HTTP/1.1 200 OK";
			Response.AddHeader("Content-Type", "text/" + Request.File);
			//Response.AddHeader("Content-Length", sr.Length.ToString());
						Response.AddHeader("Transfer-encoding", "chunked");
			
			FileInfo fileinfo = new FileInfo(path);
			if (!fileinfo.Exists)
			{
				throw new HTTPException("Файл не найден " + path, HTTPCode._404_);
			}
			else
			{
				using (FileStream sr = fileinfo.OpenRead())
				{
					int _count = (int)(sr.Length / chunk);
					int length = (int)(sr.Length - _count * chunk);
					while (i++ < _count)
					{
						int recive = 0;
						byte[] buffer = new byte[chunk];
						while ((chunk - recive) > 0)
						{
							recive = sr.Read(buffer, recive, chunk - recive);
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
						if (!Message(buffer, 0, length))
							return;
					}
				}
			}
		}
		public override bool Message(byte[] buffer, int start, int write)
		{
			bool result = true;
		/*==============================================================
			Отправляем заголвоки если они еще не были отправлены.
			Если указан заголвок Content-Length равен 0 устанавливаем
			заголовок Transfer-Encoding равным chunked.
			Отправляем данные, если заголвок Content-Length больше 0
			отправляем данные как есть, иначе отправляем данные 
			в формате chunked.
		================================================================*/
			lock (Sync)
			{
				// Заголвоки HTTP
				if (!Response.IsRes)
				{
					if (Response.ContentLength == 0)
						Response.TransferEncoding = "chunked";
					// отправить HTTP заголвоки запроса
					result = Message(Response.ToByte());
									 Response.SetRes();

					if (!result)
						return false;
				}
					// оптравить форматированные данные запроса
					if (Response.TransferEncoding != "chunked")
						result = message(  buffer, start, write  );
					else
					{
						byte[] lenCHUNCK = Encoding.UTF8.GetBytes(
											  write.ToString("X"));
						
						result = Message(lenCHUNCK);
						if (!result)
							return false;
						result = Message(ENDCHUNCK);
						if (!result)
							return false;
						result = message(  buffer, start, write  );
						if (!result)
							return false;
						result = Message(ENDCHUNCK);
					}
			}

			return result;
		}

		protected override void End()
		{
			bool result = true;
			lock (Sync)
			{
					// Отправить блок данных chunked 0CRLFCRLF
					if (Response.TransferEncoding == "chunked")
						result = Message(EOFCHUNCK);
			}
			if (result)
				Console.WriteLine("Успешно");
		}
		protected override void Work()
		{
			OnEventWork();
			if ((__twaitconn + ALIVE) < DateTime.Now.Ticks)
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
			if (Response.IsRes)
				close();
			else
			{
				if (Exception.Status.value >= 500)
					close();
				else if (Exception.Status.value >= 400)
					;
				else if (Exception.Status.value >= 300)
					;
				else if (Exception.Status.value >= 200)
					;
				else
					;
			}

		}
		protected override void Connection()
		{
			OnEventConnect();
		}
    }
}
