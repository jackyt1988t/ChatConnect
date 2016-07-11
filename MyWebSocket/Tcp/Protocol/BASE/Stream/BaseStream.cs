using System;
using System.IO;


namespace MyWebSocket.Tcp.Protocol
{
	public class TcpStream : Stream
	{
		/// <summary>
		/// Поток записи в сокет
		/// </summary>
		public MyStream Writer;
		/// <summary>
		/// Поток чтения из сокета
		/// </summary>
		public MyStream Reader;
		/// <summary>
		/// Создает новй экземпляр потока
		/// </summary>
		public TcpStream()
		{
			Writer = new MyStream(0);
			Reader = new MyStream(10000);
		}

		/// <summary>
		/// Количество байт доступных для чтения
		/// </summary
		public override long Length
		{
			get
			{
				return Reader.Length;
			}
		}
		
		/// <summary>
		/// Указвает возможность чтения в базовый поток
		/// </summary>
		public override bool CanRead
		{
			get
			{
				return true;
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
		/// Читает данные из буффера которые были записаны из сокета
		/// </summary>
		/// <param name="buffer">>массив данных для записи</param>
		/// <param name="offset">начальная позиция</param>
		/// <param name="length">количество для чтения</param>
		/// <returns>количетсво прочитанных байт</returns>
		public override int Read(byte[] buffer, int offset, int length)
		{
			return Reader.Read(buffer, offset, length);
		}
		/// <summary>
		/// Записывает данные в кольцевой поток для последующей записи в сокет
		/// </summary>
		/// <param name="buffer">>массив данных для записи</param>
		/// <param name="offset">начальная позиция</param>
		/// <param name="length">количество для чтения</param>
		public override void Write(byte[] buffer, int offset, int length)
		{
				   Writer.Write(buffer, offset, length);
		}
	}
}
