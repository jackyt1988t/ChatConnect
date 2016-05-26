using System;

namespace ChatConnect.Tcp.Protocol.WS
{
    class WSBinnary
    {
        
        public byte[] Data
        {
            get;
            set;
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
		}
    }
}
