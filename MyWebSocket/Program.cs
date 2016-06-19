using System;
using System.Text;

using MyWebSocket.Tcp;
using MyWebSocket.Tcp.Protocol;
using MyWebSocket.Tcp.Protocol.WS;
using MyWebSocket.Tcp.Protocol.HTTP;
using System.Collections.Generic;

namespace MyWebSocket
{
    class Program
    {
        static void Main(string[] args)
        {
			//WS.Debug = true;
			// Список подписчиков
			List<WS> Array = new List<WS>();
			List<HTTP> Pollings = new List<HTTP>();
			WS.EventConnect += (object obj, PEventArgs a) =>
			{
				// Объект WebSocket
				WS WebSocket = obj as WS;
				lock (Array)
					Array.Add(WebSocket);
				// Событие наступает когда приходят новые данные
				string message = "";
				WebSocket.EventData += (object sender, PEventArgs e) =>
				{
					WSData data = e.sender as WSData;
					message += data.ToString();
					lock (Array)
					{
						for (int i = 0; i < Array.Count; i++)
						{
							Array[i].Message(message);
						}
					}
					lock (Pollings)
					{
						for (int i = 0; i < Pollings.Count; i++)
						{
							Pollings[i].Flush(message);
						}
					}
					message = "";
				};
				WebSocket.EventChunk += (object sender, PEventArgs e) =>
				{
					WSData data = e.sender as WSData;
					message += Encoding.UTF8.GetString(data._Data);
				};
				// Событие наступает если произошла ошибка данных
				WebSocket.EventError += (object sender, PEventArgs e) =>
				{
					Console.WriteLine(e.sender.ToString());
				};
				// Событие наступает если соединение было закрыто
				WebSocket.EventClose += (object sender, PEventArgs e) =>
				{
					lock (Array)
						Array.Remove(WebSocket);
					Console.WriteLine(e.sender.ToString());
				};
			};
			// Обрабатываем новый http запрос
			HTTP.EventConnect += (object obj, PEventArgs a) =>
			{
				// Объект Http
				HTTP Http = obj as HTTP;
				bool polling = false;
				// Событие наступает когда приходят новые данные
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
							lock (Pollings)
							{
								for (int i = 0; i < Pollings.Count; i++)
								{
									Pollings[i].Flush(Http.Request.Body);
								}
								if (!polling)
								{
									Http.Message("Работает...");
									Http.Flush();
								}
							}
							break;
						case "/subscribe":
							
							lock (Pollings)
							{
								if (!polling)
								{
									polling = true;
									Pollings.Add(Http);
								}
							}
							break;
						default:
							Http.File("Html" + Http.Request.Path);
							break;
					}
				};
				Http.EventError += (object sender, PEventArgs e) =>
				{
					HTTPException err = e.sender as HTTPException;
					Console.WriteLine(err.Message);
				};
				Http.EventClose += (object sender, PEventArgs e) =>
				{
					if (polling)
					{
						lock (Pollings)
							Pollings.Remove(Http);
					}
					Console.WriteLine("CLOSE");
				};
				// События наступает когда приходят заголовки
				Http.EventOnOpen += (object sender, PEventArgs e) =>
				{	
					// здесь можно проверить заголовки и ели необходимо закрыть моединение
					Console.WriteLine("*OPEN*");
				};
			};
			WServer Server = new WServer("0.0.0.0", 8081, 2);
			
        }
    }
}
