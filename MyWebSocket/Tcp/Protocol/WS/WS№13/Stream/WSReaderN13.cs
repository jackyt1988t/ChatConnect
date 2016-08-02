using System;
using System.IO;
using MyWebSocket.Tcp.Protocol.WS.WS_13;

namespace MyWebSocket.Tcp.Protocol.WS
{
    /// <summary>
    /// Класс WSReaderN13
    /// </summary>
    public class WSReaderN13
    {
        #region properties
        /// <summary>
        /// Основной поток
        /// </summary>
        internal Stream Stream;
        /// <summary>
        /// Последний прочитаный фрейм
        /// </summary>
        internal WSFrameN13 __Frame;
        /// <summary>
        /// Коллекция прочитанных(форматированное сообщение) фреймов
        /// </summary>
        internal WSFramesN13 __Frames;
        #endregion

        #region constructor
        /// <summary>
        /// Создает экземпляр класса WSReaderN13
        /// </summary>
        /// <param name="stream">Основной поток записи данных</param>
		public WSReaderN13(Stream stream) :
			base()
        {
			Stream = stream;
            __Frame = new WSFrameN13();
            __Frames = new WSFramesN13();
        }
        #endregion

        #region all methods
        /// <summary>
        /// Читает тело WS фрейма
        /// </summary>
        /// <returns><c>true</c>тело проситано</returns>
		public bool ReadBody()
        {
            int lenght =
                (int)(__Frame.LengBody -
                      __Frame.PartBody);
            int offset =
                (int)(__Frame.PartBody);
			byte[] buffer =
                     __Frame.Raw_Body.GetBuffer();

            __Frame.PartBody += Stream.Read(buffer,
											    offset,
												    lenght);

            // Если true тело сообщение успешно получено...
			if (__Frame.PartBody    ==    __Frame.LengBody)
			{
                __Frame.Decoding();

                lock (  __Frames  )
                {
                    // Добавить фрейм в коллекцию
                    __Frames.__Frames.Add(__Frame);

                    if (Log.Loging.Mode  >  Log.Log.Info)
                        Log.Loging.AddMessage(
                            "WS данные успешно получены", "log.log", Log.Log.Info);
                    else
                        Log.Loging.AddMessage(
                            "WS данные успешно получены" +
                            "\r\n" + WSDebug.DebugN13(__Frame), "log.log", Log.Log.Info);
                }

				return (__Frame.Get_Body = true);
			}
            return false;
        }
        /// <summary>
        /// Читает Загловоки WS фрейма
        /// </summary>
        /// <returns><c>true</c>если заголвки прочитаны</returns>
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
						__Frame.Raw_Mask = new byte[4];
						__Frame.Raw_Mask[0] = (byte)@char;
                        break;
                    case 11:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;

						__Frame.MaskVal = __Frame.MaskVal | (@char << 16);
						__Frame.Raw_Mask[1] = (byte)@char;
                        break;
                    case 12:
                        /*  Обработчик.  */
                        __Frame.Handler += 1;
						__Frame.MaskVal = __Frame.MaskVal | (@char << 08);
						__Frame.Raw_Mask[2] = (byte)@char;
                        break;
                    case 13:
						__Frame.MaskVal = __Frame.MaskVal | (@char << 00);
						__Frame.Raw_Mask[3] = (byte)@char;
                        break;
                }

                __Frame.PartHead++;
                __Frame.Raw_Head.WriteByte( (byte)@char );

                // Если true заг-ки сообщения успешно получены.
                if (__Frame.PartHead    ==    __Frame.LengHead)
                {
                    if (__Frames.Opcod == WSOpcod.None)
                    {
                        if (__Frame.BitFin == 1)
                            __Frames.isEnd = true;

                        __Frames.Opcod = 
                                WSFrameN13.Convert(__Frame.BitPcod);
                    }
                    else
                    {
                        if (__Frame.BitFin == 1)
                        {
                            if (!__Frames.isEnd)
                                 __Frames.isEnd = true;
                            else
                                throw new WSException("Неправильный фрейм");
                        }
                        if (__Frame.BitPcod != WSFrameN13.CONTINUE)
                                throw new WSException("Неправильный опкод");
                    }
						if (__Frame.LengBody > 0)
							__Frame.Raw_Body.SetLength(  __Frame.LengBody  );

					return (__Frame.Get_Head = true);
                }
            }
            return false;
        }
        #endregion
    }
}
