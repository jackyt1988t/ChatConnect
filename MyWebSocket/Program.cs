using System;
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
			// Обрабатываем новый http запрос
			HTTP.EventConnect += (object obj, PEventArgs a) =>
			{
				// Объект Http
				HTTP Http = obj as HTTP;
				Http.EventError += (object sender, PEventArgs e) =>
				{
					HTTPException err = e.sender as HTTPException;
					Console.WriteLine(err.Message);
				};
				// Событие наступает когда приходят новые данные
				Http.EventOnOpen += (object sender, PEventArgs e) =>
				{
					switch (Http.Request.Path)
					{
						case "/":
							Http.MessageFile("Html/index.html", "html");
							break;
						default:
							Http.MessageFile("Html" + Http.Request.Path, Http.Request.File);
							break;
					}
				};
			};
			WServer Server = new WServer("0.0.0.0", 8081, 2);
			
        }
    }
}
