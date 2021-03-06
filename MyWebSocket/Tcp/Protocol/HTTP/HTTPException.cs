﻿using System;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
    [Serializable]
    public class HTTPException : Exception
    {
		public int Num
		{
			get;
			private set;
		}
		public string Error
		{
			get;
			private set;
		}
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
		public HTTPException(string message, codexxx status, Exception except) :
			base(message, except)
		{
			Status = status;
		}
	}
}
