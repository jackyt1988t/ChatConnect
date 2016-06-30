using System;
using System.IO;
using System.Text;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	/// <summary>
	/// Записывет данные в поток
	/// </summary>
	public class HTTPWriter : MyStream
	{
		/// <summary>
		/// Минимальный размер потока
		/// </summary>
		public static int MINRESIZE;
		/// <summary>
		/// Максимальный размер потока
		/// </summary>
		public static int MAXRESIZE;

		private static readonly byte[] ENDCHUNCK;
		private static readonly byte[] EOFCHUNCK;

		/// <summary>
		/// Заголвоки запрос
		/// </summary>
		public Header Header;
		/// <summary>
		/// Информация о записи
		/// </summary>
		public HTTPFrame _Frame;
		/// <summary>
		/// Устанавливает позицию прочитанных данных,
		/// при совободном месте уменьшает поток в 4 раза,
		/// но не меньше сем указанное минимальное значение.
		/// </summary>
		public override long Position
		{
			get
			{
				return base.Position;
			}

			set
			{
				lock (__Sync)
				{
					base.Position = value;
					if (Count > MINRESIZE)
					{
						int resize;

						if (Length > 0)
							resize = (int)Count / 4;
						else
							resize = 0;

						if (resize < MINRESIZE)
							resize = MINRESIZE;
						if (resize > (int)Length)
							Resize(resize);
					}
				}
			}
		}

		static HTTPWriter()
		{
			MINRESIZE = 32000;
			MAXRESIZE = 1000000;
			ENDCHUNCK =
				new byte[] { 0x0D, 0x0A };
			EOFCHUNCK =
				new byte[] { 0x30, 0x0D, 0x0A, 0x0D, 0x0A };
		}
		/// <summary>
		/// Создает кольцевой поток указанной емкости
		/// </summary>
		/// <param name="length">длинна потока</param>
		public HTTPWriter(int length) :
			base(length)
		{
			_Frame = new HTTPFrame();
		}
		/// <summary>
		/// Записывает CRLF в поток
		/// </summary>
		public void End()
		{
			lock (__Sync)
				base.Write(ENDCHUNCK, 0, 2);
		}
		/// <summary>
		/// Записывает 0CRLFCRLF в поток
		/// </summary>
		public void Eof()
		{
			lock (__Sync)
				base.Write(EOFCHUNCK, 0, 5);
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
		/// Записывает указанный массив данных в поток.
		/// Строка об-ся в соответствии с установленными заголвоками.
		/// Отправляет http заголвоки есл они еще не были отправлены.
		/// </summary>
		/// <param name="buffer">массив данных для записи</param>
		/// <param name="start">начальная позиция</param>
		/// <param name="length">количество для записи</param>
		public override void Write(byte[] buffer, int start, int length)
		{
			if (buffer.Length < (length - start))
				throw new IOException("MAXLENGTH");

			lock (__Sync)
			{
				_Frame.Handl++;
				if (!Header.IsRes)
				{
						byte[] data = Header.ToByte();
					base.Write(data, 0, data.Length);

					Header.SetRes();
					_Frame.hleng = data.Length;
				}
				if ((length + 64) > Clear)
				{
					int resize = (int)Count * 2;
					if (resize - (int)Length - 64 < length)
						resize = (int)Length + length + 64;

					if (resize < MAXRESIZE)
						Resize (  resize  );
					else
						throw new IOException("MAXRESIZE");
				}
					_Frame.bleng += length;
				if (!string.IsNullOrEmpty(
									Header.ContentEncoding))
					_Frame.bpart += length;
				if (length > 0)
				{
					// оптравить форматированные данные
					if (Header.TransferEncoding != "chunked")
						base.Write(  buffer, start, length  );
					else
					{
						byte[] hex = Encoding.UTF8.GetBytes(
										length.ToString("X"));
						
						base.Write(hex, 0, hex.Length);
						End();
						base.Write(  buffer, start, length  );
						HTTPWriter re = this;
						End();
					}
				}
			}
		}
	}
}
