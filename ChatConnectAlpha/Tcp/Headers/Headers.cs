using System;
using System.Text;
using System.Collections.Generic;

namespace ChatConnect.Tcp
{
    class Header : Dictionary<string, string>, IHeader
    {
		public const int NONE = 0;
		public const int HEADERS = 1;
		public const int REQUEST = 2;
		public const int RESPONSE = 3;
		
		public int State
		{
			get;
			set;
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
		public byte[] Body
        {
            get;
            set;
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
 
        public Header()
        {
            Body = null;
            State = NONE;
			TimeConnection = DateTime.Now;
			SegmentsBuffer = new Queue<byte[]>(50);
        }
		public void Req()
		{
			IsReq = true;
		}
		public void Res()
		{
			IsRes = true;
		}
		public void End()
		{
			IsEnd = true;
			SegmentsBuffer.Enqueue(new byte[0]);	
		}
		public virtual byte[] ToByte()
			{	
				return Encoding.UTF8.GetBytes(ToString());
			}
			public override string ToString()
			{				
				string request = StartString + "\r\n";
				foreach (KeyValuePair<string, string> keyvalue in this)
				{
					request += keyvalue.Key + ": " + keyvalue.Value + "\r\n";
				}
				request += "\r\n";
				return request;
			}
	}
    class Headers : IHeaders
    {
        public IHeader ResHeaders
        {
            get;
            set;
        }
        public IHeader ReqHeaders
        {
            get;
            set;
        }

        public Headers()
        {
            ResHeaders = new Header();
            ReqHeaders = new Header();
        }
        public string ToRequest()
        {
            string request = ReqHeaders.StartString + "\r\n";
            foreach (KeyValuePair<string, string> keyvalue in ReqHeaders)
            {
                request += keyvalue.Key + ": " + keyvalue.Value + "\r\n";
            }
            request += "\r\n";
            if (ReqHeaders.Body != null)
                request += ReqHeaders.Body;
            return request;
        }
        public string ToResponse()
        {
            string response = ResHeaders.StartString + "\r\n";
            foreach (KeyValuePair<string, string> keyvalue in ResHeaders)
            {
                response += keyvalue.Key + ": " + keyvalue.Value + "\r\n";
            }
            response += "\r\n";
            if (ResHeaders.Body != null)
                response += ResHeaders.Body;
            return response;
        }
    }
}
