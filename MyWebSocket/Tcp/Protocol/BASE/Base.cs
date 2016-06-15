using System;
using System.IO;
using System.Net.Sockets;

namespace MyWebSocket.Tcp.Protocol
{
	class BaseProtocol : IProtocol, IDisposable
	{
		/// <summary>
		/// Длинна чтения сокета
		/// </summary
		public static int LENGTHREAD = 1000 * 8;
		/// <summary>
		/// Длинна записи сокета
		/// </summary>
		public static int LENGTHWRITE = 1000 * 64;
		/// <summary>
		/// минимальный размер потока
		/// </summary>
		public static int MINLENGTHBUFFER = 1000 * 32;
		/// <summary>
		/// максимальный размер потока
		/// </summary>
		public static int MAXLENGTHBUFFER = 1000 * 1024;
		/// <summary>
		/// tcp / ip сокет подключения
		/// </summary>
		public Socket Tcp
		{
			get;
			protected set;
		}
		/// <summary>
		/// Статус текущего протокола.
		/// </summary>
		public virtual States State
		{
			get;
			protected set;
		}
		/// <summary>
		/// Кольцевой буффер хранения данных
		/// </summary>
		public virtual MyStream Reader
		{
			get;
			protected set;
		}
		/// <summary>
		/// Кольцевой буфер хранения данных
		/// </summary>
		public virtual MyStream Writer
		{
			get;
			protected set;
		}
		/// <summary>
		/// Содержит Полученные заголовки запроса
		/// </summary>
		public virtual IHeader Request
		{
			get;
			protected set;
		}
		/// <summary>
		/// Содержит Отправленные заголовки запроса
		/// </summary>
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
			
			int count = 
			   LENGTHREAD;
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
				try
				{
					Reader.SetLength(length);
				}
				catch (IOException)
				{
					error = SocketError.SocketError;
				}
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
				if (write   >   LENGTHWRITE)
					write = LENGTHWRITE;
				byte[] buffer =
						Writer.Buffer;

				if (Writer.Count - start < write)
					write =
					  (int)(Writer.Count - start);
				int length = Tcp.Send(buffer, start, write, SocketFlags.None, out error);
				if (length > 0)
				{
					try
					{
						Writer.Position = length;
					}
					catch (IOException)
					{
						error = SocketError.SocketError;
					}
				}
			}
			return error;
		}
		protected SocketError Write(byte[] buffer, int start, int write)
		{
			SocketError error = SocketError.Success;

			lock (Writer.__Sync)
			{

				if (Writer.Empty)
					start = Tcp.Send(buffer, start, write, SocketFlags.None, out error);

				int length = write - start;
				if (length > 0)
				{
					try
					{
						Writer.Write(buffer, write - length, length);
					}
					catch (IOException)
					{
						error = SocketError.SocketError;
					}
				}
			}

			return error;
		}
	}
}
