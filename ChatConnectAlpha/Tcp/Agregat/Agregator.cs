﻿using System;
using System.Net.Sockets;
	using System.Threading;
	using System.Collections.Concurrent;

using ChatConnect.Tcp.Protocol;
using ChatConnect.Tcp.Protocol.WS;
using ChatConnect.Tcp.Protocol.HTTP;

namespace ChatConnect.Tcp
{
	class Agregator
	{
		public IProtocol Protocol;

		private long _nextloop = 0;
		private static PHandlerEvent Connect;
		private static ConcurrentQueue<Agregator> Container;

		static Agregator()
		{
			Container = new ConcurrentQueue<Agregator>();
		}
		public Agregator(Socket tcp)
		{
			Protocol = new HTTPProtocol( tcp, Connect );
			Container.Enqueue(this);
		}
 static public void loop()
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
						agregator.Protocol.TaskLoopHandler();
						if (loop++ > 500)
						{
							loop = 0;
							Thread.Sleep(2);
						}
						Container.Enqueue(agregator);
					}
				}
				catch (FieldAccessException exc)
				{
					Console.WriteLine("Обработчик: " + exc.Message);
				}
			}
		}
 static public void Loop()
		{
			short loop = 0;
			while ( true )
			{
				try
				{
					Agregator agregator = null;
					if (!Container.TryDequeue(out agregator))
						Thread.Sleep(1);
					else
					{
						agregator.TaskLoopHandler();
						if (loop++ > 200)
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