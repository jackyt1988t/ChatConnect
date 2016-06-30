using System;
using System.Net.Sockets;

namespace MyWebSocket.Tcp.Protocol.WS
{
    [Serializable]
    public class WSException : Exception
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
		public WSClose Close
		{
			get;
			private set;
		}
		public WSException() :
            base()
			{

			}
        public WSException(string message) :
            base(message)
			{

			}
        public WSException(string message, int num) :
            base(message)
			{
				Num = num;
			}
		public WSException(string message, WsError num, WSClose close) :
			base( message )
			{
				Close = close;
				Num = (int)num;
				Error = WSErrors.WSErrorInfo(num);
			}
		public WSException(string message, SocketError num, WSClose close) :
			base( message )
			{
				Close = close;
				Num = (int)num;				
				Error = SocketErrors.SocketErrorInfo(num);
			}
			
		public override string ToString()
		{
			return "Код ошибки: " + Num + "("+ Close.ToString() +"). Сообщение об ошибке: " + Error;	
		}
    }
}
