using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

using ChatConnect.Tcp.Protocol;
using ChatConnect.Tcp.Protocol.WS;
using ChatConnect.Tcp.Protocol.HTTP;
using System.Collections.Generic;

namespace ChatConnect.Tcp
{
	class Agregator
	{
		public IProtocol Protocol;

		private long _nextloop = 0;
		private static PHandlerEvent Connect;
		private static Queue<Agregator> Work;
		private static ConcurrentQueue<Agregator> Read;
		private static ConcurrentQueue<Agregator> Write;
		private static ConcurrentQueue<Agregator> Container;

		static Agregator()
		{
			Work = new Queue<Agregator>();
			Read = new ConcurrentQueue<Agregator>();
			Write = new ConcurrentQueue<Agregator>();
			Container = new ConcurrentQueue<Agregator>();
		}
		public Agregator(Socket tcp)
		{
			Protocol = new HTTPProtocol(  tcp, Connect  );
			Container.Enqueue(this);
		}
		static public void Loop()
		{
			short loop = 0;
			while (true)
			{
				try
				{
					Agregator agregator = null;
					if (!Container.TryDequeue(out agregator))
						Thread.Sleep(1);
					else
					{
						agregator.TaskLoopHandler();
						if (loop++ > 500)
						{
							loop = 0;
							Thread.Sleep(1);
						}
					}
				}
				catch (FieldAccessException exc)
				{
					Console.WriteLine("Обработчик: " + exc.Message);
				}
			}
		}
		static public void WorkLoop()
		{
			short loop = 0;
			while (true)
			{
				try
				{
					Agregator agregator = null;
					if (Work.Count == 0)
						Thread.Sleep(1);
					else
					{
						agregator = Work.Dequeue();
						if (agregator == null)
							continue;
						agregator.TaskLoopWork();
						if (loop++ > 500)
						{
							loop = 0;
							Thread.Sleep(1);
						}
					}
				}
				catch (FieldAccessException exc)
				{
					Console.WriteLine("Обработчик: " + exc.Message);
				}
			}
		}
		static public void ReadLoop()
		{
			short loop = 0;
			while (true)
			{
				try
				{
					Agregator agregator = null;
					if (!Read.TryDequeue(out agregator))
						Thread.Sleep(1);
					else
					{
						agregator.TaskLoopRead();
						if (loop++ > 500)
						{
							loop = 0;
							Thread.Sleep(1);
						}
					}
				}
				catch (FieldAccessException exc)
				{
					Console.WriteLine("Обработчик: " + exc.Message);
				}
			}
		}
		static public void WriteLoop()
		{
			short loop = 0;
			while ( true )
			{
				try
				{
					Agregator agregator = null;
					if (!Write.TryDequeue(out agregator))
						Thread.Sleep(1);
					else
					{
						agregator.TaskLoopWrite();
						if (loop++ > 500)
						{
							loop = 0;
							Thread.Sleep(1);
						}
					}
				}
				catch ( FieldAccessException exc )
				{
					Console.WriteLine("Обработчик: " + exc.Message);
				}
			}
		}
 static public void Connection(PHandlerEvent connect)
		{
			Connect = connect;
		}
		private void TaskLoopHandler()
		{
			TaskResult TaskResult = Protocol.TaskLoopHandlerProtocol();
			switch (TaskResult.Option)
			{
				case TaskOption.Loop:
					Container.Enqueue(this);
					break;
				case TaskOption.Work:
					Work.Enqueue(this);
					break;
				case TaskOption.Read:
					Read.Enqueue(this);
					break;
				case TaskOption.Write:
					Write.Enqueue(this);
					break;
				case TaskOption.Protocol:
					if (TaskResult.Protocol == TaskProtocol.WSRFC76)
						Protocol =
						   new WSProtocol7(Protocol);
					Container.Enqueue(this);
					break;
				case TaskOption.Threading:
					Thread thr = new Thread(TaskLoopThreading);
					thr.IsBackground = true;
					thr.Start();
					break;
			}
		}
		private void TaskLoopWork()
		{
			TaskResult TaskResult = Protocol.LoopWork();
			switch (TaskResult.Option)
			{
				case TaskOption.Work:
					TaskResult.Option = TaskOption.Read;
					Work.Enqueue(this);
					Read.Enqueue(this);
					break;
				case TaskOption.Delete:
					break;
				default:
					Work.Enqueue(this);
					break;
			}
		}
		private void TaskLoopRead()
		{
			TaskResult TaskResult = Protocol.LoopRead();
			switch (TaskResult.Option)
			{
				case TaskOption.Loop:
					Container.Enqueue(this);
					break;
				case TaskOption.Work:
					Work.Enqueue(this);
					break;
				case TaskOption.Read:
					Read.Enqueue(this);
					break;
				case TaskOption.Write:
					Write.Enqueue(this);
					break;
				case TaskOption.Protocol:
					if (TaskResult.Protocol == TaskProtocol.WSRFC76)
						Protocol =
						   new WSProtocol7(Protocol);
					Container.Enqueue(this);
					break;
				case TaskOption.Threading:
					Thread thr = new Thread(TaskLoopThreading);
					thr.IsBackground = true;
					thr.Start();
					break;
			}
		}
		private void TaskLoopWrite()
		{
				TaskResult TaskResult = Protocol.LoopWrite();
			switch (TaskResult.Option)
			{
				case TaskOption.Loop:
					Container.Enqueue(this);
					break;
				case TaskOption.Work:
					Work.Enqueue(this);
					break;
				case TaskOption.Read:
					Read.Enqueue(this);
					break;
				case TaskOption.Write:
					Write.Enqueue(this);
					break;
				case TaskOption.Protocol:
					if (TaskResult.Protocol == TaskProtocol.WSRFC76)
						Protocol =
						   new WSProtocol7(Protocol);
					Container.Enqueue(this);
					break;
				case TaskOption.Threading:
						Thread thr = new Thread( TaskLoopThreading );
							   thr.IsBackground = true;
							   thr.Start();
						break;
			}
		}
		private void TaskLoopThreading()
		{
					while (true)
					{
						TaskResult TaskResult = Protocol.TaskLoopHandlerProtocol();
						switch (TaskResult.Option)
						{
							case TaskOption.Loop:
								break;
							case TaskOption.Delete:
								return;
							default:
								throw new ArgumentException("TaskResult");
						}
						Thread.Sleep(1);
					}
		} 
	}
}