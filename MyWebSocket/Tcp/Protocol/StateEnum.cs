namespace MyWebSocket.Tcp.Protocol
{
    public enum States : int
    {
        Work = 0,
        Data = 1,
        Send = 2,
        work = -1,		
        Error = 4,
        Close = 5,
        Upgrade = 8,
		Connection = 3,       
        Disconnect = 7,
    }
}
