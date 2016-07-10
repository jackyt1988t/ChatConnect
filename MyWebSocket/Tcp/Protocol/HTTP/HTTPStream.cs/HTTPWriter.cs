using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	/// <summary>
	/// Записывет данные в поток
	/// </summary>
	public class HTTPWriter : Stream
	{
		private static readonly byte[] ENDCHUNCK;
		private static readonly byte[] EOFCHUNCK;

		/// <summary>
		/// Заголвоки запрос
		/// </summary>
		public Header Header;
		/// <summary>
		/// 
		/// </summary>
		public Stream Stream;
		/// <summary>
		/// Информация о записи
		/// </summary>
		public HTTPFrame __Frame;


		/// <summary>
		/// Возвращает длинну базового потока
		/// </summary>
		public override long Length
		{
			get
			{
				Stream.Length;
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



		static HTTPWriter()
		{
			ENDCHUNCK =
				new byte[] { 0x0D, 0x0A };
			EOFCHUNCK =
				new byte[] { 0x30, 0x0D, 0x0A, 0x0D, 0x0A };
		}
		/// <summary>
		/// Создает поток
		/// </summary>
		/// <param name="stream">кольцевой поток</param>
		public HTTPWriter(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException( "stream" );
			
			Stream = stream;
			__Frame = new HTTPFrame();
		}
		/// <summary>
		/// Записывает CRLF в поток
		/// </summary>
		public void End()
		{
			Stream.Write(ENDCHUNCK, 0, 2);
		}
		/// <summary>
		/// Записывает 0CRLFCRLF в поток
		/// </summary>
		public void Eof()
		{
			Stream.Write(EOFCHUNCK, 0, 5);
		}
		/// <summary>
		/// Записывет указанную строку в поток.
		/// Строка об-ся в соответствии с установленными заголвоками
		/// </summary>
		/// <param name="str">строка для записи</param>
		public void Write(string str)
		{
			Write(Encoding.UTF8.GetBytes(str));
		}
		/// <summary>
		/// Записывает указанный массив данных в поток.
		/// Строка об-ся в соответствии с установленными заголвоками.
		/// </summary>
		/// <param name="buffer">массив данных для записи</param>
		public void Write(byte[] buffer)
		{
			Stream.Write(buffer, 0, buffer.Length);
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
		public override void SetLength(  long value  )
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
		/// Строка об-ся в соответствии с установленными заголвоками.
		/// Отправляет http заголвоки есл они еще не были отправлены.
		/// </summary>
		/// <param name="buffer">массив данных для записи</param>
		/// <param name="offset">начальная позиция</param>
		/// <param name="length">количество для записи</param>
		public override void Write(byte[] buffer, int offset, int length)
		{
			__Frame.Handl++;
			if (!Header.IsRes)
			{
							  Header.SetRes();
				byte[] data = Header.ToByte();
				Stream.Write(data, 0, data.Length);

					if (Header.ContentLength > 0)
						__Frame.bleng = 
							   Header.ContentLength;
					
							__Frame.hleng = 
										data.Length;
			}

			if (length > 0)
			{
				// оптравить форматированные данные
				if (Header.TransferEncoding == "chunked")
				{
					byte[] hex = Encoding.UTF8.GetBytes(length.ToString("X"));

					Stream.Write( hex, 0, hex.Length );
					End();
					Stream.Write( buffer, offset, length );
					End();

						__Frame.bpart += length;
						__Frame.bleng += length;
				}
				else
				{
					if ( __Frame.bpart + length > __Frame.bleng )
						throw new HTTPException("Превышенна длинна Content-Length");
					else
						__Frame.bpart += length;

					Stream.Write( buffer, offset, length );
				}
			}
		}

		/// <summary>
		/// Очищает управляемые и некправляемые ресурсы занимаемые объектом
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (Header.TransferEncoding == "chunked")
				Eof();
			Header = null;
			Stream = null;
			if (disposing)
			{
				
			}
			base.Dispose(disposing);
		}
	}
}
