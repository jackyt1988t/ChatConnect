using System;

namespace ChatConnect.Tcp.Protocol.WS
{
	static class WSDebug
	{
		public static void DebugN13(WSFrameN13 frame)
		{

			Console.WriteLine("*******Protcool N13*******");
			if (frame.GetsBody)
				Console.WriteLine("Get Frame");
			else
				Console.WriteLine("Send Frame");
			Console.WriteLine("Head byte");
			for (int i = 0; i < frame.DataHead.Length; i++)
			{
				Console.Write(frame.DataHead[i].ToString("X") + " ");
			}
			Console.WriteLine();
			Console.WriteLine("FIND: " + frame.BitFin.ToString("X"));
			Console.WriteLine("RSV1: " + frame.BitRsv1.ToString("X"));
			Console.WriteLine("RSV2: " + frame.BitRsv2.ToString("X"));
			Console.WriteLine("RSV3: " + frame.BitRsv3.ToString("X"));
			Console.Write("PCOD: " + frame.BitPcod.ToString("X"));
			switch (frame.BitPcod)
			{
				case WSFrameN13.TEXT:
					Console.WriteLine(" --> Text");
					break;
				case WSFrameN13.PING:
					Console.WriteLine(" --> Ping");
					break;
				case WSFrameN13.PONG:
					Console.WriteLine(" --> Pong");
					break;
				case WSFrameN13.CLOSE:
					Console.WriteLine(" --> Close");
					break;
				case WSFrameN13.BINNARY:
					Console.WriteLine(" --> binnary");
					break;
				case WSFrameN13.CONTINUE:
					Console.WriteLine(" --> Continue");
					break;
				default:
					Console.WriteLine();
					break;

			}
			Console.Write("MASK: " + frame.BitMask.ToString("X"));
			if (frame.BitMask == 0)
				Console.WriteLine();
			else
				Console.WriteLine(" --> " + frame.MaskVal.ToString());
			Console.Write("LENG: " + frame.BitLeng.ToString("X"));
			if (frame.BitLeng < 126)
				Console.WriteLine();
			else
				Console.WriteLine(" --> " + frame.LengBody.ToString());
			Console.WriteLine("Body byte");
			for (int i = 0; i < frame.DataBody.Length; i++)
			{
				Console.Write(frame.DataBody[i].ToString("X") + " ");
			}
			Console.WriteLine();
			Console.WriteLine("*******Protcool N13*******");
		}
	}
}