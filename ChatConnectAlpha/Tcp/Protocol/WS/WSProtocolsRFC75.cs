using System;

namespace ChatConnect.Tcp.Protocol.WS
{
	class WSProtocolsRFC75 : WSProtocol
	{
		public WSProtocolsRFC75() :
			base()
		{
			__WSFrame = new WSFrameRFC76();
		}
		public WSProtocolsRFC75(IProtocol http, PHandlerEvent connect) : base()
		{
			__WSFrame = new WSFrameRFC76();
		}
		protected override void HandlerFrame(byte[] buffer, int recive, int length)
		{
			base.HandlerFrame(buffer, recive, length);
		}
	}
}
