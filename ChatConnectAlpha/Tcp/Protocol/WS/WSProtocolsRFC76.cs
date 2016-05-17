using System;
using System.Text;
using System.Security.Cryptography;


namespace ChatConnect.Tcp.Protocol.WS
{
	class WSProtocolRFC76 : WSProtocol
	{
		private WSFrameRFC76 __WSFrame;

		private static readonly string CHECKKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
		
		public WSProtocolRFC76() : 
			base()
			{
				__WSFrame = new WSFrameRFC76();
			}
		public WSProtocolRFC76(IProtocol http, PHandlerEvent connect) : 
			base(http, connect)
			{
				__WSFrame = new WSFrameRFC76();
			}
		protected override void Connection()
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
			
			base.Connection();
		}
		protected override void HandlerFrame(byte[] buffer, int recive, int length)
		{
			WStreamRFC76 stream = new WStreamRFC76 (  buffer, recive, length  );
			while ((stream.Length - stream.Position) > 0)
			{
				if (!__WSFrame.GetsHead)
				{
					/*  */
					if (stream.ReadHeader(ref __WSFrame) == -1)
						break;

					if (__WSFrame.BitRsv1 == (int)WSChecks.Rcv1)
					{
						throw new WSException("Установлен бит rcv1. rcv1", WsError.HeaderFrameError,
																			 WSCloseNum.PolicyViolation);
					}
					if (__WSFrame.BitRsv2 == (int)WSChecks.Rcv2)
					{
						throw new WSException("Установлен бит rcv2. rcv2", WsError.HeaderFrameError,
																			 WSCloseNum.PolicyViolation);
					}
					if (__WSFrame.BitRsv3 == (int)WSChecks.Rcv3)
					{
						throw new WSException("Установлен бит rcv3. rcv3", WsError.HeaderFrameError,
																			 WSCloseNum.PolicyViolation);
					}
					if (__WSFrame.BitMask == (int)WSChecks.Mask)
					{
						throw new WSException("Установлен бит mask. mask", WsError.HeaderFrameError,
																			 WSCloseNum.PolicyViolation);
					}
					if (__WSFrame.LengBody == 0 || __WSFrame.LengBody > WSChecks.Leng)
					{
						throw new WSException("Длинна: " + __WSFrame.LengBody, WsError.BodyFrameError,
																				 WSCloseNum.PolicyViolation);
					}
				}
				if (!__WSFrame.GetsBody)
				{
					if (stream.ReadBody(ref __WSFrame) == -1)
						break;

					if (__Binnary == null)
						__Binnary = new WSBinnary(__WSFrame.BitPcod);
					else
					{
						if (__Binnary.Opcod != __WSFrame.BitPcod)
						{
							throw new WSException("Опкод данных не совпадает: " +
								__Binnary.Opcod + " <=> " + __WSFrame.BitPcod, WsError.PcodNotRepeat,
																			     WSCloseNum.InvalidFrame);
						}
					}
					/*      Добавляем данные в буффер     */
					__Binnary.AddBinary(__WSFrame.DataBody);

					if (__WSFrame.BitFind == 1)
					{

						switch (__WSFrame.BitPcod)
						{
							case WSFrameRFC76.TEXT:
								OnEventData(__Binnary);
								break;
							case WSFrameRFC76.PING:
								OnEventPing(__Binnary);
								break;
							case WSFrameRFC76.PONG:
								OnEventPong(__Binnary);
								break;
							case WSFrameRFC76.CLOSE:
								State = States.Close;
								if (__Binnary.Buffer.Length > 1)
								{
									int number;
									number = __Binnary.Buffer[0] << 8;
									number = __Binnary.Buffer[1] | number;

									if (number >= 1000 || number <= 1012)
										close = new WSClose(Address(),(WSCloseNum)number);
									else
										close = new WSClose(Address(), WSCloseNum.Abnormal);
								}
								else
								{
									close = new WSClose(Address(), WSCloseNum.Abnormal);
								}
								return;
							case WSFrameRFC76.BINARY:
								OnEventData(__Binnary);
								break;
							default:
								throw new WSException("Опкод: " + __WSFrame.BitPcod, WsError.PcodNotSuported,
																					   WSCloseNum.UnsupportedData);
						}
					}
					else
					{
						WSBinnary Binnary = new WSBinnary(__WSFrame.BitPcod);
								  Binnary.AddBinary(__WSFrame.DataBody);
						OnEventChunk(Binnary);
					}
					/*   Очистить   */
					__WSFrame.Clear();
				}
			}
		}
	}
}
