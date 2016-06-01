# MyWebSocket
## WebSocket Server написанный на языке c#.
Поддержка .NET Framework 4.5, Mono 4.2 <br>
http://jackyt1988t.github.io/WebSocket <br>
Пример простейшего WebSocket echo сервера <br> 

```C#
using System;
using System.Text;

using ChatConnect.Tcp;
using ChatConnect.Tcp.Protocol;
using ChatConnect.Tcp.Protocol.WS;
using ChatConnect.Tcp.Protocol.HTTP;


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
					WSData data = e.sender as WSData;
					if (data.Opcod != WSOpcod.Text)
					{
						
						byte[] raw = data.ToByte(); 
					}
					else
					{
						Console.WriteLine(data.ToString());
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

			// Обрабатываем новый http запрос
			HTTP.EventConnect += (object obj, PEventArgs a) =>
			{
				// Объект Http
				HTTP Http = obj as HTTP;
	
				switch (Http.Request.Path)
				{
					case "/":
		  				// отправляем index.html
						Http.File("Html/index.html", "html");
						break;
					default:
						Http.File("Html" + Http.Request.Path, Http.Request.File);
						break;
				}
				// Вывод информации о полученных заголовках
				Console.WriteLine(Http.Request.ToString());
			};
			// Запускаем сервер с указанным адресом и портом, с 2 потоками параллельной обработки соединений
			WServer Server = new WServer("0.0.0.0", 8081, 2);
			
        }
    }
}
