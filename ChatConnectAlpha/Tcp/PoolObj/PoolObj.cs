using System;
using System.Collections.Concurrent;

namespace ChatConnect.Tcp
{
	class PoolObj<T> 
		where T : class
	{
		private static int MAXCOUNT = 100;
		private ConcurrentQueue<T> __Container;

		public PoolObj()
		{
			__Container = new ConcurrentQueue<T>();
		}
		public PoolObj(int count)
		{
			MAXCOUNT = count;
			__Container = new ConcurrentQueue<T>();
		}

		public T GetObject()
		{
			T item;
			__Container.TryDequeue(out item);
			return item;
		}

		public void PutObject(T item)
		{
			if (__Container.Count < MAXCOUNT)
				__Container.Enqueue(  item  );
		}
	}
}
