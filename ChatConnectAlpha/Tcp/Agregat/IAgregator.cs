namespace ChatConnect.Tcp
{
	interface IAgregator
	{
		TaskResult LoopWork();
		TaskResult LoopRead();
		TaskResult LoopWrite();
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		TaskResult TaskLoopHandlerProtocol();
	}
}