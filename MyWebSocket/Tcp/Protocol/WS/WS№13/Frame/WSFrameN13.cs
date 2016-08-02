using System;
using System.IO;
using System.Text;

namespace MyWebSocket.Tcp.Protocol.WS
{
    /// <summary>
    /// Класс WSFrameN13
    /// </summary>
    public class WSFrameN13
    {
        #region const
        /// <summary>
        /// Фрейм TEXT
        /// </summary>
        public const int TEXT     = 0x01;
        /// <summary>
        /// Фрейм PING
        /// </summary>
        public const int PING     = 0x09;
        /// <summary>
        /// Фрейм PONG
        /// </summary>
        public const int PONG     = 0x0A;
        /// <summary>
        /// Фрейм CLOSE
        /// </summary>
        public const int CLOSE    = 0x08;
        /// <summary>
        /// Фрейм BINNARY
        /// </summary>
        public const int BINNARY  = 0x02;
        /// <summary>
        /// Фрейм CONTINUE
        /// </summary>
        public const int CONTINUE = 0x00;
        #endregion

        #region properties
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
		public bool Set_Head
		{
			get;
			private set;
		}
		/// <summary>
		/// Если заголвоки получены true
		/// </summary>
        public bool Get_Head
        {
            get;
            set;
        }
		/// <summary>
		/// Если тело получено true
		/// </summary>
        public bool Get_Body
        {
            get;
            set;
        }
        /// <summary>
        /// Текущая позиция при (де)кодирования
        /// </summary>
        /// <value>The position.</value>
        public long Position
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
		public byte[] Raw_Mask
		{
			get;
			set;
		}
        /// <summary>
        /// Буффер тела
        /// </summary>
        public MemoryStream Raw_Body
        {
            get;
            private set;
        }
		/// <summary>
		/// Буффер заголвоков
		/// </summary>
		public MemoryStream Raw_Head
        {
            get;
            private set;
        }

        private bool _decode = false;
        private bool _encode = false;
        #endregion

        #region constructor
        /// <summary>
        /// Создает экземпляр класса WSFrameN13
        /// </summary>
        public WSFrameN13()
        {
            Raw_Head = new MemoryStream(0);
            Raw_Body = new MemoryStream(0);
        }
        /// <summary>
        /// Создает экземпляр класса WSFrameN13
        /// </summary>
        /// <param name="length">Длинна Raw_Body потока данных</param>
		public WSFrameN13(int length)
		{
			Raw_Head = new MemoryStream(0);
			Raw_Body = new MemoryStream(length);
		}
        /// <summary>
        /// Создает экземпляр класса WSFrameN13
        /// </summary>
        /// <param name="buffer">Буффер Raw_Body потока данных</param>
        /// <param name="offset">Начальная позиция</param>
        /// <param name="length">Количество инициализированных байт</param>
        public WSFrameN13(byte[] buffer, int offset, int length)
        {
            Raw_Head = new MemoryStream(0);
            Raw_Body = new MemoryStream(buffer,
                                           offset,
                                               length,
                                                   true,
                                                       true);
        }
        #endregion

        #region all methods
        /// <summary>
        /// Конвертирует число в WSOpcod
        /// </summary>
        /// <param name="ws_opcod">WSOpcod</param>
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
        /// <summary>
        /// Иницмализирует заголвки сообщения
        /// </summary>
		public void InitData()
		{
			if (Set_Head)
				return;
			else
				Set_Head = true;

            Log.Loging.AddMessage(
                "Попытка установить WS заголвоки фрейма", "log.log", Log.Log.Info);

            LengHead = 2;
			LengBody =
				Raw_Body.Length;

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

            Raw_Head = new MemoryStream((int)LengHead);
            Raw_Head.WriteByte((byte)(( BitFin  << 7 ) | 
                                      ( BitRsv1 << 6 ) |
			                          ( BitRsv2 << 5 ) |
			                          ( BitRsv3 << 4 ) |
                                      ( BitPcod << 0 )));

			Raw_Head.WriteByte((byte)(( BitMask << 7 ) |
                                      ( BitLeng << 0 )));

			if (BitLeng == 127)
			{
				Raw_Head.WriteByte((byte)(LengBody >> 56));
				Raw_Head.WriteByte((byte)(LengBody >> 48));
				Raw_Head.WriteByte((byte)(LengBody >> 40));
				Raw_Head.WriteByte((byte)(LengBody >> 32));
				Raw_Head.WriteByte((byte)(LengBody >> 24));
				Raw_Head.WriteByte((byte)(LengBody >> 16));
			}
			if (BitLeng >= 126)
			{
				Raw_Head.WriteByte((byte)(LengBody >> 08));
				Raw_Head.WriteByte((byte)(LengBody >> 00));
			}

            Log.Loging.AddMessage(
                "Заголвоки WS фрейма успешно установлены", "log.log", Log.Log.Info);
		}
        /// <summary>
        /// Кодирует тело сообщения в соотвествии с маской
        /// </summary>
		public void Encoding()
		{
            if ( BitMask == 1 && !_encode)
			{
                _encode = true;
                Log.Loging.AddMessage(
                    "Попвтка закодировать фрейм сообщения", "log.log", Log.Log.Info);

				MaskVal =
    	                new Random().Next();

				Raw_Mask = new byte[ 4 ];
				Raw_Mask[0] = (byte)(MaskVal >> 24);
				Raw_Mask[1] = (byte)(MaskVal >> 16);
				Raw_Mask[2] = (byte)(MaskVal >> 08);
				Raw_Mask[3] = (byte)(MaskVal >> 00);

                Raw_Head.Write(   Raw_Mask, 0, 4   );

                int lenght    = 
                           (int)Raw_Body.Length;
				byte[] buffer = Raw_Body.GetBuffer();

                for (int offset = 0; offset < lenght; offset++)
				{
                    buffer[offset] = (byte)(buffer[offset] ^ Raw_Mask[Position++ % 4]);
				}

                Log.Loging.AddMessage(
                    "Маска тела WS фрейма установлена" +
                    "Данные тела сообщения успещно зашифрованы", "log.log", Log.Log.Info);
			}
		}
        /// <summary>
        /// Расшифровывает тело сообщения в сотвествии с маской
        /// </summary>
        public void Decoding()
        {
            if (BitMask == 1 && !_decode)
            {
                _decode = true;
                Log.Loging.AddMessage(
                    "Попвтка раскодировать фрейм сообщения", "log.log", Log.Log.Info);

                Raw_Head.Write(   Raw_Mask, 0, 4   );

                int lenght    = 
                           (int)Raw_Body.Length;
                byte[] buffer = Raw_Body.GetBuffer();

                for (int offset = 0; offset < lenght; offset++)
                {
                    buffer[offset] = (byte)(buffer[offset] ^ Raw_Mask[Position++ % 4]);
                }

                Log.Loging.AddMessage(
                    "Данные тела сообщения успещно расшифрованы", "log.log", Log.Log.Info);
            }
        }
        /// <summary>
        /// Строковое предстовление текщуего экземпляра класса
        /// </summary>
        /// <returns>строковое представление</returns>
		public override string ToString()
		{
			return "This WebSocket protocol release №13. Supported version №13";
		}
        #endregion
	}
}
