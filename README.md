# MyWebSocket
## WebSocket Server написанный на языке c#.
<div style="size: 11px">
	Поддержка .NET Framework 4.5, Mono 4.2 <br>
	http://jackyt1988t.github.io/WebSocket <br>
	В данный момент поддерживается Протокол 13(RFC6455) https://tools.ietf.org/html/rfc6455<br>
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
```
