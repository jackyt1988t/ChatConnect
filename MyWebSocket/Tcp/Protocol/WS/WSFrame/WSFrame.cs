namespace MyWebSocket.Tcp.Protocol.WS
{
	enum WSOpcod : int
	{
		Text = 0,
		Ping = 1,
		Pong = 2,
		Close = 3,
		Binnary = 4,
		Continue = 5
	}
}
