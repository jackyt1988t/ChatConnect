using System;
using System.IO;
using MyWebSocket.Tcp.Protocol.WS.WS_13;

namespace MyWebSocket.Tcp.Protocol.WS
{
    /// <summary>
    /// Класс WSWriterN13
    /// </summary>
	public class WSWriterN13 : Stream
	{
        #region properties
        /// <summary>
        /// Основной поток
        /// </summary>
        internal Stream Stream;
        /// <summary>
        /// Коллекция прочитанных(форматированное сообщение) фреймов
        /// </summary>
        internal WSFramesN13 __Frames;
        #endregion

        #region properties Stream
		/// <summary>
		/// Возвращает длинну базового потока
		/// </summary>
		public override long Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		/// <summary>
		/// Не поддерживается данной реализацией
		/// </summary>
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}
		/// <summary>
		/// Указвает возможность чтения в базовый поток
		/// </summary>
		public override bool CanRead
		{
			get
			{
				return false;
			}
		}
		/// <summary>
		/// Указвает возможность записи в базовый поток
		/// </summary>
		public override bool CanWrite
		{
			get
			{
				return true;
			}
		}
		/// <summary>
		/// Не поддерживается данной реализацией
		/// </summary>
		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}

			set
			{
				throw new NotImplementedException();
			}
		}
        #endregion

        #region constructor
        /// <summary>
        /// Создает экземпляр класса WSWriterN13
        /// </summary>
        /// <param name="stream">Основной поток записи данных</param>
		public WSWriterN13(Stream stream) : 
			base()
		{
			Stream = stream;
			__Frames = new WSFramesN13();
		}
        #endregion

        #region all methods
        /// <summary>
        /// Записывает форматированные данные в поток
        /// </summary>
        /// <param name="__Frame">WS фрейм для инициализации данных</param>
		public void Write(WSFrameN13 __Frame)
		{
			int lenght;
			int offset;
			byte[] buffer;

            Log.Loging.AddMessage(
                "Попытка добавить WS данные", "log.log", Log.Log.Info);

			__Frame.InitData();
			if (__Frame.BitMask == 1)
				__Frame.Encoding();

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
                
			offset = 0;
            lenght = (int)
                     __Frame.LengHead;
            buffer = __Frame.Raw_Head.GetBuffer();
			Stream.Write( buffer, offset, lenght );

            offset = 0;
            lenght = (int)
                     __Frame.Raw_Body.Length;
            buffer = __Frame.Raw_Body.GetBuffer();

			Stream.Write( buffer, offset, lenght );

				lock (__Frames)
				{
					// Добавить фрейм в коллекцию
					__Frames.__Frames.Add(__Frame);

					if (Log.Loging.Mode  >  Log.Log.Info)
                        Log.Loging.AddMessage(
                            "WS данные успешно добавлены", "log.log", Log.Log.Info);
                    else
                        Log.Loging.AddMessage(
                            "WS данные успешно добавлены" +
                            "\r\n" + WSDebug.DebugN13(__Frame), "log.log", Log.Log.Info);
				}
		}
		/// <summary>
		/// Не поддерживается данной реализацией
		/// </summary>
		public override void Flush()
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Не поддерживается данной реализацией
		/// </summary>
		/// <param name="value">...</param>
		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Не поддерживается данной реализацией
		/// </summary>
		/// <param name="offset">...</param>
		/// <param name="origin">...</param>
		/// <returns>...</returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException("Seek");
		}
		/// <summary>
		/// Не поддерживается данной реализацией
		/// </summary>
		/// <param name="buffer">...</param>
		/// <param name="offset">...</param>
		/// <param name="length">...</param>
		/// <returns>...</returns>
		public override int Read(byte[] buffer, int offset, int length)
		{
			throw new NotImplementedException("Read");
		}
		/// <summary>
		/// Записывает сырые данные в основной поток.
		/// Данные не будут обработаны в соответствии с текущим протоколом WS 
		/// </summary>
		/// <param name="buffer">массив данных</param>
		/// <param name="offset">начальная позиция</param>
		/// <param name="length">количество для записи</param>
		public override void Write(byte[] buffer, int offset, int length)
		{
			Stream.Write(  buffer, offset, length  );
		}
        #endregion
	}
}
