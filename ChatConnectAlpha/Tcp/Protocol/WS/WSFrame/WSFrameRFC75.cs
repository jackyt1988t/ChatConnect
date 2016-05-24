

namespace ChatConnect.Tcp.Protocol.WS
{
	class WSFrameSample
	{
		/// <summary>
		/// Text опкод
		/// </summary>
		public const int TEXT = 0x04;
		/// <summary>
		/// Ping опкод
		/// </summary>
		public const int PING = 0x02;
		/// <summary>
		/// Pong опкод
		/// </summary>
		public const int PONG = 0x03;
		/// <summary>
		/// close опкод
		/// </summary>
		public const int CLOSE = 0x01;
		/// <summary>
		/// Binary опкод
		/// </summary>
		public const int BINNARY = 0x04;
		public const int CONTINUE = 0x05;
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
		/// бит RSV4
		/// </summary>
		public int BitRsv4
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
		public int BitExtn
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
		/// 
		/// </summary>
		public long PartExtn
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
		/// 
		/// </summary>
		public long LengExtn
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
		/// длинна тела
		/// </summary>
		public long LengData
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
		/// 
		/// </summary>
		public byte[] DataExtn
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
			BitFin = 0;
			Handler = 0;			
			BitRsv1 = 0;
			BitRsv2 = 0;
			BitRsv3 = 0;
			BitPcod = 0;
			BitExtn = 0;
			BitLeng = 0;
			PartBody = 0;
			PartHead = 0;
			LengHead = 0;
			LengBody = 0;
			DataHead = null;
			DataBody = null;
			GetsHead = false;
			GetsBody = false;
		}
		public void SetHeader()
		{
			if (this.SetsHead)
				return;

			this.LengHead = 2;
			this.SetsHead = true;
			this.LengData = this.LengExtn 
				      + this.LengBody;
			if ((this.LengData) <= 125)
			{
				this.BitLeng = (int)this.LengBody;
			}
			else if ((this.LengData) <= 65556)
			{
				this.BitLeng = 126;
				this.LengHead += 2;
			}
			else if ((this.LengData) >= 65557)
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
													 (this.BitPcod));
			length++;

			this.DataHead[length] = (byte)(this.BitRsv4 << 7);
			this.DataHead[length] = (byte)(this.DataHead[length] |
													 (this.BitLeng));
			length++;

			if (this.BitLeng == 127)
			{
				this.DataHead[length] = (byte)(this.LengData >> 56);
				length++;
				this.DataHead[length] = (byte)(this.LengData >> 48);
				length++;
				this.DataHead[length] = (byte)(this.LengData >> 40);
				length++;
				this.DataHead[length] = (byte)(this.LengData >> 32);
				length++;
				this.DataHead[length] = (byte)(this.LengData >> 24);
				length++;
				this.DataHead[length] = (byte)(this.LengData >> 16);
				length++;
			}
			if (this.BitLeng >= 126)
			{
				this.DataHead[length] = (byte)(this.LengData >> 08);
				length++;
				this.DataHead[length] = (byte)(this.LengData >> 00);
				length++;
			}
		}
		public byte[] GetDataFrame()
		{
			SetHeader();

			byte[] buffer = new byte[ this.LengHead + this.LengData ];

				this.DataHead.CopyTo(buffer, 0);
				if (this.LengExtn > 0)
					this.DataExtn.CopyTo(buffer, this.LengHead);
				if (this.LengBody > 0)
					this.DataBody.CopyTo(buffer, this.LengExtn);

			return buffer;
		}
	}
}
