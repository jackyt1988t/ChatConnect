namespace MyWebSocket.Tcp.Protocol.WS
{
	public enum WSClose
	{
		Normal		  = 1000,
		GoingAway	  = 1001,
		ProtocolError     = 1002,
		UnsupportedData   = 1003,
		Reserved	  = 1004,
		NoStatusRcvd	  = 1005,
		Abnormal	  = 1006,
		InvalidFrame	  = 1007,
		PolicyViolation	  = 1008,
		BigMessage	  = 1009,
		Mandatory	  = 1010,
		ServerError	  = 1011,
		TLSHandshake	  = 1012
	}
}
