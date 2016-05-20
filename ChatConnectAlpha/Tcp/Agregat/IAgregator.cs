namespace ChatConnect.Tcp
{
	interface IAgregator
	{
		void TaskLoopHandler();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        TaskResult TaskLoopHandlerProtocol();
	}
}