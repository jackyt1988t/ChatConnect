using System;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
    [Serializable]
    public class HTTPException : Exception
    {
		public int Num
		{
			get;
			internal set;
		}
		public string Error
		{
			get;
			internal set;
		}
		public codexxx Status
		{
			get;
			internal set;
		}
		public HTTPException() :
            base()
        {

        }
        public HTTPException(string message) :
            base(message)
        {

        }
		public HTTPException(string message, codexxx status) :
			base(message)
		{
			Status = status;
		}
		public HTTPException(string message, codexxx status, Exception except) :
			base(message, except)
		{
			Status = status;
		}
	}
}
