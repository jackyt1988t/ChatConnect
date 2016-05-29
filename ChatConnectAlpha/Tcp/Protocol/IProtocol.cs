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
		StreamS Reader
		{
			get;
		}
		StreamS Writer
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
    }
}