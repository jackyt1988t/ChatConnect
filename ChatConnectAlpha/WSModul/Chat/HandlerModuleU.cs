using System;
using System.IO;
using Newtonsoft.Json;
using ChatConnect.Log;
using ChatConnect.Chats;
using System.Text.RegularExpressions;
using ChatConnect.WebModul.Chat.JsonObject;

namespace ChatConnect.WebModul.Chat
{
    class HandlerModuleU : HandlerModuleN
    {
        const long IntervalNext = (long)10 * 1000 * 500;

        private long __timelast;

        public HandlerModuleU() :
            base()
            {
                __timelast = 0;
			}
        public HandlerModuleU(IWebModule wm) :
            base(wm)
            {
                __timelast = 0;
				wm.WS.EventWork -= EventWork;
            }
        public virtual void Info(IWebModule module, JsInfo io)
        {
            if (io == null)
            {
				module.Helpers(new JsHelp(0 , "Неверный json формат события Info."));
				return;
            }
            if (io.Room != module.User.Room)
            {
				module.Helpers(new JsHelp(0 , "Указан непарвильный номер комнаты."));
				return;
            }

            /*   Информация о комнате.   */
            WsChats.WelcomToRoom( module );
        }
        public virtual void Message(IWebModule module, JsMessage msg)
        {

            if (msg == null)
            {
				module.Helpers(new JsHelp(0, "Неверный json формат события Message."));
				return;
            }
            if (__timelast > DateTime.Now.Ticks)
            {
				module.Helpers(new JsHelp( 0,"Сообщение не отправлено. Слишком частая " + 
					"отправка. Установлена защита от спама. Частота отправки меньше 500 мс."));
				return;
            }
            else
                __timelast = DateTime.Now.Ticks + IntervalNext;

            Match mth;
            WebModuleCommand cmd;
            if ((mth = Regex.Match(msg.Text, @"^/\w+")).Success)
            {
                cmd = new WebModuleCommand();

                cmd.Cmd = "/";
                cmd.Val = mth.Value.Remove(0, 1);
                cmd.Text = msg.Text = msg.Text.Remove(0, mth.Length)
                                         .Trim (new char[] { ' ' });
                if (HandlerExpr(module, cmd))
                    return;
            }
            if ((mth = Regex.Match(msg.Text, @"^@\w+")).Success)
            {
                cmd = new WebModuleCommand();

                cmd.Cmd = "@";
                cmd.Val = mth.Value.Remove(0, 1);
                cmd.Text = msg.Text = msg.Text.Remove(0, mth.Length)
                                         .Trim (new char[] { ' ' });
                if (HandlerExpr(module, cmd))
                    return;
            }
            using(StringWriter sw = new StringWriter())
            {
                JsonTextWriter writer = new JsonTextWriter(sw);

                writer.WriteStartObject();
                    writer.WritePropertyName("Text");
                    writer.WriteValue(msg.Text); // Сообщение
                    writer.WritePropertyName("Date");
                    writer.WriteValue(msg.Date); // Вермя отправления
                    writer.WritePropertyName("Room");
                    writer.WriteValue(module.User.Room); // Номер комнаты
                    writer.WritePropertyName("Role");
                    writer.WriteValue(module.User.Role); // Роль пользователя
                    writer.WritePropertyName("Name");
                    writer.WriteValue(module.User.Name); // Логин пользователя
                writer.WriteEndObject();

                /*         Рассылка сообщения всем пользователям комнаты         */
                WsChats.Message(module.User, new WsJson("Message", sw.ToString()));
            }
        }
        public virtual bool HandlerExpr(IWebModule module, WebModuleCommand cmd)
        {
			if (cmd.Cmd == "/")
			{
				switch (cmd.Val.ToLower())
				{
					case "file":
						module.MsgFile(cmd.Text);
						break;
					default:
						return false;
				}
				return true;
			}
			if (cmd.Cmd == "@")
            {
                /*         получатель сообщения          */
                IUser User = new User() { Name = cmd.Val };

                if (module.User.Equals(User))
                {
					module.Helpers(new JsHelp(0 , "Попытка отправить сообщение " + 
						"самому себе. Убедитесь в парвильности ввода. Формат ввода: @Имя."));
					return false;
                }

                using(StringWriter sw = new StringWriter())
                {
                    JsonTextWriter writer = new JsonTextWriter(sw);

                    writer.WriteStartObject();
                        writer.WritePropertyName("Text");
                        writer.WriteValue(cmd.Text); // Сообщение
                        writer.WritePropertyName("Date"); 
                        writer.WriteValue(cmd.Date); // Текущее время
                        writer.WritePropertyName("Role");
                        writer.WriteValue(module.User.Role); // Роль пользователя
                        writer.WritePropertyName("Name");
                        writer.WriteValue(module.User.Name); // Логин пользователя
                    writer.WriteEndObject();

                    if (!WsChats.Private(User, new WsJson("Private", sw.ToString())))
                    {
						module.Helpers(new JsHelp(0 ,"Пользователь " + cmd.Val + 
							" отсутствует. Убедитесь в парвильности ввода . Формат ввода: @Имя."));
						return false;
                    }
                }
                return true;
            }
            return false;
        }
        public override void AddWM()
        {
            WsChats.SetRgisterUser(__WM);
        }
        public override void DelWM()
        {
            WsChats.UnsetRegisterUser(__WM);
        }
        /* Функция обрабатывает входящие json сообщения, проверяет их валидность */
        public override bool HandlerJson(IWebModule module, WebMooduleJson js)
        {
            switch (js.JsEvent)
            {
				case "Wav":
					WsChats.TM(module.User, js.JsData);
					return true;
				case "Info":
                    Info(module, JsCheckObject.JsCheckDesirializer<JsInfo>(js.JsReader));
                    return true;
                case "Message":
                    Message(module, JsCheckObject.JsCheckDesirializer<JsMessage>(js.JsReader));
                    return true;
                default:
                    return false;
            }
        }
    }
}