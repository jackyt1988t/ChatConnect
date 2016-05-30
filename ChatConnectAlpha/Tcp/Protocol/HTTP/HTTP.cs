using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatConnect.Tcp.Protocol.HTTP
{
    abstract class HTTP : IProtocol
    {
 
        public Socket Tcp
        {
            get;
            protected set;
        }
		/// <summary>
		/// Объект синхронизации
		/// </summary>
		public object Sync
		{
			get;
			protected set;
		}
		public States State
		{
			get
			{
				return (States)state;
			}
			protected set
			{
				state = ( int )value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
abstract
		public StreamS Reader
		{
			get;
		}
		/// <summary>
		/// 
		/// </summary>
abstract
		public StreamS Writer
		{
			get;
		}
		public IHeader Request
		{
			get;
			protected set;
		}
		public IHeader Response
        {
            get;
            protected set;
        }
        public TaskResult TaskResult
        {
        	get;
            protected set;
		}
		/// <summary>
		/// Событие которое наступает при проходе по циклу
		/// </summary>
		public event PHandlerEvent EventWork
		{
			add
			{
				lock (SyncEvent)
					__EventWork += value;

			}
			remove
			{
				lock (SyncEvent)
					__EventWork -= value;
			}
		}
		/// <summary>
		/// Событие которое наступает когда приходит фрейм с данными
		/// </summary>
		public event PHandlerEvent EventData
		{
			add
			{
				lock (SyncEvent)
					__EventData += value;

			}
			remove
			{
				lock (SyncEvent)
					__EventData -= value;
			}
		}
		/// <summary>
		/// Событие которое наступает когда приходит заврешающий фрейм
		/// </summary>
		public event PHandlerEvent EventClose
		{
			add
			{
				lock (SyncEvent)
					__EventClose += value;

			}
			remove
			{
				lock (SyncEvent)
					__EventClose -= value;
			}
		}
		/// <summary>
		/// Событие которое наступает когда приходит при ошибке протокола
		/// </summary>
		public event PHandlerEvent EventError
		{
			add
			{
				lock (SyncEvent)
					__EventError += value;

			}
			remove
			{
				lock (SyncEvent)
					__EventError -= value;
			}
		}
		/// <summary>
		/// Событие которое наступает когда приходит кусок отправленных данных
		/// </summary>
		public event PHandlerEvent EventChunk
		{
			add
			{
				lock (SyncEvent)
					__EventChunk += value;

			}
			remove
			{
				lock (SyncEvent)
					__EventChunk -= value;
			}
		}
		/// <summary>
		/// Событие которое наступает при открвтии соединения когда получены заголвоки
		/// </summary>
		static
			  public event PHandlerEvent EventConnect
		{
			add
			{
				__EventConnect += value;

			}
			remove
			{
				__EventConnect -= value;
			}
		}

		private object SyncEvent = new object();
		private event PHandlerEvent __EventWork;
		private event PHandlerEvent __EventPing;
		private event PHandlerEvent __EventPong;
		private event PHandlerEvent __EventData;
		private event PHandlerEvent __EventError;
		private event PHandlerEvent __EventClose;
		private event PHandlerEvent __EventChunk;
		static private event PHandlerEvent __EventConnect;


		private volatile int state;

		protected long __twaitconn = DateTime.Now.Ticks;

		public HTTP()
		{
			Sync = new object();
			State = States.Work;
			Request = new Header();
			Response = new Header();
			TaskResult = new TaskResult();
		}
		async public void File(string path, string type)
		{
			await Task.Run(() =>
			{
				try
				{
					file(path, type);
				}
				catch (Exception exc)
				{
					Error(exc.Message, exc.StackTrace);
				}
			});
		}
		public void file(string path, string type)
		{
			int i = 0;
			int maxlen = 1000 * 32;
			using (FileStream sr = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				int _count = (int)(sr.Length / maxlen);
				int length = (int)(sr.Length - _count * maxlen);

				Response.StartString = "HTTP/1.1 200 OK";
				Response.Add("Connection", "keep-alive");
				Response.Add("Content-Type", "text/" + type);
				Response.Add("Content-Length", sr.Length.ToString());

				byte[] header = Response.ToByte();
				if (Send(header, 0, header.Length))
					Response.Res();

				while (i++ < _count)
				{
					byte[] buffer = new byte[maxlen];
					int recive = sr.Read(buffer, 0, maxlen);
					if (!Send(buffer, 0, recive))
						return;
					Thread.Sleep(10);
				}
				if (length > 0)
				{
					byte[] buffer = new byte[length];
					int recive = sr.Read(buffer, 0, length);
					if (!Send(buffer, 0, recive))
						return;
					Thread.Sleep(10);
				}
				Response.End();
			}
		}/// <summary>
		 /// Отправляет данные текущему подключению
		 /// </summary>
		 /// <param name="message">массив байт для отправки</param>
		 /// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Send(byte[] buffer, int start, int write)
		{

			if (state > 3)
				return false;
			
			SocketError error = SocketError.Success;
			lock (Writer)
			{
				if (Writer.Empty)
					start = Tcp.Send(buffer, start, write, SocketFlags.None, out error);

				int length = write - start;
				if (length > 0)
				{
					if (Writer.Clear < length)
						error = SocketError.NoData;
					else
					{
						Writer.Write(buffer, start, length);
						Writer.SetLength(length);
					}
				}
			}
			if (error != SocketError.Success)
			{
				if (error != SocketError.WouldBlock
					&& error != SocketError.NoBufferSpaceAvailable)
				{
					if (error != SocketError.Disconnecting && error != SocketError.ConnectionReset
														   && error != SocketError.ConnectionAborted)
					{
						if (!SetError())
							Error(new HTTPException("Ошибка записи http данных: " + error.ToString()));
					}
					Close();
				}
			}
			return true;
		}
        public bool Close( string message )
        {
			if (SetClose())
				return false;
			
			return true;
		}
		public void Error(string message, string stack)
		{
			string html =
				"<html>" +
					"<head>" +
						"<style>" +
							".head {margin: auto; color: red; text-align: center; width: 300px}" +
							".body {display: block; color: blue; text-align: center; width: 600px}" +
						"</style>" +
					"</head>" +
					"<body>" +
						"<div class=\"head\">" +
							message +
						"</div>" +
						"<div class=\"body\">" +
							"<pre>" +
								stack +
							"</pre>" +
						"</div>" +
					"</body>" +
				"</html>";

			byte[] __body = Encoding.UTF8.GetBytes(html);
			
			if (!Response.IsRes)
			{
				Response.Res();
				Response.StartString = "Http/1.1 503 BAD";
				Response.Add("Content-Type", "text/html; charset=utf-8");
				Response.Add("Content-Length", __body.Length.ToString());

				byte[] header = Response.ToByte();
				if (Send(header, 0, header.Length))
					Send(__body, 0, __body.Length);
				Response.End();
			}
			
		}
        public TaskResult TaskLoopHandlerProtocol()
        {
			try
			{
				if (state == 0)
				{
					Work();
					if (Interlocked.CompareExchange(ref state, 1, 0) != 0)
						return TaskResult;
					if (!Request.IsReq)
					{
						Read();
						Data();
					}
					else
						state = 3;
					if (Interlocked.CompareExchange(ref state, 0, 1) == 1)
						return TaskResult;
				}
				if (state == 3)
				{
					Connection(Request, Response);
					if (Interlocked.CompareExchange(ref state,-1, 3) == 3)
					{
						File("Html" + Request.Path, Request.File);
						return TaskResult;
					}
				}
				if (state ==-1)
				{
					Work();
					if (Interlocked.CompareExchange(ref state, 2,-1) !=-1)
						return TaskResult;
					if (!Response.IsEnd || !Writer.Empty)
						Write();	
					else
					{
						state = 0;
						Request = new Header();
						Response = new Header();
					}
					if (Interlocked.CompareExchange(ref state,-1, 2) == 2)
						return TaskResult;
				}
				
				if (state == 5)
				{
					state = 7;
					Tcp.Close();
					Close();
				}
				if (state == 7)
				{
					TaskResult.Option = TaskOption.Delete;
					if (Tcp != null)
						Tcp.Dispose();
				}
			}
			catch (HTTPException exc)
			{
				if (!SetError())
					Error(exc);
				Close();
			}
			catch (Exception exc)
			{
				Log.Logout.AddMessage(exc.Message, "Log/log.log", Log.Log.Fatail);
				state = 5;
			}
			return TaskResult;
        }
        public override string ToString()
        {
        	return "HTTP";
        }
		/// <summary>
		/// 
		/// </summary>
		private void Read()
		{
			int count = 8000;
			int start =
			   (int)Reader.PointW;
			byte[] buffer =
					Reader.Buffer;
			SocketError error = SocketError.Success;
			if (Reader.Clear == 0)
			{
				error = SocketError.NoData;
			}
			else
			{
				if (Reader.Count - start < count)
					count =
					   (int)(Reader.Count - start);
				int length = Tcp.Receive(buffer, start, count, SocketFlags.None, out error);
				if (length > 0)
					Reader.SetLength(length);
			}
			if (error != SocketError.Success)
			{
				if (error != SocketError.WouldBlock
					&& error != SocketError.NoBufferSpaceAvailable)
				{
					if (error != SocketError.Disconnecting && error != SocketError.ConnectionReset
														   && error != SocketError.ConnectionAborted)
					{
						if (!SetError())
							Error(new HTTPException("Ошибка записи http данных: " + error.ToString()));
					}
					Close();
				}
			}
		}
		/// <summary>
		/// Отправляет сообщение
		/// </summary>
		/// <param name="data">Данные</param>
		private void Write()
		{
			if (Writer.Empty)
				return;
			int start =
				(int)Writer.PointR;
			int write =
				(int)Writer.Length;
			if (write > 16000)
				write = 16000;
			byte[] buffer =
					 Writer.Buffer;
			SocketError error = SocketError.Success;
			if (Writer.Count - start < write)
				write =
				  (int)(Writer.Count - start);
			int length = Tcp.Send(buffer, start, write, SocketFlags.None, out error);
			if (length > 0)
				Writer.Position = length;
			if (error != SocketError.Success)
			{
				if (error != SocketError.WouldBlock
					&& error != SocketError.NoBufferSpaceAvailable)
				{
					if (error != SocketError.Disconnecting && error != SocketError.ConnectionReset
														   && error != SocketError.ConnectionAborted)
					{
						if (!SetError())
							Error(new HTTPException("Ошибка записи http данных: " + error.ToString()));
					}
						
						Close();
				}
			}
		}
		protected bool SetClose()
		{
			lock (Sync)
			{
				if (state > 3)
					return true;
				state = 5;
				return false;
			}

		}
		protected bool SetError()
		{
			lock (Sync)
			{
				if (state > 3)
					return true;
				state = 4;
				return false;
			}
		}
		protected void OnEventWork()
		{
			string s = "work";
			string m = "Цикл обработки";
			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventWork;
			if (e != null)
				e(this, new PEventArgs(m, s));
		}
		protected void OnEventData()
		{
			string s = "data";
			string m = "Получен фрейм с данными";

			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventData;
			if (e != null)
				e(this, new PEventArgs(s, m, null));
		}
		protected void OnEventClose()
		{
			string s = "close";
			string m = "Соединение было закрыто";

			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventClose;
			if (e != null)
				e(this, new PEventArgs(s, m, null));
		}
		protected void OnEventError(HTTPException _error)
		{
			string s = "error";
			string m = "Произошла ошибка во время исполнения";

			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventError;
			if (e != null)
				e(this, new PEventArgs(s, m, _error));
		}
		protected void OnEventConnect(IHeader request, IHeader response)
		{
			string s = "connect";
			string m = "Соединение было установлено, протокол ws";

			PHandlerEvent e;
			lock (SyncEvent)
				e = __EventConnect;
			if (e != null)
				e(this, new PEventArgs(s, m, null));
		}
		/// <summary>
		/// 
		/// </summary>
		protected abstract void Work();
        /// <summary>
        /// 
        /// </summary>
        protected abstract void Data();
        /// <summary>
        /// 
        /// </summary>
        protected abstract void Close();
		/// <summary>
		/// 
		/// </summary>
		protected abstract void Error(HTTPException error);
		/// <summary>
		/// 
		/// </summary>
        protected abstract void Connection(IHeader reauest, IHeader response);
    }
}