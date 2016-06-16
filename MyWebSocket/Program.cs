﻿using System;
using System.Text;

using MyWebSocket.Tcp;
using MyWebSocket.Tcp.Protocol;
using MyWebSocket.Tcp.Protocol.WS;
using MyWebSocket.Tcp.Protocol.HTTP;


namespace MyWebSocket
{
    class Program
    {
        static void Main(string[] args)
        {
			//WS.Debug = true;
			WS.EventConnect += (object obj, PEventArgs a) =>
			{
				// Объект WebSocket
				WS WebSocket = obj as WS;
				// Событие наступает когда приходят новые данные
				string message = "";
				WebSocket.EventData += (object sender, PEventArgs e) =>
				{
					WSData data = e.sender as WSData;
					message += data.ToString();
					WebSocket.Message(message);
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
					Console.WriteLine(e.sender.ToString());
				};
			};
			// Обрабатываем новый http запрос
			HTTP.EventConnect += (object obj, PEventArgs a) =>
			{
				// Объект Http
				HTTP Http = obj as HTTP;
				// Событие наступает когда приходят новые данные
				Http.EventData += (object sender, PEventArgs e) =>
				{
					switch (Http.Request.Path)
					{
						case "/":
							Http.File("Html/index.html");
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
					Console.WriteLine("CLOSE");
				};
				// События наступает когда приходят заголовки
				Http.EventOnOpen += (object sender, PEventArgs e) =>
				{
					Console.WriteLine("OPEN");
				};
			};
			WServer Server = new WServer("0.0.0.0", 8081, 2);
			
        }
    }
}
