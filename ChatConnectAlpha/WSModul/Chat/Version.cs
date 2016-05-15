using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChatConnect.WebModul.Chat
{
    class WsVersion
    {
        public int Version;
        public string Greeting;

        public WsVersion(int version, string greeting)
        {
            Version = version;
            Greeting = greeting;
        }
        public virtual string ToJson(IList<string > setting)
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);

            writer.WriteStartObject();

            writer.WritePropertyName("Version");
            writer.WriteValue(Version);
            writer.WritePropertyName("Greeting");
            writer.WriteValue(Greeting);

            writer.WriteEndObject();

            return sw.ToString();
        }
    }
}
