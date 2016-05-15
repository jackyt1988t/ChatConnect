using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System.Runtime.Serialization;

namespace ChatConnect.WebModul.Chat.JsonObject
{
    [JsonObject(MemberSerialization.OptIn)]
    class JsInit
    {
        public int Room { get; set; }
        public string Pcod { get; set; }
        public DateTime Date { get; set; }

        public JsInit()
        {
            Room = 0;
            Pcod = string.Empty;
            Date = DateTime.Now;
        }
    }
}
