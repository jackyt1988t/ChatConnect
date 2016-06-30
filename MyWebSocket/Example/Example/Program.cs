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
            List<HTTP> Polling = new List<HTTP>();
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
                                Polling[i].Flush(text);
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

            HTTP.EventConnect += (object obj, PEventArgs a) =>
            {
                Console.WriteLine("HTTP");

                bool poll = false;
                HTTP Http = obj as HTTP;
                Http.EventData += (object sender, PEventArgs e) =>
                {
                    switch (Http.Request.Path)
                    {
                        case "/":
                            Http.File("Html/index.html");
                        break;
                        case "/message":
                        lock (Array)
                        {
                            for (int i = 0; i < Array.Count; i++)
                            {
                                Array[i].Message(Http.Request.Body, WSOpcod.Text, WSFin.Last);
                            }
                        }
                        lock (Polling)
                        {
                            for (int i = 0; i < Polling.Count; i++)
                            {
                                Polling[0].Flush(Http.Request.Body);
								Polling.RemoveAt(0);
                            }
							Http.Flush("Данные получены");
                        }
                        break;
                        case "/subscribe":
							poll = true;
                            lock (Polling)
                                Polling.Add(Http);
                        break;
                        default:
                            Http.File("Html" + Http.Request.Path);
                        break;
                    }
                };
                Http.EventError += (object sender, PEventArgs e) =>
                {
                    Console.WriteLine("ERROR ." + e.sender.ToString());
                };
                Http.EventClose += (object sender, PEventArgs e) =>
                {
                     poll = false;
                        lock (Polling)
                            Polling.Remove(Http);
                    Console.WriteLine("CLOSE");
                };
                Http.EventOnOpen += (object sender, PEventArgs e) =>
                {
                    Console.WriteLine("Соединение Http Установлено");
                };
            };
                    WServer Server = new WServer("0.0.0.0", 8081, 2);
        }
    }
}
