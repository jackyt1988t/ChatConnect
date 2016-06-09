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
		
		 
		public WsPolicy Policy
		{
			get;
			private set;
		}
		protected bool Rchunk;
		protected WStreamN13 reader;
		public override StreamS Reader
		{
			get
			{
				return (StreamS)reader;
			}
		}
		protected bool Wchunk;
		protected WStreamN13 writer;
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
			Policy = 
				new WsPolicy();
			reader = 
				new WStreamN13(
						SizeRead);
			writer = 
				new WStreamN13( 
						SizeWrite);
			Request = new Header();
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
			Policy.SetPolicy(0, 1, 1, 1, 0, 32000);
			Request = http.Request;
			if (!http.Reader.Empty)
			{
				int start = (int)http.Reader.PointR;
				int length = (int)http.Reader.Length;
				Reader.Write(http.Reader.Buffer, start, length);
			}
			try
			{
				Session = new WSEssion(((IPEndPoint)Tcp.RemoteEndPoint).Address);
			}
			catch (SocketException exc)
			{
				ExcServer(new WSException("Ошибка сокета", exc.SocketErrorCode, WSClose.ServerError));
			}
		}
		static
		public void Set101(IHeader header)
		{
			header.StartString = "HTTP/1.1 101 Switching Protocols";
			header.AddHeader("Upgrade", "WebSocket");
			header.AddHeader("Connection", "Upgrade");
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
					byte[] _buffer = new byte[2 + message.Length];
						   _buffer[0] = (byte)((int)___Close._InitCode >> 08);
						   _buffer[1] = (byte)((int)___Close._InitCode >> 00);
				length = _buffer.Length;
					message.CopyTo(_buffer, 2);
							 message = _buffer;
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
				writer._Frame.Null();
				writer._Frame.BitFin   = Fin;
				writer._Frame.BitPcod  = Opcod;
				writer._Frame.BitMask  = Policy.Mask;
				writer._Frame.PartBody = recive;
				writer._Frame.LengBody = length;
				writer._Frame.DataBody = message;
				
				writer._Frame.InitializationHeader();
				if (Debug)
					WSDebug.DebugN13( writer._Frame );
				if (!Message(writer._Frame.DataHead, 0, (int)writer._Frame.LengHead))
					return false;
				else
					return Message(writer._Frame.DataBody, (int)writer._Frame.PartBody, (int)writer._Frame.LengBody);
			}
		}
		protected override void Work()
		{
			OnEventWork();
			
			if (!PingControl.IsPong && PingControl.GetPong.Ticks < DateTime.Now.Ticks)
				 throw new WSException( "Нет ответа Понг", WsError.PingNotResponse, WSClose.PolicyViolation);

			if (!PingControl.IsPing && PingControl.SetPing.Ticks < DateTime.Now.Ticks)
			{	
				 PingControl.SetPing  =  new TimeSpan(  DateTime.Now.Ticks  +  TimeSpan.TicksPerSecond * 5  );
			Ping(PingControl.SetPing.ToString());
			}			
		}

		protected override void Data()
		{
			if (Reader.Empty)
				return;
			
			if (reader._Frame.GetsHead 
			 && reader._Frame.GetsBody)
				reader._Frame.Null();

			if (!reader._Frame.GetsHead)
			{
				if (reader.ReadHead() == -1)
					return;
				if (reader._Frame.BitRsv1 == Policy.Bit2)
					throw new WSException("Неверный бит rcv1", WsError.HeaderFrameError, WSClose.PolicyViolation);
				if (reader._Frame.BitRsv2 == Policy.Bit3)
					throw new WSException("Неверный бит rcv2", WsError.HeaderFrameError, WSClose.PolicyViolation);
				if (reader._Frame.BitRsv3 == Policy.Bit4)
					throw new WSException("Неверный бит rcv3", WsError.HeaderFrameError, WSClose.PolicyViolation);
				if (reader._Frame.BitMask == Policy.Mask)
					throw new WSException("Неверный бит mask", WsError.HeaderFrameError, WSClose.PolicyViolation);
				if (reader._Frame.LengBody < 0 || reader._Frame.LengBody > Policy.MaxLeng)
				{
					string length = reader._Frame.LengBody.ToString("X");
					throw new WSException("Длинна: " + length, WsError.HeaderFrameError, WSClose.PolicyViolation);
				}
			}
			if (!reader._Frame.GetsBody)
			{
				if (reader.ReadBody() == -1)
					return;

				if (Debug)
					WSDebug.DebugN13(reader._Frame);
				switch (reader._Frame.BitPcod)
				{
					
					case WSFrameN13.TEXT:
						if (Rchunk)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (reader._Frame.BitFin == 1)
							OnEventData(new WSData(reader._Frame.DataBody, WSOpcod.Text, WSFin.Last));
						else
						{
							Rchunk = true;
							OnEventChunk(new WSData(reader._Frame.DataBody, WSOpcod.Text, WSFin.Next));
						}
						break;
					case WSFrameN13.PING:
						if (reader._Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);

							OnEventPing(new WSData(reader._Frame.DataBody, WSOpcod.Ping, WSFin.Last));
							Message(   reader._Frame.DataBody, 0, (int)reader._Frame.LengBody, WSOpcod.Pong, WSFin.Last   );
						break;
					case WSFrameN13.PONG:
						if (reader._Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (PingControl.SetPing.ToString() != Encoding.UTF8.GetString(reader._Frame.DataBody))
							throw new WSException("Неверный бит fin.", WsError.PongBodyIncorrect,WSClose.PolicyViolation);
							PingControl.GetPong = new TimeSpan( DateTime.Now.Ticks );

							OnEventPong(new WSData(reader._Frame.DataBody, WSOpcod.Pong, WSFin.Last));
						break;
					case WSFrameN13.CLOSE:
						if (reader._Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						
						string message;
						WSClose __close;
					
						if (reader._Frame.LengBody > 1)
						{
							int number = reader._Frame.DataBody[0] << 8;
								number = reader._Frame.DataBody[1] | number;
							
							if (number >= 1000 && number <= 1012)
								__close = (WSClose)number;
							else
								__close = (WSClose.Abnormal);
						}
							else
								__close = (WSClose.Abnormal);
						
						if (reader._Frame.LengBody < 3)
							message = string.Empty;
						else
							message = Encoding.UTF8.GetString(reader._Frame.DataBody, 2, (int)(reader._Frame.LengBody - 2));
							CloseServer(__close, message, false);
						break;
					case WSFrameN13.BINNARY:
						if (Rchunk)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (reader._Frame.BitFin == 1)
							OnEventData(new WSData(reader._Frame.DataBody, WSOpcod.Binnary, WSFin.Last));
						else
						{
							Rchunk = true;
							OnEventChunk(new WSData(reader._Frame.DataBody, WSOpcod.Binnary, WSFin.Next));
						}
						break;
					case WSFrameN13.CONTINUE:
						if (!Rchunk)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (reader._Frame.BitFin == 1)
						{
							Rchunk = false;
							OnEventData(new WSData(reader._Frame.DataBody, WSOpcod.Continue, WSFin.Last));
						}
						else
							OnEventChunk(new WSData(reader._Frame.DataBody, WSOpcod.Continue, WSFin.Next));
						break;
					default:
						throw new WSException("Опкод не поддерживается " + 
												     reader._Frame.BitPcod, WsError.PcodNotSuported, WSClose.UnsupportedData);
				}			
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
			string key;
			if (!request.ContainsKeys("sec-websocket-key", out key, true))
				throw new WSException("Отсутствует заголовок sec-webspcket-key", WsError.PcodNotSuported, WSClose.UnsupportedData);
			string hex = Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(key + CHECKKEY)));
			sha1.Clear();
				Response.AddHeader("Sec-WebSocket-Accept", hex);

			Set101(Response);
			OnEventConnect(request, response);
			byte[] buffer = response.ToByte();
			Message(buffer, 0, buffer.Length);
		}
	}

}
