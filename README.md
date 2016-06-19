# MyWebSocket
## WebSocket Server написанный на языке c#.
<div>
	Поддержка .NET Framework 4.5, Mono 4.2 <br>
	http://jackyt1988t.github.io/WebSocket <br>
	В данный момент поддерживается: <br>
	WebSocket Протокол Sample - требует тестов https://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-03 <br>
	WebSocket Протокол №13(RFC6455) - требует тестов https://tools.ietf.org/html/rfc6455 <br>
</div>

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

# Средствами сервера можно обрабатывать HTTP запросы и устанавливать LongPolling соединения

## С чего начать?

<div>
	Для того чтобы начать обрабатывать входящие подключения HTTP необходимо подписаться на статическое
	событие EventConnect класса HTTP, событие наступает когда устанавливается новое tcp/ip соеинение.
</div>

```C#
	using ChatConnect.Tcp.Protocol.WS;
	
	HTTP.EventConnect += (object obj, PEventArgs a) =>
	{
		Console.WriteLine("Установлено новое http соедиение");
	};
```

<div>
	после того как будут получены заголвоки и не будет инициирован переход на протокол WebSocket, наступает
	событие EventOnOpen, данное событие наступает перед началом приема данных(если таковые имеются). Так же
	до данного события будут установлены некоторые заголвки:
	Date
    Server
    Connection(если необходимо)
    Content-Encoding(если в заголвоке Accept-Encoding указаны gzip или deflate)
	Transfer-Encoding
	
	В данной реализации поддерживается сжатие gzip и deflate Чтобы не сжимать данные надо присвоить заголвоку 
	Content-encoding null или пустую строку. По умолчанию данные отправляются в кодировке chunked, 
	после отправки всех данных необходимо вызвать ф-цию Flush(), чтобы очистить все буфферы и если используется
	кодмровка chuncked отправить заврешающий блок данных.
</div>

```C#
	using ChatConnect.Tcp.Protocol.WS;
	
	HTTP.EventConnect += (object obj, PEventArgs a) =>
	{
		Console.WriteLine("Установлено новое http соедиение");
		
		HTTP http = obj as HTTP;
		
		ws.EventOnOpen += (object obj, PEventArgs a) =>
		{
			Console.WriteLine("Заголовки были получены и установлены");
			Console.WriteLine("Входящие заголовки:\r\n" + ws.Request.ToString());
			Console.WriteLine("Исходящие заголовки:\r\n" + ws.Response.ToString());
			
		};
	};
```

<div>
	После ролучения всех данных(если имеются) наступает событие EventData. ниже приведен пример обработки longpolling
	и отправка статических файлов. Заголвоки можно изменить только до их отправки, иначе будет выброшено исключение,
	информацию о котором можно будет прочитать в файле log.log в корневой папке с проектом.
</div>

```C#
// Список подписчиков
	List<HTTP> Pollings = new List<HTTP>();
	
	HTTP.EventConnect += (object obj, PEventArgs a) =>
	{
		Console.WriteLine("HTTP");
		HTTP Http = obj as HTTP;
		bool polling = false;
		// здесь можно проверить правильность заголовков
		Http.EventOnOpen += (object sender, PEventArgs e) =>
		{	
			Console.WriteLine("OPEN");
		};
		
		// Событие наступает когда приходят новые данные
		Http.EventData += (object sender, PEventArgs e) =>
		{
			switch (Http.Request.Path)
			{
				case "/":
					// асинхроноо отправляет файл
					Http.File("Html/index.html");
					break;
				case "/message":
					lock (Pollings)
					{
						for (int i = 0; i < Pollings.Count; i++)
						{
							Pollings[i].Flush(Http.Request._Body);
						}
						if (!polling)
							Http.Flush("Данные получены");
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
					Console.WriteLine("ERROR");
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
			};
```
