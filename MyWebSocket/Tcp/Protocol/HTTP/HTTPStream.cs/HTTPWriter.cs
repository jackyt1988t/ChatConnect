using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	class HTTPWriter : MyStream
	{
		public static int MINRESIZE;
		public static int MAXRESIZE;
		public static readonly byte[] ENDCHUNCK;
		public static readonly byte[] EOFCHUNCK;

		public IHeader header;
		public HTTPFrame _Frame;

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
		public HTTPWriter(int length) :
			base(length)
		{
			_Frame = new HTTPFrame();
		}
		public void End()
		{
			base.Write(ENDCHUNCK, 0, 2);
		}
		public void Eof()
		{
			base.Write(EOFCHUNCK, 0, 5);
		}
		
		public void Write(string str)
		{
			Write(Encoding.UTF8.GetBytes(str));
		}
		public void Write(byte[] buffer)
		{
			Write(  buffer, 0, buffer.Length  );
		}
		public override void Write(byte[] buffer, int start, int length)
		{
			lock (__Sync)
			{
				_Frame.Handl++;
				if (!header.IsRes)
				{
						byte[] data = header.ToByte();
					base.Write(data, 0, data.Length);

					header.SetRes();
					_Frame.hleng = data.Length;
				}
				if (length > Clear)
				{
					int resize = (int)Count * 2;
					if (resize - (int)Length < length)
						resize = (int)Length + length + 64;

					if (resize < MAXRESIZE)
						Resize (  resize  );
					else
						throw new IOException("MAXRESIZE");
				}
					_Frame.bleng += length;
				if (!string.IsNullOrEmpty(
									header.ContentEncoding))
					_Frame.bpart += length;
					// оптравить форматированные данные
					if (header.TransferEncoding != "chunked")
						base.Write(  buffer, start, length  );
					else
					{
						byte[] data = Encoding.UTF8.GetBytes(
										length.ToString("X"));
						
						base.Write(data, 0, data.Length);
						End();
						base.Write(  buffer, start, length  );
						End();
					}
			}
		}
	}
}
