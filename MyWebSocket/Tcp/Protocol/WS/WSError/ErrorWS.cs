using System;
using System.Collections.Generic;

namespace MyWebSocket.Tcp.Protocol.WS
{
	public class ErrorWS
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
						return Errors[Errors.Count - 1];
				}
			}
		}
		public List<WSException> Errors;

		public ErrorWS()
		{
			Sync = new object();
			Errors = new List<WSException>(2);
		}
		public void _AddError_(  WSException error  )
		{
			lock (Sync)
				Errors.Add(error);
		}
	}
}
