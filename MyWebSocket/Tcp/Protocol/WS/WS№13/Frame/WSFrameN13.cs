using System;
using System.IO;

namespace MyWebSocket.Tcp.Protocol.WS
{
    public class WSFrameN13
    {
        public const int TEXT     = 0x01;
		public const int PING     = 0x09;
		public const int PONG     = 0x0A;
		public const int CLOSE    = 0x08;
		public const int BINNARY  = 0x02;
		public const int CONTINUE = 0x00;
		
		/// <summary>
		/// бит FIN 
		/// </summary>
        public int BitFin
        {
            get;
            set;
        }
		/// <summary>
		/// Номер обработчика
		/// </summary>
		public int Handler
        {
            get;
            set;
        }
		/// <summary>
		/// Текущая позиция маски
		/// </summary>
        public int MaskPos
        {
            get;
            set;
        }
		/// <summary>
		/// бит RSV1
		/// </summary>
        public int BitRsv1
        {
            get;
            set;
        }
		/// <summary>
		/// бит RSV2
		/// </summary>
        public int BitRsv2
        {
            get;
            set;
        }
		/// <summary>
		/// бит RSV3
		/// </summary>
        public int BitRsv3
        {
            get;
            set;
        }
		/// <summary>
		/// Опкод
		/// </summary>
        public int BitPcod
        {
            get;
            set;
        }
		public int RecLeng
		{
			get;
			set;
		}
		/// <summary>
		/// Показывает установлена маска тела или нет
		/// </summary>
		public int BitMask
        {
            get;
            set;
        }
		/// <summary>
		/// 4 бита длинны тела, если 126 и 137 дополнительная длинна
		/// </summary>
        public int BitLeng
        {
            get;
            set;
        }
		/// <summary>
		/// Значение маски тела
		/// </summary>
		public int MaskVal
        {
            get;
            set;
        }
		/// <summary>
		/// 
		/// </summary>
		public bool SetHead
		{
			get;
			private set;
		}
		/// <summary>
		/// Если заголвоки получены true
		/// </summary>
        public bool GetHead
        {
            get;
            set;
        }
		/// <summary>
		/// Если тело получено true
		/// </summary>
        public bool GetBody
        {
            get;
            set;
        }
		/// <summary>
		/// Текущще количество полученный байт тела
		/// </summary>
        public long PartBody
        {
            get;
            set;
        }
		/// <summary>
		/// Текущее количество полученных байт заголвока
		/// </summary>
        public long PartHead
        {
            get;
            set;
        }
		/// <summary>
		/// Длинна заголвоков
		/// </summary>
        public long LengHead
        {
            get;
            set;
        }
		/// <summary>
		/// длинна тела
		/// </summary>
        public long LengBody
        {
            get;
            set;
        }
		/// <summary>
		/// 
		/// </summary>
		public byte[] D__Mask
		{
			get;
			set;
		}
		/// <summary>
		/// Буффер заголвоков
		/// </summary>
		public MemoryStream D__Head
        {
            get;
            private set;
        }
        public MemoryStream D__Body
        {
            get;
            private set;
        }

        public WSFrameN13()
        {
            D__Head = new MemoryStream(0);
            D__Body = new MemoryStream(0);
        }
		public WSFrameN13(int length)
		{
			D__Head = new MemoryStream(0);
			D__Body = new MemoryStream(length);
		}

		static public WSOpcod Convert(int ws_opcod)
		{
			switch (ws_opcod)
            {
	            case WSFrameN13.TEXT:
    	            return WSOpcod.Text;
        	    case WSFrameN13.PING:
            	    return WSOpcod.Ping;
	            case WSFrameN13.PONG:
    	            return WSOpcod.Pong;
        	    case WSFrameN13.CLOSE:
            	    return WSOpcod.Close;
	            case WSFrameN13.BINNARY:
    	            return WSOpcod.Binnary;
        	    case WSFrameN13.CONTINUE:
            	    return WSOpcod.Continue;
	            default:
    	            throw new WSException("Неизвестный опкод");
            }
		}

		public void InitData()
		{
			if (SetHead)
				return;
			else
				SetHead = true;

            LengHead = 2;
			LengBody = 
				D__Body.Length;

			if ( BitMask == 1 )
            {
				LengHead += 4;
            }
            if ( LengBody <= 125 )
            {
                 BitLeng = 
				 	(int)LengBody;
			}
            else if ( LengBody <= 65556 )
            {
                	BitLeng = 126;
                	LengHead += 2;
            }
            else if ( LengBody >= 65557 )
            {
                	BitLeng = 127;
                	LengHead += 8;
            }
			
            D__Head = new MemoryStream((int)LengHead);
			D__Head.WriteByte((byte)( BitFin  << 7 ));
			D__Head.WriteByte((byte)( BitRsv1 << 6 ));
			D__Head.WriteByte((byte)( BitRsv2 << 5 ));
			D__Head.WriteByte((byte)( BitRsv3 << 4 ));
			D__Head.WriteByte((byte)( BitPcod << 0 ));

			D__Head.WriteByte((byte)( BitMask << 7 ));
			D__Head.WriteByte((byte)( BitLeng << 0 ));

			if (BitLeng == 127)
			{
				D__Head.WriteByte((byte)(LengBody >> 56));
				D__Head.WriteByte((byte)(LengBody >> 48));
				D__Head.WriteByte((byte)(LengBody >> 40));
				D__Head.WriteByte((byte)(LengBody >> 32));
				D__Head.WriteByte((byte)(LengBody >> 24));
				D__Head.WriteByte((byte)(LengBody >> 16));
			}
			if (BitLeng >= 126)
			{
				D__Head.WriteByte((byte)(LengBody >> 08));
				D__Head.WriteByte((byte)(LengBody >> 00));
			}
		}
		public void Encoding()
		{
			if ( BitMask == 1 && MaskVal == 0)
			{
				MaskVal = 
    	                new Random().Next();

				D__Mask = new byte[ 4 ];
				D__Mask[0] = (byte)(MaskVal >> 24);
				D__Mask[1] = (byte)(MaskVal >> 16);
				D__Mask[2] = (byte)(MaskVal >> 08);
				D__Mask[3] = (byte)(MaskVal >> 00);

                int lenght    = (int)
                                D__Body.Length;
                int offset    = (int)
                                D__Body.Position;
				byte[] buffer = D__Body.GetBuffer();

                for (int i = offset; i < lenght; i++)
				{
                    buffer[offset] = (byte)
                                     (buffer[offset] ^ D__Mask[PartBody++ % 4]);
				}
			}
		}
		public override string ToString()
		{
			return "This WebSocket protocol release №13. Supported version №13";
		}
	}
}
