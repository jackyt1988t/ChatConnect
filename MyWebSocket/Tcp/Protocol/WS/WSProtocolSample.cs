using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MyWebSocket.Tcp.Protocol.WS
{
    class WSProtocol : WS
    {
		bool Rchunk;
		WStreamSample reader;
		public override StreamS Reader
		{
			get
			{
				return reader;
			}
		}
		bool Wchunk;
		WStreamSample writer;
		public override StreamS Writer
		{
			get
			{
				return writer;
			}
		}
		
		/// <summary>
		/// Ининцилазириует класс протокола WS без подключения
		/// </summary>
		public WSProtocol() :
			base()
		{
			reader = new WStreamSample(SizeRead);
			writer = new WStreamSample(SizeWrite);
			TaskResult.Protocol = TaskProtocol.WSN13;
		}
		/// <summary>
		/// Инициализрует класс протокола WS с указанным обработчиком
		/// </summary>
		/// <param name="http">протокол  http</param>
		/// <param name="connect">обрабтчик собятия подключения</param>
		public WSProtocol(IProtocol http, PHandlerEvent connect) :
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
			header.StartString = "HTTP/1.1 101 Web Socket Protocol Handshake";
			header.Add("Upgrade", "WebSocket");
			header.Add("Connection", "Upgrade");
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
				writer.Frame.Null();

				writer.Frame.BitMore  = Fin;
				writer.Frame.BitPcod  = Opcod;
				writer.Frame.PartBody = recive;
				writer.Frame.LengBody = length;
				writer.Frame.DataBody = message;
				writer.Frame.InitializationHeader();
				if (Debug)
					WSDebug.DebugSample(writer.Frame);
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
				 PingControl.SetPing = new TimeSpan(DateTime.Now.Ticks + TimeSpan.TicksPerSecond * 5);
			Ping(PingControl.SetPing.ToString());
			}
		}
		protected override void Data()
		{
			if (Reader.Empty)
				return;

			if (reader.Frame.GetsHead 
			 && reader.Frame.GetsBody)
				reader.Frame.Null();

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
				if (reader.Frame.LengBody < 0 || reader.Frame.LengBody > 32000)
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
						if (Rchunk)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (reader.Frame.BitMore == 0)
							OnEventData(new WSData(reader.Frame.DataBody, WSOpcod.Text, WSFin.Last));
						else
						{
							Rchunk = true;
							OnEventChunk(new WSData(reader.Frame.DataBody, WSOpcod.Text, WSFin.Next));
						}
						break;
					case WSFrameN13.PING:
						if (reader.Frame.BitMore == 1)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);

							OnEventPing(new WSData(reader.Frame.DataBody, WSOpcod.Ping, WSFin.Last));
						break;
					case WSFrameN13.PONG:
						if (reader.Frame.BitMore == 1)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (PingControl.SetPing.ToString() != Encoding.UTF8.GetString(reader.Frame.DataBody))
							throw new WSException("Неверный бит fin.", WsError.PongBodyIncorrect,WSClose.PolicyViolation);
							PingControl.GetPong = new TimeSpan( DateTime.Now.Ticks );

							OnEventPong(new WSData(reader.Frame.DataBody, WSOpcod.Pong, WSFin.Last));
						break;
					case WSFrameN13.CLOSE:
						if (reader.Frame.BitMore == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						
							CloseServer(WSClose.Normal, Encoding.UTF8.GetString(reader.Frame.DataBody), false);
						break;
					case WSFrameN13.BINNARY:
						if (Rchunk)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (reader.Frame.BitMore == 0)
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
						if (reader.Frame.BitMore == 0)
						{
							Rchunk = false;
							OnEventData(new WSData(reader.Frame.DataBody, WSOpcod.Continue, WSFin.Last));
						}
						else
							OnEventChunk(new WSData(reader.Frame.DataBody, WSOpcod.Continue, WSFin.Next));
						break;
					default:
						throw new WSException("Опкод: " + reader.Frame.BitPcod, WsError.PcodNotSuported, WSClose.UnsupportedData);
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
			MD5 md5 = MD5.Create();
			Regex regex = new Regex(@"\D");

			if (!request.ContainsKey("sec-websocket-key1"))
				throw new WSException("Отсутствует заголовок sec-webspcket-key1", WsError.PcodNotSuported, WSClose.UnsupportedData);
			if (!request.ContainsKey("sec-websocket-key"))
				throw new WSException("Отсутствует заголовок sec-webspcket-key2", WsError.PcodNotSuported, WSClose.UnsupportedData);
			
			long space_1 = Regex.Matches(request["sec-websocket-key1"], @" ").Count;
			long space_2 = Regex.Matches(request["sec-websocket-key2"], @" ").Count;

			byte[] key1_byte = BitConverter.GetBytes((int)(Convert.ToInt64(regex.Replace(request["sec-websocket-key1"], "")) / space_1));
			byte[] key2_byte = BitConverter.GetBytes((int)(Convert.ToInt64(regex.Replace(request["sec-websocket-key2"], "")) / space_2));
						   if (BitConverter.IsLittleEndian)
						   {
						       Array.Reverse(key1_byte);
							   Array.Reverse(key2_byte);
						   }
			byte[] key_string = new byte[16];
			Array.Copy(key1_byte, 0, key_string, 0, 4);
			Array.Copy(key2_byte, 0, key_string, 4, 4);
			Array.Copy(request.SegmentsBuffer.Dequeue(), 0, key_string, 8, 8);
			byte[] __data = md5.ComputeHash(key_string);
			md5.Clear();

			Set101(Response);
			OnEventConnect(request, response);
			byte[] buffer = response.ToByte();
			if (Message(buffer, 0, buffer.Length))
				Message(__data, 0, __data.Length);
		}		
    }
	
}