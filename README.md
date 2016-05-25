# MyWebSocket
WebSocket Server написанный на языке c#.
Пример простейшего WebSocket echo сервера
```C#
  static void Main(string[] args)
  {
		int work = 0;
		int count = 1;
		// Запускаем рабочие потоки которые будут обрабатывать соединения
		while ( work++ < count )
		{
			Thread thr = new Thread(Agregator.Loop);
				thr.IsBackground = true;
				thr.Start();
				Thread.Sleep (100);
		}
		IPEndPoint point = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8081);
		Socket listener = new Socket(point.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		listener.Bind(point);
		listener.Listen(1000);

		LingerOption LOption = new LingerOption(true, 0);
		// Событие наступает когда подключается новый WebSocket клиент
		WS.EventConnect += (object obj, PEventArgs e) =>
		{
		  	// Объект WebSocket
			WS WebSocket = obj as WS;
			// Событие наступает когда приходят новые данные
			WebSocket.EventData += (object sender, PEventArgs ev) =>
			{
				WSBinnary binnary = ev.sender as WSBinnary;
				if (binnary.Opcod == WSOpcod.Text)
				{
				  string text = Encoding.UTF8.GetString(binnary.Data);
				  Console.WriteLine(text);
				  // Отправляем текстовый фрейм
				  WebSocket.Message(text);
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
		while ( true )
		{
			Socket socket = null;
			try
			{
				socket = listener.Accept();
				socket.NoDelay = false;
				socket.Blocking = false;					
				socket.SendBufferSize = 1000 * 64;
				socket.ReceiveBufferSize = 1000 * 16;
				// Обработчик входящих соединений
				Agregator ObjectProtocol = new Agregator(socket);
      			}
			catch ( Exception exc )
			{
				if ( socket != null )
					socket.Dispose();
        			Console.WriteLine ( exc.Message );
			}
		}
  }
