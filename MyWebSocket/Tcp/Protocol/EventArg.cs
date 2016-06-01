using System;

namespace MyWebSocket.Tcp.Protocol
{
    [Serializable]
    public class PEventArgs : EventArgs
    {
        public string state
        {
            get;
            set;
        }
        public object sender
        {
            get;
            set;
        }
        public string message
        {
            get;
            set;
        }
		public static PEventArgs EmptyArgs
		{
			get;
			private set;
		}
		static PEventArgs()
		{
			EmptyArgs = new PEventArgs(string.Empty, string.Empty);
		}
        public PEventArgs(string state, string message)
        {
            this.state = state;
            this.message = message;
        }
        public PEventArgs(string state, string message, object sender)
        {
            this.state = state;
            this.sender = sender;
            this.message = message;
        }
    }
    /*        Делегат для обработки событий парсера        */
    delegate void PHandlerEvent(object sender, PEventArgs e);
}