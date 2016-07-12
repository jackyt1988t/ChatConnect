using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWebSocket.Tcp.Protocol.HTTP;

namespace MyWebSocket.Tcp.Protocol.WS.WS_13
{
	class WSContext_13 : IContext
	{
		internal bool _to_;
		/// <summary>
		/// Поток
		/// </summary>
		internal Stream __Encode;
		/// <summary>
		/// Протолкол HTTP
		/// </summary>
		internal WSProtocol Protocol;
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
		public WSN13 Request
		{
			get;
			private set;
		}
		public WSN13 Response
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
		public WSContext_13(WSProtocol protocol)
		{
			Request  = new WSN13();
			Protocol =    protocol;
			Response = new WSN13();
			__ObSync = new object();

			__Reader = new WSReaderN13(protocol.GetStream, Request);

			//__Writer = new WSWriterN13(protocol.GetStream);
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

				if (!Request.GetsBody)
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
					case WSN13.TEXT:
						//if (Rchunk)
							//throw new WSException("Неверный бит fin.", 
													//WsError.HeaderFrameError, 
														//WSClose.PolicyViolation);
						if (Request.BitFin == 1)
							Protocol.OnEventData(this);
						//else
						//{
							//Rchunk = true;
							//Protocol.OnEventChunk(this);
						//}
						break;
					case WSN13.PING:
						if (__Reader.__Frame.BitFin == 0)
							//throw new WSException("Неверный бит fin.", 
													//WsError.HeaderFrameError, 
														//WSClose.PolicyViolation);

							Protocol.OnEventPing(this);
							//Message(Request.DataBody, 0, (int)Request.LengBody, WSOpcod.Pong, WSFin.Last);
					break;
					case WSN13.PONG:
						//if (__Reader._Frame.BitFin == 0)
							//throw new WSException("Неверный бит fin.", 
													//WsError.HeaderFrameError, 
														//WSClose.PolicyViolation);

							Protocol.OnEventPong(this);
					break;
					case WSN13.CLOSE:
						//if (__Reader._Frame.BitFin == 0)
							//throw new WSException("Неверный бит fin.", 
													//WsError.HeaderFrameError, 
														//WSClose.PolicyViolation);

						HandlerClose();

						break;
					case WSN13.BINNARY:
						//if (Rchunk)
							//throw new WSException("Неверный бит fin.", 
													//WsError.HeaderFrameError, 
														//WSClose.PolicyViolation);
						if (__Reader.__Frame.BitFin == 1)
							Protocol.OnEventData(this);
						//else
						//{
							//Rchunk = true;
							//Protocol.OnEventChunk(this);
						//}
						break;
					case WSN13.CONTINUE:
						//if (!Rchunk)
							//throw new WSException("Неверный бит fin.", 
													//WsError.HeaderFrameError, 
														//WSClose.PolicyViolation);
						if (__Reader.__Frame.BitFin == 1)
						{
							//Rchunk = false;
							Protocol.OnEventData(this);
						}
						else
							Protocol.OnEventChunk(this);
						break;
					default:
							throw new WSException("Опкод не поддерживается " + Request.BitPcod, 
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

				if (number >= 1000   &&   number <= 1012)
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
