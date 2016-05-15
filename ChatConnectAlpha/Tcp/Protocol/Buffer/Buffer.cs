using System;
using System.Collections.Generic;

namespace ChatConnect.Tcp.Protocol
{
	class Buffer : IBuffer
	{
		public int Count
		{
			get
			{
				return __Buff.Count + __Buffer.Count;
			}
		}
		public int Length
		{
			get;
			private set;
		}
		public object SyncRoot
		{
			get;
			private set;
		}
		private Queue<byte[]> __Buff;
		private Queue<byte[]> __Buffer;

		public Buffer()
		{
			SyncRoot = new object();

			__Buff = new Queue<byte[]>();
			__Buffer = new Queue<byte[]>();
			
		}
		public byte[] Pull()
		{
			byte[] data = null;
			byte[] last = null;

			while (__Buff.Count > 0)
			{
				if (data == null)
					lock (__Buff)
						data = __Buff.Dequeue();
				else
				{
					lock (__Buff)
						last = __Buff.Dequeue();
					byte[] part = new byte[data.Length + last.Length];
					data.CopyTo(part, 0);
					last.CopyTo(last, data.Length);
					data = part;
					if (data.Length > 30000)
						return data;
				}
			}
			
			while (__Buffer.Count != 0)
			{
				if (data == null)
					lock (__Buffer)
						data = __Buffer.Dequeue();
				else
				{
					lock (__Buffer)
						last = __Buffer.Dequeue();
					byte[] part = new byte[data.Length + last.Length];
					data.CopyTo(part, 0);
					last.CopyTo(part, data.Length);
					data = part;
					if (data.Length > 30000)
						return data;
				}
			}
			return data;
		}
		public  void  Push( byte[] data )
		{
			lock (__Buffer)
				__Buffer.Enqueue(data);
		}
		public  void  Push( byte[] data, int length )
		{
			byte[] part = new byte[data.Length - length];
			data.CopyTo(part, length);
				lock (__Buff)
					__Buff.Enqueue(data);
		}
	}
}
