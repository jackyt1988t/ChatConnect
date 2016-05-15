using System;

namespace ChatConnect.Tcp.Protocol.HTTP
{
	struct HTTPFrame
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
        public bool GetBody;
        public bool GetHead;
        public string StStr;
        public string Param;
        public string Value;
		public byte[] DataBody;

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
			DataBody = null;
		}
	}
}