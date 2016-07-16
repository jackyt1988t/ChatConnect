using System;
using System.IO;

namespace MyWebSocket.Tcp.Protocol.WS
{
	class WSWriterN13 : Stream
	{
		public static int MINRESIZE = 0;
		public static int MAXRESIZE = 512000;
		
		public Stream Stream;
		public WSFrameN13 __Frame;

		/// <summary>
		/// Возвращает длинну базового потока
		/// </summary>
		public override long Length
		{
			get
			{
				return Stream.Length;
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

		public WSWriterN13(Stream stream, WSFrameN13 frame) : 
			base()
		{
			Stream = stream;
			__Frame = frame;
		}

		public void Write(WSFrameN13 frame)
		{
			(__Frame = 
				frame).InitData();
			
				Write(frame.DataHead);
				Write(frame.DataBody);
		}
		public void Write(byte[] buffer)
		{
			Write(buffer, 0, buffer.Length);
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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}
		/// <summary>
		/// Записывает указанный массив данных в поток.
		/// </summary>
		/// <param name="buffer">массив данных для записи</param>
		/// <param name="offset">начальная позиция</param>
		/// <param name="length">количество для записи</param>
		public override void Write(byte[] buffer, int offset, int length)
		{
			Stream.Write(buffer, offset, length);
		}
	}
}
