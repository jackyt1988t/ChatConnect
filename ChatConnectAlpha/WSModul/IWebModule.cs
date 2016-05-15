using Npgsql;

using System;

using ChatConnect.Tcp;
using ChatConnect.Tcp.Protocol;
using ChatConnect.Tcp.Protocol.WS;

	using ChatConnect.WebModul.Chat;
	using ChatConnect.WebModul.Chat.JsonObject;


namespace ChatConnect.WebModul
{
    interface IWebModule
    {
		WS WS { get; }
		IUser User { get; set; }
        string Uniq { get; set; }
        DateTime DateTime { get; }
        IHandlerModule HandlerModule { get; set; }
        NpgsqlConnectionStringBuilder ConnectionSetting { get; }

        void Working(long time);
        void Message(string data);
		void MsgFile(string path);
		void Helpers(JsHelp help);

		void EventWork(object sender, PEventArgs e);
        void EventText(object sender, PEventArgs e);
        void EventError(object sender, PEventArgs e);
        void EventClose(object sender, PEventArgs e);
		void EventBroadcast(object sender, PEventArgs e);
	}
}
