namespace MyWebSocket.Tcp.Protocol.WS
{
	public class WsPolicy
	{
		public int Bit1
		{
			get;
			set;
		}
		public int Bit2
		{
			get;
			set;
		}
		public int Bit3
		{
			get;
			set;
		}
		public int Bit4
		{
			get;
			set;
		}
		public int Mask
		{
			get;
			set;
		}
		public long MaxLeng
		{
			get;
			set;
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
