using System;
using System.IO;
using MyWebSocket.Tcp.Protocol.WS.WS_13;

namespace MyWebSocket.Tcp.Protocol.WS
{
    public class WSReaderN13
    {
        public Stream Stream;
        public WSFrameN13 __Frame;
        public WSFramesN13 __Frames;

		public WSReaderN13(Stream stream) :
			base()
        {
			Stream = stream;
            __Frame = new WSFrameN13();
        }

		public bool ReadBody()
        {
		    if ( __Frame.BitLeng == 0 )
				return true;
            
            int lenght    = (int)
                            (__Frame.LengBody - 
                                __Frame.PartBody);
            int offset    = (int)
                            __Frame.D__Body.Position;
			byte[] buffer = __Frame.D__Body.GetBuffer();

            __Frame.PartBody += Stream.Read(buffer, 
											    offset, 
												    lenght);

			if (__Frame.PartBody == __Frame.LengBody)
			{
				if ( __Frame.BitMask == 1 )
				{
					
					for (int part = 0;part < __Frame.LengBody; part++)
					{
						buffer[part] = 
                            (byte)(buffer[part] ^ __Frame.D__Mask[part % 4]);
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
						
						break;
                    case 2:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.LengBody  =  @char << 56;
                        break;
                    case 3:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.LengBody = __Frame.LengBody | ((long)@char << 48);
                        break;
                    case 4:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.LengBody = __Frame.LengBody | ((long)@char << 40);
                        break;
                    case 5:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.LengBody = __Frame.LengBody | ((long)@char << 32);
                        break;
                    case 6:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.LengBody = __Frame.LengBody | ((long)@char << 24);
                        break;
                    case 7:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.LengBody = __Frame.LengBody | ((long)@char << 16);
                        break;
                    case 8:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.LengBody = __Frame.LengBody | ((long)@char << 08);
                        break;
                    case 9:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.LengBody = __Frame.LengBody | ((long)@char << 00);
                        break;
                    case 10:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						
						__Frame.MaskVal = @char << 24;
						__Frame.D__Mask = new byte[4];
						__Frame.D__Mask[0] = (byte)@char;		
                        break;
                    case 11:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						
						__Frame.MaskVal = __Frame.MaskVal | (@char << 16);
						__Frame.D__Mask[1] = (byte)@char;   		
                        break;
                    case 12:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.MaskVal = __Frame.MaskVal | (@char << 08);
						__Frame.D__Mask[2] = (byte)@char;								
                        break;
                    case 13:
						__Frame.MaskVal = __Frame.MaskVal | (@char << 00);
						__Frame.D__Mask[3] = (byte)@char;							
                        break;
                }

                __Frame.PartHead++;
                __Frame.D__Body.WriteByte( (byte)@char );

                if (__Frame.PartHead == __Frame.LengHead)
                {
						if (__Frame.LengBody > 0)
							__Frame.D__Body.SetLength( __Frame.LengBody );

					return (__Frame.GetHead = true);
                }
            }
            return false;
        }
    }
	
}
