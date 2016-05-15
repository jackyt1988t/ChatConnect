using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ChatConnect.WebModul.Chat
{
    class WsJson
    {
        public string JsEvent;
        public string JObject;
        public WsJson(string jsevent, string jobject)
        {
            JsEvent = jsevent;
            JObject = jobject;
        }
        public string ToJson()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);

            writer.WriteStartObject();
            writer.WritePropertyName("Jsevent");
            writer.WriteValue(JsEvent);
            writer.WritePropertyName("jstring");
            writer.WriteRawValue(JObject);
            writer.WriteEnd();

            return sw.ToString();
        }
    }
}
