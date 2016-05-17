using System.Net.Sockets;
using ChatConnect.Tcp.Protocol.WS;

namespace ChatConnect.Tcp.Protocol
{
	interface IProtocol : IAgregator
	{
		Socket Tcp
        {
            get;
        }
		States State
		{
			get;
		}
		IHeader Request
		{
			get;
		}
		IHeader Response
		{
			get;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
		bool Ping(byte[] message);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
		bool Close(string message);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		bool Message(byte[] message);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		bool Message(string message);
		/// <summary>
		/// 
		/// </summary>
		event PHandlerEvent EventWork;
        /// <summary>
        /// 
        /// </summary>
        event PHandlerEvent EventData;
        /// <summary>
        /// 
        /// </summary>
        event PHandlerEvent EventError;
        /// <summary>
        /// 
        /// </summary>
        event PHandlerEvent EventClose;
        /// <summary>
        /// 
        /// </summary>
        event PHandlerEvent EventConnect;
    }
}