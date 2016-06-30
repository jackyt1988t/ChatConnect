using System;
using System.Collections.Generic;
 
using MyWebSocket.Tcp;
using MyWebSocket.Tcp.Protocol;
using MyWebSocket.Tcp.Protocol.WS;
using MyWebSocket.Tcp.Protocol.HTTP;


namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            List<WS> Array = new List<WS>();
            List<HTTPContext> Polling = new List<HTTPContext>();
            WS.EventConnect += (object o, PEventArgs e) =>
            {
                WS ws = o as WS;

                List<string> Text = new List<string>();
                ws.EventData += (object obj, PEventArgs a) =>
                {
                    WSData data = a.sender as WSData;

                    if (data.Opcod  ==  WSOpcod.Text)
                    {
                        Text.Add(data.ToString());

                        string text = string.Empty;
                        for (int i = 0; i < Text.Count; i++)
                        {
                            text += Text[i];
                        }
                        Text.Clear();

                        lock (Array)
                        {
                            for (int i = 0; i < Array.Count; i++)
                            {
                                Array[i].Message(text);
                            }
                        }
                        lock (Polling)
                        {
                            for (int i = 0; i < Polling.Count; i++)
                            {
                                Polling[i].Message(text);
								Polling[i].End();

							}
                        }
                    }
                    else
                    {
                        ws.Close(WSClose.Normal);
                    }

                };
                ws.EventChunk += (object obj, PEventArgs a) =>
                {
                    WSData data = a.sender as WSData;

                    if (data.Opcod  ==  WSOpcod.Text)
                    {
                        Text.Add(data.ToString());
                    }
                    else
                    {
                        ws.Close(WSClose.Normal);
                    }
                };
                ws.EventError += (object sender, PEventArgs a) =>
                {
                    Console.WriteLine(a.sender.ToString());
                };
                ws.EventClose += (object sender, PEventArgs a) =>
                {
                    lock (Array)
                        Array.Remove(ws);
                    Console.WriteLine(a.sender.ToString());
                };
                ws.EventOnOpen += (object sender, PEventArgs a) =>
                {
                    lock (Array)
                        Array.Add(ws);
                    Console.WriteLine("Соединение WS Установлено");
                };
            };

            HTTProtocol.EventConnect += (object obj, PEventArgs a) =>
            {
                Console.WriteLine("HTTP");

                bool poll = false;
                HTTProtocol Http = obj as HTTProtocol;
                Http.EventData += async (object sender, PEventArgs e) =>
                {
					HTTPContext ctx = e.sender as HTTPContext;
                    switch (ctx.Request.Path)
                    {
                        case "/":
							await ctx.AsFile("Html/index.html");
							ctx.End();
							break;
                        case "/message":
                        lock (Array)
                        {
                            for (int i = 0; i < Array.Count; i++)
                            {
                                Array[i].Message(ctx.Request.Body, WSOpcod.Text, WSFin.Last);
                            }
                        }
                        lock (Polling)
                        {
							int count = Polling.Count;
							for (int i = 0; i < count; i++)
                            {
                                Polling[0].Message(ctx.Request.Body);
								Polling[0].End();
								Polling.RemoveAt(0);
                            }
							ctx.Message("Данные получены");
							ctx.End();
                        }
                        break;
                        case "/subscribe":
						if (poll)
							;
							poll = true;
							lock (Polling)
								Polling.Add(ctx);
                        break;
                        default:
							await ctx.AsFile("Html" + ctx.Request.Path);
							ctx.End();
							break;
                    }
                };
                Http.EventError += (object sender, PEventArgs e) =>
                {
                    Console.WriteLine("ERROR");
                };
                Http.EventClose += (object sender, PEventArgs e) =>
                {
					HTTPContext[] cntx = 
							e.sender as HTTPContext[];
					foreach (HTTPContext ctx in cntx)
					{
						if (ctx.Request.Path == "/subscribe")
						{
							lock (Polling)
								Polling.Remove(ctx);
						}
					}
                    Console.WriteLine("CLOSE");
                };
                Http.EventOnOpen += (object sender, PEventArgs e) =>
                {
                    Console.WriteLine("Соединение Http Установлено");
                };
            };
					//Log.Loging.Mode = Log.Log.Debug;
                    WServer Server = new WServer("0.0.0.0", 8081, 2);
        }
    }
}
