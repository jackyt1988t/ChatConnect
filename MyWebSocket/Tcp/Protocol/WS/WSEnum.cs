namespace MyWebSocket.Tcp.Protocol.WS
{
	enum WsError : int 
	{
		PongBodyIncorrect = 0,
		PingNotResponse = 1,
		BodyWaitLimit = 2,
		PcodNotSuported = 3,
		PcodNotRepeat = 4,
		HeaderFrameError = 5,
		BodyFrameError = 6,
		HandshakeError = 7,
		BufferLimitLength = 8,
		CriticalError = 9
	}
}
