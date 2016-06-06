using System;
using System.Collections.Generic;

namespace MyWebSocket.Tcp.Protocol.WS
{
	class ErrorWS
	{
		public object Sync;
		public WSException Error
		{
			get
			{	lock (Sync)
				{
					if (Errors.Count == 0)
						return null;
					else
						return Errors[Errors.Count];
				}
			}
		}
		public List<WSException> Errors;

		public ErrorWS()
		{
			Sync = new object();
			Errors = new List<WSException>(2);
		}
		public void AddError(  WSException error  )
		{
			lock (Sync)
				Errors.Add(error);
		}
	}
}
