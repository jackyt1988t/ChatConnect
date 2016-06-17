using System;
using System.IO;

namespace MyWebSocket.Tcp.Protocol.WS
{
	class WStreamSample : MyStream
	{
		byte _ngHead;
		public WSFrameSample Frame;

		public WStreamSample() :
			base(1024)
		{
			Frame = new WSFrameSample();
		}
		public WStreamSample(int length) :
			base(length)
		{
			Frame = new WSFrameSample();
		}

		public override void Reset()
		{
			base.Reset();
			Frame.Null();
		}
		unsafe public override int ReadBody()
		{
			int read = 0;
			if (Frame.BitLeng == 0)
				return read;
			fixed (byte* sourse = _buffer, target = Frame.DataBody)
			{
				byte* ps = sourse + PointR;
				byte* pt = target + Frame.PartBody;

				while (!Empty)
				{
					*pt = *ps;
					ps++;
					pt++;
					read++;
					PointR++;

					if (++Frame.PartBody == Frame.LengBody)
					{
						Frame.GetsBody = true;
						return read;
					}
				}
			}

			read = -1;
			return read;
		}
		public override int ReadHead()
		{
			int read = 0;

			while (!Empty)
			{
				switch (Frame.Handler)
				{
					case 0:
						Frame.BitMore = (Buffer[PointR] & 0x80) >> 7;
						Frame.BitRsv1 = (Buffer[PointR] & 0x40) >> 6;
						Frame.BitRsv2 = (Buffer[PointR] & 0x20) >> 5;
						Frame.BitRsv3 = (Buffer[PointR] & 0x10) >> 4;
						Frame.BitPcod = (Buffer[PointR] & 0x0F);

						/* Общая длинна  */
						Frame.LengHead = 2;
						/* Длинна ответа */
						Frame.PartHead = 0;
						/* Длинна ответа */
						Frame.PartBody = 0;
						/*  Обработчик.  */
						Frame.Handler += 1;

						_ngHead	= Buffer[PointR];
					break;
					case 1:
						Frame.BitRsv4 = (Buffer[PointR] & 0x80) >> 7;
						Frame.BitLeng = (Buffer[PointR] & 0x7F);

						if (Frame.BitLeng == 127)
						{
							Frame.Handler += 1;
							Frame.LengHead += 8;
						}
						else if (Frame.BitLeng == 126)
						{
							Frame.Handler += 7;
							Frame.LengHead += 2;
						}
						else if (Frame.BitLeng <= 125)
						{
							Frame.Handler += 9;
							Frame.LengBody = Frame.BitLeng;
						}

						Frame.DataHead =
						   new byte[Frame.LengHead];
						Frame.DataHead[0] = _ngHead;
						Frame.DataHead[Frame.PartHead] = Buffer[PointR];
						break;
					case 2:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.DataHead[Frame.PartHead] = Buffer[PointR];
						Frame.LengBody = (long)(Buffer[PointR] << 56);
						break;
					case 3:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.DataHead[Frame.PartHead] = Buffer[PointR];
						Frame.LengBody = Frame.LengBody | (long)(Buffer[PointR] << 48);
						break;
					case 4:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.DataHead[Frame.PartHead] = Buffer[PointR];
						Frame.LengBody = Frame.LengBody | (long)(Buffer[PointR] << 40);
						break;
					case 5:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.DataHead[Frame.PartHead] = Buffer[PointR];
						Frame.LengBody = Frame.LengBody | (long)(Buffer[PointR] << 32);
						break;
					case 6:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.DataHead[Frame.PartHead] = Buffer[PointR];
						Frame.LengBody = Frame.LengBody | (long)(Buffer[PointR] << 24);
						break;
					case 7:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.DataHead[Frame.PartHead] = Buffer[PointR];
						Frame.LengBody = Frame.LengBody | (long)(Buffer[PointR] << 16);
						break;
					case 8:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.DataHead[Frame.PartHead] = Buffer[PointR];
						Frame.LengBody = Frame.LengBody | (long)(Buffer[PointR] << 08);
						break;
					case 9:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.DataHead[Frame.PartHead] = Buffer[PointR];
						Frame.LengBody = Frame.LengBody | (long)(Buffer[PointR] << 00);
						break;
				}

				read++;
				PointR++;
				Frame.PartHead++;

				if (Frame.PartHead == Frame.LengHead)
				{
					Frame.GetsHead = true;
					if (Frame.LengBody > 0)
						Frame.DataBody = new byte[Frame.LengBody];
					return read;
				}
			}

			read = -1;
			return read;
		}
	}
}
