using System;

namespace ChatConnect.Tcp.Protocol.WS
{
    class WStream7 : WStream
    {
		public WSFrameN13 Frame;

        public WStream7(int length)
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
						if (Frame.MaskPos > 3)
							Frame.MaskPos = 0;

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
				fixed (byte * sourse = _buffer, target = Frame.DataBody, header = Frame.DataHead)
				{
					byte* ps = sourse + PointR;
					byte* pt = target + Frame.PartBody;
					byte* mask = header + (Frame.LengHead - 4);

					while (!Empty)
					{
						*pt = (byte)(*ps ^ (mask + Frame.PartBody % 4);
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

            _read = -1;
            return _read;
        }
        public override int ReadHead()
        {
            int read = 0;

            while (!Empty)
            {
                switch (Frame.Handler)
                {
                    case 0:
                        /* FIN */
                        Frame.BitFin = Buffer[PointR] & 0x80;
                        /* RCV1 */
                        Frame.BitRsv1 = Buffer[PointR] & 0x40;
                        /* RCV2 */
                        Frame.BitRsv2 = Buffer[PointR] & 0x20;
                        /* RCV3 */
                        Frame.BitRsv3 = Buffer[PointR] & 0x10;
                        /*      Опкод-хранит информацию о данных      */
                        Frame.BitPcod = Buffer[PointR] << 28 >> 28 );

                        /* Общая длинна  */
                        Frame.LengHead = 2;
                        /* Длинна ответа */
                        Frame.PartHead = 0;
                        /* Длинна ответа */
                        Frame.PartBody = 0;
                        /*  Обработчик.  */
                        Frame.Handler += 1;

                        break;
                    case 1:
                        /*       Бит маски тела сообщения       */
                        Frame.BitMask = Buffer[PointR] & 0x80);
                        /*      Длинна полученного тела сообщения      */
                        Frame.BitLeng = Buffer[PointR] << 25 >> 25 );
			
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
                            break;
                        }
			/*     Заголовок полученных данных.     */
			Frame.DataHead = new byte[Frame.LengHead];
			Frame.DataBody = new byte[Frame.LengBody];

			Frame.DataHead[0] = Buffer[PointR -1];
			Frame.DataHead[1] = Buffer[PointR]
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
                                          	Frame.MaskVal = (Buffer[PointR] << 24);
                        break;
                    case 11:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
			Frame.DataHead[Frame.PartHead] = Buffer[PointR];
                        	Frame.MaskVal = Frame.MaskVal | (Buffer[PointR] << 16);
                        break;
                    case 12:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
			Frame.DataHead[Frame.PartHead] = Buffer[PointR];
                        	Frame.MaskVal = Frame.MaskVal | (Buffer[PointR] << 08);
                        break;
                    case 13:
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
						return read;
                }
            }

            _read = -1;
            return _read;
        }
    }
	
}
