# MyWebSocket
## WebSocket Server написанный на языке c#.
<div>
	Поддержка .NET Framework 4.5, Mono 4.2 <br>
	http://jackyt1988t.github.io/WebSocket <br>
	В данный момент поддерживается: <br>
	WebSocket Протокол Sample - требует тестов https://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-03 <br>
	WebSocket Протокол №13(RFC6455) - требует тестов https://tools.ietf.org/html/rfc6455 <br>
	ПланируетсЯ поддержка всех протоколов.
	Пример простейшего WebSocket echo сервера <br> 
	Данный пример показывает как запустить  WebSocket Сервер и зарегистировать обработчики событий
</div>
```C#
using System;
using System.Text;

using ChatConnect.Tcp;
using ChatConnect.Tcp.Protocol;
using ChatConnect.Tcp.Protocol.WS;

// Включить вывод отладочной информации
//WS.Debug = true;
			WS.EventConnect += (object obj, PEventArgs a) =>
			{
				// Объект WebSocket
				WS WebSocket = obj as WS;
				// Максимально допустимая длинна фрейма
				WebSocket.Pollicy.MaxLeng = 32000;
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
// Запуск серверами с указанным адресом и номером порта с 2 потоками обработки подключений
WServer Server = new WServer("0.0.0.0", 8081, 2);
```
<div>Отладочная информация</div>
<img src="https://github.com/jackyt1988t/WebSocket/blob/master/MyWebSocketDebug.png" alt="Отладочная информация">
<div>
	Средствами сервера можно отдавать статические фалы по http протоколу
	Как и для WebSocket необходимо зарегистрировать обработчик события полученного запроса
</div>
```C#
// Список подписчиков
			List<HTTP> Pollings = new List<HTTP>();
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
							lock (Pollings)
							{
								for (int i = 0; i < Pollings.Count; i++)
								{
									Pollings[i].Response.StartString = "HTTP/1.1 200 OK";
									Pollings[i].Response.ContentType = "text/plain; charset=utf-8";
									Pollings[i].Message(Http.Request._Body);
									Pollings[i].flush();
								}
								Http.Response.StartString = "HTTP/1.1 200 OK";
								Http.Message(string.Empty);
								Http.flush();
						}
							break;
						case "/subscribe":
							polling = true;
							lock (Pollings)
								Pollings.Add(Http);
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
					// здесь можно проверить заголовки и еcли необходимо закрыть cоединение
					Console.WriteLine("*OPEN*");
				};
			};
```
