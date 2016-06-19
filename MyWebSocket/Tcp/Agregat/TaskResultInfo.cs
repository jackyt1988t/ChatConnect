namespace MyWebSocket.Tcp
{
	public class TaskResult
	{
		public bool Jump
		{
			get;
			set;
		}
		public object Result
		{
			get;
			set;
		}
        public TaskOption Option
        {
            get;
            set;
        }
        public TaskProtocol Protocol
		{
			get;
			set;
		}
	}
}