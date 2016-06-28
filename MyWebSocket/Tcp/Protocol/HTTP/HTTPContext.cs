using System;

using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	public class HTTPContext
	{
		public Header Request;
		public Header Response;
		public Object __ObSync;

		internal Stream __Encode;
		internal HTTPWriter __Writer;
		public HTTPContext()
		{
			Request = new Header();
			Response = new Header();
			__ObSync = new object();
			__Writer = new HTTPWriter(36000)
			{
				header = Response = new Header()
			};
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
		}
		async public void File(string path, int chunk = 1000 * 64)
		{
			await Task.Run(() =>
			{
				try
				{
					file(path, chunk);
				}
				catch (Exception err)
				{
					Response.SetClose();
				}
				finally
				{
					End();
				}
			});
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="_chunk"></param>
		protected void file(string path, int _chunk)
		{
			FileInfo fileinfo = new FileInfo(path);

			if (!fileinfo.Exists)
			{
				throw new HTTPException("Файл не найден " + path, HTTPCode._404_);
			}
			else
			{
				if (Response.ContentType == null
					|| Response.ContentType.Count == 0)
				{
					string extension =
							fileinfo.Extension.Substring(1);
					Response.ContentType = new List<string>()
										   {
											   "text/" + extension,
											   "charset=utf-8"
										   };
				}
				using (FileStream stream = fileinfo.OpenRead())
				{
					int i = 0;
					int _count = (int)(stream.Length / _chunk);
					int length = (int)(stream.Length - _count * _chunk);

					if (string.IsNullOrEmpty(Response.TransferEncoding))
						Response.ContentLength = (int)stream.Length;

					while (i++ < _count)
					{
						int recive = 0;
						byte[] buffer = new byte[_chunk];
						while ((_chunk - recive) > 0)
						{
							recive = stream.Read(buffer, recive, _chunk - recive);
						}
						Message(buffer, 0, _chunk);
					}
					if (length > 0)
					{
						int recive = 0;
						byte[] buffer = new byte[length];
						while ((length - recive) > 0)
						{
							recive = stream.Read(buffer, recive, length - recive);
						}
						Message(buffer, 0, length);
					}
				}
			}
		}
		public void End()
		{
			lock (__ObSync)
			{
				if (Response.IsEnd)
					throw new HTTPException("Отправка данных закончена");
				
					Response.SetEnd();
				if (__Encode != null)
					__Encode.Dispose();
				// Отправить блок данных chunked 0CRLFCRLF
				if (Response.TransferEncoding == "chunked")
				{
					try
					{
						__Writer.Eof();
					}
					catch (IOException exc)
					{
						Response.SetClose();
						Log.Loging.AddMessage(exc.Message + "./r/n" + exc.StackTrace, "log.log", Log.Log.Debug);
					}
				}
			}
		}
		
		public void Message(string message)
		{
			Message(Encoding.UTF8.GetBytes(message));
		}
		public void Message(byte[] message)
		{
			Message(  message, 0 , message.Length  );
		}
		/// <summary>
		/// Записываем данные в стандартный поток, если заголвок Content-Encoding
		/// установлен в gzip декодируем данные в формате gzip(быстрое сжатие)
		/// </summary>
		/// <param name="message">массив данных</param>
		/// <param name="recive">стартовая позиция</param>
		/// <param name="length">количество которое необходимо записать</param>
		public void Message(byte[] message, int recive, int length)
		{
			lock (__ObSync)
			{
				if (Response.IsEnd)
					throw new HTTPException("Отправка данных закончена");
				if (!Response.IsRes)
				{
					if (string.IsNullOrEmpty(Response.StrStr))
						Response.StrStr = "HTTP/1.1 200 OK";

					if (Response.Connection == "close")
						Response.SetClose();
					if (Response.ContentType == null
						 || Response.ContentType.Count == 0)
						Response.ContentType = new List<string>
												   {
													   "text/plain",
													   "charset=utf-8"
												   };
					if (Response.CacheControl == null
						 || Response.CacheControl.Count == 0)
						Response.CacheControl = new List<string>
													{
														"no-store",
														   "no-cache",
													};
					if (Response.ContentEncoding == "gzip")
						__Encode = new GZipStream(__Writer, CompressionLevel.Fastest, true);
					else if (Response.ContentEncoding == "deflate")
						__Encode = new DeflateStream(__Writer, CompressionLevel.Fastest, true);
					Log.Loging.AddMessage("Http заголовки успешно обработаны: \r\n" +
										  "Заголовки зап:\r\n" + Response.ToString(), "log.log", Log.Log.Info);
				}
				try
				{
					switch (Response.ContentEncoding)
					{
						case "gzip":
						__Encode.Write(message, recive, length);
						break;
						case "deflate":
						__Encode.Write(message, recive, length);
						break;
						default:
						__Writer.Write(message, recive, length);
						break;
					}
					Log.Loging.AddMessage("Http данные успешно добавлены к отправке", "log.log", Log.Log.Info);
				}
				catch (IOException error)
				{
					Response.SetClose();
					Log.Loging.AddMessage(error.Message + "./r/n" + error.StackTrace, "log.log", Log.Log.Info);
				}
			}
		}
	}
}