using System;
using System.Text;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	public class HTTPFrame
	{
		public const int DATA = 0;
		public const int CHUNK = 1; 

		public int Pcod;
		public int Hand;
		public int Handl;
		public int hleng;
		public int bleng;
		public int bpart;
		public int ststr;
		public int param;
		public int value;
		public long alleng;
		public bool GetBody;
        public bool GetHead;
        public StringBuilder StStr;
        public StringBuilder Param;
        public StringBuilder Value;

		public void Clear()
		{
			Pcod = 0;
			Hand = 0;
			Handl = 0;
			hleng = 0;
			bleng = 0;
			bpart = 0;
			ststr = 0;
			param = 0;
			value = 0;
			StStr = null;
			Param = null;
			Value = null;
			GetBody = false;
			GetHead = false;
		}
	}
}
