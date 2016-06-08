using System;
using MyWebSocket.Tcp.Protocol;
using MyWebSocket.Tcp.Protocol.WS;

namespace MyWebSocket.Test
{
	class Program
	{
		static void Main(string[] args)
		{
			WS.Debug = true;

			TimeSpan interval = 
				new TimeSpan(DateTime.Now.Ticks + TimeSpan.TicksPerSecond * 2);
			using (wsProtocolN13 WebSocket = new wsProtocolN13( "127.0.0.1", 8081 ))
			{
				WebSocket.EventWork += (object sender, PEventArgs e) =>
				{
					if (interval.Ticks < DateTime.Now.Ticks)
					{
						interval =
							new TimeSpan(DateTime.Now.Ticks + TimeSpan.TicksPerSecond * 10);

						WebSocket.Message("Привет");
					}
				};
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

					WebSocket.Connection();
				};

				WebSocket.Connection();
			}
		}
	}
}
