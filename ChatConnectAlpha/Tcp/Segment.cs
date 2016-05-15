using System;
using System.Collections.Generic;

namespace ChatConnect.Tcp
{
	class SArray : List<ArraySegment<byte>>
	{
		public int Length
		{
			get;
			private set;
		}
		public void DelSArray(int pos)
		{
			Length -= 
				this[pos].Count;
			RemoveAt(pos);
		}
		public void DelSArray(ArraySegment<byte> sarray)
		{
			DelSArray(IndexOf(
						sarray));
		}
		public void AddSArray(ArraySegment<byte> sarray)
		{
			Length += sarray.Count;
			Add(sarray);
		}
		public void InsertSArray(int pos, ArraySegment<byte> sarray)
		{
			Length += sarray.Count;
			Insert(pos, sarray);
		}
	}
}
