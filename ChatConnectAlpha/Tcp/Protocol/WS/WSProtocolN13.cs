﻿using System;
using System.Net;
using System.Net.Sockets;
		using System.Text;
		using System.Security.Cryptography;


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
			reader     = new WStreamN13(SizeRead);
			writer     = new WStreamN13(SizeWrite);
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
			int Fin = 1;
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
					Fin = 0;
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
				if (!Send(writer.Frame.DataHead, 0, (int)writer.Frame.LengHead))
					return false;
				else
					return Send(writer.Frame.DataBody, (int)writer.Frame.PartBody, (int)writer.Frame.LengBody);
			}
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
						if (reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);

						OnEventData(new WSData(reader.Frame.DataBody, WSOpcod.Text, WSFin.Last));
						break;
					case WSFrameN13.PING:
						if (reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);

						OnEventPing(new WSData(reader.Frame.DataBody, WSOpcod.Ping, WSFin.Last));
						break;
					case WSFrameN13.PONG:
						if (reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);

						OnEventPong(new WSData(reader.Frame.DataBody, WSOpcod.Pong, WSFin.Last));
						break;
					case WSFrameN13.CLOSE:
						if (reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);

						if (reader.Frame.DataBody.Length > 1)
						{
							int number;
							number = reader.Frame.DataBody[0] << 8;
							number = reader.Frame.DataBody[1] | number;
							Console.WriteLine(Encoding.UTF8.GetString(reader.Frame.DataBody));
							if ( number  >=  1000  &&  number  <= 1012 )
								сlose(WSClose.Normal);
							else
								сlose(WSClose.Abnormal);
						}
							else
								сlose(WSClose.Abnormal);
						return;
					case WSFrameN13.BINNARY:
						if (reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						OnEventData(new WSData(reader.Frame.DataBody, WSOpcod.Binnary, WSFin.Last));
						break;
					case WSFrameN13.CONTINUE:
						if (reader.Frame.BitFin == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						OnEventChunk(new WSData(reader.Frame.DataBody, WSOpcod.Continue, WSFin.Next));
						break;
					default:
						throw new WSException("Опкод: " + reader.Frame.BitPcod, WsError.PcodNotSuported, WSClose.UnsupportedData);
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
			string key = Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(Request["sec-websocket-key"] + CHECKKEY)));
			sha1.Clear();

			Response.StartString = "HTTP/1.1 101 Switching Protocols";
			Response.Add("Upgrade", "WebSocket");
			Response.Add("Connection", "Upgrade");
			Response.Add("Sec-WebSocket-Accept", key);

			OnEventConnect(request, response);
			byte[] buffer = response.ToByte();
			Send(  buffer, 0, buffer.Length  );
			if (Request.SegmentsBuffer.Count > 0)
			{
				byte[] buff = Request.SegmentsBuffer.Dequeue();
				reader.Write (     buff, 0, buff.Length      );
			}
		}
	}

}
