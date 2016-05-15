using System;

namespace ChatConnect.Tcp
{
    [Serializable]
    public class HeadersException : Exception
    {
        public HeadersException() :
            base()
        {

        }
        public HeadersException(string message) :
            base(message)
        {

        }
        public HeadersException(string message, Exception inner) :
            base(message, inner)
        {

        }
    }
}
