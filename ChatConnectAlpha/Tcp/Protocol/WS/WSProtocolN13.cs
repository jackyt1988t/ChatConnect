using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace ChatConnect.Tcp.Protocol.WS
{
	class WSProtocolN13 : WS
	{
		const string CHECKKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

		WStreamN13 reader;
		public override WStream Reader
		{
			get
			{
				return reader;
			}
		}
		WStreamN13 writer;
		public override WStream Writer
		{
			get
			{
				return writer;
			}
		}

		/// <summary>
		/// Ининцилазириует класс протокола WS без подключения
		/// </summary>
		public WSProtocolN13()
		{
			Sync = new object();
			State = 
				States.Connection;
			reader     = new WStreamN13(1024 * 24);
			writer     = new WStreamN13(1204 * 128);
			Response   = new Header();
			TaskResult = new TaskResult();
			TaskResult.Protocol = TaskProtocol.WSRFC76;
		}
		/// <summary>
		/// Инициализрует класс протокола WS с указанным обработчиком
		/// </summary>
		/// <param name="http">протокол  http</param>
		/// <param name="connect">обрабтчик собятия подключения</param>
		public WSProtocolN13(IProtocol http) :
			this()
		{
			Tcp = http.Tcp;
			Request = http.Request;
			Session = new WSEssion(((IPEndPoint)Tcp.RemoteEndPoint).Address);
		}
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
			WSFrameN13 frame = new WSFrameN13()
			{
				BitFin   = Fin,
				BitPcod  = Opcod,
				PartBody = recive,
				LengBody = length,
				DataBody = message
			};
			frame.InitializationHeaders();
			lock (Writer)
			{
				if (Writer.Clear > ( frame.DataHead.Length
								   + frame.DataBody.Length ))
				{
					Writer.Write(frame.DataHead, 0, (int)frame.LengHead);
					Writer.Write(frame.DataBody, (int)frame.PartBody, 
												    (int)frame.LengBody);
					return true;
				}
			}
			return false;
		}
		protected override void Work()
		{
			OnEventWork();
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
					if ( reader.Frame.BitMask == 0 )
						throw new WSException("Неверный бит mask", WsError.HeaderFrameError, WSClose.PolicyViolation);
					if ( reader.Frame.LengBody > 32000 )
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

				switch (reader.Frame.BitPcod)
				{
					case WSFrameN13.TEXT:
						if (reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						OnEventData(new WSBinnary(reader.Frame.DataBody, WSOpcod.Text));
						break;
					case WSFrameN13.PING:
						if (reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						OnEventPing(new WSBinnary(reader.Frame.DataBody, WSOpcod.Ping));
						break;
					case WSFrameN13.PONG:
						if (reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						OnEventPong(new WSBinnary(reader.Frame.DataBody, WSOpcod.Pong));
						break;
					case WSFrameN13.CLOSE:
						if (reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						State = States.Close;
						if (reader.Frame.DataBody.Length > 1)
						{
							int number;
							number = reader.Frame.DataBody[0] << 8;
							number = reader.Frame.DataBody[1] | number;

							if (number  >=  1000  &&  number  <=  1012)
								close = new CloseWS(Session.Address.ToString(),(WSClose)number);
							else
								close = new CloseWS(Session.Address.ToString(), WSClose.Abnormal);
						}
							else
							{
								close = new CloseWS(Session.Address.ToString(), WSClose.Abnormal);
							}
						return;
					case WSFrameN13.BINNARY:
						if (reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						OnEventData(new WSBinnary(reader.Frame.DataBody, WSOpcod.Binnary));
						break;
					case WSFrameN13.CONTINUE:
						if (reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						OnEventChunk(new WSBinnary(reader.Frame.DataBody, WSOpcod.Continue));
						break;
					default:
						throw new WSException("Опкод: " + reader.Frame.BitPcod, WsError.PcodNotSuported, WSClose.UnsupportedData);
				}
				if (Debug)
					WSDebug.DebugN13(reader.Frame);
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

			Response.StartString =
							  "HTTP/1.1 101 Switching Protocols";
			string key = Request["sec-websocket-key"] + CHECKKEY;
			byte[] val = Encoding.UTF8.GetBytes(key);
				   val = sha1.ComputeHash(val);
				   key = Convert.ToBase64String(val);

			sha1.Clear();

			Response.Add("Upgrade", "WebSocket");
			Response.Add("Connection", "Upgrade");
			Response.Add("Sec-WebSocket-Accept", key);

			OnEventConnect(request, response);
			Send(response.ToByte());
			if (Request.SegmentsBuffer.Count > 0)
			{
				byte[] buff = Request.SegmentsBuffer.Dequeue();
				reader.Write (     buff, 0, buff.Length      );
			}
		}
	}

}
