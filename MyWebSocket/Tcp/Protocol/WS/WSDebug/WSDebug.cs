using System;
using System.Text;

namespace MyWebSocket.Tcp.Protocol.WS
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
			debug.Append("[ ");
			for (int i = 0; i < frame.DataHead.Length; i++)
			{
				string bin = Convert.ToString(frame.DataHead[i], 2);
				while (bin.Length < 8)
				{
					bin = "0" + bin;
				}
				debug.Append(bin + " ");
			}
			debug.Append("] ");
			debug.AppendLine();
			debug.AppendLine("FIND: " + frame.BitFin.ToString("X"));
			debug.AppendLine("RSV1: " + frame.BitRsv1.ToString("X"));
			debug.AppendLine("RSV2: " + frame.BitRsv2.ToString("X"));
			debug.AppendLine("RSV3: " + frame.BitRsv3.ToString("X"));
			  debug.Append("PCOD: 0x" + frame.BitPcod.ToString("X"));
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
			  debug.Append("LENG: 0x" + frame.BitLeng.ToString("X") +
							 "(" +  frame.BitLeng.ToString()  + ")");
			if (frame.BitLeng < 126)
				debug.AppendLine();
			else
				debug.AppendLine(" --> " + frame.LengBody.ToString());
			debug.AppendLine("Body byte");
			for (int i = 0; i < frame.DataBody.Length; i++)
			{
				debug.Append(frame.DataBody[i].ToString("X")  +  " ");
			}
			debug.AppendLine();
			debug.AppendLine("*******Protcool N13*******");
			/*   Вывод в консоль   */
			Console.WriteLine(debug);
		}
		public static void DebugSample(WSFrameSample frame)
		{
			StringBuilder debug = new StringBuilder(6000);
			debug.AppendLine("*******Protcool Sample*******");
			if (frame.GetsBody)
				debug.AppendLine("Get Frame");
			else
				debug.AppendLine("Send Frame");
			debug.AppendLine("Head byte");
			for (int i = 0; i < frame.DataHead.Length; i++)
			{
				debug.Append(frame.DataHead[i].ToString("X") + " ");
			}
			debug.Append("[ ");
			for (int i = 0; i < frame.DataHead.Length; i++)
			{
				string bin = Convert.ToString(frame.DataHead[i], 2);
				while (bin.Length < 8)
				{
					bin = "0" + bin;
				}
				debug.Append(bin + " ");
			}
			debug.Append("] ");
			debug.AppendLine();
			debug.AppendLine("More: " + frame.BitMore.ToString("X"));
			debug.AppendLine("RSV1: " + frame.BitRsv1.ToString("X"));
			debug.AppendLine("RSV2: " + frame.BitRsv2.ToString("X"));
			debug.AppendLine("RSV3: " + frame.BitRsv3.ToString("X"));
			  debug.Append("PCOD: 0x" + frame.BitPcod.ToString("X"));
			switch (frame.BitPcod)
			{
				case WSFrameSample.TEXT:
					debug.AppendLine(" --> Text");
					break;
				case WSFrameSample.PING:
					debug.AppendLine(" --> Ping");
					break;
				case WSFrameSample.PONG:
					debug.AppendLine(" --> Pong");
					break;
				case WSFrameSample.CLOSE:
					debug.AppendLine(" --> Close");
					break;
				case WSFrameSample.BINNARY:
					debug.AppendLine(" --> binnary");
					break;
				case WSFrameSample.CONTINUE:
					debug.AppendLine(" --> Continue");
					break;
				default:
					debug.AppendLine();
					break;

			}
			   debug.Append("LENG: 0x" + frame.BitLeng.ToString("X") +
							    "(" + frame.BitLeng.ToString() + ")");
			if (frame.BitLeng < 126)
				debug.AppendLine();
			else
				debug.AppendLine(" --> " + frame.LengBody.ToString());
			debug.AppendLine("Body byte");
			for (int i = 0; i < frame.DataBody.Length; i++)
			{
				debug.Append(frame.DataBody[i].ToString("X")  +  " ");
			}
			debug.AppendLine();
			debug.AppendLine("*******Protcool Sample*******");
			/*   Вывод в консоль   */
			Console.WriteLine(debug);
		}
	}
}