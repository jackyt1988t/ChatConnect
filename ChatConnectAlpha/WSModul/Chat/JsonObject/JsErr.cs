using Newtonsoft.Json;

namespace ChatConnect.WebModul.Chat.JsonObject
{
    [JsonObject(MemberSerialization.OptIn)]
    partial class JsHelp
    {
        [JsonProperty]
        public int Number { get; set; }
        [JsonProperty]
        public string Message { get; set; }

        public JsHelp()
        {

        }
        public JsHelp(int num, string msg)
        {
            Number = num;
            Message = msg;
        }
    }
}
