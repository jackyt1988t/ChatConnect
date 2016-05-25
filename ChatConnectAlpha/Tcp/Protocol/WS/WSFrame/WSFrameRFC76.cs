using System;

namespace ChatConnect.Tcp.Protocol.WS
{
	enum Bit : int
	{
		Set = 0,
		All = 2,
		Unset = 1,
	}
    class WSFrameN13
    {
        public const int TEXT   = 0x01;
		public const int PING   = 0x09;
		public const int PONG   = 0x0A;
		public const int CLOSE  = 0x08;
		public const int BINNARY = 0x02;
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
		/// <summary>
		/// Сбрасывает все перменный в стандартные значения
		/// </summary>
        public void Clear()
        {
            Handler = 0;
            MaskPos = 0;
            BitFin = 0;
            BitRsv1 = 0;
            BitRsv2 = 0;
            BitRsv3 = 0;
            BitPcod = 0;
            BitMask = 0;
            BitLeng = 0;
            MaskVal = 0;
            PartBody = 0;
            PartHead = 0;
            LengHead = 0;
            LengBody = 0;
            DataHead = null;
            DataBody = null;
			SetsHead = false;
            GetsHead = false;
            GetsBody = false;
        }
		public void SetHeader()
		{
			if (this.SetsHead)
				return;
						
            this.LengHead = 2;
			this.SetsHead = true;
			if (this.BitMask == 1)
            {
				this.MaskVal = new Random().Next();
                this.LengHead += 4;
            }
            if (this.LengBody <= 125)
            {
                 this.BitLeng = (int)this.LengBody;
			}
            else if (this.LengBody <= 65556)
            {
                this.BitLeng = 126;
                this.LengHead += 2;
            }
            else if (this.LengBody >= 65557)
            {
                this.BitLeng = 127;
                this.LengHead += 8;
            }
			
            int length = 0;
            this.DataHead = new byte[this.LengHead];

            this.DataHead[length] = (byte)(this.BitFin << 7);
            this.DataHead[length] = (byte)(this.DataHead[length] | 
											  (this.BitRsv1 << 6));
            this.DataHead[length] = (byte)(this.DataHead[length] | 
											  (this.BitRsv2 << 5));
            this.DataHead[length] = (byte)(this.DataHead[length] | 
											  (this.BitRsv3 << 4));
            this.DataHead[length] = (byte)(this.DataHead[length] | 
											  (this.BitPcod << 0));
				length++;

            this.DataHead[length] = (byte)(this.BitMask << 7);
            this.DataHead[length] = (byte)(this.DataHead[length] | 
											  (this.BitLeng << 0));
				length++;

			if (this.BitLeng == 127)
			{
				this.DataHead[length] = (byte)(this.LengBody >> 56);
				length++;
				this.DataHead[length] = (byte)(this.LengBody >> 48);
				length++;
				this.DataHead[length] = (byte)(this.LengBody >> 40);
				length++;
				this.DataHead[length] = (byte)(this.LengBody >> 32);
				length++;
				this.DataHead[length] = (byte)(this.LengBody >> 24);
				length++;
				this.DataHead[length] = (byte)(this.LengBody >> 16);
				length++;
			}
			if (this.BitLeng >= 126)
			{
				this.DataHead[length] = (byte)(this.LengBody >> 08);
				length++;
				this.DataHead[length] = (byte)(this.LengBody >> 00);
				length++;
			}

			if (this.BitMask == 1)
			{
				this.DataHead[length] = (byte)(this.MaskVal >> 24);
				length++;
				this.DataHead[length] = (byte)(this.MaskVal >> 16);
				length++;
				this.DataHead[length] = (byte)(this.MaskVal >> 08);
				length++;
				this.DataHead[length] = (byte)(this.MaskVal >> 00);
				length++;
			}
		}
		public byte[] GetDataFrame()
		{
			SetHeader();

			byte[] buffer = new byte[ this.LengHead + this.LengBody ];

			this.DataHead.CopyTo(buffer, 0);
			this.DataBody.CopyTo(buffer, this.LengHead );

			return buffer;
		}
		public override string ToString()
		{
			return "RFC76";
		}
	}
}
