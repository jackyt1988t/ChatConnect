using System;
using System.Text;
using System.Collections.Generic;

namespace MyWebSocket.Tcp
{
    class Header : IHeader
    {
		int contentlength;
		public int ContentLength
		{
			get
			{
				return contentlength;
			}
			private set
			{
				ContainerHeaders.Add("Content-Length", value.ToString());
			}
		}
		string transferencoding;
		public string TransferEncoding
		{
			get
			{
				return transferencoding;
			}
			private set
			{
				ContainerHeaders.Add("TransferEncoding", value.ToString());
			}
		}
		public bool IsEnd
		{
			get;
			private set;
		}
		public bool IsReq
		{
			get;
			private set;
		}
		public bool IsRes
		{
			get;
			private set;
		}
		public bool Close
		{
			get;
			set;
		}
		public byte[] Body
        {
            get;
            set;
        }
		public object Sync
		{
			get;
		}
		public string File
		{
			get;
			set;
		}
		public string Path
        {
            get;
            set;
        }
        public string Http
        {
            get;
            set;
        }
        public string Method
        {
            get;
            set;
        }
        public string StartString
        {
            get;
            set;
        }
		public DateTime TimeConnection
		{
			get;
			private set;
		}
		public Queue<byte[]> SegmentsBuffer
		{
			get;
			private set;
		}
		private Dictionary<string, string> ContainerHeaders;
		public Header()
        {
			Sync = new object();
			TimeConnection = DateTime.Now;
			SegmentsBuffer = new Queue<byte[]>(50);
			ContainerHeaders = new Dictionary<string, string>();

		}
		internal bool SetReq()
		{
			if (!IsReq)
			{
				IsReq = true;
				return false;
			}
			return true;
		}
		public bool SetRes()
		{
			if (!IsRes)
			{
				IsRes = true;
				return false;
			}
			return true;
		}
		public bool SetEnd()
		{
			if (!IsEnd)
			{
				IsEnd = true;
				return false;
			}
			return true;
		}
		public void AddHeader(string key, string value)
		{
			if (SearchHeader(key, value))
				throw new HeadersException("Заголвок уже был добавлен");

			switch (key.ToLower())
			{
				case "content-length":
					if (!int.TryParse(key, out contentlength))
						throw new HeadersException("Неверный Content-Length");
				
				break;
				case "transfer-encoding":
					TransferEncoding = key;
				break;
			}
			
		}
		public bool SearchHeader(string key, string value)
		{
			if (IsRes)
				throw new HeadersException("заголовки были отправлены");
			foreach (KeyValuePair<string, string> header in ContainerHeaders)
			{
				if (header.Key.ToLower() == key)
					return true;
			}
			ContainerHeaders.Add(value, key);
			return false;
		}
		public bool ContainsKeys(string key, bool @case = true)
		{
			foreach (KeyValuePair<string, string> header in ContainerHeaders)
			{
				if (header.Key.ToLower() == key)
					return true;
			}
			return false;
		}


			public virtual byte[] ToByte()
			{	
				return Encoding.UTF8.GetBytes(ToString());
			}
			public override string ToString()
			{				
				string request = StartString + "\r\n";
				foreach (KeyValuePair<string, string> keyvalue in ContainerHeaders)
				{
					request += keyvalue.Key + ": " + keyvalue.Value + "\r\n";
				}
				request += "\r\n";
				return request;
			}
	}
}
