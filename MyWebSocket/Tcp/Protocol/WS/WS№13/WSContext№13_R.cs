using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using MyWebSocket.Tcp.Protocol.HTTP;
using System.Collections.Generic;

namespace MyWebSocket.Tcp.Protocol.WS.WS_13
{
	public class WSContext_13_R : IContext
	{
		internal const string KEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

		internal bool _to_;
		internal bool _ow_;
		internal bool _in_next;
		internal bool _out_next_;

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
			get
			{
				return false;
			}
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
		public WSReaderN13 __Reader
		{
			get;
		}
        public List<WSFrameN13> Request
        {
            get;
            private set;
        }

        /// <summary>
        /// Создает контекст получения, отправки данных
        /// </summary>
        /// <param name="protocol">HTTP</param>
        public WSContext_13_R(HTTProtocol protocol, bool ow)
        {
            _ow_ = ow;

            Request  = 
                 new List<WSFrameN13>();

            __ObSync =     new object();
            Protocol =         protocol;
            
            __Reader = new WSReaderN13(Protocol.GetStream);
        }

		static
		public void Handshake(Header request, Header response)
		{
			using (SHA1 crypt = SHA1.Create())
			{
				byte[] key = Encoding.UTF8.GetBytes(
					request["sec-websocket-key"] + KEY);
				string hex = Convert.ToBase64String(
									 crypt.ComputeHash(key));

				response.StrStr =
					"HTTP/1.1 101 Switching Protocols";
				response.AddHeader("Upgrade", "WebSocket");
				response.AddHeader("Connection", "Upgrade");
				response.AddHeader("Sec-WebSocket-Accept", hex);
			}
		}
		public IContext Refresh()
		{
            throw new NotImplementedException("Refresh");
		}
		/// <summary>
		/// Возвращает новый контекст
		/// </summary>
		/// <returns></returns>
		public IContext Context()
		{
            IContext cntx;
            lock (Protocol.AllContext)
                Protocol.AllContext.Enqueue(
                    cntx = new WSContext_13_W(Protocol, false));
            return cntx;
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
				if (!__Reader.__Frame.GetBody && __Reader.__Frame.GetHead)
				{
					HandlerBody();
				}
			}

			catch (WSException error)
			{
				HandlerError(error);

				Log.Loging.AddMessage("Ошибка об-тки WS запроса " +
									  "Ошибка: " + error.Message, "log.log", Log.Log.Info);
			}
			catch (IOException error)
			{
				Log.Loging.AddMessage("Ошибка об-тки WS запроса " +
									  "Ошибка: " + error.Message, "log.log", Log.Log.Debug);

				HandlerError(new WSException("Ошибка получения http данных " +
											 "Ошибка: " + error.Message,
													WsError.CriticalError,
															WSClose.Abnormal));
			}
			finally
			{
                if (__Reader.__Frame.GetHead && __Reader.__Frame.GetBody)
					Log.Loging.AddMessage("WS запрос обработан.", "log.log", Log.Log.Info);
			}
		}
		/// <summary>
        /// tНе поодерживается, объект использкется для чтения данных
		/// </summary>
		/// <param name="message"></param>
		public void Message(string message)
		{
            throw new NotImplementedException("Message");
		}
		/// <summary>
        /// Не поодерживается, объект использкется для чтения данных
		/// </summary>
		/// <param name="message"></param>
		public void Message(byte[] message)
		{
            throw new NotImplementedException("Message");
		}
		/// <summary>
		/// Не поодерживается, объект использкется для чтения данных
		/// </summary>
		/// <param name="message">массив данных</param>
		/// <param name="offset">стартовая позиция</param>
		/// <param name="length">количество которое необходимо записать</param>
		public void Message(byte[] message, int offset, int length)
		{
            throw new NotImplementedException("Message");
		}

		private void HandlerHead()
		{
			if (__Reader.ReadHead())
			{
                if (__Reader.__Frame.BitRsv1 == 1)
					throw new WSException("Неверный бит rcv1",
											WsError.HeaderFrameError,
												WSClose.PolicyViolation);
                if (__Reader.__Frame.BitRsv2 == 1)
					throw new WSException("Неверный бит rcv2",
											WsError.HeaderFrameError,
												WSClose.PolicyViolation);
                if (__Reader.__Frame.BitRsv3 == 1)
					throw new WSException("Неверный бит rcv3",
											WsError.HeaderFrameError,
												 WSClose.PolicyViolation);
                if (__Reader.__Frame.BitMask == 0)
					throw new WSException("Неверный бит mask",
											WsError.HeaderFrameError,
												WSClose.PolicyViolation);
                if (__Reader.__Frame.LengBody < 0)
				{
                    string length = __Reader.__Frame.LengBody.ToString("X");
					throw new WSException("Длинна: " + length,
											WsError.HeaderFrameError,
												WSClose.PolicyViolation);
				}
			}
		}
		private void HandlerBody()
		{
			if (__Reader.ReadBody())
			{
				//if (Debug)
				//WSDebug.DebugN13(__Reader._Frame);
                switch (__Reader.__Frame.BitPcod)
				{
					case WSFrameN13.TEXT:
					if (_in_next)
						throw new WSException("Неверный бит fin.", 
												WsError.HeaderFrameError, 
													WSClose.PolicyViolation);
                        if (__Reader.__Frame.BitFin == 1)
						Protocol.OnEventData(this);
					else
					{
						_in_next = true;
						Protocol.OnEventChunk(this);
					}
					break;
					case WSFrameN13.PING:
					if (__Reader.__Frame.BitFin == 0)
						throw new WSException("Неверный бит fin.", 
												WsError.HeaderFrameError, 
													WSClose.PolicyViolation);

						Protocol.NewContext(this);
						Protocol.OnEventPing(this);
					//Message(Request.DataBody, 0, (int)Request.LengBody, WSOpcod.Pong, WSFin.Last);
					break;
					case WSFrameN13.PONG:
					if (__Reader.__Frame.BitFin == 0)
						throw new WSException("Неверный бит fin.", 
												WsError.HeaderFrameError, 
													WSClose.PolicyViolation);

						Protocol.NewContext(this);
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
					if (_in_next)
						throw new WSException("Неверный бит fin.", 
												WsError.HeaderFrameError, 
													WSClose.PolicyViolation);

					if (__Reader.__Frame.BitFin == 1)
						Protocol.OnEventData(this);
					else
					{
						_in_next = true;
						Protocol.OnEventChunk(this);
					}
					break;
					case WSFrameN13.CONTINUE:
					if (!_in_next)
						throw new WSException("Неверный бит fin.", 
												WsError.HeaderFrameError, 
													WSClose.PolicyViolation);
					if (__Reader.__Frame.BitFin == 1)
					{
						Protocol.NewContext(this);
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

                Request.Add(__Reader.__Frame);
                            __Reader.__Frame = new WSFrameN13();

                if (Log.Loging.Mode  >  Log.Log.Info)
                    Log.Loging.AddMessage(
                        "WS фрейм успешно обработан", "log.log", Log.Log.Info);
                else
                    Log.Loging.AddMessage(
                        "WS фрейм успешно обработан" +
                        "\r\n" + WSDebug.DebugN13(__Reader.__Frame), "log.log", Log.Log.Info);
			}
		}
		protected void HandlerClose()
		{
			string message = string.Empty;
			WSClose __close = WSClose.Abnormal;

            if (__Reader.__Frame.LengBody < 2)
				Protocol.Close();
			else
			{
                int number = __Reader.__Frame.DataBody[0] << 8;
                number = __Reader.__Frame.DataBody[1] | number;

				if (number < 1000   ||  number > 1012)
					Protocol.Close();
				{
					__close = 
						(WSClose)number;
					Protocol.Close(true);
				}
			}
            if (__Reader.__Frame.LengBody > 2)
                message = Encoding.UTF8.GetString(
                                __Reader.__Frame.DataBody, 2, 
                                    (int)(__Reader.__Frame.LengBody - 2));
		}
		protected void HandlerError(WSException _1_error)
		{
					Protocol.Close(true);
		}
	}
}
