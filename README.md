# MyWebSocket
## WebSocket Server написанный на языке c#.
Поддержка .NET Framework 4.5, Mono 4.2 <br>
http://jackyt1988t.github.io/MyWebSocket <br>
Пример простейшего WebSocket echo сервера <br> 

```C#
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
        	// Включить вывод отладочной информации
        	WS.Debug = true; 
			WS.EventConnect += (object obj, PEventArgs a) =>
			{
				// Объект WebSocket
				WS WebSocket = obj as WS;
				// Событие наступает когда приходят новые данные
				WebSocket.EventData += (object sender, PEventArgs e) =>
				{
					
					if (binnary.Opcod == WSOpcod.Text)
					{
						Console.WriteLine(e.sender.ToString());
						// Отправляем текстовый фрейм
						WebSocket.Message(e.sender.ToString());
						
					}
					else
					{
						WSBinnary data = e.sender as WSBinnary;
						byte[] buffer = data.ToByte(); 	
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
			// Запускаем сервер с указанным адресом и портом, с 2 потоками параллельной обработки соединений
			WServer Server = new WServer("0.0.0.0", 8081, 2);
			
        }
    }
}
