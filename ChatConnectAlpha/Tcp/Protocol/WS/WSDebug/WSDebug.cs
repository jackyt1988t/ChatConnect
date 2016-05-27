using System;
using System.Text;

namespace ChatConnect.Tcp.Protocol.WS
{
	static class WSDebug
	{
		public static void DebugN13(WSFrameN13 frame)
		{
			StringBuilder debug = new StringBuilder(6000);
			debug.AppendLine("*******Protcool N13*******");
			if (frame.GetsBody)
				debug.AppendLine("Get Frame");
			else
				debug.AppendLine("Send Frame");
			debug.AppendLine("Head byte");
			for (int i = 0; i < frame.DataHead.Length; i++)
			{
				debug.Append(frame.DataHead[i].ToString("X") + " ");
			}
			debug.AppendLine();
			debug.AppendLine("FIND: " + frame.BitFin.ToString("X"));
			debug.AppendLine("RSV1: " + frame.BitRsv1.ToString("X"));
			debug.AppendLine("RSV2: " + frame.BitRsv2.ToString("X"));
			debug.AppendLine("RSV3: " + frame.BitRsv3.ToString("X"));
				debug.Append("PCOD: " + frame.BitPcod.ToString("X"));
			switch (frame.BitPcod)
			{
				case WSFrameN13.TEXT:
					debug.AppendLine(" --> Text");
					break;
				case WSFrameN13.PING:
					debug.AppendLine(" --> Ping");
					break;
				case WSFrameN13.PONG:
					debug.AppendLine(" --> Pong");
					break;
				case WSFrameN13.CLOSE:
					debug.AppendLine(" --> Close");
					break;
				case WSFrameN13.BINNARY:
					debug.AppendLine(" --> binnary");
					break;
				case WSFrameN13.CONTINUE:
					debug.AppendLine(" --> Continue");
					break;
				default:
					debug.AppendLine();
					break;

			}
				debug.Append("MASK: " + frame.BitMask.ToString("X"));
			if (frame.BitMask == 0)
				debug.AppendLine();
			else
				debug.AppendLine(" --> " + frame.MaskVal.ToString());
				debug.Append("LENG: " + frame.BitLeng.ToString("X"));
			if (frame.BitLeng < 126)
				debug.AppendLine();
			else
				debug.AppendLine(" --> " + frame.LengBody.ToString());
			debug.AppendLine("Body byte");
			for (int i = 0; i < frame.DataBody.Length; i++)
			{
				debug.Append(frame.DataBody[i].ToString("X") + " ");
			}
			debug.AppendLine();
			debug.AppendLine("*******Protcool N13*******");
			/*   Вывод в консоль   */
			Console.WriteLine(debug);
		}
	}
}