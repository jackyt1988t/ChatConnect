using System;
using System.IO;
using MyWebSocket.Tcp.Protocol.WS.WS_13;

namespace MyWebSocket.Tcp.Protocol.WS
{
	public class WSWriterN13 : Stream
	{
		
		public Stream Stream;
		public WSFramesN13 __Frame;

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

		public WSWriterN13(Stream stream) : 
			base()
		{
			Stream = stream;
			__Frame = new WSFramesN13();
		}


		public void Write(WSFrameN13 _frame)
		{
			int lenght;
			int offset;
			byte[] buffer;

            Log.Loging.AddMessage(
                "Попытка добавить WS данные", "log.log", Log.Log.Info);

			_frame.InitData();
			if (_frame.BitMask == 1)
				_frame.Encoding();

			if (__Frame.Opcod == WSOpcod.None)
			{
				if (_frame.BitFin == 1)
					__Frame.isEnd = true;
				
                __Frame.Opcod = WSFrameN13.Convert(_frame.BitPcod);
			}
			else
			{
				if (_frame.BitFin == 1)
				{
					if (!__Frame.isEnd)
						 __Frame.isEnd = true;
					else
						throw new WSException("Неправильный фрейм");
				}
				if (_frame.BitPcod != WSFrameN13.CONTINUE)
						throw new WSException("Неправильный опкод");
			}

            lenght = (int)
                     _frame.LengHead;
			offset = 0;
            buffer = _frame.D__Head.GetBuffer();
			Stream.Write(buffer, offset, lenght);

            lenght = (int)
                     _frame.D__Body.Length;
            offset = (int)
                     _frame.D__Body.Position;
            buffer = _frame.D__Body.GetBuffer();

			Stream.Write(buffer, offset, lenght);

				lock (__Frame)
				{
					// Добавить фрейм в коллекцию
					__Frame.__Frames.Add(_frame);

					if (Log.Loging.Mode  >  Log.Log.Info)
                        Log.Loging.AddMessage(
                            "WS данные успешно добавлены", "log.log", Log.Log.Info);
                    else
                        Log.Loging.AddMessage(
                            "WS данные успешно добавлены" +
                            "\r\n" + WSDebug.DebugN13(_frame), "log.log", Log.Log.Info);
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
	}
}
