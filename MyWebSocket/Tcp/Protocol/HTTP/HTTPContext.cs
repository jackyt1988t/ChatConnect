﻿using System;

using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	public class HTTPContext
	{
		internal bool _to_;
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
		/// HTTP
		/// </summary>
		internal HTTProtocol Protocol;
		/// <summary>
		/// Последняя ошибка
		/// </summary>
		internal HTTPException _1_Error;

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

		/// <summary>
		/// Создает контекст получения, отправки данных
		/// </summary>
		/// <param name="http">HTTP</param>
		public HTTPContext(HTTProtocol http)
		{
			Request  = new Header();
			Protocol = http;
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
		internal void Hadler()
		{
			try
			{
				if (!__Reader._Frame.GetHead)
				{
					HandlerHead();
				}

				if (!__Reader._Frame.GetBody)
				{
					HandlerBody();
				}
			}
			catch (HTTPException error)
			{
					HandlerError(error);
			}
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
				
			}
			if (__Encode != null)
				__Encode.Dispose();
			// Отправить блок данных chunked 0CRLFCRLF
			if (Response.TransferEncoding == "chunked")
			{
				try
				{
					__Writer.Eof();
				}
				catch (IOException error)
				{
					Protocol.HTTPClose();
					Log.Loging.AddMessage(
						"Ошибка при записи ответа на запрос HTTP" +
						error.Message + "./r/n" + error.StackTrace, "log.log", Log.Log.Fatal);		
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

					}
				}
				catch (Exception error)
				{
					
					Protocol.HTTPClose();
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
		/// <param name="recive">стартовая позиция</param>
		/// <param name="length">количество которое необходимо записать</param>
		public void Message(byte[] message, int recive, int length)
		{
			lock (__ObSync)
			{
				try
				{
					SetResponse();
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
					Log.Loging.AddMessage(
						"Http данные успешно добавлены к отправке", "log.log", Log.Log.Info);
				}
				catch (Exception error)
				{
					Protocol.HTTPClose();
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
					__Encode = new GZipStream(
						__Writer, CompressionLevel.Fastest, true);
				else if (Response.ContentEncoding == "deflate")
					__Encode = new DeflateStream(
						__Writer, CompressionLevel.Fastest, true);
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
			if (!__Reader.ReadHead())
			{
				switch (Request.Method)
				{
					case "GET":
						if (__Reader._Frame.bleng > 0)
						{
							throw new HTTPException(
												"Неверная длина запроса GET", HTTPCode._400_);
						}
						break;
						case "POST":
						if (__Reader._Frame.Handl == 0)
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
						Protocol.Result.Jump = true;
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
				switch (__Reader._Frame.Pcod)
				{
					case HTTPFrame.DATA:
						if (!Protocol.Result.Jump)
						{
							Protocol.OnEventData(this);

							Log.Loging.AddMessage(
								"Все данные Http запроса получены", "log.log", Log.Log.Info);
						}
						else
							Protocol.Result.Option = TaskOption.Protocol;
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
			if (_1_Error != null)
			{
					Response.ClearHeaders();
					Protocol.HTTPError (_1_Error  =  _1_error);
				switch (_1_error.Status.value)
				{
					case 400:
							Response.StrStr = "HTTP/1.1 400 " + _1_error.Status.ToString();
							if (await AsMsg(Encoding.UTF8.GetBytes(
								"400"
							)))
								End();
						break;
					case 404:
							Response.StrStr = "HTTP/1.1 404 " + _1_error.Status.ToString();
							if (await AsMsg(Encoding.UTF8.GetBytes(
								"404"
							)))
								End();
						break;
					case 501:
							Response.StrStr = "HTTP/1.1 501 " + _1_error.Status.ToString();
							if (await AsMsg(Encoding.UTF8.GetBytes(
								"501"
							)))
								End();
						break;
					default:
							_1_error.Status  =  HTTPCode._500_;
						break;
				}
			}
			else
			{
					Response.ClearHeaders();
					Protocol.HTTPError (_1_Error  =  _1_error);
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
