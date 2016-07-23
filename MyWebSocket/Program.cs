using System;
using System.Collections.Generic;
using System.Text;
using MyWebSocket.Tcp;
using MyWebSocket.Tcp.Protocol;

using MyWebSocket.Tcp.Protocol.HTTP;

using MyWebSocket.Tcp.Protocol.WS;
using MyWebSocket.Tcp.Protocol.WS.WS_13;

namespace Example
{
	static class WSHandler
	{
		public static PHandlerEvent Data = async (object obj, PEventArgs a) =>
		{
			WSContext_13_R ctx =
					a.sender as WSContext_13_R;
            
            WSContext_13_W cntx = (WSContext_13_W)ctx.Context();
            await cntx.AsMssg("Привет я плучил твое сообщение " + 
							  Encoding.UTF8.GetString(ctx.Request[0].DataBody));
		};
		public static PHandlerEvent Error = (object sender, PEventArgs a) =>
		{
			Console.WriteLine(a.sender.ToString());
		};
		public static PHandlerEvent Close = (object sender, PEventArgs a) =>
		{
			Console.WriteLine("WS соединение закрыто");
		};
	}
	static class HTTPHandler
	{
		static Queue<IContext> Container = new Queue<IContext>();
		public static PHandlerEvent Data = async (object obj, PEventArgs a) =>
		{
			try
			{
				HTTPContext ctx = 
					a.sender as HTTPContext;

				switch (ctx.Request.Path)
				{
					case "/":
						if (await ctx.AsFile("Html/index.html"))
							ctx.End();
						break;
					case "/message":
						lock (Container)
						{
							foreach (HTTPContext c in Container)
							{
								try
								{
									c.Message(ctx.Request.Body);
									c.End();
								}
								catch (Exception exc)
								{
									;
								}
							}
						}

						ctx.Message(string.Empty);
						ctx.End();

						break;
					case "/subscribe":
						lock (Container)
							Container.Enqueue(ctx);
						break;
					default:
						if (await ctx.AsFile("Html" + ctx.Request.Path))
							ctx.End();
						break;
				}
			}
			catch (Exception exc)
			{
				;
			}
		};
		public static PHandlerEvent Error = (object sender, PEventArgs a) =>
		{
			Console.WriteLine(a.sender.ToString());
		};
		public static PHandlerEvent Close = (object sender, PEventArgs a) =>
		{
			Console.WriteLine("HTTP соединение закрыто");
		};
	}
	class Program
    {
        static void Main(string[] args)
        {
			Console.WriteLine("sdasdasd");
			HTTProtocol.EventConnect += (object obj, PEventArgs a) =>
			{
				Console.WriteLine("HTTP");
				
				HTTProtocol Http = obj as HTTProtocol;
				Http.EventData += HTTPHandler.Data;
				Http.EventError += HTTPHandler.Error;
				Http.EventClose += HTTPHandler.Close;

				Http.EventOnOpen += (object sender, PEventArgs e) =>
				{
					Console.WriteLine("Соединение Http Установлено");
					
					HTTPContext ctx =
					e.sender as HTTPContext;

					if (!string.IsNullOrEmpty(ctx.Request.Upgrade))
					{
						Http.EventData -= HTTPHandler.Data;
						Http.EventError -= HTTPHandler.Error;
						Http.EventClose -= HTTPHandler.Close;

						Http.EventData += WSHandler.Data;
						Http.EventError += WSHandler.Error;
						Http.EventClose += WSHandler.Close;
					}
				};
			};
			
			WServer Server = new WServer("0.0.0.0", 443, 2);
		}
	}
}
