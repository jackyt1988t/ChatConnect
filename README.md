# MyWebSocket
## WebSocket Server написанный на языке c#.
<div>
	Поддержка .NET Framework 4.5, Mono 4.2 <br>
	http://jackyt1988t.github.io/WebSocket <br>
	В данный момент поддерживается: <br>
	WebSocket Протокол Sample - требует тестов https://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-03 <br>
	WebSocket Протокол №13(RFC6455) - требует тестов https://tools.ietf.org/html/rfc6455 <br>
</div>
    '

## С чего начать?

<div>
	Для того чтобы начать обрабатывать входящие подключения WebSocket клиентов необходимо подписаться на статическое
	событие EventConnect класса ws, событие наступает если клиент инициирует переход c протокола http на websocket.
	Был был указан заголвок Upgrade: websocket и версия websocket поддерживается данной реализацией.
</div>

```C#

	using ChatConnect.Tcp;
	using ChatConnect.Tcp.Protocol.WS;

	WS.EventConnect += (object obj, PEventArgs a) =>
	{
		Console.WriteLine("был выполнен переход на websocket");
	};
	
	// Запуск Сервера
	WServer Server = new WServer("0.0.0.0", 8081, 2);
```

## Как обрабатывать?

<div>
	Если после события EventConnect обработка заголвоков закончится успехом наступит событие EventOnOpen. заголвки
	будут отправлены после обработки всех подписчиков на событие EventOnOpen данного экземпляра websocket.
</div>

```C#

	using ChatConnect.Tcp.Protocol.WS;

	WS.EventConnect += (object obj, PEventArgs a) =>
	{
		Console.WriteLine("был выполнен переход на websocket");
		WS ws = obj as WS;
		ws.EventOnOpen += (object obj, PEventArgs a) =>
		{
			Console.WriteLine("Заголовки были получены и установлены");
			Console.WriteLine("Входящие заголовки:\r\n" + ws.Request.ToString());
			Console.WriteLine("Исходящие заголовки:\r\n" + ws.Response.ToString());
		};
	};

```

<div>
    При получение днных могут произойти несколько событий, точнее два, это EventChunk и EventData. Событие EventChuck
    наступает если удаленная сторона отправляет данные по частям, что предусмотрено всеми версиями websocket, например
    в версии 13 удаленная сторона должна в первом фрейме указать Опкод данных(Text, Binnary) и бит FIN равным 0,
    далее следует 0 или несколько фреймов где опкод равен(Continuation), а бит FIN равен 0 и последним всегда должен
    приходить фрейм с опкодом(Continuation) и битом FIN равным 1, он же может быть одним единственным фреймом. Когда
    все данные получены наступает событие EventData.
</div>

```C#

	using ChatConnect.Tcp.Protocol.WS;

	WS.EventConnect += (object obj, PEventArgs a) =>
	{
		Console.WriteLine("был выполнен переход на websocket");
		WS ws = obj as WS;
		List<byte[]> Data = new List<byte[]>();
		List<string> Text = new List<string>();
		ws.EventData += (object obj, PEventArgs a) =>
		{
			// Информация о полученных данных
			WSData data = e.sender as WSData;

			if (data.Opcod == WSOpcod.Text)
			{
				Text.Add(data.ToString());
				// обрабатываем данные
			}
			else if (data.Opcod == WSOpcod.Binnary)
			{
				Data.Add(data.ToByte());
				// обрабатываем данные 
			}

		};
		ws.EventChunk += (object obj, PEventArgs a) =>
		{
			if (data.Opcod == WSOpcod.Text)
			{
				Text.Add(data.ToString());
			}
			else if (data.Opcod == WSOpcod.Binnary)
			{
				Data.Add(data.ToByte());
			}
		};
	};

```

<div>
	Последним всегда наступает событие EventClose было соединение закрыто чисто или произошла ошибка.
</div>

```C#

	using ChatConnect.Tcp.Protocol.WS;

	WS.EventConnect += (object obj, PEventArgs a) =>
	{
		Console.WriteLine("был выполнен переход на websocket");
		WS ws = obj as WS;
		ws.EventClose += (object obj, PEventArgs a) =>
		{
			Console.WriteLine(e.sender.ToString());
		};
	};

```

## Что с ошибками?

<div>
	Все критисекие ошибки будут записаны в файл log.log где раполагается запущенный сервер. Ошибки обрабатывается
	в событие EventError
</div>

```C#

	using ChatConnect.Tcp.Protocol.WS;

	WS.EventConnect += (object obj, PEventArgs a) =>
	{
		Console.WriteLine("был выполнен переход на websocket");
		WS ws = obj as WS;
		ws.EventError += (object obj, PEventArgs a) =>
		{
			Console.WriteLine(e.sender.ToString());
		};
	};

```

<div>
	Чтобы включить отладочную информацию необходимо установить свойство WS.Debug = true
	Отладочная информация:
</div>
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
