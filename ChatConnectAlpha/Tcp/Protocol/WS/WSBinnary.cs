using System;

namespace ChatConnect.Tcp.Protocol.WS
{
    class WSBinnary
    {
        public int Opcod
        {
            get;
            private set;
        }
        public byte[] Buffer
        {
            get;
            set;
        }
        public DateTime Create
        {
            get;
            private set;
        }
		public DateTime Mofieid
		{
			get;
			private set;
		}
		public WSBinnary(int opcod)
        {
            Opcod = opcod;
            Create = DateTime.Now;
			Mofieid = DateTime.MinValue;
		}
        public void AddBinary(byte[] buffer)
        {
            if (Buffer == null)
            {
                Buffer = buffer;
                Create = DateTime.Now;
            }
            else
            {
                byte[] part = new byte[Buffer.Length + buffer.Length];
                Buffer.CopyTo(part, 0);
                buffer.CopyTo(part, Buffer.Length);
                Buffer = part;
            }
				Mofieid = DateTime.Now;
        }
    }
}
