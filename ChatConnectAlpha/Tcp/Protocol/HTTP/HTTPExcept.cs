using System;

namespace ChatConnect.Tcp.Protocol.HTTP
{
    [Serializable]
    public class HTTPException : Exception
    {
        public int Number
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
        public HTTPException(string message, int number) :
            base(message)
        {
            Number = number;
        }
        public HTTPException(string message, Exception except) :
            base(message, except)
        {

        }
    }
}
