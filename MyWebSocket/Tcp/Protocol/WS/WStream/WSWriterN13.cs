using System;
using System.IO;

namespace MyWebSocket.Tcp.Protocol.WS
{
	class WSWriterN13 : MyStream
	{
		public static int MINRESIZE = 0;
		public static int MAXRESIZE = 512000;

		public WSN13 _Frame;

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
							resize = (int)Length / 4;
						else
							resize = 0;

						if (resize < MINRESIZE)
							resize = MINRESIZE;
						if (resize > (int)Length)
							Resize(    resize    );
					}
				}
			}
		}

		public WSWriterN13() : 
			base(1024)
		{
			_Frame = new WSN13();
		}
		public WSWriterN13(int length) :
			base(length)
        {
			_Frame = new WSN13();
		}

		public override int Read(byte[] buffer, int start, int length)
		{
			
			int read;
			lock (__Sync)
			{
				read = base.Read(buffer, start, length);
				if (Count > MINRESIZE)
				{
					int resize;

					if (Length > 0)
						resize = (int)Length / 4;
					else
						resize = 0;

					if (resize < MINRESIZE)
						resize = MINRESIZE;
					if (resize > (int)Length)
						Resize(    resize    );
				}
			}
			return read;
		}

		public void Write(WSN13 frame)
		{
			_Frame = frame;
			lock (__Sync)
			{	 
				Write(frame.DataHead);
				Write(frame.DataBody);
			}
		}
		public void Write(byte[] buffer)
		{
			Write(buffer, 0, buffer.Length);
		}
		public override void Write(byte[] buffer, int start, int length)
		{
			lock (__Sync)
			{
				if (length > Clear)
				{
					int resize = (int)Count * 2;
					if (resize - (int)Length < length)
						resize = (int)Length + length;
					
					if (resize < MAXRESIZE)
						Resize (  resize  );
					else
						throw new IOException("MAXRESIZE");
				}
					base.Write( buffer, 0, buffer.Length );
			}
			
		}
	}
}
