using System;

using Newtonsoft.Json;

namespace ChatConnect.WSModul.Chat.JsonObject
{
	[JsonObject(MemberSerialization.OptIn)]
	class JsFile
	{
		[JsonProperty]
		public int Block
		{
			get;
			private set;
		}
		[JsonProperty]
		public long Length
		{
			get;
			private set;
		}
		[JsonProperty]
		public byte[] Buffer
		{
			get;
			set;
		}

		public JsFile(int block, long length, byte[] buffer)
		{
			Block = block;
			Length = length;
			Buffer = buffer;
		}
	}
}
