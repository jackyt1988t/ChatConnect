using System;

namespace ChatConnect.Tcp.Protocol.WS
{
    class WStreamN13 : WStream
    {
		byte _ngHead;
		public WSFrameN13 Frame;

        public WStreamN13(int length)
        {
			Frame  =  new WSFrameN13();

			_len = length;
			_buffer = new byte[ length ];
        }

 unsafe public override int ReadBody()
        {
            int read = 0;

            if (  Frame.BitMask == 0  )
            {
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
			}
            else
            {
				fixed (byte * sourse = _buffer, target = Frame.DataBody)
				{
					byte* ps = sourse + PointR;
					byte* pt = target + Frame.PartBody;

					while (!Empty)
					{
						*pt = (byte)(*ps ^ Frame.DataMask[Frame.PartBody % 4]);
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

						Frame.BitFin = (Buffer[PointR] & 0x80) >> 7;
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

						_ngHead = Buffer[PointR];
						break;
                    case 1:
						Frame.BitMask = (Buffer[PointR] & 0x80) >> 7;
                        Frame.BitLeng = (Buffer[PointR] & 0x7F);
			
						if (Frame.BitMask == 1)
							Frame.LengHead += 4;
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
                    case 10:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
						Frame.DataMask = new byte[4];
						Frame.DataMask[0] = Buffer[PointR];
						Frame.DataHead[Frame.PartHead] = Buffer[PointR];
						
                                          		Frame.MaskVal = (Buffer[PointR] << 24);
                        break;
                    case 11:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
						Frame.DataMask[1] = Buffer[PointR];
						Frame.DataHead[Frame.PartHead] = Buffer[PointR];
						
                        		Frame.MaskVal = Frame.MaskVal | (Buffer[PointR] << 16);
                        break;
                    case 12:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
						Frame.DataMask[2] = Buffer[PointR];
						Frame.DataHead[Frame.PartHead] = Buffer[PointR];
						
								Frame.MaskVal = Frame.MaskVal | (Buffer[PointR] << 08);
                        break;
                    case 13:
						Frame.DataMask[3] = Buffer[PointR];
						Frame.DataHead[Frame.PartHead] = Buffer[PointR];
								Frame.MaskVal = Frame.MaskVal | (Buffer[PointR] << 00);
                        break;
                }

                read++;
				PointR++;
                Frame.PartHead++;

                if (Frame.PartHead == Frame.LengHead)
                {
                    Frame.GetsHead = true;
					Frame.DataBody = new byte[Frame.LengBody];
					return read;
                }
            }

            read = -1;
            return read;
        }
    }
	
}
