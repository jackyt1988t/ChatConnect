using System;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
    [Serializable]
    class HTTPException : Exception
    {
		public codexxx Status
		{
			get;
			private set;
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
		public HTTPException(string message, Exception except) :
            base(message, except)
        {

        }
		public HTTPException(string message, codexxx status, Exception except) :
			base(message, except)
		{
			Status = status;
		}
	}
}
