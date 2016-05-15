namespace ChatConnect.Tcp
{
	class TaskResult
	{
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