using System;
using System.IO;
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
		public Stream stream;
		/// <summary>
		/// 
		/// </summary>
		public MyStream Stream;
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
				return true;
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
		public HTTPWriter(MyStream stream)
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
			lock (Stream.__Sync)
				Stream.Write(ENDCHUNCK, 0, 2);
		}
		/// <summary>
		/// Записывает 0CRLFCRLF в поток
		/// </summary>
		public void Eof()
		{
			lock (Stream.__Sync)
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
			Write(  buffer, 0, buffer.Length  );
		}



		/// <summary>
		/// Не поддерживается данной реализацией
		/// </summary>
		public override void Flush()
		{
			throw new NotImplementedException();
		}
		public ovverride void Dispose()
		{
			if (Header.ContentEncoding == "gzip"
			  || Header.ContentEncoding == "deflate")
				stream.Dispose();
			if (Header.TransferEncoding == "chunked")
				Eof();
			Header = null;
			Stream = null;
			base.Dispose();
		}
		/// <summary>
		/// Не поддерживается данной реализацией
		/// </summary>
		/// <param name="value"></param>
		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Не поддерживается данной реализацией
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="origin"></param>
		/// <returns></returns>
		


		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Записывает данные в базовый кольцевой поток
		/// </summary>
		/// <param name="buffer">>массив данных для записи</param>
		/// <param name="offset">начальная позиция</param>
		/// <param name="length">количество для чтения</param>
		/// <returns></returns>
		public override int Read(byte[] buffer, int offset, int length)
		{
			return Stream.Read(buffer, offset, length);
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
			lock (Stream.__Sync)
			{
				__Frame.Handl++;
				if (!Header.IsRes)
				{
								  Header.SetRes();
					byte[] data = Header.ToByte();
					Stream.Write(data, 0, data.Length);
					
					if (Header.ContentEncoding == "gzip")
						stream = new GZipStream(
							Stream, CompressionLevel.Fastest, true);
					else if (Header.ContentEncoding == "deflate")
						stream = new DeflateStream(
							Stream, CompressionLevel.Fastest, true);
					
					__Frame.hleng = 
								data.Length;
				}
					
					__Frame.bpart += length;
				if (length > 0)
				{
					// оптравить форматированные данные
					if (Header.TransferEncoding != "chunked")
						stream.Write( buffer, offset, length );
					else
					{
						byte[] hex = Encoding.UTF8.GetBytes(
										 length.ToString("X"));

						Stream.Write( hex, 0, hex.Length );
						End();
						stream.Write( buffer, offset, length );
						End();
					}
				}
			}
		}
	}
}
