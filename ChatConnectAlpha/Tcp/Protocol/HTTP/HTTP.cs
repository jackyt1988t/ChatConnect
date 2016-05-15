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
        /// 
        /// </summary>
        public abstract event PHandlerEvent EventWork;
        /// <summary>
        /// 
        /// </summary>
        public abstract event PHandlerEvent EventData;
        /// <summary>
        /// 
        /// </summary>
        public abstract event PHandlerEvent EventClose;
        /// <summary>
        /// 
        /// </summary>
        public abstract event PHandlerEvent EventError;
        /// <summary>
        /// 
        /// </summary>
        public abstract event PHandlerEvent EventConnect;

		
		private volatile int state;

		protected long __twaitconn;
  async public void File(string path)
		{
			await Task.Run(() =>
			{
				try
				{
					file(path);
				}
				catch (Exception exc)
				{
					Error(exc.Message, exc.StackTrace);
				}
			});
		}
		public void file(string path)
		{
			int i = 0;
			int sleep = 20;
			int maxlen = 1000 * 32;
			using (FileStream sr = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				int count = (int)(sr.Length / maxlen);
				int length = (int)(sr.Length - count *
											  maxlen);

				Response.StartString = "HTTP/1.1 200 OK";
				Response.Add("Connection", "keep-alive");
				Response.Add("Content-Type",
								   "text/" + Request.File);
				Response.Add("Content-Length",
								   sr.Length.ToString(  ));

				while (i++ < count)
				{


					byte[] buffer = new byte[maxlen];
					int __read = sr.Read(buffer, 0, maxlen);

					Response.SegmentsBuffer.Enqueue(buffer);
					if (Response.SegmentsBuffer.Count < 10)
						sleep = 20;
					else
					{
						sleep += 10;
						Thread.Sleep(sleep);
					}
				}
				if (length > 0)
				{
					byte[] buffer = new byte[length];
					int __read = sr.Read(buffer, 0, length);

					Response.SegmentsBuffer.Enqueue(buffer);
				}
				Response.End();
			}
		}
		public bool Ping(byte[] message)
		{
			throw new NotSupportedException();
		}
		public bool Send(string message)
		{
			return Send(Encoding.UTF8.GetBytes(message));
		}
		public bool Send(byte[] message)
		{
			throw new NotSupportedException();
		}
        public bool Close( string message )
        {
			throw new NotSupportedException();
		}
		public void Reset(  Socket socket  )
		{
			if (socket == null || !socket.Connected)
				throw new ArgumentNullException("Socket is null or disconnect");

			Tcp = socket;
			State = States.Work;
			Request = new Header();
			Response = new Header();
			__twaitconn = DateTime.Now.Ticks;
			TaskResult.Option = TaskOption.Loop;
			TaskResult.Protocol = TaskProtocol.HTTP;
		}
        public bool Close(string message, int number)
        {
			throw new NotSupportedException();
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

			byte[] body = Encoding.UTF8.GetBytes(html);
			
			if (!Response.IsEnd)
			{
				Response.StartString = "Http/1.1 503 BAD";
				Response.Add("Content-Type",
							  "text/html; charset=utf-8");
					Response.Add("Content-Length",
								  body.Length.ToString());
					Response.SegmentsBuffer.Enqueue(body);
				Response.End();
			}
		}
        public TaskResult TaskLoopHandlerProtocol(    )
        {
			try
			{
					if (state == 0)
					{
						Work();
						Interlocked.CompareExchange(ref state, 1, 0);
					}
					if (state == 3)
					{
						Work();
						Interlocked.CompareExchange(ref state, 2, 3);
					}
				if (state == 1)
				{
					if (Tcp.Poll(0, SelectMode.SelectRead))
					{
						if (Tcp.Available > 0)
						{
							Data();
							Interlocked.CompareExchange(ref state, 0, 1);
						}
						else
						{
							state = 5;
						}
					}
					else
						state = 0;
				}
						if (state == 6)
						{
							Connection();
							Interlocked.CompareExchange(ref state, 2, 6);
						}
					if (state == 2)
					{
						if (Tcp.Poll(0, SelectMode.SelectWrite))
						{
							Send();
							Interlocked.CompareExchange(ref state, 3, 2);
						}
						else if (!Tcp.Connected)
							state = 5;
						else
							state = 3;
						return TaskResult;
					}

				if (state == 5)
				{
					if (Response.SegmentsBuffer.Count > 0)
					{
						Send();
					}
					else
					{
						TaskResult.Option  =  TaskOption.Delete;
						if (Tcp.Connected
								&& !Tcp.Poll(0, SelectMode.SelectError))
							Tcp.Shutdown(  SocketShutdown.Both  );
						if (Tcp != null)
							Tcp.Close(0);

						state = 7;
						Close();

					}
				}
			}
			catch (HTTPException exc)
			{
				state = 4;
				Error(exc.Message, 
						exc.StackTrace);
				OnEventError(    exc    );
				state = 5;
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
        protected abstract void Work();
        /// <summary>
        /// 
        /// </summary>
        protected abstract void Send();
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
        protected abstract void Connection();
        /// <summary>
        /// Обрабочик
        /// </summary>
        protected abstract void OnEventWork();
        /// <summary>
        /// Пришел Фрейм TEXT или Binnary
        /// </summary>
        /// <param name="Frame"></param>
        protected abstract void OnEventData();
        /// <summary>
        /// Вызывается при закрытии WS
        /// </summary>
        /// <param name="Frame">Информация о данных</param>
        protected abstract void OnEventClose();
        /// <summary>
        ///  Обрабатывает полученный Фрейм проктокола WS
        /// </summary>
        /// <param name="stream">поток для четния</param>
        protected abstract void HandlerFrame(HTTPStream Stream);
        /// <summary>
        /// Вызывается при ощибке WS
        /// </summary>
        /// <param name="Frame">Информация о ошибке</param>
        protected abstract void OnEventError(HTTPException Error);
        /// <summary>
        /// Вызывается при открытии WS
        /// </summary>
        protected abstract void OnEventConnect();
    }
}