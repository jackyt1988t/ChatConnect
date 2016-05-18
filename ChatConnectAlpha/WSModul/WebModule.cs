using Npgsql;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using ChatConnect.Tcp;
using ChatConnect.Log;
using ChatConnect.Tcp.Protocol;
using ChatConnect.WebModul.Chat;
using ChatConnect.Tcp.Protocol.WS;
using ChatConnect.WSModul.Chat.JsonObject;
using ChatConnect.WebModul.Chat.JsonObject;

namespace ChatConnect.WebModul
{
    [JsonObject(MemberSerialization.OptIn)]
    class WebModule : IWebModule
    {
		[JsonIgnore]
		public WS WS
		{
			get
			{
				return __WS;
			}
		}
		[ JsonIgnore ]
        public Work Work
        {
            get
            {
                return __Work;
            }
            set
            {
                __Work = value;
            }
        }
        [JsonProperty]
        public IUser User
        {
			get
			{
				return __User;
			}
			set
			{
				__User = value;
			}
		}
		public string Uniq
		{
			get;
			set;
		}
		public DateTime DateTime
        {
            get
            {
                return __Date;
            }
        }
            public IHandlerModule HandlerModule
            {
                get
                {
                    lock (this)
                        return __HandlerModule;
                }
                set
                {
                    IHandlerModule hm;
                    lock (this)
                    {
                        hm  =  __HandlerModule;
                        __HandlerModule = value;
                    }
                    if (hm != null)
                        hm.Clear();
                }
            }
        public NpgsqlConnectionStringBuilder ConnectionSetting
        {
            get
            {
                return __ConnectionSetting;
            } 
        }

		protected WS __WS;
		
        protected Work __Work;
		protected IUser __User;
        protected DateTime __Date;
        protected IHandlerModule __HandlerModule;

        protected static object __SYNC = new object();
        protected static string __Descriptor = string.Empty;
        protected static NpgsqlConnectionStringBuilder __ConnectionSetting = null;

        static WebModule()
        {
            StreamReader reader = null;
            try
            {
                FileInfo fileinfo = new FileInfo("Conf/DbConfig.json");
                reader = fileinfo.OpenText();
                __ConnectionSetting =
                    JsonConvert.DeserializeObject<NpgsqlConnectionStringBuilder>(reader.ReadToEnd());
            }
            catch (Exception exc)
            {
                Logout.AddMessage("Ошибка в базе данных: " + exc.Message, @"Log/user.log", Log.Log.Info);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        public WebModule()
        {

        }
        public WebModule(WS protocol)
        {
			__WS = protocol;
			User = new User();
            __Work = new Work();
            __Date = DateTime.Now;
            
            
			
			__WS.EventData += EventText;
			__WS.EventWork += EventWork;
			__WS.EventError += EventError;
			__WS.EventClose += EventClose;

			HandlerModule = new HandlerModuleS(this);

			lock (__SYNC)
            {
                long time = DateTime.Now.Ticks;
                Uniq = Convert.ToString (time);

                if (Uniq.Equals (__Descriptor))
                    Uniq = Convert.ToString(time += 1);

                                 __Descriptor  =  Uniq;
            }
        }
		public void Reset()
		{
			__Date = DateTime.Now;
		}
        public void Working(long time)
        {
            Work.AvgSpeed(time);
        }
        public void Message(string data)
        {
            WS.Message(data);
        }
  async public void MsgFile(string path)
		{
			await Task.Run(() =>
			{
				try
				{
					int i = 0;
					int sleep = 20;
					int maxlen = 1000 * 128;
					using (FileStream sr = new FileStream(path, FileMode.Open, FileAccess.Read))
					{
						int count = (int)(sr.Length / maxlen);
						int length = (int)(sr.Length - count * 
													  maxlen);
						while (i++ < count)
						{
							

							byte[] buffer = new byte [maxlen];
							int __read = sr.Read(buffer, 0, maxlen);
							
							JsFile jsfile = new JsFile(i, sr.Length, buffer);
							Message(new WsJson("File", JsonConvert.SerializeObject(jsfile)).ToJson());
							if (WS.Response.SegmentsBuffer.Count < 10)
							{
								if (sleep > 20)
									sleep -= 20;
							}
							else
							{
								sleep += 20;
								
							}
							Thread.Sleep(sleep);
						}
						if (length > 0)
						{
							byte[] buffer = new byte [length];
							int __read = sr.Read(buffer, 0, length);

							JsFile jsfile = new JsFile(i, sr.Length, buffer);
							Message(new WsJson("File", JsonConvert.SerializeObject(jsfile)).ToJson());
						}
					}
				}
				catch (Exception exc)
				{
					Helpers(new JsHelp(0, exc.Message));
				}
			});
		}
		public void Helpers(JsHelp help)
		{
			WS.Message(new WsJson("Help", JsonConvert.SerializeObject(
												   help)).ToJson());
		}		
        public void EventWork(object sender, PEventArgs e)
        {
			Working(DateTime.Now.Ticks);

            if (Work.TimeWait())
            {
                Message(new WsJson("Working", JsonConvert.SerializeObject(
														  Work)).ToJson());
                Work.Reset();
            }
        }
        public void EventText(object sender, PEventArgs e)
        {
            if (e.sender == null)
                throw new ArgumentNullException("WsMessage");
            WSBinnary binnary = e.sender as WSBinnary;
            if (binnary == null)
                throw new ArgumentNullException("WsMessage");
			IHandlerModule handlermodule   =   HandlerModule;
			WebMooduleJson js = new WebMooduleJson();
			if (binnary.Opcod == WSFrame7.BINNARY)
			{
				js.JsEvent = "Wav";
				js.JsData = binnary.Buffer;
				if (handlermodule != null)
				{
					if (!handlermodule.HandlerJson(this, js))
						throw new WebModuleException("Json сообщение не поддерживается...");

				}
				return;
			}
            string message = Encoding.UTF8.GetString(binnary.Buffer);

            StringReader r = new StringReader(message);
			JsonTextReader jsreader = new JsonTextReader(r);

			try
            {
                bool ret = false;
                js.JsReader = jsreader;
                while (jsreader.Read())
                {
                    if (ret)
                        break;
                    switch (jsreader.TokenType)
                    {
                        case JsonToken.String:
                            if (string.IsNullOrEmpty(js.JsEvent))
                                js.JsEvent = (string)jsreader.Value;
                            else
                                throw new WebModuleException("Json сообщение не поддерживается...");
                            break;
                        case JsonToken.PropertyName:
                            if (!string.IsNullOrEmpty(js.JsEvent))
                                ret = true;
                            break;
                    }
                }
                if (handlermodule != null)
                {
					if (!handlermodule.HandlerJson(this, js))
						throw new WebModuleException("Json сообщение не поддерживается...");

                }
            }
            catch (JsonReaderException exc)
            {
                Logout.AddMessage("Невалидный json формат: " + exc.Message, 
                    @"Log/user.log", Log.Log.Debug);
            }
            catch (WebModuleException exc)
            {
                if (handlermodule != null)
                    handlermodule.HandlerError(this, exc);

                WS.Close(exc.Message);
                Logout.AddMessage("Пользовательскся ошибка: " + exc.Message, 
                    @"Log/user.log", Log.Log.Info);
            }
            catch (Exception exc)
            {
                WS.Close(exc.Message);
                Logout.AddMessage("Ошибка в пользовательском модуле: " + exc.Message + "\r\n" + exc.StackTrace, 
                    @"Log/user.log", Log.Log.Debug);
            }
            finally
            {
                if (jsreader != null)
                    jsreader.Close();
            }

        }
        public void EventClose(object sender, PEventArgs e)
        {
			__HandlerModule = null;
			/*   Отключение событий   */
            __WS.EventData -= EventText;
			__WS.EventWork -= EventWork;
			__WS.EventError -= EventError;
			__WS.EventClose -= EventClose;
			
        }
        public void EventError(object sender, PEventArgs e)
        {

        }
		public void EventBroadcast(object sender, PEventArgs e)
		{
			if (sender == null)
				throw new ArgumentNullException("sender");

			byte[] buffer = sender as byte[];
			WS.Send(buffer);
		}

	}
}