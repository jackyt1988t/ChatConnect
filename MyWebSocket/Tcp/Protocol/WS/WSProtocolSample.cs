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
		public WsPolicy Policy
		{
			get;
			private set;
		}
		bool Rchunk;
		WSSampleReader reader;
		public override MyStream Reader
		{
			get
			{
				return reader;
			}
		}
		bool Wchunk;
		WSSampleWriter writer;
		public override MyStream Writer
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
			reader = new WSSampleReader(
								SizeRead);
			writer = new WSSampleWriter(0);
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
			if (http.Reader.Length > 0)
				http.Reader.CopyTo(reader,
						 (int)http.Reader.Length);

			Policy.SetPolicy(0, 1, 1, 1, 0, 32000);
			Request = http.Request;

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
			header.AddHeader("Upgrade", "WebSocket");
			header.AddHeader("Connection", "Upgrade");
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
					Opcod = WSN13.TEXT;
					break;
				case WSOpcod.Ping:
					Opcod = WSN13.PING;
					break;
				case WSOpcod.Pong:
					Opcod = WSN13.PONG;
					break;
				case WSOpcod.Close:
					Opcod = WSN13.CLOSE;
					break;
				case WSOpcod.Binnary:
					Opcod = WSN13.BINNARY;
					break;
				case WSOpcod.Continue:
					Opcod = WSN13.CONTINUE;
					break;
			}
			lock (Sync)
			{
				/*      Очитстить.      */
				writer._Frame.Null();

				writer._Frame.BitMore  = Fin;
				writer._Frame.BitPcod  = Opcod;
				writer._Frame.PartBody = recive;
				writer._Frame.LengBody = length;
				writer._Frame.DataBody = message;
				writer._Frame.InitializationHeader();
				if (Debug)
					WSDebug.DebugSample(writer._Frame);
				if (!Message(writer._Frame.DataHead, 0, (int)writer._Frame.LengHead))
					return false;
				else
					return Message(writer._Frame.DataBody, 0, (int)writer._Frame.LengBody);
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
			if (reader._Frame.GetsHead && reader._Frame.GetsBody)
				reader._Frame.Null();

			if (!reader._Frame.GetsHead)
			{
				if (reader.ReadHead() == -1)
					return;
				if (reader._Frame.BitRsv1 == 1)
					throw new WSException("Неверный бит rcv1", WsError.HeaderFrameError, WSClose.PolicyViolation);
				if (reader._Frame.BitRsv2 == 1)
					throw new WSException("Неверный бит rcv2", WsError.HeaderFrameError, WSClose.PolicyViolation);
				if (reader._Frame.BitRsv3 == 1)
					throw new WSException("Неверный бит rcv3", WsError.HeaderFrameError, WSClose.PolicyViolation);
				if (reader._Frame.BitRsv4 == 1)
					throw new WSException("Неверный бит rcv4", WsError.HeaderFrameError, WSClose.PolicyViolation);
				if (reader._Frame.LengBody < 0 || reader._Frame.LengBody > 32000)
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
					WSDebug.DebugSample(reader._Frame);
				switch (reader._Frame.BitPcod)
				{

					case WSN13.TEXT:
						if (Rchunk)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (reader._Frame.BitMore == 0)
							OnEventData(new WSData(reader._Frame.DataBody, WSOpcod.Text, WSFin.Last));
						else
						{
							Rchunk = true;
							OnEventChunk(new WSData(reader._Frame.DataBody, WSOpcod.Text, WSFin.Next));
						}
						break;
					case WSN13.PING:
						if (reader._Frame.BitMore == 1)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);

							OnEventPing(new WSData(reader._Frame.DataBody, WSOpcod.Ping, WSFin.Last));
						break;
					case WSN13.PONG:
						if (reader._Frame.BitMore == 1)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (PingControl.SetPing.ToString() != Encoding.UTF8.GetString(reader._Frame.DataBody))
							throw new WSException("Неверный бит fin.", WsError.PongBodyIncorrect,WSClose.PolicyViolation);
							PingControl.GetPong = new TimeSpan( DateTime.Now.Ticks );

							OnEventPong(new WSData(reader._Frame.DataBody, WSOpcod.Pong, WSFin.Last));
						break;
					case WSN13.CLOSE:
						if (reader._Frame.BitMore == 0)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						
							CloseServer(WSClose.Normal, Encoding.UTF8.GetString(reader._Frame.DataBody), false);
						break;
					case WSN13.BINNARY:
						if (Rchunk)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (reader._Frame.BitMore == 0)
							OnEventData(new WSData(reader._Frame.DataBody, WSOpcod.Binnary, WSFin.Last));
						else
						{
							Rchunk = true;
							OnEventChunk(new WSData(reader._Frame.DataBody, WSOpcod.Binnary, WSFin.Next));
						}
						break;
					case WSN13.CONTINUE:
						if (!Rchunk)
							throw new WSException("Неверный бит fin.", WsError.HeaderFrameError, WSClose.PolicyViolation);
						if (reader._Frame.BitMore == 0)
						{
							Rchunk = false;
							OnEventData(new WSData(reader._Frame.DataBody, WSOpcod.Continue, WSFin.Last));
						}
						else
							OnEventChunk(new WSData(reader._Frame.DataBody, WSOpcod.Continue, WSFin.Next));
						break;
					default:
						throw new WSException("Опкод: " + reader._Frame.BitPcod, WsError.PcodNotSuported, WSClose.UnsupportedData);
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

			string key1;
			string key2;
			if (!request.ContainsKeys("sec-websocket-key1", out key1, true))
				throw new WSException("Отсутствует заголовок sec-webspcket-key1", WsError.PcodNotSuported, WSClose.UnsupportedData);
			if (!request.ContainsKeys("sec-websocket-key2", out key2, true))
				throw new WSException("Отсутствует заголовок sec-webspcket-key2", WsError.PcodNotSuported, WSClose.UnsupportedData);
			
			long space_1 = Regex.Matches(key1, @" ").Count;
			long space_2 = Regex.Matches(key2, @" ").Count;

			byte[] key1_byte = BitConverter.GetBytes((int)(Convert.ToInt64(regex.Replace(key1, "")) / space_1));
			byte[] key2_byte = BitConverter.GetBytes((int)(Convert.ToInt64(regex.Replace(key2, "")) / space_2));
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