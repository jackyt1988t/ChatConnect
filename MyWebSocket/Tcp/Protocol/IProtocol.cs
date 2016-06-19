using System.Net.Sockets;
using MyWebSocket.Tcp.Protocol.WS;

namespace MyWebSocket.Tcp.Protocol
{
	public interface IProtocol : IAgregator
	{
		Socket Tcp
        {
            get;
        }
		States State
		{
			get;
		}
		Header Request
		{
			get;
		}
		Header Response
		{
			get;
		}
		MyStream Reader
		{
			get;
		}
		MyStream Writer
		{
			get;
		}
		
    }
}