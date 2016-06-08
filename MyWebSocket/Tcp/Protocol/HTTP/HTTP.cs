﻿using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
    abstract class HTTP : BaseProtocol
    {
		/// <summary>
		/// Объект синхронизации
		/// </summary>
		public object Sync
		{
			get;
			protected set;
		}
		volatile int state;
override
		public States State
		{
			protected set
			{
				state = (int)value;
			}
			get
			{
				return (States)state;
			}
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
		public event PHandlerEvent EventOnOpen
		{
			add
			{
				__handconn = true;
				__EventConnect += value;
			}
			remove
			{
				__handconn = false;
				__EventConnect -= value;
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
				__handconn = true;
				__EventConnect += value;
			}
			remove
			{
				__handconn = false;
				__EventConnect -= value;
			}
		}

		private object SyncEvent = new object();
		private  event PHandlerEvent __EventWork;
		private  event PHandlerEvent __EventData;
		private  event PHandlerEvent __EventError;
		private  event PHandlerEvent __EventClose;
		private  event PHandlerEvent __EventChunk;
		private  event PHandlerEvent __EventOnOpen;
		static 
		private  event PHandlerEvent __EventConnect;
		
		static
		protected bool __handconn = false;
		protected long __twaitconn = DateTime.Now.Ticks;

		public HTTP()
		{
			Sync = new object();
			State = States.work;
			TaskResult = new TaskResult();
		}
		async 
		public void File(string path, string type)
		{
			await Task.Run(() =>
			{
				try
				{
					file(path, type);
				}
				catch (Exception err)
				{
					exc(new HTTPException(err.Message, err));
				}
				finally
				{
					Response.SetEnd();
				}
			});
		}
		public void file(string path, string type)
		{
			int i = 0;
			int maxlen = 1000 * 16;
			using (FileStream sr = new FileStream(
							path, FileMode.Open, FileAccess.Read))
			{
				if (Response.SetRes())
					return;
				Response.StartString = "HTTP/1.1 200 OK";
				if (!Response.ContainsKey("Content-Type"))
					Response.Add("Content-Type", "text/" + type);
				Response.Add("Content-Length", sr.Length.ToString());

				int _count = (int)(sr.Length / maxlen);
				int length = (int)(sr.Length - _count * maxlen);
				byte[] header = Response.ToByte();
				if (!Message(header, 0, header.Length))
					return;
				while (i++ < _count)
				{
					int recive = 0;
					if ( state > 4 )
						return;
					byte[] buffer = new byte[maxlen];
					while ((maxlen - recive) > 0)
					{
						recive = sr.Read(buffer, 0, maxlen - recive);
					}
					if ( !Message(buffer, 0, recive ))
						return;
					Thread.Sleep(10);
				}
					if (length > 0)
					{
						int recive = 0;
						if ( state > 4 )
							return;
						byte[] buffer = new byte[length];
						while ((length - recive) > 0)
						{
							recive = sr.Read(buffer, 0, length - recive);
						}
						if ( !Message(buffer, 0, recive ))
							return;
						Thread.Sleep(10);
					}
			}
		}
		public bool Close(string message)
		{
			lock (Sync)
			{
				if (state > 4)
					return true;
					state = 5;
					return false;
			}
		}
		/// <summary>
		/// Отправляет данные текущему подключению
		/// </summary>
		/// <param name="message">массив байт для отправки</param>
		/// <returns>true в случае ечсли данные можно отправить</returns>
		public bool Message(byte[] message, int start, int write)
		{
			lock (Sync)
			{
				if (state > 4)
					return false;

				SocketError error;
				if ((error = Write(message, start, write)) != SocketError.Success)
				{
					if (error != SocketError.WouldBlock
						   && error != SocketError.NoBufferSpaceAvailable)
					{
						exc( new HTTPException( "Ошибка записи http данных: " + error.ToString() ) );
						Response.Close = true;
						return false;
					}
				}
			}
			return true;
		}
		public void Error(string message, string stack)
		{
			if (Response.SetRes())
				return;

			if (string.IsNullOrEmpty(stack))
				stack = string.Empty;
			if (string.IsNullOrEmpty(message))
				message = string.Empty;
			Response.StartString = "Http/1.1 503 BAD";
			Response.Add("Content-Type", "text/html; charset=utf-8");
			byte[] __body = Encoding.UTF8.GetBytes(
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
			"</html>");

			Response.Add("Content-Length", __body.Length.ToString());
			byte[] header = Response.ToByte();

			if (Message(header, 0, header.Length))
				Message(__body, 0, __body.Length);
			Response.SetEnd();
			
		}
        public override TaskResult TaskLoopHandlerProtocol()
        {
			try
			{
				if (state ==-1)
				{
					Work();
					if (Interlocked.CompareExchange(ref state, 2,-1) !=-1)
						return TaskResult;
					if (Response != null && (!Response.IsEnd || !Writer.Empty))
						write();
					else
					{
						if (Response != null && Response.Close)
							state = 5;
						else
						{
							state = 0;
							Request = new Header();
							Response = new Header();
						}
					}
					if (Interlocked.CompareExchange(ref state,-1, 2) == 2)
						return TaskResult;
				}
				if (state == 0)
				{
					Work();
					if (Interlocked.CompareExchange(ref state, 1, 0) != 0)
						return TaskResult;
					if (!Request.IsReq)
					{
						read();
					if (state == 1)
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
						return TaskResult;
				}
				
								if (state == 5)
								{
									Close();
									state = 7;
								}
								if (state == 7)
								{
									Dispose();
									TaskResult.Option = TaskOption.Delete;
								}
			}
			catch (HTTPException exc)
			{
				Error(exc);
			}
			return TaskResult;
        }
        public override string ToString()
        {
        	return "HTTP";
        }
		/// <summary>
		/// Обрабатывает происходящие ошибки и назначает оьраьотчики
		/// </summary>
		/// <param name="err">Ошибка WebSocket</param>
		private void exc(HTTPException err)
		{
			lock (Sync)
			{
				if (state < 7)
				{
					state = 4;
					Error(err);
					
					if (Response.Close)
						Close(string.Empty);
					else
						Error(err.Message, err.StackTrace);
				}
				Interlocked.CompareExchange(ref state, 7, 4);
			}
		}
		/// <summary>
		/// 
		/// </summary>
		private void read()
		{
			SocketError error;
			if ((error = Read()) != SocketError.Success)
			{
				if (error != SocketError.WouldBlock
					&& error != SocketError.NoBufferSpaceAvailable)
				{
					exc( new HTTPException("Ошибка чтения http данных: " + error.ToString()));
					Response.Close = true;
				}
			}
		}
		/// <summary>
		/// Отправляет сообщение
		/// </summary>
		/// <param name="data">Данные</param>
		private void write()
		{
			if (Writer.Empty)
				return;
			SocketError error;
			if ((error = Send()) != SocketError.Success)
			{
				if (error != SocketError.WouldBlock
					&& error != SocketError.NoBufferSpaceAvailable)
				{
						exc( new HTTPException("Ошибка записи http данных: " + error.ToString()));
						Response.Close = true;
				}
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
				e = __EventOnOpen;
			if (e != null)
				e(this, new PEventArgs(s, m, null));
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
