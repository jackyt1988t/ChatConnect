using System;
using Npgsql;
using System.IO;
using Newtonsoft.Json;
using ChatConnect.Log;
using ChatConnect.Chats;
using System.Data.Common;
using System.Threading.Tasks;
using ChatConnect.WebModul.Chat.JsonObject;
using ChatConnect.Tcp.Protocol;
using System.Data;

namespace ChatConnect.WebModul.Chat
{
    class HandlerModuleS : HandlerModuleN
    {
        const long TimeWait = (long)10 * 1000 * 1000 * 20;

        protected long _wait;

        public HandlerModuleS() : 
			base()
			{
            
			}
        public HandlerModuleS(IWebModule wm) :
			base(wm)
			{
				/*     Текущее время     */
				_wait = DateTime.Now.Ticks;
			}

        public static void Handler (IWebModule module)
        {
			if (module.User.Bann > 0)
				module.HandlerModule = new HandlerModuleB(module);
			else if (module.User.IsUser())
                module.HandlerModule = new HandlerModuleU(module);
            else if (module.User.IsAdmin())
                module.HandlerModule = new HandlerModuleA(module);
            else if (module.User.IsModer())
                module.HandlerModule = new HandlerModuleM(module);
            else if (module.User.IsStrimer())
                ;
            else
                module.HandlerModule = new HandlerModuleN(module);

        }
		public static void ConnectionRoom(IWebModule module, JsInit init)
		{
			if (init == null)
			{
				module.Helpers(new JsHelp(0, "Неверный json формат события Init."));
				return;
			}
			if (init.Room < 0)
			{
				module.Helpers(new JsHelp(0, "Указан неправильный номер комнаты. Комната №: " + init.Room));
				return;
			}
			
            if (WsChats.IsRoom(init.Room))
            {
				module.User.Room = init.Room;
				/*    Убираем обработчик    */
            	module.HandlerModule  =  null;
            	/*   Информация о комнате.   */
            	WsChats.WelcomToRoom( module );
                if (!string.IsNullOrEmpty(init.Pcod))
                    ASyncInitializationUser(module, init);
            }
            else
                throw new WebModuleException("Указанная комната отсутсвует в списке. Комната №: " + init.Room);
        }

		public override void AddWM()
		{
			// Не добавляем пользователя
		}
		public override void DelWM()
		{
			// не удалем пользователя
		}
		public override void EventWork(object sender, PEventArgs e)
        {
            if (_wait != 0 && _wait + TimeWait < DateTime.Now.Ticks)
                __WM.WS.Close(Tcp.Protocol.WS.WSClose.Normal);
        }
        public override bool HandlerJson(IWebModule module, WebMooduleJson js)
        {
            switch (js.JsEvent)
            {
                case "Init":
                    ConnectionRoom(module, JsCheckObject.JsCheckDesirializer<JsInit>(js.JsReader));
                    return true;
                default:
                    return false;
            }
        }
        public override void HandlerError(IWebModule module, WebModuleException exc)
        {
			;
        }

        protected static async void ASyncInitializationRoom(IWebModule module, JsInit room)
        {
            NpgsqlCommand comand = new NpgsqlCommand ("SELECT * FROM Rooms WHERE Room = @Room");
            comand.Parameters.Add ("@Room", NpgsqlTypes.NpgsqlDbType.Integer).Value = room.Room;
            if (module.ConnectionSetting == null)
            {
				module.Helpers(new JsHelp(0, "В данный момент авторизация не доступна"));
				Logout.AddMessage("Ошибка в базе данных, нет натсроек ", @"Log/user.log", Log.Log.Info);
				return;
			}

            DbDataReader db = null;
            NpgsqlConnection connect = null;
			try
			{
				connect = new NpgsqlConnection(module.ConnectionSetting);
				await connect.OpenAsync();
				comand.Connection = connect;
				db = await comand.ExecuteReaderAsync();
			}
			catch (Exception exc)
			{
				module.Helpers(new JsHelp(0, "В данный момент авторизация не доступна"));
				Logout.AddMessage("Ошибка в базе данных: " + exc.Message, @"Log/user.log", Log.Log.Info);
			}

            if (db.HasRows)
            {
                while (db.Read())
                {

                }
                // добавить комнату к чату
                ConnectionRoom(module, room);
            }
        }
        protected static async void ASyncInitializationUser(IWebModule module, JsInit room)
        {
			NpgsqlCommand cusers = new NpgsqlCommand("SELECT Users.id as Id," +
															"Users.name as Name," +
															"Users.role as Role " +
													 "FROM   Users " +
													 "WHERE  Users.pcod = @Pcod ");
            cusers.Parameters.Add ( "@Pcod", NpgsqlTypes.NpgsqlDbType.Text).Value = room.Pcod;
			if (module.ConnectionSetting == null)
            {
                Handler(module);

				module.Helpers(new JsHelp(0, "В данный момент авторизация не доступна"));
				Logout.AddMessage("Ошибка в базе данных, нет натсроек ", @"Log/user.log", Log.Log.Info);
				return;
            }

            DbDataReader reader = null;
            NpgsqlConnection connect = null;
			try
			{
					  connect = new NpgsqlConnection(module.ConnectionSetting);
				await connect.OpenAsync();
				cusers.Connection = connect;
				reader = await cusers.ExecuteReaderAsync();
			}
			catch (Exception exc)
			{
				module.Helpers(new JsHelp(0, "В данный момент авторизация не доступна"));
				Logout.AddMessage("Ошибка в базе данных: " + exc.Message, @"Log/user.log", Log.Log.Info);
			}
			if (reader != null)
			{
				if (reader.HasRows)
				{
					reader.Read();
					module.User.Id = reader.GetInt32(0);
					module.User.Name = reader.GetString(1);
					module.User.Role = (Role)reader.GetInt32(2);
					reader.Close();

					NpgsqlCommand busers = new NpgsqlCommand("SELECT Banns.sec as Time," +
																	"Banns.Date as Date " +
															 "FROM   Banns " +
															 "WHERE  Banns.id = @Id     " +
															 "AND    Banns.room = @Room ");
					busers.Parameters.Add("@Id", NpgsqlTypes.NpgsqlDbType.Integer).Value = module.User.Id;
					busers.Parameters.Add("@Room", NpgsqlTypes.NpgsqlDbType.Integer).Value = module.User.Room;
					try
					{
						busers.Connection = connect;
						reader = await busers.ExecuteReaderAsync();
					}
					catch (Exception exc)
					{
						module.Helpers(new JsHelp(0, "В данный момент авторизация не доступна"));
						Logout.AddMessage("Ошибка в базе данных: " + exc.Message, @"Log/user.log", Log.Log.Info);
					}
					if (reader != null)
					{
						if (reader.HasRows)
						{
							reader.Read();
							module.User.Bann = reader.GetInt32(0);
							module.User.Date = reader.GetDateTime(1);
							reader.Close();
						}
					}
				}
			}
			Handler(module);
			if (connect != null)
                connect.Close();
        }
    }
}