using System;
using Newtonsoft.Json;

namespace ChatConnect.WebModul.Chat.JsonObject
{
	[JsonObject(MemberSerialization.OptIn)]
	class JsMessage
    {
		[JsonProperty]
		public int Room { get; set; }
		[JsonProperty]
		public Role Role { get; set; }
		[JsonProperty]
		public string Text { get; set; }
		[JsonProperty]
		public string Name { get; set; }
		[JsonProperty]
		public DateTime Date { get; set; }

        public JsMessage()
        {
            Text = string.Empty;
            Date = DateTime.Now;
        }
    }
}