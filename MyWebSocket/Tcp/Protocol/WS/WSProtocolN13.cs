using System;
using System.Net;
using System.Net.Sockets;
		using System.Text;
		using System.Security.Cryptography;


namespace MyWebSocket.Tcp.Protocol.WS
{
	class WSProtocolN13 : WS
	{
		private const int PING = 5;
		private const string CHECKKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"; 
			
		bool Rchunk;
		WStreamN13 reader;
		public override StreamS Reader
		{
			get
			{
				return (StreamS)reader;
			}
		}
		bool Wchunk;
		WStreamN13 writer;
		public override StreamS Writer
		{
			get
			{
				return (StreamS)writer;
			}
		}
		/// <summary>
		/// Ининцилазириует класс протокола WS без подключения
		/// </summary>
		public WSProtocolN13() :
			base()
		{
			reader = new WStreamN13(SizeRead);
			writer = new WStreamN13(SizeWrite);
			TaskResult.Protocol = TaskProtocol.WSN13;
		}
		/// <summary>
		/// Инициализрует класс протокола WS с указанным обработчиком
		/// </summary>
		/// <param name="http">протокол  http</param>
		public WSProtocolN13(IProtocol http) :
			this()
		{
			Tcp = http.Tcp;
			Request = http.Request;
			if (!http.Reader.Empty)
			{
				int start = (int)http.Reader.PointR;
				int length = (int)http.Reader.Length;
				Reader.Write(http.Reader.Buffer, start, length);
			}
			Session = new WSEssion(((IPEndPoint)Tcp.RemoteEndPoint).Address);
		}
		static
		public void Set101(IHeader header)
		{
			header.StartString = "HTTP/1.1 101 Switching Protocols";
			header.Add("Upgrade", "WebSocket");
			header.Add("Connection", "Upgrade");
		}
		/// <summary>
		/// Отправляет фрейм по протоколу N13 с указанными входными параметрами
		/// </summary>
		/// <param name="message">масси байт</param>
		/// <param name="recive">начальная позиция</param>
		/// <param name="length">количество отправляемых байт</param>
		/// <param name="opcod">опкод который необходимо отправить</param>
		/// <param name="fin">указывает проводить фрагментацию или нет</param>
		/// <returns></returns>
		public override bool Message(byte[] message, int recive, int length, WSOpcod opcod, WSFin fin)
		{
			int Fin = 0;
			switch (fin)
			{
				case WSFin.Next:
					Fin = 0;
					break;
				case WSFin.Last:
					Fin = 1;
					break;
			}
			int Opcod = 0;
			switch (opcod)
			{
				case WSOpcod.Text:
					Opcod = WSFrameN13.TEXT;
					break;
				case WSOpcod.Ping:
					Opcod = WSFrameN13.PING;
					break;
				case WSOpcod.Pong:
					Opcod = WSFrameN13.PONG;
					break;
				case WSOpcod.Close:
					Opcod = WSFrameN13.CLOSE;
					break;
				case WSOpcod.Binnary:
					Opcod = WSFrameN13.BINNARY;
					break;
				case WSOpcod.Continue:
					Opcod = WSFrameN13.CONTINUE;
					break;
			}
			
			lock (Writer)
			{
				/*      Очитстить.      */
				writer.Frame.ClearFrame();

				writer.Frame.BitFin   = Fin;
				writer.Frame.BitPcod  = Opcod;
				writer.Frame.PartBody = recive;
				writer.Frame.LengBody = length;
				writer.Frame.DataBody = message;
				writer.Frame.InitializationHeader();
				if (Debug)
					WSDebug.DebugN13( writer.Frame );
				if (!Message(writer.Frame.DataHead, 0, (int)writer.Frame.LengHead))
					return false;
				else
					return Message(writer.Frame.DataBody, (int)writer.Frame.PartBody, (int)writer.Frame.LengBody);
			}
		}
		protected override void Work()
		{
			OnEventWork();
			
			if (!PingControl.IsPong && PingControl.GetPong.Ticks < DateTime.Now.Ticks)
				 throw new WSException("Нет ответа Понг", WsError.PingNotResponse, WSClose.ServerError);

			if (!PingControl.IsPing && PingControl.SetPing.Ticks < DateTime.Now.Ticks)
			{	
				 PingControl.SetPing  =  new TimeSpan(DateTime.Now.Ticks + TimeSpan.TicksPerSecond * 5);
			Ping(PingControl.SetPing.ToString());
			}			
		}

		protected override void Data()
		{
			if (Reader.Empty)
				return;
			if (!reader.Frame.GetsHead)
			{
				if (reader.ReadHead() > 0)
				{
					if (reader.Frame.BitRsv1 == 1)
						throw new WSException("Неверный бит rcv1", WsError.HeaderFrameError, WSClose.PolicyViolation);
					if (reader.Frame.BitRsv2 == 1)
						throw new WSException("Неверный бит rcv2", WsError.HeaderFrameError, WSClose.PolicyViolation);
					if (reader.Frame.BitRsv3 == 1)
						throw new WSException("Неверный бит rcv3", WsError.HeaderFrameError, WSClose.PolicyViolation);
					if (reader.Frame.BitMask == 0)
						throw new WSException("Неверный бит mask", WsError.HeaderFrameError, WSClose.PolicyViolation);
					if (reader.Frame.LengBody > 32000 || reader.Frame.LengBody == 0)
					{
						string length = reader.Frame.LengBody.ToString("X");
						throw new WSException("Длинна: " + length, WsError.HeaderFrameError, WSClose.PolicyViolation);
					}
				}
			}
			if (!reader.Frame.GetsBody)
			{
				if (reader.ReadBody() == -1)
					return;

				if (Debug)
					WSDebug.DebugN13(reader.Frame);
				switch (reader.Frame.BitPcod)
				{
					
					case WSFrameN13.TEXT:
						if (Rchunk)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (reader.Frame.BitFin == 1)
							OnEventData(new WSData(reader.Frame.DataBody, WSOpcod.Text, WSFin.Last));
						else
						{
							Rchunk = true;
							OnEventChunk(new WSData(reader.Frame.DataBody, WSOpcod.Text, WSFin.Next));
						}
						break;
					case WSFrameN13.PING:
						if (reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);

							OnEventPing(new WSData(reader.Frame.DataBody, WSOpcod.Ping, WSFin.Last));
						break;
					case WSFrameN13.PONG:
						if (reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (PingControl.SetPing.ToString() != Encoding.UTF8.GetString(reader.Frame.DataBody))
							throw new WSException("Неверный бит fin.", WsError.PongBodyIncorrect,WSClose.PolicyViolation);
							PingControl.GetPong = new TimeSpan( DateTime.Now.Ticks );

							OnEventPong(new WSData(reader.Frame.DataBody, WSOpcod.Pong, WSFin.Last));
						break;
					case WSFrameN13.CLOSE:
						if (reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);

						if (reader.Frame.LengBody > 1)
						{
							int number;
							number = reader.Frame.DataBody[0] << 8;
							number = reader.Frame.DataBody[1] | number;
							if ( number  >=  1000  &&  number  <= 1012 )
								сlose((WSClose)number);
							else
								сlose( WSClose.Abnormal );
						}
							else
								сlose( WSClose.Abnormal );
						if (reader.Frame.LengBody > 2)
							close.CloseMsg = Encoding.UTF8.GetString(reader.Frame.DataBody, 2, 
																					( int )( reader.Frame.LengBody - 2 ));
						return;
					case WSFrameN13.BINNARY:
						if (Rchunk)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (reader.Frame.BitFin == 1)
							OnEventData(new WSData(reader.Frame.DataBody, WSOpcod.Binnary, WSFin.Last));
						else
						{
							Rchunk = true;
							OnEventChunk(new WSData(reader.Frame.DataBody, WSOpcod.Binnary, WSFin.Next));
						}
						break;
					case WSFrameN13.CONTINUE:
						if (!Rchunk)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (reader.Frame.BitFin == 1)
						{
							Rchunk = false;
							OnEventData(new WSData(reader.Frame.DataBody, WSOpcod.Continue, WSFin.Last));
						}
						else
							OnEventChunk(new WSData(reader.Frame.DataBody, WSOpcod.Continue, WSFin.Next));
						break;
					default:
						throw new WSException("Опкод не поддерживается " + 
												     reader.Frame.BitPcod, WsError.PcodNotSuported, WSClose.UnsupportedData);
				}
				/*      Очитстить.      */
				reader.Frame.ClearFrame();				
			}
		}
		protected override void Close(CloseWS close)
		{
			OnEventClose(close);
		}
		protected override void Error(WSException error)
		{
			OnEventError(error);
		}

		protected override void Connection(IHeader request, IHeader response)
		{
			SHA1 sha1 = SHA1.Create();
			if (!request.ContainsKey("sec-websocket-key"))
				throw new WSException("Отсутствует заголовок sec-webspcket-key", WsError.PcodNotSuported, WSClose.UnsupportedData);
			string key = Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(Request["sec-websocket-key"] + CHECKKEY)));
			sha1.Clear();
				Response.Add("Sec-WebSocket-Accept", key);

			Set101(Response);
			OnEventConnect(request, response);
			byte[] buffer = response.ToByte();
			Message(buffer, 0, buffer.Length);
		}
	}

}
