using System;
using System.IO;

namespace MyWebSocket.Tcp.Protocol.WS
{
    class WSReaderN13
    {
		byte _ngHead;
		public WSFrameN13 __Frame;
		public Stream Stream;

		public WSReaderN13(Stream stream, WSFrameN13 frame) :
			base()
        {
			Stream = stream;
			__Frame = frame;
        }

		public bool ReadBody()
        {
			if (  __Frame.BitLeng == 0  )
				return true;
           __Frame.PartBody += Stream.Read(__Frame.DataBody, 
											(int)__Frame.PartBody, 
												(int)(__Frame.LengBody - __Frame.PartBody));

			if (__Frame.PartBody == __Frame.LengBody)
			{
				if (  __Frame.BitMask == 1  )
				{
					
					for (int part = 0;part < __Frame.LengBody; part++)
					{
						__Frame.DataBody[part] = (byte)(__Frame.DataBody[part] ^ __Frame.DataMask[part % 4]);
					}
				}
				
				return (__Frame.GetBody = true);
			}
            return false;
        }
        public bool ReadHead()
        {
			int @char = 0;
			while ((@char = Stream.ReadByte()) > -1)
            {
                switch (__Frame.Handler)
                {
                    case 0:

						__Frame.BitFin  = (@char & 0x80) >> 7;
                        __Frame.BitRsv1 = (@char & 0x40) >> 6;
						__Frame.BitRsv2 = (@char & 0x20) >> 5;
						__Frame.BitRsv3 = (@char & 0x10) >> 4;
                        __Frame.BitPcod = (@char & 0x0F);

                        /* Общая длинна  */
                        __Frame.LengHead = 2;
                        /* Длинна ответа */
                        __Frame.PartHead = 0;
                        /* Длинна ответа */
                        __Frame.PartBody = 0;
                        /*  Обработчик.  */
                        __Frame.Handler += 1;

						_ngHead = (byte)@char;
						break;
                    case 1:
						__Frame.BitMask = (@char & 0x80) >> 7;
                        __Frame.BitLeng = (@char & 0x7F);
			
						if (__Frame.BitMask == 1)
							__Frame.LengHead += 4;
                        if (__Frame.BitLeng == 127)
                        {
                            __Frame.Handler += 1;
                            __Frame.LengHead += 8;
                        }
                        else if (__Frame.BitLeng == 126)
                        {
                            __Frame.Handler += 7;
                            __Frame.LengHead += 2;
                        }
                        else if (__Frame.BitLeng <= 125)
                        {
                            __Frame.Handler += 9;
                            __Frame.LengBody = __Frame.BitLeng;
                        }
						
						__Frame.DataHead = 
							new byte[__Frame.LengHead];
						__Frame.DataHead[0] = _ngHead;
						__Frame.DataHead[__Frame.PartHead] = (byte)@char;
						
						break;
                    case 2:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.DataHead[__Frame.PartHead] = (byte)@char;
                                         __Frame.LengBody  =  @char << 56;
                        break;
                    case 3:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.DataHead[__Frame.PartHead] = (byte)@char;
						__Frame.LengBody = __Frame.LengBody | ((long)@char << 48);
                        break;
                    case 4:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.DataHead[__Frame.PartHead] = (byte)@char;
						__Frame.LengBody = __Frame.LengBody | ((long)@char << 40);
                        break;
                    case 5:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.DataHead[__Frame.PartHead] = (byte)@char;
						__Frame.LengBody = __Frame.LengBody | ((long)@char << 32);
                        break;
                    case 6:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.DataHead[__Frame.PartHead] = (byte)@char;
						__Frame.LengBody = __Frame.LengBody | ((long)@char << 24);
                        break;
                    case 7:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.DataHead[__Frame.PartHead] = (byte)@char;
						__Frame.LengBody = __Frame.LengBody | ((long)@char << 16);
                        break;
                    case 8:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.DataHead[__Frame.PartHead] = (byte)@char;
						__Frame.LengBody = __Frame.LengBody | ((long)@char << 08);
                        break;
                    case 9:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.DataHead[__Frame.PartHead] = (byte)@char;
						__Frame.LengBody = __Frame.LengBody | ((long)@char << 00);
                        break;
                    case 10:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						
						__Frame.MaskVal = @char << 24;
						__Frame.DataMask = new byte[4];
						__Frame.DataMask[0] = (byte)@char;
						__Frame.DataHead[__Frame.PartHead] = (byte)@char;
						
                        break;
                    case 11:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						
						__Frame.MaskVal = __Frame.MaskVal 
										  | (@char << 16);
						__Frame.DataMask[1] = (byte)@char;
						__Frame.DataHead[__Frame.PartHead] = (byte)@char;
						
                        		
                        break;
                    case 12:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.MaskVal = __Frame.MaskVal 
										  | (@char << 08);
						__Frame.DataMask[2] = (byte)@char;
						__Frame.DataHead[__Frame.PartHead] = (byte)@char;
						
								
                        break;
                    case 13:
						__Frame.MaskVal = __Frame.MaskVal 
										  | (@char << 00);
						__Frame.DataMask[3] = (byte)@char;
						__Frame.DataHead[__Frame.PartHead] = (byte)@char;
								
                        break;
                }

                __Frame.PartHead++;

                if (__Frame.PartHead == __Frame.LengHead)
                {
						if (__Frame.LengBody > -1)
							__Frame.DataBody = new byte[__Frame.LengBody];

					return (__Frame.GetHead = true);
                }
            }
            return false;
        }
    }
	
}
