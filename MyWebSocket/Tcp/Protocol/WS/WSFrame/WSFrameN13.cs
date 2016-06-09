using System;

namespace MyWebSocket.Tcp.Protocol.WS
{
    class WSFrameN13
    {
        public const int TEXT     = 0x01;
		public const int PING     = 0x09;
		public const int PONG     = 0x0A;
		public const int CLOSE    = 0x08;
		public const int BINNARY  = 0x02;
		public const int CONTINUE = 0x00;
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
		/// бит FIN 
		/// </summary>
        public int BitFin
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
		public bool SetsHead
		{
			get;
			private set;
		}
		/// <summary>
		/// Если заголвоки получены true
		/// </summary>
        public bool GetsHead
        {
            get;
            set;
        }
		/// <summary>
		/// Если тело получено true
		/// </summary>
        public bool GetsBody
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
		public byte[] DataMask
		{
			get;
			set;
		}
		/// <summary>
		/// Буффер заголвоков
		/// </summary>
		public byte[] DataHead
        {
            get;
            set;
        }
		/// <summary>
		/// Буффер тела
		/// </summary>
        public byte[] DataBody
        {
            get;
            set;
        }

		public void Null()
		{
			BitFin = 0;
			Handler = 0;
			BitRsv1 = 0;
			BitRsv2 = 0;
			BitRsv3 = 0;
			BitPcod = 0;
			BitMask = 0;
			BitLeng = 0;
			RecLeng = 0;
			MaskVal = 0;
			PartBody = 0;
			PartHead = 0;
			LengHead = 0;
			LengBody = 0;
			DataHead = null;
			DataBody = null;
			DataMask = null;
			GetsHead = false;
			GetsBody = false;
			SetsHead = false;
		}
		unsafe public void InitializationHeader()
		{
			if (SetsHead)
				return;
						
            LengHead = 2;
			SetsHead = true;
			if (BitMask == 1)
            {
				MaskVal = new Random().Next();
                LengHead += 4;
            }
            if (LengBody <= 125)
            {
                 BitLeng = (int)LengBody;
			}
            else if (LengBody <= 65556)
            {
                BitLeng = 126;
                LengHead += 2;
            }
            else if (LengBody >= 65557)
            {
                BitLeng = 127;
                LengHead += 8;
            }
			
            int length = 0;
            DataHead = new byte[LengHead];

            DataHead[length] = (byte)(BitFin << 7);
            DataHead[length] = (byte)(DataHead[length] | 
										(BitRsv1 << 6));
            DataHead[length] = (byte)(DataHead[length] | 
										(BitRsv2 << 5));
            DataHead[length] = (byte)(DataHead[length] | 
										(BitRsv3 << 4));
            DataHead[length] = (byte)(DataHead[length] | 
										(BitPcod << 0));
				length++;

            DataHead[length] = (byte)(BitMask << 7);
            DataHead[length] = (byte)(DataHead[length] | 
										(BitLeng << 0));
				length++;

			if (BitLeng == 127)
			{
				DataHead[length] = (byte)(LengBody >> 56);
				length++;
				DataHead[length] = (byte)(LengBody >> 48);
				length++;
				DataHead[length] = (byte)(LengBody >> 40);
				length++;
				DataHead[length] = (byte)(LengBody >> 32);
				length++;
				DataHead[length] = (byte)(LengBody >> 24);
				length++;
				DataHead[length] = (byte)(LengBody >> 16);
				length++;
			}
			if (BitLeng >= 126)
			{
				DataHead[length] = (byte)(LengBody >> 08);
				length++;
				DataHead[length] = (byte)(LengBody >> 00);
				length++;
			}

			if (BitMask == 1)
			{
				DataMask = new byte[4];
				DataMask[0] = DataHead[length] = (byte)(MaskVal >> 24);
				length++;
				DataMask[1] = DataHead[length] = (byte)(MaskVal >> 16);
				length++;
				DataMask[2] = DataHead[length] = (byte)(MaskVal >> 08);
				length++;
				DataMask[3] = DataHead[length] = (byte)(MaskVal >> 00);
				length++;

				fixed (byte* target = DataBody)
				{
					byte* pt = target + PartBody;
						while (RecLeng < LengBody)
						{
							*pt = (byte)(*pt ^ DataMask[RecLeng % 4]);
							pt++;
							RecLeng++;
						}
				}
			}
		}
		public override string ToString()
		{
			return "WebSocket protocol version release N13";
		}
	}
}
