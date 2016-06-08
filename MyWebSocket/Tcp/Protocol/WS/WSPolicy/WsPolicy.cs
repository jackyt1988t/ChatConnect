namespace MyWebSocket.Tcp.Protocol.WS
{
	class WsPolicy
	{
		public int Bit1
		{
			get;
			private set;
		}
		public int Bit2
		{
			get;
			private set;
		}
		public int Bit3
		{
			get;
			private set;
		}
		public int Bit4
		{
			get;
			private set;
		}
		public int Mask
		{
			get;
			private set;
		}
		public long MaxLeng
		{
			get;
			private set;
		}
		public void SetPolicy(int bit1, int bit2, int bit3, int bit4, int mask, long maxleng)
		{
			Bit1 = bit1;
			Bit2 = bit2;
			Bit3 = bit3;
			Bit4 = bit4;
			Mask = mask;
			MaxLeng = maxleng;
		}
	}
	
}
