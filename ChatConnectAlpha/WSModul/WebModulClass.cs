using System;
using System.Text;
using Newtonsoft.Json;
using ChatConnect.Tcp.Protocol;
using Newtonsoft.Json.Converters;

namespace ChatConnect.WebModul
{

    [JsonObject(MemberSerialization.OptIn)]
    class JsonObject
    {
        [JsonProperty(PropertyName = "event")]
        public string send { get; set; }
        [JsonProperty]
        public object content { get; set; }
        [JsonConverter(typeof(JavaScriptDateTimeConverter))]
        public DateTime datetime { get; set; }
        public JsonObject(string send, object content)
        {
            this.send = send;
            this.content = content;
            this.datetime = DateTime.Now;
        }

        public byte[] TextFrame()
        {
            return new byte[0];
        }
    }

    class WebMooduleJson
    {
		public byte[] JsData;
        public string JsEvent;
        public JsonTextReader JsReader;

        public WebMooduleJson()
        {
			
        }
    }
    class WebModuleCommand
    {
        public string Cmd;
        public string Val;
        public string Text;
        public DateTime Date;

        public WebModuleCommand()
        {
            Cmd = string.Empty;
            Val = string.Empty;
            Text = string.Empty;
            Date = DateTime.Now;
        }
    }
    [Serializable]
    class WebModuleException : Exception
    {
        public WebModuleException() :
            base()
        {

        }
        public WebModuleException(string message) :
            base(message)
        {

        }
    }
}
