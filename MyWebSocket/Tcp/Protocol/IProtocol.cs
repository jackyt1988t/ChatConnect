using System.Net.Sockets;
using MyWebSocket.Tcp.Protocol.WS;

namespace MyWebSocket.Tcp.Protocol
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
    }
}