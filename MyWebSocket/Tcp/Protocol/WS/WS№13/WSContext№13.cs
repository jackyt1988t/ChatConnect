using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using MyWebSocket.Tcp.Protocol.HTTP;

namespace MyWebSocket.Tcp.Protocol.WS.WS_13
{
	class WSContext_13 : IContext
	{
		internal const string CHECKKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

		internal bool _to_;
		internal bool _ow_;
		internal bool _in_next;
		internal bool _out_next_;
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
		internal WSException _1_Error;

		/// <summary>
		/// Закончена обр-ка
		/// </summary>
		public bool Cancel
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
		public WSFrameN13 Request
		{
			get;
			private set;
		}
		public WSFrameN13 Response
		{
			get;
			private set;
		}
		/// <summary>
		/// Поток чтения
		/// </summary>
		public WSReaderN13 __Reader
		{
			get;
		}
		/// <summary>
		/// Поток записи
		/// </summary>
		public WSWriterN13 __Writer
		{
			get;
		}

		/// <summary>
		/// Создает контекст получения, отправки данных
		/// </summary>
		/// <param name="protocol">HTTP</param>
		public WSContext_13(HTTProtocol protocol)
		{
			Request = new WSFrameN13();
			Protocol = protocol;
			Response = new WSFrameN13();
			__ObSync = new object();

			__Reader = new WSReaderN13(protocol.GetStream, Request);

			//__Writer = new WSWriterN13(protocol.GetStream);
		}
		static
		public void Handshake(Header request, Header response)
		{
			using (SHA1 crypt = SHA1.Create())
			{
				byte[] strkey = Encoding.UTF8.GetBytes(request["sec-websocket-key"] + CHECKKEY);
							string numhex = Convert.ToBase64String(crypt.ComputeHash(strkey));

				response.StrStr = "HTTP/1.1 101 Switching Protocols";
				response.AddHeader("Upgrade", "WebSocket");
				response.AddHeader("Connection", "Upgrade");
				response.AddHeader("Sec-WebSocket-Accept", numhex);
			}
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
			return new WSContext_13(Protocol);
		}
		/// <summary>
		/// 
		/// </summary>
		public void Handler()
		{
			try
			{
				if (!Request.GetHead)
				{
					HandlerHead();
				}

				if (!Request.GetBody)
				{
					HandlerBody();
				}
			}

			catch (WSException error)
			{
				HandlerError(error);

				Log.Loging.AddMessage("Ошибка об-тки HTTP запроса " +
									  "Ошибка: " + error.Message, "log.log", Log.Log.Info);
			}
			catch (IOException error)
			{
				Log.Loging.AddMessage("Ошибка об-тки HTTP запроса " +
									  "Ошибка: " + error.Message, "log.log", Log.Log.Info);

				HandlerError(new WSException("Ошибка получения http данных " +
											 "Ошибка: " + error.Message,
													WsError.CriticalError,
															WSClose.Abnormal));
			}
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
			Message(   message, 0, message.Length   );
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
			WSFrameN13 frame = new WSFrameN13();

			frame.BitFin   = 1;
			frame.BitPcod  = WSFrameN13.TEXT;
			frame.BitMask  = 0;
			frame.PartBody = offset;
			frame.LengBody = length;
			frame.DataBody = message;

				__Writer.Write(frame);
		}

		private void HandlerHead()
		{
			if (!__Reader.ReadHead())
			{
				if (Request.BitRsv1 == 1)
					throw new WSException("Неверный бит rcv1",
											WsError.HeaderFrameError,
												WSClose.PolicyViolation);
				if (Request.BitRsv2 == 1)
					throw new WSException("Неверный бит rcv2",
											WsError.HeaderFrameError,
												WSClose.PolicyViolation);
				if (Request.BitRsv3 == 1)
					throw new WSException("Неверный бит rcv3",
											WsError.HeaderFrameError,
												 WSClose.PolicyViolation);
				if (Request.BitMask == 0)
					throw new WSException("Неверный бит mask",
											WsError.HeaderFrameError,
												WSClose.PolicyViolation);
				if (Request.LengBody < 0)
				{
					string length = Request.LengBody.ToString("X");
					throw new WSException("Длинна: " + length,
											WsError.HeaderFrameError,
												WSClose.PolicyViolation);
				}
			}
		}
		private void HandlerBody()
		{
			if (!__Reader.ReadBody())
			{
				//if (Debug)
				//WSDebug.DebugN13(__Reader._Frame);
				switch (Request.BitPcod)
				{
					case WSFrameN13.TEXT:
					if (_out_next_)
						throw new WSException("Неверный бит fin.", 
												WsError.HeaderFrameError, 
													WSClose.PolicyViolation);
					if (Request.BitFin == 1)
						Protocol.OnEventData(this);
					else
					{
						_out_next_ = true;
						Protocol.OnEventChunk(this);
					}
					break;
					case WSFrameN13.PING:
					if (__Reader.__Frame.BitFin == 0)
						throw new WSException("Неверный бит fin.", 
											WsError.HeaderFrameError, 
												WSClose.PolicyViolation);

						Protocol.OnEventPing(this);
					//Message(Request.DataBody, 0, (int)Request.LengBody, WSOpcod.Pong, WSFin.Last);
					break;
					case WSFrameN13.PONG:
					if (__Reader.__Frame.BitFin == 0)
						throw new WSException("Неверный бит fin.", 
												WsError.HeaderFrameError, 
													WSClose.PolicyViolation);

					Protocol.OnEventPong(this);
					break;
					case WSFrameN13.CLOSE:
					if (__Reader.__Frame.BitFin == 0)
						throw new WSException("Неверный бит fin.", 
												WsError.HeaderFrameError, 
													WSClose.PolicyViolation);

					HandlerClose();

					break;
					case WSFrameN13.BINNARY:
					if (_out_next_)
						throw new WSException("Неверный бит fin.", 
												WsError.HeaderFrameError, 
													WSClose.PolicyViolation);

					if (__Reader.__Frame.BitFin == 1)
						Protocol.OnEventData(this);
					else
					{
						_out_next_ = true;
						Protocol.OnEventChunk(this);
					}
					break;
					case WSFrameN13.CONTINUE:
					if (!_out_next_)
						throw new WSException("Неверный бит fin.", 
												WsError.HeaderFrameError, 
													WSClose.PolicyViolation);
					if (__Reader.__Frame.BitFin == 1)
					{
						_out_next_ = false;
						Protocol.OnEventData(this);
					}
					else
						Protocol.OnEventChunk(this);
					break;
					default:
						throw new WSException("Опкод не поддерживается.",
												WsError.PcodNotSuported,
													WSClose.UnsupportedData);
				}
			}
		}
		protected void HandlerClose()
		{
			string message = string.Empty;
			WSClose __close = WSClose.Abnormal;

			if (Request.LengBody > 1)
			{
				int number = Request.DataBody[0] << 8;
				number = Request.DataBody[1] | number;

				if (number >= 1000 && number <= 1012)
					__close = (WSClose)number;
			}
			if (Request.LengBody > 2)
				message = Encoding.UTF8.GetString(Request.DataBody, 2, (int)(Request.LengBody - 2));
		}
		protected void HandlerError(WSException _1_error)
		{
		}
	}
}
