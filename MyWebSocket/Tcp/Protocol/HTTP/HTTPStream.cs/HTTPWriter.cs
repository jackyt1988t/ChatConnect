using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	class HTTPWriter : Mytream
	{
		public static int MAXRESIZE;
		public static readonly byte[] ENDCHUNCK;
		public static readonly byte[] EOFCHUNCK;

		public IHeader header;
		public HTTPFrame _Frame;

		static HTTPWriter()
		{
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
			Write(ENDCHUNCK);
		}
		public void Eof()
		{
			Write(EOFCHUNCK);
		}
		
		public void Write(string str)
		{
			Write(  Encoding.UTF8.GetBytes(str)  );
		}
		public void Write(byte[] buffer)
		{
			base.Write( buffer, 0, buffer.Length );
		}

		public override void Write(byte[] buffer, int start, int length)
		{
			if (!header.IsRes)
			{
				Write(header.ToByte());
					  header.SetRes();
			}
			if ( buffer.Length > Clear )
			{
				int resize = Count * 2;
				if (resize < buffer.Length)
				    resize = buffer.Length;
				if (resize   <   MAXRESIZE)
				    Resize(resize);
				else
					throw new IOException();
			}
			// оптравить форматированные данные
			if (header.TransferEncoding != "chunked")
				base.Write(  buffer, start, length  );
			else
			{
				Write(length.ToString("X"));
				End();
				base.Write(  buffer, start, length  );
				End();
			}
		}
	}
}
