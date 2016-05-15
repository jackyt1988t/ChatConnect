using System;

namespace ChatConnect.Tcp.Protocol.WS
{
	class WSChecks
	{
		public Bit Rcv1
		{
			get;
			set;
		}
		public Bit Rcv2
		{
			get;
			set;
		}
		public Bit Rcv3
		{
			get;
			set;
		}
		public Bit Mask
		{
			get;
			set;
		}
		public int Leng
		{
			get;
			set;
		}
		public WSChecks()
		{
			Leng = 48000;
			Mask = Bit.All;
			Rcv1 = Bit.Unset;
			Rcv2 = Bit.Unset;
			Rcv3 = Bit.Unset;
			
		}
	}
}
