using System;
using System.IO;
using Newtonsoft.Json;
using ChatConnect.WebModul;
using ChatConnect.WebModul.Chat;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace ChatConnect.Chats
{
    class WsChat : ConcurrentDictionary<int, WsRoom>
    {
    	public WsChat()
    	{
    		User user = new User();
    		 	 user.Name = "Тест‚";
        	 	 user.Role = Role.Owner;
        	TryAdd( 0, new WsRoom(0, user ) { Capture = "Тестовая комната" });
        }
    }

    static class WsChats
    {

        static WsChat Chats = new WsChat();
		
		public static bool IsRoom(int room)
		{
			return Chats.ContainsKey( room );
		}
		public static void Update(IUser User)
		{
			WsRoom Room;

			if (!Chats.TryGetValue(User.Room, out Room))
				throw new ArgumentNullException("Room");
			Room.Update(User);
		}
		public static void TM(IUser User, byte[] data)
		{
			WsRoom Room;

			if (!Chats.TryGetValue(User.Room, out Room))
				throw new ArgumentNullException("Room");
			Room.TestM(data);
		}
		public static void Message(IUser User, WsJson Json)
        {
            WsRoom Room;
			
            if (!Chats.TryGetValue(User.Room, out Room))
                throw new ArgumentNullException("Room");
			if (Room.Count < 500)
				Room.Message(Json.ToJson());
			else
				Room.MsAsync(Json.ToJson());
        }

        public static bool Private(IUser user, WsJson Json)
        {
            WsRoom Room;

            if (!Chats.TryGetValue(user.Room, out Room))
                throw new ArgumentNullException("Room");
            return Room.Private(user, Json.ToJson());
        }
        public static void WelcomToRoom(IWebModule module)
        {
        	WsRoom Room;

            if (!Chats.TryGetValue(module.User.Room, out Room))
                throw new ArgumentNullException("Room");

            using(StringWriter sw = new StringWriter())
            {
                JsonTextWriter writer = new JsonTextWriter(sw);
                writer.WriteStartObject();
                	writer.WritePropertyName( "Owner" );
                	writer.WriteRawValue(JsonConvert.SerializeObject(Room.Owner));
                	writer.WritePropertyName("Capture");
                	writer.WriteRawValue(JsonConvert.SerializeObject(Room.Capture));
                writer.WriteEndObject();

                module.Message(new WsJson("welcome", sw.ToString()).ToJson());
            }
        }
        public static void SetRgisterUser(IWebModule module)
        {
            WsRoom Room;

            if (!Chats.ContainsKey(module.User.Room))
                throw new ArgumentNullException("Room");
            if (!Chats.TryGetValue(module.User.Room, out Room))
                throw new ArgumentNullException("Room");

            using(StringWriter sw = new StringWriter())
            {
                JsonTextWriter writer = new JsonTextWriter(sw);
                writer.WriteStartObject();
                    writer.WritePropertyName( "Register" );
                    lock (Room.Register)
                    {
                        writer.WriteRawValue(JsonConvert.SerializeObject(Room.Register));
                    }
                writer.WriteEndObject();
				module.Message(new WsJson("[users]", sw.ToString()).ToJson());
            }

            if (Room.SetUserinRoom(module))
            {
                using (StringWriter sw = new StringWriter())
                {
                    JsonTextWriter writer = new JsonTextWriter(sw);
                    writer.WriteStartObject();
                        writer.WritePropertyName("Uniq");
                        writer.WriteValue(module.Uniq); // Р”РµСЃРєСЂРёРїС‚РѕСЂ
                        writer.WritePropertyName("Date");
                        writer.WriteValue(module.DateTime); // РџРѕРґРєР»СЋС‡РёР»СЃСЏ
                        writer.WritePropertyName("Room");
                        writer.WriteValue(module.User.Room); // РќРѕРјРµСЂ РєРѕРјРЅР°С‚С‹
                        writer.WritePropertyName("Role");
                        writer.WriteValue(module.User.Role); // Р РѕР»СЊ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ
                        writer.WritePropertyName("Name");
                        writer.WriteValue(module.User.Name); // Р›РѕРіРёРЅ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ
                    writer.WriteEndObject();

                        Room.Message(new WsJson("setuser", sw.ToString()).ToJson());
                }
            }
        }
        public static void UnsetRegisterUser(IWebModule module)
        {
            WsRoom Room;

            if (!Chats.ContainsKey(module.User.Room))
                throw new ArgumentNullException("Room");
            if (!Chats.TryGetValue(module.User.Room, out Room))
                throw new ArgumentNullException("Room");
            
            if (Room.UnsetUserinRoom(module))
            {
                using(StringWriter sw = new StringWriter())
                {
                    JsonTextWriter writer = new JsonTextWriter(sw);
                    writer.WriteStartObject();
                        writer.WritePropertyName("Room");
                        writer.WriteValue(module.User.Room); // РќРѕРјРµСЂ РєРѕРјРЅР°С‚С‹
                        writer.WritePropertyName("Role");
                        writer.WriteValue(module.User.Role); // Р РѕР»СЊ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ
                        writer.WritePropertyName("Name");
                        writer.WriteValue(module.User.Name); // Р›РѕРіРёРЅ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ
                    writer.WriteEndObject();
                    /*  */
                    Room.Message(new WsJson("deluser", sw.ToString()).ToJson());
                }
            }
        }
        public static void SetUnregisterUser(IWebModule module)
        {
            WsRoom Room;

            if (!Chats.ContainsKey(module.User.Room))
                throw new ArgumentNullException("Room");
            if (!Chats.TryGetValue(module.User.Room, out Room))
                throw new ArgumentNullException("Room");
                
            Room.SetUnregisterUser(module);
        }
        public static void UnsetUnregisterUser(IWebModule module)
        {
            WsRoom Room;

            if (!Chats.ContainsKey(module.User.Room))
                throw new ArgumentNullException("Room");
            if (!Chats.TryGetValue(module.User.Room, out Room))
                throw new ArgumentNullException("Room");
            
            Room.UnsetUnregisterUser(module);
        }
    }
}
