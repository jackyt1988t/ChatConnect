

namespace MyWebSocket.Tcp.Protocol.WS
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
		public const int BINNARY = 0x05;
		/// <summary>
		/// Continue опкод
		/// </summary>
		public const int CONTINUE = 0x00;
		/// <summary>
		/// бит More 
		/// </summary>
		public int BitMore
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
		public void Null()
		{
			BitMore = 0;
			Handler = 0;			
			BitRsv1 = 0;
			BitRsv2 = 0;
			BitRsv3 = 0;
			BitPcod = 0;
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
		public void InitializationHeader()
		{
			if (SetsHead)
				return;

			LengHead = 2;
			SetsHead = true;
			if ((LengData) <= 125)
			{
				BitLeng = 
				(int)LengBody;
			}
			else if (LengData <= 65556)
			{
				BitLeng = 126;
				LengHead += 2;
			}
			else if (LengData >= 65557)
			{
				BitLeng = 127;
				LengHead += 8;
			}

			int length = 0;
			DataHead = new byte[LengHead];

			DataHead[length] = (byte)(BitMore << 7);
			DataHead[length] = (byte)(DataHead[length] |
										(BitRsv1 << 6));
			DataHead[length] = (byte)(DataHead[length] |
										(BitRsv2 << 5));
			DataHead[length] = (byte)(DataHead[length] |
										(BitRsv3 << 4));
			DataHead[length] = (byte)(DataHead[length] |
									    (BitPcod << 0));
			length++;

			DataHead[length] = (byte)(BitRsv4 << 7);
			DataHead[length] = (byte)(DataHead[length] |
										(BitLeng << 0));
			length++;

			if (BitLeng == 127)
			{
				DataHead[length] = (byte)(LengData >> 56);
				length++;
				DataHead[length] = (byte)(LengData >> 48);
				length++;
				DataHead[length] = (byte)(LengData >> 40);
				length++;
				DataHead[length] = (byte)(LengData >> 32);
				length++;
				DataHead[length] = (byte)(LengData >> 24);
				length++;
				DataHead[length] = (byte)(LengData >> 16);
				length++;
			}
			if (BitLeng >= 126)
			{
				DataHead[length] = (byte)(LengData >> 08);
				length++;
				DataHead[length] = (byte)(LengData >> 00);
				length++;
			}
		}
	}
}
