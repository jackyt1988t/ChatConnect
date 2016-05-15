

namespace ChatConnect.Tcp.Protocol.WS
{
	struct WSFrameRFC75
	{
		/// <summary>
		/// Text опкод
		/// </summary>
		public const int TEXT = 0x01;
		/// <summary>
		/// Ping опкод
		/// </summary>
		public const int PING = 0x09;
		/// <summary>
		/// Pong опкод
		/// </summary>
		public const int PONG = 0x0A;
		/// <summary>
		/// close опкод
		/// </summary>
		public const int CLOSE = 0x08;
		/// <summary>
		/// Binary опкод
		/// </summary>
		public const int BINARY = 0x02;
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
		public int BitFind
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
			BitFind = 0;
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
			GetsHead = false;
			GetsBody = false;
		}
	}
}
