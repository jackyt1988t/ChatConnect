namespace MyWebSocket.Tcp.Protocol.WS
{
	public enum WSOpcod : int
	{
		None = 0,
		Text = 1,
		Ping = 2,
		Pong = 3,
		Close = 4,
		Binnary = 5,
		Continue = 6
	}
}
