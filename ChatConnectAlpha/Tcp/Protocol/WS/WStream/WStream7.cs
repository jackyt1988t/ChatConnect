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
            int _read = 0;

            if (Frame.DataBody == null)
                Frame.DataBody  =  new byte[ Frame.LengBody ];

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
						_read++;
						PointR++;

						if (++Frame.PartBody == Frame.LengBody)
						{
							Frame.GetsBody = true;
							return _read;
						}
					}
				}
			}
            else
            {
                /*     массив байт      */
                byte[] mask = new byte[4];
                       mask[0] = (byte)((Frame.MaskVal >> 24));
                       mask[1] = (byte)((Frame.MaskVal << 08) >> 24);
                       mask[2] = (byte)((Frame.MaskVal << 16) >> 24);
                       mask[3] = (byte)((Frame.MaskVal << 24) >> 24);

				fixed (byte * sourse = _buffer, target = Frame.DataBody)
				{
					byte* ps = sourse + PointR;
					byte* pt = target + Frame.PartBody;

					while (!Empty)
					{
						if (Frame.MaskPos > 3)
							Frame.MaskPos = 0;
						*pt = (byte)(*ps ^ mask[Frame.MaskPos]);
						ps++;
						pt++;
						_read++;
						PointR++;
						Frame.MaskPos++;

						if (++Frame.PartBody == Frame.LengBody)
						{
							Frame.GetsBody = true;
							return _read;
						}
					}
				}
            }

            _read = -1;
            return _read;
        }
        public override int ReadHead()
        {
            int _read = 0;
            int _byte = -1;

            while ((_byte = ReadByte()) > -1)
            {
                switch (Frame.Handler)
                {
                    case 0:
                        /*        FIN - доставка сообщения      */
                        Frame.BitFin = (int)((uint) _byte >> 7 );
                        /*      RCV1 - устанавливается сервером.      */
                        Frame.BitRsv1 = (int)((uint)_byte << 25 >> 31 );
                        /*      RCV2 - устанавливается сервером.      */
                        Frame.BitRsv2 = (int)((uint)_byte << 26 >> 31 );
                        /*      RCV3 - устанавливается сервером.      */
                        Frame.BitRsv3 = (int)((uint)_byte << 27 >> 31 );
                        /*      Опкод-хранит информацию о данных      */
                        Frame.BitPcod = (int)((uint)_byte << 28 >> 28 );

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
                        Frame.BitMask = (int)((uint) _byte >> 7 );
                        /*      Длинна полученного тела сообщения      */
                        Frame.BitLeng = (int)((uint) _byte << 25 >> 25 );

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
                            goto case 10;
                        }

                        break;
                    case 2:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
                                         Frame.LengBody = (long)(_byte << 56);
                        break;
                    case 3:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
                        Frame.LengBody = Frame.LengBody | (long)(_byte << 48);
                        break;
                    case 4:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
                        Frame.LengBody = Frame.LengBody | (long)(_byte << 40);
                        break;
                    case 5:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
                        Frame.LengBody = Frame.LengBody | (long)(_byte << 32);
                        break;
                    case 6:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
                        Frame.LengBody = Frame.LengBody | (long)(_byte << 24);
                        break;
                    case 7:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
                        Frame.LengBody = Frame.LengBody | (long)(_byte << 16);
                        break;
                    case 8:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
                        Frame.LengBody = Frame.LengBody | (long)(_byte << 08);
                        break;
                    case 9:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
                        Frame.LengBody = Frame.LengBody | (long)(_byte << 00);
                        goto case 10;
                    case 10:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
                        
                        if (Frame.BitMask == 1)
                            Frame.LengHead += 4;

                        break;
                    case 11:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
                                          	Frame.MaskVal = (_byte << 24);
                        break;
                    case 12:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
                        	Frame.MaskVal = Frame.MaskVal | (_byte << 16);
                        break;
                    case 13:
                        /*  Обработчик.  */
                        Frame.Handler += 1;
                        	Frame.MaskVal = Frame.MaskVal | (_byte << 08);
                        break;
                    case 14:
                        	Frame.MaskVal = Frame.MaskVal | (_byte << 00);
                        break;
                }

                _read++;
                Frame.PartHead++;

                if (Frame.PartHead == Frame.LengHead)
                {
                    Frame.GetsHead = true;
						return _read;
                }
            }

            _read = -1;
            return _read;
        }
    }
	
}
