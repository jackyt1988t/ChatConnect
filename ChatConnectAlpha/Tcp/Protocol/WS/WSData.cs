using System;
using System.Text;

namespace ChatConnect.Tcp.Protocol.WS
{
    class WSBinnary
    {
		public string Text
		{
			get;
		}
        public byte[] Data
        {
			get;
        }
		public WSOpcod Opcod
        {
            get;
            private set;
        }
        public DateTime Create
        {
            get;
            private set;
        }
		public WSBinnary(byte[] data)
        {
			Data = data;            
		}
		public WSBinnary(byte[] data, WSOpcod opcod) :
			this(data)
		{
			Opcod = opcod;
			Create = DateTime.Now;
			if (opcod == WSOpcod.Text)
				Text = Encoding.UTF8.GetString(data);		
		}
    }
}
