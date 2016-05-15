using System;
using Npgsql;
using ChatConnect.Log;
using ChatConnect.Chats;
using ChatConnect.Tcp.Protocol;
using ChatConnect.WebModul.Chat.JsonObject;

namespace ChatConnect.WebModul.Chat
{
	class HandlerModuleB : HandlerModuleN
	{
		private static long MLS;
		static HandlerModuleB()
		{
			MLS = 1000*1000*10;
		}
		public HandlerModuleB() :
			base()
			{
			}
		public HandlerModuleB(IWebModule wm) :
			base(wm)
			{
			}
		public override void AddWM()
		{
			WsChats.SetRgisterUser(__WM);
		}
		public override void DelWM()
		{
			WsChats.UnsetRegisterUser(__WM);
		}
		public override void EventWork(object sender, PEventArgs e)
		{
			if (__WM.User.Date.Ticks + 
				__WM.User.Bann * MLS < DateTime.Now.Ticks)
			{
				lock (__WM.User)
				{
					if (__WM.User.Bann > 0)
						__WM.User.Bann = 0;
					else
						__WM.WS.EventWork   -=   EventWork;
				}
				ASyncUnbunnUser( __WM, __WM.User );
			}
		}
		protected static async void ASyncUnbunnUser(IWebModule module, IUser user)
		{
			NpgsqlCommand comand = new NpgsqlCommand("DELETE " +
													 "FROM   Banns " +
													 "WHERE  Banns.id = @Id " +
													 "AND    Banns.room = @Room");
			comand.Parameters.Add("@Id", NpgsqlTypes.NpgsqlDbType.Integer).Value = module.User.Id;
			comand.Parameters.Add("@Room", NpgsqlTypes.NpgsqlDbType.Integer).Value = module.User.Room;
			int result = 0;
			NpgsqlConnection connect = null;
			try
			{
					  connect = new NpgsqlConnection(module.ConnectionSetting);
				await connect.OpenAsync();
				comand.Connection = connect;
				result = await comand.ExecuteNonQueryAsync();
			}
			catch (Exception exc)
			{
				module.Helpers(new JsHelp(0, "Невозможно разбанить пользователя, произошла ошибка"));
				Logout.AddMessage("Ошибка в базе данных: " + exc.Message, @"Log/user.log", Log.Log.Info);
			}
			if (result > 0)
				WsChats.Update(user);
		}
	}
}
