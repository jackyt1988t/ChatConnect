using System;
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
			WS.EventConnect += (object obj, PEventArgs a) =>
			{
				// Объект WebSocket
				WS WebSocket = obj as WS;
				// Событие наступает когда приходят новые данные
				WebSocket.EventData += (object sender, PEventArgs e) =>
				{
					WSData data = e.sender as WSData;
					if (data.Opcod != WSOpcod.Text)
					{
						
						byte[] raw = data.ToByte(); 
					}
					else
					{
						//Console.WriteLine(data.ToString());
						// Отправляем текстовый фрейм
						WebSocket.Message(data.ToString());
					}
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
			WServer Server = new WServer("0.0.0.0", 8081, 2);
			
        }
    }
}
