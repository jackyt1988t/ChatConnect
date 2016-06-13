using System;
using System.IO;
using System.Net.Sockets;

namespace MyWebSocket.Tcp.Protocol
{
	class BaseProtocol : IProtocol, IDisposable
	{
		/// <summary>
		/// минимальный размер потока
		/// </summary>
		public static int MINLENGTHBUFFER = 1000 * 32;
		/// <summary>
		/// максимальный размер потока
		/// </summary>
		public static int MAXLENGTHBUFFER = 1000 * 1024;
		private TimeSpan __INTERVALRESIZE;
		/// <summary>
		/// tcp/ip соединение
		/// </summary>
		public Socket Tcp
		{
			get;
			set;
		}
		public virtual States State
		{
			get;
			protected set;
		}
		public virtual Mytream Reader
		{
			get;
			protected set;
		}
		public virtual Mytream Writer
		{
			get;
			protected set;
		}
		public virtual IHeader Request
		{
			get;
			protected set;
		}
		public virtual IHeader Response
		{
			get;
			protected set;
		}

		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		public virtual void Dispose(bool disposing)
		{
			
			if (disposing)
			{
				if (Tcp != null)
					Tcp.Dispose();
				if (Writer != null)
					Writer.Dispose();
				if (Reader != null)
					Reader.Dispose();
			}
		}
		public virtual TaskResult TaskLoopHandlerProtocol()
		{
			throw new NotSupportedException("Не поддерживается");
		}

		protected SocketError Read()
		{
			SocketError error = SocketError.Success;
			
			int count = 8000;
			int start =
			   (int)Reader.PointW;
			byte[] buffer =
					Reader.Buffer;
			
			if (Reader.Count - start < count)
				count =
					  (int)(Reader.Count - start);
			
			int length = Tcp.Receive(buffer, start, count, SocketFlags.None, out error);
			if (length > 0)
			{
				if (length < Reader.Clear)
					Reader.SetLength(length);
				else
					error = SocketError.SocketError;	
			}
			return error;
		}
		protected SocketError Send()
		{
			SocketError error = SocketError.Success;

			lock (Writer.__Sync)
			{
				int start =
					(int)Writer.PointR;
				int write =
					(int)Writer.Length;
				if (write > 16000)
					write = 16000;
				byte[] buffer =
						Writer.Buffer;

				if (Writer.Count - start < write)
					write =
					  (int)(Writer.Count - start);
				int length = Tcp.Send(buffer, start, write, SocketFlags.None, out error);
				if (length > 0)
					Writer.Position = length;
				if (MINLENGTHBUFFER < Writer.Count)
				{
					int resize = (int)Writer.Count / 2;
					if (resize > Writer.Length 
							&& __INTERVALRESIZE.Ticks < DateTime.Now.Ticks)
						Writer.Resize(resize);
				}
			}
			return error;
		}
			protected SocketError Write(byte[] buffer, int start, int write)
			{
				SocketError error = SocketError.Success;
				if (Writer.Empty)
					start  =  Tcp.Send(buffer, start, write, SocketFlags.None, out error);

				int length = write - start;
				if (length > 0)
				{
					lock (Writer.__Sync)
					{
						if (Writer.Clear - 1 < length)
						{
							int resize = (int)Writer.Count * 2;
							if (resize < length)
								resize = length;
							if (Writer.Count > MAXLENGTHBUFFER)
								error = SocketError.SocketError;
							else 
							{
								Writer.Resize(resize);
								Writer.Write(buffer, start, length);
								__INTERVALRESIZE = new TimeSpan(DateTime.Now.Ticks + TimeSpan.TicksPerSecond * 8);
							}
							
						}
							else
								Writer.Write(buffer, start, length);
					}	
				}
				return error;
		}
	}
}
