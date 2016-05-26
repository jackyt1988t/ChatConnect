﻿using System;
using System.Text;

using ChatConnect.Tcp;
using ChatConnect.Tcp.Protocol;
using ChatConnect.Tcp.Protocol.WS;


namespace ChatConnect
{
    class Program
    {
        static void Main(string[] args)
        {
			//WS.Debug = true;
			WS.EventConnect += (object obj, PEventArgs e) =>
			{
				// Объект WebSocket
				WS WebSocket = obj as WS;
				// Событие наступает когда приходят новые данные
				WebSocket.EventData += (object sender, PEventArgs ev) =>
				{
					WSData binnary = ev.sender as WSData;
					if (binnary.Opcod != WSOpcod.Text)
					{
						byte[] _buffer = binnary.ToByte(); 
					}
					else
					{
						Console.WriteLine(binnary.ToString());
						// Отправляем текстовый фрейм
						WebSocket.Message(binnary.ToString());
					}
				};
				// Событие наступает если произошла ошибка данных
				WebSocket.EventError += (object sender, PEventArgs ev) =>
				{
					Console.WriteLine(ev.sender.ToString());
				};
				// Событие наступает если соединение было закрыто
				WebSocket.EventClose += (object sender, PEventArgs ev) =>
				{
					Console.WriteLine(ev.sender.ToString());
				};
			};
			WServer Server = new WServer("0.0.0.0", 8081, 2);
			
        }
    }
}
