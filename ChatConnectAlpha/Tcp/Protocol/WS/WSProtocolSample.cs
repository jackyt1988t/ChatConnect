using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ChatConnect.Tcp.Protocol.WS
{
    class WSProtocol : WS
    {
		WStreamSample reader;
		public override WStream Reader
		{
			get;
		}
		WStreamSample writer;
		public override WStream Writer
		{
			get;
		}
		
		/// <summary>
		/// Ининцилазириует класс протокола WS без подключения
		/// </summary>
		public WSProtocol()
		{
			Sync       = new object();
			State      = 
					States.Connection;
			reader	   = new WStreamSample(1204 * 512);
			writer	   = new WStreamSample(1204 * 512);
			Response   = new Header();
			TaskResult = new TaskResult();
			TaskResult.Protocol   =   TaskProtocol.WSRFC76;
		}
		/// <summary>
		/// Инициализрует класс протокола WS с указанным обработчиком
		/// </summary>
		/// <param name="http">протокол  http</param>
		/// <param name="connect">обрабтчик собятия подключения</param>
		public WSProtocol(IProtocol http, PHandlerEvent connect)
        {
			Tcp        = http.Tcp;
			Sync       = new object();
			State      = 
					States.Connection;
			Reader     = new WStreamSample(1204 * 512);
			Writer     = new WStreamSample(1204 * 512);
			Request    = http.Request;
			Response   = new Header();
			TaskResult = new TaskResult();
			TaskResult.Protocol   =   TaskProtocol.WSRFC76;
		}
		public override bool Message(byte[] message, int recive, int length, WSOpcod opcod, WSFin fin)
		{
			int Fin = 0;
			int Opcod = 0;
			if (fin == WSFin.Last)
				Fin = 0;
			else if (fin == WSFin.Next)
				Fin = 1;
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

				writer.Frame.BitMore  = Fin;
				writer.Frame.BitPcod  = Opcod;
				writer.Frame.PartBody = recive;
				writer.Frame.LengBody = length;
				writer.Frame.DataBody = message;
				writer.Frame.InitializationHeader();
				if (Debug)
					WSDebug.DebugSample(writer.Frame);
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
				if (reader.ReadHead() == -1)
					return;
				if (reader.Frame.BitRsv1 == 1)
					throw new WSException("Неверный бит rcv1", WsError.HeaderFrameError, WSClose.PolicyViolation);
				if (reader.Frame.BitRsv2 == 1)
					throw new WSException("Неверный бит rcv2", WsError.HeaderFrameError, WSClose.PolicyViolation);
				if (reader.Frame.BitRsv3 == 1)
					throw new WSException("Неверный бит rcv3", WsError.HeaderFrameError, WSClose.PolicyViolation);
				if (reader.Frame.BitRsv4 == 1)
					throw new WSException("Неверный бит rcv4", WsError.HeaderFrameError, WSClose.PolicyViolation);
				if (reader.Frame.LengBody > 32000 || reader.Frame.LengBody == 0)
				{
					string length = reader.Frame.LengBody.ToString("X");
					throw new WSException("Длинна: " + length, WsError.HeaderFrameError, WSClose.PolicyViolation);
				}
			}
			if (!reader.Frame.GetsBody)
			{
				if (reader.ReadBody() == -1)
					return;

				if (Debug)
					WSDebug.DebugSample(reader.Frame);
				switch (reader.Frame.BitPcod)
				{

					case WSFrameN13.TEXT:
						if (reader.Frame.BitMore == 1)
							throw new WSException("Неверный бит more", WsError.HeaderFrameError, WSClose.PolicyViolation);

						OnEventData(new WSData(reader.Frame.DataBody, WSOpcod.Text, WSFin.Last));
						break;
					case WSFrameN13.PING:
						if (reader.Frame.BitMore == 1)
							throw new WSException("Неверный бит more", WsError.HeaderFrameError, WSClose.PolicyViolation);

						OnEventPing(new WSData(reader.Frame.DataBody, WSOpcod.Ping, WSFin.Last));
						break;
					case WSFrameN13.PONG:
						if (reader.Frame.BitMore == 1)
							throw new WSException("Неверный бит more", WsError.HeaderFrameError, WSClose.PolicyViolation);

						OnEventPong(new WSData(reader.Frame.DataBody, WSOpcod.Pong, WSFin.Last));
						break;
					case WSFrameN13.CLOSE:
						if (reader.Frame.BitMore == 1)
							throw new WSException("Неверный бит more", WsError.HeaderFrameError, WSClose.PolicyViolation);
						
						return;
					case WSFrameN13.BINNARY:
						if (reader.Frame.BitMore == 1)
							throw new WSException("Неверный бит more", WsError.HeaderFrameError, WSClose.PolicyViolation);
						OnEventData(new WSData(reader.Frame.DataBody, WSOpcod.Binnary, WSFin.Last));
						break;
					case WSFrameN13.CONTINUE:
						if (reader.Frame.BitMore == 1)
							OnEventChunk(new WSData(reader.Frame.DataBody, WSOpcod.Continue, WSFin.Next));
						else
							OnEventChunk(new WSData(reader.Frame.DataBody, WSOpcod.Continue, WSFin.Last));
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
			MD5 md5 = MD5.Create();

			string key1 = request["sec-websocket-key1"];
			long space_1 = Regex.Matches(key1, @" ").Count;
			string key2 = request["sec-websocket-key2"];
			long space_2 = Regex.Matches(key2, @" ").Count;

			Regex regex = new Regex(@"\D");
			long key1_64 = Convert.ToInt64(regex.Replace(key1, ""));
			long key2_64 = Convert.ToInt64(regex.Replace(key2, ""));
			
			byte[] keyb_byte = request.SegmentsBuffer.Dequeue();
			byte[] key1_byte = BitConverter.GetBytes((int)(key1_64 / space_1));
			byte[] key2_byte = BitConverter.GetBytes((int)(key2_64 / space_2));
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(key1_byte);
				Array.Reverse(key2_byte);
			}

			byte[] key_string = new byte[16];
			Array.Copy(key1_byte, 0, key_string, 0, 4);
			Array.Copy(key2_byte, 0, key_string, 4, 4);
			Array.Copy(keyb_byte, 0, key_string, 8, 8);

			request.StartString = "HTTP/1.1 101 Web Socket Protocol Handshake";

			request.Add("Upgrade", "WebSocket");
			request.Add("Connection", "Upgrade");
			request.Body = md5.ComputeHash(key_string);

			md5.Clear();
			OnEventConnect(request, response);
			byte[] buffer = response.ToByte();
			Send(  buffer, 0, buffer.Length  );
			Send(request.Body, 0, request.Body.Length);
			if (Request.SegmentsBuffer.Count > 0)
			{
				byte[] buff = Request.SegmentsBuffer.Dequeue();
				reader.Write(      buff, 0, buff.Length      );
			}
		}		
    }
	
}