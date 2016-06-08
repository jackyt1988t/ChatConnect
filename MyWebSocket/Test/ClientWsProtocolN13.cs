using System;
using System.Text;
	using System.Net;
	using System.Net.Sockets;
using System.Security.Cryptography;
			using System.Threading;
		

using MyWebSocket.Tcp;
using MyWebSocket.Tcp.Protocol;
using MyWebSocket.Tcp.Protocol.WS;
using MyWebSocket.Tcp.Protocol.HTTP;



namespace MyWebSocket.Test
{
	class ProtocolN13 : WS
	{
		static string CHECKKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
		bool Rchunk;
		public IPEndPoint Point
		{
			get;
		}
		WStreamN13 _Reader;
		public override StreamS Reader
		{
			get
			{
				return(StreamS)_Reader;
			}
		}
		bool Wchunk;
		WStreamN13 _Writer;
		public override StreamS Writer
		{
			get
			{
				return(StreamS)_Writer;
			}
		}
				

		public ProtocolN13(string adress, int port) :
			base()
		{
			_Reader = new WStreamN13(32000);
			_Writer = new WStreamN13(32000);

			Request.StartString = "GET /chat/websocket HTTP/1.1\r\n";
			Request.Add("Upgrade", "WebSocket");
			Request.Add("Connection", "Upgrade");
			Request.Add("Sec-WebSocket-Key", "");
			Request.Add("Sec-WebSocket-Protocol", "13");

			Point = new IPEndPoint(IPAddress.Parse(adress), port);
		}
		public void Connection()
		{
			Request["Sec-WebSocket-Key"] = "";
			for (int i = 0; i < 24; i++)
			{
				Request["Sec-WebSocket-Key"] += (char)new Random().Next(0x30, 0x79);
			}

			HTTPStream _reader = new HTTPStream(32000)
			{
				header = Request
			};
			HTTPStream _writer = new HTTPStream(32000)
			{
				header = Response
			};
			Tcp = new Socket(Point.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			Tcp.Connect(Point);
			
			SocketError error;
			byte[] buffer = Request.ToByte();
			if ((error = Write(buffer, 0, buffer.Length)) != SocketError.Success)
			{
				if (error != SocketError.WouldBlock
					   && error != SocketError.NoBufferSpaceAvailable)
				{
						Close(WSClose.Abnormal);
						OnEventClose (___Close);
						return;
				}
			}
			while (true)
			{
				if (!_writer.Empty)
				{
					if ((error = Send()) != SocketError.Success)
					{
						if (error != SocketError.WouldBlock
							   && error != SocketError.NoBufferSpaceAvailable)
						{
							Close(WSClose.Abnormal);
							OnEventClose(___Close);
							return;
						}
					}

				}
				else
				{
					if ((error = Read()) != SocketError.Success)
					{
						if (error != SocketError.WouldBlock
							   && error != SocketError.NoBufferSpaceAvailable)
						{
							Close(WSClose.Abnormal);
						}
					}
					if (!_reader.Empty)
					{
						if (_reader.ReadHead() == 1)
						{
							if (!Request.ContainsKey("sec-websocket-accept"))
							{
								Close(WSClose.TLSHandshake);
									 OnEventClose(___Close);
								return;
							}

							SHA1 sha1 = SHA1.Create();
							string checkkey = Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(Request["sec-websocket-key"] + CHECKKEY)));
							sha1.Clear();
							if ( Request["sec-websocket-accept"] != checkkey )
							{
								Close(WSClose.TLSHandshake);
									 OnEventClose(___Close);
								return;
							}
							break;
						}											
					}
				}
				Thread.Sleep(1);
			}
			

			if (!_reader.Empty)
			{
				int start = (int)_reader.PointR;
				int length = (int)_reader.Length;
				Reader.Write(_reader.Buffer, start, length);
			}

			_reader.Dispose();
			_writer.Dispose();

			while (true)
			{
				TaskResult TaskResult = TaskLoopHandlerProtocol();
					if (TaskResult.Option  ==  TaskOption.Delete)
						break;
				Thread.Sleep(5);
			}
		}
		public bool Message(byte[] message, int fin, int rcv1, int rcv2, int rcv3, int mask, int opcod)
		{
			lock (Writer)
			{
				/*      Очитстить.      */
				_Writer.Frame.Null();

				_Writer.Frame.BitFin   = fin;
				_Writer.Frame.BitRsv1  = rcv1;
				_Writer.Frame.BitRsv2  = rcv2;
				_Writer.Frame.BitRsv3  = rcv3;
				_Writer.Frame.BitMask  = mask;
				_Writer.Frame.BitPcod  = opcod;
				_Writer.Frame.PartBody = 0;
				_Writer.Frame.DataBody = message;
				_Writer.Frame.LengBody = message.Length;
				_Writer.Frame.InitializationHeader();
				if (Debug)
					WSDebug.DebugN13(_Writer.Frame);
				if (!Message(_Writer.Frame.DataHead, 0, (int)_Writer.Frame.LengHead))
					return false;
				else
					return Message(_Writer.Frame.DataBody, (int)_Writer.Frame.PartBody, (int)_Writer.Frame.LengBody);
			}
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
		public override bool Message( byte[] message, int recive, int length, WSOpcod opcod, WSFin fin )
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
				_Writer.Frame.Null();
				
				_Writer.Frame.BitFin   = Fin;
				_Writer.Frame.BitMask  = 1;
				_Writer.Frame.BitPcod  = Opcod;
				_Writer.Frame.PartBody = recive;
				_Writer.Frame.LengBody = length;
				_Writer.Frame.DataBody = message;
				_Writer.Frame.InitializationHeader();
				if (Debug)
					WSDebug.DebugN13( _Writer.Frame );
				if (!Message(_Writer.Frame.DataHead, 0, (int)_Writer.Frame.LengHead))
					return false;
				else
					return Message(_Writer.Frame.DataBody, (int)_Writer.Frame.PartBody, (int)_Writer.Frame.LengBody);
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
			if (_Reader.Empty)
				return;
			
			if (_Reader.Frame.GetsHead 
			 && _Reader.Frame.GetsBody)
				_Reader.Frame.Null();

			if (!_Reader.Frame.GetsHead)
			{
				if (_Reader.ReadHead() > 0)
				{
					if (_Reader.Frame.BitRsv1 == 1)
						throw new WSException("Неверный бит rcv1", WsError.HeaderFrameError, WSClose.PolicyViolation);
					if (_Reader.Frame.BitRsv2 == 1)
						throw new WSException("Неверный бит rcv2", WsError.HeaderFrameError, WSClose.PolicyViolation);
					if (_Reader.Frame.BitRsv3 == 1)
						throw new WSException("Неверный бит rcv3", WsError.HeaderFrameError, WSClose.PolicyViolation);
					if (_Reader.Frame.BitMask == 1)
						throw new WSException("Неверный бит mask", WsError.HeaderFrameError, WSClose.PolicyViolation);
					if (_Reader.Frame.LengBody < 0 || _Reader.Frame.LengBody > 32000)
					{
						string length = _Reader.Frame.LengBody.ToString("X");
						throw new WSException("Длинна: " + length, WsError.HeaderFrameError, WSClose.PolicyViolation);
					}
				}
			}
			if (!_Reader.Frame.GetsBody)
			{
				if (_Reader.ReadBody() == -1)
					return;

				if (Debug)
					WSDebug.DebugN13(_Reader.Frame);
				switch (_Reader.Frame.BitPcod)
				{
					
					case WSFrameN13.TEXT:
						if (Rchunk)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (_Reader.Frame.BitFin == 1)
							OnEventData(new WSData(_Reader.Frame.DataBody, WSOpcod.Text, WSFin.Last));
						else
						{
							Rchunk = true;
							OnEventChunk(new WSData(_Reader.Frame.DataBody, WSOpcod.Text, WSFin.Next));
						}
						break;
					case WSFrameN13.PING:
						if (_Reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);

							OnEventPing(new WSData(_Reader.Frame.DataBody, WSOpcod.Ping, WSFin.Last));
						break;
					case WSFrameN13.PONG:
						if (_Reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (PingControl.SetPing.ToString() != Encoding.UTF8.GetString(_Reader.Frame.DataBody))
							throw new WSException("Неверный бит fin.", WsError.PongBodyIncorrect,WSClose.PolicyViolation);
							PingControl.GetPong = new TimeSpan( DateTime.Now.Ticks );

							OnEventPong(new WSData(_Reader.Frame.DataBody, WSOpcod.Pong, WSFin.Last));
						break;
					case WSFrameN13.CLOSE:
						if (_Reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						
						string message;
						WSClose __close;
					
						if (_Reader.Frame.LengBody > 1)
						{
							int number = _Reader.Frame.DataBody[0] << 8;
								number = _Reader.Frame.DataBody[1] | number;
							
							if (number >= 1000 && number <= 1012)
								__close = (WSClose)number;
							else
								__close = (WSClose.Abnormal);
						}
							else
								__close = (WSClose.Abnormal);
						
						if (_Reader.Frame.LengBody < 3)
							message = string.Empty;
						else
							message = Encoding.UTF8.GetString(_Reader.Frame.DataBody, 2, (int)(_Reader.Frame.LengBody - 2));
							CloseServer(__close, message, false);
						break;
					case WSFrameN13.BINNARY:
						if (Rchunk)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (_Reader.Frame.BitFin == 1)
							OnEventData(new WSData(_Reader.Frame.DataBody, WSOpcod.Binnary, WSFin.Last));
						else
						{
							Rchunk = true;
							OnEventChunk(new WSData(_Reader.Frame.DataBody, WSOpcod.Binnary, WSFin.Next));
						}
						break;
					case WSFrameN13.CONTINUE:
						if (!Rchunk)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (_Reader.Frame.BitFin == 1)
						{
							Rchunk = false;
							OnEventData(new WSData(_Reader.Frame.DataBody, WSOpcod.Continue, WSFin.Last));
						}
						else
							OnEventChunk(new WSData(_Reader.Frame.DataBody, WSOpcod.Continue, WSFin.Next));
						break;
					default:
						throw new WSException("Опкод не поддерживается " + 
												    _Reader.Frame.BitPcod, WsError.PcodNotSuported, WSClose.UnsupportedData);
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
			OnEventConnect(request, response);
		}
	}
}
