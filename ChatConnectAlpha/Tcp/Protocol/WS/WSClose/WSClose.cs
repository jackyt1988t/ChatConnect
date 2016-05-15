using System;

namespace ChatConnect.Tcp.Protocol.WS
{
	class WSClose
	{
		public string Host
		{
			get;
			private set;
		}
		public string CloseMsg
		{
			get;
			private set;
		}
		public DateTime CloseTime
		{
			get;
			private set;
		}
		public WSCloseNum CloseCode
		{
			get;
			private set;
		}

		public WSClose(string host, WSCloseNum code)
		{
			Host	  = host;
			CloseMsg  = WSCloseMsg.Message(code);
			CloseCode = code;
		}

		public override string ToString()
		{
			return "Инициатор " + Host + ". " + CloseCode.ToString() + ": " + CloseMsg;
		}
	}
}
