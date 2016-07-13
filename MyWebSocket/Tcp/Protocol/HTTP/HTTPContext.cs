using System;

using System.IO;
using System.IO.Compression;

	using System.Text;

		using System.Threading;
		using System.Threading.Tasks;

			using System.Collections.Generic;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	public class HTTPContext : IContext
	{
		internal bool _to_;
		internal bool _ow_;
		/// <summary>
		/// Поток
		/// </summary>
		internal Stream __Encode;
		/// <summary>
		/// Протолкол HTTP
		/// </summary>
		internal HTTProtocol Protocol;
		/// <summary>
		/// Последняя ошибка
		/// </summary>
		internal HTTPException _1_Error;

		/// <summary>
		/// Закончена обр-ка
		/// </summary>
		public bool Cancel
		{
			get;
			private set;
		}
		/// <summary>
		/// Входящие заголвоки
		/// </summary>
		public Header Request
		{
			get;
			private set;
		}
		/// <summary>
		/// Исходящие заголвоки
		/// </summary>
		public Header Response
		{
			get;
			private set;
		}
		/// <summary>
		/// Синхронизация текущего объекта
		/// </summary>
		public object __ObSync
		{
			get;
			private set;
		}
		/// <summary>
		/// Поток чтения
		/// </summary>
		public HTTPReader __Reader
		{
			get;
		}
		/// <summary>
		/// Поток записи
		/// </summary>
		public HTTPWriter __Writer
		{
			get;
		}

		/// <summary>
		/// Создает контекст получения, отправки данных
		/// </summary>
		/// <param name="protocol">HTTP</param>
		public HTTPContext(HTTProtocol protocol, bool ow)
		{
			_ow_ = ow;

			Request  = new Header();
			Protocol =     protocol;
			Response = new Header();
			__ObSync = new object();

				__Reader = new HTTPReader( Protocol.GetStream );
				__Reader.Header = Request;
			
			if (!_ow_)
				__Writer = new HTTPWriter( new MyStream(4096) );
			else
				__Writer = new HTTPWriter( Protocol.GetStream );
				__Writer.Header = Response;	
		}
		public IContext Refresh()
		{
			lock (__ObSync)
			{
				if (!_ow_)
				{
					_ow_ = true;

					__Writer.Stream.CopyTo( Protocol.GetStream );
					__Writer.Stream.Dispose();

					if (!Cancel)
						__Writer.Stream  =  Protocol.GetStream;
				}
			}
				return this;
		}
		/// <summary>
		/// Возвращает новый контекст
		/// </summary>
		/// <returns></returns>
		public IContext Context()
		{
			//if ( Request.Upgrade == "websocket" )
				//return null;
			//else
				return new HTTPContext(Protocol, Cancel);
		}
		/// <summary>
		/// 
		/// </summary>
		public void End()
		{
			lock (__ObSync)
			{
				if (Cancel)
					throw new HTTPException("Отправка данных закончена");
				else
					Cancel = true;
			}
			try
			{
				if (__Encode != null)
					__Encode.Dispose();
				
				if (__Writer != null)
					__Writer.Dispose();
			}
			catch (IOException error)
			{
				Log.Loging.AddMessage(
					"Ошибка при записи ответа на запрос HTTP" +
					error.Message + "./r/n" + error.StackTrace, "log.log", Log.Log.Info);

				HandlerError(new HTTPException("Ошибка получения http данных " +
														  error.Message, HTTPCode._500_));
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public void Handler()
		{
			try
			{
				if (!__Reader.__Frame.GetHead)
				{
					HandlerHead();
				}

				if (!__Reader.__Frame.GetBody)
				{
					HandlerBody();
				}
			}
			
			catch (HTTPException error)
			{
				HandlerError(error);
				
				Log.Loging.AddMessage("Ошибка об-тки HTTP запроса " +
									  "Ошибка: " + error.Message, "log.log", Log.Log.Info);
			}
			catch (IOException error)
			{
				Log.Loging.AddMessage("Ошибка об-тки HTTP запроса " +
									  "Ошибка: " + error.Message, "log.log", Log.Log.Info);
				HandlerError(new HTTPException("Ошибка получения http данных " +
															 error.Message, HTTPCode._500_));
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		async
		public Task<bool> AsMssg(string msg)
		{
			return await AsMssg(Encoding.UTF8.GetBytes(msg));
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		async
		public Task<bool> AsMssg(byte[] msg)
		{
			
			int i = 0;
			int _chunk = 1000 * 32;
			int _count = (int)(msg.Length / _chunk);
			int length = (int)(msg.Length - _count * _chunk);

			lock (__ObSync)
			{
				if (_to_)
					throw new HTTPException("Дождитесь окончание записи");
				else
					_to_ = true;
			}

			return await Task.Run<bool>(() =>
			{
				if (Response.ContentLength == 0
						&& string.IsNullOrEmpty(
									Response.TransferEncoding))
					Response.ContentLength = ( int )msg.Length;
					while (i++ < _count)
				{
					if (Protocol.State == States.Close
						 || Protocol.State == States.Disconnect)
					{
						_to_ = false;
						return false;
					}
					
					Message(msg, i * _chunk, _chunk);
					        Thread.Sleep(5);

				}
				if (length > 0)
				{
					if (Protocol.State == States.Close
						 || Protocol.State == States.Disconnect)
					{
						_to_ = false;
						return false;
					}
					
					Message(msg, (i - 1) * _chunk, length);
					        Thread.Sleep(5);

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
			
			lock (__ObSync)
			{
				if (_to_)
					throw new HTTPException( "Дождитесь окончание записи " + path );
				else
					_to_ = true;
			}
			return await Task.Run<bool>(() =>
			{
				int i = 0;
				int _chunk = 1000 * 32;
				FileStream stream = null;

				try
				{

					FileInfo Info = new FileInfo(path);
					if (!Info.Exists)
					{
						_to_ = false;
						HandlerError(
							new HTTPException("Файл не найден " + path, HTTPCode._404_));
						
						return false;
					}

					stream = Info.OpenRead();
					int _count = (int)(stream.Length / _chunk);
					int length = (int)(stream.Length - _count * _chunk);

					if (!Response.IsRes)
					{
						if (Response.ContentType == null
							 || Response.ContentType.Count == 0)
						{
							string ext = string.Empty;
							if (string.IsNullOrEmpty(Info.Extension))
								ext = "plain";
							else
								ext = Info.Extension.Substring(1);
								Response.ContentType =
									new List<string>()
									{
										"text/" + ext,
										"charset=utf-8"
									};
						}
						if (Response.ContentLength == 0
							 && string.IsNullOrEmpty(Response.TransferEncoding))
							Response.ContentLength   =   (  int  )stream.Length;
					}
					
					
					byte[] buffer = new byte[_chunk];
					while (i++ < _count)
					{
						int recive = 0;

						while ((_chunk - recive) > 0)
						{
							recive = stream.Read(buffer, recive, _chunk - recive);
						}

						if (Protocol.State == States.Close
							 || Protocol.State == States.Disconnect)
						{
							_to_ = false;
							return false;
						}

						Message(buffer, 0, _chunk);
						        Thread.Sleep(5);

					}
					if (length > 0)
					{
						int recive = 0;

						while ((length - recive) > 0)
						{
							recive = stream.Read(buffer, recive, length - recive);
						}

						if (Protocol.State == States.Close
							 || Protocol.State == States.Disconnect)
						{
							_to_ = false;
							return false;
						}

						Message(buffer, 0, length);
						        Thread.Sleep(5);

					}
				}
				catch (Exception error)
				{
					
					Protocol.Close();
					Log.Loging.AddMessage(
						"Ошибка при отпарвке файла HTTP ответа, " +
						error.Message + "./r/n" + error.StackTrace, "log.log", Log.Log.Fatal);
				}
				finally
				{
					_to_ = false;
					if (stream != null)
						stream.Dispose();
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
		/// <param name="offset">стартовая позиция</param>
		/// <param name="length">количество которое необходимо записать</param>
		public void Message(byte[] message, int offset, int length)
		{
			lock (__ObSync)
			{
				if (Cancel)
					throw new HTTPException("Отправка данных закончена");
				try
				{
					SetResponse();
					switch (Response.ContentEncoding)
					{
						case "gzip":
						__Encode.Write(message, offset, length);
						break;
						case "deflate":
						__Encode.Write(message, offset, length);
						break;
						default:
						__Writer.Write(message, offset, length);
						break;
					}

					Log.Loging.AddMessage(
						"Http данные успешно добавлены к отправке", "log.log", Log.Log.Info);
				}
				catch (Exception error)
				{
					Protocol.Close();
					Log.Loging.AddMessage(
						"Ошибка при записи ответа на запрос HTTP" +
						error.Message + "./r/n" + error.StackTrace, "log.log", Log.Log.Fatal);

				}
			}
		}

		private void SetResponse()
		{
			if ( !Response.IsRes )
			{
				if (string.IsNullOrEmpty(Response.StrStr))
					Response.StrStr  =  "HTTP/1.1 200 OK";

				if (Response.Connection == "close")
					Response.SetClose();
				if (Response.ContentType == null
					 || Response.ContentType.Count == 0)
					Response.ContentType = 
						new List<string>
						{
							"text/plain",
							"charset=utf-8"
						};
				if (Response.CacheControl == null
					 || Response.CacheControl.Count == 0)
					Response.CacheControl = 
						new List<string>
						{
							"no-store",
							"no-cache",
						};
				if (Response.ContentEncoding == "gzip")
					__Encode = new GZipStream(__Writer, CompressionLevel.Fastest, true);
				else if (Response.ContentEncoding == "deflate")
					__Encode = new DeflateStream(__Writer, CompressionLevel.Fastest, true);

				Log.Loging.AddMessage(
						"Http заголовки успешно обработаны: \r\n" +
						"Заголовки зап:\r\n" + Response.ToString(), "log.log", Log.Log.Info);
			}
		}
		private void HandlerHead()
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
			if (__Reader.ReadHead())
			{
				switch (Request.Method)
				{
					case "GET":
						if (__Reader.__Frame.bleng > 0)
						{
							throw new HTTPException(
												"Неверная длина запроса GET", HTTPCode._400_);
						}
						break;
						case "POST":
						if (__Reader.__Frame.Handl == 0)
						{
							throw new HTTPException(
												"Неверная длина запроса POST", HTTPCode._400_);
						}
						break;
					default:
							throw new HTTPException(
												"Метод не поддерживается " +
														Request.Method + ".", HTTPCode._501_);
				}
				if (!string.IsNullOrEmpty(Request.Upgrade))
				{
						Protocol.TaskResult.Jump = true;
						if (Request.Upgrade.ToLower() == "websocket")
						{

						}
						else
							throw new HTTPException(
												"Протокол не поддерживается " +
														Request.Upgrade + ".", HTTPCode._400_);
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

								Protocol.OnEventOpen(this);
				}
			}
		}
		private void HandlerBody()
		{
			/*--------------------------------------------------------------------------------------------------------

			   Обрабатываем тело запроса. Если тело не было получено, читаем данные из кольцевого потока данных.
			   При заголвоке Transfer-Encoding тело будет приходяить по частям и будет полность получено после 0
			   данных и насупить событие EventData, до этого момента будет насупать событиеEventChunk, если был 
			   указан заголвок Content-Length событие EventChunk происходить не будет, а событие EventData 
			   произойдет только тогда когда все данные будут получены. Максимальный размер блока данных chuncked
			   можно указать задав значение HTTPReader.LENCHUNK. В случае с Content-Length можно обработать 
			   заголвок в соыбтие EventOpen и при привышении допустимого значения закрыть соединение.

		   --------------------------------------------------------------------------------------------------------*/
			if (__Reader.ReadBody())
			{
				switch (__Reader.__Frame.Pcod)
				{
					case HTTPFrame.DATA:
						if (!Protocol.TaskResult.Jump)
						{
							Protocol.NewContext(this);
							Protocol.OnEventData(this);

							Log.Loging.AddMessage(
								"Все данные Http запроса получены", "log.log", Log.Log.Info);
						}
						else
							Protocol.TaskResult.Option = TaskOption.Protocol;
						break;
					case HTTPFrame.CHUNK:
							Protocol.OnEventChunk(this);

							Log.Loging.AddMessage(
								"Часть данных Http запроса получена", "log.log", Log.Log.Info);
						break;
				}
			}
		}
		/// <summary>
		/// Обрабатывает ошибки и вызывает события класса HTTP
		/// </summary>
		/// <param name="_1_error">Ошибка протокола HTTP</param>
		async
		private void HandlerError(HTTPException _1_error)
		{
			if (!Request.IsReq)
				Protocol.NewContext(this);

			if (_1_Error != null)
			{
					Response.ClearHeaders();
					Protocol.Error(_1_Error  =  _1_error);
				switch (_1_error.Status.value)
				{
					case 400:
							Response.StrStr = "HTTP/1.1 400 " + _1_error.Status.ToString();
							if (await AsMssg(Encoding.UTF8.GetBytes(
								"400"
							)))
								End();
						break;
					case 404:
							Response.StrStr = "HTTP/1.1 404 " + _1_error.Status.ToString();
							if (await AsMssg(Encoding.UTF8.GetBytes(
								"404"
							)))
								End();
						break;
					//case 501:
					//		Response.StrStr = "HTTP/1.1 501 " + _1_error.Status.ToString();
					//		if (await AsMssg(Encoding.UTF8.GetBytes(
					//			"501"
					//		)))
					//			End();
					//	break;
					default:
							_1_error.Status  =  HTTPCode._500_;
						break;
				}
			}
			else
			{
					Response.ClearHeaders();
					Protocol.Error(_1_Error  =  _1_error);
				switch (_1_error.Status.value)
				{
					case 400:
							Response.StrStr = "HTTP/1.1 400 " + _1_error.Status.ToString();
							if (await AsFile("Html/400.html"))
								End();
						break;
					case 404:
							Response.StrStr = "HTTP/1.1 404 " + _1_error.Status.ToString();
							if (await AsFile("Html/404.html"))
								End();
						break;
					case 501:
							Response.StrStr = "HTTP/1.1 501 " + _1_error.Status.ToString();
							if (await AsFile("Html/501.html"))
								End();
						break;
				}
			}	
		}
	}
}
