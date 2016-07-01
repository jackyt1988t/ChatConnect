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
		/// <summary>
		/// Входящие заголвоки
		/// </summary>
		public Header Request;
		/// <summary>
		/// Исходящие заголвоки
		/// </summary>
		public Header Response;
		/// <summary>
		/// Синхронизация текущего объекта
		/// </summary>
		public Object __ObSync;

		volatile bool _to_;
		/// <summary>
		/// HTTP
		/// </summary>
		internal HTTProtocol HTTP;
		/// <summary>
		/// Поток
		/// </summary>
		internal Stream __Encode;
		/// <summary>
		/// Поток чтения
		/// </summary>
		internal HTTPReader __Reader;
		/// <summary>
		/// Поток записи
		/// </summary>
		internal HTTPWriter __Writer;
		/// <summary>
		/// Последняя ошибка
		/// </summary>
		internal HTTPException _1_Error;

		/// <summary>
		/// Создает контекст получения, отправки данных
		/// </summary>
		/// <param name="http">HTTP</param>
		public HTTPContext(HTTProtocol http)
		{
			HTTP = http;

			Request  = new Header();
			Response = new Header();
			__ObSync = new object();

			(__Reader =
			(HTTPReader)http.Reader)
					   ._Frame.Clear();
			__Reader.Header  =  Request;

			__Writer = new HTTPWriter(10000)
			{
				Header = Response
			};	
		}
		/// <summary>
		/// 
		/// </summary>
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
							HTTP.HTTPClose();
						Log.Loging.AddMessage(exc.Message + "./r/n" + exc.StackTrace, "log.log", Log.Log.Debug);
						
					}
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		async
		public Task<bool> AsMsg(byte[] msg)
		{
			
			int i = 0;
			int _chunk = 1000 * 32;
			int _count = (int)(msg.Length / _chunk);
			int length = (int)(msg.Length - _count * _chunk);

			lock (__ObSync)
			{
				if (!_to_)
					_to_ = true;
				else
					throw new HTTPException("Дождитесь окончание записи");
			}

			return await Task.Run<bool>(() =>
			{
				while (i++ < _count)
				{
					if (HTTP.State == States.Close
						 || HTTP.State == States.Disconnect)
					{
						_to_ = false;
						return false;
					}
					HTTP.Len += _chunk;
					Message(msg, i * _chunk, _chunk);

				}
				if (length > 0)
				{
					if (HTTP.State == States.Close
						 || HTTP.State == States.Disconnect)
					{
						_to_ = false;
						return false;
					}
					HTTP.Len += _chunk;
					Message(msg, i * _chunk, length);

				}
				return true;
			});
			
		}
		/// <summary>
		/// Асинхрооно записывает файл
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		async
		public Task<bool> AsFile(string path)
		{
			FileInfo Info = new FileInfo(path);
			lock (__ObSync)
			{
				if (!_to_)
					_to_ = true;
				else
					throw new HTTPException("Дождитесь окончание записи");
				if (Response.ContentType == null
					|| Response.ContentType.Count == 0)
				{
					string ext = string.Empty;
					if (string.IsNullOrEmpty(Info.Extension))
						ext = "plain";
					else
						ext = Info.Extension.Substring(1);
					Response.ContentType = new List<string>()
											   {
												   "text/" + ext,
												   "charset=utf-8"
											   };
				}
			}
			if (!Info.Exists)
			{
				if (_1_Error == null)
					HTTP.HTTPError((_1_Error =
						new HTTPException("Файл не найден " + path, HTTPCode._404_)));
				else
					HTTP.HTTPError((_1_Error =
						new HTTPException("Файл не найден " + path, HTTPCode._500_)));
			}
				return await Task.Run<bool>(() =>
				{
					int i = 0;
					int _chunk = 1000 * 32;
					try
					{
						using (FileStream stream = Info.OpenRead())
						{
							int _count = (int)(stream.Length / _chunk);
							int length = (int)(stream.Length - _count * _chunk);

							if (Response.ContentLength == 0
								 && string.IsNullOrEmpty(Response.TransferEncoding))
								Response.ContentLength = (int)stream.Length;
							
							byte[] buffer = new byte[_chunk];
							while (i++ < _count)
							{
								int recive = 0;

								while ((_chunk - recive) > 0)
								{
									recive = stream.Read(buffer, recive, _chunk - recive);
								}

								if (HTTP.State == States.Close
									 || HTTP.State == States.Disconnect)
									return false;

								HTTP.Len += _chunk;
								Message(buffer, 0, _chunk);

							}
							if (length > 0)
							{
								int recive = 0;

								while ((length - recive) > 0)
								{
									recive = stream.Read(buffer, recive, length - recive);
								}

								if (HTTP.State == States.Close
									 || HTTP.State == States.Disconnect)
									return false;

								HTTP.Len += _chunk;
								Message(buffer, 0, length);

							}
						}
					}
					catch (Exception error)
					{
						HTTP.HTTPError((_1_Error =
							new HTTPException(error.Message + "./r/n" + error.StackTrace, HTTPCode._500_)));
						return false;
					}
					finally
					{
						_to_ = false;
					}
					return true;
				});
		}
		/// <summary>
		/// Записывает строку в стандартный поток, если заголвок Content-Encoding
		/// установлен в gzip декодируем данные в формате gzip(  быстрое сжатие  ) 
		/// </summary>
		/// <param name="message"></param>
		public void Message(string message)
		{
			Message(Encoding.UTF8.GetBytes(message));
		}
		/// <summary>
		/// Записываем данные в стандартный поток, если заголвок Content-Encoding
		/// установлен в gzip декодируем данные в формате gzip(  быстрое сжатие  )
		/// </summary>
		/// <param name="message"></param>
		public void Message(byte[] message)
		{
			Message(  message, 0 , message.Length  );
		}
		/// <summary>
		/// Записываем данные в стандартный поток, если заголвок Content-Encoding
		/// установлен в gzip декодируем данные в формате gzip(  быстрое сжатие  )
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
					HTTP.HTTPError(new HTTPException(error.Message + "./r/n" + error.StackTrace, HTTPCode._500_));
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		internal void Hadler()
		{
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
				if (!__Reader.ReadHead())
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
					HTTP.Result.Jump = true;
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
							HTTP.Result.Jump = true;
							HTTP.Result.Protocol = TaskProtocol.WSN13;
							break;
							case "8":
							HTTP.Result.Jump = true;
							HTTP.Result.Protocol = TaskProtocol.WSN13;
							break;
							case "13":
							HTTP.Result.Jump = true;
							HTTP.Result.Protocol = TaskProtocol.WSN13;
							break;
							case "sample":
							HTTP.Result.Jump = true;
							__Reader._Frame.Handl = 1;
							__Reader._Frame.bleng = 8;
							HTTP.Result.Protocol = TaskProtocol.WSAMPLE;
							break;
						}
					}
				}
				else
				{
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

					HTTP.OnEventOpen(this);
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
				if (!__Reader.ReadBody())
					return;

				switch (__Reader._Frame.Pcod)
				{
					case HTTPFrame.DATA:
					if (!HTTP.Result.Jump)
					{
						HTTP.OnEventData(this);

						Log.Loging.AddMessage("Все данные Http запроса получены", "log.log", Log.Log.Info);
					}
					else
						HTTP.Result.Option = TaskOption.Protocol;
					break;
					case HTTPFrame.CHUNK:
					HTTP.OnEventChunk(this);
					Log.Loging.AddMessage("Часть данных Http запроса получена", "log.log", Log.Log.Info);
					break;
				}
			}
		}
	}
}
