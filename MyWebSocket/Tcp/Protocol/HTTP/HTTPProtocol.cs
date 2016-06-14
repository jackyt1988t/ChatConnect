using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	
	class HTTPProtocol : HTTP
	{
		public static readonly long ALIVE;
		public static readonly byte[] ENDCHUNCK;
		public static readonly byte[] EOFCHUNCK;
		
		/// <summary>
		/// Время ожидания запросов
		/// </summary>
		public TimeSpan Alive
		{
			get;
		}
		public GZipStream Compress
		{
			get;
			set;
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
			ENDCHUNCK =
				new byte[] { 0x0D, 0x0A };
			EOFCHUNCK =
				new byte[] { 0x30, 0x0D, 0x0A, 0x0D, 0x0A };
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
		public override void file(string path, int chunk)
		{
			int i = 0;
			Response.StartString = "HTTP/1.1 200 OK";
			Response.AddHeader("Content-Type", "text/" + 
							 Request.File + "; charset=utf-8");
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
					//Response.AddHeader("Content-Length", sr.Length.ToString());
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
		public override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Compress != null)
					Compress.Dispose();
			}
				base.Dispose(disposing);
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
			lock (Sync)
			{
				if (!Loop)
					result = false;
				else
				{
					try
					{
						if (Response.ContentEncoding != "gzip")
							__Writer.Write(buffer, start, write);
						else
							Compress.Write(buffer, start, write);
					}
					catch (IOException exc)
					{
						close();
						result = false;
						Log.Loging.AddMessage(exc.Message + Log.Loging.NewLine + exc.StackTrace, "Log/log.log", Log.Log.Debug);
					}
				}
			}
			return result;
		}

		protected override void End()
		{
			bool result = true;
			lock (Sync)
			{
				__Writer.Resize(MINLENGTHBUFFER);
				if (Response.ContentEncoding == "gzip")
				{
					if (Compress != null)
						Compress.Dispose();
				}
				// Отправить блок данных chunked 0CRLFCRLF
				if (Response.TransferEncoding == "chunked")
				{
					try
					{
						__Writer.Eof();
					}
					catch (IOException exc)
					{
						close();
						result = false;
						Log.Loging.AddMessage(exc.Message + Log.Loging.NewLine + exc.StackTrace, "Log/log.log", Log.Log.Debug);
					}
				}
			}
			if (result)
				Console.WriteLine("Успешно");
		}
		protected override void Work()
		{
			OnEventWork();
			if (Alive.Ticks < DateTime.Now.Ticks)
				close();
			
		}
		protected override void Data()
		{
			
			if (__Reader.Empty)
				return;

			if ( __Reader._Frame.GetHead && __Reader._Frame.GetBody )
			{
				__Reader._Frame.Clear();
				__Writer._Frame.Clear();
				__Reader.header = Request;
				__Writer.header = Response;
			}
			if (!__Reader._Frame.GetHead)
			{
				if (__Reader.ReadHead() == -1)
					return;

				switch (Request.Method)
				{
					case "GET":
						if (__Reader._Frame.bleng > 0)
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
								__Reader._Frame.Handl = 1;
								__Reader._Frame.bleng = 8;
								Result.Protocol = TaskProtocol.WSAMPLE;
								break;
						}
					}
				}
				if (Request.AcceptEncoding != null
					&& Request.AcceptEncoding.Count > 0
					&& Request.AcceptEncoding.Contains("gzip"))
				{
					Response.ContentEncoding = "gzip";
					Compress = new GZipStream(__Writer, CompressionLevel.Fastest, true);
				}

				if (!Result.Jump)
					OnEventOpen(Request, Response);
			}
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
